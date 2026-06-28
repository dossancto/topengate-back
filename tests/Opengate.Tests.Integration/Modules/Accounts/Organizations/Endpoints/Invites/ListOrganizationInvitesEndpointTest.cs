
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;
using Opengate.Modules.Shared.Domain.ValueObjects.Pagination;
using Opengate.Tests.Integration.Modules.Shared.Dtos;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints.Invites;

public class ListOrganizationInvitesEndpointTest : IClassFixture<AppFactory>
{
    readonly JsonSerializerOptions options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AppFactory _appFactory;

    public ListOrganizationInvitesEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _appFactory.OutputHelper = outputHelper;
    }

    [Fact]
    public void Dispose()
    {
        _appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task ListOrganizationInvitesEndpointTest_ShouldListInvites_WhenValidRequestAsync()
    {
        using var scope = _appFactory.Services.CreateScope();
        var client = await _appFactory.GetAuthorizedClient();
        var db = _appFactory.GetApplicationDbContext(scope);

        var response = await client.PostAsJsonAsync("/organizations", new CreateOrganizationRequest(
                    Name: $"Test Organization {Guid.NewGuid()}",
                    Description: "Test organization description",
                    OwnerEmail: "test@test.com",
                    OwnerPhoneNumber: "12345678",
                    Document: "12345678",
                    DocumentType: "test",
                    Country: "test",
                    Logo: "test"
        )
        , cancellationToken: TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var createdOrganizationContent = await response.Content.ReadFromJsonAsync<CreateOrganizationResponse>(cancellationToken: TestContext.Current.CancellationToken);

        var invite = await db.OrganizationInvites.FirstOrDefaultAsync(
            x => x.OrganizationId == createdOrganizationContent!.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        var inviteResponse = await client.PostAsJsonAsync(
            requestUri: $"/organizations/invites/accept",
            new AcceptOrganizationInviteRequest(invite!.Id),
            cancellationToken: TestContext.Current.CancellationToken);

        inviteResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync($"/organizations/invites?page=1&pageSize=10",
            cancellationToken: TestContext.Current.CancellationToken);

        listResponse.EnsureSuccessStatusCode();

        var invitesList = await listResponse.Content.ReadFromJsonAsync<TestPaginationResponse<ListOrganizationInvitesResponse>>(options, cancellationToken: TestContext.Current.CancellationToken);

        invitesList.ShouldNotBeNull();
        invitesList.Details.PageSize.ShouldBe(10);
        invitesList.Details.CurrentPage.ShouldBe(1);
        invitesList.Items.Count.ShouldBeGreaterThan(0);
    }
}