namespace Opengate.Modules.Accounts.Organizations.Application.ResendOrganizationInvite;

/// <summary>
/// Input model for resending an organization invite.
/// </summary>
/// <param name="InviteId">The unique identifier of the invite.</param>
/// <param name="OrganizationId">The unique identifier of the organization.</param>
public record ResendOrganizationInviteInput
(
     Guid InviteId,
     Guid OrganizationId
);

/// <summary>
/// Output model for resending an organization invite.
/// </summary>
public record ResendOrganizationInviteOutput();