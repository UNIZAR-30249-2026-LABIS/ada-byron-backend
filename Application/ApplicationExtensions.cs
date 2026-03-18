using AdaByron.Application.UseCases.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace AdaByron.Application;

// Registra todos los casos de uso de la capa de Application en el contenedor DI.
public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginUseCase>();

        // TODO: registrar los demás casos de uso cuando se implementen:
        // services.AddScoped<HazReservaUseCase>();
        // services.AddScoped<ApruebaReservaUseCase>();
        // services.AddScoped<RechazaReservaUseCase>();

        return services;
    }
}
