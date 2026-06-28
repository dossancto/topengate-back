namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Resend.Clients.Dtos;

public record ResendSendRawHtmlRequest
(
   string From,
   string To,
   string Subject,
   string Html
);