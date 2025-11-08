![Logo](logo.png)

# Hitch

Hitch provides generalized plugin capabilities for Aspire, allowing application developers to focus on adding business value rather than managing infrastructure and resources.

We add Hitch to the Aspire AppHost, creating the plugin resource. In the following example, we are informing Hitch to look for any DLL matching the included pattern.

```csharp
var hitch = builder.AddHitch(b =>
    b.WithFilePattern("Covalent.*.Plugin.dll")
);
```

Plugins extend this builder by providing extension methods to add them to the Hitch resource. The following example adds Storyblok.

```csharp
hitch.WithStoryblokManagement(
	"storyblok", 
	storyblokSpaceId, 
	storyblokApiUrl, 
	storyblokToken
);
```

When the `hitch` resource is added to another resource, for example the Covalent Silo, a few things happen. Hitch adds environment variables that tells the target what plugins are loaded, and it provides configuration for each of them via environment variables. These variables build out a "configuration section" that can be used to configure services within the plugin code.

For instance, the above code may end up passing the following:

```
HITCH__PLUGINS__storyblok__SPACEID=...
HITCH__PLUGINS__storyblok__APIURL=...
HITCH__PLUGINS__storyblok__TOKEN=...
```

On the target side, plugins are activated by calling

```csharp
builder.Services.AddHitch();
```

For plugin developers, DLLs are auto detected based on pattern and `HitchPlugin` assembly attributes are used to activate provider builder objects.

```csharp
[assembly: HitchPlugin("Tool", "Storyblok", typeof(StoryblokToolProviderBuilder))]
```

This type uses the `IPluginProvider` interface to receive the data from Hitch that was configured as part of the AppHost.

```csharp
void Attach(IServiceCollection services, IConfigurationSection configurationSection, string? name = null)
```

From there, it can inject any services that are needed to support the behaviors expected by those including the plugin. In the case of Storyblok, Management API Services, Agent Tools and more are added.

It is important that these utilize **Keyed Services** such that application developers can add multiple instances of the same plugin with differing configurations (for instance, multiple Storyblok plugins pointing at different **Storyblok Spaces**)