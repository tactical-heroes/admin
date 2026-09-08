using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;
using TacticalHeroes.Admin.Shared.Model;

using ConfirmEmailPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Ui.ConfirmEmailPage;
using LoginPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui.LoginPage;
using ResetPasswordPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Ui.ResetPasswordPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages;

public sealed class IdentityMudFormTests : BunitContext
{
    private readonly AuthenticationHandler _handler = new();

    public IdentityMudFormTests()
    {
        Services.AddMudServices();
        Services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        Services.Configure<HttpClientFactoryOptions>(
            "TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = _handler));
        Services.AddIdentityAdminModule();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Register form validates an empty model through MudForm")]
    public void Register_Should_DisplayValidationErrors_When_ModelIsEmpty()
    {
        var component = Render<RegisterForm>();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email.");
            component.Markup.ShouldContain("Укажите имя пользователя.");
            component.Markup.ShouldContain("Укажите пароль.");
            component.Markup.ShouldContain("Повторите пароль.");
        });
    }

    [Fact(DisplayName = "Register form submits a valid MudForm model")]
    public void Register_Should_Submit_When_ModelIsValid()
    {
        var component = Render<RegisterForm>();
        component.Find("#register-email").Input("admin@example.com");
        component.Find("#register-user-name").Input("Administrator");
        component.Find("#register-password").Input("Password1!");
        component.Find("#register-password-confirmation").Input("Password1!");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("Аккаунт создан.");
        });
    }

    [Fact(DisplayName = "Login form displays the authentication error display name")]
    public void Login_Should_DisplayErrorMessage_When_AuthenticationErrorIsProvided()
    {
        var component = Render<LoginForm>(parameters => parameters
            .Add(form => form.Error, AuthenticationError.Unavailable));

        component.Markup.ShouldContain(
            AuthenticationError.Unavailable.GetDisplayName());
    }

    [Theory(DisplayName = "Login page binds query parameters and displays the selected mode title")]
    [InlineData("register", "Register - " + Branding.GameName)]
    [InlineData("confirmation", "Confirm email - " + Branding.GameName)]
    [InlineData("recover", "Recover access - " + Branding.GameName)]
    [InlineData("", "Sign in - " + Branding.GameName)]
    [InlineData("unknown", "Sign in - " + Branding.GameName)]
    public void LoginPage_Should_BindQueryAndDisplayTitle_When_ModeIsProvided(
        string mode,
        string expectedTitle)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            $"/login?returnUrl=%2Fusers&error=unavailable&mode={mode}");
        var head = Render<HeadOutlet>();

        var component = Render<LoginPageComponent>();

        component.Instance.ReturnUrl.ShouldBe("/users");
        component.Instance.Error.ShouldBe("unavailable");
        component.Instance.Mode.ShouldBe(mode);
        head.Find("title").TextContent.ShouldBe(expectedTitle);
        component.Find(".auth-brand-title").GetAttribute("aria-label")
            .ShouldBe(Branding.GameName);
        string.Join(" ", component.FindAll(".auth-brand-title span")
            .Select(span => span.TextContent)).ShouldBe(Branding.GameName);
    }

    [Fact(DisplayName = "Forgot password form validates an empty model through MudForm")]
    public void ForgotPassword_Should_DisplayValidationError_When_ModelIsEmpty()
    {
        var component = Render<ForgotPasswordForm>();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email.");
        });
    }

    [Fact(DisplayName = "Forgot password form submits a valid MudForm model")]
    public void ForgotPassword_Should_Submit_When_ModelIsValid()
    {
        var component = Render<ForgotPasswordForm>();
        component.Find("#recovery-email").Input("admin@example.com");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("Если подтверждённый аккаунт существует");
        });
    }

    [Fact(DisplayName = "Confirmation form validates an empty model through MudForm")]
    public void Confirmation_Should_DisplayValidationError_When_ModelIsEmpty()
    {
        var component = Render<ResendConfirmationForm>();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email.");
        });
    }

    [Fact(DisplayName = "Confirmation form submits a valid MudForm model")]
    public void Confirmation_Should_Submit_When_ModelIsValid()
    {
        var component = Render<ResendConfirmationForm>();
        component.Find("#confirmation-email").Input("admin@example.com");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("новое письмо уже отправлено");
        });
    }

    [Fact(DisplayName = "Reset password form validates an empty model through MudForm")]
    public void ResetPassword_Should_DisplayValidationErrors_When_ModelIsEmpty()
    {
        var component = RenderResetPasswordPage();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите новый пароль.");
            component.Markup.ShouldContain("Повторите новый пароль.");
        });
    }

    [Fact(DisplayName = "Reset password form submits a valid MudForm model")]
    public void ResetPassword_Should_Submit_When_ModelIsValid()
    {
        var component = RenderResetPasswordPage();
        component.Find("#reset-password").Input("Password1!");
        component.Find("#reset-password-confirmation").Input("Password1!");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("Пароль изменён");
        });
    }

    [Theory(DisplayName = "Email confirmation runs once after prerender and displays the API result")]
    [InlineData(HttpStatusCode.NoContent, "Email подтверждён")]
    [InlineData(HttpStatusCode.BadRequest, "Не удалось подтвердить")]
    public void ConfirmEmail_Should_DisplayResultOnce_When_PageBecomesInteractive(
        HttpStatusCode statusCode,
        string expectedHeading)
    {
        Guid userId = Guid.NewGuid();
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.ConfirmEmailPage(userId, "token/+=="));
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        var prerendered = Render<ConfirmEmailPageComponent>();
        prerendered.Find("h1").TextContent.ShouldBe("Подтверждаем email");
        _handler.PostCount.ShouldBe(0);
        prerendered.Dispose();
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        var component = Render<ConfirmEmailPageComponent>();

        component.Instance.UserId.ShouldBe(userId);
        component.Instance.EmailConfirmationToken.ShouldBe("token/+==");
        component.Find("h1").TextContent.ShouldBe("Подтверждаем email");
        component.Render();
        _handler.PostCount.ShouldBe(1);
        _handler.ConfirmationResponse.SetResult(new HttpResponseMessage(statusCode));
        component.WaitForAssertion(() =>
            component.Find("h1").TextContent.ShouldBe(expectedHeading));
        component.Render();
        _handler.PostCount.ShouldBe(1);
    }

    [Theory(DisplayName = "Email confirmation does not submit an incomplete link")]
    [InlineData("")]
    [InlineData("?userId=19641d4e-0c67-4892-a952-7eb71725a064")]
    [InlineData("?emailConfirmationToken=token")]
    [InlineData("?userId=19641d4e-0c67-4892-a952-7eb71725a064&emailConfirmationToken=%20")]
    public void ConfirmEmail_Should_RejectLink_When_QueryParametersAreMissing(string query)
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.ConfirmEmail + query);

        var component = Render<ConfirmEmailPageComponent>();

        component.Find("h1").TextContent.ShouldBe("Ссылка недействительна");
        _handler.PostCount.ShouldBe(0);
    }

    private IRenderedComponent<ResetPasswordPageComponent> RenderResetPasswordPage()
    {
        return Render<ResetPasswordPageComponent>(parameters => parameters
            .Add(component => component.UserId, Guid.Parse(
                "19641d4e-0c67-4892-a952-7eb71725a064"))
            .Add(component => component.PasswordResetToken, "reset-token"));
    }

    private sealed class AuthenticationHandler : HttpMessageHandler
    {
        public int PostCount { get; private set; }

        public TaskCompletionSource<HttpResponseMessage> ConfirmationResponse { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Method.ShouldBe(HttpMethod.Post);
            PostCount++;

            if (request.RequestUri!.AbsolutePath.EndsWith(
                    "/confirm-email",
                    StringComparison.Ordinal))
            {
                return ConfirmationResponse.Task.WaitAsync(cancellationToken);
            }

            if (request.RequestUri!.AbsolutePath.EndsWith(
                    "/register",
                    StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"id":"87ae99d5-abfe-40f7-a2a0-84ebf16a24ab"}""",
                        Encoding.UTF8,
                        "application/json"),
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }
}
