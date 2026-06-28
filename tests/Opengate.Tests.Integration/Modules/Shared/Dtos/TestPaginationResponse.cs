using System.Text.Json.Serialization;

namespace Opengate.Tests.Integration.Modules.Shared.Dtos;

public record TestPaginationResponse<T>(
    [property: JsonPropertyName("items")] IReadOnlyList<T> Items,
    [property: JsonPropertyName("details")] TestPaginationResponseDetails Details
);


public record TestPaginationResponseDetails(
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("currentPage")] int CurrentPage,
    [property: JsonPropertyName("hasMore")] bool HasMore,
    [property: JsonPropertyName("hasPrevious")] bool HasPrevious
);