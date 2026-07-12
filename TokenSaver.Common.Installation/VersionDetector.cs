using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace TokenSaver.Common.Installation;

/// <summary>
/// Detects and manages TokenSaver version information
/// </summary>
public static class VersionDetector
{
    /// <summary>
    /// Get the current assembly version
    /// </summary>
    public static Version GetAssemblyVersion(Assembly? assembly = null)
    {
        if (assembly == null)
            assembly = Assembly.GetExecutingAssembly();

        var versionAttr = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
        if (versionAttr != null && Version.TryParse(versionAttr.Version, out var parsed))
            return parsed;

        return new Version(0, 0, 0, 0);
    }

    /// <summary>
    /// Get the installed service version
    /// </summary>
    public static Version? GetInstalledServiceVersion()
    {
        try
        {
            var exePath = ServicePathResolver.GetServiceExecutablePath();
            if (!File.Exists(exePath))
                return null;

            var fileInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);
            var version = fileInfo.FileVersion;
            if (!string.IsNullOrEmpty(version) && Version.TryParse(version, out var parsed))
                return parsed;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (System.IO.FileNotFoundException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Check if an update is available
    /// </summary>
    public static bool IsUpdateAvailable(Version current, Version? installed)
    {
        if (current == null)
            throw new ArgumentNullException(nameof(current));
        if (installed == null)
            return false;

        return current > installed;
    }

    /// <summary>
    /// Read version from version.json (Nerdbank.GitVersioning)
    /// </summary>
    public static Version? ReadVersionFromFile(string versionFilePath)
    {
        if (string.IsNullOrWhiteSpace(versionFilePath))
            throw new ArgumentException("Version file path cannot be null or empty", nameof(versionFilePath));

        if (!File.Exists(versionFilePath))
            return null;

        try
        {
            var json = File.ReadAllText(versionFilePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("version", out var versionProp))
            {
                var versionStr = versionProp.GetString();
                if (!string.IsNullOrEmpty(versionStr) && Version.TryParse(versionStr, out var version))
                    return version;
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Get a version string representation
    /// </summary>
    public static string GetVersionString(Version? version = null)
    {
        version ??= GetAssemblyVersion();
        return version.ToString();
    }
}
