using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;

namespace Dalamud.Interface.Keybind;

public enum KeyType
{
    VirtualKey,
    Gamepad,
}

public struct Keybind
{
    public KeybindKey MainKey { get; set; }

    public KeybindKey Modifier1 { get; set; }

    public KeybindKey Modifier2 { get; set; }
}

public struct KeybindKey
{
    public KeyType Type { get; set; }

    public VirtualKey? VirtualKey { get; set; }

    public GamepadButtons? GamepadButtons { get; set; }
}
