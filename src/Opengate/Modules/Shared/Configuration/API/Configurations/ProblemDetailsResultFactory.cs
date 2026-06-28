using System.Net;

using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;

namespace Opengate.Modules.Shared.Configuration.API.Configurations;

public class ProblemDetailsResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext context, FluentValidation.Results.ValidationResult validationResult)
    {
        var validationProblemErrors = validationResult.Errors
          .GroupBy(x => x.PropertyName)
          .Select(x => new
          {
              name = x.Key,
              failures = x.DistinctBy(x => x.ErrorCode).Select(x => new
              {
                  reason = x.ErrorMessage,
                  code = x.ErrorCode,
                  // severity = x.Severity,
              })
          });

        List<KeyValuePair<string, object?>>? extensions = [
          new("errors", validationProblemErrors)
        ];

        return Results.Problem(
            extensions: extensions,
            detail: "Invalid model",
            statusCode: (int)HttpStatusCode.BadRequest,
            title: "Validation fail"
        );
    }

}