using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Types;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.LoggerEmail;

public static class LoggerEmailSenderProviderSetup
{
    /// <summary>
    /// Registers the <see cref="LoggerEmailSenderProvider"/> as the email sender provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        if (AppEnv.APP.INTEGRATIONS.PROVIDERS.EMAIL_SENDERS.LOGGER.ENABLED.GetDefault(false) is false)
            return;

        services.AddKeyedTransient<IEmailSender, LoggerEmailSenderProvider>(EmailSenderIntegration.Logger.ToString());
        services.AddKeyedTransient<IEmailSender<ApplicationUser>, LoggerEmailSenderProvider>(EmailSenderIntegration.Logger.ToString());

        EmailSendersModule.AddAvailableSender(EmailSenderIntegration.Logger);
    }
}