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

        var shopId = context.User.FindFirst("shop_id")?.Value;

        if (string.IsNullOrWhiteSpace(organizationId))
        {
            return null;
        }

        return new()
        {
            Id = userId,
            Email = Email.Parse(userEmail ?? string.Empty),
            OrganizationId = Guid.Parse(organizationId),
            ShopId = shopId is null ? null : Guid.Parse(shopId)
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

    public static UserInfosWithShopFromToken GetRequiredUserInfoWithShop(this HttpContext context)
    {
        var userInfo = context.GetUserInfo();

        if (userInfo is null)
        {
            throw new HttpUserResolutionException(HttpUserResolutionError.UserNotFound);
        }

        // if (userInfo.ShopId is null)
        // {
        //     throw new HttpUserResolutionException(HttpUserResolutionError.ShopNotFound);
        // }

        return new()
        {
            Id = userInfo.Id,
            Email = userInfo.Email,
            OrganizationId = userInfo.OrganizationId,
            ShopId = Guid.Parse("b8772276-7b3d-4193-ba0a-a676d196f77c") // TODO : FIX!
        };
    }
}

public class UserInfosFromToken()
{
    public Guid Id { get; set; }
    public Email Email { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ShopId { get; set; }
};

public class UserInfosWithShopFromToken() : UserInfosFromToken
{
    public new Guid ShopId { get; set; }
};