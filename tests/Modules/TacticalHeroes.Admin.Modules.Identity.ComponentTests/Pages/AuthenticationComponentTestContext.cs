using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using ResetPasswordPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Ui.ResetPasswordPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages;

public abstract class AuthenticationComponentTestContext : BunitContext
{
    protected readonly AuthenticationHandler _handler = new();

    protected AuthenticationComponentTestContext()
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

    protected IRenderedComponent<ResetPasswordPageComponent> RenderResetPasswordPage()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.ResetPasswordPage(
                Guid.Parse("19641d4e-0c67-4892-a952-7eb71725a064"),
                "reset-token"));

        return Render<ResetPasswordPageComponent>();
    }

    protected sealed class AuthenticationHandler : HttpMessageHandler
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
