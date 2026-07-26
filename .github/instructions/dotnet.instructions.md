---
description: '.NET solution and build structure — Directory.Build.props, central package management, solution format, SDK pinning.'
applyTo: '**/*.csproj,**/*.slnx,**/Directory.Build.props,**/Directory.Packages.props,**/global.json'
---

# .NET Solution & Build Structure

## Central Build Configuration

- **`Directory.Build.props`** in the repository root holds shared/repeated MSBuild properties — `RootNamespace`, `LangVersion`, `ImplicitUsings`, `Nullable`, `GenerateDocumentationFile`, `TreatWarningsAsErrors`, NuGet packaging metadata (`Authors`, `PackageProjectUrl`, `PackageLicenseFile`, `PackageReadmeFile`, symbol packaging), and `ContinuousIntegrationBuild` for CI. Individual `.csproj` files stay minimal — only project-specific properties and references belong there.
- **Warning suppressions** (`NoWarn`) are centralised in `Directory.Build.props`, each with an explanatory comment naming the suppressed code (see the *Suppressed Warnings* list in `csharp.instructions.md`).
- Conditional property groups keep cross-cutting concerns out of individual projects — e.g. disabling `GenerateDocumentationFile` for test projects via `Condition="$(MSBuildProjectName.Contains('Test'))"`, and gating `IsPackable` to opt-in per project.

## Central Package Management

- **`Directory.Packages.props`** in the repository root centralises every NuGet package version via `<PackageVersion>` (Central Package Management). It sets `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`.
- Project files reference packages with `<PackageReference Include="..." />` and **no** `Version` attribute — the version is resolved centrally.

## Solution Format

- Use the modern XML **`.slnx`** solution format. Convert legacy `.sln` files to `.slnx` rather than maintaining the old format.
- Where Debug/Release variants exist, name them `<Name>.Debug.slnx` / `<Name>.Release.slnx`. Debug variants wire up local `ProjectReference`s to the CasCap.Common projects; Release variants use published `PackageReference`s. When building, prefer the `.Debug.slnx`.

## SDK Pinning

- **`global.json`** in the repository root pins the .NET SDK version (and roll-forward policy) for reproducible builds across machines and CI.

## EF Core Migrations

CAS runs one Postgres database per environment (`cas_dev` / `cas_tst` / `cas_prd`) with rolling `Deployment` updates, so every migration MUST follow the expand/contract (parallel change) convention in [`efcore.instructions.md`](efcore.instructions.md).

CAS-specific schema-application wiring:

- **tst** (and prd once it moves to k3s) sets `CasCap__DatabaseConfig__Provider=Postgres` and `CasCap__DatabaseConfig__MigrateOnStartup=false` in its environment override (`appsettings.Test.json` for tst; the base `appsettings.json` is the Production layer). Setting `MigrateOnStartup=false` de-registers `DatabaseMigrationService`, so no `MigrateAsync` runs at pod startup. Schema is applied by the **`DbMigrator`** feature — a one-shot `IBgFeature` (`DbMigratorBgService`) that migrates then stops the host — launched via `CasCap__FeatureConfig__EnabledFeatures=DbMigrator` from the generic PreSync `job.yaml` in the `monolith` chart, which runs to completion **before** the app workloads roll. The per-env `ConnectionString` is delivered as a Secret env var (overriding the appsettings value with the in-cluster service DNS); the non-secret `Provider`/`MigrateOnStartup` live in the appsettings layer. The `default-app-config` ConfigMap holds only deployment-environment vars (`ASPNETCORE_ENVIRONMENT`, `SLOT_NAME`, etc.), never app config.
- **Local dev** keeps `MigrateOnStartup=true` (in-process `DatabaseMigrationService`) for convenience.
- **prd on AKS** currently uses `Provider=InMemory` (set in the base `appsettings.json`, which is the Production config — there is no `appsettings.Production.json`), since the k3s CNPG cluster is unreachable from AKS; the `DbMigrator` PreSync Job stays disabled there until prd moves onto the k3s cluster.
