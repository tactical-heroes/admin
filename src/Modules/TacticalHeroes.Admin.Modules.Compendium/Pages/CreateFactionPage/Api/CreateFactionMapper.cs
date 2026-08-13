using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;

[Mapper]
internal static partial class CreateFactionMapper
{
    [MapperIgnoreTarget(nameof(CreateFactionRequest.AdditionalData))]
    public static partial CreateFactionRequest ToRequest(CreateFactionFormModel faction);

    [MapperIgnore]
    public static Guid ToId(CreateFactionResponse response)
    {
        return response.Id!.Value;
    }
}
