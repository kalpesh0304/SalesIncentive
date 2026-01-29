namespace Dorise.Incentive.Core.Plugins;

/// <summary>
/// Registry for discovering and managing plugins.
/// </summary>
public interface IPluginRegistry
{
    /// <summary>
    /// Registers a plugin with the registry.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    void Register(IPlugin plugin);

    /// <summary>
    /// Gets a plugin by its identifier.
    /// </summary>
    /// <param name="id">The plugin identifier.</param>
    /// <returns>The plugin if found, null otherwise.</returns>
    IPlugin? GetPlugin(string id);

    /// <summary>
    /// Gets all registered plugins.
    /// </summary>
    /// <returns>A collection of all registered plugins.</returns>
    IEnumerable<IPlugin> GetAllPlugins();

    /// <summary>
    /// Checks if a plugin with the given identifier is registered.
    /// </summary>
    /// <param name="id">The plugin identifier.</param>
    /// <returns>True if the plugin is registered, false otherwise.</returns>
    bool IsRegistered(string id);
}
