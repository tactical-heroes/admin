namespace TacticalHeroes.Admin.Modules.Identity;

public static class IdentityPermissions
{
    public static class Roles
    {
        public const string Read = "identity.roles.read";
        public const string Create = "identity.roles.create";
        public const string Update = "identity.roles.update";
        public const string Delete = "identity.roles.delete";
    }

    public static class Users
    {
        public const string Read = "identity.users.read";
        public const string Create = "identity.users.create";
        public const string Update = "identity.users.update";
        public const string Delete = "identity.users.delete";
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
