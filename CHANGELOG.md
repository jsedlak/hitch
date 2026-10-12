## 10.0.0

- Updates to .NET 10 and Aspire 13

## 1.0.1

- Versioning moves from 10.x to 1.x
- Fixes multiple providers in the same category mixmatching service registration

## 1.1.0

- Superseded by 1.2.0; upgrade from 1.0.1 straight to 1.2.0

## 1.2.0

- Elevates Alias to Plugin Name, required field
- Fixes duplicative/overlapped routing
- Uses `HitchPlugin` as the reserved owner key so configuration deploys to Azure Container Apps and Kubernetes (`$plugin` still works)
