using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.UpdateHeroPage.Model;

public sealed class UpdateHeroFormModelValidatorTests
{
    [Fact(DisplayName = "Validate should require text and faction when model is empty")]
    public void Validate_Should_RequireTextAndFaction_When_ModelIsEmpty()
    {
        var validator = new UpdateHeroFormModelValidator();

        var result = validator.Validate(new UpdateHeroFormModel());

        result.Errors.Select(error => error.PropertyName).ShouldBe(["Name", "Description", "FactionId"]);
    }

    [Fact(DisplayName = "Validate should reject long text when field limits are exceeded")]
    public void Validate_Should_RejectLongText_When_FieldLimitsAreExceeded()
    {
        var model = new UpdateHeroFormModel
        {
            Name = new string('a', 129),
            Description = new string('b', 2001),
            FactionId = Guid.NewGuid()
        };
        var validator = new UpdateHeroFormModelValidator();

        var result = validator.Validate(model);

        result.Errors.Select(error => error.PropertyName).ShouldBe(["Name", "Description"]);
    }

    [Theory(DisplayName = "Validate should reject invalid stats when domain bounds are violated")]
    [InlineData(-1, 0, 0, 0, 0, 0, 0, "Attack")]
    [InlineData(0, -1, 0, 0, 0, 0, 0, "Defense")]
    [InlineData(0, 0, -1, 0, 0, 0, 0, "MinimumDamage")]
    [InlineData(0, 0, 0, -1, 0, 0, 0, "MaximumDamage")]
    [InlineData(0, 0, 4, 3, 0, 0, 0, "MaximumDamage")]
    [InlineData(0, 0, 0, 0, -0.5, 0, 0, "Initiative")]
    [InlineData(0, 0, 0, 0, double.NaN, 0, 0, "Initiative")]
    [InlineData(0, 0, 0, 0, double.PositiveInfinity, 0, 0, "Initiative")]
    [InlineData(0, 0, 0, 0, double.NegativeInfinity, 0, 0, "Initiative")]
    [InlineData(0, 0, 0, 0, 0, -1, 0, "Morale")]
    [InlineData(0, 0, 0, 0, 0, 6, 0, "Morale")]
    [InlineData(0, 0, 0, 0, 0, 0, -1, "Luck")]
    [InlineData(0, 0, 0, 0, 0, 0, 6, "Luck")]
    public void Validate_Should_RejectInvalidStats_When_DomainBoundsAreViolated(
        int attack, int defense, int minimumDamage, int maximumDamage, double initiative,
        int morale, int luck, string field)
    {
        var model = new UpdateHeroFormModel
        {
            Name = "Catherine",
            Description = "Leader of the alliance.",
            FactionId = Guid.NewGuid(),
            Attack = attack,
            Defense = defense,
            MinimumDamage = minimumDamage,
            MaximumDamage = maximumDamage,
            Initiative = initiative,
            Morale = morale,
            Luck = luck
        };
        var validator = new UpdateHeroFormModelValidator();

        var result = validator.Validate(model);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).Distinct().ShouldBe([field]);
    }

    [Theory(DisplayName = "Validate should accept boundary stats when all fields satisfy domain rules")]
    [InlineData(0, 0, 0)]
    [InlineData(5, 5, 1.5)]
    public void Validate_Should_AcceptBoundaryStats_When_AllFieldsSatisfyDomainRules(int morale, int luck, double initiative)
    {
        var model = new UpdateHeroFormModel
        {
            Name = new string('a', 128),
            Description = new string('b', 2000),
            FactionId = Guid.NewGuid(),
            Morale = morale,
            Luck = luck,
            Initiative = initiative
        };
        var validator = new UpdateHeroFormModelValidator();

        var result = validator.Validate(model);

        result.IsValid.ShouldBeTrue();
    }
}
