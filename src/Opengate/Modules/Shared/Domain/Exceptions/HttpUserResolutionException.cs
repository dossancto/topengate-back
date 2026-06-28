using System.ComponentModel;

namespace Opengate.Modules.Shared.Domain.Exceptions;

/// <summary>
/// Exception thrown when inviting a user to an organization fails.
/// </summary>
public class HttpUserResolutionException(
    HttpUserResolutionError error
) : Exception($"User resolution error: {error}")
{
    public HttpUserResolutionError Error { get; private init; } = error;
}


public enum HttpUserResolutionError
{
    [Description("Error in resolving user from token.")]
    UserNotFound = 1,
    [Description("Error in resolving organization from token.")]
    OrganizationNotFound,
    [Description("Error in resolving shop from token.")]
    ShopNotFound,
}