using System.Collections.Generic;
using System.Security.AccessControl;

namespace Dalamud.Injector.Isolation;

/// <summary>
/// Configuration for AppContainer isolation.
/// </summary>
public class IsolationConfig
{
    /// <summary>
    /// Path access mode.
    /// </summary>
    public enum PathMode
    {
        /// <summary>
        /// Access should be granted.
        /// </summary>
        Allow,

        /// <summary>
        /// Access should be denied.
        /// </summary>
        Deny,
    }

    public enum IntegrityLevel
    {
        Unchanged,
        Low,
        Medium
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not isolation should be applied
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not local network access should be allowed.
    /// </summary>
    public bool AllowLocalNetwork { get; set; }

    /// <summary>
    /// Gets or sets a list of entries that control access to the filesystem.
    /// Applied in order.
    /// </summary>
    public List<PathPermissionEntry> Paths { get; set; } = new();

    public void Grant(string path, FileSystemRights rights, IntegrityLevel integrityLevel)
        => this.Paths.Add(new PathPermissionEntry(PathMode.Allow, path, rights, integrityLevel));

    public void Deny(string path, FileSystemRights rights, IntegrityLevel integrityLevel)
        => this.Paths.Add(new PathPermissionEntry(PathMode.Deny, path, rights, integrityLevel));

    public record PathPermissionEntry(PathMode Mode, string Path, FileSystemRights Rights, IntegrityLevel IntegrityLevel);
}
