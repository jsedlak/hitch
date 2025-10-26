The `Hitch.Aspire.Hosting` project adds support for `AddHitch` as a resource in an Aspire project. The primary output is configuration that is compatible with the client side `AddHitch` (the one that already exists).

The usage is as follows...

```csharp
distributedBuilder.AddHitch(builder => {
    builder.WithAssemblies(
        typeof(this).Assembly,
        typeof(string).Assembly
    );

    builder.WithFilePattern("*.Hitch.Plugin.dll");
})
    .WithPlugin("Category", "SubCategory", "Service Name")
```

This should output configuration compatible with [adding-hitch.md](adding-hitch.md).

Additional configuration should also support adding Assemblies and File Patterns. These can be placed in the section Hitch\_\_Configuration.

Update the client side `AddHitch` to auto-load from configuration prior to calling the option configure callback.
