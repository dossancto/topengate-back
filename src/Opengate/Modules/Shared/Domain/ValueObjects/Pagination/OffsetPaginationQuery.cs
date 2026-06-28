namespace Opengate.Modules.Shared.Domain.ValueObjects.Pagination;

/// <summary>
/// Represents a query object for offset-based pagination, encapsulating page size and page number logic.
/// </summary>
public class OffsetPaginationQuery
{
    /// <summary>
    /// The initial page number.
    /// </summary>
    public const int InitialPage = 1;

    /// <summary>
    /// The initial page size.
    /// </summary>
    public const int InitialPageSize = 25;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OffsetPaginationQuery"/> class with default page size and page number.
    /// </summary>
    public OffsetPaginationQuery() : this(InitialPageSize, InitialPage) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OffsetPaginationQuery"/> class with a specified page size and default page number.
    /// </summary>
    /// <param name="pageSize">The number of items per page.</param>
    public OffsetPaginationQuery(int pageSize) : this(pageSize, InitialPage) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OffsetPaginationQuery"/> class with a specified page size and page number.
    /// </summary>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="page">The current page number.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="pageSize"/> is less than or equal to zero,
    /// or when <paramref name="page"/> is less than <see cref="InitialPage"/>.
    /// </exception>
    public OffsetPaginationQuery(int pageSize, int page)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be greater than zero.");
        }

        if (page < InitialPage)
        {
            throw new ArgumentOutOfRangeException(nameof(page), $"Page must be greater than or equal to {InitialPage}.");
        }

        PageSize = pageSize;
        Page = page;
    }

    /// <summary>
    /// Calculates the number of items to skip based on the current page and page size.
    /// </summary>
    /// <returns>The number of items to skip.</returns>
    public int GetSkip() => (Page - InitialPage) * PageSize;

    /// <summary>
    /// Gets the maximum number of items to take for the current page.
    /// </summary>
    /// <returns>The page size (limit).</returns>
    public int GetLimit() => PageSize;

    /// <summary>
    /// Creates a new <see cref="OffsetPaginationQuery"/> instance with the specified page size and the default page number.
    /// </summary>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A new <see cref="OffsetPaginationQuery"/> instance.</returns>
    public static OffsetPaginationQuery InitWithPageSize(int pageSize)
        => new(pageSize);

    /// <summary>
    /// Creates a new <see cref="OffsetPaginationQuery"/> instance with default values.
    /// </summary>
    /// <returns>A new <see cref="OffsetPaginationQuery"/> instance with default page size and page number.</returns>
    public static OffsetPaginationQuery Default()
        => new();
}