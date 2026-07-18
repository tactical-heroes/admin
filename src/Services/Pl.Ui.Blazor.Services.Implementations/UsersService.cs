using Common.Clients.Interfaces;
using Common.Constants.ApiEndpoints;
using Common.ConvertParams;
using Common.SearchParams;

using Pl.Api.Http.Dtos.Models;
using Pl.Ui.Blazor.Mappers;
using Pl.Ui.Blazor.Services.Implementations.Core;
using Pl.Ui.Blazor.Services.Interfaces;
using Pl.Ui.Blazor.ViewModels;

namespace Pl.Ui.Blazor.Services.Implementations;

public sealed class UsersService(IApiHttpClient apiHttpClient) :
    BaseService<UsersApiEndpointsConstants, Guid, UserDto, UserViewModel, UsersMapper, UsersSearchParams, UsersConvertParams>(apiHttpClient),
    IUsersService
{
}