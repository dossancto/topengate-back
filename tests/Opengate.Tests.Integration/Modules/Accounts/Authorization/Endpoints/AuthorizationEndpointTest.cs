using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Opengate.Modules.Accounts.Users.Domain.Enum;
using Opengate.Modules.Accounts.Users.Models;

namespace Opengate.Tests.Integration.Modules.Accounts.Authorization.Endpoints;

public class CreateUserAndValidateAuthorizationEndpointTest : IClassFixture<AppFactory>
{
    private readonly AppFactory _appFactory;

    public CreateUserAndValidateAuthorizationEndpointTest(AppFactory appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _appFactory.OutputHelper = outputHelper;
    }

    internal void Dispose()
    {
        _appFactory.OutputHelper = null;
    }

    [Fact]
    public async Task PolicyAuthorization_ShouldRespectRoleHierarchy()
    {
        using var scope = _appFactory.Services.CreateScope();

        var adminClient = await CreateUserWithRoleAsync(scope, ApplicationRoles.ADMIN);
        var staffClient = await CreateUserWithRoleAsync(scope, ApplicationRoles.STAFF);
        var operatorClient = await CreateUserWithRoleAsync(scope, ApplicationRoles.OPERATOR);

        await AssertPolicyAccess(adminClient, "admin", HttpStatusCode.OK);
        await AssertPolicyAccess(adminClient, "staff", HttpStatusCode.Forbidden);
        await AssertPolicyAccess(adminClient, "operator", HttpStatusCode.Forbidden);

        await AssertPolicyAccess(staffClient, "admin", HttpStatusCode.Forbidden);
        await AssertPolicyAccess(staffClient, "staff", HttpStatusCode.OK);
        await AssertPolicyAccess(staffClient, "operator", HttpStatusCode.Forbidden);

        await AssertPolicyAccess(operatorClient, "admin", HttpStatusCode.Forbidden);
        await AssertPolicyAccess(operatorClient, "staff", HttpStatusCode.Forbidden);
        await AssertPolicyAccess(operatorClient, "operator", HttpStatusCode.OK);
    }

    private async Task<HttpClient> CreateUserWithRoleAsync(IServiceScope scope, ApplicationRoles role)
    {
        var email = $"test{Guid.NewGuid()}@test.com";
        var password = "Test123#";

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;

        var user = new ApplicationUser();

        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user for policy authorization test.");
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        result = await userManager.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
        {
            throw new Exception("Failed to confirm email for policy authorization test.");
        }

        result = await userManager.AddToRoleAsync(user, role.ToString());
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to add role {role} to user for policy authorization test.");
        }

        var client = _appFactory.CreateDefaultClient();

        var login = await client.PostAsJsonAsync("/login", new
        {
            email = email,
            password = password
        }, cancellationToken: TestContext.Current.CancellationToken);

        login.EnsureSuccessStatusCode();

        var response = await login.Content.ReadFromJsonAsync<AccessTokenResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.AccessToken);
        client.DefaultRequestHeaders.Add("refresh_token", response.RefreshToken);

        return client;
    }

    private static async Task AssertPolicyAccess(HttpClient client, string roleRoute, HttpStatusCode expectedStatusCode)
    {
        var response = await client.PostAsync($"/test/auth/policy/{roleRoute}", null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldAffectPolicyAuthorization()
    {
        using var scope = _appFactory.Services.CreateScope();

        var (client, user) = await _appFactory.GetAuthorizedClientNewUser(scope);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var organizationId = Guid.Parse(user.OrganizationId!);
        var userId = Guid.Parse(user.Id);

        var updateToOperator = await client.PatchAsJsonAsync(
            $"/organizations/{organizationId}/users/{userId}/role",
            new { Role = ApplicationRoles.OPERATOR },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, updateToOperator.StatusCode);

        await _appFactory.RefreshClient(client);

        await AssertPolicyAccess(client, "OPERATOR", HttpStatusCode.OK);
        await AssertPolicyAccess(client, "STAFF", HttpStatusCode.Forbidden);
        await AssertPolicyAccess(client, "ADMIN", HttpStatusCode.Forbidden);

        var updateToAdmin = await client.PatchAsJsonAsync(
            $"/organizations/{organizationId}/users/{userId}/role",
            new { Role = ApplicationRoles.ADMIN },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, updateToAdmin.StatusCode);

        await _appFactory.RefreshClient(client);

        await AssertPolicyAccess(client, "OPERATOR", HttpStatusCode.Forbidden);
        await AssertPolicyAccess(client, "STAFF", HttpStatusCode.Forbidden);
        await AssertPolicyAccess(client, "ADMIN", HttpStatusCode.OK);
    }
}
