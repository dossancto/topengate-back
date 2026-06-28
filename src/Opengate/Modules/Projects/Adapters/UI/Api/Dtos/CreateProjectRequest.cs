using FluentValidation;

namespace Opengate.Modules.Projects.Adapters.UI.Api.Dtos;

public record CreateProjectRequest
(
     string Name,
     string Description
);

public record CreateProjectResponse
(
    Guid Id,
    string ApiKey
);

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}