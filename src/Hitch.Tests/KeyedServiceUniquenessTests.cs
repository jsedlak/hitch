namespace Hitch.Tests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hitch.Tests.TestPlugins;

/// <summary>
/// Validates that the instance <c>name</c> (the keyed-service key) is the uniqueness factor:
/// two registrations under the same (Category, SubCategory, PluginName) but different names
/// coexist, and two different builders under the same bucket also coexist — no clobbering.
/// </summary>
[Collection("Hitch Tests")]
public class KeyedServiceUniquenessTests
{
    private static ServiceProvider Build(Dictionary<string, string?> config)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddHitch(b => b.WithAssemblies(typeof(KeyedServiceUniquenessTests).Assembly));
        return services.BuildServiceProvider();
    }

    [Fact]
    public void SamePluginNameTwoNamesRegisterTwoDistinctKeyedServices()
    {
        // Same builder (KeyedTool), two different instance names → two distinct keyed services.
        using var sp = Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Keyed:Tools:tool-prod:$plugin"] = "KeyedTool",
            ["Hitch:Plugins:Keyed:Tools:tool-dev:$plugin"] = "KeyedTool",
        });

        var prod = sp.GetRequiredKeyedService<IKeyedToolProvider>("tool-prod");
        var dev = sp.GetRequiredKeyedService<IKeyedToolProvider>("tool-dev");

        Assert.NotSame(prod, dev);
        Assert.Equal("Keyed", prod.Kind);
        Assert.Equal("Keyed", dev.Kind);
        Assert.Equal("tool-prod", prod.Name);
        Assert.Equal("tool-dev", dev.Name);
    }

    [Fact]
    public void DifferentBuildersDifferentNamesDoNotClobber()
    {
        // Two different builders sharing the bucket, each with its own instance name.
        using var sp = Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Keyed:Tools:datetime:$plugin"] = "KeyedTool",
            ["Hitch:Plugins:Keyed:Tools:mcp:$plugin"] = "OtherTool",
        });

        var datetime = sp.GetRequiredKeyedService<IKeyedToolProvider>("datetime");
        var mcp = sp.GetRequiredKeyedService<IKeyedToolProvider>("mcp");

        Assert.Equal("Keyed", datetime.Kind);   // routed to KeyedToolProviderBuilder
        Assert.Equal("Other", mcp.Kind);         // routed to OtherKeyedToolProviderBuilder
    }
}
