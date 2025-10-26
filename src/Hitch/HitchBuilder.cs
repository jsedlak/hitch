namespace Hitch;

using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Internal builder class for configuring and building Hitch plugin infrastructure.
/// </summary>
internal sealed class HitchBuilder : IHitchBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly List<Assembly> _assemblies = new();
    private readonly List<string> _filePatterns = new();

    public HitchBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public IHitchBuilder WithAssemblies(params Assembly[] assemblies)
    {
        if (assemblies != null)
        {
            _assemblies.AddRange(assemblies);
        }
        return this;
    }

    public IHitchBuilder WithFilePattern(string pattern)
    {
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            _filePatterns.Add(pattern);
        }
        return this;
    }

    /// <summary>
    /// Builds and attaches all discovered Hitch plugins.
    /// </summary>
    public void Build()
    {
        // Collect all assemblies to scan
        var assembliesToScan = new HashSet<Assembly>(_assemblies);

        // If no assemblies were explicitly specified, scan all loaded assemblies
        if (assembliesToScan.Count == 0)
        {
            assembliesToScan.UnionWith(AppDomain.CurrentDomain.GetAssemblies());
        }

        // Load assemblies from file patterns
        foreach (var pattern in _filePatterns)
        {
            LoadAssembliesFromPattern(pattern, assembliesToScan);
        }

        // Get all plugin attributes from assemblies
        var plugins = new List<(HitchPluginAttribute Attribute, Assembly Assembly)>();
        foreach (var assembly in assembliesToScan)
        {
            try
            {
                var attributes = assembly.GetCustomAttributes<HitchPluginAttribute>();
                foreach (var attr in attributes)
                {
                    plugins.Add((attr, assembly));
                }
            }
            catch (Exception ex)
            {
                // Skip assemblies that fail to load attributes
                Console.Error.WriteLine($"[Hitch] Error loading attributes from assembly '{assembly.FullName}': {ex.Message}");
                continue;
            }
        }

        // Load configuration section for Hitch plugins
        var hitchConfigSection = _configuration.GetSection("Hitch:Plugins");

        // Process plugins
        foreach (var (attribute, assembly) in plugins)
        {
            // Case 1: No category - attach with null name
            if (string.IsNullOrEmpty(attribute.Category))
            {
                AttachPlugin(attribute.PluginType, null);
            }
            // Case 2: Has category - look for matching configuration
            else
            {
                ProcessCategorizedPlugin(attribute, hitchConfigSection);
            }
        }
    }

    private void ProcessCategorizedPlugin(HitchPluginAttribute attribute, IConfigurationSection hitchConfigSection)
    {
        // Look for configuration in structure: Hitch:Plugins:[CATEGORY]:[SUBCATEGORY] = [SERVICE NAMES ARRAY]
        if (string.IsNullOrEmpty(attribute.Category) || string.IsNullOrEmpty(attribute.SubCategory))
        {
            return;
        }

        // Navigate to Hitch:Plugins:[CATEGORY]
        var categorySection = hitchConfigSection.GetSection(attribute.Category);
        if (!categorySection.Exists())
        {
            return;
        }

        // Get the array of service names at [SUBCATEGORY] key
        var subCategorySection = categorySection.GetSection(attribute.SubCategory);
        if (!subCategorySection.Exists())
        {
            return;
        }

        // The subcategory section should be an array
        var serviceNames = subCategorySection.Get<string[]>();
        if (serviceNames != null)
        {
            foreach (var serviceName in serviceNames)
            {
                if (!string.IsNullOrEmpty(serviceName))
                {
                    AttachPlugin(attribute.PluginType, serviceName);
                }
            }
        }
    }

    private void AttachPlugin(Type pluginType, string? name)
    {
        try
        {
            // Instantiate the plugin provider
            var pluginInstance = Activator.CreateInstance(pluginType);
            
            if (pluginInstance is IPluginProvider provider)
            {
                provider.Attach(_services, name);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Plugin type '{pluginType.FullName}' does not implement IPluginProvider.");
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to attach plugin of type '{pluginType.FullName}' with name '{name ?? "(null)"}'.";
            Console.Error.WriteLine($"[Hitch] {errorMessage}");
            Console.Error.WriteLine($"[Hitch] Exception: {ex}");
            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    private void LoadAssembliesFromPattern(string pattern, HashSet<Assembly> assemblies)
    {
        try
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var matchingFiles = Directory.GetFiles(currentDirectory, pattern, SearchOption.AllDirectories);

            foreach (var file in matchingFiles)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                    assemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    // Skip files that cannot be loaded as assemblies
                    Console.Error.WriteLine($"[Hitch] Error loading assembly from file '{file}': {ex.Message}");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            // Skip if directory or pattern is invalid
            Console.Error.WriteLine($"[Hitch] Error loading assemblies from pattern '{pattern}': {ex.Message}");
        }
    }
}

