using Opengate.Modules.Shared.Domain.ValueObjects.Emails;
using Opengate.Modules.Shared.Domain.ValueObjects.Slugs;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

/// <summary>
/// Request to retrieve details for a specific organization.
/// </summary>
/// <example>
/// var request = new GetOrganizationDetailsRequest();
/// </example>
public record GetOrganizationDetailsRequest();

/// <summary>
/// Response containing detailed information about an organization.
/// </summary>
/// <example>
/// var response = new GetOrganizationDetailsResponse
/// {
///     Id = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
///     Name = "Acme Corporation",
///     Slug = new Slug("acme-corp"),
///     Description = "Leading provider of business solutions.",
///     OwnerEmail = new Email("owner@acme.com"),
///     Logo = "https://cdn.acme.com/logo.png",
///     CreatedAt = new DateTime(2020, 1, 15, 10, 30, 0)
/// };
/// </example>
public record class GetOrganizationDetailsResponse
{
    /// <summary>
    /// Unique identifier of the organization.
    /// </summary>
    /// <example>
    /// d290f1ee-6c54-4b01-90e6-d701748f0851
    /// </example>
    public Guid Id { get; init; }

    /// <summary>
    /// Name of the organization.
    /// </summary>
    /// <example>
    /// Acme Corporation
    /// </example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// URL-friendly slug for the organization.
    /// </summary>
    /// <example>acme-corp</example>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Description of the organization.
    /// </summary>
    /// <example>
    /// Leading provider of business solutions.
    /// </example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Email address of the organization owner.
    /// </summary>
    /// <example>
    /// owner@acme.com
    /// </example>
    public string OwnerEmail { get; init; } = string.Empty;

    /// <summary>
    /// URL to the organization's logo image (optional).
    /// </summary>
    /// <example>
    /// https://cdn.acme.com/logo.png
    /// </example>
    public string? Logo { get; init; }

    /// <summary>
    /// Date and time when the organization was created.
    /// </summary>
    /// <example>
    /// 2020-01-15T10:30:00
    /// </example>
    public DateTime CreatedAt { get; init; }

}