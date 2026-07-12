using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TokenSaver.Common.Installation;

/// <summary>
/// Resolves paths for TokenSaver service installation and configuration
/// </summary>
public static class ServicePathResolver
{
    /// <summary>
    /// Get the program files installation directory
    /// </summary>
    public static string GetProgramFilesPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("Service installation is only supported on Windows");

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "TokenSaver"
        );
    }

    /// <summary>
    /// Get the global configuration directory
    /// </summary>
    public static string GetGlobalConfigDirectory()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("Service configuration is only supported on Windows");

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TokenSaver"
        );
    }

    /// <summary>
    /// Get the global configuration file path
    /// </summary>
    public static string GetGlobalConfigPath()
    {
        return Path.Combine(GetGlobalConfigDirectory(), "config.json");
    }

    /// <summary>
    /// Get the service executable path
    /// </summary>
    public static string GetServiceExecutablePath()
    {
        return Path.Combine(GetProgramFilesPath(), "TokenSaver.MCP.exe");
    }

    /// <summary>
    /// Get the service name
    /// </summary>
    public static string GetServiceName()
    {
        return "TokenSaverMCP";
    }

    /// <summary>
    /// Check if the service is installed
    /// </summary>
    public static bool IsServiceInstalled()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        var exePath = GetServiceExecutablePath();
        return File.Exists(exePath);
    }

    /// <summary>
    /// Get the user data directory
    /// </summary>
    public static string GetUserDataDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TokenSaver"
        );
    }

    /// <summary>
    /// Get the logs directory
    /// </summary>
    public static string GetLogsDirectory()
    {
        return Path.Combine(GetUserDataDirectory(), "Logs");
    }

    /// <summary>
    /// Ensure a service directory exists
    /// </summary>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// Get the cached models directory
    /// </summary>
    public static string GetModelsDirectory()
    {
        return Path.Combine(GetUserDataDirectory(), "Models");
    }
}
