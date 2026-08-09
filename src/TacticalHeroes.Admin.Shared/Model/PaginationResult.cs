namespace TacticalHeroes.Admin.Shared.Model;

public sealed record PaginationResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    long TotalCount,
    int TotalPages)
{
    public static PaginationResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PaginationResult<T>(
            [],
            pageNumber,
            pageSize,
            TotalCount: 0,
            TotalPages: 0);
    }
}
