using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class FormPageConventionTests
{
    [Fact(DisplayName = "FormPages should inherit operation bases when admin forms are scanned")]
    public void FormPages_Should_InheritOperationBases_When_AdminFormsAreScanned()
    {
        PageConventionSource[] pages = DiscoverForms();
        string[] violations = [.. pages
            .Where(page => !page.Inherits(IsCreate(page)
                ? "Forms.MudCreateFormComponentBase"
                : "Forms.MudUpdateFormComponentBase"))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "FormPages should use shared composition when admin forms are scanned")]
    public void FormPages_Should_UseSharedComposition_When_AdminFormsAreScanned()
    {
        PageConventionSource[] pages = DiscoverForms();
        string[] violations = [.. pages
            .Where(page => !UsesSharedComposition(page.Markup))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "FormPages should bind validation and submission when admin forms are scanned")]
    public void FormPages_Should_BindValidationAndSubmission_When_AdminFormsAreScanned()
    {
        PageConventionSource[] pages = DiscoverForms();
        string[] violations = [.. pages
            .Where(page => !BindsFormState(page.Markup, IsCreate(page)))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "UpdatePages should bind loadable content when admin forms are scanned")]
    public void UpdatePages_Should_BindLoadableContent_When_AdminFormsAreScanned()
    {
        PageConventionSource[] pages = PageConventionSource.Discover("Update*Page.razor", "Edit*Page.razor");
        string[] violations = [.. pages
            .Where(page => !BindsLoadState(page.Markup))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Form bindings should reject disconnected state when markup is inspected")]
    [InlineData("Validation=\"@Validator.ValidateValue\"", "Validation=\"null\"")]
    [InlineData("@ref=\"Form\"", "@ref=\"OtherForm\"")]
    [InlineData("Model=\"Model\"", "Model=\"OtherModel\"")]
    [InlineData("Busy=\"IsSaving\"", "Busy=\"false\"")]
    [InlineData("OnSubmit=\"SubmitAsync\"", "OnSubmit=\"OtherSubmitAsync\"")]
    [InlineData("IsNew=\"true\"", "IsNew=\"false\"")]
    [InlineData("CancelHref=\"@ExampleRoutes.Examples\"", "CancelHref=\"ExampleRoutes.Examples\"")]
    public void FormBindings_Should_RejectDisconnectedState_When_MarkupIsInspected(string original, string replacement)
    {
        const string markup = """
            <MudForm @ref="Form" Model="Model" Validation="@Validator.ValidateValue" @bind-IsValid="IsValid">
                <EditFormActions CancelHref="@ExampleRoutes.Examples" IsNew="true" Busy="IsSaving" OnSubmit="SubmitAsync" />
            </MudForm>
            """;
        string broken = markup.Replace(original, replacement, StringComparison.Ordinal);

        BindsFormState(markup, isCreate: true).ShouldBeTrue();
        BindsFormState(broken, isCreate: true).ShouldBeFalse();
    }

    [Theory(DisplayName = "Update bindings should reject disconnected loading when markup is inspected")]
    [InlineData("Loading=\"IsLoading\"", "Loading=\"false\"")]
    [InlineData("LoadError=\"@LoadError\"", "LoadError=\"LoadError\"")]
    [InlineData("OnRetry=\"ReloadAsync\"", "OnRetry=\"OtherReloadAsync\"")]
    [InlineData("<MudForm />", "")]
    public void UpdateBindings_Should_RejectDisconnectedLoading_When_MarkupIsInspected(string original, string replacement)
    {
        const string markup = """
            <LoadableContent Loading="IsLoading" LoadError="@LoadError" OnRetry="ReloadAsync">
                <ChildContent><MudForm /></ChildContent>
            </LoadableContent>
            """;
        string broken = markup.Replace(original, replacement, StringComparison.Ordinal);

        BindsLoadState(markup).ShouldBeTrue();
        BindsLoadState(broken).ShouldBeFalse();
    }

    [Theory(DisplayName = "Update conventions should protect the validated form when multiple forms are present")]
    [InlineData("only", false, true)]
    [InlineData("before", false, false)]
    [InlineData("before", true, false)]
    [InlineData("after", false, false)]
    [InlineData("after", true, false)]
    [InlineData("inside", false, false)]
    [InlineData("inside", true, false)]
    [InlineData("nested", false, false)]
    [InlineData("nested", true, false)]
    public void UpdateConventions_Should_ProtectTheValidatedForm_When_MultipleFormsArePresent(
        string placement,
        bool duplicateForm,
        bool expected)
    {
        const string header = """
            <PageHeader Title="Example" Subtitle="Edit example">
                <Actions><PageBackButton Href="@ExampleRoutes.Examples" /></Actions>
            </PageHeader>
            """;
        const string form = """
            <MudForm @ref="Form" Model="Model" Validation="@Validator.ValidateValue" @bind-IsValid="IsValid">
                <EditSection Title="Details" />
                <EditFormActions CancelHref="@ExampleRoutes.Examples" Busy="IsSaving" OnSubmit="SubmitAsync" />
            </MudForm>
            """;
        string extraForm = duplicateForm ? form : "<MudForm />";
        string content = placement switch
        {
            "before" => extraForm,
            "inside" => form + extraForm,
            "nested" => form.Replace("</MudForm>", extraForm + "</MudForm>", StringComparison.Ordinal),
            _ => form
        };
        string markup = header + (placement == "before" ? form : "") +
            "<LoadableContent Loading=\"IsLoading\" LoadError=\"@LoadError\" OnRetry=\"ReloadAsync\">" +
            "<ChildContent>" + content + "</ChildContent></LoadableContent>" +
            (placement == "after" ? extraForm : "");

        bool valid = UsesSharedComposition(markup) && BindsFormState(markup, isCreate: false) && BindsLoadState(markup);

        valid.ShouldBe(expected);
    }

    private static PageConventionSource[] DiscoverForms()
    {
        return PageConventionSource.Discover("Create*Page.razor", "Update*Page.razor", "Edit*Page.razor");
    }

    private static bool IsCreate(PageConventionSource page)
    {
        return Path.GetFileName(page.RelativePath).StartsWith("Create", StringComparison.Ordinal);
    }

    private static bool UsesSharedComposition(string markup)
    {
        Match header = PageConventionSource.Element(markup, "PageHeader");
        Match actions = PageConventionSource.Element(header.Groups["content"].Value, "Actions");
        Match back = PageConventionSource.Element(actions.Groups["content"].Value, "PageBackButton");
        Match form = PageConventionSource.Element(markup, "MudForm");
        Match section = PageConventionSource.Element(form.Groups["content"].Value, "EditSection");
        Match submit = PageConventionSource.Element(form.Groups["content"].Value, "EditFormActions");
        string? backHref = PageConventionSource.Attribute(back, "Href");

        // Search after the first opening '<' so nested forms are counted as well.
        bool hasSingleForm = form.Success &&
            !PageConventionSource.Element(markup[(form.Index + 1)..], "MudForm").Success;

        return hasSingleForm && PageConventionSource.HasHeader(markup) && section.Success && submit.Success &&
            backHref?.StartsWith('@') == true && backHref == PageConventionSource.Attribute(submit, "CancelHref");
    }

    private static bool BindsFormState(string markup, bool isCreate)
    {
        Match form = PageConventionSource.Element(markup, "MudForm");
        Match actions = PageConventionSource.Element(form.Groups["content"].Value, "EditFormActions");

        return PageConventionSource.Binds(form, "@ref", "Form") &&
            PageConventionSource.Binds(form, "Model", "Model") &&
            PageConventionSource.Binds(form, "Validation", "Validator.ValidateValue") &&
            PageConventionSource.Binds(form, "@bind-IsValid", "IsValid") &&
            PageConventionSource.Binds(actions, "Busy", "IsSaving") &&
            PageConventionSource.Binds(actions, "OnSubmit", "SubmitAsync") &&
            PageConventionSource.Attribute(actions, "CancelHref") is { Length: > 1 } cancelHref && cancelHref.StartsWith('@') &&
            (PageConventionSource.Binds(actions, "IsNew", isCreate ? "true" : "false") ||
             (!isCreate && PageConventionSource.Attribute(actions, "IsNew") is null));
    }

    private static bool BindsLoadState(string markup)
    {
        Match loading = PageConventionSource.Element(markup, "LoadableContent");
        Match form = PageConventionSource.Element(markup, "MudForm");
        Group content = loading.Groups["content"];

        return PageConventionSource.Binds(loading, "Loading", "IsLoading") &&
            PageConventionSource.Binds(loading, "LoadError", "LoadError", explicitExpression: true) &&
            PageConventionSource.Binds(loading, "OnRetry", "ReloadAsync") &&
            form.Success && form.Index >= content.Index && form.Index + form.Length <= content.Index + content.Length;
    }
}
