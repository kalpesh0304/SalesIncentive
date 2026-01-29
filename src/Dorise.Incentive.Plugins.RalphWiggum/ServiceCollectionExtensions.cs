using Dorise.Incentive.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Dorise.Incentive.Plugins.RalphWiggum;

/// <summary>
/// Extension methods for registering the Ralph Wiggum plugin with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Ralph Wiggum plugin to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRalphWiggumPlugin(this IServiceCollection services)
    {
        services.AddSingleton<IPlugin, RalphWiggumPlugin>();
        services.AddSingleton<RalphWiggumPlugin>();

        return services;
    }

    /// <summary>
    /// Registers the Ralph Wiggum plugin with the plugin registry.
    /// </summary>
    /// <param name="registry">The plugin registry.</param>
    /// <returns>The registry for chaining.</returns>
    public static IPluginRegistry AddRalphWiggum(this IPluginRegistry registry)
    {
        registry.Register(new RalphWiggumPlugin());
        return registry;
    }
}
