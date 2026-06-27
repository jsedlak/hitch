namespace Hitch.Tests.TestPlugins;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contract shared by tool providers, resolved from DI by keyed service name — mirrors how a real
/// consumer (e.g. Covalent's tool providers) registers keyed services off the instance name.
/// </summary>
public interface IKeyedToolProvider
{
    string Kind { get; }
    string Name { get; }
}

/// <summary>
/// Two builders that share <c>(Keyed, Tools)</c> and each register a keyed <see cref="IKeyedToolProvider"/>
/// under the instance name. Used to prove that distinct instance names never clobber, regardless of
/// whether they route to the same or different builders.
/// </summary>
public sealed class KeyedToolProviderBuilder : IPluginProvider
{
    public void Attach(IServiceCollection services, IConfigurationSection configurationSection, string? name = null)
    {
        services.AddKeyedSingleton<IKeyedToolProvider>(name!, (_, key) => new KeyedToolProvider("Keyed", (string)key!));
    }
}

public sealed class OtherKeyedToolProviderBuilder : IPluginProvider
{
    public void Attach(IServiceCollection services, IConfigurationSection configurationSection, string? name = null)
    {
        services.AddKeyedSingleton<IKeyedToolProvider>(name!, (_, key) => new KeyedToolProvider("Other", (string)key!));
    }
}

public sealed class KeyedToolProvider : IKeyedToolProvider
{
    public KeyedToolProvider(string kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    public string Kind { get; }
    public string Name { get; }
}
