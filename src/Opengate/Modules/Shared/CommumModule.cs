using Opengate.Modules.Shared.Adapters.Monitoring;
using Opengate.Modules.Shared.Configuration.API;
using Opengate.Modules.Shared.Configuration.Databases;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders;

namespace Opengate.Modules.Shared;

public static class SharedModule
{
    public static IServiceCollection AddSharedModule(this IServiceCollection services)
    {
        services.AddDatabaseConfiguration();
        services.AddMonitoring();
        services.AddApiConfiguration();

        services.AddEmailSenderIntegrationModule();

        return services;
    }

    public static WebApplication UseSharedModule(this WebApplication app)
    {
        app.UseApiConfiguration();

        return app;
    }
}