using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ComponentTestConventionTests
{
    [Fact(DisplayName = "Components should have mirrored tests when production components are discovered")]
    public void Components_Should_HaveMirroredTests_When_ProductionComponentsAreDiscovered()
    {
        ComponentTarget[] targets = GetTargets();
        TestMethodSource[] testMethods = TestSourceDiscovery.GetTestMethods();

        targets.ShouldNotBeEmpty();
        string[] violations = [.. targets
            .Where(target => !File.Exists(target.TestPath) || GetTestMethods(target, testMethods).Length == 0)
            .Select(target => $"{target.Component.FullName}: add Fact/Theory tests in {target.TestPath}")];

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ComponentTests should cover declared methods when components have behavior")]
    public void ComponentTests_Should_CoverDeclaredMethods_When_ComponentsHaveBehavior()
    {
        ComponentTarget[] targets = GetTargets();
        TestMethodSource[] testMethods = TestSourceDiscovery.GetTestMethods();

        string[] violations = [.. targets.SelectMany(target =>
            MissingMethods(GetBehaviorMethods(target.Component), GetTestMethods(target, testMethods))
                .Select(name => $"{target.Component.FullName}.{name}: add {name}_Should_*_When_* in {target.TestPath}"))];

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetBehaviorMethods should include only declared behavior when component has mixed members")]
    public void GetBehaviorMethods_Should_IncludeOnlyDeclaredBehavior_When_ComponentHasMixedMembers()
    {
        string[] methods = GetBehaviorMethods(typeof(DiscoveryComponent));

        methods.Order().ShouldBe(["OnInitialized", "Submit", "Submit"]);
    }

    [Fact(DisplayName = "GetBehaviorMethods should separate base behavior when component inherits methods")]
    public void GetBehaviorMethods_Should_SeparateBaseBehavior_When_ComponentInheritsMethods()
    {
        string[] baseMethods = GetBehaviorMethods(typeof(DiscoveryBase));
        string[] componentMethods = GetBehaviorMethods(typeof(DiscoveryComponent));

        baseMethods.Order().ShouldBe(["InheritedAction", "OnInitialized"]);
        componentMethods.ShouldNotContain("InheritedAction");
        componentMethods.ShouldContain("OnInitialized");
    }

    [Theory(DisplayName = "MissingMethods should report uncovered overloads when test names are insufficient")]
    [InlineData("Submit_Should_Save_When_Valid", 1)]
    [InlineData("SubmitAsync_Should_Save_When_Valid", 2)]
    [InlineData("", 2)]
    public void MissingMethods_Should_ReportUncoveredOverloads_When_TestNamesAreInsufficient(
        string testName,
        int missingCount)
    {
        string[] missing = MissingMethods(["Submit", "Submit"], [testName]);

        missing.Length.ShouldBe(missingCount);
    }

    [Theory(DisplayName = "GetTestMethods should require tests in the expected class and file when mirrored source is inspected")]
    [InlineData("", false)]
    [InlineData("// [Fact] public void TestCase() { }", false)]
    [InlineData("partial class ComponentTestConventionTests { }", false)]
    [InlineData("class ComponentTestConventionTests { public void TestCase() { } }", false)]
    [InlineData("class OtherTests { [Fact] public void TestCase() { } }", false)]
    [InlineData("class Wrapper { class ComponentTestConventionTests { [Fact] public void TestCase() { } } }", false)]
    [InlineData("namespace Other { class ComponentTestConventionTests { [Fact] public void TestCase() { } } }", false)]
    [InlineData("partial class ComponentTestConventionTests { [Fact] public void TestCase() { } }", true)]
    [InlineData("class ComponentTestConventionTests { [global::Xunit.Theory] public void TestCase() { } }", true)]
    [InlineData("using Test = Xunit.FactAttribute; class ComponentTestConventionTests { [Test] public void TestCase() { } }", true)]
    public void GetTestMethods_Should_RequireTestsInTheExpectedClassAndFile_When_MirroredSourceIsInspected(
        string source,
        bool containsTest)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.cs");
        const string testName = nameof(GetBehaviorMethods_Should_IncludeOnlyDeclaredBehavior_When_ComponentHasMixedMembers);

        try
        {
            File.WriteAllText(path, "namespace TacticalHeroes.Admin.ArchitectureTests {\n" +
                source.Replace("TestCase", testName, StringComparison.Ordinal) + "\n}");
            var target = new ComponentTarget(typeof(DiscoveryComponent), path, typeof(ComponentTestConventionTests));
            TestMethodSource[] declarations = [.. TestSourceDiscovery.GetTestMethods(path, File.ReadAllText(path))];
            string[] methods = GetTestMethods(target, declarations);

            methods.ShouldBe(containsTest ? [testName] : []);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory(DisplayName = "Runnable tests should exclude skip conditions when coverage eligibility is checked")]
    [InlineData(null, false, null, null, true)]
    [InlineData("Skipped", false, null, null, false)]
    [InlineData(null, true, null, null, false)]
    [InlineData(null, false, "Condition", null, false)]
    [InlineData(null, false, null, "Condition", false)]
    public void RunnableTests_Should_ExcludeSkipConditions_When_CoverageEligibilityIsChecked(
        string? skip,
        bool explicitlyRun,
        string? skipWhen,
        string? skipUnless,
        bool expected)
    {
        FactAttribute[] attributes =
        [
            new() { Skip = skip, Explicit = explicitlyRun, SkipWhen = skipWhen, SkipUnless = skipUnless },
            new TheoryAttribute { Skip = skip, Explicit = explicitlyRun, SkipWhen = skipWhen, SkipUnless = skipUnless }
        ];

        attributes.Select(IsUnconditionallyRunnable).ShouldAllBe(actual => actual == expected);
    }

    private static ComponentTarget[] GetTargets()
    {
        string root = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(root, "src");

        return [.. Directory.EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => Directory.EnumerateFiles(Path.GetDirectoryName(path)!, "*.razor", SearchOption.AllDirectories).Any())
            .SelectMany(projectPath =>
            {
                string projectName = Path.GetFileNameWithoutExtension(projectPath);
                string testProjectName = projectName + ".ComponentTests";
                string projectParent = Path.GetRelativePath(sourceRoot, Path.GetDirectoryName(Path.GetDirectoryName(projectPath))!);
                string testRoot = Path.Combine(root, "tests", projectParent, testProjectName);
                Assembly productionAssembly = Assembly.Load(projectName);
                Assembly testAssembly = Assembly.Load(testProjectName);

                return productionAssembly.GetTypes()
                    .Where(type => !type.IsNested && type.Name != "_Imports" && type.IsSubclassOf(typeof(ComponentBase)))
                    .Select(type =>
                    {
                        string relativeName = type.FullName![(projectName.Length + 1)..].Split('`')[0];
                        return new ComponentTarget(
                            type,
                            Path.Combine(testRoot, relativeName.Replace('.', Path.DirectorySeparatorChar) + "Tests.cs"),
                            testAssembly.GetType(testProjectName + "." + relativeName + "Tests"));
                    });
            })
            .OrderBy(target => target.Component.FullName, StringComparer.Ordinal)];
    }

    private static string[] GetTestMethods(ComponentTarget target, TestMethodSource[] testMethods)
    {
        if (!File.Exists(target.TestPath) ||
            target.TestType is not { IsPublic: true, IsAbstract: false, ContainsGenericParameters: false } testType)
        {
            return [];
        }

        string root = RepositoryPaths.FindRoot();
        string testPath = Path.GetFullPath(target.TestPath);
        var declaredTestNames = testMethods
            .Where(source => Path.GetFullPath(source.RelativePath, root) == testPath)
            .Where(source => source.Declaration.Parent is ClassDeclarationSyntax { Parent: BaseNamespaceDeclarationSyntax } declaration
                && declaration.Identifier.ValueText == testType.Name
                && declaration.TypeParameterList is null
                && string.Join('.', declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>()
                    .Reverse().Select(node => node.Name.ToString())) == testType.Namespace)
            .Select(source => source.Name);

        return testType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttributes<FactAttribute>().Any(IsUnconditionallyRunnable))
            .Select(method => method.Name)
            .Intersect(declaredTestNames, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsUnconditionallyRunnable(FactAttribute attribute)
    {
        return attribute.Skip is null && !attribute.Explicit &&
            attribute.SkipWhen is null && attribute.SkipUnless is null;
    }

    private static string[] GetBehaviorMethods(Type component)
    {
        return [.. component.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName && !method.IsAbstract && method.Name != "BuildRenderTree"
                && method.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            .Select(method => method.Name)];
    }

    private static string[] MissingMethods(string[] methods, string[] tests)
    {
        return [.. methods.GroupBy(name => name, StringComparer.Ordinal)
            .SelectMany(group => group.Skip(tests.Count(test => test.StartsWith(group.Key + "_Should_", StringComparison.Ordinal))))];
    }

    private sealed record ComponentTarget(Type Component, string TestPath, Type? TestType);

    private abstract class DiscoveryBase : ComponentBase
    {
        public void InheritedAction() { }

        protected override void OnInitialized() { }
    }

    private sealed class DiscoveryComponent : DiscoveryBase
    {
        public string Value { get; set; } = string.Empty;

        protected override void OnInitialized() { }

        private void Submit() { }

        private void Submit(string value) { }

        [CompilerGenerated]
        private void Generated() { }
    }
}
