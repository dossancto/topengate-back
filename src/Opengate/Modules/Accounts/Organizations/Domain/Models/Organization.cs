using System.Text.RegularExpressions;

using Opengate.Modules.Shared.Domain.ValueObjects.Emails;
using Opengate.Modules.Shared.Domain.ValueObjects.Slugs;

namespace Opengate.Modules.Accounts.Organizations.Domain.Models;

/// <summary>
/// Represents an organization entity with identification, ownership, and metadata.
/// </summary>
public record Organization
{ /// <summary>
  /// Unique identifier for the organization.
  /// </summary>
    public Guid Id { get; private init; }

    /// <summary>
    /// URL-friendly slug generated from the organization's name.
    /// </summary>
    public Slug Slug { get; private init; }

    /// <summary>
    /// Name of the organization.
    /// </summary>
    public string Name { get; private init; } = string.Empty;

    /// <summary>
    /// Description of the organization. In markdown format.
    /// </summary>
    public string Description { get; private init; } = string.Empty;

    /// <summary>
    /// Email address of the organization owner.
    /// </summary>
    public Email OwnerEmail { get; private init; }

    /// <summary>
    /// Phone number of the organization owner.
    /// </summary>
    public string OwnerPhoneNumber { get; private init; } = string.Empty;

    /// <summary>
    /// Document identifier (e.g., registration number) for the organization.
    /// </summary>
    public string Document { get; private init; } = string.Empty;

    /// <summary>
    /// Type of document associated with the organization.
    /// </summary>
    public string DocumentType { get; private init; } = string.Empty;

    /// <summary>
    /// Country where the organization is registered.
    /// </summary>
    public string Country { get; private init; } = string.Empty;

    /// <summary>
    /// Logo image URL or path for the organization.
    /// </summary>
    public string? Logo { get; private init; }

    /// <summary>
    /// Timestamp when the organization was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Timestamp when the organization was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private init; }

    /// <summary>
    /// Creates a new <see cref="Organization"/> instance with the provided details.
    /// </summary>
    /// <param name="name">Name of the organization.</param>
    /// <param name="description">Description of the organization. In Markdown format</param>
    /// <param name="ownerEmail">Email address of the owner.</param>
    /// <param name="ownerPhoneNumber">Phone number of the owner.</param>
    /// <param name="document">Document identifier for the organization.</param>
    /// <param name="documentType">Type of document.</param>
    /// <param name="country">Country of registration.</param>
    /// <param name="logo">Logo image URL or path.</param>
    /// <returns>A new <see cref="Organization"/> instance.</returns>
    public static Organization Create(
            string name,
            string description,
            Email ownerEmail,
            string ownerPhoneNumber,
            string document,
            string documentType,
            string country,
            string? logo
    )
    {
        var slug = Slug.Parse(name);

        var timestamp = DateTime.UtcNow;

        return new Organization
        {
            Id = Guid.CreateVersion7(),
            Slug = slug,
            Name = name,
            Description = description,
            OwnerEmail = ownerEmail,
            OwnerPhoneNumber = ownerPhoneNumber,
            Document = document,
            DocumentType = documentType,
            Country = country.ToUpperInvariant(),
            Logo = logo,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };
    }

    /// <summary>
    /// Updates the organization's name, description, and logo.
    /// </summary>
    /// <param name="name">New name for the organization.</param>
    /// <param name="description">New description for the organization.</param>
    /// <param name="logo">New logo image URL or path.</param>
    /// <returns>An updated <see cref="Organization"/> instance.</returns>
    public Organization Update(
        string name,
        string description,
        string logo
    )
    {
        var timestamp = DateTime.UtcNow;

        return this with
        {
            Name = name,
            Description = description,
            Logo = logo,
            UpdatedAt = timestamp
        };
    }

    /// <summary>
    /// Transfers ownership of the organization to a new owner.
    /// </summary>
    /// <param name="newOwnerEmail">Email address of the new owner.</param>
    /// <param name="newOwnerPhoneNumber">Phone number of the new owner.</param>
    /// <returns>An updated <see cref="Organization"/> instance with new ownership details.</returns>
    public Organization TransferOwner(
            Email newOwnerEmail,
            string newOwnerPhoneNumber
    )
    {
        var timestamp = DateTime.UtcNow;

        return this with
        {
            OwnerEmail = newOwnerEmail,
            OwnerPhoneNumber = newOwnerPhoneNumber,
            UpdatedAt = timestamp
        };
    }


    /// <summary>
    /// Initializes a new <see cref="Organization"/> instance with all required data.
    /// </summary>
    /// <param name="id">Unique identifier for the organization.</param>
    /// <param name="slug">URL-friendly slug generated from the organization's name.</param>
    /// <param name="name">Name of the organization.</param>
    /// <param name="description">Description of the organization. In markdown format.</param>
    /// <param name="ownerEmail">Email address of the organization owner.</param>
    /// <param name="ownerPhoneNumber">Phone number of the organization owner.</param>
    /// <param name="document">Document identifier for the organization.</param>
    /// <param name="documentType">Type of document associated with the organization.</param>
    /// <param name="country">Country where the organization is registered.</param>
    /// <param name="logo">Logo image URL or path for the organization.</param>
    /// <param name="createdAt">Timestamp when the organization was created.</param>
    /// <param name="updatedAt">Timestamp when the organization was last updated.</param>
    /// <returns>A new <see cref="Organization"/> instance.</returns>
    public static Organization Init(
        Guid id,
        Slug slug,
        string name,
        string description,
        Email ownerEmail,
        string ownerPhoneNumber,
        string document,
        string documentType,
        string country,
        string? logo,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        return new Organization
        {
            Id = id,
            Slug = slug,
            Name = name,
            Description = description,
            OwnerEmail = ownerEmail,
            OwnerPhoneNumber = ownerPhoneNumber,
            Document = document,
            DocumentType = documentType,
            Country = country,
            Logo = logo,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}