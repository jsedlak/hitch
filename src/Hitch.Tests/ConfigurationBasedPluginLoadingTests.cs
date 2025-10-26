namespace Hitch.Tests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hitch.Tests.TestPlugins;

[Collection("Hitch Tests")]
public class ConfigurationBasedPluginLoadingTests : IDisposable
{
    public ConfigurationBasedPluginLoadingTests()
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
    public void AddHitch_WithMatchingConfiguration_AttachesPluginWithServiceName()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Plugins:Database:Postgres:0"] = "MyPostgresConnection"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(ConfigurationBasedPluginLoadingTests).Assembly);
        });

        // Assert
        Assert.Contains("MyPostgresConnection", CategorizedTestPluginProvider.AttachedNames);
        var serviceProvider = services.BuildServiceProvider();
        var testServices = serviceProvider.GetServices<CategorizedTestService>().ToList();
        Assert.Contains(testServices, s => s.Name == "MyPostgresConnection");
    }

    [Fact]
    public void AddHitch_WithMultipleServiceNames_AttachesPluginMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Plugins:Database:Postgres:0"] = "Connection1",
                ["Hitch:Plugins:Database:Postgres:1"] = "Connection2",
                ["Hitch:Plugins:Database:Postgres:2"] = "Connection3"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(ConfigurationBasedPluginLoadingTests).Assembly);
        });

        // Assert
        Assert.Contains("Connection1", CategorizedTestPluginProvider.AttachedNames);
        Assert.Contains("Connection2", CategorizedTestPluginProvider.AttachedNames);
        Assert.Contains("Connection3", CategorizedTestPluginProvider.AttachedNames);
        Assert.Equal(3, CategorizedTestPluginProvider.AttachCallCount);
    }

    [Fact]
    public void AddHitch_WithNonMatchingConfiguration_DoesNotAttachPlugin()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Plugins:Storage:S3:0"] = "MyS3Connection" // Different category
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(ConfigurationBasedPluginLoadingTests).Assembly);
        });

        // Assert
        Assert.DoesNotContain("MyS3Connection", CategorizedTestPluginProvider.AttachedNames);
        Assert.Equal(0, CategorizedTestPluginProvider.AttachCallCount);
    }

    [Fact]
    public void AddHitch_WithNoConfiguration_DoesNotAttachCategorizedPlugin()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(ConfigurationBasedPluginLoadingTests).Assembly);
        });

        // Assert
        Assert.Equal(0, CategorizedTestPluginProvider.AttachCallCount);
    }

    [Fact]
    public void AddHitch_WithPartialMatchingConfiguration_DoesNotAttachPlugin()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Plugins:Database:MySQL:0"] = "MyMySQLConnection" // Wrong subcategory
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(ConfigurationBasedPluginLoadingTests).Assembly);
        });

        // Assert
        Assert.DoesNotContain("MyMySQLConnection", CategorizedTestPluginProvider.AttachedNames);
        Assert.Equal(0, CategorizedTestPluginProvider.AttachCallCount);
    }

    [Fact]
    public void AddHitch_WithEmptyServiceName_DoesNotAttachPlugin()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Plugins:Database:Postgres:0"] = "" // Empty service name
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddHitch(builder =>
        {
            builder.WithAssemblies(typeof(ConfigurationBasedPluginLoadingTests).Assembly);
        });

        // Assert
        Assert.DoesNotContain("", CategorizedTestPluginProvider.AttachedNames);
        Assert.Equal(0, CategorizedTestPluginProvider.AttachCallCount);
    }

    [Fact]
    public void AddHitch_WithJsonConfiguration_LoadsPluginsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var jsonConfig = @"
{
  ""Hitch"": {
    ""Plugins"": {
      ""Database"": {
        ""Postgres"": [""ServiceA"", ""ServiceB""]
      }
    }
  }
}";
        var configPath = Path.Combine(Path.GetTempPath(), $"appsettings-{Guid.NewGuid()}.json");
        File.WriteAllText(configPath, jsonConfig);

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Act
            services.AddHitch(builder =>
            {
                builder.WithAssemblies(typeof(ConfigurationBasedPluginLoadingTests).Assembly);
            });

            // Assert
            Assert.Contains("ServiceA", CategorizedTestPluginProvider.AttachedNames);
            Assert.Contains("ServiceB", CategorizedTestPluginProvider.AttachedNames);
            Assert.Equal(2, CategorizedTestPluginProvider.AttachCallCount);
        }
        finally
        {
            // Cleanup
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
        }
    }

    [Fact]
    public void AddHitch_WithBothCategorizedAndUncategorized_LoadsBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hitch:Plugins:Database:Postgres:0"] = "MyConnection"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act - Don't specify assemblies, let it scan all loaded assemblies
        services.AddHitch();

        // Assert
        // TestPluginProvider should be loaded (uncategorized)
        Assert.Contains("(null)", TestPluginProvider.AttachedNames);
        
        // CategorizedTestPluginProvider should be loaded with service name
        Assert.Contains("MyConnection", CategorizedTestPluginProvider.AttachedNames);
    }
}

