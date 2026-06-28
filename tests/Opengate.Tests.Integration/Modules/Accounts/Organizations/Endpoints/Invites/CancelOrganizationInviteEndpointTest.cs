using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;
using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints.Invites;

public class CancelOrganizationInviteEndpointTest : IClassFixture<AppFactory>
{
    readonly JsonSerializerOptions options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AppFactory _appFactory;

    public CancelOrganizationInviteEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
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
    public async Task CancelOrganizationInviteEndpointTest_ShouldCancelInviteAsync()
    {
        using var scope = _appFactory.Services.CreateScope();

        var (client, user) = await _appFactory.GetAuthorizedClientNewUser(scope);
        var db = _appFactory.GetApplicationDbContext(scope);

        var response = await client.PostAsJsonAsync("/organizations", new CreateOrganizationRequest(
                    Name: $"Test Organization {Guid.NewGuid()}",
                    Description: "Test organization description",
                    OwnerEmail: user.Email!,
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

        var inviteUserToOrganization = new InviteUserToOrganizationRequest()
        {
            TargetEmail = $"newuser{Guid.NewGuid()}@test.com",
            TargetUserFirstName = "New",
            TargetUserLastName = "User"
        };

        var newUserInviteResponse = await client.PostAsJsonAsync($"/organizations/invites",
            inviteUserToOrganization,
            cancellationToken: TestContext.Current.CancellationToken);

        newUserInviteResponse.EnsureSuccessStatusCode();

        var inviteCreatedOnDatabase = await db.OrganizationInvites.FirstOrDefaultAsync(
            x => x.Email.ToUpper() == inviteUserToOrganization.TargetEmail.ToUpper()
            && x.Status == OrganizationInviteStatus.Pending
            && x.IsDeleted == false
            && x.OrganizationId == createdOrganizationContent!.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        inviteCreatedOnDatabase.ShouldNotBeNull();

        var cancellInviteResponse = await client.DeleteAsync(
            requestUri: $"/organizations/invites/{inviteCreatedOnDatabase.Id}/cancel",
            cancellationToken: TestContext.Current.CancellationToken);

        cancellInviteResponse.EnsureSuccessStatusCode();

        var inviteAfterCancellation = await db.OrganizationInvites.FirstOrDefaultAsync(
            x => x.Id == inviteCreatedOnDatabase.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        inviteAfterCancellation!.Status.ShouldBe(OrganizationInviteStatus.Cancelled);
        inviteAfterCancellation!.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelOrganizationInviteEndpointTest_ShouldNotFound_WhenInviteDoesNotExists()
    {
        var client = await _appFactory.GetAuthorizedClient();

        var cancellInviteResponse = await client.DeleteAsync(
            requestUri: $"/organizations/invites/{Guid.NewGuid()}/cancel",
            cancellationToken: TestContext.Current.CancellationToken);

        cancellInviteResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelOrganizationInviteEndpointTest_ShouldNotFound_WhenUserAlreadyAccept()
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

        var cancellInviteResponse = await client.DeleteAsync(
            requestUri: $"/organizations/invites/{invite.Id}/cancel",
            cancellationToken: TestContext.Current.CancellationToken);

        cancellInviteResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }
}