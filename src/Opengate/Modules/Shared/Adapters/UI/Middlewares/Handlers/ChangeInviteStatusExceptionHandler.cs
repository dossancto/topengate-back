using Microsoft.AspNetCore.Mvc;

using Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;

namespace Opengate.Modules.Shared.Adapters.UI.Middlewares.Handlers;

public static class ChangeInviteStatusExceptionHandler
{
    public static ProblemDetails HandleError(this ChangeInviteStatusException ex)
          => ex.Error switch
          {
              _ => new()
              {
                  Title = "Change Invite Status Error",
                  Detail = ex.Message,
                  Status = StatusCodes.Status400BadRequest,
                  Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                  Extensions = {
                        {"Detail", new{
                            ex.InviteId
                        }},
                 }
              },
          };
}