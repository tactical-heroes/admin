using System.Reflection;
using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

internal sealed partial record PageConventionSource(string RelativePath, string Markup, string ProjectName, string TypeName)
{
    public static PageConventionSource[] Discover(params string[] patterns)
    {
        string root = RepositoryPaths.FindRoot();

        return [.. DiscoverPaths(Path.Combine(root, "src"), patterns)
            .Select(path =>
            {
                string relativePath = Path.GetRelativePath(root, path).Replace('\\', '/');
                string[] segments = relativePath.Split('/');
                int projectIndex = segments[1] == "Modules" ? 2 : 1;
                string projectName = segments[projectIndex];
                string typeName = string.Join('.', segments.Skip(projectIndex))[..^".razor".Length];

                return new PageConventionSource(relativePath, WithoutComments(File.ReadAllText(path)), projectName, typeName);
            })];
    }

    internal static string[] DiscoverPaths(string sourceRoot, params string[] patterns)
    {
        return [.. patterns.SelectMany(pattern => Directory.EnumerateFiles(sourceRoot, pattern, SearchOption.AllDirectories))
            .Where(path =>
            {
                string[] segments = Path.GetRelativePath(sourceRoot, path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return segments.Contains("Pages", StringComparer.Ordinal) &&
                    !segments.Contains("obj", StringComparer.OrdinalIgnoreCase) &&
                    !segments.Contains("bin", StringComparer.OrdinalIgnoreCase);
            })
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
    }

    public bool Inherits(string sharedBaseName)
    {
        for (Type? type = Assembly.Load(ProjectName).GetType(TypeName, throwOnError: true)?.BaseType;
             type is not null;
             type = type.BaseType)
        {
            if (type.FullName?.Split('`')[0] == "TacticalHeroes.Admin.Shared.Ui." + sharedBaseName)
            {
                return true;
            }
        }

        return false;
    }

    public static string WithoutComments(string markup)
    {
        return CommentRegex().Replace(markup, "");
    }

    public static Match Element(string markup, string name)
    {
        // ponytail: inspect conventional, non-nested shared tags; use the Razor syntax tree if composition needs nesting of the same tag.
        string tag = Regex.Escape(name);
        return Regex.Match(markup,
            $"<{tag}(?=[\\s/>])(?<attributes>(?:[^>\"']|\"[^\"]*\"|'[^']*')*?)(?:/\\s*>|>(?<content>.*?)</{tag}\\s*>)",
            RegexOptions.CultureInvariant | RegexOptions.Singleline);
    }

    public static string? Attribute(Match element, string name)
    {
        Match? match = AttributeRegex().Matches(element.Groups["attributes"].Value)
            .SingleOrDefault(attribute => attribute.Groups["name"].Value == name);
        return match?.Groups["value"].Value.Trim();
    }

    public static bool Binds(Match element, string attribute, string expression, bool explicitExpression = false)
    {
        string? value = Attribute(element, attribute);
        return (!explicitExpression && value == expression) || value == "@" + expression || value == "@(" + expression + ")";
    }

    public static bool HasHeader(string markup)
    {
        Match header = Element(markup, "PageHeader");
        return !string.IsNullOrWhiteSpace(Attribute(header, "Title")) &&
            !string.IsNullOrWhiteSpace(Attribute(header, "Subtitle"));
    }

    [GeneratedRegex(@"@\*.*?\*@|<!--.*?-->", RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex("(?<name>[@\\w:.-]+)\\s*=\\s*(?:\"(?<value>[^\"]*)\"|'(?<value>[^']*)')", RegexOptions.CultureInvariant)]
    private static partial Regex AttributeRegex();
}
