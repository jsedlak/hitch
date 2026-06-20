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
    /// Gets or sets a stable alias used to route a configured instance to its owning builder
    /// when multiple plugins share the same <see cref="Category"/>/<see cref="SubCategory"/>.
    /// An instance declares its owner via the reserved <c>$plugin</c> config key.
    /// When omitted, the plugin matches on <see cref="Type.FullName"/> (or <see cref="MemberInfo.Name"/>) of <see cref="PluginType"/>.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HitchPluginAttribute"/> class.
    /// </summary>
    /// <param name="pluginType">The type that implements <see cref="IPluginProvider"/>.</param>
    public HitchPluginAttribute(Type pluginType)
    {
        PluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HitchPluginAttribute"/> class with category identification.
    /// </summary>
    /// <param name="category">The category for the plugin.</param>
    /// <param name="subCategory">The subcategory for the plugin.</param>
    /// <param name="pluginType">The type that implements <see cref="IPluginProvider"/>.</param>
    public HitchPluginAttribute(string category, string subCategory, Type pluginType)
    {
        Category = category;
        SubCategory = subCategory;
        PluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
    }
}

