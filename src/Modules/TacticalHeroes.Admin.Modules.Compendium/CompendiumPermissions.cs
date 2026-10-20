namespace TacticalHeroes.Admin.Modules.Compendium;

public static class CompendiumPermissions
{
    public static class Factions
    {
        public const string Read = "compendium.factions.read";
        public const string Create = "compendium.factions.create";
        public const string Update = "compendium.factions.update";
        public const string Delete = "compendium.factions.delete";
    }

    public static class Heroes
    {
        public const string Read = "compendium.heroes.read";
        public const string Create = "compendium.heroes.create";
        public const string Update = "compendium.heroes.update";
        public const string Delete = "compendium.heroes.delete";
    }

    public static class Units
    {
        public const string Read = "compendium.units.read";
        public const string Create = "compendium.units.create";
        public const string Update = "compendium.units.update";
        public const string Delete = "compendium.units.delete";
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
