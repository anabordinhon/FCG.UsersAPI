using FCG.Users.Application.Auth.Ports;
using FCG.Users.Application.Auth.UseCases.Queries.LoginUserQuery;
using FCG.Users.Application.Common.Ports;
using FCG.Users.Application.Users.Ports;
using FCG.Users.Application.Users.UseCases.Commands.AddUser;
using FCG.Users.Application.Users.UseCases.Commands.DeactivateUser;
using FCG.Users.Application.Users.UseCases.Queries.GetUserById;
using FCG.Users.Application.Users.UseCases.Queries.GetUsersPaged;
using FCG.Users.Infrastructure.Adapters.Auth.Jwt;
using FCG.Users.Infrastructure.Adapters.Common;
using FCG.Users.Infrastructure.Adapters.Users.Repositories;
using FCG.Users.Infrastructure.Messaging;
using FCG.Users.Infrastructure.Persistence;
using FCG.Users.Infrastructure.Persistence.Interceptors;
using FCG.Users.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.AWS.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.ParseStateValues = true;
});
builder.Logging.AddAWSProvider();

const string serviceName = "FCG.Users";
const string serviceVersion = "1.0.0";

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddXRayTraceId()
        .SetSampler(new AlwaysOnSampler())
        .AddConsoleExporter()
       .AddOtlpExporter(opts =>
       {
           opts.Endpoint = new Uri("http://localhost:4317");
           opts.Protocol = OtlpExportProtocol.Grpc;
       })
    )
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(
                $"https://logs.{builder.Configuration["AWS:Region"] ?? "us-east-1"}.amazonaws.com/v1/metrics");
            opts.Protocol = OtlpExportProtocol.HttpProtobuf;
        })
    )
    .WithLogging(logging => logging
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddConsoleExporter()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(
                $"https://logs.{builder.Configuration["AWS:Region"] ?? "us-east-1"}.amazonaws.com/v1/logs");
            opts.Protocol = OtlpExportProtocol.HttpProtobuf;
        })
    );

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var userContext = serviceProvider.GetService<IUserContext>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
    options.AddInterceptors(new AuditInterceptor(userContext));
}, ServiceLifetime.Scoped);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IAddOrUpdateUserCommandHandler, AddOrUpdateUserCommandHandler>();
builder.Services.AddScoped<IEventPublisher, SqsEventPublisher>();
builder.Services.AddScoped<IUserCommandRepository, UserCommandRepository>();
builder.Services.AddScoped<IUserQueryRepository, UserQueryRepository>();
builder.Services.AddScoped<IGetUserByIdQueryHandler, GetUserByIdQueryHandler>();
builder.Services.AddScoped<IGetUsersPagedQueryHandler, GetUsersPagedQueryHandler>();
builder.Services.AddScoped<IDeactivateUserCommandHandler, DeactivateUserCommandHandler>();
builder.Services.AddScoped<ILoginUserQueryHandler, LoginUserQueryHandler>();
builder.Services.AddScoped<IHashHelper, HashHelper>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fiap Cloud Games Users API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey não configurada");

var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddHealthChecks();

var app = builder.Build();

Sdk.SetDefaultTextMapPropagator(
    new CompositeTextMapPropagator(new TextMapPropagator[]
    {
        new AWSXRayPropagator(),
        new TraceContextPropagator(),
        new BaggagePropagator()
    }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hashHelper = scope.ServiceProvider.GetRequiredService<IHashHelper>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await AdminUserSeed.EnsureAdminUserAsync(db, hashHelper, configuration);
}

app.MapHealthChecks("/health");
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();