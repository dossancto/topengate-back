using Opengate.Modules.Accounts.Organizations.Domain.Enums;

namespace Opengate.Modules.Accounts.Organizations.Application.ChangeInviteStatus;

public record ChangeInviteStatusInput
(
    string UserId,
    string UserEmail,
    Guid InviteId,
    OrganizationInviteStatus Status
)
{
}

public record ChangeInviteStatusOutput();