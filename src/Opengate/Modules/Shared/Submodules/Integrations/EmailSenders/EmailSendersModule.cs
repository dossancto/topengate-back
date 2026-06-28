using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.EmailsTemplates.OrganizationInvite;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.LoggerEmail;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Resend.Clients;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Manager;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Types;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders;

public static class EmailSendersModule
{
    /// <summary>
    /// Available email sender types that have been registered.
    /// </summary>
    public static List<EmailSenderIntegration> AvailibleEmailSenders { get; private set; } = [];

    public static void AddAvailableSender(EmailSenderIntegration integration)
        => AvailibleEmailSenders.Add(integration);

    /// <summary>
    /// This method is responsible for adding the email senders module
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddEmailSenderIntegrationModule(this IServiceCollection services)
    {
        services.AddEmailSenderManager();

        services.AddEmailSenderIntegrations();

        return services;
    }

    /// <summary>
    /// Registers the <see cref="EmailSenderManager"/> as the central email sender.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddEmailSenderManager(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSenderManager>();
        services.AddScoped<IEmailSender<ApplicationUser>, EmailSenderManager>();
        services.AddScoped<IOrganizationInviteEmailSender, EmailSenderManager>();
    }

    /// <summary>
    /// Registers the available email sender provider integrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddEmailSenderIntegrations(this IServiceCollection services)
    {
        ResendEmailSenderProviderSetup.Register(services);
        BrevoEmailSenderProviderSetup.Register(services);
        LoggerEmailSenderProviderSetup.Register(services);
    }
}