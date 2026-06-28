namespace Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;

/// <summary>
/// Exception thrown when inviting a user to an organization fails.
/// </summary>
public class InviteUserToOrganizationException(
    InviteUserToOrganizationError error,
    string organizationId,
    string targetEmail
    ) : Exception($"Error inviting user to organization. Error: {error}, OrganizationId: {organizationId}, TargetEmail: {targetEmail}")
{
    public InviteUserToOrganizationError Error { get; private init; } = error;

    public string OrganizationId { get; private set; } = organizationId;

    public string TargetEmail { get; private set; } = targetEmail;
}

public enum InviteUserToOrganizationError
{
    /// <summary>
    /// The user is already a member of the organization.
    /// </summary>
    UserAlreadyMember,

    /// <summary>
    /// An invite has already been sent to the user.
    /// </summary>
    InviteAlreadySent,

    /// <summary>
    /// Sender user has no organization.
    /// </summary>
    SenderHasNoOrganization,

    /// <summary>
    /// The sender of the invite was not found.
    /// </summary>
    SenderNotFound,

    /// <summary>
    /// The organization was not found.
    /// </summary>
    OrganizationNotFound,

    /// <summary>
    /// Failed to create the user.
    /// </summary>
    UserCreationFailed

}