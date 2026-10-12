Hitch is powered by an assembly attribute in client projects.

```csharp
[assembly: HitchPlugin(typeof(MyOtherHitchPlugin))]
[assembly: HitchPlugin("Category", "SubCategory", "PluginName", typeof(MyHitchPlugin))]
```

- The attribute should be public and apply only at the assembly level.
- Uncategorized plugins use the single-argument constructor and attach with a null name.
- Categorized plugins supply Category, SubCategory and a required, first-party `PluginName`. The
  `PluginName` is the stable routing identity and must be unique among builders that share a
  `(Category, SubCategory)`.

## Multiple plugins in one subcategory

More than one plugin can register for the same `(Category, SubCategory)`. Each declares its own
`PluginName`, and every configured instance routes to exactly one builder by naming it:

```csharp
[assembly: HitchPlugin("Covalent", "Responses", "OpenAi", typeof(OpenAiResponsesProviderBuilder))]
[assembly: HitchPlugin("Covalent", "Responses", "Foundry", typeof(FoundryResponsesProviderBuilder))]
```

Each instance declares its owner via the reserved `HitchPlugin` config key:

```
Hitch:Plugins:Covalent:Responses:openai:HitchPlugin        = "OpenAi"
Hitch:Plugins:Covalent:Responses:azure-foundry:HitchPlugin = "Foundry"
```

Only the builder whose `PluginName` matches `HitchPlugin` is attached for that instance. The instance
name (`openai`, `azure-foundry`) is the uniqueness factor — it is the key under which the builder
registers its keyed service — so the same builder may back many instances under one
`PluginName`, each with its own name, without collision.

`HitchPlugin` is **required** for every categorized instance (even when a bucket has a single builder).
An instance that declares no owner, names an unregistered owner, or matches more than one builder
**throws at startup** with a diagnostic — there is no silent skip. Because routing requires a
per-instance key, the legacy array/scalar config form (`SubCategory: ["name"]` or `SubCategory:0 = "name"`)
is no longer supported for categorized plugins; use the object form shown above. On the Aspire hosting
side, pass the owner through `WithPlugin(..., plugin: "Foundry")` instead of hand-writing `HitchPlugin`.

### The legacy `$plugin` key

`$plugin` is the original spelling of `HitchPlugin` and is still accepted, so existing
`appsettings.json` files keep working. Prefer `HitchPlugin` for new configuration: `$` is not a valid
character in environment variable names on Azure Container Apps or Kubernetes
(`^[-._a-zA-Z][-._a-zA-Z0-9]*$`), so `$plugin` cannot be delivered through environment variables
there. `Hitch.Aspire.Hosting` always writes `HitchPlugin`.

An instance may carry both keys if they name the same plugin (compared case-insensitively, like
`PluginName`). If they differ, Hitch throws at startup naming both values.

Because the binder is case-insensitive, an options class bound to an instance section should not
declare a property named `HitchPlugin`; it would receive the owner's name.
