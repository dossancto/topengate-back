using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Namotion.Reflection;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints;

public class GetOrganizationDetailsEndpointTest : IClassFixture<AppFactory>
{
    private readonly AppFactory _appFactory;

    public GetOrganizationDetailsEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
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
    public async Task GetOrganizationDetailsEndpointTest_ShouldListOrganization_Details_WhenValidRequest()
    {
        using var scope = _appFactory.Services.CreateScope();

        var (client, user) = await _appFactory.GetAuthorizedClientNewUser(scope);
        var db = _appFactory.GetApplicationDbContext(scope);

        var createOrgRequest = new CreateOrganizationRequest(
                    Name: $"Test Organization {Guid.NewGuid()}",
                    Description: "Test organization description",
                    OwnerEmail: user.Email!,
                    OwnerPhoneNumber: "12345678",
                    Document: "12345678",
                    DocumentType: "test",
                    Country: "test",
                    Logo: "test"
        );

        var response = await client.PostAsJsonAsync("/organizations",
                createOrgRequest
                , cancellationToken: TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var organization = await response.Content.ReadFromJsonAsync<CreateOrganizationResponse>(TestContext.Current.CancellationToken);

        var invite = await db.OrganizationInvites.FirstOrDefaultAsync(
            x => x.OrganizationId == organization!.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        var inviteResponse = await client.PostAsJsonAsync(
            requestUri: $"/organizations/invites/accept",
            new AcceptOrganizationInviteRequest(invite!.Id),
            cancellationToken: TestContext.Current.CancellationToken);

        inviteResponse.EnsureSuccessStatusCode();

        client = await _appFactory.RefreshClient(client);

        var organizationDetailsResponse = await client.GetFromJsonAsync<GetOrganizationDetailsResponse>("/organizations",
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(organizationDetailsResponse);

        organizationDetailsResponse.Id.ShouldBe(organization!.Id);
    }
}