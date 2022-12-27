using System.Numerics;
using ImGuiNET;

namespace Dalamud.Interface.Internal.Windows.Settings;

public abstract class SettingsTab
{
    public abstract SettingsEntry[] Entries { get; }

    public abstract string Title { get; }

    public virtual void Draw()
    {
        foreach (var settingsEntry in Entries)
        {
            if (settingsEntry.IsVisible)
                settingsEntry.Draw();

            ImGui.Dummy(new Vector2(10) * ImGuiHelpers.GlobalScale);
        }
    }

    public void Load()
    {
        foreach (var settingsEntry in Entries)
        {
            settingsEntry.Load();
        }
    }

    public virtual void Save()
    {
        foreach (var settingsEntry in Entries)
        {
            settingsEntry.Save();
        }
    }
}
