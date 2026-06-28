namespace Opengate.Modules.Shared.Validators;

using FluentValidation;

using Opengate.Modules.Shared.Domain.ValueObjects.Prices;

public class PriceValueValidator : AbstractValidator<PriceValue>
{
    public PriceValueValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty();
    }
}