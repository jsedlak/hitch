namespace Aspire.Hosting;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Implementation of IHitchResourceBuilder.
/// </summary>
internal class HitchResourceBuilder : IHitchResourceBuilder
{
    public IDistributedApplicationBuilder ApplicationBuilder { get; }
    public HitchResource Resource { get; }

    public HitchResourceBuilder(IDistributedApplicationBuilder applicationBuilder, HitchResource resource)
    {
        ApplicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
    }

    public IResourceBuilder<HitchResource> WithAnnotation<TAnnotation>(
        TAnnotation annotation,
        ResourceAnnotationMutationBehavior behavior = ResourceAnnotationMutationBehavior.Append) 
        where TAnnotation : IResourceAnnotation
    {
        Resource.Annotations.Add(annotation);
        return this;
    }
}

