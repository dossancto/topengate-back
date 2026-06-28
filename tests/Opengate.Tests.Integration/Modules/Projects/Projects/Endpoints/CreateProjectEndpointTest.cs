using System.Net;
using System.Net.Http.Json;

using Opengate.Modules.Projects.Adapters.UI.Api.Dtos;

namespace Opengate.Tests.Integration.Modules.Projects.Projects.Endpoints;

public class CreateProjectEndpointTest : IClassFixture<AppFactory>, IDisposable
{
    private readonly AppFactory appFactory;

    public CreateProjectEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        this.appFactory = appFactory;
        this.appFactory.OutputHelper = outputHelper;
    }

    public void Dispose()
    {
        appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task CreateOrganization_ShouldReturnCreatedOrganization()
    {
        var client = await appFactory.GetAuthorizedClient();

        var response = await client.PostAsJsonAsync("/projects", new CreateProjectRequest(
            Name: $"Test Project {Guid.NewGuid()}",
            Description: "Test project description"
        )
        , cancellationToken: TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var statusCode = response.StatusCode;

        Assert.Equal(HttpStatusCode.Created, statusCode);
    }
}