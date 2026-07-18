using Common.Clients.Interfaces;
using Common.Constants.ApiEndpoints;

using Pl.Ui.Blazor.Services.Implementations.Core;
using Pl.Ui.Blazor.Services.Interfaces;

namespace Pl.Ui.Blazor.Services.Implementations;

public sealed class AvatarsService(IApiHttpClient apiHttpClient) :
    BaseStorageService<AvatarsApiEndpointsConstants>(apiHttpClient),
    IAvatarsService
{
}
