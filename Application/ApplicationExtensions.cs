using AdaByron.Application.UseCases.Auth;
using AdaByron.Application.UseCases.Reservas;
using Microsoft.Extensions.DependencyInjection;

namespace AdaByron.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registro de Autenticación (HU-02)
        services.AddScoped<LoginUseCase>();

        // Registro del Flujo de Reservas (HU-12)
        // Este UseCase es el que orquesta la HU-13, HU-14 y HU-15
        services.AddScoped<CrearReservaUseCase>();

        return services;
    }
}
