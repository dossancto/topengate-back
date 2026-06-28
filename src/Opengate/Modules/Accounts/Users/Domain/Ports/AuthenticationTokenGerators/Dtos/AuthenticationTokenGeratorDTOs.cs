using Opengate.Modules.Accounts.Users.Models;

namespace Opengate.Modules.Accounts.Users.Domain.Ports.AuthenticationTokenGerators.Dtos;

public record GenerateTokenOutputInput
(
    ApplicationUser User,
    IList<string> Roles
);

public record GenerateTokenOutput
(
    string AcessToken,
    string RefreshToken,
    DateTimeOffset ValidTo,
    DateTimeOffset ValidFrom
)
{
    public TimeSpan ExpiresIn() => ValidTo - DateTimeOffset.UtcNow;
}

public record ReadTokenInput
(
    string Token,
    bool IsAccessToken = true
);

public record ReadTokenOutput
(
    string UserId,
    string UserEmail
);

public record RefreshTokenInput
(
    ApplicationUser User,
    string Token,
    IList<string> Roles
);