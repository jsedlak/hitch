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
    public void InstanceWithoutOwnerInSharedSubcategoryIsSkipped()
    {
        Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:orphan:SomeKey"] = "value",
        });

        Assert.Equal(0, AlphaProviderBuilder.AttachCallCount);
        Assert.Equal(0, BetaProviderBuilder.AttachCallCount);
    }

    [Fact]
    public void UnknownOwnerIsSkipped()
    {
        Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:weird:$plugin"] = "DoesNotExist",
        });

        Assert.Equal(0, AlphaProviderBuilder.AttachCallCount);
        Assert.Equal(0, BetaProviderBuilder.AttachCallCount);
    }

    [Fact]
    public void OwnerMatchesOnTypeNameWhenAliasFallbackUsed()
    {
        // "BetaProviderBuilder" is the CLR type name; aliases are declared, but the type name
        // remains a valid fallback identifier.
        Build(new Dictionary<string, string?>
        {
            ["Hitch:Plugins:Shared:Providers:by-type:$plugin"] = "BetaProviderBuilder",
        });

        Assert.Equal(new[] { "by-type" }, BetaProviderBuilder.AttachedNames);
        Assert.Equal(0, AlphaProviderBuilder.AttachCallCount);
    }
}
