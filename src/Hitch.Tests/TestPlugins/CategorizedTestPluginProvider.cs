namespace Hitch.Tests.TestPlugins;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Test plugin provider with category for unit testing.
/// </summary>
public class CategorizedTestPluginProvider : IPluginProvider
{
    public static List<string> AttachedNames { get; } = new();
    public static int AttachCallCount { get; private set; }

    public static void Reset()
    {
        AttachedNames.Clear();
        AttachCallCount = 0;
    }

    public void Attach(IServiceCollection services, IConfigurationSection configurationSection, string? name = null)
    {
        AttachCallCount++;
        AttachedNames.Add(name ?? "(null)");
        services.AddSingleton(new CategorizedTestService { Name = name });
    }
}

/// <summary>
/// Test service for categorized plugins.
/// </summary>
public class CategorizedTestService
{
    public string? Name { get; set; }
}

