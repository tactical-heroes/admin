namespace TacticalHeroes.Admin.Shared.Model;

public static class PaginationOptions
{
    public const int DefaultPageSize = 10;

    public static readonly int[] PageSizes = [10, 25, 50, 100];

    public static int NormalizePageSize(int? pageSize)
    {
        return pageSize.HasValue && PageSizes.Contains(pageSize.Value)
            ? pageSize.Value
            : DefaultPageSize;
    }
}
