namespace Aspire.Hosting;

using Hitch;
using System.Reflection;

/// <summary>
/// Internal builder for configuring Hitch in Aspire applications.
/// </summary>
internal class AspireHitchBuilder : IHitchBuilder
{
    private readonly HitchResource _resource;

    public AspireHitchBuilder(HitchResource resource)
    {
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
    }

    public IHitchBuilder WithAssemblies(params Assembly[] assemblies)
    {
        if (assemblies != null)
        {
            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName().Name;
                if (!string.IsNullOrEmpty(assemblyName) && !_resource.Assemblies.Contains(assemblyName))
                {
                    _resource.Assemblies.Add(assemblyName);
                }
            }
        }
        return this;
    }

    public IHitchBuilder WithFilePattern(string pattern)
    {
        if (!string.IsNullOrWhiteSpace(pattern) && !_resource.FilePatterns.Contains(pattern))
        {
            _resource.FilePatterns.Add(pattern);
        }
        return this;
    }
}

