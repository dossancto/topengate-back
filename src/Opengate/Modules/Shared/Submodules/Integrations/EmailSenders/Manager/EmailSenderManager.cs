
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.EmailsTemplates.OrganizationInvite;
using Opengate.Modules.Shared.Utils.Providers.ResilientProviders;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Manager;

public class EmailSenderManager(
        IServiceProvider sp
    ) : IEmailSender, IEmailSender<ApplicationUser>, IOrganizationInviteEmailSender
{
    private readonly IServiceProvider sp = sp;

    private readonly List<string> providers = EmailSendersModule
        .AvailibleEmailSenders
        .Select(x => x.ToString())
        .ToList();

    public Aff<Unit> Send(InviteUserToOrganizationInviteInput input)
    => Aff(async () =>
    {
        // TODO: Save this template on database
        var htmlTemplate = $"""
            <h1>Olá {input.TargetEmail}!</h1>
            <p>Você foi convidado para participar da Organização {input.OrganizationName}.</p>
            <p>Para aceitar, clique no link abaixo:</p>
            <p><a href="{input.InviteCode}">Aceitar convite</a></p>
            <p>Se você não puder aceitar, clique no link abaixo:</p>
            <p><a href="{input.ExpiresAt}">Recusar convite</a></p>
        """;

        var res = await sp.CallResilientServiceFin<IEmailSender>
           (p => p.SendEmailAsync(
                email: input.TargetEmail,
                subject: "Convite de convite",
                htmlMessage: htmlTemplate)
            , providers);

        res.ThrowIfFail();

        return unit;
    });

    /// <inheritdoc />
    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var htmlTemplate = $"""
            <h1>Olá {email}!</h1>
            <p>Por favor, confirme seu e-mail clicando no link abaixo:</p>
            <p><a href="{confirmationLink}">Confirmar e-mail</a></p>
        """;

        Task Action(IEmailSender sender) =>
             sender.SendEmailAsync(email, "Confirmação de e-mail", htmlTemplate);


        await sp.CallResilientService<IEmailSender>
            (Action, providers);
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await sp.CallResilientService<IEmailSender>
                    (sender => sender.SendEmailAsync(email, subject, htmlMessage),
                    providers);
    }

    /// <inheritdoc />
    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var htmlTemplate = $"""
            <h1>Olá {user.UserName}!</h1>
            <p>Você solicitou a redefinição de sua senha. Use o código abaixo para redefinir sua senha:</p>
            <h2>{resetCode}</h2>
            <p>Se você não solicitou essa alteração, ignore este e-mail.</p>
        """;

        await sp.CallResilientService<IEmailSender>
                    (sender => sender.SendEmailAsync(email, "Redefinição de senha", htmlTemplate),
                     providers
                );
    }

    /// <inheritdoc />
    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var htmlTemplate = $"""
            <h1>Olá {email}!</h1>
            <p>Você solicitou a redefinição de sua senha. Clique no link abaixo para redefinir sua senha:</p>
            <p><a href="{resetLink}">Redefinir senha</a></p>
            <p>Se você não solicitou essa alteração, ignore este e-mail.</p>
        """;
        await sp.CallResilientService<IEmailSender>
                    (sender => sender.SendEmailAsync(email, "Redefinição de senha", htmlTemplate),
                    providers
                );
    }
}