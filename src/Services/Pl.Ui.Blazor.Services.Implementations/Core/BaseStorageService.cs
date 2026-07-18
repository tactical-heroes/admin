using Common.Clients.Interfaces;
using Common.Constants.ApiEndpoints.Core;
using Common.Storage.Dtos;

using Pl.Api.Http.Dtos.Core;
using Pl.Ui.Blazor.Services.Interfaces.Core;

using System.Net;

namespace Pl.Ui.Blazor.Services.Implementations.Core;

public abstract class BaseStorageService<TEndpoint>(IApiHttpClient apiHttpClient) : IBaseStorageService
    where TEndpoint : IBaseStorageApiEndpointsConstants<TEndpoint>
{
    public async Task<PresignedUrl> GetPresignedUploadUrlAsync(string fileName, string? contentType = null, CancellationToken cancellationToken = default)
    {
        var response = await apiHttpClient.GetAsync<RestApiResponse<PresignedUrl>>(
            endpoint: TEndpoint.PresignedUpload(fileName, contentType),
            cancellationToken: cancellationToken);

        return response.Payload!;
    }

    public async Task<PresignedUrl> GetPresignedDownloadUrlAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await apiHttpClient.GetAsync<RestApiResponse<PresignedUrl>>(
            endpoint: TEndpoint.PresignedDownload(key),
            cancellationToken: cancellationToken);

        return response.Payload!;
    }

    public Task<FileContent> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        return apiHttpClient.DownloadAsync(
            endpoint: TEndpoint.Download(key),
            cancellationToken: cancellationToken);
    }

    public async Task<string> UploadAsync(FileContent fileContent, CancellationToken cancellationToken = default)
    {
        var response = await apiHttpClient.UploadAsync<RestApiResponse<string>>(
            endpoint: TEndpoint.Upload(),
            fileContent: fileContent,
            expectedStatus: HttpStatusCode.OK,
            cancellationToken: cancellationToken);

        return response.Payload!;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        return apiHttpClient.DeleteAsync<object>(
            endpoint: TEndpoint.Delete(key),
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        return apiHttpClient.PostAsync<IReadOnlyCollection<string>, object>(
            endpoint: TEndpoint.DeleteBatch(),
            request: keys,
            expectedStatus: HttpStatusCode.NoContent,
            cancellationToken: cancellationToken);
    }
}
