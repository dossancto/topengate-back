using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Users.Adapters.Endpoints.Dtos;

namespace Opengate.Tests.Integration.Modules.Accounts.Users.Endpoints;

public class RegisterUserEndpointTest : IClassFixture<AppFactory>, IDisposable
{
    private readonly AppFactory _appFactory;

    public RegisterUserEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _appFactory.OutputHelper = outputHelper;
    }

    public void Dispose()
    {
        _appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task RegisterUser_ShouldSetFirstAndLastName()
    {
        using var scope = _appFactory.Services.CreateScope();

        var client = _appFactory.CreateClient();

        var db = _appFactory.GetApplicationDbContext(scope);

        var registerRequest = new RegisterUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"john{Guid.NewGuid()}@test.com",
            Password = "TestPassword123!"
        };

        var response = await client.PostAsJsonAsync("/register", registerRequest, cancellationToken: TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email, TestContext.Current.CancellationToken);

        Assert.NotNull(dbUser);
        Assert.Equal("John", dbUser!.FirstName);
        Assert.Equal("Doe", dbUser.LastName);

    }
}