using System;
using System.IO;
using System.Threading.Tasks;

namespace TokenSaver.Common.Installation;

/// <summary>
/// Detects the workspace root directory by searching for .git or .sln markers
/// </summary>
public static class WorkspaceRootDetector
{
    private const string GitMarker = ".git";
    private const string SolutionMarker = "*.slnx";

    /// <summary>
    /// Find the workspace root starting from a given directory
    /// </summary>
    public static string? FindWorkspaceRoot(string? startPath = null)
    {
        var current = startPath ?? Environment.CurrentDirectory;

        while (!string.IsNullOrEmpty(current))
        {
            if (IsWorkspaceRoot(current))
                return current;

            var parent = Directory.GetParent(current);
            current = parent?.FullName;
        }

        return null;
    }

    /// <summary>
    /// Find the workspace root asynchronously
    /// </summary>
    public static Task<string?> FindWorkspaceRootAsync(string? startPath = null)
    {
        return Task.FromResult(FindWorkspaceRoot(startPath));
    }

    /// <summary>
    /// Check if a directory is a workspace root
    /// </summary>
    public static bool IsWorkspaceRoot(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return false;

        // Check for .git directory or .slnx file
        var gitPath = Path.Combine(directoryPath, GitMarker);
        if (Directory.Exists(gitPath))
            return true;

        try
        {
            var slnxFiles = Directory.GetFiles(directoryPath, SolutionMarker);
            return slnxFiles.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the .tokensaver directory for a workspace
    /// </summary>
    public static string GetTokenSaverDirectory(string workspaceRoot)
    {
        return Path.Combine(workspaceRoot, ".tokensaver");
    }

    /// <summary>
    /// Get the config file path in the workspace
    /// </summary>
    public static string GetWorkspaceConfigPath(string workspaceRoot)
    {
        return Path.Combine(GetTokenSaverDirectory(workspaceRoot), "config.json");
    }

    /// <summary>
    /// Get the index database path in the workspace
    /// </summary>
    public static string GetIndexDatabasePath(string workspaceRoot)
    {
        return Path.Combine(GetTokenSaverDirectory(workspaceRoot), "index.db");
    }
}
