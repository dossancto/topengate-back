namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

/// <summary>
/// Request to get details for a specific invite.
/// The InviteId value comes from an email invite sent to the user.
/// </summary>
public class GetInviteDetailsRequest
{
    /// <summary>
    /// The unique identifier of the invite. Value comes from an email invite sent to the user.
    /// </summary>
    public required Guid InviteId { get; init; }
}

/// <summary>
/// Response containing details about an invite.
/// </summary>
public class GetInviteDetailsResponse
{
    /// <summary>
    /// Organization associated with the invite.
    /// </summary>
    public GetInviteDetailsResponseOrganization Organization { get; set; } = new();

    /// <summary>
    /// Information about the invited user.
    /// </summary>
    public GetInviteDetailsResponseInvitedUser InvitedUser { get; set; } = new();

    /// <summary>
    /// The date and time when the invite was created.
    /// </summary>
    /// <example> 2024-06-01T12:34:56Z </example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the invite expires.
    /// </summary>
    /// <example> 2024-07-01T12:34:56Z </example>
    public DateTime ExpiresAt { get; set; }

}

/// <summary>
/// Organization details in the invite response.
/// </summary>
public class GetInviteDetailsResponseOrganization
{
    /// <summary>
    /// Name of the organization.
    /// </summary>
    /// <example> Acme </example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the organization.
    /// </summary>
    /// <example> A company </example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Logo of the organization (optional).
    /// </summary>
    /// <example> https://mocked.example.com/assets/logo.png </example>
    public string? Logo { get; set; }
}

/// <summary>
/// Details about the invited user in the invite response.
/// </summary>
public class GetInviteDetailsResponseInvitedUser
{
    /// <summary>
    /// Email address of the invited user.
    /// </summary>
    /// <example> user@example.com </example>
    public string Email { get; set; } = string.Empty;
}