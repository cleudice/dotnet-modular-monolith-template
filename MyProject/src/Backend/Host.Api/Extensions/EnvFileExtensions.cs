namespace Host.Api;

public static class EnvFileExtensions
{
    public static IConfigurationBuilder AddEnvFile(
        this IConfigurationBuilder builder,
        string path = ".env")
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);

        if (!File.Exists(fullPath))
        {
            return builder;
        }

        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadLines(fullPath))
        {
            var trimmed = line.Trim();

            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            // Remove "export " prefix if present (shell compatibility)
            if (trimmed.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[7..].TrimStart();
            }

            var eqIndex = trimmed.IndexOf('=');
            if (eqIndex <= 0)
            {
                continue;
            }

            var key = trimmed[..eqIndex].Trim();
            var value = trimmed[(eqIndex + 1)..].Trim();

            // Environment variable convention: __ maps to : in .NET configuration
            key = key.Replace("__", ":");

            // Remove surrounding quotes if present
            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            config[key] = value;
        }

        builder.AddInMemoryCollection(config!);
        return builder;
    }
}
