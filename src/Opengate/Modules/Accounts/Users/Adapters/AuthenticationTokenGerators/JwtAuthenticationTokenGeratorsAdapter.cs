using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using Opengate.Modules.Accounts.Users.Domain.Enum;
using Opengate.Modules.Accounts.Users.Domain.Ports.AuthenticationTokenGerators;
using Opengate.Modules.Accounts.Users.Domain.Ports.AuthenticationTokenGerators.Dtos;

namespace Opengate.Modules.Accounts.Users.Adapters.AuthenticationTokenGerators;

public class JwtAuthenticationTokenGeratorsAdapter : IAuthenticationTokenGenerator
{
    private readonly string JwtKey = AppEnv.APP.AUTHENTICATION.JWT.KEY.NotNull();
    private readonly string JwtIssuer = AppEnv.APP.AUTHENTICATION.JWT.ISSUER.NotNull();
    private readonly string JwtAudience = AppEnv.APP.AUTHENTICATION.JWT.AUDIENCE.NotNull();

    private readonly TimeSpan AccessTokenDuration = TimeSpan.FromHours(1);

    private readonly TimeSpan RefreshTokenDuration = TimeSpan.FromDays(30);


    public Task<GenerateTokenOutput> GenerateTokenAsync(GenerateTokenOutputInput input)
        => GenerateTokenFromInfosAsync(
                userId: input.User.Id,
                userName: input.User.UserName ?? input.User.Email ?? input.User.Id,
                email: input.User.Email ?? string.Empty,
                organizationId: input.User.OrganizationId,
                role: string.Join(",", input.Roles)
        );

    private Task<GenerateTokenOutput> GenerateTokenFromInfosAsync(
            string userId,
            string userName,
            string email,
            string? organizationId,
            string role
            )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(JwtKey);

        // Create a list of claims for the user
        var accesTokenclaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, userId),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (ClaimTypes.NameIdentifier, userId),
            new (ClaimTypes.Email, email),
            new ("organization_id", organizationId ?? string.Empty),
            new (ClaimTypes.Name, userName),
            new (ClaimTypes.Role, role.ToString())
        };

        var accessTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(accesTokenclaims),
            Expires = DateTime.UtcNow.Add(AccessTokenDuration),
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var accesstoken = tokenHandler.CreateToken(accessTokenDescriptor);
        var accessTokenStr = tokenHandler.WriteToken(accesstoken);

        var refreshTokenClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, userId),
            new ("IsRefreshToken", "true"),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (ClaimTypes.NameIdentifier, userId),
            new (ClaimTypes.Name, userName)
        };

        var refreshTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(refreshTokenClaims),
            Expires = DateTime.UtcNow.Add(RefreshTokenDuration),
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var refreshtoken = tokenHandler.CreateToken(refreshTokenDescriptor);
        var refreshTokenStr = tokenHandler.WriteToken(refreshtoken);

        var output = new GenerateTokenOutput
        (
            AcessToken: accessTokenStr,
            RefreshToken: refreshTokenStr,
            ValidTo: accesstoken.ValidTo,
            ValidFrom: accesstoken.ValidFrom
        );

        return Task.FromResult(output);
    }

    public Task<ReadTokenOutput> ReadTokenAsync(ReadTokenInput input)
    {
        return ReadToken(input, input.IsAccessToken);
    }

    private Task<ReadTokenOutput> ReadToken(ReadTokenInput input, bool isAccessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(JwtKey);

        var validationParameters = new TokenValidationParameters
        {
            // Validate the signature of the token using the secret key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // Validate the token's lifetime (expiration)
            ValidateLifetime = true,

            // Optionally, set the clock skew to zero for precise expiration
            // This ensures the token expires at the exact time, not a few minutes later
            ClockSkew = TimeSpan.Zero,

            // Set these to false if you are only validating signature and lifetime
            // In a real-world scenario, you would typically validate these as well
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

        tokenHandler.ValidateToken(
                token: input.Token,
                validationParameters: validationParameters,
                validatedToken: out var validatedToken);

        if (validatedToken is not JwtSecurityToken token)
        {
            throw new Exception("Invalid token");
        }

        var userId = token.Claims.First(c => c.Type == "nameid").Value;
        var userName = token.Claims.First(c => c.Type == "unique_name").Value;

        var refreshTokenFlag = token.Claims.FirstOrDefault(c => c.Type == "IsRefreshToken")?.Value;

        var isRefreshToken = refreshTokenFlag is "true";

        var invalidAccessToken = isAccessToken is true && isRefreshToken is true;

        var invalidRefreshToken = isAccessToken is false && isRefreshToken is false;

        if (invalidAccessToken)
        {
            throw new Exception("Invalid Access Token");
        }

        if (invalidRefreshToken)
        {
            throw new Exception("Invalid Refresh Token");
        }

        var output = new ReadTokenOutput
        (
            UserId: userId,
            UserEmail: userName
        );

        return Task.FromResult(output);
    }

    public async Task<GenerateTokenOutput> RefreshTokenAsync(RefreshTokenInput input)
    {
        var token = await ReadToken(
                input: new(
                    Token: input.Token
                ),
                isAccessToken: false
        );

        var newToken = await GenerateTokenFromInfosAsync(
                userId: token.UserId,
                userName: token.UserEmail,
                email: input.User.Email ?? token.UserEmail,
                organizationId: input.User.OrganizationId,
                role: string.Join(",", input.Roles)
        );

        return newToken;
    }
}