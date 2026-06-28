using System.Net.Http.Json;

using Opengate.Modules.Accounts.Users.Adapters.Endpoints.Dtos;

namespace Opengate.Tests.Integration.Modules.Accounts.Users.Endpoints;

public class UpdateUserEndpointTest : IClassFixture<AppFactory>, IDisposable
{
    private readonly AppFactory _appFactory;

    public UpdateUserEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _appFactory.OutputHelper = outputHelper;
    }

    public void Dispose()
    {
        _appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task UpdateUser_ShouldEditFirstAndLastName()
    {
        var client = await _appFactory.GetAuthorizedClient();

        var updateRequest = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        var response = await client.PostAsJsonAsync("/manage/info", updateRequest, cancellationToken: TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var infoResponse = await client.GetAsync("/manage/info", cancellationToken: TestContext.Current.CancellationToken);
        infoResponse.EnsureSuccessStatusCode();
        var userInfo = await infoResponse.Content.ReadFromJsonAsync<UserInfoResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Jane", userInfo!.FirstName);
        Assert.Equal("Smith", userInfo.LastName);

    }
}