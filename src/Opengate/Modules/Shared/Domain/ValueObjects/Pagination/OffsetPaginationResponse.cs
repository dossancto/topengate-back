namespace Opengate.Modules.Shared.Domain.ValueObjects.Pagination;

/// <summary>
/// Represents a paginated response containing a list of items and pagination details.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public record OffsetPaginationResponse<T>
{
    /// <summary>
    /// Gets or sets the list of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the pagination details for the response.
    /// </summary>
    public required OffsetPaginationResponseDetails Details { get; set; }

    /// <summary>
    /// Gets an empty paginated response.
    /// </summary>
    public static OffsetPaginationResponse<T> Empty => new()
    {
        Items = [],
        Details = OffsetPaginationResponseDetails.Empty
    };

    /// <summary>
    /// Creates a paginated response from a pagination query, a list of items, and the total number of items.
    /// </summary>
    /// <param name="query">The pagination query used to generate the response.</param>
    /// <param name="items">The list of items for the current page.</param>
    /// <param name="totalItems">The total number of items available.</param>
    /// <returns>A new <see cref="OffsetPaginationResponse{T}"/> instance.</returns>
    public static OffsetPaginationResponse<T> FromQuery(OffsetPaginationQuery query, List<T> items, int totalItems)
    {
        return new OffsetPaginationResponse<T>
        {
            Items = items,
            Details = OffsetPaginationResponseDetails.FromQuery(query, totalItems)
        };
    }

    /// <summary>
    /// Projects each item in the response to a new form using the specified selector function.
    /// </summary>
    /// <typeparam name="TOut">The type of the projected items.</typeparam>
    /// <param name="selector">A transform function to apply to each item.</param>
    /// <returns>A new <see cref="OffsetPaginationResponse{TOut}"/> with projected items.</returns>
    public OffsetPaginationResponse<TOut> Select<TOut>(Func<T, TOut> selector)
    {
        var projectedItems = Items.Select(selector).ToList();

        return new OffsetPaginationResponse<TOut>
        {
            Items = projectedItems,
            Details = Details
        };
    }
}

/// <summary>
/// Contains details about the pagination state of a response.
/// </summary>
public class OffsetPaginationResponseDetails
{
    /// <summary>
    /// Gets or sets the total number of items available.
    /// </summary>
    /// <example>150</example>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    /// <example>25</example>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    /// <example>2</example>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets a value indicating whether there are more items after the current page.
    /// </summary>
    /// <example>true</example>
    public bool HasMore => TotalItems > CurrentPage * PageSize;

    /// <summary>
    /// Gets a value indicating whether there are previous pages before the current page.
    /// </summary>
    /// <example>true</example>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>
    /// Gets the total number of pages available.
    /// </summary>
    /// <example>6</example>
    public int TotalIPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>
    /// Gets an empty pagination details instance with default values.
    /// </summary>
    public static OffsetPaginationResponseDetails Empty => new()
    {
        TotalItems = 0,
        PageSize = OffsetPaginationQuery.InitialPageSize,
        CurrentPage = OffsetPaginationQuery.InitialPage
    };

    /// <summary>
    /// Creates pagination details from a pagination query and the total number of items.
    /// </summary>
    /// <param name="query">The pagination query.</param>
    /// <param name="totalItems">The total number of items available.</param>
    /// <returns>A new <see cref="OffsetPaginationResponseDetails"/> instance.</returns>
    public static OffsetPaginationResponseDetails FromQuery(OffsetPaginationQuery query, int totalItems)
    {
        return new OffsetPaginationResponseDetails
        {
            TotalItems = totalItems,
            PageSize = query.PageSize,
            CurrentPage = query.Page
        };
    }
}