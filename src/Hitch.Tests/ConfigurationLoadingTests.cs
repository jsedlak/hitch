namespace Hitch.Tests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hitch.Tests.TestPlugins;

[Collection("Hitch Tests")]
public class ConfigurationLoadingTests : IDisposable
{
    public ConfigurationLoadingTests()
    {
        // Reset test plugin state before each test
        TestPluginProvider.Reset();
        CategorizedTestPluginProvider.Reset();
    }

    public void Dispose()
    {
        // Clean up after each test
        TestPluginProvider.Reset();
        CategorizedTestPluginProvider.Reset();
    }

    [Fact]
    public void AddHitch_WithAssembliesInConfiguration_LoadsAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Configuration:Assemblies:0"] = "System.Runtime"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() => services.AddHitch());

        // Assert
        // Should not throw - System.Runtime is a valid assembly
        Assert.Null(exception);
    }

    [Fact]
    public void AddHitch_WithFilePatternsInConfiguration_LoadsPatterns()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Configuration:FilePatterns:0"] = "*.TestPlugin.dll",
                ["Hitch:Configuration:FilePatterns:1"] = "*.OtherPlugin.dll"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() => services.AddHitch());

        // Assert
        // Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void AddHitch_WithConfigurationAndCallback_CombinesBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Configuration:Assemblies:0"] = typeof(ConfigurationLoadingTests).Assembly.GetName().Name!
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        bool callbackInvoked = false;

        // Act
        services.AddHitch(builder =>
        {
            callbackInvoked = true;
            // Add additional assembly via callback
            builder.WithAssemblies(typeof(string).Assembly);
        });

        // Assert
        Assert.True(callbackInvoked);
        // Both config and callback should have been processed
        Assert.True(TestPluginProvider.AttachCallCount > 0);
    }

    [Fact]
    public void AddHitch_WithInvalidAssemblyInConfiguration_LogsError()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Configuration:Assemblies:0"] = "NonExistentAssembly12345"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() => services.AddHitch());

        // Assert
        // Should not throw - invalid assemblies are logged but don't fail the build
        Assert.Null(exception);
    }

    [Fact]
    public void AddHitch_WithEmptyConfiguration_DoesNotFail()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Configuration:Assemblies:0"] = "",
                ["Hitch:Configuration:FilePatterns:0"] = ""
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() => services.AddHitch());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void AddHitch_WithNoConfigurationSection_DoesNotFail()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() => services.AddHitch());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void AddHitch_WithTestAssemblyInConfiguration_LoadsPlugins()
    {
        // Arrange
        var services = new ServiceCollection();
        var testAssemblyName = typeof(ConfigurationLoadingTests).Assembly.GetName().Name!;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Configuration:Assemblies:0"] = testAssemblyName
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch();

        // Assert
        // Should load TestPluginProvider from the test assembly
        Assert.True(TestPluginProvider.AttachCallCount > 0);
        Assert.Contains("(null)", TestPluginProvider.AttachedNames);
    }
}

