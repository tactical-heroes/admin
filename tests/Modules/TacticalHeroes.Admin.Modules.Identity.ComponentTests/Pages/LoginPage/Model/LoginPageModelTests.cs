using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.LoginPage.Model;

public sealed class LoginPageModelTests
{
    [Fact(DisplayName = "Stores an email")]
    public void EmailModel_Should_StoreEmail_When_ValueIsAssigned()
    {
        var model = new EmailModel
        {
            Email = "hero@example.com",
        };

        model.Email.ShouldBe("hero@example.com");
    }

    [Fact(DisplayName = "Stores registration values")]
    public void RegisterModel_Should_StoreValues_When_ValuesAreAssigned()
    {
        var model = new RegisterModel
        {
            Email = "hero@example.com",
            UserName = "hero",
            Password = "secret-password",
            PasswordConfirmation = "secret-password",
        };

        model.Email.ShouldBe("hero@example.com");
        model.UserName.ShouldBe("hero");
        model.Password.ShouldBe("secret-password");
        model.PasswordConfirmation.ShouldBe("secret-password");
    }
}
