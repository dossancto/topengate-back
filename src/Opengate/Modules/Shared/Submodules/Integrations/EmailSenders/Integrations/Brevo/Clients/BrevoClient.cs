using Opengate.Modules.Shared.Domain.Exceptions;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo.Clients.Dtos;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Brevo.Clients;

public class BrevoClient(
        HttpClient http,
        ILogger<BrevoClient> logger
        )
{
    private readonly HttpClient http = http;
    private readonly ILogger<BrevoClient> logger = logger;

    public Aff<Unit> SendEmailAsync(BrevoSendRawEmailRequest request)
    => Aff(async () =>
            {
                var res = await http.PostAsJsonAsync("/v3/smtp/email", request);

                await res.EnsureSuccess();

                return unit;
            });
}