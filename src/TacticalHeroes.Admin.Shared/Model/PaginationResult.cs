namespace TacticalHeroes.Admin.Shared.Model;

public sealed class PaginationResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }

    public int TotalPages { get; set; }

    public static PaginationResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PaginationResult<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
    }
}
