using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Shared.Domain.ValueObjects.Pagination;

namespace Opengate.Modules.Shared.Adapters.Databases.Extensions;

/// <summary>
/// Provides extension methods for applying offset-based pagination to Entity Framework queries.
/// </summary>
public static class EntityFrameworkPaginationExtension
{
    /// <summary>
    /// Asynchronously paginates the given <see cref="IQueryable{T}"/> using the specified pagination parameters.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The source query to paginate.</param>
    /// <param name="pagination">The pagination parameters.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="OffsetPaginationResponse{T}"/>
    /// with the paginated items and pagination metadata.
    /// </returns>
    public static async Task<OffsetPaginationResponse<T>> PaginatedAsync<T>(this IQueryable<T> query, OffsetPaginationQuery pagination, CancellationToken ct = default)
    {
        var data = await query
                            .Skip(pagination.GetSkip())
                            .Take(pagination.GetLimit())
                            .ToListAsync(ct);

        var totalCount = await query.CountAsync(ct);

        return OffsetPaginationResponse<T>.FromQuery(
            query: pagination,
            items: data,
            totalItems: totalCount
        );
    }
}