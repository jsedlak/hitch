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
    /// <param name="configurations">Optional dictionary of configuration key-value pairs for the plugin instance.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IHitchResourceBuilder WithPlugin(
        this IHitchResourceBuilder builder,
        string category,
        string subCategory,
        string serviceName,
        IDictionary<string, object>? configurations = null)
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

        // Add plugin configurations if provided
        if (configurations != null && configurations.Count > 0)
        {
            var configKey = $"{category}__{subCategory}__{serviceName}";
            if (!builder.Resource.PluginConfigurations.ContainsKey(configKey))
            {
                builder.Resource.PluginConfigurations[configKey] = new Dictionary<string, object>();
            }

            var config = builder.Resource.PluginConfigurations[configKey];
            foreach (var kvp in configurations)
            {
                config[kvp.Key] = kvp.Value;
            }
        }

        return builder;
    }

    /// <summary>
    /// Adds a reference to a Hitch resource from a consumer resource.
    /// This will automatically inject the Hitch configuration as environment variables.
    /// </summary>
    /// <typeparam name="TConsumer">The type of the consumer resource.</typeparam>
    /// <param name="consumer">The consumer resource builder.</param>
    /// <param name="hitch">The Hitch resource builder.</param>
    /// <returns>The consumer resource builder for chaining.</returns>
    public static IResourceBuilder<TConsumer> WithReference<TConsumer>(
        this IResourceBuilder<TConsumer> consumer,
        IResourceBuilder<HitchResource> hitch)
        where TConsumer : IResourceWithEnvironment
    {
        if (consumer == null)
        {
            throw new ArgumentNullException(nameof(consumer));
        }

        if (hitch == null)
        {
            throw new ArgumentNullException(nameof(hitch));
        }

        // Hook an environment callback onto the consumer
        // At runtime Aspire will evaluate this and add the environment variables
        return consumer.WithEnvironment(ctx =>
        {
            // Get all environment exports from the Hitch resource
            foreach (var (key, value) in hitch.Resource.GetEnvironmentExports())
            {
                // Push each one as an env var on the consumer
                ctx.EnvironmentVariables[key] = value;
            }
        });
    }
}

