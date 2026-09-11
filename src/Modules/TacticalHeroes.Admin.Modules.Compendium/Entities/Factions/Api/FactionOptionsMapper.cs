using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class FactionOptionsMapper
{
    public static partial IReadOnlyList<SelectOption<Guid>> ToOptions(List<FactionSelectOptionResponse> response);

    [MapperIgnoreSource(nameof(FactionSelectOptionResponse.AdditionalData))]
    private static partial SelectOption<Guid> ToOption(FactionSelectOptionResponse response);
}
