using Common.Clients.Interfaces;
using Common.Constants.ApiEndpoints.Core;
using Common.SearchParams.Core;

using Pl.Api.Http.Dtos.Core;
using Pl.Api.Http.Dtos.Models.Core;
using Pl.Ui.Blazor.Mappers.Core;
using Pl.Ui.Blazor.Services.Interfaces.Core;
using Pl.Ui.Blazor.ViewModels.Core;

namespace Pl.Ui.Blazor.Services.Implementations.Core;

public abstract class BaseService<TEndpoint, TId, TDto, TViewModel, TMapper, TSearchParams, TConvertParams>(IApiHttpClient apiHttpClient)
    : IBaseService<TId, TViewModel, TSearchParams, TConvertParams>
    where TEndpoint : IBaseApiEndpointsConstants<TEndpoint, TId>
    where TId : struct
    where TDto : BaseDto<TId>
    where TViewModel : BaseViewModel<TId>
    where TMapper : IMapper<TViewModel, TDto>
    where TSearchParams : BaseSearchParams
    where TConvertParams : class
{
    public async Task<TViewModel> GetAsync(TId id, TConvertParams? convertParams = null, CancellationToken cancellationToken = default)
    {
        var dto = (await apiHttpClient.GetAsync<RestApiResponse<TDto>>(
            endpoint: TEndpoint.ById(id),
            convertParams: convertParams,
            cancellationToken: cancellationToken)).Payload;

        return TMapper.ToViewModel(dto!);
    }

    public async Task<SearchResult<TViewModel>> GetAsync(TSearchParams searchParams, TConvertParams? convertParams = null, CancellationToken cancellationToken = default)
    {
        var searchResult = (await apiHttpClient.GetAsync<RestApiResponse<SearchResult<TDto>>>(
            endpoint: TEndpoint.GetByFilter(),
            searchParams: searchParams,
            convertParams: convertParams,
            cancellationToken: cancellationToken)).Payload;

        return searchResult!.Map(TMapper.ToViewModel);
    }

    public async Task<TId> CreateAsync(TViewModel viewModel, CancellationToken cancellationToken = default)
    {
        return (await apiHttpClient.PostAsync<TDto, RestApiResponse<TId>>(
            endpoint: TEndpoint.Base(),
            request: TMapper.ToDto(viewModel),
            cancellationToken: cancellationToken)).Payload!;
    }

    public Task UpdateAsync(TId id, TViewModel viewModel, CancellationToken cancellationToken = default)
    {
        return apiHttpClient.PutAsync<TDto, RestApiResponse<NoContent>>(
            endpoint: TEndpoint.ById(id),
            request: TMapper.ToDto(viewModel),
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        return apiHttpClient.DeleteAsync<RestApiResponse<object>>(
            endpoint: TEndpoint.ById(id),
            cancellationToken: cancellationToken);
    }
}
