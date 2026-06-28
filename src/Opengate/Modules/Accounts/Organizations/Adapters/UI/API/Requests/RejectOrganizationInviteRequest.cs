using FluentValidation;

using Opengate.Modules.Accounts.Organizations.Application.ChangeInviteStatus;
using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Modules.Accounts.Organizations.Adapters.UI.API.Requests;

public record RejectOrganizationInviteRequest(
        Guid InviteId
)
{
    public ChangeInviteStatusInput ToInput(string userId, string userEmail)
    => new(
        UserId: userId,
        UserEmail: userEmail,
        InviteId: InviteId,
        Status: OrganizationInviteStatus.Rejected
    );
}

public record RejectOrganizationInviteResponse
(

);

public class RejectOrganizationInviteRequestValidator : AbstractValidator<RejectOrganizationInviteRequest>
{
    public RejectOrganizationInviteRequestValidator()
    {
        RuleFor(x => x.InviteId).NotEmpty();
    }
}