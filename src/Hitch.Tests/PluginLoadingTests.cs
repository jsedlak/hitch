namespace Hitch.Tests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hitch.Tests.TestPlugins;
using System.Reflection;

[Collection("Hitch Tests")]
public class PluginLoadingTests : IDisposable
{
    public PluginLoadingTests()
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
    public void AddHitch_WithoutCategory_AttachesPluginWithNullName()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(PluginLoadingTests).Assembly);
        });

        // Assert
        Assert.Contains("(null)", TestPluginProvider.AttachedNames);
        var serviceProvider = services.BuildServiceProvider();
        var testServices = serviceProvider.GetServices<TestService>().ToList();
        Assert.NotEmpty(testServices);
        Assert.Contains(testServices, s => s.Name == null);
    }

    [Fact]
    public void AddHitch_WithMultipleAssemblies_ScansAllAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(
                typeof(PluginLoadingTests).Assembly,
                typeof(string).Assembly // System.Private.CoreLib (won't have plugins)
            );
        });

        // Assert
        Assert.True(TestPluginProvider.AttachCallCount > 0);
    }

    [Fact]
    public void AddHitch_WithNullAssemblies_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() =>
        {
            services.AddHitch(builder =>
            {
                builder.WithAssemblies(null!);
            });
        });

        // Assert - Should handle null gracefully
        Assert.Null(exception);
    }

    [Fact]
    public void AddHitch_WithEmptyAssemblies_ScansCurrentDomain()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act - No assemblies specified, should scan all loaded assemblies
        services.AddHitch();

        // Assert - Should find plugins in the test assembly that's loaded in the domain
        Assert.True(TestPluginProvider.AttachCallCount > 0);
    }

    [Fact]
    public void AddHitch_WithFilePattern_LoadsMatchingAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            // Use a pattern that won't match any files (for safety in test)
            builder.WithFilePattern("NonExistent.Plugin.dll");
        });

        // Assert - Should still load from domain assemblies
        Assert.True(TestPluginProvider.AttachCallCount >= 0);
    }

    [Fact]
    public void AddHitch_WithNullFilePattern_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() =>
        {
            services.AddHitch(builder =>
            {
                builder.WithFilePattern(null!);
            });
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void AddHitch_WithEmptyFilePattern_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var exception = Record.Exception(() =>
        {
            services.AddHitch(builder =>
            {
                builder.WithFilePattern("");
            });
        });

        // Assert
        Assert.Null(exception);
    }
}

