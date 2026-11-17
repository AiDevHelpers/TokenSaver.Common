using System;
using System.IO;

namespace TokenSaver.Common.Models;

/// <summary>
///     Where each AI client keeps its MCP config, relative to the user's profile and roaming AppData folders. Injectable
///     so tests (and dry runs) work on a temporary tree and never touch real client files.
/// </summary>
public sealed record ProviderPaths(string Home, string AppData)
{
    /// <summary>The current user's real folders.</summary>
    public static ProviderPaths Default() => new(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

    private string VsCodeUser => Path.Combine(AppData, "Code", "User");

    /// <summary>The config file this provider's entry goes into.</summary>
    public string ConfigPath(ProviderType type) => type switch
    {
        ProviderType.VsCodeCopilot => Path.Combine(VsCodeUser, "mcp.json"),
        ProviderType.ClaudeCode => Path.Combine(Home, ".claude.json"),
        ProviderType.Cursor => Path.Combine(Home, ".cursor", "mcp.json"),
        ProviderType.Continue => Path.Combine(Home, ".continue", "mcpServers", "tokensaver.yaml"),
        ProviderType.Cline => Path.Combine(VsCodeUser, "globalStorage", "saoudrizwan.claude-dev", "settings", "cline_mcp_settings.json"),
        ProviderType.Sixth => Path.Combine(VsCodeUser, "globalStorage", "sixth.sixth-ai", "settings", "sixth-mcp-settings.json"),
        ProviderType.ClaudeDesktop => Path.Combine(AppData, "Claude", "claude_desktop_config.json"),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>
    ///     True when the client itself is present (its folder or, for Claude Code, its state file exists). A config file is
    ///     only created for an installed client; nothing is ever written for one that is not there.
    /// </summary>
    public bool IsInstalled(ProviderType type) => type switch
    {
        ProviderType.VsCodeCopilot => Directory.Exists(VsCodeUser),
        ProviderType.ClaudeCode => File.Exists(ConfigPath(type)),
        ProviderType.Cursor => Directory.Exists(Path.Combine(Home, ".cursor")),
        ProviderType.Continue => Directory.Exists(Path.Combine(Home, ".continue")),
        ProviderType.Cline => Directory.Exists(Path.Combine(VsCodeUser, "globalStorage", "saoudrizwan.claude-dev")),
        ProviderType.Sixth => Directory.Exists(Path.Combine(VsCodeUser, "globalStorage", "sixth.sixth-ai")),
        ProviderType.ClaudeDesktop => Directory.Exists(Path.Combine(AppData, "Claude")),
        _ => false,
    };
}
