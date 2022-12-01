using System;

namespace Dalamud.Interface.Keybind;

/// <summary>
/// Flags that can modify keybind behaviour.
/// </summary>
[Flags]
public enum KeybindFlags
{
    /// <summary>
    /// No flags set.
    /// </summary>
    None,

    /// <summary>
    /// Allow modifier keys.
    /// </summary>
    AllowModifiers = 1,

    /// <summary>
    /// Allow gamepad binds.
    /// </summary>
    AllowGamepad = 1 << 1,

    /// <summary>
    /// Allow this key to be double-bound with a game keybind.
    /// </summary>
    AllowGameBind = 1 << 2,

    /// <summary>
    /// Allow this key to be double-bound with a plugin keybind.
    /// </summary>
    AllowPluginBind = 1 << 3,
}
