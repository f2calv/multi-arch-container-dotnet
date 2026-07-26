---
description: 'appsettings.json configuration sync rules for IAppConfig properties.'
applyTo: '**/appsettings*.json'
---

# Configuration

## Environment Layering (ASP.NET Core)

- `appsettings.json` is **both** the base configuration **and** the `Production` environment. ASP.NET Core loads `appsettings.json` first, then `appsettings.{Environment}.json`, merging **by key** (later wins). By convention there is **no** `appsettings.Production.json` — the `Production` environment is served by `appsettings.json` alone. **Never create one.**
- An `appsettings.{Environment}.json` override only needs to restate the keys it changes; every other key falls through to `appsettings.json`.
- Environment variables (`CasCap__Section__Property`) override all JSON layers, so Kubernetes Secrets/ConfigMaps can override any appsettings value at runtime.

## Configuration Sync

- Configuration properties (e.g. polling delays, feature flags, thresholds) are defined with sensible defaults directly on the `IAppConfig` record/class. Having defaults in the record means the application works out-of-the-box, but every property can be overridden via `appsettings*.json` or directly with environment variables in Kubernetes deployments (using the standard `CasCap__SectionName__PropertyName` double-underscore convention).
- When adding, renaming, or removing a property on any class or record that implements `IAppConfig` — or on any child/nested type reachable from such a class — update **all** `appsettings*.json` files (`appsettings.json`, `appsettings.Development.json`, and any other environment-specific variants) in the same commit. This includes adding new keys with sensible defaults, renaming keys to match the new property name, and removing keys for deleted properties. If the new property's record default is already the desired value for all environments, the `appsettings*.json` files do not need a new entry — only add one when an environment-specific override is required.
