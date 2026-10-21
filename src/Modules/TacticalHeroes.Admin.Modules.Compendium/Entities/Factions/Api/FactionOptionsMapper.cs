using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class FactionOptionsMapper
{
    public static IReadOnlyList<FactionSelectOption> ToOptions(List<FactionSelectOptionResponse>? response) =>
        RequiredValueMapper.ToRequiredList(response, ToOption);

    [MapperIgnoreSource(nameof(FactionSelectOptionResponse.AdditionalData))]
    private static partial FactionSelectOption ToOption(FactionSelectOptionResponse response);
}
