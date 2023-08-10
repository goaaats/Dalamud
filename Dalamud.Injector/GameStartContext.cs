using Dalamud.Injector.Isolation;

namespace Dalamud.Injector;

public record GameStartContext
{
    /// <summary>
    /// Gets the working directory.
    /// </summary>
    public string WorkingDir { get; init; } = null!;

    /// <summary>
    /// Gets the path to the executable file
    /// </summary>
    public string ExePath { get; init; } = null!;

    /// <summary>
    /// Gets the arguments to pass to the executable file.
    /// </summary>
    public string Arguments { get; init; } = null!;

    /// <summary>
    /// Gets a value indicating whether or not we fix the ACL.
    /// </summary>
    public bool DontFixAcl { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether we wait for the game window to be ready before proceeding.
    /// </summary>
    public bool WaitForGameWindow { get; init; } = true;

    /// <summary>
    /// Gets configuration for AppContainer isolation.
    /// </summary>
    public IsolationConfig? Isolation { get; init; } = null;

    /// <summary>
    /// Gets the path to where dalamud binaries (i.e. directory containing Dalamud.dll) are located.
    /// </summary>
    public string? DalamudBinaryDirectory { get; init; } = null!;
}
