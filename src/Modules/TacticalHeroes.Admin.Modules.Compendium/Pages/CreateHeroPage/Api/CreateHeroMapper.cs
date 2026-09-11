using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Api;

[Mapper]
internal static partial class CreateHeroMapper
{
    [MapperIgnoreTarget(nameof(CreateHeroRequest.AdditionalData))]
    public static partial CreateHeroRequest ToRequest(CreateHeroFormModel hero);

    [MapperIgnore]
    public static Guid ToId(CreateHeroResponse response)
    {
        return response.Id!.Value;
    }
}
