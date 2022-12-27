namespace Dalamud.Interface.Internal.Windows.Settings;

public class GapEntry : SettingsEntry
{
    private readonly float size;

    public GapEntry(float size)
    {
        this.size = size;
    }

    public override void Load()
    {
        // ignored
    }

    public override void Save()
    {
        // ignored
    }

    public override void Draw()
    {
        ImGuiHelpers.ScaledDummy(this.size);
    }
}
