using Opengate.Modules.Accounts.Organizations;
using Opengate.Modules.Accounts.Users;

namespace Opengate.Modules.Accounts;

public static class AccountsModule
{
    public static IServiceCollection AddAccountsModule(this IServiceCollection services)
    {
        services
            .AddUsersModule()
            .AddOrganizationsModule()
            ;

        return services;
    }

    public static WebApplication UseAccountsModule(this WebApplication app)
    {
        app
            .UseUsersModule()
            .UseOrganizationsModule()
            ;

        return app;
    }
}