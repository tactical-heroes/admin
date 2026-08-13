using System.Text.Json.Serialization;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Shared.Model;

public sealed class PagedListState<TItem>
{
    public PaginationResult<TItem>? Page { get; set; }

    public string? LoadError { get; set; }

    public int? LoadedPageNumber { get; set; }

    public int? LoadedPageSize { get; set; }

    public string? LoadedFilter { get; set; }

    [JsonIgnore]
    public bool Loading { get; private set; }

    [JsonIgnore]
    public Guid? DeletingId { get; private set; }

    public bool Matches(int pageNumber, int pageSize, string? filter = null)
    {
        return LoadedPageNumber == pageNumber
            && LoadedPageSize == pageSize
            && string.Equals(LoadedFilter, filter, StringComparison.OrdinalIgnoreCase);
    }

    public async Task LoadAsync(
        int pageNumber,
        int pageSize,
        Func<CancellationToken, Task<Result<PaginationResult<TItem>>>> loadAsync,
        CancellationToken cancellationToken,
        string? filter = null)
    {
        Loading = true;
        LoadError = null;
        LoadedPageNumber = pageNumber;
        LoadedPageSize = pageSize;
        LoadedFilter = filter;

        try
        {
            Result<PaginationResult<TItem>> result = await loadAsync(cancellationToken);

            if (result.IsFailure)
            {
                Page = null;
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
            }
            else
            {
                Page = result.Value;
            }
        }
        finally
        {
            Loading = false;
        }
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        Func<CancellationToken, Task<Result>> deleteAsync,
        CancellationToken cancellationToken)
    {
        DeletingId = id;

        try
        {
            return await deleteAsync(cancellationToken);
        }
        finally
        {
            DeletingId = null;
        }
    }
}
