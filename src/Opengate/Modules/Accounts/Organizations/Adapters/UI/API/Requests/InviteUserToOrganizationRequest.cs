using FluentValidation;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

public class InviteUserToOrganizationRequest
{
    public string TargetEmail { get; set; } = string.Empty;

    public string TargetUserFirstName { get; set; } = string.Empty;
    public string TargetUserLastName { get; set; } = string.Empty;
}

public class InviteUserToOrganizationResponse
{
}

public class InviteUserToOrganizationRequestValidator : AbstractValidator<InviteUserToOrganizationRequest>
{
    public InviteUserToOrganizationRequestValidator()
    {
        RuleFor(x => x.TargetEmail).NotEmpty().EmailAddress();

        RuleFor(x => x.TargetUserFirstName).NotEmpty();
        RuleFor(x => x.TargetUserLastName).NotEmpty();
    }
}