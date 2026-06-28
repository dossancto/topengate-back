using Opengate.Modules.Accounts.Users.Adapters.AuthenticationTokenGerators;
using Opengate.Modules.Accounts.Users.Adapters.Endpoints;
using Opengate.Modules.Accounts.Users.Configurations;
using Opengate.Modules.Accounts.Users.Domain.Ports.AuthenticationTokenGerators;
using Opengate.Modules.Accounts.Users.Models;

namespace Opengate.Modules.Accounts.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.ConfigureIdentity();
        services.AddSingleton<IAuthenticationTokenGenerator, JwtAuthenticationTokenGeratorsAdapter>();

        return services;
    }

    public static WebApplication UseUsersModule(this WebApplication app)
    {
        app.MapCustomIdentityApi<ApplicationUser>();

        return app;
    }
}