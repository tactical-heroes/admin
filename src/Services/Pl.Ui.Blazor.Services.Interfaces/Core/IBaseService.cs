using Common.SearchParams.Core;

namespace Pl.Ui.Blazor.Services.Interfaces.Core;

public interface IBaseService<TId, TViewModel, TSearchParams, TConvertParams>
    where TId : notnull
    where TSearchParams : BaseSearchParams
    where TConvertParams : class
{
    Task<TViewModel> GetAsync(TId id, TConvertParams? convertParams = null, CancellationToken cancellationToken = default);
    Task<SearchResult<TViewModel>> GetAsync(TSearchParams searchParams, TConvertParams? convertParams = null, CancellationToken cancellationToken = default);
    Task<TId> CreateAsync(TViewModel viewModel, CancellationToken cancellationToken = default);
    Task UpdateAsync(TId id, TViewModel viewModel, CancellationToken cancellationToken = default);
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
}
