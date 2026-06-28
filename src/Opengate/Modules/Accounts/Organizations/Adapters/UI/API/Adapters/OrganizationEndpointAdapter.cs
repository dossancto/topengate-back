using System.Security.Claims;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;
using Opengate.Modules.Accounts.Organizations.Application.ChangeInviteStatus;
using Opengate.Modules.Accounts.Organizations.Application.InitOrganization;
using Opengate.Modules.Accounts.Organizations.Application.InviteUserToOrganization;
using Opengate.Modules.Accounts.Organizations.Application.ResendOrganizationInvite;
using Opengate.Modules.Accounts.Organizations.Domain.Enums;
using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Adapters.Databases;
using Opengate.Modules.Shared.Adapters.Databases.Extensions;
using Opengate.Modules.Shared.Domain.ValueObjects.Emails;
using Opengate.Modules.Shared.Domain.ValueObjects.Pagination;
using Opengate.Modules.Shared.Domain.ValueObjects.Slugs;
using Opengate.Modules.Shared.Utils.HttpUtils;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Adapters;

public class OrganizationEndpointAdapterInstance;

public static class OrganizationEndpointAdapter
{
    /// <summary>
    /// Init a new Organization by configuring the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="request">The request.</param>
    /// <param name="workflow">The workflow.</param>
    /// <returns>The result.</returns>
    public static async Task<Created<CreateOrganizationResponse>> CreateOrganization(
            HttpContext context,
            CreateOrganizationRequest request,
            [FromServices] InitOrganizationWorkflow workflow
    )
    {
        var input = request.ToInitOrganizationInput();

        var result = (await workflow.Execute(input).Run()).ThrowIfFail();

        return TypedResults.Created(
                $"/organizations/{result.Id}",
                new CreateOrganizationResponse(result.Id)
        );
    }

    /// <summary>
    /// Get Organization details.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="db"></param>
    /// <returns>The result.</returns>
    public static async Task<Results<Ok<GetOrganizationDetailsResponse>, NotFound<ProblemDetails>>> GetOrganizationDetails(
            HttpContext context,
            [FromServices] ApplicationDbContext db
    )
    {
        var user = context.GetRequiredUserInfo();

        var organization = await db.Organizations
            .AsNoTracking()
            .Where(x => x.Id == user.OrganizationId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                x.Logo,
                x.OwnerEmail,
                x.CreatedAt,
            })
            .FirstOrDefaultAsync();

        if (organization is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Organization not found",
                Status = StatusCodes.Status404NotFound,
                Detail = "The organization associated with the user was not found."
            });
        }

        var response = new GetOrganizationDetailsResponse
        {
            CreatedAt = organization.CreatedAt,
            Description = organization.Description,
            Id = organization.Id,
            Slug = organization.Slug,
            Logo = organization.Logo,
            Name = organization.Name,
            OwnerEmail = organization.OwnerEmail
        };

        return TypedResults.Ok(response);
    }

    public static async Task<Results<Ok<AcceptOrganizationInviteResponse>, UnauthorizedHttpResult>> AcceptInvite(
            HttpContext context,
            [FromBody] AcceptOrganizationInviteRequest request,
            [FromServices] ChangeInviteStatusWorkflow workflow,
            [FromServices] ILogger<OrganizationEndpointAdapterInstance> logger,
            [FromServices] UserManager<ApplicationUser> userManager
    )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null || user.Email is null || userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var userEmail = user.Email;

        var input = request.ToInput(
            userId: userId,
            userEmail: userEmail);

        var result = (await workflow.Execute(input).Run()).ThrowIfFail();

        return TypedResults.Ok(new AcceptOrganizationInviteResponse());
    }

    public static async Task<Results<Ok<RejectOrganizationInviteResponse>, UnauthorizedHttpResult>> RejectInvite(
            HttpContext context,
            RejectOrganizationInviteRequest request,
            [FromServices] ChangeInviteStatusWorkflow workflow,
            [FromServices] UserManager<ApplicationUser> userManager
    )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null || user.Email is null || userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var userEmail = user.Email;

        var input = request.ToInput(
            userId: userId,
            userEmail: userEmail);

        var result = (await workflow.Execute(input).Run()).ThrowIfFail();

        return TypedResults.Ok(new RejectOrganizationInviteResponse());
    }

    /// <summary>
    /// Get organization invite details
    /// </summary>
    /// <remarks>Use this endpoint to receive specific details about an organization before accept invite using the invite ID.</remarks>
    /// <param name="context">The current HTTP context containing user information.</param>
    /// <param name="inviteId">The unique identifier of the invite. Value comes from an email invite sent to the user.</param>
    /// <param name="db">The application's database context.</param>
    /// <param name="userManager">The user manager for accessing user information.</param>
    /// <returns>Invite data received from invite</returns>
    public static async Task<Results<Ok<GetInviteDetailsResponse>, ProblemHttpResult, UnauthorizedHttpResult>> InviteDetails(
            HttpContext context,
            [FromRoute] Guid inviteId,
            [FromServices] ApplicationDbContext db,
            [FromServices] UserManager<ApplicationUser> userManager
    )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var userEmail = user.Email!;

        var now = DateTime.UtcNow;

        var response = await db.OrganizationInvites
            .Include(x => x.Organization)
            .Where(x => x.Id == inviteId
                    && x.IsDeleted == false
                    && (x.Status == OrganizationInviteStatus.Pending)
                    && (x.ExpiresAt > now)
                    && x.Email.ToLower() == userEmail.ToLower()
            )
            .Select(invite => new GetInviteDetailsResponse
            {
                Organization = new GetInviteDetailsResponseOrganization
                {
                    Name = invite.Organization!.Name,
                    Description = invite.Organization.Description,
                    Logo = invite.Organization!.Logo
                },
                InvitedUser = new GetInviteDetailsResponseInvitedUser
                {
                    Email = invite.Email
                },
                CreatedAt = invite.CreatedAt,
                ExpiresAt = invite.ExpiresAt
            })
            .FirstOrDefaultAsync()
            ;

        if (response is null)
        {
            return TypedResults.Problem(
                detail: "Invite not found or is no longer valid.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Invite Not Found",
                type: "InviteNotFound"
            );
        }

        return TypedResults.Ok(response);
    }

    public static async Task<Results<Ok<OffsetPaginationResponse<ListOrganizationInvitesResponse>>, NotFound<string>, UnauthorizedHttpResult>> ListInvites(
            HttpContext context,
            [FromServices] ApplicationDbContext db,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] OrganizationInviteStatus[]? statuses = null,
            CancellationToken ct = default
    )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        if (user.OrganizationId is null)
        {
            return TypedResults.NotFound("User does not belong to an organization.");
        }

        var pagination = new OffsetPaginationQuery(
            page: page,
            pageSize: pageSize
        );

        var organizationId = Guid.Parse(user!.OrganizationId);

        var now = DateTime.UtcNow;

        var query = db.OrganizationInvites
            .Include(x => x.Organization)
            .Where(x => x.IsDeleted == false
                    && (x.ExpiresAt > now)
                    && x.OrganizationId == organizationId
            );

        if (statuses is not null && statuses.Length > 0)
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        var response = await query
            .OrderBy(x => x.Email)
            .Select(x => new ListOrganizationInvitesResponse()
            {
                Email = x.Email,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                ExpiresAt = x.ExpiresAt,
                RespondedAt = x.RespondedAt
            })
            .PaginatedAsync(pagination, ct)
            ;

        return TypedResults.Ok(response);
    }

    public static async Task<Results<NoContent, UnauthorizedHttpResult, ProblemHttpResult>> InviteUserToOrganization(
            HttpContext context,
            [FromBody] InviteUserToOrganizationRequest request,
            [FromServices] InviteUserToOrganizationWorkflow workflow,
            [FromServices] UserManager<ApplicationUser> userManager,
            CancellationToken ct = default
    )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        if (user.OrganizationId is null)
        {
            return TypedResults.Problem(detail: "User does not belong to an organization.", statusCode: StatusCodes.Status401Unauthorized);
        }

        var input = new InviteUserToOrganizationInput(
            SenderUserId: Guid.Parse(userId!),
            OrganizationId: Guid.Parse(user.OrganizationId),
            TargetEmail: Email.Parse(request.TargetEmail),
            TargetUserFirstName: request.TargetUserFirstName,
            TargetUserLastName: request.TargetUserLastName
        );

        (await workflow.Execute(input, ct).Run()).ThrowIfFail();

        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> DeleteInvite(
            HttpContext context,
            [FromRoute] Guid inviteId,
            [FromServices] ApplicationDbContext db,
            [FromServices] UserManager<ApplicationUser> userManager,
            CancellationToken ct = default
    )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var invite = await db.OrganizationInvites
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                    x.Id == inviteId
                    && x.IsDeleted == false
                    && x.Status == OrganizationInviteStatus.Pending
                    && x.OrganizationId == Guid.Parse(user.OrganizationId!)
                    , ct);

        if (invite is null)
        {
            return TypedResults.NotFound();
        }

        var cancelledInvite = invite.Cancel();

        db.OrganizationInvites.Update(cancelledInvite);

        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }

    /// <summary>
    /// Resends an organization invite to a user based on the provided invite ID.
    /// </summary>
    /// <remarks>Use this endpoint to resend an organization invite to a user.</remarks>
    /// <param name="context">The current HTTP context containing user claims.</param>
    /// <param name="inviteId">The unique identifier of the organization invite to resend.</param>
    /// <param name="userManager">The user manager service for accessing user information.</param>
    /// <param name="workflow">The workflow service responsible for resending the organization invite.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// Returns <see cref="NoContent"/> if the invite was resent successfully.
    /// Returns <see cref="NotFound"/> if the invite or user was not found.
    /// Returns <see cref="UnauthorizedHttpResult"/> if the user is not authorized.
    /// </returns>
    public static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> ResendOrganiationInvite(
            HttpContext context,
            [FromRoute] Guid inviteId,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] ResendOrganizationInviteWorkflow workflow,
            CancellationToken ct = default
    )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var input = new ResendOrganizationInviteInput(
            InviteId: inviteId,
            OrganizationId: Guid.Parse(user.OrganizationId!)
        );

        var _ = (await workflow.Execute(input, ct).Run()).ThrowIfFail();

        return TypedResults.NoContent();
    }


    /// <summary>
    /// Updates the user role in an organization.
    /// </summary>
    /// <param name="context">The current HTTP context containing user claims.</param>
    /// <param name="request"> The request containing the user role to update.</param>
    /// <param name="userManager">The user manager service for accessing user information.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    ///
    public static async Task<Results<NoContent, NotFound, ProblemHttpResult>> UpdateUserRoleInOrganization(
        HttpContext context,
        [FromBody] UpdateUserRoleInOrganizationRequest request,
        [FromServices] UserManager<ApplicationUser> userManager
        )
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("nameid")?.Value;

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        if (user.OrganizationId is null)
        {
            return TypedResults.Problem(detail: "User does not belong to an organization.", statusCode: StatusCodes.Status401Unauthorized);
        }

        var roles = await userManager.GetRolesAsync(user);

        if (roles.Contains(request.Role.ToString()) == true)
        {
            return TypedResults.Problem(detail: "User already has this role.", statusCode: StatusCodes.Status400BadRequest);
        }

        await userManager.RemoveFromRolesAsync(user, roles);
        await userManager.AddToRoleAsync(user, request.Role.ToString());

        return TypedResults.NoContent();
    }
}