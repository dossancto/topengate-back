using Opengate.Modules.Projects.Submodules.Checkouts.Domain.ValueObjects;

namespace Opengate.Modules.Projects.Submodules.Checkouts.Domain.Models;

public record CheckoutConfiguration
(
        Guid Id,
        string Name,
        string Description,
        Guid ProjectId,
        Guid OrganizationId,
        Guid CreatorUserId,
        ReceiverBankAccountDetails ReceiverBankAccountDetails,
        CheckoutPaymentDetails PaymentDetails,
        CheckoutWebhookConfiguration Webhook,
        Guid CreatedBy,
        DateTime CreatedAt,
        DateTime UpdatedAt
);