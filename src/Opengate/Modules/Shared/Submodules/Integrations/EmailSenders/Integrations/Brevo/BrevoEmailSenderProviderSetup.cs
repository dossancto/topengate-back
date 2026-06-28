using Microsoft.AspNetCore.Identity.UI.Services;

using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo.Clients;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Types;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo;

public static class BrevoEmailSenderProviderSetup
{
    public static void Register(IServiceCollection services)
    {
        if (AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.BREVO.BASEURL.IsNotDefined())
        {
            return;
        }

        var key = EmailSenderIntegration.Brevo;

        services.AddHttpClient<BrevoClient>(c =>
        {
            c.BaseAddress = new Uri(AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.BREVO.BASEURL.NotNull());
            c.DefaultRequestHeaders.Add("api-key", AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.BREVO.APIKEY.NotNull());
        });

        services.AddKeyedTransient<IEmailSender, BrevoEmailSenderProvider>(key.ToString());

        EmailSendersModule.AddAvailableSender(key);
    }
}