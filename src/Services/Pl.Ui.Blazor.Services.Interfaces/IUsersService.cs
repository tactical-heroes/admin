using Common.ConvertParams;
using Common.SearchParams;

using Pl.Ui.Blazor.Services.Interfaces.Core;
using Pl.Ui.Blazor.ViewModels;

namespace Pl.Ui.Blazor.Services.Interfaces;

public interface IUsersService : IBaseService<Guid, UserViewModel, UsersSearchParams, UsersConvertParams>
{
}
