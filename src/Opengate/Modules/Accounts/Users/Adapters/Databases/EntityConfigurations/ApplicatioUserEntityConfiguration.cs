using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Accounts.Users.Models;

namespace Opengate.Modules.Accounts.Users.Adapters.Databases.EntityConfigurations;

public class ApplicatioUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
    }
}