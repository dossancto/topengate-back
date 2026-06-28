using Opengate.Modules.Accounts.Users.Domain.Enum;

namespace Opengate.Modules.Accounts.Users.Adapters.Endpoints.Dtos;

/// <summary>
/// Represents a request to register a new user.
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    /// <example>P@ssw0rd!</example>
    public string Password { get; set; } = string.Empty;
}