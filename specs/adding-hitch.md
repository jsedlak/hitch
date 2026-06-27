Hitch should be added via a single method in a .net application..

```csharp
builder.AddHitch();
```

It should also offer the following configure callback:

```csharp
builder.AddHitch(builder => {
    builder.WithAssemblies(
        typeof(this).Assembly,
        typeof(string).Assembly
    );

    builder.WithFilePattern("*.Hitch.Plugin.dll");
})
```

In any of the above cases, the internals of `AddHitch` should create a concrete, internal builder class object which implements a public `IHitchBuilder` interface. It then calls the optional `configure` parameter.

After that, it calls `builder.Build()` which loops through all available assemblies and does two things:

1. for any `HitchPluginAttribute` attribute that has no category, instantiates the type and calls `Attach` with a null name.
2. Looks in configuration under the `Hitch__Plugins` section and loads Category, SubCategory and Service Name based on the following section structure: `[CATEGORY]__[SUBCATEGORY]__[SERVICE NAME]`. Each instance must carry a reserved `$plugin` key naming the owning builder's `PluginName`; Hitch routes the instance to that builder and calls `Attach`, supplying the `SERVICE NAME` (the instance key) as the keyed-service key. A missing, unknown, or ambiguous `$plugin` throws at startup.

For instance, configuration would look like:

```json
{
  "Hitch": {
    "Plugins": {
      "MyCategory": {
        "MySubCategory": {
          "ServiceName": { "$plugin": "MyPluginName" }
        }
      }
    }
  }
}
```
