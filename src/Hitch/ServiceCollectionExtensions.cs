namespace Hitch;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to add Hitch plugin support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Hitch plugin support to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add Hitch to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHitch(this IServiceCollection services)
    {
        return AddHitch(services, null);
    }

    /// <summary>
    /// Adds Hitch plugin support to the service collection with configuration.
    /// </summary>
    /// <param name="services">The service collection to add Hitch to.</param>
    /// <param name="configure">Optional configuration action for the Hitch builder.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHitch(
        this IServiceCollection services,
        Action<IHitchBuilder>? configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Get or create configuration from the service collection
        var configuration = GetConfiguration(services);

        // Create the builder
        var builder = new HitchBuilder(services, configuration);

        // Load configuration from Hitch:Configuration section first
        builder.LoadFromConfiguration();

        // Apply user configuration if provided (additive)
        configure?.Invoke(builder);

        // Build and attach all plugins
        builder.Build();

        return services;
    }

    private static IConfiguration GetConfiguration(IServiceCollection services)
    {
        // Try to find an existing IConfiguration in the service collection
        var configurationDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
        
        if (configurationDescriptor?.ImplementationInstance is IConfiguration configuration)
        {
            return configuration;
        }

        // If no configuration is registered, build a temporary service provider to resolve it
        using var serviceProvider = services.BuildServiceProvider();
        var resolvedConfiguration = serviceProvider.GetService<IConfiguration>();
        
        if (resolvedConfiguration != null)
        {
            return resolvedConfiguration;
        }

        // If still no configuration found, return an empty configuration
        return new ConfigurationBuilder().Build();
    }
}

