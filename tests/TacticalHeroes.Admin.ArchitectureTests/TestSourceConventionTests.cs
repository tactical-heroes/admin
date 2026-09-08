using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class TestSourceConventionTests
{
    [GeneratedRegex(
        @"^[A-Z][A-Za-z0-9]*_Should_[A-Z][A-Za-z0-9]*" +
        @"_When_[A-Z][A-Za-z0-9]*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex TestMethodNameRegex();

    [GeneratedRegex(@"\r?\n[\t ]*\r?\n", RegexOptions.CultureInvariant)]
    private static partial Regex BlankLineRegex();

    [Fact(DisplayName = "Test methods should use MethodName Should Behavior When Condition naming when a test is declared")]
    public void TestMethods_Should_FollowNamingConvention_When_ATestIsDeclared()
    {
        var violations = TestSourceDiscovery.GetTestMethods()
            .Where(testMethod =>
                !TestMethodNameRegex().IsMatch(testMethod.Name))
            .Select(testMethod =>
                $"{testMethod.Location}: {testMethod.Name}")
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Test method names must match " +
            $"'MethodName_Should_DoSomething_When_Condition':" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Test methods should follow Arrange Act Assert structure when a test is declared")]
    public void TestMethods_Should_FollowArrangeActAssert_When_ATestIsDeclared()
    {
        var violations = TestSourceDiscovery.GetTestMethods()
            .Select(GetArrangeActAssertViolation)
            .OfType<string>()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Test methods must separate setup and execution from a final " +
            $"assertion section with a blank line:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Test namespaces should match project folders when test sources are scanned")]
    public void TestNamespaces_Should_MatchProjectFolders_When_TestSourcesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string testsRoot = Path.Combine(repositoryRoot, "tests");
        List<string> violations = [];

        foreach (string project in Directory.EnumerateFiles(testsRoot, "*.csproj", SearchOption.AllDirectories))
        {
            string projectDirectory = Path.GetDirectoryName(project)!;
            string rootNamespace = XDocument.Load(project).Descendants("RootNamespace")
                .Select(element => element.Value).SingleOrDefault() ?? Path.GetFileNameWithoutExtension(project);

            foreach (string path in TestSourceDiscovery.GetSourceFiles(projectDirectory))
            {
                string relativeDirectory = Path.GetRelativePath(projectDirectory, Path.GetDirectoryName(path)!);
                string expectedNamespace = relativeDirectory == "."
                    ? rootNamespace
                    : rootNamespace + "." + relativeDirectory.Replace(Path.DirectorySeparatorChar, '.');
                var root = CSharpSyntaxTree.ParseText(
                    File.ReadAllText(path),
                    cancellationToken: TestContext.Current.CancellationToken)
                    .GetRoot(TestContext.Current.CancellationToken);

                foreach (BaseNamespaceDeclarationSyntax declaration in root.DescendantNodes()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .Where(declaration => !declaration.Members.OfType<BaseNamespaceDeclarationSyntax>().Any()))
                {
                    string actualNamespace = string.Join('.', declaration.AncestorsAndSelf()
                        .OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(node => node.Name.ToString()));
                    if (actualNamespace != expectedNamespace)
                    {
                        violations.Add($"{Path.GetRelativePath(repositoryRoot, path)}: '{actualNamespace}' must be '{expectedNamespace}'.");
                    }
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Test source discovery should ignore comments and strings when test attributes are scanned")]
    public void TestSourceDiscovery_Should_IgnoreCommentsAndStrings_When_TestAttributesAreScanned()
    {
        const string source = """
            // [Fact] public void Comment() { }
            class ExampleTests
            {
                const string Text = "[Theory] public void StringContent() { }";
                [Xunit.FactAttribute(DisplayName = "A fact")]
                public void FactCase() { }
                [global::Xunit.Theory(DisplayName = "A theory")]
                public void TheoryCase(int value) { }
            }
            """;

        TestMethodSource[] methods = [.. TestSourceDiscovery.GetTestMethods("tests/ExampleTests.cs", source)];

        methods.Select(method => method.Name).ShouldBe(["FactCase", "TheoryCase"]);
        methods.Select(method => method.DisplayName).ShouldBe(["A fact", "A theory"]);
    }

    [Theory(DisplayName = "Test source discovery should resolve attribute aliases when using directives rename attributes")]
    [InlineData("using Test = Xunit.FactAttribute;", "Test", true)]
    [InlineData("using Test = global::Xunit.TheoryAttribute;", "Test", true)]
    [InlineData("using TestAttribute = Xunit.FactAttribute;", "Test", true)]
    [InlineData("using Tests = Xunit;", "Tests::Fact", true)]
    [InlineData("using Fact = System.ObsoleteAttribute;", "Fact", false)]
    public void TestSourceDiscovery_Should_ResolveAttributeAliases_When_UsingDirectivesRenameAttributes(
        string usingDirective,
        string attributeName,
        bool isTest)
    {
        string source = usingDirective + "\nclass ExampleTests { [" + attributeName + "] public void Example() { } }";

        TestMethodSource[] methods = [.. TestSourceDiscovery.GetTestMethods("tests/ExampleTests.cs", source)];

        methods.Length.ShouldBe(isTest ? 1 : 0);
    }

    [Fact(DisplayName = "Test source discovery should respect alias scopes when global and namespace aliases are declared")]
    public void TestSourceDiscovery_Should_RespectAliasScopes_When_GlobalAndNamespaceAliasesAreDeclared()
    {
        SyntaxTree[] sources =
        [
            CSharpSyntaxTree.ParseText("global using Test = Xunit.FactAttribute;", path: "Aliases.cs",
                cancellationToken: TestContext.Current.CancellationToken),
            CSharpSyntaxTree.ParseText("""
                namespace Example
                {
                    class ExampleTests
                    {
                        [Test(DisplayName = "A fact")]
                        public void FactCase() { }
                    }
                }
                namespace Other
                {
                    using Test = System.ObsoleteAttribute;
                    class OtherTests
                    {
                        [Test]
                        public void NotATest() { }
                    }
                }
                """, path: "ExampleTests.cs", cancellationToken: TestContext.Current.CancellationToken)
        ];

        TestMethodSource[] methods = [.. TestSourceDiscovery.GetTestMethods(sources)];

        methods.Select(method => method.Name).ShouldBe(["FactCase"]);
        methods.Single().RelativePath.ShouldBe("ExampleTests.cs");
        methods.Single().DisplayName.ShouldBe("A fact");
    }

    [Theory(DisplayName = "Arrange Act Assert validation should recognize final assertions when test bodies are inspected")]
    [InlineData("var value = 1;\n\nvalue.ShouldBe(1);", true)]
    [InlineData("var action = CreateAction();\n\nawait Should.ThrowAsync<Exception>(action);", true)]
    [InlineData("var component = Render();\n\ncomponent.MarkupMatches(\"<p />\");", true)]
    [InlineData("var component = Render();\n\ncomponent.WaitForAssertion(() => component.Markup.ShouldContain(\"text\"));", true)]
    [InlineData("var value = 1; value.ShouldBe(1);", false)]
    [InlineData("var value = 1;\n// Assert\nvalue.ShouldBe(1);", false)]
    [InlineData("var value = 1;\n\nLog(\"Assert.True(false)\");", false)]
    [InlineData("var value = 1;\n\n// value.ShouldBe(1);\nLog(value);", false)]
    [InlineData("var value = 1;\n\nAction verify = () => Assert.True(false);", false)]
    [InlineData("var value = 1;\n\nvoid Verify() { Assert.True(false); }", false)]
    [InlineData("var value = 1;\n\nAction verify = delegate { Assert.True(false); };", false)]
    [InlineData("var value = 1;\n\nRegister(() => Assert.True(false));", false)]
    [InlineData("var component = Render();\n\nAction verify = () => component.WaitForAssertion(() => Assert.True(false));", false)]
    [InlineData("var component = Render();\n\ncomponent.WaitForAssertion(() => { Action verify = () => Assert.True(false); });", false)]
    [InlineData("var component = Render();\n\ncomponent.WaitForAssertion(delegate { Assert.True(true); });", true)]
    public void ArrangeActAssert_Should_RecognizeFinalAssertions_When_TestBodiesAreInspected(string body, bool valid)
    {
        string source = "class ExampleTests { [Fact] public async Task Example() { " + body + " } }";
        TestMethodSource method = TestSourceDiscovery.GetTestMethods("tests/ExampleTests.cs", source).Single();

        string? violation = GetArrangeActAssertViolation(method);

        (violation is null).ShouldBe(valid);
    }

    private static string? GetArrangeActAssertViolation(
        TestMethodSource testMethod)
    {
        var body = testMethod.Declaration.Body;

        if (body is null)
        {
            return $"{testMethod.Location}: {testMethod.Name} must use a block body.";
        }

        var sections = GetSections(body);
        const int minimumSectionCount = 2;

        if (sections.Count < minimumSectionCount)
        {
            return $"{testMethod.Location}: {testMethod.Name} has " +
                $"{sections.Count} logical section(s), expected at least " +
                $"{minimumSectionCount}.";
        }

        if (!sections[^1].Any(ContainsAssertion))
        {
            return $"{testMethod.Location}: {testMethod.Name} must keep " +
                $"assertions in its final logical section.";
        }

        return null;
    }

    private static List<IReadOnlyList<StatementSyntax>> GetSections(
        BlockSyntax body)
    {
        if (body.Statements.Count == 0)
        {
            return [];
        }

        var sections = new List<IReadOnlyList<StatementSyntax>>();
        var sourceText = body.SyntaxTree.GetText();
        var currentSection = new List<StatementSyntax>
        {
            body.Statements[0]
        };

        for (var index = 1; index < body.Statements.Count; index++)
        {
            var previousStatement = body.Statements[index - 1];
            var currentStatement = body.Statements[index];
            var separator = sourceText.ToString(
                TextSpan.FromBounds(
                    previousStatement.Span.End,
                    currentStatement.SpanStart));

            if (BlankLineRegex().IsMatch(separator))
            {
                sections.Add(currentSection);
                currentSection = [];
            }

            currentSection.Add(currentStatement);
        }

        sections.Add(currentSection);

        return sections;
    }

    private static bool ContainsAssertion(SyntaxNode node)
    {
        // ponytail: follow inline WaitForAssertion callbacks; resolve helper calls only when the suite needs them.
        return node
            .DescendantNodesAndSelf(child => child is not AnonymousFunctionExpressionSyntax and not LocalFunctionStatementSyntax)
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation => IsAssertion(invocation) ||
                invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "WaitForAssertion" } &&
                invocation.ArgumentList.Arguments.Any(argument => argument.Expression switch
                {
                    LambdaExpressionSyntax lambda => ContainsAssertion(lambda.Body),
                    AnonymousMethodExpressionSyntax anonymous => ContainsAssertion(anonymous.Block),
                    _ => false
                }));
    }

    private static bool IsAssertion(InvocationExpressionSyntax invocation)
    {
        var name = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess =>
                memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier =>
                identifier.Identifier.ValueText,
            _ => string.Empty
        };

        var containingType = invocation.Expression is
            MemberAccessExpressionSyntax
        {
            Expression: IdentifierNameSyntax containingIdentifier
        }
            ? containingIdentifier.Identifier.ValueText
            : string.Empty;

        return name.Contains("Should", StringComparison.Ordinal) ||
               name.StartsWith("Assert", StringComparison.Ordinal) ||
               containingType is "Assert" or "Should" ||
               string.Equals(name, "MarkupMatches", StringComparison.Ordinal) ||
               string.Equals(name, "Check", StringComparison.Ordinal) ||
               string.Equals(name, "Received", StringComparison.Ordinal) ||
               string.Equals(name, "DidNotReceive", StringComparison.Ordinal);
    }
}

internal static class TestSourceDiscovery
{
    private const string TestsDirectoryName = "tests";
    private static readonly SyntaxTree XunitUsing = CSharpSyntaxTree.ParseText("global using Xunit;");
    private static readonly MetadataReference[] References =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(FactAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
    ];

    public static TestMethodSource[] GetTestMethods()
    {
        var repositoryRoot = RepositoryPaths.FindRoot();
        var testsRoot = Path.Combine(repositoryRoot, TestsDirectoryName);

        return
        [
            .. Directory.EnumerateFiles(testsRoot, "*.csproj", SearchOption.AllDirectories)
                .OrderBy(project => project, StringComparer.Ordinal)
                .SelectMany(project => GetTestMethods(
                [
                    .. GetSourceFiles(Path.GetDirectoryName(project)!)
                        .OrderBy(path => path, StringComparer.Ordinal)
                        .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path),
                            path: Path.GetRelativePath(repositoryRoot, path)))
                ]))
        ];
    }

    internal static IEnumerable<string> GetSourceFiles(string root)
    {
        return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).Where(IsSourceFile);
    }

    internal static IEnumerable<TestMethodSource> GetTestMethods(string relativePath, string source)
    {
        return GetTestMethods([CSharpSyntaxTree.ParseText(source, path: relativePath)]);
    }

    internal static IEnumerable<TestMethodSource> GetTestMethods(SyntaxTree[] syntaxTrees)
    {
        var compilation = CSharpCompilation.Create("TestSourceDiscovery", syntaxTrees.Append(XunitUsing), References);

        return syntaxTrees.SelectMany(syntaxTree =>
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            return syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(method => new
                {
                    Method = method,
                    TestAttribute = method.AttributeLists
                        .SelectMany(attributeList => attributeList.Attributes)
                        .FirstOrDefault(attribute => IsTestAttribute(attribute, semanticModel))
                })
                .Where(item => item.TestAttribute is not null)
                .Select(item => new TestMethodSource(
                    RelativePath: syntaxTree.FilePath,
                    Declaration: item.Method,
                    TestAttribute: item.TestAttribute!));
        });
    }

    private static bool IsTestAttribute(AttributeSyntax attribute, SemanticModel semanticModel)
    {
        return semanticModel.GetTypeInfo(attribute).Type is { Name: "FactAttribute" or "TheoryAttribute" } type &&
            type.ContainingNamespace.ToDisplayString() == "Xunit";
    }

    private static bool IsSourceFile(string path)
    {
        return !path
            .Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries)
            .Any(segment =>
                string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed record TestMethodSource(
    string RelativePath,
    MethodDeclarationSyntax Declaration,
    AttributeSyntax TestAttribute)
{
    public string Name => Declaration.Identifier.ValueText;

    public string? DisplayName
    {
        get
        {
            var expression = TestAttribute
                .ArgumentList?
                .Arguments
                .FirstOrDefault(argument =>
                    string.Equals(
                        argument.NameEquals?.Name.Identifier.ValueText,
                        "DisplayName",
                        StringComparison.Ordinal))
                ?.Expression;

            return expression is LiteralExpressionSyntax literalExpression &&
                   literalExpression.IsKind(SyntaxKind.StringLiteralExpression)
                ? literalExpression.Token.ValueText
                : null;
        }
    }

    public string Location
    {
        get
        {
            var lineNumber = Declaration
                .GetLocation()
                .GetLineSpan()
                .StartLinePosition
                .Line + 1;

            return $"{RelativePath}:{lineNumber}";
        }
    }
}
