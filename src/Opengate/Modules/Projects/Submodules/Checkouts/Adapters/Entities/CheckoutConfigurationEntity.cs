using Opengate.Modules.Projects.Submodules.Checkouts.Domain.ValueObjects;

namespace Opengate.Modules.Projects.Submodules.Checkouts.Adapters.Entities;

public class CheckoutConfigurationEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid CreatorUserId { get; set; }

    // TODO: Encrypt the receiver bank account details
    public ReceiverBankAccountDetails ReceiverBankAccountDetails { get; set; } = new();

    public CheckoutPaymentDetails PaymentDetails { get; set; } = new();

    public CheckoutWebhookConfiguration Webhook { get; set; } = new();

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long DeletedAt { get; set; }
}