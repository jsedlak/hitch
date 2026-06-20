Hitch is powered by an assembly attribute in client projects.

```csharp
[assembly: HitchPlugin(typeof(MyOtherHitchPlugin))]
[assembly: HitchPlugin("Category", "SubCategory", typeof(MyHitchPlugin))]
```

- The attribute should be public and apply only at the assembly level.
- The attribute makes use of an optional Category and SubCategory identification

## Multiple plugins in one subcategory

More than one plugin can register for the same `(Category, SubCategory)`. Give each a stable
`Alias` so configured instances can be routed to the builder that owns them:

```csharp
[assembly: HitchPlugin("Covalent", "Responses", typeof(OpenAiResponsesProviderBuilder), Alias = "OpenAi")]
[assembly: HitchPlugin("Covalent", "Responses", typeof(FoundryResponsesProviderBuilder), Alias = "Foundry")]
```

Each instance declares its owner via the reserved `$plugin` config key:

```
Hitch:Plugins:Covalent:Responses:openai:$plugin        = "OpenAi"
Hitch:Plugins:Covalent:Responses:azure-foundry:$plugin = "Foundry"
```

Only the builder whose `Alias` (or plugin type name, when no alias is declared) matches `$plugin`
is attached for that instance. With a single builder in the subcategory, `$plugin` is optional and
the lone builder is attached. With multiple builders, an instance that declares no `$plugin` — or
names an owner that isn't registered — is skipped with a diagnostic. On the Aspire hosting side,
pass the owner through `WithPlugin(..., plugin: "Foundry")` instead of hand-writing `$plugin`.
