## Unreleased

- **Breaking:** `[HitchPlugin]` for categorized plugins now requires a first-party `PluginName` as a constructor argument — `HitchPlugin(category, subCategory, pluginName, type)`. Removes the optional `Alias` property and the `(category, subCategory, type)` constructor. `PluginName` is the routing identity and must be unique within a `(Category, SubCategory)`.
- **Breaking:** every categorized config instance must declare its owner via the reserved `$plugin` key (matched against `PluginName`). A missing, unknown, or ambiguous `$plugin` now **throws at startup** instead of silently skipping or cross-registering. The plugin type name is no longer accepted as a routing fallback.
- **Breaking:** the legacy array/scalar config form for categorized plugins (`SubCategory: ["name"]` / `SubCategory:0 = "name"`) is no longer supported — it cannot carry `$plugin`. Use the object form: `SubCategory:name:$plugin = "PluginName"`.
- The instance name remains the keyed-service key, so one builder can back many named instances under a single `PluginName` without collision.

## 10.0.0

- Updates to .NET 10 and Aspire 13

## 10.0.1

- Fixes multiple providers in the same category mixmatching service registration
