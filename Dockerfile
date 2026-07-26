# syntax=docker/dockerfile:1
#
# Multi-architecture container image built from a SINGLE Dockerfile.
#
# This file deliberately mirrors its sibling repositories stage-for-stage and
# comment-for-comment, so that a developer fluent in one language can learn the
# containerisation story of another by diffing the two files:
#
#   https://github.com/f2calv/multi-arch-container-dotnet   <- you are here
#   https://github.com/f2calv/multi-arch-container-go
#   https://github.com/f2calv/multi-arch-container-rust
#
# ------------------------------------------------------------------------------
# Stage 1 of 2: build
#
# Pinned to $BUILDPLATFORM (the native architecture of the machine running the
# build) and CROSS-COMPILES to $TARGETPLATFORM. The alternative - emulating the
# target architecture under QEMU - is typically 10-50x slower.
# ------------------------------------------------------------------------------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG APP_NAME=multi-arch-container-dotnet
ARG CONFIGURATION=Release

# -- Dependency layer ----------------------------------------------------------
# Copy ONLY the files that influence `dotnet restore` (--parents preserves the
# directory structure) so that editing a .cs file reuses the cached restore.
# Restore is platform-agnostic, so it is performed BEFORE TARGETARCH is
# introduced and is therefore shared by every target architecture.
COPY --parents Directory.Build.props Directory.Packages.props src/**/*.csproj ./
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "src/$APP_NAME/$APP_NAME.csproj"

# -- Compile layer -------------------------------------------------------------
COPY . .

# buildx injects TARGETARCH/TARGETVARIANT automatically:
#   linux/amd64  -> TARGETARCH=amd64  TARGETVARIANT=
#   linux/arm64  -> TARGETARCH=arm64  TARGETVARIANT=
#   linux/arm/v7 -> TARGETARCH=arm    TARGETVARIANT=v7
# Concatenating the two gives a single flat token to switch on: amd64|arm64|armv7.
ARG TARGETARCH
ARG TARGETVARIANT
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked <<EOF
set -eux
# Map the Docker platform onto a .NET Runtime Identifier (RID).
# https://learn.microsoft.com/dotnet/core/rid-catalog
case "${TARGETARCH}${TARGETVARIANT}" in
    amd64) RID=linux-x64   ;;
    arm64) RID=linux-arm64 ;;
    armv7) RID=linux-arm   ;;
    *) echo "unsupported platform: linux/${TARGETARCH}/${TARGETVARIANT}" >&2; exit 1 ;;
esac
dotnet publish "src/$APP_NAME/$APP_NAME.csproj" \
    --configuration "$CONFIGURATION" \
    --runtime "$RID" \
    --self-contained false \
    --output /out
EOF

# ------------------------------------------------------------------------------
# Stage 2 of 2: final
#
# No --platform override here, so buildx resolves the base image for
# $TARGETPLATFORM and the resulting image is genuinely native to the target.
#
# Alternatives, smallest to largest:
#   mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled  distroless-style, non-root, no shell
#   mcr.microsoft.com/dotnet/runtime:10.0-alpine          musl, has a shell, no tzdata by default
#   mcr.microsoft.com/dotnet/runtime:10.0                 full Debian, largest, easiest to debug
# Swap `runtime` for `aspnet` if the application needs the ASP.NET Core shared framework.
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /out .

# -- Provenance ----------------------------------------------------------------
# Supplied by the CI workflow (.github/workflows/ci.yml) or by build.sh/build.ps1.
ARG GIT_REPOSITORY=n/a
ENV GIT_REPOSITORY=$GIT_REPOSITORY
ARG GIT_BRANCH=n/a
ENV GIT_BRANCH=$GIT_BRANCH
ARG GIT_COMMIT=n/a
ENV GIT_COMMIT=$GIT_COMMIT
ARG GIT_TAG=n/a
ENV GIT_TAG=$GIT_TAG

ARG GITHUB_WORKFLOW=n/a
ENV GITHUB_WORKFLOW=$GITHUB_WORKFLOW
ARG GITHUB_RUN_ID=0
ENV GITHUB_RUN_ID=$GITHUB_RUN_ID
ARG GITHUB_RUN_NUMBER=0
ENV GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER

# https://github.com/opencontainers/image-spec/blob/main/annotations.md
LABEL org.opencontainers.image.title="multi-arch-container-dotnet" \
    org.opencontainers.image.description="Multi-architecture container build (amd64/arm64/armv7) w/.NET" \
    org.opencontainers.image.source="https://github.com/f2calv/multi-arch-container-dotnet" \
    org.opencontainers.image.licenses="MIT" \
    org.opencontainers.image.version="$GIT_TAG" \
    org.opencontainers.image.revision="$GIT_COMMIT"

# $APP_UID is defined by the .NET base images (1654). Chiseled images already run
# as this user - setting it explicitly documents the intent and keeps the three
# sibling repositories consistent.
USER $APP_UID

ENTRYPOINT ["dotnet", "multi-arch-container-dotnet.dll"]
