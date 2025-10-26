namespace Hitch.Tests.TestPlugins;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Test plugin provider that throws an exception.
/// </summary>
public class FailingPluginProvider : IPluginProvider
{
    public void Attach(IServiceCollection services, string? name = null)
    {
        throw new InvalidOperationException("Test exception from plugin");
    }
}

