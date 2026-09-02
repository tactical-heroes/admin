using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TacticalHeroes.Admin.Shared.Model;

public static class EnumExtensions
{
    public static string ToSnakeCase(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Type enumerationType = value.GetType();
        string? name = Enum.GetName(enumerationType, value);

        return name is null
            ? value.ToString()
            : GetSnakeCaseName(enumerationType, name);
    }

    public static bool TryParseSnakeCase<TEnum>(
        this string? value,
        out TEnum result)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse(value, ignoreCase: true, out result) &&
            Enum.IsDefined(result))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            Type enumerationType = typeof(TEnum);
            string? name = Enum.GetNames<TEnum>()
                .FirstOrDefault(name => string.Equals(
                    GetSnakeCaseName(enumerationType, name),
                    value,
                    StringComparison.OrdinalIgnoreCase));

            if (name is not null)
            {
                return Enum.TryParse(name, out result);
            }
        }

        result = default;
        return false;
    }

    public static string GetDisplayName<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        string? name = Enum.GetName(value);
        if (name is null)
        {
            return value.ToString();
        }

        return typeof(TEnum)
                .GetField(name, BindingFlags.Public | BindingFlags.Static)?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName()
            ?? name;
    }

    private static string GetSnakeCaseName(Type enumerationType, string name)
    {
        return enumerationType
                .GetField(name, BindingFlags.Public | BindingFlags.Static)?
                .GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?
                .Name
            ?? JsonNamingPolicy.SnakeCaseLower.ConvertName(name);
    }
}
