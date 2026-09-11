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

    [MapPropertyFromSource(nameof(UpdateUnitRequest.AdditionalData), Use = nameof(ToAdditionalData))]
    public static partial UpdateUnitRequest ToRequest(UpdateUnitFormModel unit);

    private static IDictionary<string?, object?> ToAdditionalData(UpdateUnitFormModel unit)
    {
        Dictionary<string, object?> additionalData = [];
        if (unit.Shots is null)
        {
            additionalData["shots"] = null;
        }

        if (unit.RangedAttackRange is null)
        {
            additionalData["rangedAttackRange"] = null;
        }

        return additionalData!;
    }
}
