using System.Collections.Concurrent;

namespace Dorise.Incentive.Core.Plugins;

/// <summary>
/// Default implementation of the plugin registry.
/// </summary>
public class PluginRegistry : IPluginRegistry
{
    private readonly ConcurrentDictionary<string, IPlugin> _plugins = new();

    /// <inheritdoc />
    public void Register(IPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        if (!_plugins.TryAdd(plugin.Id, plugin))
        {
            throw new InvalidOperationException($"A plugin with ID '{plugin.Id}' is already registered.");
        }
    }

    /// <inheritdoc />
    public IPlugin? GetPlugin(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _plugins.TryGetValue(id, out var plugin);
        return plugin;
    }

    /// <inheritdoc />
    public IEnumerable<IPlugin> GetAllPlugins()
    {
        return _plugins.Values;
    }

    /// <inheritdoc />
    public bool IsRegistered(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return _plugins.ContainsKey(id);
    }
}
