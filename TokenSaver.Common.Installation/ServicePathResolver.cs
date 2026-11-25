using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AiDevTools.TokenSaver.Common.Installation;

/// <summary>
/// Resolves paths for TokenSaver service installation and configuration.
/// All methods throw PlatformNotSupportedException on non-Windows platforms for consistency.
/// </summary>
public static class ServicePathResolver
{
    private const string ServiceDirName = "TokenSaver";
    private const string ServiceExeName = "TokenSaver.MCP.exe";
    private const string ServiceName = "TokenSaverMCP";

    private static void AssertWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("TokenSaver service is only supported on Windows");
    }

    /// <summary>
    /// Get the program files installation directory.
    /// Throws on non-Windows platforms.
    /// </summary>
    public static string GetProgramFilesPath()
    {
        AssertWindows();
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            ServiceDirName
        );
    }

    /// <summary>
    /// Get the global configuration directory.
    /// Throws on non-Windows platforms.
    /// </summary>
    public static string GetGlobalConfigDirectory()
    {
        AssertWindows();
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            ServiceDirName
        );
    }

    /// <summary>
    /// Get the global configuration file path.
    /// Throws on non-Windows platforms.
    /// </summary>
    public static string GetGlobalConfigPath()
    {
        return Path.Combine(GetGlobalConfigDirectory(), "config.json");
    }

    /// <summary>
    /// Get the service executable path.
    /// Throws on non-Windows platforms.
    /// </summary>
    public static string GetServiceExecutablePath()
    {
        return Path.Combine(GetProgramFilesPath(), ServiceExeName);
    }

    /// <summary>
    /// Get the Windows service name for TaskScheduler/SCM
    /// </summary>
    public static string ServiceRegistrationName => ServiceName;

    /// <summary>
    /// Check if the service executable exists (weak indicator of installation).
    /// Returns false on non-Windows platforms.
    /// </summary>
    public static bool IsServiceInstalled()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        try
        {
            var exePath = GetServiceExecutablePath();
            return File.Exists(exePath);
        }
        catch (ArgumentException)
        {
            // Path contains invalid characters
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // No permission to check
            return false;
        }
    }

    /// <summary>
    /// Get the user data directory (LocalApplicationData\TokenSaver)
    /// </summary>
    public static string GetUserDataDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ServiceDirName
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
    /// Get the cached models directory
    /// </summary>
    public static string GetModelsDirectory()
    {
        return Path.Combine(GetUserDataDirectory(), "Models");
    }
}
