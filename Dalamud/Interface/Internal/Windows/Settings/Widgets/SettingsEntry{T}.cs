using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Dalamud.Configuration.Internal;
using Dalamud.Interface.Colors;
using Dalamud.Utility;
using ImGuiNET;

namespace Dalamud.Interface.Internal.Windows.Settings;

internal class SettingsEntry<T> : SettingsEntry
{
    private object? valueBacking;

    public delegate T? LoadSettingDelegate(DalamudConfiguration config);

    public delegate void SaveSettingDelegate(T? value, DalamudConfiguration config);

    private readonly LoadSettingDelegate load;
    private readonly SaveSettingDelegate save;

    public T? Value => (T)this.valueBacking;

    public string Description { get; }

    public Func<T?, string?>? CheckValidity { get; init; }

    public Func<T?, string?>? CheckWarning { get; init; }

    public Func<bool>? CheckVisibility { get; init; }

    public override bool IsVisible => CheckVisibility?.Invoke() ?? true;

    public SettingsEntry(string name, string description, LoadSettingDelegate load, SaveSettingDelegate save)
    {
        this.load = load;
        this.save = save;
        this.Name = name;
        this.Description = description;
    }

    public override void Draw()
    {
        var type = typeof(T);

        if (type == typeof(DirectoryInfo))
        {
            ImGuiHelpers.SafeTextWrapped(this.Name);

            var value = this.Value as DirectoryInfo;
            var nativeBuffer = value?.FullName ?? string.Empty;

            if (ImGui.InputText($"###{Id.ToString()}", ref nativeBuffer, 1000))
            {
                this.valueBacking = !string.IsNullOrEmpty(nativeBuffer) ? new DirectoryInfo(nativeBuffer) : null;
            }
        }
        else if (type == typeof(string))
        {
            ImGuiHelpers.SafeTextWrapped(this.Name);

            var nativeBuffer = this.Value as string ?? string.Empty;

            if (ImGui.InputText($"###{Id.ToString()}", ref nativeBuffer, 1000))
            {
                this.valueBacking = nativeBuffer;
            }
        }
        else if (type == typeof(bool))
        {
            var nativeValue = this.Value as bool? ?? false;

            if (ImGui.Checkbox($"{Name}###{Id.ToString()}", ref nativeValue))
            {
                this.valueBacking = nativeValue;
            }
        }
        /*
        else if (type.IsEnum)
        {
            ImGuiHelpers.SafeTextWrapped(this.Name);

            var idx = (int)(this.InternalValue ?? 0);
            var values = Enum.GetValues(type);
            var descriptions = values.Cast<Enum>().Select(x => x.GetAttribute<SettingsDescriptionAttribute>() ?? new SettingsDescriptionAttribute(x.ToString(), string.Empty)).ToArray();

            if (ImGui.BeginCombo($"###{Id.ToString()}", descriptions[idx].FriendlyName))
            {
                foreach (int value in values)
                {
                    if (ImGui.Selectable(descriptions[value].FriendlyName, idx == value))
                    {
                        this.InternalValue = value;
                    }
                }

                ImGui.EndCombo();
            }
        }
        */

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        ImGuiHelpers.SafeTextWrapped(this.Description);
        ImGui.PopStyleColor();

        if (this.CheckValidity != null)
        {
            var validityMsg = this.CheckValidity.Invoke(this.Value);
            this.IsValid = string.IsNullOrEmpty(validityMsg);

            if (!this.IsValid)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.Text(validityMsg);
                ImGui.PopStyleColor();
            }
        }
        else
        {
            this.IsValid = true;
        }

        var warningMessage = this.CheckWarning?.Invoke(this.Value);

        if (warningMessage != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.Text(warningMessage);
            ImGui.PopStyleColor();
        }
    }

    public override void Load()
    {
        this.valueBacking = this.load(Service<DalamudConfiguration>.Get());

        if (this.CheckValidity != null)
        {
            this.IsValid = this.CheckValidity(this.Value) == null;
        }
        else
        {
            this.IsValid = true;
        }
    }

    public override void Save() => this.save(this.Value, Service<DalamudConfiguration>.Get());
}
