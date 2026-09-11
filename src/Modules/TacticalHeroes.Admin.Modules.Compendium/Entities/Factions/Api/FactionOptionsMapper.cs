using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class FactionOptionsMapper
{
    public static partial IReadOnlyList<FactionOption> ToOptions(List<FactionSelectOptionResponse> response);

    [MapperIgnoreSource(nameof(FactionSelectOptionResponse.AdditionalData))]
    private static partial FactionOption ToOption(FactionSelectOptionResponse response);

    [MapperIgnoreSource(nameof(GetFactionDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetFactionDetailsResponse.Description))]
    public static partial FactionOption ToOption(GetFactionDetailsResponse response);
}
