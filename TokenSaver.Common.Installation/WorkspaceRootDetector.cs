using System;
using System.IO;

namespace TokenSaver.Common.Installation;

/// <summary>
/// Detects the workspace root directory by searching for .git or .slnx markers.
/// Walk upward from startPath until finding a directory with either marker.
/// </summary>
public static class WorkspaceRootDetector
{
    private const string GitMarker = ".git";
    private const string SolutionMarker = "*.slnx";

    /// <summary>
    /// Find the workspace root starting from a given directory by walking upward.
    /// Returns null if no workspace root is found.
    /// </summary>
    public static string? FindWorkspaceRoot(string? startPath = null)
    {
        var current = startPath ?? Environment.CurrentDirectory;

        while (!string.IsNullOrEmpty(current))
        {
            if (IsWorkspaceRoot(current))
                return current;

            try
            {
                var parent = Directory.GetParent(current);
                if (parent == null)
                    break;
                current = parent.FullName;
            }
            catch (ArgumentException)
            {
                // Path has invalid characters
                break;
            }
        }

        return null;
    }

    /// <summary>
    /// Check if a directory is a workspace root (has .git or .slnx).
    /// Returns false on any error (access denied, not found, invalid path).
    /// </summary>
    public static bool IsWorkspaceRoot(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            return false;

        if (!Directory.Exists(directoryPath))
            return false;

        // Check for .git directory or .slnx file
        try
        {
            var gitPath = Path.Combine(directoryPath, GitMarker);
            if (Directory.Exists(gitPath))
                return true;

            var slnxFiles = Directory.GetFiles(directoryPath, SolutionMarker);
            return slnxFiles.Length > 0;
        }
        catch (ArgumentException)
        {
            // Path has invalid characters
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // No permission to read directory
            return false;
        }
        catch (System.IO.PathTooLongException)
        {
            return false;
        }
    }

    /// <summary>
    /// Get the .tokensaver directory path for a workspace root.
    /// </summary>
    public static string GetTokenSaverDirectory(string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot))
            throw new ArgumentException("Workspace root cannot be null or empty", nameof(workspaceRoot));

        return Path.Combine(workspaceRoot, ".tokensaver");
    }

    /// <summary>
    /// Get the config file path in the workspace.
    /// </summary>
    public static string GetWorkspaceConfigPath(string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot))
            throw new ArgumentException("Workspace root cannot be null or empty", nameof(workspaceRoot));

        return Path.Combine(GetTokenSaverDirectory(workspaceRoot), "config.json");
    }

    /// <summary>
    /// Get the index database path in the workspace.
    /// </summary>
    public static string GetIndexDatabasePath(string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot))
            throw new ArgumentException("Workspace root cannot be null or empty", nameof(workspaceRoot));

        return Path.Combine(GetTokenSaverDirectory(workspaceRoot), "index.db");
    }
}
