using System;

namespace Dalamud.Interface.Internal.Windows.Settings;

public abstract class SettingsEntry
{
    protected Guid Id { get; } = Guid.NewGuid();

    public string Name { get; set; }

    public bool IsValid { get; protected set; }

    public virtual bool IsVisible { get; }

    public abstract void Load();

    public abstract void Save();

    public abstract void Draw();
}
