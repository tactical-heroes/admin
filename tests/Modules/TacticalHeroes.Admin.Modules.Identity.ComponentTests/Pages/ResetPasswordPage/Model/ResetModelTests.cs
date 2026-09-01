using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.ResetPasswordPage.Model;

public sealed class ResetModelTests
{
    [Fact(DisplayName = "Stores password values")]
    public void ResetModel_Should_StoreValues_When_ValuesAreAssigned()
    {
        var model = new ResetModel
        {
            Password = "secret-password",
            PasswordConfirmation = "secret-password",
        };

        model.Password.ShouldBe("secret-password");
        model.PasswordConfirmation.ShouldBe("secret-password");
    }
}
