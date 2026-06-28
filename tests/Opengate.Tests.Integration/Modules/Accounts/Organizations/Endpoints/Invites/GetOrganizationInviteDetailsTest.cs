using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints.Invites;

public class GetOrganizationInviteDetailsTest : IClassFixture<AppFactory>
{
    readonly JsonSerializerOptions options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AppFactory _appFactory;

    public GetOrganizationInviteDetailsTest(AppFactory appFactory, ITestOutputHelper outputHelper)
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
    public async Task GetOrganizationInviteDetailsTest_ShouldReturnOrganizationInviteDetailsAsync()
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

        var inviteResponse = await client.GetFromJsonAsync<GetInviteDetailsResponse>(
            requestUri: $"/organizations/invites/{invite!.Id}/details",
            cancellationToken: TestContext.Current.CancellationToken);

        inviteResponse!.Organization.Name.ShouldBe(createOrgRequest.Name);
        inviteResponse!.Organization.Description.ShouldBe(createOrgRequest.Description);
        inviteResponse!.Organization.Logo.ShouldBe(createOrgRequest.Logo);
        inviteResponse!.InvitedUser.Email.ToUpper().ShouldBe(createOrgRequest.OwnerEmail.ToUpper());
    }

    [Fact]
    public async Task GetOrganizationInviteDetailsTest_ShouldReturnNotFound_WhenInvalidInviteId()
    {
        var client = await _appFactory.GetAuthorizedClient();

        var invalidInviteId = Guid.NewGuid();

        var inviteResponse = await client.GetAsync(
            requestUri: $"/organizations/invites/{invalidInviteId}/details",
            cancellationToken: TestContext.Current.CancellationToken);

        inviteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}