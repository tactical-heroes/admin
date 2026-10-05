using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Api;

[Mapper]
internal static partial class CreateUnitMapper
{
    [MapperIgnoreTarget(nameof(CreateUnitRequest.AdditionalData))]
    public static partial CreateUnitRequest ToRequest(CreateUnitFormModel unit);

    [MapperIgnore]
    public static Guid ToId(CreateUnitResponse response)
    {
        return response.Id!.Value;
    }

    [ObjectFactory]
    private static CreateUnitRequest CreateRequest(CreateUnitFormModel unit)
    {
        var request = new CreateUnitRequest();
        if (unit.Shots is null)
        {
            request.AdditionalData["shots"] = null!;
        }

        if (unit.RangedAttackRange is null)
        {
            request.AdditionalData["rangedAttackRange"] = null!;
        }

        return request;
    }
}
