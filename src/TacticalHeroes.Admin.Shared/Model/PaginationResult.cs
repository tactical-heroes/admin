namespace TacticalHeroes.Admin.Shared.Model;

public sealed class PaginationResult<T>(
    IReadOnlyList<T> items,
    int pageNumber,
    int pageSize,
    long totalCount,
    int totalPages)
{
    public IReadOnlyList<T> Items { get; } = items;

    public int PageNumber { get; } = pageNumber;

    public int PageSize { get; } = pageSize;

    public long TotalCount { get; } = totalCount;

    public int TotalPages { get; } = totalPages;

    public static PaginationResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PaginationResult<T>(
            [],
            pageNumber,
            pageSize,
            totalCount: 0,
            totalPages: 0);
    }
}
