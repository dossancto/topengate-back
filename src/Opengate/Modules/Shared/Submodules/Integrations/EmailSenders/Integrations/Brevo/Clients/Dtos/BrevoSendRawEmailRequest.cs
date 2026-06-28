namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo.Clients.Dtos;

public record BrevoSendRawEmailRequest
(
   BrevoSendRawEmailRequestEmailPerson Sender,
   List<BrevoSendRawEmailRequestEmailPerson> To,
   string Subject,
   string HtmlContent
);

public record BrevoSendRawEmailRequestEmailPerson(
    string Name,
    string Email
  );