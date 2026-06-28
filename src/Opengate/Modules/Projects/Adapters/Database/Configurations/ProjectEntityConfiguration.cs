using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Opengate.Modules.Projects.Models;

namespace Opengate.Modules.Projects.Adapters.Database.Configurations;

public class ProjectEntityConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> b)
    {
        b.ToTable("Projects");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        b.Property(x => x.Description)
            .HasMaxLength(500);

        b.Property(x => x.CreatorUserId)
            .IsRequired();

        b.Property(x => x.CreatorOrganizationId)
            .IsRequired();

        b.Property(x => x.ApiKey)
            .IsRequired()
            .HasMaxLength(255);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .IsRequired();

        b.Property(x => x.DeletedAt)
            .IsRequired();
    }
}