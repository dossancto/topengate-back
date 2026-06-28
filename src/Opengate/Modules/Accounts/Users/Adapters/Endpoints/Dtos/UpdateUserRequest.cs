namespace Opengate.Modules.Accounts.Users.Adapters.Endpoints.Dtos;

/// <summary>
/// Represents a request to update user information.
/// </summary>
/// <remarks>
/// This class is used to encapsulate the data required to update a user's profile, including name, email, and password changes.
/// Only the properties that are not null will be updated; properties left as null will remain unchanged.
/// </remarks>
public class UpdateUserRequest
{
    /// <summary>
    /// Gets or sets the user's new first name.
    /// </summary>
    /// <example>John</example>
    public string? FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's new last name.
    /// </summary>
    /// <example>Doe</example>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's new email address.
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string? NewEmail { get; set; }

    /// <summary>
    /// Gets or sets the user's current password (required for password change).
    /// </summary>
    /// <example>currentPassword123</example>
    public string? OldPassword { get; set; }

    /// <summary>
    /// Gets or sets the user's new password.
    /// </summary>
    /// <example>newPassword456</example>
    public string? NewPassword { get; set; }
}