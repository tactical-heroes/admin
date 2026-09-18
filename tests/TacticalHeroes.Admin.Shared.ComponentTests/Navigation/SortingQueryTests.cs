namespace TacticalHeroes.Admin.Shared.ComponentTests.Navigation;

public sealed class SortingQueryTests
{
    [Theory(DisplayName = "Parse should preserve API criteria when compact or legacy values are provided")]
    [InlineData("isConfirmed", "isConfirmed:asc")]
    [InlineData("-email", "email:desc")]
    [InlineData(" -userName ", "userName:desc")]
    [InlineData("Email:desc", "Email:desc")]
    [InlineData("Name:asc", "Name:asc")]
    [InlineData("Name:invalid", "Name:invalid")]
    public void Parse_Should_PreserveApiCriteria_When_CompactOrLegacyValuesAreProvided(string value, string expected)
    {
        string criterion = SortingQuery.Parse(value);

        criterion.ShouldBe(expected);
    }

    [Theory(DisplayName = "Format should emit compact camel case when sorting values are provided")]
    [InlineData("IsConfirmed:asc", "isConfirmed")]
    [InlineData("Email:desc", "-email")]
    [InlineData(" UserName : DESC ", "-userName")]
    [InlineData("URL:asc", "url")]
    [InlineData("-email", "-email")]
    [InlineData("Name:invalid", "Name:invalid")]
    public void Format_Should_EmitCompactCamelCase_When_SortingValuesAreProvided(string value, string expected)
    {
        string criterion = SortingQuery.Format(value);

        criterion.ShouldBe(expected);
    }
}
