using ALGarage.Application.Maintenance;
using ALGarage.Application.Parts;
using ALGarage.Application.ServiceHistory;
using ALGarage.Application.Vehicles;
using ALGarage.Domain.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ALGarage.Application;

/// <summary>Registra os serviços de aplicação e do domínio no contêiner de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Serviços puros do domínio.
        services.AddSingleton<IMaintenanceEstimator, MaintenanceEstimator>();
        services.TryAddSingleton(TimeProvider.System);

        // Casos de uso.
        services.AddScoped<VehicleService>();
        services.AddScoped<MaintenanceStatusService>();
        services.AddScoped<ServiceHistoryService>();
        services.AddScoped<PartsLinkService>();
        services.AddScoped<Catalog.ModelCatalogService>();
        services.AddScoped<Notifications.MaintenanceNotificationService>();

        return services;
    }
}
