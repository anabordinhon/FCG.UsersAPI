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
using FCG.Users.Infrastructure.Persistence;
using FCG.Users.Infrastructure.Persistence.Interceptors;
using FCG.Users.Infrastructure.Messaging.Bus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey não configurada (verifique appsettings / User Secrets)");

var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // Em produção true
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddMassTransitConfiguration();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseAuthorization();

app.MapControllers();

app.Run();
