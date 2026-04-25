namespace EventManager.Models;

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public static class PaginationExtensions
{
    public static IEnumerable<T> ApplyPagination<T>(
        this IEnumerable<T> query,
        int page,
        int pageSize)
    {
        var skip = (page - 1) * pageSize;

        return query
            .Skip(skip)
            .Take(pageSize);
    }
}
