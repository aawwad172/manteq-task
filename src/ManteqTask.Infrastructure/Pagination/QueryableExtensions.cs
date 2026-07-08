using ManteqTask.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace ManteqTask.Infrastructure.Pagination;

public static class QueryableExtensions
{
    // This method is an extension method that extends the IQueryable interface.
    // The default behavior of this method is to return everything from the query.
    public static async Task<PaginationResult<T>> ToPagedQueryAsync<T>(this IQueryable<T> query, int? pageNumber, int? pageSize)
    {
        int totalRecords = await query.CountAsync();

        int number = pageNumber is > 0 ? pageNumber.Value : 1;
        int size = pageSize is > 0 ? pageSize.Value : totalRecords;

        // Page at the database level (OFFSET/LIMIT). When no page size is supplied, return everything.
        if (pageSize is not null)
            query = query.Skip((number - 1) * size).Take(size);

        List<T> result = await query.ToListAsync();

        return new PaginationResult<T>
        {
            Page = result,
            TotalRecords = totalRecords,
            TotalDisplayRecords = result.Count,
            PageNumber = number,
            PageSize = pageSize is not null ? size : totalRecords
        };
    }
}
