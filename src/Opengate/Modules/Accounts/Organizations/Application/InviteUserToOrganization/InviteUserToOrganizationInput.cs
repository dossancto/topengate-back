using Opengate.Modules.Shared.Domain.ValueObjects.Emails;

namespace Opengate.Modules.Accounts.Organizations.Application.InviteUserToOrganization;

public record InviteUserToOrganizationInput
(
     Guid OrganizationId,
     Guid SenderUserId,
     Email TargetEmail,
     string TargetUserFirstName,
     string TargetUserLastName
)
{
}