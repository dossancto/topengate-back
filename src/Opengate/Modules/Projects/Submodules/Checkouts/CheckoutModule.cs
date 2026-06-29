namespace Opengate.Modules.Projects.Submodules.Checkouts.Checkouts;

public static class CheckoutModule
{
    public static IServiceCollection AddCheckoutModule(this IServiceCollection services)
    {
        return services;
    }

    public static WebApplication UseCheckoutModule(this WebApplication app)
    {
        return app;
    }
}