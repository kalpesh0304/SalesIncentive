namespace Dorise.Incentive.Core.Plugins;

/// <summary>
/// Base interface for all plugins in the Dorise Sales Incentive Framework.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the unique identifier for this plugin.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the description of what this plugin does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Initializes the plugin.
    /// </summary>
    /// <returns>A task representing the async initialization operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Executes the plugin's main functionality.
    /// </summary>
    /// <returns>The result of the plugin execution.</returns>
    Task<PluginResult> ExecuteAsync();
}
