using FluentValidation;

using Opengate.Modules.Accounts.Users.Domain.Enum;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

public record UpdateUserRoleInOrganizationRequest(ApplicationRoles Role);

public class UpdateUserRoleInOrganizationRequestValidator : AbstractValidator<UpdateUserRoleInOrganizationRequest>
{
    public UpdateUserRoleInOrganizationRequestValidator()
    {
        RuleFor(x => x.Role).NotNull().IsInEnum().WithMessage("Invalid role");
    }
}