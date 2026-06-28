namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Types;

/// <summary>
/// Represents the different ways to send emails
/// </summary>
public enum EmailSenderIntegration
{
    /// <summary>
    /// Send email using the default Logger
    /// </summary>
    Logger,

    /// <summary>
    /// Send email using Brevo
    /// </summary>
    Brevo,

    /// <summary>
    /// Send email using Resend
    /// </summary>
    Resend
}