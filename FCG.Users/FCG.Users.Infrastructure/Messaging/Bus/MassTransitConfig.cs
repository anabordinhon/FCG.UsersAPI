using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Users.Infrastructure.Messaging.Bus;
public static class MassTransitConfig
{
    public static IServiceCollection AddMassTransitConfiguration(
        this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        });

        return services;
    }
}
