namespace AutoRent.Api.Extensions;

/// <summary>
/// Minimal .env loader for local development: finds the nearest .env file above the working
/// directory and copies its KEY=VALUE lines into environment variables that are not already set.
/// Real environment variables always win, so CI/containers are unaffected.
/// </summary>
public static class DotEnvFile
{
    public static void LoadNearest(string fileName = ".env")
    {
        var path = FindUpwards(Directory.GetCurrentDirectory(), fileName);
        if (path is null)
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"');
            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? FindUpwards(string startDirectory, string fileName)
    {
        for (var dir = new DirectoryInfo(startDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
