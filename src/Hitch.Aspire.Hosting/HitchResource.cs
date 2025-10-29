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
    /// Gets the plugin configuration values.
    /// Key format: "Category__SubCategory__ServiceName"
    /// Value: Dictionary of configuration key-value pairs
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> PluginConfigurations { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HitchResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    public HitchResource(string name) : base(name)
    {
    }

    internal IEnumerable<(string Key, object Value)> GetEnvironmentExports()
    {
        // Export assemblies
        if (Assemblies.Count > 0)
        {
            for (int i = 0; i < Assemblies.Count; i++)
            {
                yield return ($"Hitch__Configuration__Assemblies__{i}", Assemblies[i]);
            }
        }

        // Export file patterns
        if (FilePatterns.Count > 0)
        {
            for (int i = 0; i < FilePatterns.Count; i++)
            {
                yield return ($"Hitch__Configuration__FilePatterns__{i}", FilePatterns[i]);
            }
        }

        // Export plugins as individual environment variables
        // Format: Hitch__Plugins__Category__SubCategory__servicename = servicename
        foreach (var kvp in Plugins)
        {
            var parts = kvp.Key.Split("__");
            if (parts.Length == 2)
            {
                var category = parts[0];
                var subCategory = parts[1];

                foreach (var serviceName in kvp.Value)
                {
                    var envKey = $"Hitch__Plugins__{category}__{subCategory}__{serviceName}";
                    yield return (envKey, serviceName);
                }
            }
        }

        // Export plugin-specific configuration values
        // Format: Hitch__Plugins__Category__SubCategory__servicename__PropertyName = value
        foreach (var configKvp in PluginConfigurations)
        {
            var parts = configKvp.Key.Split("__");
            if (parts.Length >= 3)
            {
                var category = parts[0];
                var subCategory = parts[1];
                var serviceName = parts[2];

                foreach (var propertyKvp in configKvp.Value)
                {
                    var key = $"Hitch__Plugins__{category}__{subCategory}__{serviceName}__{propertyKvp.Key}";
                    
                    // Handle ParameterResource by creating a proper reference expression
                    if (propertyKvp.Value is ParameterResource paramResource)
                    {
                        // Create a reference expression that Aspire will resolve to the parameter's value
                        yield return (key, ReferenceExpression.Create($"{paramResource}"));
                    }
                    else
                    {
                        // For other types, pass through directly
                        yield return (key, propertyKvp.Value);
                    }
                }
            }
        }
    }
}

