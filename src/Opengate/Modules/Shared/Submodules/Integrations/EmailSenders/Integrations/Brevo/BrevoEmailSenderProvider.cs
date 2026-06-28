
using Microsoft.AspNetCore.Identity.UI.Services;

using Namotion.Reflection;

using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo.Clients;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo.Clients.Dtos;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo;

public class BrevoEmailSenderProvider(
        BrevoClient brevo
        ) : IEmailSender
{
    private readonly BrevoClient _brevo = brevo;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var request = new BrevoSendRawEmailRequest(
                Sender: new(
                    Name: AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.BREVO.SENDER.NAME.NotNull(),
                    Email: AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.BREVO.SENDER.EMAIL.NotNull()
                ),
                To: [
                    new(
                        Name: email,
                        Email: email
                    ),
                ],
                Subject: subject,
                HtmlContent: htmlMessage
        );

        var res = await _brevo.SendEmailAsync(request).Run();

        res.ThrowIfFail();
    }
}