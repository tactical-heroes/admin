using Common.ConvertParams;
using Common.SearchParams;

using Pl.Ui.Blazor.Admin.Pages.Core;
using Pl.Ui.Blazor.Services.Interfaces;
using Pl.Ui.Blazor.ViewModels;

namespace Pl.Ui.Blazor.Admin.Pages.Users;

public partial class Users : BaseFilterableTable<Guid, UserViewModel, UsersSearchParams, UsersConvertParams, IUsersService>
{
    private const string Title = "Пользователи";
    protected override string EditRoute => "/users/edit";
}
