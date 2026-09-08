using System.Net;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using ConfirmEmailPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Ui.ConfirmEmailPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.ConfirmEmailPage.Ui;

public sealed class ConfirmEmailPageTests : AuthenticationComponentTestContext
{
    [Theory(DisplayName = "OnInitializedAsync should display result once when page becomes interactive")]
    [InlineData(HttpStatusCode.NoContent, "Email подтверждён")]
    [InlineData(HttpStatusCode.BadRequest, "Не удалось подтвердить")]
    public void OnInitializedAsync_Should_DisplayResultOnce_When_PageBecomesInteractive(
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

    [Theory(DisplayName = "OnInitializedAsync should reject link when query parameters are missing")]
    [InlineData("")]
    [InlineData("?userId=19641d4e-0c67-4892-a952-7eb71725a064")]
    [InlineData("?emailConfirmationToken=token")]
    [InlineData("?userId=19641d4e-0c67-4892-a952-7eb71725a064&emailConfirmationToken=%20")]
    public void OnInitializedAsync_Should_RejectLink_When_QueryParametersAreMissing(string query)
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.ConfirmEmail + query);

        var component = Render<ConfirmEmailPageComponent>();

        component.Find("h1").TextContent.ShouldBe("Ссылка недействительна");
        _handler.PostCount.ShouldBe(0);
    }
}
