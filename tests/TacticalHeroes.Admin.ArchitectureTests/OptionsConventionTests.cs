using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class OptionsConventionTests
{
    private static readonly Regex OptionsTypeRegex = new(
        @"\b(?:public|internal)\s+(?<modifier>sealed|static)?\s*class\s+(?<name>[A-Za-z_]\w*Options)\b",
        RegexOptions.CultureInvariant);

    [Fact(DisplayName = "Configuration options use dedicated folders with adjacent validators")]
    public void ConfigurationOptions_Should_HaveValidators_When_OptionsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<OptionsType> optionsTypes = FindOptionsTypes(repositoryRoot);
        List<string> violations = [];

        foreach (OptionsType optionsType in optionsTypes)
        {
            if (!optionsType.IsSealed)
            {
                violations.Add($"{optionsType.RelativePath}: options class must be sealed");
            }

            string optionDirectory = Path.GetDirectoryName(optionsType.SourcePath)!;
            string? optionsDirectory = Path.GetDirectoryName(optionDirectory);
            if (optionsDirectory is null ||
                !Path.GetFileName(optionsDirectory).Equals("Options", StringComparison.Ordinal))
            {
                violations.Add(
                    $"{optionsType.RelativePath}: options class must be in a dedicated Options/<name> folder");
            }

            if (!Path.GetFileName(optionsType.SourcePath).Equals(
                    $"{optionsType.Name}.cs",
                    StringComparison.Ordinal))
            {
                violations.Add(
                    $"{optionsType.RelativePath}: file name must match the options type");
            }

            if (optionsTypes.Count(candidate => string.Equals(
                    Path.GetDirectoryName(candidate.SourcePath),
                    optionDirectory,
                    StringComparison.OrdinalIgnoreCase)) != 1)
            {
                violations.Add(
                    $"{optionsType.RelativePath}: each options folder must contain exactly one options class");
            }

            string validatorPath = Path.Combine(
                optionDirectory,
                $"{optionsType.Name}Validator.cs");
            if (!File.Exists(validatorPath))
            {
                violations.Add($"{optionsType.RelativePath}: adjacent validator is missing");
                continue;
            }

            string validatorSource = File.ReadAllText(validatorPath);
            if (!Regex.IsMatch(
                    validatorSource,
                    $@"IValidateOptions\s*<\s*{Regex.Escape(optionsType.Name)}\s*>",
                    RegexOptions.CultureInvariant))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, validatorPath)}: " +
                    $"validator must implement IValidateOptions<{optionsType.Name}>");
            }
        }

        optionsTypes.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Configuration options register validators and validate on start")]
    public void ConfigurationOptions_Should_ValidateOnStart_When_RegistrationsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<OptionsType> optionsTypes = FindOptionsTypes(repositoryRoot);
        string productionSource = string.Join(
            Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(repositoryRoot, "src"))
                .Select(File.ReadAllText));
        List<string> violations = [];

        foreach (OptionsType optionsType in optionsTypes)
        {
            string optionsName = Regex.Escape(optionsType.Name);
            string validatorRegistrationPattern =
                $@"AddSingleton\s*<\s*IValidateOptions\s*<\s*{optionsName}\s*>\s*,\s*" +
                $@"{optionsName}Validator\s*>\s*\(";
            string validateOnStartPattern =
                $@"AddOptions\s*<\s*{optionsName}\s*>\s*\(\s*\)" +
                @"(?:(?!;).)*?ValidateOnStart\s*\(\s*\)";

            if (!Regex.IsMatch(
                    productionSource,
                    validatorRegistrationPattern,
                    RegexOptions.CultureInvariant | RegexOptions.Singleline))
            {
                violations.Add($"{optionsType.Name}: IValidateOptions validator is not registered");
            }

            if (!Regex.IsMatch(
                    productionSource,
                    validateOnStartPattern,
                    RegexOptions.CultureInvariant | RegexOptions.Singleline))
            {
                violations.Add($"{optionsType.Name}: ValidateOnStart is not registered");
            }
        }

        optionsTypes.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    private static List<OptionsType> FindOptionsTypes(string repositoryRoot)
    {
        List<OptionsType> optionsTypes = [];

        foreach (string sourcePath in EnumerateSourceFiles(Path.Combine(repositoryRoot, "src")))
        {
            string source = File.ReadAllText(sourcePath);

            foreach (Match match in OptionsTypeRegex.Matches(source))
            {
                if (match.Groups["modifier"].Value.Equals(
                        "static",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                optionsTypes.Add(new OptionsType(
                    match.Groups["name"].Value,
                    sourcePath,
                    Path.GetRelativePath(repositoryRoot, sourcePath),
                    match.Groups["modifier"].Value.Equals(
                        "sealed",
                        StringComparison.Ordinal)));
            }
        }

        return optionsTypes;
    }

    private static IEnumerable<string> EnumerateSourceFiles(string root)
    {
        return Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase));
    }

    private sealed record OptionsType(
        string Name,
        string SourcePath,
        string RelativePath,
        bool IsSealed);
}
