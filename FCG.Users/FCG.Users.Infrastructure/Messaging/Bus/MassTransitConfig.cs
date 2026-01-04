using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Users.Infrastructure.Messaging.Bus;

public static class MassTransitConfig
{
    public static IServiceCollection AddMassTransitConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
        var rabbitUser = configuration["RabbitMq:Username"] ?? "guest";
        var rabbitPass = configuration["RabbitMq:Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}