using System.Net.Http.Json;

using Opengate.Modules.Projects.Adapters.UI.Api.Dtos;
using Opengate.Modules.Shared.Domain.ValueObjects.Pagination;

namespace Opengate.Tests.Integration.Modules.Projects.Projects.Endpoints;

public class ListProjectsEndpointTest : IClassFixture<AppFactory>, IDisposable
{
    private readonly AppFactory appFactory;

    public ListProjectsEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
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

        var response = await client
            .GetFromJsonAsync<OffsetPaginationResponse<ListProjectsResponse>>(
            "/projects?page=1&pageSize=10",
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(response);

        response.Details.TotalItems.ShouldBeGreaterThan(0);
    }

}
