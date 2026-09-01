namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

public sealed class UserListFilter : IEquatable<UserListFilter>
{
    public string? Email { get; set; }

    public bool Equals(UserListFilter? other)
    {
        return other is not null &&
               string.Equals(Email, other.Email, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) ||
               obj is UserListFilter other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Email);
    }
}
