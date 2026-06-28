using Microsoft.EntityFrameworkCore;

using Namotion.Reflection;

using Opengate.Modules.Accounts.Organizations.Adapters.Entities;

namespace Opengate.Modules.Accounts.Organizations.Adapters.EntityConfigurations;

public class OrganizationEntityConfiguration : IEntityTypeConfiguration<OrganizationEntity>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OrganizationEntity> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Slug)
            .HasMaxLength(255)
            .HasColumnName("slug")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1024)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(x => x.OwnerEmail)
            .HasMaxLength(255)
            .HasColumnName("owner_email")
            .IsRequired();

        builder.Property(x => x.OwnerPhoneNumber)
            .HasMaxLength(255)
            .HasColumnName("owner_phone_number")
            .IsRequired();

        builder.Property(x => x.Document)
            .HasMaxLength(255)
            .HasColumnName("document")
            .IsRequired();

        builder.Property(x => x.DocumentType)
            .HasMaxLength(255)
            .HasColumnName("document_type")
            .IsRequired();

        builder.Property(x => x.Country)
            .HasMaxLength(255)
            .HasColumnName("country")
            .IsRequired();

        builder.Property(x => x.Logo)
            .HasMaxLength(255)
            .HasColumnName("logo")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired()
            ;

        builder.HasIndex(x => new { x.Slug, x.DeletedAt })
            .HasDatabaseName("IX_organizations_slug_deleted_at")
            .IsUnique();
    }
}