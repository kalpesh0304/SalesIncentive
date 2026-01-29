namespace Dorise.Incentive.Core.Plugins;

/// <summary>
/// Represents the result of a plugin execution.
/// </summary>
public class PluginResult
{
    /// <summary>
    /// Gets or sets whether the plugin execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message returned by the plugin.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional data returned by the plugin.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets any error message if the execution failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Creates a successful plugin result.
    /// </summary>
    public static PluginResult Ok(string message, object? data = null) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    /// <summary>
    /// Creates a failed plugin result.
    /// </summary>
    public static PluginResult Fail(string error) => new()
    {
        Success = false,
        Error = error
    };
}
