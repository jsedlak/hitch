namespace Hitch.Tests.TestPlugins;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Test plugin provider that throws an exception.
/// </summary>
public class FailingPluginProvider : IPluginProvider
{
    public void Attach(IServiceCollection services, IConfigurationSection configurationSection, string? name = null)
    {
        throw new InvalidOperationException("Test exception from plugin");
    }
}

