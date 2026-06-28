
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Opengate.Modules.Accounts.Organizations.Domain.Exceptions;
using Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;
using Opengate.Modules.Shared.Adapters.UI.Middlewares.Handlers;
using Opengate.Modules.Shared.Domain.Exceptions;

namespace Opengate.Modules.Shared.Adapters.UI.Middlewares;

public class GlobalRestErrorHandler;

public class ErrorHandlerMiddleware(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalRestErrorHandler> logger
        ) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly ILogger<GlobalRestErrorHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var content = _logger.HandleThenLogException(exception);

        if (content is null)
        {
            return true;
        }

        httpContext.Response.StatusCode = content.Status ?? StatusCodes.Status500InternalServerError;

        await _problemDetailsService.WriteAsync(new ProblemDetailsContext()
        {
            HttpContext = httpContext,
            ProblemDetails = content
        });

        return true;
    }
}

public static class ExceptionHandlerExtension
{
    private static ProblemDetails InternalServerErrorProblem
    => new()
    {
        Title = "Internal Server Error",
        Detail = null,
        Status = StatusCodes.Status500InternalServerError,
        Type = null
    };

    public static ProblemDetails? ResolveError(this Exception ex)
      => ex switch
      {
          FluentValidation.ValidationException e => e.HandleError(),
          ChangeInviteStatusException e => e.HandleError(),
          InitOrganizationException e => e.HandleError(),
          InviteUserToOrganizationException e => e.HandleError(),
          InvalidOperationException e => e.Message.Contains("authenticationScheme") ? new()
          {
              Title = "Unauthorized",
              Status = StatusCodes.Status401Unauthorized
          }
          : InternalServerErrorProblem,
          HttpUserResolutionException e => e.HandleError(),
          Exception => InternalServerErrorProblem,
          _ => null
      };

    public static void LogProblemDetail(this ILogger logger, ProblemDetails problem)
    {
        logger.LogError("[ProblemDetail] {@problemDetail}", problem);
    }

    public static void LogExceptionAsWarn(this ILogger logger, Exception e, string? detail = null)
    {
        var exceptionType = e.GetType().Name;
        var reason = detail ?? e.Message;

        logger.LogWarning(e, "[{@exceptionType}] due to [{@reason}]. [{@stackTrace}]", exceptionType, reason, e.StackTrace);
    }

    public static void LogException(this ILogger logger, Exception e, string? detail = null)
    {
        var exceptionType = e.GetType().Name;
        var reason = detail ?? e.Message;

        if (e is HttpRequestFailtureException httpEx)
        {
            logger.LogError("Request to [{@url}] with payload [{@payload}] failed with body [{@body}] and status code [{@statusCode}]", httpEx.Path, httpEx.RequestBody, httpEx.ResponseBody, httpEx.StatusCode);
        }

        if (e is AggregateException aggEx)
        {
            foreach (var inner in aggEx.InnerExceptions)
            {
                logger.LogException(inner);
            }
        }

        // if you use refit
        // (e is Refit.ApiException rEx)
        // {
        //     string? requestBody = null;
        //
        //     if (rEx.RequestMessage.Content is not null)
        //     {
        //         var r  = rEx.RequestMessage.Content.ReadAsStringAsync();
        //         r.Wait();
        //         requestBody = r.Result;
        //     }
        //
        //     var responseBody = rEx.Content;
        //
        //     logger.LogError("Request to [{@url}] with payload [{@payload}] failed with body [{@body}] and status code [{@statusCode}]",
        //             rEx.RequestMessage.RequestUri, requestBody, responseBody, rEx.StatusCode);
        // }

        logger.LogError(e, "[{@exceptionType}] due to [{@reason}]. [{@stackTrace}]", exceptionType, reason, e.StackTrace);
    }

    public static void LogFailture(this ILogger logger, ProblemDetails problem, Exception ex)
    {
        logger.LogProblemDetail(problem);
        logger.LogException(ex, problem.Detail);
    }

    public static ProblemDetails? HandleThenLogException(this ILogger logger, Exception e)
    {
        var content = e.ResolveError();

        if (content is null)
        {
            return null;
        }

        logger.LogFailture(content, e);

        return content;
    }
}