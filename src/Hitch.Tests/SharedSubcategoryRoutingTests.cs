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
        // Every categorized instance must name its owner via HitchPlugin — absence is a loud error.
        var ex = Assert.Throws<InvalidOperationException>(() => Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:orphan:SomeKey"] = "value",
        }));

        Assert.Contains("declares no 'HitchPlugin' (or '$plugin')", ex.Message);
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
    public void InstanceWithOnlyHitchPluginKeyIsRouted()
    {
        Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:alpha-instance:HitchPlugin"] = "Alpha",
            ["Hitch:Plugins:Shared:Providers:beta-instance:HitchPlugin"] = "Beta",
        });

        Assert.Equal(new[] { "alpha-instance" }, AlphaProviderBuilder.AttachedNames);
        Assert.Equal(new[] { "beta-instance" }, BetaProviderBuilder.AttachedNames);
    }

    [Fact]
    public void HitchPluginKeyFromEnvironmentVariablesIsRouted()
    {
        // This is the shape Hitch.Aspire.Hosting injects; it must survive the env var provider.
        const string prefix = "HITCH_ROUTING_TEST_";
        var name = $"{prefix}Hitch__Plugins__Shared__Providers__alpha-instance__HitchPlugin";
        Environment.SetEnvironmentVariable(name, "Alpha");
        try
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables(prefix).Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHitch(builder => builder.WithAssemblies(typeof(SharedSubcategoryRoutingTests).Assembly));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }

        Assert.Equal(new[] { "alpha-instance" }, AlphaProviderBuilder.AttachedNames);
        Assert.Equal(0, BetaProviderBuilder.AttachCallCount);
    }

    [Fact]
    public void BothKeysAgreeingIsRouted()
    {
        Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:alpha-instance:HitchPlugin"] = "Alpha",
            ["Hitch:Plugins:Shared:Providers:alpha-instance:$plugin"] = "alpha",
        });

        Assert.Equal(new[] { "alpha-instance" }, AlphaProviderBuilder.AttachedNames);
        Assert.Equal(0, BetaProviderBuilder.AttachCallCount);
    }

    [Fact]
    public void BothKeysDisagreeingThrowsNamingBoth()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:split:HitchPlugin"] = "Alpha",
            ["Hitch:Plugins:Shared:Providers:split:$plugin"] = "Beta",
        }));

        Assert.Contains("'HitchPlugin' = 'Alpha'", ex.Message);
        Assert.Contains("'$plugin' = 'Beta'", ex.Message);
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
