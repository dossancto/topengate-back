namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.EmailsTemplates.OrganizationInvite;

public interface IOrganizationInviteEmailSender
{
    Aff<Unit> Send(InviteUserToOrganizationInviteInput input);
}