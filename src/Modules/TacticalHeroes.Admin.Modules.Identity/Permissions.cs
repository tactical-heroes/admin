namespace TacticalHeroes.Admin.Modules.Identity;

public static class Permissions
{
    public const string Code = "identity";

    public static class Roles
    {
        public const string ResourceCode = "roles";

        public const string Read = $"{Code}.{ResourceCode}.read";
        public const string Create = $"{Code}.{ResourceCode}.create";
        public const string Update = $"{Code}.{ResourceCode}.update";
        public const string Delete = $"{Code}.{ResourceCode}.delete";
    }

    public static class Users
    {
        public const string ResourceCode = "users";

        public const string Read = $"{Code}.{ResourceCode}.read";
        public const string Create = $"{Code}.{ResourceCode}.create";
        public const string Update = $"{Code}.{ResourceCode}.update";
        public const string Delete = $"{Code}.{ResourceCode}.delete";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        Roles.Read,
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        Users.Read,
        Users.Create,
        Users.Update,
        Users.Delete
    ];
}
