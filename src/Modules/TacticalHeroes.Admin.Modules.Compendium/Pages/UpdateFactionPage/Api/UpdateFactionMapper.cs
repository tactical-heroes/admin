using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UpdateFactionMapper
{
    [MapperIgnoreSource(nameof(GetFactionDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetFactionDetailsResponse.Id))]
    public static partial UpdateFactionFormModel ToForm(GetFactionDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateFactionRequest.AdditionalData))]
    public static partial UpdateFactionRequest ToRequest(UpdateFactionFormModel faction);
}
