using Opengate.Modules.Shared.Domain.ValueObjects.ApiKeys;

namespace Opengate.Modules.Projects.Models;

public class ProjectEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CreatorUserId { get; set; }

    public Guid CreatorOrganizationId { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long DeletedAt { get; set; }

    public static (ApiKeyValue ApiKey, ProjectEntity Project) Create(
        string name,
        string? description,
        Guid creatorUserId,
        Guid creatorOrganizationId)
    {
        var apiKey = GenerateApiKey();

        var project = new ProjectEntity()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatorUserId = creatorUserId,
            CreatorOrganizationId = creatorOrganizationId,
            ApiKey = apiKey.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = 0
        };

        return (apiKey, project);
    }

    private static ApiKeyValue GenerateApiKey()
    {
        return ApiKeyValue.Generate("sk_opengate");
    }

    public ApiKeyValue RegenApiKey()
    {
        var apiKey = GenerateApiKey();

        ApiKey = apiKey.ToString();

        return apiKey;
    }
}