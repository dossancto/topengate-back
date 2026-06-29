using Opengate.Modules.Projects.Submodules.Checkouts.Domain.Enums;

namespace Opengate.Modules.Projects.Submodules.Checkouts.Domain.ValueObjects;

public record CheckoutPaymentDetails
{
    public string Currency { get; private init; }

    public PaymentServiceType ServiceType { get; private init; }

    public List<string> PaymentTypes { get; private init; }

    public CheckoutPaymentDetails(
            string currency,
            PaymentServiceType serviceType,
            List<string> paymentTypes
            )
    {
        Currency = currency.ToUpperInvariant();
        ServiceType = serviceType;
        PaymentTypes = paymentTypes;
    }

    public CheckoutPaymentDetails() : this(
            currency: "",
            serviceType: PaymentServiceType.Unit,
            paymentTypes: []
            )
    {
    }
}