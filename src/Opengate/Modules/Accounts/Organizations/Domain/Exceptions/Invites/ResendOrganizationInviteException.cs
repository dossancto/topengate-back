namespace Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;

/// <summary>
/// Exception thrown when some error occurs when resending an organization invite.
/// </summary>
public class ResendOrganizationInviteExceptionException(
    ResendOrganizationInviteExceptionError error,
    string organizationId,
    string inviteId
    ) : Exception($"Error reinviting user to organization. Error: {error}, OrganizationId: {organizationId}, inviteid: {inviteId}")
{
    public ResendOrganizationInviteExceptionError Error { get; private init; } = error;

    public string OrganizationId { get; private set; } = organizationId;

    public string InviteId { get; } = inviteId;
}

/// <summary>
/// Represents errors that can occur when resending an organization invite.
/// </summary>
public enum ResendOrganizationInviteExceptionError
{
    /// <summary>
    /// The invite was not found.
    /// </summary>
    InviteNotFound
}