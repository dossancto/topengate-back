namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.EmailsTemplates.OrganizationInvite;

public record InviteUserToOrganizationInviteInput
(
    string TargetEmail,
    string OrganizationName,
    string InviteCode,
    DateTime ExpiresAt
);