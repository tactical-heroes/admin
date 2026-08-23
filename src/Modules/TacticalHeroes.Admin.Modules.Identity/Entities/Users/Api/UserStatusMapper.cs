using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UserStatusMapper
{
    [MapperIgnoreSource(nameof(UserStatusResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(UserStatusResponse.Id))]
    public static partial UserStatus ToValue(UserStatusResponse response);

    public static partial IReadOnlyList<UserStatus> ToValues(
        IReadOnlyCollection<UserStatusResponse> response);
}
