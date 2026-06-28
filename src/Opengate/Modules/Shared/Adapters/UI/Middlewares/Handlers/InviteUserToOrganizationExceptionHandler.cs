using Microsoft.AspNetCore.Mvc;

using Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;

namespace Opengate.Modules.Shared.Adapters.UI.Middlewares.Handlers;


public static class InviteUserToOrganizationExceptionHandler
{
    public static ProblemDetails HandleError(this InviteUserToOrganizationException ex)
          => new()
          {
              Title = "Invitation Error",
              Detail = "Error inviting user to organization.",
              Status = StatusCodes.Status400BadRequest,
              Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
              Extensions = {
                    {"Detail", new{
                        ex.OrganizationId,
                        ex.TargetEmail,
                    }},
                    { "errors", new[] { new {
                        name = "Invitation",
                        reason = "Error inviting user to organization.",
                        code = ex.Error.ToString(),
                        severity  = "Error",
                } } } }
          };
}