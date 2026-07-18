using Common.Storage.Dtos;

namespace Pl.Ui.Blazor.Services.Interfaces.Core;

public interface IBaseStorageService
{
    Task<PresignedUrl> GetPresignedUploadUrlAsync(string fileName, string? contentType = null, CancellationToken cancellationToken = default);
    Task<PresignedUrl> GetPresignedDownloadUrlAsync(string key, CancellationToken cancellationToken = default);
    Task<FileContent> DownloadAsync(string key, CancellationToken cancellationToken = default);
    Task<string> UploadAsync(FileContent fileContent, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default);
}
