using Dorise.Incentive.Core.Plugins;

namespace Dorise.Incentive.Plugins.RalphWiggum;

/// <summary>
/// A plugin that provides wisdom and motivation from Ralph Wiggum.
/// Perfect for bringing joy to your sales incentive calculations.
/// </summary>
public class RalphWiggumPlugin : IPlugin
{
    private static readonly Random _random = new();

    private static readonly string[] _quotes = new[]
    {
        "Me fail English? That's unpossible!",
        "I'm learnding!",
        "Hi, Super Nintendo Chalmers!",
        "I bent my Wookiee.",
        "My cat's breath smells like cat food.",
        "I choo-choo-choose you!",
        "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there.",
        "When I grow up, I want to be a principal or a caterpillar.",
        "I'm a unitard!",
        "That's where I saw the leprechaun. He tells me to burn things.",
        "My knob tastes funny.",
        "I found a moon rock in my nose!",
        "I heard your dad went into a restaurant and ate everything in the restaurant and they had to close the restaurant.",
        "Even my boogers are spicy!",
        "Go Banana!",
        "Mrs. Krabappel and Principal Skinner were in the closet making babies and I saw one of the babies and the baby looked at me.",
        "That's my sandbox. I'm not allowed to go in the deep end.",
        "I sleep in a drawer!",
        "And when the doctor said I didn't have worms anymore, that was the happiest day of my life.",
        "Eww, Daddy, this tastes like Gramma!"
    };

    /// <inheritdoc />
    public string Id => "ralph-wiggum";

    /// <inheritdoc />
    public string Name => "Ralph Wiggum Wisdom";

    /// <inheritdoc />
    public string Version => "1.0.0";

    /// <inheritdoc />
    public string Description => "Provides motivational quotes and wisdom from Ralph Wiggum to brighten your sales incentive experience.";

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        // Ralph doesn't need much initialization - he's always ready!
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<PluginResult> ExecuteAsync()
    {
        var quote = GetRandomQuote();
        return Task.FromResult(PluginResult.Ok(quote, new RalphWiggumQuote
        {
            Quote = quote,
            Character = "Ralph Wiggum",
            Show = "The Simpsons"
        }));
    }

    /// <summary>
    /// Gets a random Ralph Wiggum quote.
    /// </summary>
    /// <returns>A random quote from Ralph.</returns>
    public static string GetRandomQuote()
    {
        return _quotes[_random.Next(_quotes.Length)];
    }

    /// <summary>
    /// Gets all available Ralph Wiggum quotes.
    /// </summary>
    /// <returns>All quotes.</returns>
    public static IReadOnlyList<string> GetAllQuotes()
    {
        return _quotes.ToList().AsReadOnly();
    }
}

/// <summary>
/// Represents a Ralph Wiggum quote with metadata.
/// </summary>
public class RalphWiggumQuote
{
    /// <summary>
    /// The quote text.
    /// </summary>
    public string Quote { get; set; } = string.Empty;

    /// <summary>
    /// The character who said the quote.
    /// </summary>
    public string Character { get; set; } = string.Empty;

    /// <summary>
    /// The show the quote is from.
    /// </summary>
    public string Show { get; set; } = string.Empty;
}
