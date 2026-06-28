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

    public static ProjectEntity Create(
        string name,
        string? description,
        Guid creatorUserId,
        Guid creatorOrganizationId)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatorUserId = creatorUserId,
            CreatorOrganizationId = creatorOrganizationId,
            ApiKey = GenerateApiKey(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = 0
        };

    private static string GenerateApiKey()
    {
        // TODO: Generate the api key the right way, maybe using a secure random generator or a hashing algorithm
        return Guid.NewGuid().ToString();
    }

    public void RegenApiKey()
    {
        ApiKey = GenerateApiKey();
    }
}