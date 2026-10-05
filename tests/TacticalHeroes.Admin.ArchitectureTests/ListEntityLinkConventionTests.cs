using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ListEntityLinkConventionTests
{
    [Fact(DisplayName = "List pages should link related entity names when related identifiers are available")]
    public void ListPages_Should_LinkRelatedEntityNames_When_RelatedIdentifiersAreAvailable()
    {
        PageConventionSource[] pages = PageConventionSource.Discover("*ListPage.razor");
        string[] violations = [.. pages.SelectMany(page =>
            FindViolations(page.Markup, GetListItemType(page))
                .Select(name => $"{page.RelativePath}: display {name} through EntityLink with the matching related identifier"))];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Related columns should require a bound entity link when markup is inspected")]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' />", true)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)'></EntityLink>", true)]
    [InlineData("<EntityLink Text='@(context.Item.FactionName)' Href='@(Routes.Faction(id: context.Item.FactionId))' />", true)]
    [InlineData("<EntityLink Text='@((context.Item.FactionName))' Href='@(Routes.Faction((context.Item.FactionId)))' />", true)]
    [InlineData("<MudText>@context.Item.FactionName</MudText>", false)]
    [InlineData("<MudLink Href='@Routes.Faction(context.Item.FactionId)'>@context.Item.FactionName</MudLink>", false)]
    [InlineData("<EntityLink Text='FactionName' Href='@Routes.Faction(context.Item.FactionId)' />", false)]
    [InlineData("<EntityLink Text='context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' />", false)]
    [InlineData("<EntityLink Text='@context.Item.Name' Href='@Routes.Faction(context.Item.FactionId)' />", false)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.Id)' />", false)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(other.Item.FactionId)' />", false)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='/factions/1' />", false)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='Routes.Faction(context.Item.FactionId)' />", false)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(\"context.Item.FactionId\")' />", false)]
    [InlineData("@* <EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' /> *@<MudText>@context.Item.FactionName</MudText>", false)]
    [InlineData("<!-- <EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' /> --><MudText>@context.Item.FactionName</MudText>", false)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' /><MudText>@context.Item.FactionName</MudText>", false)]
    [InlineData("<EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' /><EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.Id)' />", false)]
    public void RelatedColumns_Should_RequireBoundEntityLink_When_MarkupIsInspected(string cell, bool expected)
    {
        string markup = "<EntityList><Columns><PropertyColumn Property='item => item.FactionName'>" +
            "<CellTemplate>" + cell + "</CellTemplate></PropertyColumn></Columns></EntityList>";

        FindViolations(markup, typeof(RelatedListItem)).ShouldBe(expected ? [] : ["FactionName"]);
    }

    [Theory(DisplayName = "Related columns should respect their own cell context when markup is inspected")]
    [InlineData("row", "row", true)]
    [InlineData("row", "context", false)]
    public void RelatedColumns_Should_RespectCellContext_When_MarkupIsInspected(string context, string binding, bool expected)
    {
        string markup = $"""
            <EntityList><Columns>
                <PropertyColumn Property="item => item.FactionName">
                    <CellTemplate Context="{context}">
                        <EntityLink Text="@{binding}.Item.FactionName" Href="@Routes.Faction({binding}.Item.FactionId)" />
                    </CellTemplate>
                </PropertyColumn>
            </Columns></EntityList>
            """;

        FindViolations(markup, typeof(RelatedListItem)).ShouldBe(expected ? [] : ["FactionName"]);
    }

    [Theory(DisplayName = "Related names should be checked in their own column when list columns are inspected")]
    [InlineData("<PropertyColumn Property='item => item.FactionName' />", false)]
    [InlineData("<PropertyColumn Property='@(item => item.FactionName)' />", false)]
    [InlineData("<PropertyColumn Property='(item) => item.FactionName' />", false)]
    [InlineData("<PropertyColumn Property='item => item.Name' /><PropertyColumn Property='item => item.StatusDisplayName' />", true)]
    [InlineData("<TemplateColumn><CellTemplate>@context.Item.FactionName</CellTemplate></TemplateColumn>", false)]
    [InlineData("<TemplateColumn><CellTemplate><EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' /></CellTemplate></TemplateColumn>", true)]
    [InlineData("<PropertyColumn Property='item => item.FactionName' /><TemplateColumn><CellTemplate><EntityLink Text='@context.Item.FactionName' Href='@Routes.Faction(context.Item.FactionId)' /></CellTemplate></TemplateColumn>", false)]
    public void RelatedNames_Should_BeCheckedInTheirOwnColumn_When_ListColumnsAreInspected(string columns, bool expected)
    {
        string markup = "<EntityList><Columns>" + columns + "</Columns></EntityList>";

        FindViolations(markup, typeof(RelatedListItem)).ShouldBe(expected ? [] : ["FactionName"]);
    }

    [Fact(DisplayName = "Related names should require an identifier pair when list item properties are inspected")]
    public void RelatedNames_Should_RequireIdentifierPair_When_ListItemPropertiesAreInspected()
    {
        string[] names = GetRelatedNames(typeof(RelatedListItem));

        names.ShouldBe(["FactionName"]);
    }

    private static Type GetListItemType(PageConventionSource page)
    {
        for (Type? type = Assembly.Load(page.ProjectName).GetType(page.TypeName, throwOnError: true);
             type is not null;
             type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().FullName?.Split('`')[0] ==
                "TacticalHeroes.Admin.Shared.Ui.Lists.MudPagedListComponentBase")
            {
                return type.GetGenericArguments()[0];
            }
        }

        throw new InvalidOperationException($"{page.RelativePath}: cannot discover the paged list item type");
    }

    private static string[] GetRelatedNames(Type itemType)
    {
        return [.. itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType == typeof(string) &&
                property.Name.Length > "Name".Length && property.Name.EndsWith("Name", StringComparison.Ordinal) &&
                itemType.GetProperty(property.Name[..^"Name".Length] + "Id", BindingFlags.Public | BindingFlags.Instance)
                    ?.PropertyType == typeof(Guid))
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)];
    }

    private static string[] FindViolations(string markup, Type itemType)
    {
        Match list = PageConventionSource.Element(PageConventionSource.WithoutComments(markup), "EntityList");
        string columns = PageConventionSource.Element(list.Groups["content"].Value, "Columns").Groups["content"].Value;
        string[] relatedNames = GetRelatedNames(itemType);
        List<string> violations = [];

        foreach (string tag in new[] { "PropertyColumn", "TemplateColumn" })
        {
            for (Match column = PageConventionSource.Element(columns, tag); column.Success; column = column.NextMatch())
            {
                Match cell = PageConventionSource.Element(column.Groups["content"].Value, "CellTemplate");
                string context = PageConventionSource.Attribute(cell, "Context") ?? "context";
                string content = cell.Groups["content"].Value;
                string? property = PageConventionSource.Attribute(column, "Property");

                foreach (string name in relatedNames)
                {
                    if ((SelectsProperty(property, name) || DisplaysRelatedName(content, context, name)) &&
                        !HasRelatedLink(content, context, name))
                    {
                        violations.Add(name);
                    }
                }
            }
        }

        return [.. violations.Distinct(StringComparer.Ordinal)];
    }

    private static bool SelectsProperty(string? value, string name)
    {
        ExpressionSyntax? expression = ParseExpression(value, requireBinding: false);
        string? parameter = expression switch
        {
            SimpleLambdaExpressionSyntax lambda => lambda.Parameter.Identifier.ValueText,
            ParenthesizedLambdaExpressionSyntax lambda when lambda.ParameterList.Parameters.Count == 1 =>
                lambda.ParameterList.Parameters[0].Identifier.ValueText,
            _ => null
        };

        return expression is LambdaExpressionSyntax { Body: MemberAccessExpressionSyntax member } &&
            member.Expression is IdentifierNameSyntax identifier &&
            identifier.Identifier.ValueText == parameter && member.Name.Identifier.ValueText == name;
    }

    private static bool HasRelatedLink(string content, string context, string name)
    {
        ExpressionSyntax text = SyntaxFactory.ParseExpression($"{context}.Item.{name}");
        ExpressionSyntax id = SyntaxFactory.ParseExpression($"{context}.Item.{name[..^"Name".Length]}Id");
        List<Match> relatedLinks = [];

        for (Match link = PageConventionSource.Element(content, "EntityLink"); link.Success; link = link.NextMatch())
        {
            if (!SyntaxFactory.AreEquivalent(ParseExpression(PageConventionSource.Attribute(link, "Text")), text))
            {
                continue;
            }

            if (ParseExpression(PageConventionSource.Attribute(link, "Href")) is not InvocationExpressionSyntax invocation ||
                !invocation.ArgumentList.Arguments.Any(argument => SyntaxFactory.AreEquivalent(Unwrap(argument.Expression), id)))
            {
                return false;
            }

            relatedLinks.Add(link);
        }

        string remaining = content;
        foreach (Match link in relatedLinks.AsEnumerable().Reverse())
        {
            remaining = remaining.Remove(link.Index, link.Length);
        }

        return relatedLinks.Count > 0 && !DisplaysRelatedName(remaining, context, name);
    }

    private static bool DisplaysRelatedName(string content, string context, string name)
    {
        return Regex.IsMatch(content, $@"@(?:\(\s*)*{Regex.Escape(context)}\s*\.\s*Item\s*\.\s*{Regex.Escape(name)}\b",
            RegexOptions.CultureInvariant);
    }

    private static ExpressionSyntax? ParseExpression(string? value, bool requireBinding = true)
    {
        if (string.IsNullOrWhiteSpace(value) || (requireBinding && !value.StartsWith('@')))
        {
            return null;
        }

        ExpressionSyntax expression = SyntaxFactory.ParseExpression(value.StartsWith('@') ? value[1..] : value);
        return expression.ContainsDiagnostics ? null : Unwrap(expression);
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }

    private sealed class RelatedListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid FactionId { get; set; }
        public string FactionName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusDisplayName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }
}
