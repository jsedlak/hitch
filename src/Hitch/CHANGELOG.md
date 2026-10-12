## 10.0.0

- Updates to .NET 10 and Aspire 13

## 1.0.1

- Versioning moves from 10.x to 1.x
- Fixes multiple providers in the same category mixmatching service registration

## 1.1.0

- Superseded by 1.2.0; upgrade from 1.0.1 straight to 1.2.0. Its reserved `$plugin` key cannot be delivered as an environment variable on Azure Container Apps or Kubernetes.

## 1.2.0

- **Breaking:** `[HitchPlugin]` for categorized plugins now requires a first-party `PluginName` as a constructor argument — `HitchPlugin(category, subCategory, pluginName, type)`. Removes the optional `Alias` property and the `(category, subCategory, type)` constructor. `PluginName` is the routing identity and must be unique within a `(Category, SubCategory)`.
- **Breaking:** every categorized config instance must name its owner via the reserved `HitchPlugin` key (matched against `PluginName`), for example `SubCategory:name:HitchPlugin = "PluginName"`. A missing, unknown, or ambiguous owner now **throws at startup** instead of silently skipping or cross-registering. The plugin type name is no longer accepted as a routing fallback.
- `HitchPlugin` is a valid environment variable name on Azure Container Apps and Kubernetes. The 1.1.0 spelling, `$plugin`, is still accepted; if an instance carries both and they disagree, Hitch throws at startup naming both values. The key names are exposed as `HitchConfigurationKeys.Plugin` and `HitchConfigurationKeys.LegacyPlugin`.
- **Breaking:** the legacy array/scalar config form for categorized plugins (`SubCategory: ["name"]` / `SubCategory:0 = "name"`) is no longer supported — it cannot carry an owner. Use the object form above.
- The instance name remains the keyed-service key, so one builder can back many named instances under a single `PluginName` without collision.
