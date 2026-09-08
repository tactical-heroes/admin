using System.Reflection;

using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class RouteStateConventionTests
{
    [Theory(DisplayName = "Paged list CLR properties expose the pagination query contract")]
    [InlineData(typeof(MudPagedListComponentBase<>), nameof(MudPagedListComponentBase<object>.PageNumber), "page")]
    [InlineData(typeof(MudPagedListComponentBase<>), nameof(MudPagedListComponentBase<object>.PageSize), "pageSize")]
    [InlineData(typeof(MudPagedListComponentBase<,>), nameof(MudPagedListComponentBase<object>.PageNumber), "page")]
    [InlineData(typeof(MudPagedListComponentBase<,>), nameof(MudPagedListComponentBase<object>.PageSize), "pageSize")]
    public void ListPages_Should_UseQueryParameters_When_ListStateIsDefined(
        Type componentType,
        string propertyName,
        string queryName)
    {
        PropertyInfo? property = componentType.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        property.ShouldNotBeNull();
        property.PropertyType.ShouldBe(typeof(int?));
        property.SetMethod.ShouldNotBeNull();
        property.SetMethod.IsPublic.ShouldBeTrue();
        var attribute = property.GetCustomAttribute<SupplyParameterFromQueryAttribute>();
        attribute.ShouldNotBeNull();
        StringComparer.OrdinalIgnoreCase.Equals(attribute.Name ?? property.Name, queryName)
            .ShouldBeTrue($"{componentType.Name}.{propertyName} must bind query parameter '{queryName}'.");
    }
}
