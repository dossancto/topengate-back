using Microsoft.AspNetCore.Identity.UI.Services;

using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Types;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Resend.Clients;

public static class ResendEmailSenderProviderSetup
{
    public static void Register(IServiceCollection services)
    {
        if (AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.RESEND.BASEURL.IsNotDefined())
        {
            return;
        }

        var key = EmailSenderIntegration.Resend;

        services.AddHttpClient<ResendClient>(c =>
        {
            c.BaseAddress = new Uri(AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.RESEND.BASEURL.NotNull());
            c.DefaultRequestHeaders.Authorization = new("Bearer", AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.RESEND.APIKEY.NotNull());
        });

        services.AddKeyedTransient<IEmailSender, ResendEmailSenderProvider>(key.ToString());

        EmailSendersModule.AddAvailableSender(key);
    }
}