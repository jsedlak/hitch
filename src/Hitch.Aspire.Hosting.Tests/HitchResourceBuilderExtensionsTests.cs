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
        resourceBuilder.WithPlugin("Database", "Postgres", "Service1");

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
            .WithPlugin("Database", "Postgres", "Service1")
            .WithPlugin("Database", "Postgres", "Service2")
            .WithPlugin("Database", "Postgres", "Service3");

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
            .WithPlugin("Database", "Postgres", "Service1")
            .WithPlugin("Database", "Postgres", "Service1")
            .WithPlugin("Database", "Postgres", "Service1");

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
            .WithPlugin("Database", "Postgres", "PgService")
            .WithPlugin("Database", "MySQL", "MySqlService")
            .WithPlugin("Storage", "S3", "S3Service");

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
            resourceBuilder.WithPlugin(null!, "SubCategory", "Service"));
    }

    [Fact]
    public void WithPlugin_NullSubCategory_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", null!, "Service"));
    }

    [Fact]
    public void WithPlugin_NullServiceName_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "SubCategory", null!));
    }

    [Fact]
    public void WithPlugin_EmptyCategory_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("", "SubCategory", "Service"));
    }

    [Fact]
    public void WithPlugin_EmptySubCategory_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "", "Service"));
    }

    [Fact]
    public void WithPlugin_EmptyServiceName_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            resourceBuilder.WithPlugin("Category", "SubCategory", ""));
    }

    [Fact]
    public void WithPlugin_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch();

        // Act
        var result = resourceBuilder.WithPlugin("Database", "Postgres", "Service1");

        // Assert
        Assert.Same(resourceBuilder, result);
    }

    [Fact]
    public void PublishAsConfiguration_AddsEnvironmentCallbackAnnotation()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddHitch(b =>
        {
            b.WithAssemblies(typeof(HitchResourceBuilderExtensionsTests).Assembly);
            b.WithFilePattern("*.dll");
        }).WithPlugin("Database", "Postgres", "Service1");

        // Act
        var result = resourceBuilder.PublishAsConfiguration();

        // Assert
        Assert.NotNull(result);
        Assert.Same(resourceBuilder, result);
        // Verify annotation was added
        var annotations = resourceBuilder.Resource.Annotations;
        Assert.NotEmpty(annotations);
    }

    [Fact]
    public void PublishAsConfiguration_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IHitchResourceBuilder builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.PublishAsConfiguration());
    }
}

