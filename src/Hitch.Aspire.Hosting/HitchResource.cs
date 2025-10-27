namespace Hitch.Aspire.Hosting;

using global::Aspire.Hosting.ApplicationModel;
using System.Collections.Generic;

/// <summary>
/// Represents a Hitch plugin configuration resource for Aspire applications.
/// </summary>
public class HitchResource : Resource
{
    /// <summary>
    /// Gets the list of assembly names to scan for plugins.
    /// </summary>
    public List<string> Assemblies { get; } = new();

    /// <summary>
    /// Gets the list of file patterns for discovering plugin assemblies.
    /// </summary>
    public List<string> FilePatterns { get; } = new();

    /// <summary>
    /// Gets the plugin configuration entries.
    /// Key format: "Category__SubCategory"
    /// Value: List of service names
    /// </summary>
    public Dictionary<string, List<string>> Plugins { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HitchResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    public HitchResource(string name) : base(name)
    {
    }

    internal IEnumerable<(string Key, string Value)> GetEnvironmentExports()
    {
        // Example strategy, tweak as you like:
        yield return ("HITCH_ASSEMBLIES", string.Join(';', Assemblies));
        yield return ("HITCH_FILE_PATTERNS", string.Join(';', FilePatterns));

        // Plugins becomes multiple vars, or one blob. Your call.
        // Here: one JSON blob called HITCH_PLUGINS
        var pluginMap = Plugins.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value);

        var json = System.Text.Json.JsonSerializer.Serialize(pluginMap);
        yield return ("HITCH_PLUGINS", json);
    }
}

