namespace Opengate.Modules.Projects.Adapters.UI.Api.Dtos;

public class ListProjectsRequest
{
}

public class ListProjectsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}