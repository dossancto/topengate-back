using Opengate.Modules.Accounts.Organizations.Adapters.Entities;
using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Modules.Accounts.Organizations.Domain.Models;

public record class OrganizationInvite
{
    /// <summary>
    /// Unique identifier for the organization invite.
    /// </summary>
    /// <example>"d290f1ee-6c54-4b01-90e6-d701748f0851"</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the organization to which the invite belongs.
    /// </summary>
    /// <example>"a123f1ee-6c54-4b01-90e6-d701748f0852"</example>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization entity associated with the invite.
    /// </summary>
    public OrganizationEntity? Organization { get; set; }

    /// <summary>
    /// Current status of the invite (e.g., Pending, Accepted, Declined).
    /// </summary>
    /// <example>OrganizationInviteStatus.Pending</example>
    public OrganizationInviteStatus Status { get; set; }

    /// <summary>
    /// Email address of the invitee. Target user email.
    /// </summary>
    /// <example>"user@example.com"</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the invitee, if provided.
    /// </summary>
    /// <example>"+1234567890"</example>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date and time when the invite was responded to, if applicable.
    /// </summary>
    /// <example>"2024-06-10T14:30:00Z"</example>
    public DateTime? RespondedAt { get; set; }

    /// <summary>
    /// Date and time when the invite was created.
    /// </summary>
    /// <example>"2024-06-01T09:00:00Z"</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the invite was last updated.
    /// </summary>
    /// <example>"2024-06-05T12:15:00Z"</example>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Date and time when the invite was deleted, if applicable.
    /// </summary>
    /// <example>"2024-06-08T17:45:00Z"</example>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Date and time when the invite expires.
    /// </summary>
    /// <example>"2024-07-01T23:59:59Z"</example>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indicates whether the invite has been marked as deleted.
    /// </summary>
    /// <example>true</example>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Duration of the token that is generated for the invite.
    /// </summary>
    public static readonly TimeSpan TokenDuration
        = TimeSpan.FromDays(7);

    public static OrganizationInvite Create(
        Guid organizationId,
        string email,
        string? phoneNumber
    )
    {
        var timestamp = DateTime.UtcNow;

        return new OrganizationInvite
        {
            Id = Guid.CreateVersion7(),
            OrganizationId = organizationId,
            Status = OrganizationInviteStatus.Pending,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = timestamp,
            ExpiresAt = timestamp.Add(TokenDuration),
            DeletedAt = null,
            IsDeleted = false
        };
    }

    public OrganizationInvite Resend()
    {
        var timestamp = DateTime.UtcNow;

        var newExpiresAt = timestamp.Add(TokenDuration);

        if (Status is not OrganizationInviteStatus.Pending)
        {
            throw new InvalidOperationException("Cannot resend an invite that has already been accepted or rejected.");
        }

        return this with
        {
            ExpiresAt = newExpiresAt,
            UpdatedAt = timestamp,
        };
    }

    /// <summary>
    /// Use this method to mark a pending invite as Accpeted.
    /// </summary>
    public OrganizationInvite Accept()
    => ChangeStatus(OrganizationInviteStatus.Accepted);

    /// <summary>
    /// Use this method to mark a pending invite as Rejected.
    /// </summary>
    public OrganizationInvite Reject()
    => ChangeStatus(OrganizationInviteStatus.Rejected);

    /// <summary>
    /// Use this method to cancel a pending invite.
    /// </summary>
    /// <returns>A cancelled organization invite</returns>
    public OrganizationInvite Cancel()
    {
        var timestamp = DateTime.UtcNow;

        if (Status is OrganizationInviteStatus.Cancelled)
        {
            return this;
        }

        if (Status is not OrganizationInviteStatus.Pending)
        {
            throw new InvalidOperationException("Cannot cancel an invite that has already been accepted or rejected.");
        }

        return this with
        {
            Status = OrganizationInviteStatus.Cancelled,
            UpdatedAt = timestamp,
            DeletedAt = timestamp,
            IsDeleted = true
        };
    }

    /// <summary>
    /// Returns the invite code.
    /// </summary>
    /// <returns>The invite code as a string.</returns>
    public string GetInviteCode()
        => Id.ToString();

    /// <summary>
    /// Changes the status of the organization invite to the specified status.
    /// </summary>
    /// <param name="status">The new status to set for the invite.</param>
    /// <returns>A new <see cref="OrganizationInvite"/> instance with the updated status and timestamps.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the current invite status is not <see cref="OrganizationInviteStatus.Pending"/>.
    /// </exception>
    private OrganizationInvite ChangeStatus(OrganizationInviteStatus status)
    {
        var timestamp = DateTime.UtcNow;

        if (Status is not OrganizationInviteStatus.Pending)
        {
            throw new InvalidOperationException("Cannot change status of an invite that has not been accepted or rejected.");
        }

        return this with
        {
            Status = status,
            RespondedAt = timestamp,
            UpdatedAt = timestamp,
        };
    }
}