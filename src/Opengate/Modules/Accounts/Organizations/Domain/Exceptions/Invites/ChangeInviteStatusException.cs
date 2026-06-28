namespace Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;

public class ChangeInviteStatusException(
    Guid inviteId,
    ChangeInviteStatusError error
    ) : Exception($"Error changing invite status. Error: {error}")
{
    public Guid InviteId { get; } = inviteId;
    public ChangeInviteStatusError Error { get; } = error;
}

public enum ChangeInviteStatusError
{
    /// <summary>
    /// The invite was not found.
    /// </summary>
    InviteNotFound,

    /// <summary>
    /// The invite has already been responded to.
    /// </summary>
    InviteAlreadyResponded,

    /// <summary>
    /// The provided status is invalid.
    /// </summary>
    InvalidStatus,
}