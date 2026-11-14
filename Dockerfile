# syntax=docker/dockerfile:1
#
# Multi-architecture container image built from a SINGLE Dockerfile.
# Build strategy: native-aot
# Native builders are required for linux/amd64 and linux/arm64. The AOT SDK image
# does not publish arm/v7; this is a deliberate .NET-specific sibling difference.
#
# This file deliberately mirrors its sibling repositories stage-for-stage and
# comment-for-comment, so that a developer fluent in one language can learn the
# containerisation story of another by diffing the two files:
# The canonical AOT publish contract fits two stages; final retains the siblings'
# provenance ENV contract rather than introducing a separate runtime stage.
#
#   https://github.com/f2calv/multi-arch-container-dotnet   <- you are here
#   https://github.com/f2calv/multi-arch-container-go
#   https://github.com/f2calv/multi-arch-container-rust
#   https://github.com/f2calv/multi-arch-container-python
#
# ------------------------------------------------------------------------------
# Stage 1 of 2: build
#
# Target-native SDK and compiler toolchain; RID selection alone cannot supply
# the native linker/sysroot needed for cross-architecture Native AOT.
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:11.0-resolute-aot AS build
WORKDIR /repo

ARG APP_NAME=multi-arch-container-dotnet
ARG WORKLOAD=$APP_NAME
ARG PROJECT=src/$APP_NAME/$APP_NAME.csproj
ARG CONFIGURATION=Release
ARG TARGET_FRAMEWORK=net11.0
ARG INVARIANT_GLOBALIZATION=false
ARG VERSION=0.0.0-local
ARG GIT_REPOSITORY=n/a
ARG GIT_BRANCH=n/a
ARG GIT_COMMIT=n/a
ARG GITHUB_WORKFLOW=n/a
ARG GITHUB_RUN_ID=0
ARG GITHUB_RUN_NUMBER=0

# -- Dependency layer ----------------------------------------------------------
# Copy ONLY the files that influence `dotnet restore` (--parents preserves the
# directory structure) so that editing a .cs file reuses the cached restore.
COPY --parents Directory.Build.props Directory.Packages.props global.json src/**/*.csproj ./
ARG BUILDPLATFORM
ARG TARGETPLATFORM
ARG TARGETOS
ARG TARGETARCH
ARG TARGETVARIANT
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked --mount=type=secret,id=nuget_source <<EOF
set -eu
test "$BUILDPLATFORM" = "$TARGETPLATFORM"
case "${TARGETOS}/${TARGETARCH}${TARGETVARIANT}" in
    linux/amd64) RID=linux-x64   ;;
    linux/arm64) RID=linux-arm64 ;;
    *) echo "unsupported platform: ${TARGETOS}/${TARGETARCH}/${TARGETVARIANT}" >&2; exit 1 ;;
esac
set --
if [ -f /run/secrets/nuget_source ]; then
    set -- --source "$(cat /run/secrets/nuget_source)"
fi
dotnet restore "$PROJECT" \
    "$@" \
    --runtime "$RID" \
    -p:Configuration="$CONFIGURATION" \
    -p:TargetFramework="$TARGET_FRAMEWORK" \
    -p:SelfContained=true \
    -p:PublishAot=true \
    -p:InvariantGlobalization="$INVARIANT_GLOBALIZATION"
printf '%s' "$RID" > /repo/target-rid
EOF

# -- Compile layer -------------------------------------------------------------
COPY . .

# Native publishing reads only the packages restored for this platform.
RUN --network=none --mount=type=cache,target=/root/.nuget/packages,sharing=shared <<EOF
set -eux
RID="$(cat /repo/target-rid)"
dotnet publish "$PROJECT" \
    --configuration "$CONFIGURATION" \
    --framework "$TARGET_FRAMEWORK" \
    --output /app/publish \
    --runtime "$RID" \
    --self-contained true \
    --no-restore \
    -p:PublishAot=true \
    -p:InvariantGlobalization="$INVARIANT_GLOBALIZATION" \
    -p:Version="$VERSION" \
    -p:SourceRevisionId="$GIT_COMMIT" \
    -p:GitRepository="$GIT_REPOSITORY" \
    -p:GitBranch="$GIT_BRANCH" \
    -p:BuildWorkflow="$GITHUB_WORKFLOW" \
    -p:BuildRunId="$GITHUB_RUN_ID" \
    -p:BuildRunNumber="$GITHUB_RUN_NUMBER"
ln -s "$WORKLOAD" /app/publish/entrypoint
EOF

# ------------------------------------------------------------------------------
# Stage 2 of 2: final
#
# No --platform override here, so buildx resolves the base image for
# $TARGETPLATFORM and the resulting image is genuinely native to the target.
#
# No managed runtime is required. The extra variant preserves normal culture
# and time-zone behavior rather than silently enabling invariant globalization.
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime-deps:11.0-resolute-chiseled-extra AS final
WORKDIR /app
COPY --link --from=build /app/publish .

# -- Provenance ----------------------------------------------------------------
# Supplied by the CI workflow (.github/workflows/ci.yml) or by build.sh/build.ps1.
ARG APP_NAME=multi-arch-container-dotnet
ARG WORKLOAD=$APP_NAME
ARG VERSION=0.0.0-local
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
LABEL org.opencontainers.image.title="$WORKLOAD" \
    org.opencontainers.image.description="Multi-architecture Native AOT container build (amd64/arm64) w/.NET" \
    org.opencontainers.image.source="https://github.com/f2calv/multi-arch-container-dotnet" \
    org.opencontainers.image.licenses="MIT" \
    org.opencontainers.image.version="$VERSION" \
    org.opencontainers.image.revision="$GIT_COMMIT"

# $APP_UID is defined by the .NET base images (1654). Chiseled images already run
# as this user - setting it explicitly documents the intent and keeps the four
# sibling repositories consistent.
USER $APP_UID

ENTRYPOINT ["/app/entrypoint"]
