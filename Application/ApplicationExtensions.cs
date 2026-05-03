using AdaByron.Application.UseCases.Auth;
using AdaByron.Application.UseCases.Reservas;
using AdaByron.Application.UseCases.Reservations;
using AdaByron.Application.UseCases.Spaces;
using Microsoft.Extensions.DependencyInjection;

namespace AdaByron.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registro de Autenticación (HU-02)
        services.AddScoped<LoginUseCase>();

        // Registro del Flujo de Reservas (HU-12)
        services.AddScoped<CrearReservaUseCase>();

        // Registro del Flujo de Cancela Reserva (HU-18)
        services.AddScoped<CancelarReservaUseCase>();
        services.AddScoped<GetMisReservasUseCase>();

        // Registra UseCase de Admin
        services.AddScoped<AdaByron.Application.UseCases.Admin.UpdateBuildingConfigUseCase>();

        // Registra UseCases de Espacios
        services.AddScoped<GetFilteredSpacesUseCase>();
        services.AddScoped<ReevaluarReservasEspacioUseCase>(); // PBI-13 (HU-O4) — reevaluación de reservas
        services.AddScoped<UpdateSpaceUseCase>();
        services.AddScoped<SetSpaceOccupancyUseCase>();        // PBI-12 (HU-O1)
        services.AddScoped<AdaByron.Application.UseCases.Reservations.GetLiveReservationsUseCase>();
        services.AddScoped<AdaByron.Application.UseCases.Reservations.DeleteReservationUseCase>();
        services.AddScoped<AdaByron.Application.UseCases.Reservations.ForceCancelReservationUseCase>();       // PBI-13
        services.AddScoped<AdaByron.Application.UseCases.Reservations.ApproveReservationExceptionUseCase>(); // PBI-13
        services.AddScoped<AdaByron.Application.UseCases.Reservations.CleanExpiredReservationsUseCase>();    // PBI-14
        
        return services;
    }
}
