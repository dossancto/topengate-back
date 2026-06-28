using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Projects.Adapters.UI.Api.Dtos;
using Opengate.Modules.Projects.Models;
using Opengate.Modules.Shared.Adapters.Databases;
using Opengate.Modules.Shared.Adapters.Databases.Extensions;
using Opengate.Modules.Shared.Domain.ValueObjects.Pagination;
using Opengate.Modules.Shared.Utils.HttpUtils;

using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Opengate.Modules.Projects.Adapters.UI.Api;

public static class ProjectEndpointExtension
{
    public static WebApplication MapProjectEndpoints(this WebApplication app)
    {
        var g = app
            .MapGroup("/projects")
            .WithTags("Projects")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        g.MapPost("/", async (
                    HttpContext context,
                    [FromServices] ApplicationDbContext db,
                    [FromBody] CreateProjectRequest request
                    ) =>
        {
            var user = context.GetRequiredUserInfo();

            // TODO: Encrypt the API key before storing it in the database
            var project = ProjectEntity.Create(
                name: request.Name,
                description: request.Description,
                creatorUserId: user.Id,
                creatorOrganizationId: user.OrganizationId
            );

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            return TypedResults.Created(
                $"/projects/{project.Id}",
                new CreateProjectResponse(
                    project.Id,
                    project.ApiKey
            ));
        });

        g.MapGet("/{projectId:Guid}", async (
                    HttpContext context,
                    [FromServices] ApplicationDbContext db,
                    [FromRoute] Guid projectId,
                    CancellationToken cancellationToken
                    ) =>
        {
            var user = context.GetRequiredUserInfo();

            var project = await db.Projects
            .Where(p => p.DeletedAt == 0)
            .Where(p => p.CreatorOrganizationId == user.OrganizationId)
            .Where(p => p.Id == projectId)
            .Select(p => new GetProjectByIdResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ApiKey = p.ApiKey,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(project);
        })
        ;

        g.MapGet("/", async (
                    HttpContext context,
                    [FromQuery] string? searchTerm,
                    [FromQuery] int page,
                    [FromQuery] int pageSize,
                    [FromServices] ApplicationDbContext db,
                    CancellationToken cancellationToken
                    ) =>
        {
            var user = context.GetRequiredUserInfo();

            var pagination = new OffsetPaginationQuery(
                pageSize: pageSize,
                page: page
            );

            var projects = await db.Projects
            .Where(p => p.DeletedAt == 0)
            .Where(p => p.CreatorOrganizationId == user.OrganizationId)
            .Select(p => new GetProjectByIdResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ApiKey = p.ApiKey,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .PaginatedAsync(pagination, cancellationToken);

            return Results.Ok(projects);
        });

        g.MapPatch("/{projectId:guid}/regen-api-key", async (
                    HttpContext context,
                    [FromServices] ApplicationDbContext db,
                    [FromRoute] Guid projectId,
                    CancellationToken cancellationToken
                    ) =>
        {
            var user = context.GetRequiredUserInfo();

            var project = await db.Projects
            .Where(p => p.DeletedAt == 0)
            .Where(p => p.CreatorOrganizationId == user.OrganizationId)
            .Where(p => p.Id == projectId)
            .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return Results.NotFound();
            }

            project.RegenApiKey();

            await db.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });

        return app;
    }
}