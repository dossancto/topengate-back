using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;
using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints.Invites;

public class InviteUserToOrganizationEndpointTest : IClassFixture<AppFactory>
{
    readonly JsonSerializerOptions options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AppFactory _appFactory;

    public InviteUserToOrganizationEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
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
    public async Task InviteUserToOrganizationEndpointTest_ShouldCreateNewInviteAsync()
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

        var invitedUser = await db.Users.FirstOrDefaultAsync(
            x => x.Email.ToUpper() == inviteUserToOrganization!.TargetEmail.ToUpper()
            && x.OrganizationId == createdOrganizationContent!.Id.ToString(),
            cancellationToken: TestContext.Current.CancellationToken);

        invitedUser.ShouldNotBeNull();

        invitedUser!.FirstName.ShouldBe(inviteUserToOrganization.TargetUserFirstName);
        invitedUser.LastName.ShouldBe(inviteUserToOrganization.TargetUserLastName);
    }

    [Fact]
    public async Task InviteUserToOrganizationEndpointTest_ShouldReturnFail_WhenInvalidEmail()
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

        var inviteUserToOrganization = new InviteUserToOrganizationRequest()
        {
            TargetEmail = $"invalid email",
            TargetUserFirstName = "New",
            TargetUserLastName = "User"
        };

        var newUserInviteResponse = await client.PostAsJsonAsync($"/organizations/invites",
            inviteUserToOrganization,
            cancellationToken: TestContext.Current.CancellationToken);

        newUserInviteResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InviteUserToOrganizationEndpointTest_ShouldReturnFail_WhenUserAlreadyInvited()
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

        var duplicateInviteResponse = await client.PostAsJsonAsync($"/organizations/invites",
            inviteUserToOrganization,
            cancellationToken: TestContext.Current.CancellationToken);

        duplicateInviteResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InviteUserToOrganizationEndpointTest_ShouldReturnFail_WhenInviterUserHasNoOrganization()
    {
        using var scope = _appFactory.Services.CreateScope();
        var (client, user) = await _appFactory.GetAuthorizedClientNewUserNoOrganization(scope);
        var db = _appFactory.GetApplicationDbContext(scope);

        var inviteUserToOrganization = new InviteUserToOrganizationRequest()
        {
            TargetEmail = $"newuser{Guid.NewGuid()}@test.com",
            TargetUserFirstName = "New",
            TargetUserLastName = "User"
        };

        var newUserInviteResponse = await client.PostAsJsonAsync($"/organizations/invites",
            inviteUserToOrganization,
            cancellationToken: TestContext.Current.CancellationToken);

        newUserInviteResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }
}