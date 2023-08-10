using System.Collections.Generic;

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
        Grant,

        /// <summary>
        /// Access should be denied.
        /// </summary>
        Deny,
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

    public record PathPermissionEntry(PathMode Mode, bool Read, bool Write, bool Execute);
}
