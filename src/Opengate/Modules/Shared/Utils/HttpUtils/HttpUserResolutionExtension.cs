using System.Security.Claims;

using Opengate.Modules.Shared.Domain.Exceptions;
using Opengate.Modules.Shared.Domain.ValueObjects.Emails;

namespace Opengate.Modules.Shared.Utils.HttpUtils;

public static class HttpUserResolutionExtension
{
    public static Guid GetRequiredUserId(this HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("nameid")?.Value
            ?? throw new UnauthorizedAccessException("User ID could not be resolved from the token.")
            ;

        return Guid.Parse(userId);
    }


    public static UserInfosFromToken? GetUserInfo(this HttpContext context)
    {
        var userId = GetRequiredUserId(context);

        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value
            ?? context.User.FindFirst("email")?.Value;

        var organizationId = context.User.FindFirst("organization_id")?.Value;

        if (string.IsNullOrWhiteSpace(organizationId))
        {
            return null;
        }

        return new()
        {
            Id = userId,
            Email = Email.Parse(userEmail ?? string.Empty),
            OrganizationId = Guid.Parse(organizationId),
        };
    }

    public static UserInfosFromToken GetRequiredUserInfo(this HttpContext context)
    {
        var userInfo = context.GetUserInfo();

        if (userInfo is null)
        {
            throw new HttpUserResolutionException(HttpUserResolutionError.UserNotFound);
        }

        return userInfo;
    }
}

public class UserInfosFromToken()
{
    public Guid Id { get; set; }
    public Email Email { get; set; }
    public Guid OrganizationId { get; set; }
};
