using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Shared.Adapters.Databases;

namespace Opengate.Modules.Shared.Configuration.Databases;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
    {
        var connectionString = AppEnv.DATABASES.POSTGRES.MAIN_DB.CONNECTION_STRING.NotNull();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        return services;
    }
}