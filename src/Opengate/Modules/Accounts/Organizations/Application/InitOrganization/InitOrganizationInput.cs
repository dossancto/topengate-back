namespace Opengate.Modules.Accounts.Organizations.Application.InitOrganization;

/// <summary>
/// Represents the input data required to initialize a new organization.
/// </summary>
/// <param name="Name">The name of the organization.</param>
/// <param name="Description">A brief description of the organization.</param>
/// <param name="OwnerEmail">The email address of the organization owner.</param>
/// <param name="OwnerPhoneNumber">The phone number of the organization owner.</param>
/// <param name="Document">The identification document of the organization.</param>
/// <param name="DocumentType">The type of identification document.</param>
/// <param name="Country">The country where the organization is registered.</param>
/// <param name="Logo">The logo of the organization (optional).</param>
public record InitOrganizationInput
(
    string Name,
    string Description,
    string OwnerEmail,
    string OwnerPhoneNumber,
    string Document,
    string DocumentType,
    string Country,
    string? Logo
);

/// <summary>
/// Represents the response after initializing a new organization.
/// </summary>
/// <param name="Id">The unique identifier of the newly created organization.</param>
public record InitOrganizationResponse
(
    Guid Id
);