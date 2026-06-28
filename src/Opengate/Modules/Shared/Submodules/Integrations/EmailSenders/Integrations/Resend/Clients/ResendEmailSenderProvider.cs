
using Microsoft.AspNetCore.Identity.UI.Services;

using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Resend.Clients.Dtos;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Resend.Clients;

public class ResendEmailSenderProvider(
        ResendClient resend
        ) : IEmailSender
{
    private readonly ResendClient _resend = resend;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var request = new ResendSendRawHtmlRequest(
                To: email,
                From: AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.RESEND.SENDER.EMAIL.NotNull(),
                Subject: subject,
                Html: htmlMessage
        );

        var res = await _resend.SendEmailAsync(request).Run();

        res.ThrowIfFail();
    }
}