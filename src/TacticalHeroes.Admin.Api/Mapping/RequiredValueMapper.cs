namespace TacticalHeroes.Admin.Api.Mapping;

public static class RequiredValueMapper
{
    public static List<TTarget> ToRequiredList<TSource, TTarget>(
        IReadOnlyCollection<TSource>? values,
        Func<TSource, TTarget> map)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(map);

        var result = new List<TTarget>(values.Count);
        foreach (var value in values)
        {
            ArgumentNullException.ThrowIfNull(value);
            var mappedValue = map(value);
            ArgumentNullException.ThrowIfNull(mappedValue);
            result.Add(mappedValue);
        }

        return result;
    }

    public static string ToRequiredString(string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value;
    }

    public static Guid ToRequiredGuid(Guid? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }

    public static bool ToRequiredBoolean(bool? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }

    public static int ToRequiredInt32(int? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }

    public static int ToRequiredInt32(long? value)
    {
        return checked((int)(value ?? throw new ArgumentNullException(nameof(value))));
    }

    public static long ToRequiredInt64(long? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }

    public static double ToRequiredDouble(double? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }
}
