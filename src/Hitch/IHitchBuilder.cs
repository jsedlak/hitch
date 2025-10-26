namespace Hitch;

using System.Reflection;

/// <summary>
/// Builder interface for configuring Hitch plugin discovery and loading.
/// </summary>
public interface IHitchBuilder
{
    /// <summary>
    /// Specifies assemblies to scan for Hitch plugins.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>The builder instance for chaining.</returns>
    IHitchBuilder WithAssemblies(params Assembly[] assemblies);

    /// <summary>
    /// Specifies a file pattern for discovering plugin assemblies.
    /// </summary>
    /// <param name="pattern">The file pattern (e.g., "*.Hitch.Plugin.dll").</param>
    /// <returns>The builder instance for chaining.</returns>
    IHitchBuilder WithFilePattern(string pattern);
}

