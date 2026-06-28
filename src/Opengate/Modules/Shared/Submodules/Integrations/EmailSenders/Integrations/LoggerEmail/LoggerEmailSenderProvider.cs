using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

using Opengate.Modules.Accounts.Users.Models;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.LoggerEmail;

public class LoggerEmailSenderProvider(
        ILogger<LoggerEmailSenderProvider> logger
        ) : IEmailSender<ApplicationUser>, IEmailSender
{
    private readonly ILogger<LoggerEmailSenderProvider> logger = logger;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        logger.LogInformation(
            "[SendConfirmationLinkAsync] Sending email to {email} with confirmation link {confirmationLink}",
            email,
            confirmationLink);

        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation(
            "[SendEmailAsync] Sending email to {email} with subject {subject} with content {htmlMessage}",
            email,
            subject,
            htmlMessage);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        logger.LogInformation(
            "[SendPasswordResetCodeAsync] Sending email to {email} with reset code {resetCode}",
            email,
            resetCode);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        logger.LogInformation(
            "[SendPasswordResetLinkAsync] Sending email to {email} with reset link {resetLink}",
            email,
            resetLink);

        return Task.CompletedTask;
    }
}