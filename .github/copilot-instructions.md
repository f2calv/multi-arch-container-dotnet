# Copilot Instructions

## Shared Instructions

Shared Copilot instruction files are maintained centrally in the [.github](https://github.com/f2calv/.github) repository under `instructions/`, and are applied to every workspace from the VS Code user profile via `~/.copilot/instructions`. They are deliberately not copied into this repository, so a change there takes effect everywhere without a pull request here.

Everything below is specific to this repository.

## Repository Purpose

This repository is a .NET application that demonstrates how to build multi-architecture container images (amd64, arm64, arm/v7) from a single `Dockerfile` using `docker buildx`. It is a reference implementation, not a production workload.

## Sibling Repositories (alignment is a hard requirement)

Four repositories implement the *same* trivial worker application in four languages:

- [multi-arch-container-dotnet](https://github.com/f2calv/multi-arch-container-dotnet) (this one)
- [multi-arch-container-go](https://github.com/f2calv/multi-arch-container-go)
- [multi-arch-container-rust](https://github.com/f2calv/multi-arch-container-rust)
- [multi-arch-container-python](https://github.com/f2calv/multi-arch-container-python)

Their premise is that a developer fluent in one language can learn another language's containerisation story by diffing two repositories. **Any change made here must be considered for the other two.** Keep the following as close to identical as possible:

- Repository layout and file names.
- `Dockerfile` stage names (`build`, `final`), section comment banners and ordering.
- The `ARG`/`ENV` provenance block and OCI `LABEL` block.
- Environment variable names consumed by the application — both the flat `GIT_*`/`GITHUB_*` provenance variables and the `APP__*` configuration overrides.
- Application file responsibilities: configuration model, logging setup, worker loop, entry-point wiring.
- `.github/workflows/ci.yml` job names and structure.
- `.editorconfig` common section, `.pre-commit-config.yaml`, `.vscode/extensions.json`.
- `build.sh` / `build.ps1` are byte-identical (all values are derived from git).
- `README.md` section headings.

## No Helm Charts

These repositories are **application code only**. Kubernetes packaging lives in the standalone [f2calv/helm-charts](https://github.com/f2calv/helm-charts) repository, which provides a single multi-purpose chart used by all deployments. Do not reintroduce a `charts/` directory or a `chart` job in `ci.yml`.

## Linting is manual, never an auto-installed git hook

Do **not** wire `pre-commit install` into `.devcontainer/postCreateCommand.sh`, `postStartCommand.sh` or the README. The hook cost is fixed interpreter start-up per hook rather than per file, so a one-file commit pays the same price as a full run — noticeable on slower hardware. Linting is run manually with `pre-commit run --all-files`, and the `lint` job in `ci.yml` is the authoritative gate. A once-per-push hook (`pre-commit install --hook-type pre-push`) is an acceptable opt-in, never a default.

## Cross-Repository docker-compose

[`docker-compose.yml`](../docker-compose.yml) lives **only in this repository** and builds/runs all four sibling images together, so environment-variable and configuration behaviour can be compared side by side. It expects the sibling repositories to be cloned alongside this one:

```text
source/github/
├── multi-arch-container-dotnet/   <- docker-compose.yml lives here
├── multi-arch-container-go/
├── multi-arch-container-rust/
└── multi-arch-container-python/
```

Keep the `x-provenance` / `x-app-config` YAML anchors in sync with the `ARG`/`ENV` block of the Dockerfiles. Do not duplicate this file into the sibling repositories.

## Project Structure

- `docker-compose.yml` – builds and runs all four sibling images together (see above).
- `src/multi-arch-container-dotnet/` – console application source.
  - `Program.cs` – entry point; configuration, logging and DI wiring only.
  - `Models/_AppConfig.cs` – application configuration bound from the `app` section.
  - `Models/_BuildInfo.cs` – build provenance bound from the flat `GIT_*`/`GITHUB_*` variables.
  - `Models/_Enums.cs` – all enums for the project.
  - `Services/WorkerService.cs` – the `BackgroundService` worker loop.
  - `appsettings.json` – base configuration.
- `Dockerfile` – two-stage, cross-compiling, multi-architecture build.
- `.github/workflows/ci.yml` – CI/CD using reusable workflows from [f2calv/gha-workflows](https://github.com/f2calv/gha-workflows).
- `build.sh` / `build.ps1` – local build scripts for manual testing.
- `Directory.Build.props` / `Directory.Packages.props` – central MSBuild properties and NuGet versions.

## Technology Stack

- **Language**: C# 14 / .NET 10.0
- **Hosting**: `Microsoft.Extensions.Hosting` generic host, `BackgroundService` worker
- **Logging**: Serilog owns the `Microsoft.Extensions.Logging` pipeline; application code depends only on `ILogger<T>`
- **Configuration**: `Microsoft.Extensions.Configuration` (appsettings.json → environment variables), bound to validated `IOptions<T>` records
- **Container**: Docker (multi-stage, chiseled Ubuntu final image, non-root)
- **CI/CD**: GitHub Actions (reusable workflows from `f2calv/gha-workflows`)
- **Versioning**: GitVersion (MainLine mode)

## Configuration Keys

Configuration keys are **snake_case**, not PascalCase, and are mapped onto idiomatic C# property names with `[ConfigurationKeyName]`. This is deliberate: the sibling Go and Rust configuration libraries lower-case environment keys, so snake_case is the only casing where the file key and the environment key resolve identically across all four languages. Do not "correct" them to PascalCase.

| Key | Environment variable | Default |
| --- | --- | --- |
| `app:greeting` | `APP__GREETING` | `Hello from a multi-architecture container` |
| `app:interval_seconds` | `APP__INTERVAL_SECONDS` | `3` |
| `app:log_format` | `APP__LOG_FORMAT` | `text` |

The flat provenance variables (`GIT_REPOSITORY`, `GIT_BRANCH`, `GIT_COMMIT`, `GIT_TAG`, `GITHUB_WORKFLOW`, `GITHUB_RUN_ID`, `GITHUB_RUN_NUMBER`) are baked into the image by the `ARG`/`ENV` block of the `Dockerfile` and bound to `BuildInfo`.

## Target Platforms

The Dockerfile maps `TARGETARCH`+`TARGETVARIANT` onto a .NET Runtime Identifier (RID):

- `linux/amd64` → `linux-x64`
- `linux/arm64` → `linux-arm64`
- `linux/arm/v7` → `linux-arm`

## Container Conventions

- Keep the `Dockerfile` single-file with multi-stage builds; never add per-architecture Dockerfiles.
- The final image is `mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled` and runs as `$APP_UID`. Do not reintroduce a dependency that drags in the ASP.NET Core shared framework (e.g. `Serilog.AspNetCore`) without also switching the base image back to `dotnet/aspnet`.
- Heredoc `RUN <<EOF` blocks require **LF line endings**. `.gitattributes` enforces this; a CRLF `Dockerfile` fails at build time with `/bin/sh: set: Illegal option -`.
