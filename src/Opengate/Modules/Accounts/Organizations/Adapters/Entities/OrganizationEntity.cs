using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Shared.Domain.ValueObjects.Emails;

namespace Opengate.Modules.Accounts.Organizations.Adapters.Entities;

public class OrganizationEntity
{
    /// <summary>
    /// Unique identifier for the organization.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// URL-friendly slug generated from the organization's name.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Name of the organization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the organization. In markdown format.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the organization owner.
    /// </summary>
    public string OwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the organization owner.
    /// </summary>
    public string OwnerPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Document identifier (e.g., registration number) for the organization.
    /// </summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>
    /// Type of document associated with the organization.
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Country where the organization is registered.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Logo image URL or path for the organization.
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// Timestamp when the organization was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the organization was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the organization is marked as deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the organization was deleted, if applicable.
    /// </summary>
    public long DeletedAt { get; set; }

    public static OrganizationEntity From(Organization organization)
    => new()
    {
        Id = organization.Id,
        Slug = organization.Slug.ToString(),
        Name = organization.Name,
        Description = organization.Description,
        OwnerEmail = organization.OwnerEmail.ToString(),
        OwnerPhoneNumber = organization.OwnerPhoneNumber,
        Document = organization.Document,
        DocumentType = organization.DocumentType,
        Country = organization.Country,
        Logo = organization.Logo,
        CreatedAt = organization.CreatedAt,
        UpdatedAt = organization.UpdatedAt,
        IsDeleted = false,
        DeletedAt = 0
    };

    public Organization ToOrganization()
    => Organization.Init(
            id: Id,
            slug: Shared.Domain.ValueObjects.Slugs.Slug.Parse(Slug),
            name: Name,
            description: Description,
            ownerEmail: Email.Parse(OwnerEmail),
            ownerPhoneNumber: OwnerPhoneNumber,
            document: Document,
            documentType: DocumentType,
            country: Country,
            logo: Logo,
            createdAt: CreatedAt,
            updatedAt: UpdatedAt
    );
}