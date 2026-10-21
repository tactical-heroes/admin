using TacticalHeroes.Admin.Api.Mapping;

namespace TacticalHeroes.Admin.Api.UnitTests.Mapping;

public sealed class RequiredValueMapperTests
{
    [Fact(DisplayName = "ToRequiredList should map objects when collection contains objects")]
    public void ToRequiredList_Should_MapObjects_When_CollectionContainsObjects()
    {
        Item[] source = [new("First"), new("Second")];

        var result = RequiredValueMapper.ToRequiredList(source, static item => item.Name);

        result.ShouldBe(["First", "Second"]);
    }

    [Fact(DisplayName = "ToRequiredList should preserve values when elements contain default values")]
    public void ToRequiredList_Should_PreserveValues_When_ElementsContainDefaultValues()
    {
        int[] numbers = [0, 42];
        bool[] flags = [false, true];
        double[] fractions = [0, 1.5];
        Guid[] identifiers = [Guid.Empty];
        string[] names = [string.Empty, "Name"];

        RequiredValueMapper.ToRequiredList(numbers, static value => value).ShouldBe(numbers);
        RequiredValueMapper.ToRequiredList(flags, static value => value).ShouldBe(flags);
        RequiredValueMapper.ToRequiredList(fractions, static value => value).ShouldBe(fractions);
        RequiredValueMapper.ToRequiredList(identifiers, static value => value).ShouldBe(identifiers);
        RequiredValueMapper.ToRequiredList(names, static value => value).ShouldBe(names);
    }

    [Fact(DisplayName = "ToRequiredList should convert nullable identifiers when every item has a value")]
    public void ToRequiredList_Should_ConvertNullableIdentifiers_When_EveryItemHasAValue()
    {
        Guid id = Guid.Parse("01992fa0-0000-7000-8000-000000000001");
        Guid?[] source = [Guid.Empty, id];

        var result = RequiredValueMapper.ToRequiredList(source, RequiredValueMapper.ToRequiredGuid);
        var apiValues = RequiredValueMapper.ToRequiredList(result, static value => (Guid?)value);

        result.ShouldBe([Guid.Empty, id]);
        apiValues.ShouldBe(source);
    }

    [Fact(DisplayName = "ToRequiredList should return empty list when collection is empty")]
    public void ToRequiredList_Should_ReturnEmptyList_When_CollectionIsEmpty()
    {
        string[] source = [];

        var result = RequiredValueMapper.ToRequiredList(source, static value => value);

        result.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ToRequiredList should reject null collection when collection is missing")]
    public void ToRequiredList_Should_RejectNullCollection_When_CollectionIsMissing()
    {
        IReadOnlyCollection<string>? source = null;

        Should.Throw<ArgumentNullException>(() => RequiredValueMapper.ToRequiredList(source, static value => value))
            .ParamName.ShouldBe("values");
    }

    [Fact(DisplayName = "ToRequiredList should reject null references when converter provides a fallback")]
    public void ToRequiredList_Should_RejectNullReferences_When_ConverterProvidesAFallback()
    {
        string?[] source = [null];

        Should.Throw<ArgumentNullException>(() => RequiredValueMapper.ToRequiredList(source, static value => value ?? "fallback"))
            .ParamName.ShouldBe("value");
    }

    [Theory(DisplayName = "ToRequiredList should reject null values when nullable value types are used")]
    [InlineData("Guid")]
    [InlineData("Int32")]
    [InlineData("Boolean")]
    [InlineData("Double")]
    public void ToRequiredList_Should_RejectNullValues_When_NullableValueTypesAreUsed(string elementType)
    {
        Action convert = elementType switch
        {
            "Guid" => () => RequiredValueMapper.ToRequiredList<Guid?, Guid>([null], static value => value ?? Guid.Empty),
            "Int32" => () => RequiredValueMapper.ToRequiredList<int?, int>([null], static value => value ?? 0),
            "Boolean" => () => RequiredValueMapper.ToRequiredList<bool?, bool>([null], static value => value ?? false),
            "Double" => () => RequiredValueMapper.ToRequiredList<double?, double>([null], static value => value ?? 0),
            _ => throw new ArgumentOutOfRangeException(nameof(elementType)),
        };

        Should.Throw<ArgumentNullException>(convert).ParamName.ShouldBe("value");
    }

    [Fact(DisplayName = "ToRequiredList should reject null result when converter returns null")]
    public void ToRequiredList_Should_RejectNullResult_When_ConverterReturnsNull()
    {
        string[] source = ["Value"];

        Should.Throw<ArgumentNullException>(() => RequiredValueMapper.ToRequiredList(source, static _ => (string?)null))
            .ParamName.ShouldBe("mappedValue");
    }

    [Fact(DisplayName = "ToRequiredList should reject null converter when converter is missing")]
    public void ToRequiredList_Should_RejectNullConverter_When_ConverterIsMissing()
    {
        string[] source = ["Value"];

        Should.Throw<ArgumentNullException>(() => RequiredValueMapper.ToRequiredList<string, string>(source, null!))
            .ParamName.ShouldBe("map");
    }

    private sealed record Item(string Name);
}
