## Unreleased

- Fixes plugins sharing a `(Category, SubCategory)` cross-registering over each other's keys. Each configured instance is now routed to a single owning builder via the reserved `$plugin` config key, matched against the builder's `[HitchPlugin(Alias = ...)]` (falling back to the plugin type name). Adds `HitchPluginAttribute.Alias`.

## 10.0.0

- Updates to .NET 10 and Aspire 13

## 10.0.1

- Fixes multiple providers in the same category mixmatching service registration
