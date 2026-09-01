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

- **`global.json`** in the repository root pins the .NET SDK version and roll-forward policy for reproducible builds across machines and CI.
