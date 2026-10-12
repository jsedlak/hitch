namespace Hitch.Aspire.Hosting.Tests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Hitch.Aspire.Hosting;

public class HitchResourceBuilderExtensionsTests
{
    [Fact]
    public void WithPlugin_AddsPluginEntry()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        resourceBuilder.WithPlugin("Database", "Postgres", "Service1", "Pg");

        // Assert
        var key = "Database__Postgres";
        Assert.True(resourceBuilder.Resource.Plugins.ContainsKey(key));
        Assert.Contains("Service1", resourceBuilder.Resource.Plugins[key]);
    }

    [Fact]
    public void WithPlugin_MultipleServiceNames_AddsAll()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        resourceBuilder
            .WithPlugin("Database", "Postgres", "Service1", "Pg")
            .WithPlugin("Database", "Postgres", "Service2", "Pg")
            .WithPlugin("Database", "Postgres", "Service3", "Pg");

        // Assert
        var key = "Database__Postgres";
        Assert.Equal(3, resourceBuilder.Resource.Plugins[key].Count);
        Assert.Contains("Service1", resourceBuilder.Resource.Plugins[key]);
        Assert.Contains("Service2", resourceBuilder.Resource.Plugins[key]);
        Assert.Contains("Service3", resourceBuilder.Resource.Plugins[key]);
    }

    [Fact]
    public void WithPlugin_DuplicateServiceName_DoesNotAddDuplicate()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        resourceBuilder
            .WithPlugin("Database", "Postgres", "Service1", "Pg")
            .WithPlugin("Database", "Postgres", "Service1", "Pg")
            .WithPlugin("Database", "Postgres", "Service1", "Pg");

        // Assert
        var key = "Database__Postgres";
        Assert.Equal(1, resourceBuilder.Resource.Plugins[key].Count);
        Assert.Contains("Service1", resourceBuilder.Resource.Plugins[key]);
    }

    [Fact]
    public void WithPlugin_DifferentCategories_CreatesMultipleKeys()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        resourceBuilder
            .WithPlugin("Database", "Postgres", "PgService", "Pg")
            .WithPlugin("Database", "MySQL", "MySqlService", "MySql")
            .WithPlugin("Storage", "S3", "S3Service", "S3");

        // Assert
        Assert.Equal(3, resourceBuilder.Resource.Plugins.Count);
        Assert.True(resourceBuilder.Resource.Plugins.ContainsKey("Database__Postgres"));
        Assert.True(resourceBuilder.Resource.Plugins.ContainsKey("Database__MySQL"));
        Assert.True(resourceBuilder.Resource.Plugins.ContainsKey("Storage__S3"));
    }

    [Fact]
    public void WithPlugin_NullCategory_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin(null!, "SubCategory", "Service", "Plug"));
    }

    [Fact]
    public void WithPlugin_NullSubCategory_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", null!, "Service", "Plug"));
    }

    [Fact]
    public void WithPlugin_NullServiceName_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "SubCategory", null!, "Plug"));
    }

    [Fact]
    public void WithPlugin_NullPlugin_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "SubCategory", "Service", null!));
    }

    [Fact]
    public void WithPlugin_EmptyCategory_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("", "SubCategory", "Service", "Plug"));
    }

    [Fact]
    public void WithPlugin_EmptySubCategory_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "", "Service", "Plug"));
    }

    [Fact]
    public void WithPlugin_EmptyServiceName_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "SubCategory", "", "Plug"));
    }

    [Fact]
    public void WithPlugin_EmptyPlugin_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "SubCategory", "Service", ""));
    }

    [Fact]
    public void WithPlugin_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        var result = resourceBuilder.WithPlugin("Database", "Postgres", "Service1", "Pg");

        // Assert
        Assert.Same(resourceBuilder, result);
    }

    [Fact]
    public void WithPlugin_StampsOwnerDiscriminator()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        resourceBuilder.WithPlugin("Covalent", "Responses", "azure-foundry", "Foundry");

        // Assert
        var configKey = "Covalent__Responses__azure-foundry";
        Assert.True(resourceBuilder.Resource.PluginConfigurations.ContainsKey(configKey));
        var config = resourceBuilder.Resource.PluginConfigurations[configKey];
        Assert.Equal("Foundry", config["HitchPlugin"]);
        Assert.False(config.ContainsKey("$plugin"));
    }

    [Fact]
    public void WithPlugin_DropsLegacyOwnerKeyFromConfigurations()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        resourceBuilder.WithPlugin("Covalent", "Responses", "azure-foundry", "Foundry",
            new Dictionary<string, object> { ["$plugin"] = "Foundry", ["Endpoint"] = "https://example" });

        // Assert
        var config = resourceBuilder.Resource.PluginConfigurations["Covalent__Responses__azure-foundry"];
        Assert.False(config.ContainsKey("$plugin"));
        Assert.Equal("Foundry", config["HitchPlugin"]);
        Assert.Equal("https://example", config["Endpoint"]);
    }

    [Fact]
    public async Task WithReference_ExportsOnlyContainerAppsAndKubernetesSafeEnvVarNames()
    {
        // Azure Container Apps and Kubernetes reject env var names outside this pattern. Guards
        // against any future reserved key reintroducing an invalid character (as '$plugin' did).
        var validName = new System.Text.RegularExpressions.Regex("^[-._a-zA-Z][-._a-zA-Z0-9]*$");

        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var hitch = builder.AddHitch(h => h.WithAssemblies(typeof(HitchResourceBuilderExtensionsTests).Assembly).WithFilePattern("*.Plugin.dll"))
            .WithPlugin("Covalent", "Responses", "azure-foundry", "Foundry",
                new Dictionary<string, object> { ["Endpoint"] = "https://example" })
            .WithPlugin("Database", "Postgres", "views", "Pg");
        var app = builder.AddContainer("app", "image").WithReference(hitch);

        // Act
        var env = await app.Resource.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Publish);

        // Assert
        Assert.Contains("Hitch__Plugins__Covalent__Responses__azure-foundry__HitchPlugin", env.Keys);
        Assert.All(env.Keys, key => Assert.Matches(validName, key));
    }
}
