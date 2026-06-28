using DotNetEnv;

using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

using Opengate.Modules.Accounts;
using Opengate.Modules.Accounts.Users.Configurations;
using Opengate.Modules.Accounts.Users.Domain.Enum;
using Opengate.Modules.Shared;
using Opengate.Modules.Shared.Adapters.Databases;
using Opengate.Modules.Test;
using Opengate.Modules.Projects;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services
    .AddSharedModule()
    .AddAccountsModule()
    .AddTestModule()
    .AddProjectsModule()
    ;

//Policies
var authBuilder = builder.Services.AddAuthorizationBuilder();
foreach (var role in Enum.GetNames<ApplicationRoles>())
{
    authBuilder.AddPolicy(role, policy => policy.RequireRole(role));
}

var app = builder.Build();

// Apply EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Seed identity roles
await app.SeedIdentityRolesAsync();

app.UseCors("AllowAll");

app.MapOpenApi();
app.MapScalarApiReference(
    options => options
    .AddPreferredSecuritySchemes("BearerAuth")
    .AddHttpAuthentication("BearerAuth", auth => auth.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")
    );

app.UseHttpsRedirection();

app
    .UseSharedModule()
    .UseAccountsModule()
    .UseTestModule()
    .UseProjectsModule()
    ;

app.Run();