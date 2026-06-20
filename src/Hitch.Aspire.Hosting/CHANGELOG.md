## Unreleased

- Adds an optional `plugin` parameter to `WithPlugin(...)` that stamps the owning-builder discriminator (`$plugin`) into an instance's config, disambiguating instances when multiple plugins share a `(Category, SubCategory)`.

## 10.0.0

- Updates to .NET 10 and Aspire 13

## 10.0.1

- Fixes multiple providers in the same category mixmatching service registration
