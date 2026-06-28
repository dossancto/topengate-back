using Microsoft.AspNetCore.Mvc;

using Opengate.Modules.Shared.Domain.Exceptions;
using Opengate.Modules.Shared.Utils.Enums;

namespace Opengate.Modules.Shared.Adapters.UI.Middlewares.Handlers;

public static class HttpUserResolutionExceptionHandler
{
    public static ProblemDetails HandleError(this HttpUserResolutionException ex)
              => new()
              {
                  Title = "User Resolution Error",
                  Detail = "Erro in resolving user from token.",
                  Status = StatusCodes.Status401Unauthorized,
                  Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                  Extensions = {
                    // { "Detail",new{ex.Error} },
                    { "errors", new[] { new {
                        name = "User Resolution",
                        reason = ex.Error.GetDescription(),
                        code = ex.Error.ToString(),
                        severity  = "Error",
                } } } }
              };
}