using System.Net;
using System.Net.Http.Json;

using Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

namespace Opengate.Tests.Integration.Modules.Accounts.Organizations.Endpoints;

public class CreateOrganizationEndpointTest : IClassFixture<AppFactory>, IDisposable
{
    private readonly AppFactory _appFactory;

    public CreateOrganizationEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _appFactory.OutputHelper = outputHelper;
    }

    public void Dispose()
    {
        _appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task CreateOrganization_ShouldReturnCreatedOrganization()
    {
        var client = await _appFactory.GetAuthorizedClient();

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

        var statusCode = response.StatusCode;

        Assert.Equal(HttpStatusCode.Created, statusCode);
    }

    [Fact]
    public async Task CreateOrganization_ShouldReturnConflictStatusCode_WhenOrganizationSlugAlreadyExists()
    {
        var client = await _appFactory.GetAuthorizedClient();

        var name = $"Test Organization {Guid.NewGuid()}";

        var mockData = await client.PostAsJsonAsync("/organizations", new CreateOrganizationRequest(
                    Name: name,
                    Description: "Test organization description",
                    OwnerEmail: "test@test.com",
                    OwnerPhoneNumber: "12345678",
                    Document: "12345678",
                    DocumentType: "test",
                    Country: "test",
                    Logo: "test"
        )
        , cancellationToken: TestContext.Current.CancellationToken);

        mockData.EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/organizations", new CreateOrganizationRequest(
                    Name: name,
                    Description: "Test organization description",
                    OwnerEmail: "test@test.com",
                    OwnerPhoneNumber: "12345678",
                    Document: "12345678",
                    DocumentType: "test",
                    Country: "test",
                    Logo: "test"
        )
        , cancellationToken: TestContext.Current.CancellationToken);

        var statusCode = response.StatusCode;

        Assert.Equal(HttpStatusCode.Conflict, statusCode);
    }

    [Fact]
    public async Task CreateOrganization_ShouldReturnBadRequest_WhenValidationFails()
    {
        var client = await _appFactory.GetAuthorizedClient();

        var response = await client.PostAsJsonAsync("/organizations", new CreateOrganizationRequest(
                    Name: "",
                    Description: "",
                    OwnerEmail: "test@test.com",
                    OwnerPhoneNumber: "12345678",
                    Document: "12345678",
                    DocumentType: "",
                    Country: "",
                    Logo: ""
        )
        , cancellationToken: TestContext.Current.CancellationToken);

        var statusCode = response.StatusCode;

        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }
}