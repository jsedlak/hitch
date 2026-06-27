namespace Hitch.Tests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hitch.Tests.TestPlugins;

[Collection("Hitch Tests")]
public class SharedSubcategoryRoutingTests : IDisposable
{
    public SharedSubcategoryRoutingTests()
    {
        AlphaProviderBuilder.Reset();
        BetaProviderBuilder.Reset();
    }

    public void Dispose()
    {
        AlphaProviderBuilder.Reset();
        BetaProviderBuilder.Reset();
    }

    private static void Build(Dictionary<string, string?> config)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(SharedSubcategoryRoutingTests).Assembly);
        });
    }

    [Fact]
    public void EachInstanceIsRoutedToOnlyItsOwningBuilder()
    {
        Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:alpha-instance:$plugin"] = "Alpha",
            ["Hitch:Plugins:Shared:Providers:beta-instance:$plugin"] = "Beta",
        });

        // Each builder attaches exactly the instance it owns — no cross-product.
        Assert.Equal(new[] { "alpha-instance" }, AlphaProviderBuilder.AttachedNames);
        Assert.Equal(new[] { "beta-instance" }, BetaProviderBuilder.AttachedNames);
        Assert.Equal(1, AlphaProviderBuilder.AttachCallCount);
        Assert.Equal(1, BetaProviderBuilder.AttachCallCount);
    }

    [Fact]
    public void InstanceWithoutOwnerThrows()
    {
        // Every categorized instance must name its owner via $plugin — absence is a loud error.
        var ex = Assert.Throws<InvalidOperationException>(() => Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:orphan:SomeKey"] = "value",
        }));

        Assert.Contains("declares no '$plugin'", ex.Message);
        Assert.Equal(0, AlphaProviderBuilder.AttachCallCount);
        Assert.Equal(0, BetaProviderBuilder.AttachCallCount);
    }

    [Fact]
    public void UnknownOwnerThrows()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:weird:$plugin"] = "DoesNotExist",
        }));

        Assert.Contains("not registered for this bucket", ex.Message);
        Assert.Equal(0, AlphaProviderBuilder.AttachCallCount);
        Assert.Equal(0, BetaProviderBuilder.AttachCallCount);
    }

    [Fact]
    public void TypeNameIsNoLongerAcceptedAsOwner()
    {
        // PluginName is the only routing identity; the CLR type name is no longer a fallback.
        Assert.Throws<InvalidOperationException>(() => Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:by-type:$plugin"] = "BetaProviderBuilder",
        }));
    }
}
