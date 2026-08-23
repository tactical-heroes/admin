using FluentValidation.Results;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.CreateRolePage.Model;

public sealed class CreateRoleFormModelValidatorTests
{
    [Fact(DisplayName = "Rejects a role containing an empty claim")]
    public void Validate_Should_ReturnClaimErrors_When_ClaimIsEmpty()
    {
        var model = new CreateRoleFormModel
        {
            Name = "Administrators",
            Claims = [new ClaimValue()],
        };

        ValidationResult result = new CreateRoleFormModelValidator().Validate(model);

        result.Errors
            .Select(error => error.PropertyName)
            .ShouldBe([
                "Claims[0].Type",
                "Claims[0].Value",
            ]);
    }
}
