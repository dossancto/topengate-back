using Opengate.Modules.Projects.Adapters.UI.Api;

namespace Opengate.Modules.Projects;

public static class ProjectsModule
{
    public static IServiceCollection AddProjectsModule(this IServiceCollection services)
    {
        return services;
    }

    public static WebApplication UseProjectsModule(this WebApplication app)
    {
        app.MapProjectEndpoints();
        return app;
    }
}