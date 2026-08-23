using PANiXiDA.Core.ResultPattern;

namespace TacticalHeroes.Admin.Shared.Model;

public interface IEnumerationProvider<TEnumeration>
    where TEnumeration : class, IEnumeration
{
    Task<Result<IReadOnlyList<TEnumeration>>> GetAllAsync(
        CancellationToken cancellationToken);
}
