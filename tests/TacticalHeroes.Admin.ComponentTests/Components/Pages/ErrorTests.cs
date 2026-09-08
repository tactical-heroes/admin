using System.Diagnostics;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

using ErrorComponent = TacticalHeroes.Admin.Components.Pages.Error;

namespace TacticalHeroes.Admin.ComponentTests.Components.Pages;

public sealed class ErrorTests : BunitContext
{
    [Fact(DisplayName = "The error page displays the current activity identifier when one exists")]
    public void OnInitialized_Should_PreferActivityId_When_ActivityExists()
    {
        using var activity = new Activity("error-page").Start();
        var context = new DefaultHttpContext { TraceIdentifier = "request-id" };

        var component = Render<CascadingValue<HttpContext>>(parameters => parameters
            .Add(value => value.Value, context)
            .AddChildContent<ErrorComponent>());

        component.Find("code").TextContent.ShouldBe(activity.Id);
    }

    [Theory(DisplayName = "The error page uses the request identifier without an activity and hides an empty identifier")]
    [InlineData("request-id")]
    [InlineData("")]
    public void OnInitialized_Should_UseRequestId_When_ActivityIsUnavailable(string requestId)
    {
        Activity? previous = Activity.Current;
        Activity.Current = null;
        try
        {
            var context = new DefaultHttpContext { TraceIdentifier = requestId };

            var component = Render<CascadingValue<HttpContext>>(parameters => parameters
                .Add(value => value.Value, context)
                .AddChildContent<ErrorComponent>());

            component.FindAll("code").Select(element => element.TextContent)
                .ShouldBe(requestId.Length == 0 ? [] : [requestId]);
            component.Find("a").GetAttribute("href").ShouldBe("/");
        }
        finally
        {
            Activity.Current = previous;
        }
    }
}
