namespace Opengate.Modules.Accounts.Users.Adapters.Endpoints.Dtos;

/// <summary>
/// Represents the response data for user information.
/// </summary>
public class UserInfoResponse
{
    /// <summary>
    /// Users first name
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Users last name
    /// </summary>
    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the user's email is confirmed.
    /// </summary>
    /// <example>true</example>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// The organization ID associated with the user, if any.
    /// </summary>
    /// <example>org-12345</example>
    public string? OrganizationId { get; set; }
}