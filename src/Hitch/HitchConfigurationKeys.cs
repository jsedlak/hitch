namespace Hitch;

/// <summary>
/// Reserved configuration keys that Hitch reads from a categorized plugin instance's section.
/// </summary>
public static class HitchConfigurationKeys
{
    /// <summary>
    /// The key naming the instance's owning builder (its <see cref="HitchPluginAttribute.PluginName"/>).
    /// Uses only letters, so it is valid in environment variable names on Azure Container Apps and
    /// Kubernetes (<c>^[-._a-zA-Z][-._a-zA-Z0-9]*$</c>). Preferred over <see cref="LegacyPlugin"/>.
    /// </summary>
    public const string Plugin = "HitchPlugin";

    /// <summary>
    /// The original spelling of <see cref="Plugin"/>. Still accepted, but <c>$</c> is not valid in
    /// environment variable names on Azure Container Apps or Kubernetes.
    /// </summary>
    public const string LegacyPlugin = "$plugin";
}
