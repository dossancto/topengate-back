using FluentValidation;

using Opengate.Modules.Accounts.Organizations.Application.ChangeInviteStatus;
using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

public record AcceptOrganizationInviteRequest(
        Guid InviteId
)
{
    public ChangeInviteStatusInput ToInput(string userId, string userEmail)
    => new(
        UserId: userId,
        UserEmail: userEmail,
        InviteId: InviteId,
        Status: OrganizationInviteStatus.Accepted
    );
}

public record AcceptOrganizationInviteResponse
(

);

public class AcceptOrganizationInviteRequestValidator : AbstractValidator<AcceptOrganizationInviteRequest>
{
    public AcceptOrganizationInviteRequestValidator()
    {
        RuleFor(x => x.InviteId).NotEmpty();
    }
}