Hitch is powered by an assembly attribute in client projects.

```csharp
[assembly: HitchPlugin(typeof(MyOtherHitchPlugin))]
[assembly: HitchPlugin("Category", "SubCategory", typeof(MyHitchPlugin))]
```

- The attribute should be public and apply only at the assembly level.
- The attribute makes use of an optional Category and SubCategory identification
