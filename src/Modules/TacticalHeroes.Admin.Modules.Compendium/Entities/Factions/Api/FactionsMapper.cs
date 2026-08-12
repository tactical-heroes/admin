using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

[Mapper(ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class FactionsMapper
{
    [MapperIgnoreSource(nameof(FactionListItemResponse.AdditionalData))]
    public static partial FactionListItem ToListItem(FactionListItemResponse response);

    [MapperIgnoreSource(nameof(GetFactionDetailsResponse.AdditionalData))]
    [MapProperty(nameof(GetFactionDetailsResponse.Id), nameof(FactionDetails.Id), Use = nameof(RequireId))]
    public static partial FactionDetails ToDetails(GetFactionDetailsResponse response);

    [MapperIgnoreSource(nameof(FactionDetails.Id))]
    [MapperIgnoreTarget(nameof(CreateFactionRequest.AdditionalData))]
    [MapProperty(nameof(FactionDetails.Name), nameof(CreateFactionRequest.Name), Use = nameof(Trim))]
    [MapProperty(
        nameof(FactionDetails.Description),
        nameof(CreateFactionRequest.Description),
        Use = nameof(Trim))]
    public static partial CreateFactionRequest ToCreateRequest(FactionDetails faction);

    [MapperIgnoreSource(nameof(FactionDetails.Id))]
    [MapperIgnoreTarget(nameof(UpdateFactionRequest.AdditionalData))]
    [MapProperty(nameof(FactionDetails.Name), nameof(UpdateFactionRequest.Name), Use = nameof(Trim))]
    [MapProperty(
        nameof(FactionDetails.Description),
        nameof(UpdateFactionRequest.Description),
        Use = nameof(Trim))]
    public static partial UpdateFactionRequest ToUpdateRequest(FactionDetails faction);

    [MapperIgnore]
    public static PaginationResult<FactionListItem> ToPage(
        PaginationResultOfFactionListItemResponse response,
        int pageNumber,
        int pageSize)
    {
        var items = response.Items?
            .Select(ToListItem)
            .ToArray() ?? [];

        return new PaginationResult<FactionListItem>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }

    [MapperIgnore]
    public static Guid ToId(CreateFactionResponse response)
    {
        return response.Id
            ?? throw new ArgumentNullException(nameof(response.Id));
    }

    [UserMapping(Default = false)]
    private static string Trim(string value)
    {
        return value.Trim();
    }

    [UserMapping(Default = false)]
    private static Guid? RequireId(Guid? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }
}
