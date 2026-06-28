using Opengate.Modules.Accounts.Organizations.Adapters.UI.API;
using Opengate.Modules.Accounts.Organizations.Application.ChangeInviteStatus;
using Opengate.Modules.Accounts.Organizations.Application.InitOrganization;
using Opengate.Modules.Accounts.Organizations.Application.InviteUserToOrganization;
using Opengate.Modules.Accounts.Organizations.Application.ResendOrganizationInvite;

namespace Opengate.Modules.Accounts.Organizations;

public static class OrganizationsModule
{
    public static IServiceCollection AddOrganizationsModule(this IServiceCollection services)
    {
        services
            .AddScoped<InitOrganizationWorkflow>()
            .AddScoped<ChangeInviteStatusWorkflow>()
            .AddScoped<InviteUserToOrganizationWorkflow>()
            .AddScoped<ResendOrganizationInviteWorkflow>()
            ;

        return services;
    }

    public static WebApplication UseOrganizationsModule(this WebApplication app)
    {
        app.MapOrganizationsEndpoint();

        return app;
    }
}