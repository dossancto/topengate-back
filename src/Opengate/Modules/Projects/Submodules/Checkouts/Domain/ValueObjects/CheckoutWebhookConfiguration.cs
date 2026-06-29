namespace Opengate.Modules.Projects.Submodules.Checkouts.Domain.ValueObjects;

public record CheckoutWebhookConfiguration
{
    public string Url { get; set; }
    public List<string> Events { get; set; }

    public CheckoutWebhookConfiguration(string url, List<string> events)
    {
        Url = url;
        Events = events;
    }

    public CheckoutWebhookConfiguration() : this("", [])
    {
    }
}