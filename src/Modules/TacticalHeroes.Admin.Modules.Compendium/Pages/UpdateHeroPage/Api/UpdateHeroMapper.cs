using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UpdateHeroMapper
{
    [MapperIgnoreSource(nameof(GetHeroDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetHeroDetailsResponse.Id))]
    public static partial UpdateHeroFormModel ToForm(GetHeroDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateHeroRequest.AdditionalData))]
    public static partial UpdateHeroRequest ToRequest(UpdateHeroFormModel hero);
}
