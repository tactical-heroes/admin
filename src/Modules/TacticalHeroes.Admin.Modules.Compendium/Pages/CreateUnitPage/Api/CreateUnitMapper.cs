using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Api;

[Mapper]
internal static partial class CreateUnitMapper
{
    [MapPropertyFromSource(nameof(CreateUnitRequest.AdditionalData), Use = nameof(ToAdditionalData))]
    public static partial CreateUnitRequest ToRequest(CreateUnitFormModel unit);

    [MapperIgnore]
    public static Guid ToId(CreateUnitResponse response)
    {
        return response.Id!.Value;
    }

    private static IDictionary<string?, object?> ToAdditionalData(CreateUnitFormModel unit)
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
