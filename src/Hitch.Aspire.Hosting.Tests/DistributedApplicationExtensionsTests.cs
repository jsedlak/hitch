namespace Hitch.Aspire.Hosting.Tests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Hitch.Aspire.Hosting;

public class DistributedApplicationExtensionsTests
{
    [Fact]
    public void AddHitch_CreatesHitchResource()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var resourceBuilder = builder.AddHitch();

        // Assert
        Assert.NotNull(resourceBuilder);
        Assert.NotNull(resourceBuilder.Resource);
        Assert.IsType<HitchResource>(resourceBuilder.Resource);
    }

    [Fact]
    public void AddHitch_WithName_UsesProvidedName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var resourceBuilder = builder.AddHitch("custom-hitch");

        // Assert
        Assert.Equal("custom-hitch", resourceBuilder.Resource.Name);
    }

    [Fact]
    public void AddHitch_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IDistributedApplicationBuilder builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddHitch());
    }

    [Fact]
    public void AddHitch_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddHitch(""));
    }

    [Fact]
    public void AddHitch_WithConfigureAction_InvokesAction()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        bool actionInvoked = false;

        // Act
        var resourceBuilder = builder.AddHitch(hitchBuilder =>
        {
            actionInvoked = true;
            hitchBuilder.WithAssemblies(typeof(DistributedApplicationExtensionsTests).Assembly);
        });

        // Assert
        Assert.True(actionInvoked);
        Assert.NotEmpty(resourceBuilder.Resource.Assemblies);
    }

    [Fact]
    public void AddHitch_WithAssemblies_AddsAssemblyNames()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var resourceBuilder = builder.AddHitch(hitchBuilder =>
        {
            hitchBuilder.WithAssemblies(
                typeof(DistributedApplicationExtensionsTests).Assembly,
                typeof(string).Assembly
            );
        });

        // Assert
        Assert.Contains(typeof(DistributedApplicationExtensionsTests).Assembly.GetName().Name, resourceBuilder.Resource.Assemblies);
        Assert.Contains("System.Private.CoreLib", resourceBuilder.Resource.Assemblies);
    }

    [Fact]
    public void AddHitch_WithFilePattern_AddsPattern()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var resourceBuilder = builder.AddHitch(hitchBuilder =>
        {
            hitchBuilder.WithFilePattern("*.Plugin.dll");
            hitchBuilder.WithFilePattern("*.Hitch.dll");
        });

        // Assert
        Assert.Contains("*.Plugin.dll", resourceBuilder.Resource.FilePatterns);
        Assert.Contains("*.Hitch.dll", resourceBuilder.Resource.FilePatterns);
    }

    [Fact]
    public void AddHitch_WithDuplicateAssemblies_DoesNotAddDuplicates()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var testAssembly = typeof(DistributedApplicationExtensionsTests).Assembly;

        // Act
        var resourceBuilder = builder.AddHitch(hitchBuilder =>
        {
            hitchBuilder.WithAssemblies(testAssembly);
            hitchBuilder.WithAssemblies(testAssembly);
            hitchBuilder.WithAssemblies(testAssembly);
        });

        // Assert
        var assemblyName = testAssembly.GetName().Name!;
        Assert.Equal(1, resourceBuilder.Resource.Assemblies.Count(a => a == assemblyName));
    }

    [Fact]
    public void AddHitch_WithDuplicateFilePatterns_DoesNotAddDuplicates()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var resourceBuilder = builder.AddHitch(hitchBuilder =>
        {
            hitchBuilder.WithFilePattern("*.Plugin.dll");
            hitchBuilder.WithFilePattern("*.Plugin.dll");
            hitchBuilder.WithFilePattern("*.Plugin.dll");
        });

        // Assert
        Assert.Equal(1, resourceBuilder.Resource.FilePatterns.Count(p => p == "*.Plugin.dll"));
    }

    [Fact]
    public void AddHitch_WithNullFilePattern_DoesNotAdd()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var resourceBuilder = builder.AddHitch(hitchBuilder =>
        {
            hitchBuilder.WithFilePattern(null!);
        });

        // Assert
        Assert.Empty(resourceBuilder.Resource.FilePatterns);
    }

    [Fact]
    public void AddHitch_WithEmptyFilePattern_DoesNotAdd()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var resourceBuilder = builder.AddHitch(hitchBuilder =>
        {
            hitchBuilder.WithFilePattern("");
            hitchBuilder.WithFilePattern("   ");
        });

        // Assert
        Assert.Empty(resourceBuilder.Resource.FilePatterns);
    }
}

