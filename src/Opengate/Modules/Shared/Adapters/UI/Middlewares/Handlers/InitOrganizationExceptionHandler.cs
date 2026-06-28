using Microsoft.AspNetCore.Mvc;

using Opengate.Modules.Accounts.Organizations.Domain.Exceptions;

namespace Opengate.Modules.Shared.Adapters.UI.Middlewares.Handlers;

public static class InitOrganizationExceptionHandler
{
    public static ProblemDetails HandleError(this InitOrganizationException ex)
    {
        return ex.Type switch
        {
            InitOrganizationExceptionType.OrganizationAlreadyExists => new()
            {
                Title = "Organization Already Exists",
                Detail = "Organization with this Slug Already Exists.",
                Status = StatusCodes.Status409Conflict,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Extensions = {
                    {"Detail", new{
                        ex.Organization,
                    }},
                    { "errors", new[] { new {
                        name = "Organization",
                        reason = "Organization Already Exists",
                        code = "OrganizationAlreadyExists",
                        severity  = "Error",
                } } } }
            },
            _ => new()
            {
                Title = "Internal Server Error",
                Detail = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Extensions = {
                    {"Detail", new {
                            ex.Organization,
                        }
                    },
                    { "errors", new[] { new {
                        name = "Organization",
                        reason = "Internal Server Error",
                        code = "InternalServerError",
                        severity  = "Error",
                } } }
                }
            }
        };
    }
}