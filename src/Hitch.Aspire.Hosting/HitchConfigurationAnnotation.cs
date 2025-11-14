namespace Aspire.Hosting;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Annotation that generates configuration for Hitch plugins.
/// </summary>
internal class HitchConfigurationAnnotation : IResourceAnnotation
{
    private readonly HitchResource _resource;

    public HitchConfigurationAnnotation(HitchResource resource)
    {
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
    }

    /// <summary>
    /// Gets the Hitch resource.
    /// </summary>
    public HitchResource Resource => _resource;
}

