using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

/// <summary>
/// Request model for listing organization invites.
/// </summary>
public class ListOrganizationInvitesRequest
{
}

/// <summary>
/// Response model representing an organization invite.
/// </summary>
public class ListOrganizationInvitesResponse
{
    /// <summary>
    /// The email address of the invitee.
    /// </summary>
    /// <example>john@doe.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The status of the organization invite.
    /// </summary>
    public OrganizationInviteStatus Status { get; set; }

    /// <summary>
    /// The date and time when the invite was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the invite expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// The date and time when the invite was responded to, if any.
    /// </summary>
    public DateTime? RespondedAt { get; set; }
}