namespace Hitch.Tests.TestPlugins;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Two plugin providers that deliberately share the same (Category, SubCategory) to exercise
/// per-instance routing via the reserved <c>$plugin</c> discriminator.
/// </summary>
public class AlphaProviderBuilder : IPluginProvider
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
    }
}

public class BetaProviderBuilder : IPluginProvider
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
    }
}
