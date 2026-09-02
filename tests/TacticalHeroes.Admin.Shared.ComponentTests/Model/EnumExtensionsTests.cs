using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Model;

public sealed class EnumExtensionsTests
{
    [Fact(DisplayName = "Returns the enum display name")]
    public void GetDisplayName_Should_ReturnDisplayName_When_AttributeIsPresent()
    {
        string displayName = TestStatus.ReadyForBattle.GetDisplayName();

        displayName.ShouldBe("Ready for battle");
    }

    [Fact(DisplayName = "Falls back to the enum member name")]
    public void GetDisplayName_Should_ReturnMemberName_When_AttributeIsMissing()
    {
        string displayName = TestStatus.Unknown.GetDisplayName();

        displayName.ShouldBe(nameof(TestStatus.Unknown));
    }

    [Fact(DisplayName = "Converts the enum member name to snake case")]
    public void ToSnakeCase_Should_ReturnSnakeCase_When_ValueHasMultipleWords()
    {
        string value = TestStatus.ReadyForBattle.ToSnakeCase();

        value.ShouldBe("ready_for_battle");
    }

    [Fact(DisplayName = "Uses the configured enum member name")]
    public void ToSnakeCase_Should_ReturnConfiguredName_When_AttributeIsPresent()
    {
        string value = TestStatus.OAuth.ToSnakeCase();

        value.ShouldBe("oauth");
    }

    [Theory(DisplayName = "Parses supported enum name formats")]
    [InlineData("ready_for_battle")]
    [InlineData("ReadyForBattle")]
    [InlineData("0")]
    public void TryParseSnakeCase_Should_ReturnValue_When_NameIsSupported(string value)
    {
        bool parsed = value.TryParseSnakeCase(out TestStatus result);

        parsed.ShouldBeTrue();
        result.ShouldBe(TestStatus.ReadyForBattle);
    }

    [Fact(DisplayName = "Parses the configured enum member name")]
    public void TryParseSnakeCase_Should_ReturnValue_When_ConfiguredNameIsValid()
    {
        bool parsed = "oauth".TryParseSnakeCase(out TestStatus result);

        parsed.ShouldBeTrue();
        result.ShouldBe(TestStatus.OAuth);
    }

    [Fact(DisplayName = "Rejects an unknown snake case enum name")]
    public void TryParseSnakeCase_Should_ReturnFalse_When_NameIsUnknown()
    {
        bool parsed = "not_available".TryParseSnakeCase(out TestStatus result);

        parsed.ShouldBeFalse();
        result.ShouldBe(default);
    }

    private enum TestStatus
    {
        [Display(Name = "Ready for battle")]
        ReadyForBattle = 0,

        Unknown = 1,

        [JsonStringEnumMemberName("oauth")]
        OAuth = 2,
    }
}
