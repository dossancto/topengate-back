using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace Opengate.Modules.Shared.Adapters.UI.Middlewares.Handlers;

public static class FluentValidationExceptionHandler
{
    public static ProblemDetails HandleError(this ValidationException ex)
          => new()
          {
              Title = "Validation Error",
              Detail = "Validation Error",
              Status = StatusCodes.Status400BadRequest,
              Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
              Extensions = { { "errors", ex.Errors.Select(x => new {
                      name = x.PropertyName,
                      reason = x.ErrorMessage,
                      code = x.ErrorCode,
                      severity  = x.Severity,
              }) } }
          };
}