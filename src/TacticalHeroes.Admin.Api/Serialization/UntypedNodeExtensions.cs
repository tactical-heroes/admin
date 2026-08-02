using System.Globalization;

using Microsoft.Kiota.Abstractions.Serialization;

namespace TacticalHeroes.Admin.Api.Serialization;

public static class UntypedNodeExtensions
{
    public static long ToInt64(this UntypedNode? node)
    {
        return node switch
        {
            UntypedInteger integer => integer.GetValue(),
            UntypedLong longInteger => longInteger.GetValue(),
            UntypedDecimal decimalNumber => decimal.ToInt64(decimalNumber.GetValue()),
            UntypedDouble doubleNumber => Convert.ToInt64(
                doubleNumber.GetValue(),
                CultureInfo.InvariantCulture),
            UntypedString text when long.TryParse(
                text.GetValue(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var value) => value,
            _ => 0,
        };
    }
}
