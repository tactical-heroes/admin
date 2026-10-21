namespace TacticalHeroes.Admin.Modules.Compendium;

public static class CompendiumPermissions
{
    public const string Code = "compendium";

    public static class Factions
    {
        public const string ResourceCode = "factions";

        public const string Read = $"{Code}.{ResourceCode}.read";
        public const string Create = $"{Code}.{ResourceCode}.create";
        public const string Update = $"{Code}.{ResourceCode}.update";
        public const string Delete = $"{Code}.{ResourceCode}.delete";
    }

    public static class Heroes
    {
        public const string ResourceCode = "heroes";

        public const string Read = $"{Code}.{ResourceCode}.read";
        public const string Create = $"{Code}.{ResourceCode}.create";
        public const string Update = $"{Code}.{ResourceCode}.update";
        public const string Delete = $"{Code}.{ResourceCode}.delete";
    }

    public static class Units
    {
        public const string ResourceCode = "units";

        public const string Read = $"{Code}.{ResourceCode}.read";
        public const string Create = $"{Code}.{ResourceCode}.create";
        public const string Update = $"{Code}.{ResourceCode}.update";
        public const string Delete = $"{Code}.{ResourceCode}.delete";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        Factions.Read,
        Factions.Create,
        Factions.Update,
        Factions.Delete,
        Heroes.Read,
        Heroes.Create,
        Heroes.Update,
        Heroes.Delete,
        Units.Read,
        Units.Create,
        Units.Update,
        Units.Delete
    ];
}
