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

    public void Grant(string path, bool r, bool w, bool x)
        => this.Paths.Add(new PathPermissionEntry(PathMode.Grant, path, r, w, x));

    public void Deny(string path, bool r, bool w, bool x)
        => this.Paths.Add(new PathPermissionEntry(PathMode.Deny, path, r, w, x));

    public record PathPermissionEntry(PathMode Mode, string Path, bool Read, bool Write, bool Execute);
}
