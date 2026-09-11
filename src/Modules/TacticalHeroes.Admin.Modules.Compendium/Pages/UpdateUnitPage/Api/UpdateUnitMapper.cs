using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UpdateUnitMapper
{
    [MapperIgnoreSource(nameof(GetUnitDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetUnitDetailsResponse.Id))]
    public static partial UpdateUnitFormModel ToForm(GetUnitDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateUnitRequest.AdditionalData))]
    public static partial UpdateUnitRequest ToRequest(UpdateUnitFormModel unit);
}
