namespace Hitch;

/// <summary>
/// Marks an assembly as containing a Hitch plugin.
/// The specified type must implement <see cref="IPluginProvider"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class HitchPluginAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the plugin provider.
    /// </summary>
    public Type PluginType { get; }

    /// <summary>
    /// Gets the optional category for the plugin.
    /// </summary>
    public string? Category { get; }

    /// <summary>
    /// Gets the optional subcategory for the plugin.
    /// </summary>
    public string? SubCategory { get; }

    /// <summary>
    /// Gets the stable, first-party identity of this builder within its
    /// <see cref="Category"/>/<see cref="SubCategory"/>. A configured instance routes to this
    /// builder by naming it in the reserved <c>$plugin</c> config key. Required for categorized
    /// plugins; <c>null</c> for uncategorized ones. Must be unique among builders sharing a bucket.
    /// </summary>
    public string? PluginName { get; }

    /// <summary>
    /// Initializes a new uncategorized plugin.
    /// </summary>
    /// <param name="pluginType">The type that implements <see cref="IPluginProvider"/>.</param>
    public HitchPluginAttribute(Type pluginType)
    {
        PluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
    }

    /// <summary>
    /// Initializes a categorized plugin with a required, first-party <paramref name="pluginName"/>.
    /// </summary>
    /// <param name="category">The category for the plugin.</param>
    /// <param name="subCategory">The subcategory for the plugin.</param>
    /// <param name="pluginName">The stable identity used to route configured instances to this builder.</param>
    /// <param name="pluginType">The type that implements <see cref="IPluginProvider"/>.</param>
    public HitchPluginAttribute(string category, string subCategory, string pluginName, Type pluginType)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category cannot be null or whitespace.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(subCategory))
        {
            throw new ArgumentException("SubCategory cannot be null or whitespace.", nameof(subCategory));
        }

        if (string.IsNullOrWhiteSpace(pluginName))
        {
            throw new ArgumentException("PluginName cannot be null or whitespace.", nameof(pluginName));
        }

        Category = category;
        SubCategory = subCategory;
        PluginName = pluginName;
        PluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
    }
}
