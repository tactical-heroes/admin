namespace TacticalHeroes.Admin.Shared.Model;

public sealed record PageResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    long TotalCount,
    int TotalPages)
{
    public static PageResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PageResult<T>(
            Array.Empty<T>(),
            pageNumber,
            pageSize,
            TotalCount: 0,
            TotalPages: 0);
    }
}
