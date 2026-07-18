using Pl.Api.Http.Dtos.Models;
using Pl.Ui.Blazor.Mappers.Core;
using Pl.Ui.Blazor.ViewModels;

using Riok.Mapperly.Abstractions;

namespace Pl.Ui.Blazor.Mappers;

[Mapper]
public sealed partial class UsersMapper : IMapper<UserViewModel, UserDto>
{
    public static partial UserViewModel ToViewModel(UserDto dto);
    public static partial IEnumerable<UserViewModel> ToViewModel(IEnumerable<UserDto> dtos);
    public static partial UserDto ToDto(UserViewModel viewModel);
    public static partial IEnumerable<UserDto> ToDto(IEnumerable<UserViewModel> viewModels);
}
