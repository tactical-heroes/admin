using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class RoleOptionsMapper
{
    public static partial IReadOnlyList<RoleOption> ToOptions(List<RoleSelectOptionResponse> response);

    [MapperIgnoreSource(nameof(RoleSelectOptionResponse.AdditionalData))]
    private static partial RoleOption ToOption(RoleSelectOptionResponse response);
}
