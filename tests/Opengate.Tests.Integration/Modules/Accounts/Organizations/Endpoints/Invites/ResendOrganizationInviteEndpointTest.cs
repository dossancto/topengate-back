using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;
using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints.Invites;

public class ResendOrganizationInviteEndpointTest : IClassFixture<AppFactory>, IDisposable
{
    private readonly AppFactory _appFactory;

    public ResendOrganizationInviteEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _appFactory.OutputHelper = outputHelper;
    }

    public void Dispose()
    {
        _appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task ResendOrganizationInvite_ShouldReturnSuccess_WhenValidInvite()
    {
        using var scope = _appFactory.Services.CreateScope();
        var (client, user) = await _appFactory.GetAuthorizedClientNewUser(scope);
        var db = _appFactory.GetApplicationDbContext(scope);

        var response = await client.PostAsJsonAsync("/organizations", new CreateOrganizationRequest(
                    Name: $"Test Organization {Guid.NewGuid()}",
                    Description: "Test organization description",
                    OwnerEmail: user.Email,
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
            && x.OrganizationId == createdOrganizationContent.Id
            ,
            cancellationToken: TestContext.Current.CancellationToken);

        inviteCreatedOnDatabase.ShouldNotBeNull();

        var resendResponse = await client.PatchAsync($"/organizations/invites/{inviteCreatedOnDatabase!.Id}/resend", null, TestContext.Current.CancellationToken);

        resendResponse.EnsureSuccessStatusCode();
    }
}