namespace Opengate.Modules.Accounts.Organizations.Domain.Enums;

/// <summary>
/// Represents the status of an organization invite.
/// </summary>
public enum OrganizationInviteStatus
{
    /// <summary>
    /// Invite is awaiting response. Only pending invites can be accepted, rejected, or cancelled.
    /// </summary>
    Pending,
    /// <summary>
    /// Invite has been accepted. The user has joined the organization.
    /// </summary>
    Accepted,
    /// <summary>
    /// Invite has been rejected. The user will not join the organization.
    /// </summary>
    Rejected,
    /// <summary>
    /// Invite has been cancelled. The invite is no longer valid.
    /// </summary>
    Cancelled
}