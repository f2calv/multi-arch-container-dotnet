# Copilot Instructions

## Shared Instructions

Shared Copilot instructions, skills and prompts are maintained centrally in the [.github](https://github.com/f2calv/.github) repository, under `.github/instructions/`, `.github/skills/` and `.github/prompts/`. They are deliberately not copied into this repository, so a change there takes effect everywhere without a pull request here.

To load them, clone that repository and either add it to this VS Code workspace, or link its folders into `~/.copilot/`. Its README explains both.

If those shared files are not visible, stop and tell the user rather than guessing the conventions — this repository depends on them.

Everything below is specific to this repository.

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

## Cross-Repository docker-compose

[`docker-compose.yml`](../docker-compose.yml) lives **only in this repository** and builds/runs all four sibling images together, so environment-variable and configuration behaviour can be compared side by side. It expects the sibling repositories to be cloned alongside this one, as laid out in the [README](../README.md#run-all-four-side-by-side).

Keep the `x-provenance` / `x-app-config` YAML anchors in sync with the `ARG`/`ENV` block of the Dockerfiles. Do not duplicate this file into the sibling repositories.

## Configuration Key Casing

Configuration keys are **snake_case**, not PascalCase, and are mapped onto idiomatic C# property names with `[ConfigurationKeyName]`. This is deliberate: the sibling Go and Rust configuration libraries lower-case environment keys, so snake_case is the only casing where the file key and the environment key resolve identically across all four languages. Do not "correct" them to PascalCase. The keys themselves are documented in the [README](../README.md#configuration).

## Container Conventions

- The final image is `mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled` and runs as `$APP_UID`. Do not reintroduce a dependency that drags in the ASP.NET Core shared framework (e.g. `Serilog.AspNetCore`) without also switching the base image back to `dotnet/aspnet`.
