using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TokenSaver.Common.Installation;

/// <summary>
/// Checks installation status and requirements
/// </summary>
public static class InstallationStatusChecker
{
    /// <summary>
    /// Installation status result
    /// </summary>
    public class InstallationStatus
    {
        public bool IsInstalled { get; set; }
        public bool ServiceExists { get; set; }
        public bool ConfigExists { get; set; }
        public Version? InstalledVersion { get; set; }
        public string? InstallPath { get; set; }
        public string? ConfigPath { get; set; }
    }

    /// <summary>
    /// Check the installation status
    /// </summary>
    public static InstallationStatus CheckInstallationStatus()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new InstallationStatus
            {
                IsInstalled = false,
                ServiceExists = false,
                ConfigExists = false
            };
        }

        var programFilesPath = ServicePathResolver.GetProgramFilesPath();
        var configPath = ServicePathResolver.GetGlobalConfigPath();
        var exePath = ServicePathResolver.GetServiceExecutablePath();

        var serviceExists = File.Exists(exePath);
        var configExists = File.Exists(configPath);

        return new InstallationStatus
        {
            IsInstalled = serviceExists,
            ServiceExists = serviceExists,
            ConfigExists = configExists,
            InstalledVersion = VersionDetector.GetInstalledServiceVersion(),
            InstallPath = programFilesPath,
            ConfigPath = configPath
        };
    }

    /// <summary>
    /// Check installation status asynchronously
    /// </summary>
    public static Task<InstallationStatus> CheckInstallationStatusAsync()
    {
        return Task.FromResult(CheckInstallationStatus());
    }

    /// <summary>
    /// Check if .tokensaver directory is in .gitignore
    /// </summary>
    public static bool IsTokenSaverInGitignore(string workspaceRoot)
    {
        var gitignorePath = Path.Combine(workspaceRoot, ".gitignore");
        if (!File.Exists(gitignorePath))
            return false;

        try
        {
            var content = File.ReadAllText(gitignorePath);
            return content.Contains(".tokensaver/") || content.Contains(".tokensaver");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if required directories can be created
    /// </summary>
    public static bool CanCreateDirectories()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var path = ServicePathResolver.GetGlobalConfigDirectory();
                if (!Directory.Exists(path))
                {
                    // Just test; don't actually create
                    var parent = Directory.GetParent(path);
                    return parent?.Exists ?? false;
                }
                return true;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get a detailed status report
    /// </summary>
    public static StatusReport GetDetailedStatus(string? workspaceRoot = null)
    {
        var installStatus = CheckInstallationStatus();
        workspaceRoot ??= WorkspaceRootDetector.FindWorkspaceRoot();

        return new StatusReport
        {
            IsInstalled = installStatus.IsInstalled,
            ServiceExists = installStatus.ServiceExists,
            ConfigExists = installStatus.ConfigExists,
            InstalledVersion = installStatus.InstalledVersion,
            InstallPath = installStatus.InstallPath,
            GlobalConfigPath = installStatus.ConfigPath,
            WorkspaceRoot = workspaceRoot,
            WorkspaceConfigPath = workspaceRoot != null
                ? WorkspaceRootDetector.GetWorkspaceConfigPath(workspaceRoot)
                : null,
            TokenSaverDir = workspaceRoot != null
                ? WorkspaceRootDetector.GetTokenSaverDirectory(workspaceRoot)
                : null,
            IsTokenSaverInGitignore = workspaceRoot != null && IsTokenSaverInGitignore(workspaceRoot),
            CanCreateDirectories = CanCreateDirectories()
        };
    }

    /// <summary>
    /// Detailed status report
    /// </summary>
    public class StatusReport
    {
        public bool IsInstalled { get; set; }
        public bool ServiceExists { get; set; }
        public bool ConfigExists { get; set; }
        public Version? InstalledVersion { get; set; }
        public string? InstallPath { get; set; }
        public string? GlobalConfigPath { get; set; }
        public string? WorkspaceRoot { get; set; }
        public string? WorkspaceConfigPath { get; set; }
        public string? TokenSaverDir { get; set; }
        public bool IsTokenSaverInGitignore { get; set; }
        public bool CanCreateDirectories { get; set; }
    }
}
