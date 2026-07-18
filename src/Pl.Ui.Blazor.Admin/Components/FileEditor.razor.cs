using Common.Storage.Dtos;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Pl.Ui.Blazor.Services.Interfaces.Core;

namespace Pl.Ui.Blazor.Admin.Components;

public partial class FileEditor<TStorageService> : ComponentBase
    where TStorageService : IBaseStorageService
{
    [Parameter, EditorRequired] public TStorageService StorageService { get; set; } = default!;

    [Parameter] public Action<Func<CancellationToken, Task>>? OnRegisterSubmitHandler { get; set; }

    [Parameter] public string? Key { get; set; }
    [Parameter] public EventCallback<string?> KeyChanged { get; set; }

    [Parameter] public string? AltText { get; set; }
    [Parameter] public string? Label { get; set; }

    [Parameter] public bool ShowKey { get; set; } = true;

    private bool _loading;

    private IBrowserFile? _pendingFile;

    private string? _previewUrl;
    private string? _keyToDelete;

    protected override void OnInitialized()
    {
        OnRegisterSubmitHandler?.Invoke(CommitAsync);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_pendingFile is not null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            _previewUrl = null;
            return;
        }

        _loading = true;
        StateHasChanged();

        try
        {
            var presigned = await StorageService.GetPresignedDownloadUrlAsync(Key);
            _previewUrl = presigned.Url;
        }
        catch
        {
            _previewUrl = null;
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        _loading = true;
        StateHasChanged();

        var oldKey = Key;
        var newKey = string.Empty;

        try
        {
            if (_pendingFile is not null)
            {
                await using var content = _pendingFile.OpenReadStream(cancellationToken: cancellationToken);

                var fileContent = new FileContent(content, _pendingFile.ContentType, _pendingFile.Name);
                newKey = await StorageService.UploadAsync(fileContent, cancellationToken);

                await KeyChanged.InvokeAsync(newKey);

                _pendingFile = null;
            }

            if (!string.IsNullOrWhiteSpace(oldKey) && !string.IsNullOrWhiteSpace(newKey) && !string.Equals(oldKey, newKey, StringComparison.Ordinal))
            {
                _keyToDelete = oldKey;
            }
            if (!string.IsNullOrWhiteSpace(_keyToDelete))
            {
                await StorageService.DeleteAsync(_keyToDelete, cancellationToken);
                _keyToDelete = null;
            }
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private async Task OnFileChangedAsync(IBrowserFile file)
    {
        _loading = true;
        StateHasChanged();

        try
        {
            _pendingFile = file;
            _previewUrl = await BuildDataUrlAsync(file);
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private async Task OnDeleteClickedAsync()
    {
        _pendingFile = null;
        _previewUrl = null;
        _keyToDelete = Key;

        await KeyChanged.InvokeAsync(null);

        StateHasChanged();
    }

    private static async Task<string> BuildDataUrlAsync(IBrowserFile file)
    {
        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        return $"data:{file.ContentType};base64,{base64}";
    }
}
