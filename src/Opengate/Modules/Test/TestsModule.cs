
using Opengate.Modules.Test.Endpoints;

namespace Opengate.Modules.Test;

public static class TestModule
{
    public static IServiceCollection AddTestModule(this IServiceCollection services)
    {
        // services;

        return services;
    }

    public static WebApplication UseTestModule(this WebApplication app)
    {
        app.MapTestEndpoints();

        return app;
    }
}