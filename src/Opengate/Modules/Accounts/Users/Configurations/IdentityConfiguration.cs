using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using Opengate.Modules.Accounts.Users.Domain.Enum;
using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Adapters.Databases;

namespace Opengate.Modules.Accounts.Users.Configurations;

public static class IdentityConfiguration
{
    private static readonly string JwtKey = AppEnv.APP.AUTHENTICATION.JWT.KEY.NotNull();
    private static readonly string JwtIssuer = AppEnv.APP.AUTHENTICATION.JWT.ISSUER.NotNull();
    private static readonly string JwtAudience = AppEnv.APP.AUTHENTICATION.JWT.AUDIENCE.NotNull();

    public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
              {
                  options.SignIn.RequireConfirmedEmail = true;
                  options.User.RequireUniqueEmail = true;
                  options.Password.RequireDigit = true;
                  options.Password.RequireLowercase = true;
                  options.Password.RequireUppercase = true;
                  options.Password.RequireNonAlphanumeric = true;
                  options.Password.RequiredLength = 8;

                  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                  options.Lockout.MaxFailedAccessAttempts = 5;
                  options.Lockout.AllowedForNewUsers = true;

              });

        services
             .AddIdentityCore<ApplicationUser>(o => o.User.RequireUniqueEmail = true)
             .AddRoles<IdentityRole>()
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)
             .AddSignInManager()
          ;

        services
        .AddAuthentication()
        .AddJwtBearer("Identity.Bearer", jwtOptions =>
        {
            jwtOptions.Audience = JwtAudience;
            jwtOptions.ClaimsIssuer = JwtIssuer;
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                // ValidateIssuer = true,
                // ValidateAudience = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtKey)),
                // ValidAudiences = builder.Configuration.GetSection("Api:ValidAudiences").Get<string[]>(),
                // ValidIssuers = builder.Configuration.GetSection("Api:ValidIssuers").Get<string[]>()
            };
        });
        ;

        services.AddAuthorization();
        return services;
    }

    public static async Task SeedIdentityRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Enum.GetValues<ApplicationRoles>())
        {
            if (!await roleManager.RoleExistsAsync(role.ToString()))
                await roleManager.CreateAsync(new IdentityRole(role.ToString()));
        }
    }
}