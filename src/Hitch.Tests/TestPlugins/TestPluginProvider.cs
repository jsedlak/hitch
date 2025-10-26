namespace Hitch.Tests.TestPlugins;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Test plugin provider for unit testing.
/// </summary>
public class TestPluginProvider : IPluginProvider
{
    public static List<string> AttachedNames { get; } = new();
    public static int AttachCallCount { get; private set; }

    public static void Reset()
    {
        AttachedNames.Clear();
        AttachCallCount = 0;
    }

    public void Attach(IServiceCollection services, string? name = null)
    {
        AttachCallCount++;
        AttachedNames.Add(name ?? "(null)");
        services.AddSingleton(new TestService { Name = name });
    }
}

/// <summary>
/// Test service for verification.
/// </summary>
public class TestService
{
    public string? Name { get; set; }
}

