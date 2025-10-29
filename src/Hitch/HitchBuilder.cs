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
    /// Loads configuration from the Hitch:Configuration section.
    /// </summary>
    public void LoadFromConfiguration()
    {
        var configSection = _configuration.GetSection("Hitch:Configuration");
        if (!configSection.Exists())
        {
            return;
        }

        // Load assemblies from configuration
        var assembliesConfig = configSection.GetSection("Assemblies");
        if (assembliesConfig.Exists())
        {
            var assemblyNames = assembliesConfig.Get<string[]>();
            if (assemblyNames != null)
            {
                foreach (var assemblyName in assemblyNames)
                {
                    if (!string.IsNullOrWhiteSpace(assemblyName))
                    {
                        try
                        {
                            var assembly = Assembly.Load(assemblyName);
                            if (!_assemblies.Contains(assembly))
                            {
                                _assemblies.Add(assembly);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[Hitch] Error loading assembly '{assemblyName}' from configuration: {ex.Message}");
                        }
                    }
                }
            }
        }

        // Load file patterns from configuration
        var filePatternsConfig = configSection.GetSection("FilePatterns");
        if (filePatternsConfig.Exists())
        {
            var patterns = filePatternsConfig.Get<string[]>();
            if (patterns != null)
            {
                foreach (var pattern in patterns)
                {
                    if (!string.IsNullOrWhiteSpace(pattern) && !_filePatterns.Contains(pattern))
                    {
                        _filePatterns.Add(pattern);
                    }
                }
            }
        }
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
            // Case 1: No category - attach with null name and empty configuration
            if (string.IsNullOrEmpty(attribute.Category))
            {
                var emptySection = _configuration.GetSection("__empty__");
                AttachPlugin(attribute.PluginType, null, null, null);
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
        // Look for configuration in structure: Hitch:Plugins:[CATEGORY]:[SUBCATEGORY]:[NAME]
        if (string.IsNullOrEmpty(attribute.Category) || string.IsNullOrEmpty(attribute.SubCategory))
        {
            return;
        }

        // Navigate to Hitch:Plugins:[CATEGORY]:[SUBCATEGORY]
        var categorySection = hitchConfigSection.GetSection(attribute.Category);
        if (!categorySection.Exists())
        {
            return;
        }

        var subCategorySection = categorySection.GetSection(attribute.SubCategory);
        if (!subCategorySection.Exists())
        {
            return;
        }

        // Iterate through all child sections (each is a named instance)
        foreach (var instanceSection in subCategorySection.GetChildren())
        {
            // Support two formats:
            // 1. New format: Hitch:Plugins:Category:SubCategory:serviceName = "serviceName" (or just serviceName as a section)
            //    Use the key as the service name
            // 2. Old format: Hitch:Plugins:Category:SubCategory:0 = "serviceName" (array-style)
            //    Use the value as the service name
            
            string? serviceName = null;
            var value = instanceSection.Value;
            
            if (!string.IsNullOrEmpty(value))
            {
                // Old format: value contains the service name
                serviceName = value;
            }
            else if (!string.IsNullOrEmpty(instanceSection.Key) && !int.TryParse(instanceSection.Key, out _))
            {
                // New format: key is the service name (and not a numeric index)
                serviceName = instanceSection.Key;
            }
            
            if (!string.IsNullOrEmpty(serviceName))
            {
                AttachPlugin(attribute.PluginType, attribute.Category, attribute.SubCategory, serviceName);
            }
        }
    }

    private void AttachPlugin(Type pluginType, string? category, string? subCategory, string? name)
    {
        try
        {
            // Construct the configuration section path
            IConfigurationSection configSection;
            if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(subCategory) && !string.IsNullOrEmpty(name))
            {
                // Path: Hitch:Plugins:{Category}:{SubCategory}:{name}
                var configPath = $"Hitch:Plugins:{category}:{subCategory}:{name}";
                configSection = _configuration.GetSection(configPath);
            }
            else
            {
                // No configuration section for uncategorized plugins
                configSection = _configuration.GetSection("__empty__");
            }

            // Instantiate the plugin provider
            var pluginInstance = Activator.CreateInstance(pluginType);
            
            if (pluginInstance is IPluginProvider provider)
            {
                provider.Attach(_services, configSection, name);
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

