## Unreleased

- **Breaking:** `WithPlugin(...)` now takes a required `plugin` parameter (the owning builder's `PluginName`), positioned before the optional `configurations`. It is always stamped into the instance config as the reserved `$plugin` discriminator so every instance routes to exactly one builder. Update call sites from `WithPlugin(cat, sub, name)` / `WithPlugin(cat, sub, name, configs, plugin: "X")` to `WithPlugin(cat, sub, name, "X", configs)`.

## 10.0.0

- Updates to .NET 10 and Aspire 13

## 10.0.1

- Fixes multiple providers in the same category mixmatching service registration
