using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

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
        assembly ??= Assembly.GetExecutingAssembly();
        var versionAttr = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
        return versionAttr != null
            ? new Version(versionAttr.Version)
            : new Version(0, 0, 0, 0);
    }

    /// <summary>
    /// Get the installed service version
    /// </summary>
    public static Version? GetInstalledServiceVersion()
    {
        var exePath = ServicePathResolver.GetServiceExecutablePath();
        if (!File.Exists(exePath))
            return null;

        try
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(exePath);
            var version = fileInfo.FileVersion;
            if (!string.IsNullOrEmpty(version) && Version.TryParse(version, out var parsed))
                return parsed;
        }
        catch
        {
            // Ignore errors reading version
        }

        return null;
    }

    /// <summary>
    /// Check if an update is available
    /// </summary>
    public static bool IsUpdateAvailable(Version current, Version? installed)
    {
        if (installed == null)
            return false;

        return current > installed;
    }

    /// <summary>
    /// Read version from version.json (Nerdbank.GitVersioning)
    /// </summary>
    public static Version? ReadVersionFromFile(string versionFilePath)
    {
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
        catch
        {
            // Ignore JSON parsing errors
        }

        return null;
    }

    /// <summary>
    /// Get a version string with Git info
    /// </summary>
    public static string GetVersionString(Version? version = null)
    {
        version ??= GetAssemblyVersion();
        return version.ToString();
    }
}

/// <summary>
/// File version info helper
/// </summary>
public static class FileVersionInfo
{
    public static (string FileVersion, string ProductVersion) GetVersionInfo(string filePath)
    {
        if (!File.Exists(filePath))
            return ("", "");

        try
        {
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
            return (versionInfo.FileVersion ?? "", versionInfo.ProductVersion ?? "");
        }
        catch
        {
            return ("", "");
        }
    }
}
