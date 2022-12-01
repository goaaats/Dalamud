using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Dalamud.Configuration.Internal;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;
using Serilog;

namespace Dalamud.Interface.Keybind;

/// <summary>
/// Class responsible for managing keybinds at runtime.
/// </summary>
[ServiceManager.EarlyLoadedService]
internal class KeybindManager : IServiceType
{
    private readonly DalamudConfiguration configuration;
    private readonly Dictionary<Guid, KeybindInfo> binds = new();

    public KeybindManager(DalamudConfiguration configuration)
    {
        this.configuration = configuration;
        this.configuration.Keybinds ??= new Dictionary<string, Keybind>();
    }

    /// <summary>
    /// Gets a rea-only dictionary of all registered keybinds.
    /// </summary>
    public IReadOnlyDictionary<Guid, KeybindInfo> Keybinds => this.binds;

    /// <summary>
    /// Register a keybind.
    /// </summary>
    /// <param name="internalName">The internal name of the plugin registering the bind.</param>
    /// <param name="id">The ID of the bind used for saving.</param>
    /// <param name="flags">Flags to apply to the keybind selection.</param>
    /// <param name="localizedNameFunc">Function resolving the localized name of the bind.</param>
    /// <param name="localizedDescFunc">Function resolving the localized description of the bind.</param>
    /// <returns>GUID uniquely identifying this keybind, used to look it up at runtime. This will change each time you register the bind.</returns>
    public Guid RegisterBind(string internalName, string id, KeybindFlags flags = KeybindFlags.None, Func<string>? localizedNameFunc = null, Func<string>? localizedDescFunc = null)
    {
        if (this.binds.Any(x => x.Value.ByInternalName == internalName && x.Value.Id == id))
            throw new ArgumentException("A bind with this ID was already registered.");

        if (localizedNameFunc == null || localizedDescFunc == null)
            Log.Warning("{Plugin}: Bind {Id} does not provide localized names for binds. You should set this up.", internalName, id);

        var guid = Guid.NewGuid();
        this.binds.Add(guid, new KeybindInfo()
        {
            ByInternalName = internalName,
            Id = id,
            Flags = flags,
        });

        return guid;
    }

    /// <summary>
    /// Unregister a bind.
    /// </summary>
    /// <param name="guid">The GUID of the bind used for saving.</param>
    public void UnregisterBind(Guid guid)
    {
        if (!this.binds.Remove(guid))
            throw new ArgumentException("Guid was not for a registered keybind.");
    }

    /// <summary>
    /// Unregister all keybinds for a specific plugin.
    /// </summary>
    /// <param name="internalName">Name of the plugin.</param>
    public void UnregisterAllFor(string internalName)
    {
        var toRemove = this.binds.Where(x => x.Value.ByInternalName == internalName)
                           .Select(x => x.Key);

        foreach (var guid in toRemove)
        {
            Debug.Assert(this.binds.Remove(guid), "this.binds.Remove(guid): bind not found");
        }
    }

    /// <summary>
    /// Get the configuration assigned to this bind.
    /// Null if unassigned.
    /// </summary>
    /// <param name="guid">The ID of the bind.</param>
    /// <returns>The config of the bind, or null.</returns>
    public Keybind? GetConfigForBind(Guid guid)
    {
        if (!this.binds.TryGetValue(guid, out var bindInfo))
            throw new ArgumentException("Keybind with this guid not registered");

        if (!this.configuration.Keybinds!.TryGetValue(bindInfo.ConfigKey, out var config))
            return null;

        return config;
    }

    public struct KeybindInfo
    {
        /// <summary>
        /// The plugin that registered this bind.
        /// </summary>
        public string ByInternalName { get; set; }

        /// <summary>
        /// The ID of the keybind.
        /// </summary>
        public string Id { get; set; }
        
        public KeybindFlags Flags { get; set; }

        public string ConfigKey => $"{ByInternalName}-{Id}";
    }
}
