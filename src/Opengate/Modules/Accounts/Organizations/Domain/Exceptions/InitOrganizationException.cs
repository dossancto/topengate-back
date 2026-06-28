namespace Opengate.Modules.Accounts.Organizations.Domain.Exceptions;

/// <summary>
/// Exception thrown when an error occurs during organization initialization.
/// </summary>
public class InitOrganizationException(
    string message,
    InitOrganizationExceptionType type) : Exception(message)
{
    /// <summary>
    /// Gets the type of the organization initialization exception.
    /// </summary>
    public InitOrganizationExceptionType Type { get; } = type;

    /// <summary>
    /// Gets the name of the organization related to the exception.
    /// </summary>
    public string Organization { get; private init; } = string.Empty;

    /// <summary>
    /// Creates an exception indicating that the organization already exists.
    /// </summary>
    /// <param name="organization">The name of the organization.</param>
    /// <param name="message">The exception message.</param>
    /// <returns>An <see cref="InitOrganizationException"/> for an existing organization.</returns>
    public static InitOrganizationException OrganizationAlreadyExists(
        string organization,
        string message = "Organization already exists."
    )
    {
        return new InitOrganizationException(message, InitOrganizationExceptionType.OrganizationAlreadyExists)
        {
            Organization = organization
        };
    }
}

/// <summary>
/// Specifies the type of organization initialization exception.
/// </summary>
public enum InitOrganizationExceptionType
{
    /// <summary>
    /// Indicates that the organization already exists.
    /// </summary>
    OrganizationAlreadyExists
}