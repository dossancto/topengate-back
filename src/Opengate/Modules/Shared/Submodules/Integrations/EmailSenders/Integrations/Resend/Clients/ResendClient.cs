using Opengate.Modules.Shared.Domain.Exceptions;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Resend.Clients.Dtos;

namespace Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.Integrations.Resend.Clients;

public class ResendClient
(
    HttpClient http
 )
{
    private readonly HttpClient _http = http;

    public Aff<Unit> SendEmailAsync(ResendSendRawHtmlRequest request)
    => Aff(async () =>
            {
                var res = await _http.PostAsJsonAsync("/emails", request);
                await res.EnsureSuccess();
                return unit;
            });
}