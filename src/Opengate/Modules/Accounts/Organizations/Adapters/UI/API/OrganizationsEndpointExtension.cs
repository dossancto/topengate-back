using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Adapters;
using Opengate.Modules.Accounts.Users.Domain.Enum;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API;

public static class OrganizationsEndpointExtension
{
    /// <summary>
    /// Maps the organizations endpoint.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns>The application.</returns>
    public static WebApplication MapOrganizationsEndpoint(this WebApplication app)
    {
        var g = app.MapGroup("/organizations")
            .WithName("Organizations")
            .WithTags("Organizations")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation()
            ;

        g.MapPost("/", OrganizationEndpointAdapter.CreateOrganization)
            .WithName("Create Organization")
            .WithSummary("Create an organization")
            .ExcludeFromDescription()
            ;

        g.MapGet("/", OrganizationEndpointAdapter.GetOrganizationDetails)
            .WithName("Organization Details")
            .WithSummary("Get organization details")
            ;

        var invitesG = g.MapGroup("/invites");

        invitesG.MapPost("/accept", OrganizationEndpointAdapter.AcceptInvite)
            .WithName("Accept a organization invite")
            .WithSummary("Accept an organization invite")
            .WithDescription("Use this endpoint to accept an organization invite using the invite ID.")
            .ProducesProblem(404)
            .ProducesValidationProblem()
            ;

        invitesG.MapPost("/reject", OrganizationEndpointAdapter.RejectInvite)
            .WithSummary("Reject an organization invite")
            .WithName("Reject a organization invite")
            .WithDescription("Use this endpoint to reject an organization invite using the invite ID.")
            ;

        invitesG.MapGet("{inviteId:Guid}/details", OrganizationEndpointAdapter.InviteDetails)
            .WithSummary("Get organization invite details")
            .WithName("Get organization invite details")
            .ProducesProblem(statusCode: 404, contentType: "application/problem+json")
            ;

        invitesG.MapGet("/", OrganizationEndpointAdapter.ListInvites)
            .WithSummary("List organization Invites")
            .WithName("List organization invite")
            .WithDescription("Use this endpoint to list all organization invites.")
            ;

        invitesG.MapPost("/", OrganizationEndpointAdapter.InviteUserToOrganization)
            .WithSummary("Create organization invite")
            .WithName("Create organization invite")
            .WithDescription("Use this endpoint to invite a user to join the organization.")
            .ProducesProblem(statusCode: 400, contentType: "application/problem+json")
            ;

        invitesG.MapPatch("/{inviteId:Guid}/resend", OrganizationEndpointAdapter.ResendOrganiationInvite)
            .WithSummary("Resend organization invite")
            .WithName("Resend organization invite")
            .ProducesProblem(statusCode: 404, contentType: "application/problem+json")
            .ProducesProblem(statusCode: 400, contentType: "application/problem+json")
            ;

        invitesG.MapDelete("/{inviteId:guid}/cancel", OrganizationEndpointAdapter.DeleteInvite)
            .WithSummary("Cancel organization invite")
            .WithName("Cancel organization invite")
            .WithDescription("Use this endpoint to cancel an organization invite using the invite ID.")
            ;

        var usersG = g.MapGroup("{organizationId:guid}/users/{userId:guid}");

        usersG.MapPatch("/role", OrganizationEndpointAdapter.UpdateUserRoleInOrganization)
            .WithSummary("Update user role in organization")
            .WithName("Update user role in organization")
            ;

        return app;
    }
}