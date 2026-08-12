using System.Text.Json;
using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ApiAdapterConventionTests
{
    private static readonly Regex TransportVariableRegex = new(
        @"\b(?<variable>[a-z][A-Za-z0-9_]*)\s*=\s*await\s+client\." +
        @"|\.Select\(\s*(?<variable>[a-z][A-Za-z0-9_]*)\s*=>" +
        @"|\(\s*Api[A-Za-z0-9_<>?]*\s+(?<variable>[a-z][A-Za-z0-9_]*)\s*\)",
        RegexOptions.CultureInvariant);

    [Fact(DisplayName = "API adapters use their OpenAPI tags for names and entity slices")]
    public void ApiAdapters_Should_UseOpenApiTag_When_NamedAndLocated()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(repositoryRoot, "openapi", "tactical-heroes.json")));
        HashSet<string> tags = document.RootElement.GetProperty("paths")
            .EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject())
            .Where(operation => operation.Value.TryGetProperty("tags", out _))
            .SelectMany(operation => operation.Value.GetProperty("tags").EnumerateArray())
            .Select(tag => tag.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        List<string> violations = [];

        foreach (string apiPath in Directory.EnumerateFiles(
                     modulesRoot,
                     "*Api.cs",
                     SearchOption.AllDirectories))
        {
            string apiName = Path.GetFileNameWithoutExtension(apiPath);
            string tag = apiName[..^"Api".Length];
            string source = File.ReadAllText(apiPath);
            string expectedDirectory = Path.Combine("Entities", tag, "Api");
            string relativeDirectory = Path.GetDirectoryName(
                Path.GetRelativePath(modulesRoot, apiPath))!;

            if (!tags.Contains(tag))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, apiPath)}: " +
                    $"'{tag}' is not an OpenAPI tag");
            }

            if (!Regex.IsMatch(
                    source,
                    $@"\bpublic\s+sealed\s+class\s+{Regex.Escape(apiName)}\b",
                    RegexOptions.CultureInvariant))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, apiPath)}: " +
                    $"does not declare '{apiName}'");
            }

            if (!relativeDirectory.EndsWith(
                    expectedDirectory,
                    StringComparison.Ordinal))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, apiPath)}: " +
                    $"must be located in '{expectedDirectory}'");
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "API adapters handle Kiota nullability according to OpenAPI responses")]
    public void ApiAdapters_Should_FollowOpenApiNullability_When_GeneratedResponsesAreMapped()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(repositoryRoot, "openapi", "tactical-heroes.json")));
        JsonElement root = document.RootElement;
        JsonElement schemas = root.GetProperty("components").GetProperty("schemas");
        List<string> violations = [];

        foreach (string apiPath in Directory.EnumerateFiles(
                     modulesRoot,
                     "*Api.cs",
                     SearchOption.AllDirectories))
        {
            string tag = Path.GetFileNameWithoutExtension(apiPath)[..^"Api".Length];
            IReadOnlyDictionary<string, bool> properties = GetResponseProperties(
                root.GetProperty("paths"),
                schemas,
                tag);
            string source = File.ReadAllText(apiPath);
            string variables = string.Join(
                '|',
                TransportVariableRegex.Matches(source)
                    .Select(match => match.Groups["variable"].Value)
                    .Distinct(StringComparer.Ordinal)
                    .Select(Regex.Escape));

            if (variables.Length == 0)
            {
                continue;
            }

            Regex propertyAccessRegex = new(
                $@"\b(?:{variables})!?\.(?<property>[A-Z][A-Za-z0-9_]*)" +
                @"(?<handling>!\.Value|!|\?(?=\s*\.)|\s*\?\?)?",
                RegexOptions.CultureInvariant);

            foreach (Match match in propertyAccessRegex.Matches(source))
            {
                string property = match.Groups["property"].Value;
                if (!properties.TryGetValue(property, out bool isRequiredNonNullable))
                {
                    continue;
                }

                string handling = match.Groups["handling"].Value.Trim();
                bool assertsNonNull = handling.StartsWith('!');
                bool handlesNull = handling.StartsWith('?');

                if ((isRequiredNonNullable && !assertsNonNull) ||
                    (!isRequiredNonNullable && !handlesNull))
                {
                    string expected = isRequiredNonNullable ? "!" : "? or ??";
                    violations.Add(
                        $"{Path.GetRelativePath(repositoryRoot, apiPath)}: " +
                        $"{property} uses '{handling}', expected '{expected}'");
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    private static IReadOnlyDictionary<string, bool> GetResponseProperties(
        JsonElement paths,
        JsonElement schemas,
        string tag)
    {
        Dictionary<string, bool> properties = new(StringComparer.Ordinal);
        HashSet<string> visitedSchemas = new(StringComparer.Ordinal);

        foreach (JsonProperty path in paths.EnumerateObject())
        {
            foreach (JsonProperty operation in path.Value.EnumerateObject())
            {
                if (!HasTag(operation.Value, tag) ||
                    !operation.Value.TryGetProperty("responses", out JsonElement responses))
                {
                    continue;
                }

                foreach (JsonProperty response in responses.EnumerateObject())
                {
                    if (!response.Name.StartsWith('2') ||
                        !response.Value.TryGetProperty("content", out JsonElement content))
                    {
                        continue;
                    }

                    foreach (JsonProperty mediaType in content.EnumerateObject())
                    {
                        if (mediaType.Value.TryGetProperty("schema", out JsonElement schema))
                        {
                            AddSchemaProperties(schema, schemas, visitedSchemas, properties);
                        }
                    }
                }
            }
        }

        return properties;
    }

    private static bool HasTag(JsonElement operation, string tag)
    {
        return operation.TryGetProperty("tags", out JsonElement tags) &&
               tags.EnumerateArray().Any(value => value.GetString() == tag);
    }

    private static void AddSchemaProperties(
        JsonElement schema,
        JsonElement schemas,
        HashSet<string> visitedSchemas,
        Dictionary<string, bool> properties)
    {
        if (schema.TryGetProperty("$ref", out JsonElement reference))
        {
            string schemaName = reference.GetString()!.Split('/')[^1];
            if (visitedSchemas.Add(schemaName))
            {
                AddSchemaProperties(
                    schemas.GetProperty(schemaName),
                    schemas,
                    visitedSchemas,
                    properties);
            }

            return;
        }

        HashSet<string> required = schema.TryGetProperty("required", out JsonElement requiredElement)
            ? requiredElement.EnumerateArray()
                .Select(value => value.GetString()!)
                .ToHashSet(StringComparer.Ordinal)
            : [];

        if (schema.TryGetProperty("properties", out JsonElement schemaProperties))
        {
            foreach (JsonProperty property in schemaProperties.EnumerateObject())
            {
                string propertyName = char.ToUpperInvariant(property.Name[0]) + property.Name[1..];
                bool isRequiredNonNullable =
                    required.Contains(property.Name) && !AllowsNull(property.Value);

                if (properties.TryGetValue(propertyName, out bool existing) &&
                    existing != isRequiredNonNullable)
                {
                    throw new InvalidOperationException(
                        $"OpenAPI property '{propertyName}' has mixed nullability for this API tag.");
                }

                properties[propertyName] = isRequiredNonNullable;
                AddSchemaProperties(property.Value, schemas, visitedSchemas, properties);
            }
        }

        if (schema.TryGetProperty("items", out JsonElement items))
        {
            AddSchemaProperties(items, schemas, visitedSchemas, properties);
        }
    }

    private static bool AllowsNull(JsonElement schema)
    {
        if (schema.TryGetProperty("nullable", out JsonElement nullable) && nullable.GetBoolean())
        {
            return true;
        }

        if (!schema.TryGetProperty("type", out JsonElement type))
        {
            return false;
        }

        return type.ValueKind == JsonValueKind.Array &&
               type.EnumerateArray().Any(value => value.GetString() == "null");
    }
}
