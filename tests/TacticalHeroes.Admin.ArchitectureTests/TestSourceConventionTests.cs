using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        List<string> violations = [];

        foreach (TestProjectSource project in TestSourceDiscovery.GetProjects())
        {
            foreach (SyntaxTree tree in project.SourceTrees)
            {
                string path = Path.GetFullPath(tree.FilePath, repositoryRoot);
                string relativeDirectory = Path.GetRelativePath(project.Directory, Path.GetDirectoryName(path)!);
                string expectedNamespace = relativeDirectory == "."
                    ? project.RootNamespace
                    : project.RootNamespace + "." + relativeDirectory.Replace(Path.DirectorySeparatorChar, '.');
                violations.AddRange(GetNamespaceViolations(tree, expectedNamespace));
            }
        }

        violations.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Namespace validation should reject misplaced types when source namespaces are inspected")]
    [InlineData("class ExampleTests { }", false)]
    [InlineData("namespace Expected; class ExampleTests { }", true)]
    [InlineData("namespace Wrong; class ExampleTests { }", false)]
    [InlineData("namespace Expected { class ExampleTests { } } class GlobalTests { }", false)]
    [InlineData("global using Xunit;", true)]
    public void NamespaceValidation_Should_RejectMisplacedTypes_When_SourceNamespacesAreInspected(string source, bool valid)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);

        string[] violations = GetNamespaceViolations(tree, "Expected");

        (violations.Length == 0).ShouldBe(valid);
    }

#if NET10_0 && NET10_0_OR_GREATER
    [Fact(DisplayName = "Test source discovery should discover active tests when framework symbols are defined")]
    public void TestSourceDiscovery_Should_DiscoverActiveTests_When_FrameworkSymbolsAreDefined()
    {
        TestMethodSource[] methods = TestSourceDiscovery.GetTestMethods();

        methods.ShouldContain(method => method.Name ==
            nameof(TestSourceDiscovery_Should_DiscoverActiveTests_When_FrameworkSymbolsAreDefined));
        methods.ShouldNotContain(method => method.Name == "InactiveFrameworkTest");
    }
#else
    [Fact]
    public void InactiveFrameworkTest() { }
#endif

    private static string[] GetNamespaceViolations(SyntaxTree tree, string expectedNamespace)
    {
        return [.. tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
            .Where(node => node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
            .Select(node => string.Join('.', node.Ancestors().OfType<BaseNamespaceDeclarationSyntax>()
                .Reverse().Select(declaration => declaration.Name.ToString())))
            .Distinct(StringComparer.Ordinal)
            .Where(actualNamespace => actualNamespace != expectedNamespace)
            .Select(actualNamespace => $"{tree.FilePath}: '{actualNamespace}' must be '{expectedNamespace}'.")];
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
    [InlineData("var value = 1;\n\nCheck();", false)]
    [InlineData("var value = 1;\n\nShouldRefresh();", false)]
    [InlineData("var value = 1;\n\nAssertRefresh();", false)]
    [InlineData("var value = 1;\n\nHelpers.ShouldBe(value);", false)]
    [InlineData("var value = 1;\n\nHelpers.MarkupMatches(\"text\");", false)]
    [InlineData("var value = 1;\n\nHelpers.WaitForAssertion(() => Assert.True(false));", false)]
    [InlineData("var value = 1;\n\nglobal::Xunit.Assert.Equal(1, value);", true)]
    [InlineData("var value = 1;\n\nVerify.Equal(1, value);", true)]
    [InlineData("var value = 1;\n\nEqual(1, value);", true)]
    public void ArrangeActAssert_Should_RecognizeFinalAssertions_When_TestBodiesAreInspected(string body, bool valid)
    {
        string source = """
            using System;
            using System.Threading.Tasks;
            using Bunit;
            using Shouldly;
            using Verify = Xunit.Assert;
            using static Xunit.Assert;
            class ExampleTests
            {
                static IRenderedComponent<Microsoft.AspNetCore.Components.ComponentBase> Render() => null!;
                static Func<Task> CreateAction() => () => Task.CompletedTask;
                static void Log(object value) { }
                static void Register(Action action) { }
                static void Check() { }
                static void ShouldRefresh() { }
                static void AssertRefresh() { }
                [Fact] public async Task Example() {
            """ + body + """
                }
            }
            static class Helpers
            {
                public static void ShouldBe(int value) { }
                public static void MarkupMatches(string markup) { }
                public static void WaitForAssertion(Action assertion) { }
            }
            """;
        TestMethodSource method = TestSourceDiscovery.GetTestMethods("tests/ExampleTests.cs", source).Single();

        string? violation = GetArrangeActAssertViolation(method);

        method.SemanticModel.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken)
            .Where(diagnostic => diagnostic.DefaultSeverity == DiagnosticSeverity.Error).ShouldBeEmpty();
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

        if (!sections[^1].Any(statement => ContainsAssertion(statement, testMethod.SemanticModel)))
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

    private static bool ContainsAssertion(SyntaxNode node, SemanticModel semanticModel)
    {
        return node
            .DescendantNodesAndSelf(child => child is not AnonymousFunctionExpressionSyntax and not LocalFunctionStatementSyntax)
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation => IsAssertion(invocation, semanticModel) ||
                IsBunitMethod(invocation, semanticModel, "RenderedComponentWaitForHelperExtensions", "WaitForAssertion") &&
                invocation.ArgumentList.Arguments.Any(argument => argument.Expression switch
                {
                    LambdaExpressionSyntax lambda => ContainsAssertion(lambda.Body, semanticModel),
                    AnonymousMethodExpressionSyntax anonymous => ContainsAssertion(anonymous.Block, semanticModel),
                    _ => false
                }));
    }

    private static bool IsAssertion(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return false;
        }

        return method.ContainingAssembly.Name == typeof(Assert).Assembly.GetName().Name &&
               method.ContainingType.ToDisplayString() == "Xunit.Assert" ||
               method.ContainingAssembly.Name == typeof(Shouldly.Should).Assembly.GetName().Name &&
               method.ContainingNamespace.ToDisplayString() == "Shouldly" &&
               (method.Name.StartsWith("Should", StringComparison.Ordinal) || method.ContainingType.Name == "Should") ||
               IsBunitMethod(invocation, semanticModel, "MarkupMatchesAssertExtensions", "MarkupMatches");
    }

    private static bool IsBunitMethod(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        string typeName,
        string methodName)
    {
        return semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method &&
            method.ContainingAssembly.Name == "bunit" && method.ContainingNamespace.ToDisplayString() == "Bunit" &&
            method.ContainingType.Name == typeName && method.Name == methodName;
    }
}

internal static class TestSourceDiscovery
{
    private const string TestsDirectoryName = "tests";
    private static readonly SyntaxTree XunitUsing = CSharpSyntaxTree.ParseText("global using Xunit;");
    private static readonly ConcurrentDictionary<string, PortableExecutableReference> References = new(StringComparer.Ordinal);
    private static readonly Lazy<TestProjectSource[]> Projects = new(() =>
        [.. Directory.EnumerateFiles(Path.Combine(RepositoryPaths.FindRoot(), TestsDirectoryName), "*.csproj", SearchOption.AllDirectories)
            .OrderBy(project => project, StringComparer.Ordinal)
            .Select(LoadProject)]);
    private static readonly Lazy<TestMethodSource[]> Methods = new(() =>
        [.. GetProjects().SelectMany(project => GetTestMethods(project.Compilation, project.SourceTrees))]);

    public static TestMethodSource[] GetTestMethods()
    {
        return Methods.Value;
    }

    internal static TestProjectSource[] GetProjects()
    {
        return Projects.Value;
    }

    private static TestProjectSource LoadProject(string projectPath)
    {
        string projectDirectory = Path.GetDirectoryName(projectPath)!;
        string configuration = typeof(TestSourceDiscovery).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = RepositoryPaths.FindRoot(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        string[] arguments =
        [
            "msbuild", projectPath, "-nologo", "-target:Compile",
            "-property:DesignTimeBuild=true", "-property:BuildProjectReferences=false",
            "-property:SkipCompilerExecution=true", "-property:ProvideCommandLineArgs=true",
            // Force argument collection even when the existing compilation outputs are up to date.
            $"-property:NonExistentFile={Guid.NewGuid():N}",
            $"-property:Configuration={configuration}", "-getProperty:RootNamespace", "-getItem:CscCommandLineArgs"
        ];
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(60_000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"MSBuild source discovery timed out for {projectPath}.");
        }

        string standardOutput = output.GetAwaiter().GetResult();
        string standardError = error.GetAwaiter().GetResult();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"MSBuild source discovery failed for {projectPath}:\n{standardOutput}\n{standardError}");
        }

        using JsonDocument metadata = JsonDocument.Parse(standardOutput);
        string[] compilerArguments = [.. metadata.RootElement.GetProperty("Items").GetProperty("CscCommandLineArgs")
            .EnumerateArray().Select(item => item.GetProperty("Identity").GetString()!)];
        if (compilerArguments.Length == 0)
        {
            throw new InvalidOperationException($"MSBuild returned no compiler arguments for {projectPath}.");
        }

        CSharpCommandLineArguments options = CSharpCommandLineParser.Default.Parse(compilerArguments, projectDirectory, sdkDirectory: null);
        SyntaxTree[] trees = [.. options.SourceFiles.Select(source => CSharpSyntaxTree.ParseText(
            File.ReadAllText(source.Path), options.ParseOptions,
            path: Path.GetRelativePath(RepositoryPaths.FindRoot(), source.Path)))];
        var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(projectPath), trees,
            options.MetadataReferences.Select(reference => References.GetOrAdd(reference.Reference,
                path => MetadataReference.CreateFromFile(path)).WithProperties(reference.Properties)),
            options.CompilationOptions);

        return new TestProjectSource(projectDirectory,
            metadata.RootElement.GetProperty("Properties").GetProperty("RootNamespace").GetString()!,
            compilation, [.. trees.Where(tree => IsSourceFile(tree.FilePath))]);
    }

    internal static IEnumerable<TestMethodSource> GetTestMethods(string relativePath, string source)
    {
        return GetTestMethods([CSharpSyntaxTree.ParseText(source, path: relativePath)]);
    }

    internal static IEnumerable<TestMethodSource> GetTestMethods(SyntaxTree[] syntaxTrees)
    {
        var compilation = GetProjects().Single(project =>
                project.Compilation.AssemblyName == typeof(TestSourceDiscovery).Assembly.GetName().Name)
            .Compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(syntaxTrees.Append(XunitUsing));

        return GetTestMethods(compilation, syntaxTrees);
    }

    private static IEnumerable<TestMethodSource> GetTestMethods(CSharpCompilation compilation, SyntaxTree[] syntaxTrees)
    {
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
                    TestAttribute: item.TestAttribute!,
                    SemanticModel: semanticModel));
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

internal sealed record TestProjectSource(
    string Directory,
    string RootNamespace,
    CSharpCompilation Compilation,
    SyntaxTree[] SourceTrees);

internal sealed record TestMethodSource(
    string RelativePath,
    MethodDeclarationSyntax Declaration,
    AttributeSyntax TestAttribute,
    SemanticModel SemanticModel)
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
