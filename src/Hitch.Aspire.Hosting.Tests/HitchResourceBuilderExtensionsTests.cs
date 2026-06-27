namespace Hitch.Aspire.Hosting.Tests;

using global::Aspire.Hosting;
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
        Assert.Equal("Foundry", resourceBuilder.Resource.PluginConfigurations[configKey]["$plugin"]);
    }
}
