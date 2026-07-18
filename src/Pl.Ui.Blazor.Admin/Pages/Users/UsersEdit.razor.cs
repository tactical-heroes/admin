using Common.ConvertParams;
using Common.SearchParams;

using Microsoft.AspNetCore.Components;

using Pl.Ui.Blazor.Admin.Pages.Core;
using Pl.Ui.Blazor.Services.Interfaces;
using Pl.Ui.Blazor.ViewModels;

namespace Pl.Ui.Blazor.Admin.Pages.Users;

public partial class UsersEdit : BaseEdit<Guid, UserViewModel, UsersSearchParams, UsersConvertParams, IUsersService>
{
    [Inject] protected IAvatarsService AvatarsService { get; set; } = default!;

    protected override string TableRoute => "/users";
    protected override string EditRoute => "/users/edit";

    private Func<CancellationToken, Task>? _avatarCommit;

    private void RegisterAvatarCommit(Func<CancellationToken, Task> commitHandler)
    {
        _avatarCommit = commitHandler;
    }

    protected override async Task OnValidSubmit()
    {
        if (_avatarCommit is not null)
        {
            await _avatarCommit.Invoke(CancellationToken.None);
        }

        await base.OnValidSubmit();
    }
}
