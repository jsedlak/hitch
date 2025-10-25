namespace Hitch;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Interface that must be implemented by all Hitch plugin providers.
/// </summary>
public interface IPluginProvider
{
    /// <summary>
    /// Called to attach the plugin's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="name">The name of the plugin.</param>
    void Attach(IServiceCollection services, string? name = null);
}

