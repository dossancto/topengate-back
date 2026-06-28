using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Accounts.Organizations.Adapters.Entities;
using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Accounts.Users.Models;

namespace Opengate.Modules.Shared.Adapters.Databases;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<OrganizationEntity> Organizations { get; set; } = null!;
    public DbSet<OrganizationInvite> OrganizationInvites { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
    }
}