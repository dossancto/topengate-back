using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints.Invites;

public class AcceptOrganizationInviteEndpointTest : IClassFixture<AppFactory>, IDisposable
{
    private readonly AppFactory _appFactory;

    public AcceptOrganizationInviteEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _appFactory.OutputHelper = outputHelper;
    }

    public void Dispose()
    {
        _appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task AcceptOrganizationInviteEndpointTest_ShouldAReturnSuccess_WhenValidInvite()
    {
        using var scope = _appFactory.Services.CreateScope();

        var client = await _appFactory.GetAuthorizedClient();
        var db = _appFactory.GetApplicationDbContext(scope);

        var createOrgRequest = new CreateOrganizationRequest(
                    Name: $"Test Organization {Guid.NewGuid()}",
                    Description: "Test organization description",
                    OwnerEmail: "test@test.com",
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
    }

    [Fact]
    public async Task AcceptOrganizationInviteEndpointTest_ShouldFail_WhenAcceptorUserIsNotTheInvitedOne()
    {
        using var scope = _appFactory.Services.CreateScope();

        var client = await _appFactory.GetAuthorizedClient();
        var (client2, _) = await _appFactory.GetAuthorizedClientNewUser(scope);
        var db = _appFactory.GetApplicationDbContext(scope);

        var createOrgRequest = new CreateOrganizationRequest(
                    Name: $"Test Organization {Guid.NewGuid()}",
                    Description: "Test organization description",
                    OwnerEmail: "test@test.com",
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

        var inviteResponse = await client2.PostAsJsonAsync(
            requestUri: $"/organizations/invites/accept",
            new AcceptOrganizationInviteRequest(invite!.Id),
            cancellationToken: TestContext.Current.CancellationToken);

        inviteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}