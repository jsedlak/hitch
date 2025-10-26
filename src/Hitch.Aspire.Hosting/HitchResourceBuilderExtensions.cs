namespace Hitch.Aspire.Hosting;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Extension methods for <see cref="IHitchResourceBuilder"/>.
/// </summary>
public static class HitchResourceBuilderExtensions
{
    /// <summary>
    /// Adds a plugin configuration entry to the Hitch resource.
    /// </summary>
    /// <param name="builder">The Hitch resource builder.</param>
    /// <param name="category">The plugin category.</param>
    /// <param name="subCategory">The plugin subcategory.</param>
    /// <param name="serviceName">The service name for this plugin instance.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IHitchResourceBuilder WithPlugin(
        this IHitchResourceBuilder builder,
        string category,
        string subCategory,
        string serviceName)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category cannot be null or whitespace.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(subCategory))
        {
            throw new ArgumentException("SubCategory cannot be null or whitespace.", nameof(subCategory));
        }

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));
        }

        var key = $"{category}__{subCategory}";
        if (!builder.Resource.Plugins.ContainsKey(key))
        {
            builder.Resource.Plugins[key] = new List<string>();
        }

        if (!builder.Resource.Plugins[key].Contains(serviceName))
        {
            builder.Resource.Plugins[key].Add(serviceName);
        }

        return builder;
    }

    /// <summary>
    /// Publishes the Hitch configuration to consuming projects.
    /// </summary>
    /// <param name="builder">The Hitch resource builder.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IHitchResourceBuilder PublishAsConfiguration(this IHitchResourceBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // Add callback to generate configuration
        builder.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        {
            var resource = builder.Resource;

            // Add assemblies configuration
            if (resource.Assemblies.Count > 0)
            {
                for (int i = 0; i < resource.Assemblies.Count; i++)
                {
                    context.EnvironmentVariables[$"Hitch__Configuration__Assemblies__{i}"] = resource.Assemblies[i];
                }
            }

            // Add file patterns configuration
            if (resource.FilePatterns.Count > 0)
            {
                for (int i = 0; i < resource.FilePatterns.Count; i++)
                {
                    context.EnvironmentVariables[$"Hitch__Configuration__FilePatterns__{i}"] = resource.FilePatterns[i];
                }
            }

            // Add plugins configuration
            foreach (var kvp in resource.Plugins)
            {
                var parts = kvp.Key.Split("__");
                if (parts.Length == 2)
                {
                    var category = parts[0];
                    var subCategory = parts[1];

                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        context.EnvironmentVariables[$"Hitch__Plugins__{category}__{subCategory}__{i}"] = kvp.Value[i];
                    }
                }
            }
        }));

        return builder;
    }
}

