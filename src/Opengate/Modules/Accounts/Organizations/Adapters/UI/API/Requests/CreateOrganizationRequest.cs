using FluentValidation;

using Opengate.Modules.Accounts.Organizations.Application.InitOrganization;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

/// <summary>
/// Request model for creating a new organization.
/// </summary>
/// <param name="Name">The name of the organization.</param>
/// <param name="Description">A description of the organization.</param>
/// <param name="OwnerEmail">The email address of the organization owner.</param>
/// <param name="OwnerPhoneNumber">The phone number of the organization owner.</param>
/// <param name="Document">The document identifier for the organization.</param>
/// <param name="DocumentType">The type of document provided.</param>
/// <param name="Country">The country where the organization is registered.</param>
/// <param name="Logo">The logo of the organization (optional).</param>
public record CreateOrganizationRequest
(
    string Name,
    string Description,
    string OwnerEmail,
    string OwnerPhoneNumber,
    string Document,
    string DocumentType,
    string Country,
    string? Logo
)
{
    /// <summary>
    /// Converts this request to an <see cref="InitOrganizationInput"/>.
    /// </summary>
    /// <returns>An <see cref="InitOrganizationInput"/> initialized with the request data.</returns>
    public InitOrganizationInput ToInitOrganizationInput()
    => new(
        Name: Name,
        Description: Description,
        OwnerEmail: OwnerEmail,
        OwnerPhoneNumber: OwnerPhoneNumber,
        Document: Document,
        DocumentType: DocumentType,
        Country: Country,
        Logo: Logo
    );
}

/// <summary>
/// Response model containing the ID of the created organization.
/// </summary>
/// <param name="Id">The unique identifier of the created organization.</param>
public record CreateOrganizationResponse(Guid Id);

/// <summary>
/// Validator for <see cref="CreateOrganizationRequest"/>.
/// </summary>
public class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.OwnerEmail).NotEmpty().MaximumLength(255);
        RuleFor(x => x.OwnerPhoneNumber).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Document).NotEmpty().MaximumLength(255);
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(255);
    }
}