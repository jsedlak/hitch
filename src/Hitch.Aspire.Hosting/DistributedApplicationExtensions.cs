namespace Hitch.Aspire.Hosting;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Extension methods for <see cref="IDistributedApplicationBuilder"/> to add Hitch configuration.
/// </summary>
public static class DistributedApplicationExtensions
{
    /// <summary>
    /// Adds Hitch plugin configuration to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="configure">Optional configuration action for the Hitch builder.</param>
    /// <returns>A resource builder for Hitch configuration.</returns>
    public static IHitchResourceBuilder AddHitch(
        this IDistributedApplicationBuilder builder,
        Action<IHitchBuilder>? configure = null)
    {
        return AddHitch(builder, "hitch", configure);
    }

    /// <summary>
    /// Adds Hitch plugin configuration to the distributed application with a specific name.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the Hitch resource.</param>
    /// <param name="configure">Optional configuration action for the Hitch builder.</param>
    /// <returns>A resource builder for Hitch configuration.</returns>
    public static IHitchResourceBuilder AddHitch(
        this IDistributedApplicationBuilder builder,
        string name,
        Action<IHitchBuilder>? configure = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        // Create the Hitch resource
        var resource = new HitchResource(name);

        // Create the Aspire-specific builder
        var hitchBuilder = new AspireHitchBuilder(resource);

        // Apply user configuration if provided
        configure?.Invoke(hitchBuilder);

        // Add the resource to the application
        builder.AddResource(resource)
            .WithAnnotation(new HitchConfigurationAnnotation(resource));

        // Create the resource builder and automatically publish configuration
        var resourceBuilder = new HitchResourceBuilder(builder, resource);
        resourceBuilder.PublishAsConfiguration();

        return resourceBuilder;
    }
}

