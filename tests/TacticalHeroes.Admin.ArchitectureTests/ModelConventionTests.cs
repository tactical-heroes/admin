using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class ModelConventionTests
{
    [Fact(DisplayName = "Model folders use property-based classes")]
    public void ModelSources_Should_UsePropertyBasedClasses_When_ModelFoldersAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");

        string[] violations =
        [
            .. EnumerateModelSourceFiles(sourceRoot)
                .Where(path => PositionalTypeDeclarationRegex().IsMatch(
                    File.ReadAllText(path)))
                .Select(path => Path.GetRelativePath(repositoryRoot, path)),
        ];

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Models use adjacent validators")]
    public void ModelSources_Should_HaveAdjacentValidators_When_ModelTypesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        string[] modelPaths =
        [
            .. EnumerateModelSourceFiles(sourceRoot)
                .Where(static path => Path
                    .GetFileNameWithoutExtension(path)
                    .EndsWith("Model", StringComparison.Ordinal)),
        ];
        List<string> violations = [];

        foreach (string modelPath in modelPaths)
        {
            string modelName = Path.GetFileNameWithoutExtension(modelPath);
            string validatorPath = Path.Combine(
                Path.GetDirectoryName(modelPath)!,
                $"{modelName}Validator.cs");

            if (!File.Exists(validatorPath))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, modelPath)}: " +
                    "adjacent validator is missing");
                continue;
            }

            string validatorSource = File.ReadAllText(validatorPath);
            string validatorPattern =
                $@"\bclass\s+{Regex.Escape(modelName)}Validator\b" +
                $@"[^{{]*:\s*MudFormValidator\s*<\s*{Regex.Escape(modelName)}\s*>";

            if (!Regex.IsMatch(
                    validatorSource,
                    validatorPattern,
                    RegexOptions.CultureInvariant))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, validatorPath)}: " +
                    $"validator must inherit MudFormValidator<{modelName}>");
            }
        }

        modelPaths.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [GeneratedRegex(
        @"\brecord\s+(?:(?:class|struct)\s+)?[A-Za-z_]|" +
        @"\bclass\s+[A-Za-z_][A-Za-z0-9_]*(?:<[^>{}\r\n]+>)?\s*\(",
        RegexOptions.CultureInvariant)]
    private static partial Regex PositionalTypeDeclarationRegex();

    private static IEnumerable<string> EnumerateModelSourceFiles(string sourceRoot)
    {
        return Directory
            .EnumerateDirectories(sourceRoot, "Model", SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
            .SelectMany(static path => Directory.EnumerateFiles(
                path,
                "*.cs",
                SearchOption.TopDirectoryOnly));
    }
}
