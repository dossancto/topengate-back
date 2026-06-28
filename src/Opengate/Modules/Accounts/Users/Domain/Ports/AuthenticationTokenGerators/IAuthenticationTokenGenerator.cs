using Opengate.Modules.Accounts.Users.Domain.Ports.AuthenticationTokenGerators.Dtos;

namespace Opengate.Modules.Accounts.Users.Domain.Ports.AuthenticationTokenGerators;

public interface IAuthenticationTokenGenerator
{
    Task<GenerateTokenOutput> GenerateTokenAsync(GenerateTokenOutputInput input);

    Task<GenerateTokenOutput> RefreshTokenAsync(RefreshTokenInput input);

    Task<ReadTokenOutput> ReadTokenAsync(ReadTokenInput input);
}