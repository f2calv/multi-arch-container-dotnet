# Copilot Instructions

## Repository Overview

This repository is a .NET application that demonstrates how to build multi-architecture container images (amd64, arm64, arm/v7) from a single `Dockerfile` using `docker buildx`. It serves as a reference implementation for cross-compiling .NET to multiple target platforms.

## Project Structure

- `src/multi-arch-container-dotnet/` – .NET console application source code.
  - `Program.cs` – Entry point. A simple worker loop that logs system/environment info using Serilog.
  - `Models/AppSettings.cs` – Configuration model that reads environment variables.
- `Dockerfile` – Multi-stage Dockerfile for building and packaging the app for multiple architectures.
- `Dockerfile.singlearch` – Simplified single-architecture Dockerfile for reference.
- `.github/workflows/ci.yml` – CI/CD pipeline using reusable workflows from [f2calv/gha-workflows](https://github.com/f2calv/gha-workflows).
- `charts/` – Helm chart for Kubernetes deployment.
- `.devcontainer/` – VS Code devcontainer configuration (.NET SDK + Docker-outside-of-Docker).
- `build.sh` / `build.ps1` – Local build scripts for manual testing.
- `Directory.Build.props` – Central MSBuild properties (language version, nullable, implicit usings).
- `Directory.Packages.props` – Centralized NuGet package version management.
- `global.json` – .NET SDK configuration.

## Technology Stack

- **Language**: C# 14 / .NET 10.0
- **Logging**: Serilog (with Console sink, Environment enricher)
- **Container**: Docker (multi-stage, chiseled Ubuntu final image)
- **CI/CD**: GitHub Actions (reusable workflows from `f2calv/gha-workflows`)
- **Helm**: Chart packaging and publishing to GHCR
- **Versioning**: GitVersion (MainLine mode)

## Build & Test Commands

```bash
# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Build in Release mode
dotnet publish -c Release

# Run the application
dotnet run --project src/multi-arch-container-dotnet
```

## Local Container Build

Use the provided helper scripts from the repository root:

```bash
# Shell
. build.sh

# PowerShell
./build.ps1
```

Or manually using `docker buildx`:

```bash
docker buildx create --name multiarchcontainerdotnet --use
docker buildx build \
    -t multi-arch-container-dotnet:dev \
    --platform linux/amd64 \
    --pull \
    -o type=docker \
    .
```

## Configuration

The application reads configuration from environment variables:

| Environment Variable  | Description                  |
|-----------------------|------------------------------|
| `GIT_REPOSITORY`      | Git repository name          |
| `GIT_BRANCH`          | Git branch name              |
| `GIT_COMMIT`          | Git commit SHA               |
| `GIT_TAG`             | Git tag                      |
| `GITHUB_WORKFLOW`     | GitHub Actions workflow name |
| `GITHUB_RUN_ID`       | GitHub Actions run ID        |
| `GITHUB_RUN_NUMBER`   | GitHub Actions run number    |

## Target Platforms

The Dockerfile supports cross-compilation to the following platforms via .NET Runtime Identifiers (RIDs):

- `linux/amd64` → `linux-x64`
- `linux/arm64` → `linux-arm64`
- `linux/arm/v7` → `linux-arm`

## Coding Conventions

- Follow standard C# conventions and the project's `.editorconfig` settings.
- Use `dotnet format` for code formatting before submitting changes.
- Keep the `Dockerfile` single-file with multi-stage builds; avoid separate per-architecture Dockerfiles.
- Configuration is injected via environment variables (no config files bundled in the image).
- The final container image is based on `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled` to minimize attack surface.
- NuGet package versions are managed centrally in `Directory.Packages.props`.
