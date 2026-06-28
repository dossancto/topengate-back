using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using DotNetEnv;

using MartinCostello.Logging.XUnit;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Opengate.Modules.Accounts.Organizations.Adapters.Entities;
using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Accounts.Users.Domain.Enum;
using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Adapters.Databases;
using Opengate.Modules.Shared.Domain.ValueObjects.Emails;

namespace Opengate.Tests.Integration.Configuration;

public class AppFactory
    : WebApplicationFactory<Program>,
    ITestOutputHelperAccessor
{
    public AppFactory()
    {
        Env.TraversePath().Load();
    }

    public ITestOutputHelper? OutputHelper { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging((logging) =>
        {
            logging.ClearProviders();
            logging.AddXUnit(this);
            logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);
            logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        });

        builder.ConfigureServices(services =>
        {
            services.ConfigureHttpJsonOptions(options
                    =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            var dbContext = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.MigrateAsync();
        });

        builder.UseEnvironment("Development");
    }

    public async Task<HttpClient> GetAuthorizedClient()
    {
        var client = CreateDefaultClient();

        var login = await client.PostAsJsonAsync("/login", new
        {
            email = "test@test.com",
            password = "Test123#"
        });

        login.EnsureSuccessStatusCode();

        var response = await login.Content.ReadFromJsonAsync<AccessTokenResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.AccessToken);
        client.DefaultRequestHeaders.Add("refresh_token", response.RefreshToken);

        return client;
    }

    public async Task<HttpClient> RefreshClient(HttpClient client)
    {
        var refresh = await client.PostAsJsonAsync("/refresh", new
        {
            refreshToken = client.DefaultRequestHeaders.GetValues("refresh_token").First()
        });

        refresh.EnsureSuccessStatusCode();

        var response = await refresh.Content.ReadFromJsonAsync<AccessTokenResponse>();

        OutputHelper?.WriteLine(response?.AccessToken ?? "Token is null");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.AccessToken);
        client.DefaultRequestHeaders.Remove("refresh_token");
        client.DefaultRequestHeaders.Add("refresh_token", response.RefreshToken);

        return client;
    }

    public async Task<(HttpClient, ApplicationUser)> GetAuthorizedClientNewUserNoOrganization(IServiceScope scope)
    {
        var email = $"test{Guid.NewGuid()}@test.com";
        var password = "Test123#";

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();

        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;

        var user = new ApplicationUser()
        {
        };

        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user for authorized client.");
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

        result = await userManager.ConfirmEmailAsync(user, code);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to confirm email for authorized client.");
        }

        var client = CreateDefaultClient();

        var login = await client.PostAsJsonAsync("/login", new
        {
            email = email,
            password = password
        });

        login.EnsureSuccessStatusCode();

        var response = await login.Content.ReadFromJsonAsync<AccessTokenResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.AccessToken);
        client.DefaultRequestHeaders.Add("refresh_token", response.RefreshToken);

        return (client, user);
    }

    public async Task<(HttpClient, ApplicationUser)> GetAuthorizedClientNewUser(IServiceScope scope)
    {
        var email = $"test{Guid.NewGuid()}@test.com";
        var password = "Test123#";

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organization = db.Organizations.Add(OrganizationEntity.From(
            organization: Organization.Create(
                name: "Test Organization " + Guid.NewGuid(),
                description: "Test organization description",
                ownerEmail: Email.Parse(email),
                ownerPhoneNumber: "12345678",
                document: "12345678",
                documentType: "test",
                country: "test",
                logo: "test"
            )
        ));

        await db.SaveChangesAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();

        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;

        var user = new ApplicationUser()
        {
            OrganizationId = organization.Entity.Id.ToString()
        };

        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user for authorized client.");
        }

        await userManager.AddToRoleAsync(user, ApplicationRoles.ADMIN.ToString());

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

        result = await userManager.ConfirmEmailAsync(user, code);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to confirm email for authorized client.");
        }

        var client = CreateDefaultClient();

        var login = await client.PostAsJsonAsync("/login", new
        {
            email = email,
            password = password
        });

        login.EnsureSuccessStatusCode();

        var response = await login.Content.ReadFromJsonAsync<AccessTokenResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.AccessToken);
        client.DefaultRequestHeaders.Add("refresh_token", response.RefreshToken);

        return (client, user);
    }

    /// <summary>
    /// Gets the application database context.
    /// </summary>
    /// <returns>The application database context.</returns>
    public ApplicationDbContext GetApplicationDbContext(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}