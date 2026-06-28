using Opengate.Modules.Accounts.Users.Domain.Enum;

namespace Opengate.Modules.Test.Endpoints;

public static class TestEndpointsExtension
{
    public static void MapTestEndpoints(this WebApplication app)
    {
        var g = app
            .MapGroup("/test")
            .WithTags("test")
            .WithName("test")
            .RequireAuthorization()
            .ExcludeFromDescription()
            ;

        g.MapPost($"/auth/policy/{ApplicationRoles.ADMIN}", () => Results.Ok())
            .RequireAuthorization(ApplicationRoles.ADMIN.ToString())
            .WithName("Test policy authorization ADMIN")
            .WithSummary("Test policy authorization ADMIN")
            ;

        g.MapPost($"/auth/policy/{ApplicationRoles.STAFF}", () => Results.Ok())
            .RequireAuthorization(ApplicationRoles.STAFF.ToString())
            .WithName("Test policy authorization STAFF")
            .WithSummary("Test policy authorization STAFF")
            ;

        g.MapPost($"/auth/policy/{ApplicationRoles.OPERATOR}", () => Results.Ok())
            .RequireAuthorization(ApplicationRoles.OPERATOR.ToString())
            .WithName("Test policy authorization OPERATOR")
            .WithSummary("Test policy authorization OPERATOR")
            ;
    }
}