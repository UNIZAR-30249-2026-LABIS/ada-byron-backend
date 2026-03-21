using AdaByron.Application.UseCases.Auth;
using AdaByron.Application.UseCases.Reservas;
using Microsoft.Extensions.DependencyInjection;

namespace AdaByron.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginUseCase>();
        services.AddScoped<CrearReservaUseCase>();

        return services;
    }
}

