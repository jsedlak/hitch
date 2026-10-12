## 10.0.0

- Updates to .NET 10 and Aspire 13

## 1.0.1

- Versioning moves from 10.x to 1.x
- Fixes multiple providers in the same category mixmatching service registration

## 1.1.0

- Superseded by 1.2.0; upgrade from 1.0.1 straight to 1.2.0. Its `WithPlugin` writes a `$plugin` environment variable, which Azure Container Apps and Kubernetes reject.

## 1.2.0

- **Breaking:** `WithPlugin(...)` now takes a required `plugin` parameter (the owning builder's `PluginName`), positioned before the optional `configurations`. Update call sites from `WithPlugin(cat, sub, name)` / `WithPlugin(cat, sub, name, configs)` to `WithPlugin(cat, sub, name, "PluginName", configs)`.
- `WithPlugin` writes the owner as the reserved `HitchPlugin` key, so every instance routes to exactly one builder and `WithReference(hitch)` exports only environment variable names that Azure Container Apps and Kubernetes accept. A `$plugin` entry passed in `configurations` is dropped in favor of the `plugin` argument.
- Requires `Hitch` 1.2.0 or later in the consuming app.
