#!/usr/bin/env bash
#
# Local equivalent of the `image` job in .github/workflows/ci.yml.
# This file is byte-identical across the sibling multi-arch-container-* repositories -
# every value it needs is derived from git rather than hard-coded.
set -euo pipefail

#Emulate the variables that the CI workflow injects into the build
GIT_REPOSITORY=$(basename "$(git rev-parse --show-toplevel)")
GIT_BRANCH=$(git branch --show-current)
GIT_COMMIT=$(git rev-parse HEAD)
GIT_TAG="latest-dev"
GITHUB_WORKFLOW="n/a"
GITHUB_RUN_ID=0
GITHUB_RUN_NUMBER=0

IMAGE_NAME="$GIT_REPOSITORY:$GIT_TAG"
BUILDER=$(echo "$GIT_REPOSITORY" | tr -d '-')

#Note: a multi-platform image cannot be loaded into the local docker image store, so a
#      local build targets a single platform. To exercise all three architectures run:
#        PLATFORM=linux/amd64,linux/arm64,linux/arm/v7 OUTPUT=--push ./build.sh
PLATFORM="${PLATFORM:-linux/amd64}"
OUTPUT="${OUTPUT:---load}"

#Create (or reuse) a builder instance capable of multi-platform builds
#https://docs.docker.com/reference/cli/docker/buildx/create/
docker buildx inspect "$BUILDER" >/dev/null 2>&1 || docker buildx create --name "$BUILDER" --bootstrap
docker buildx use "$BUILDER"

#Start a build
#https://docs.docker.com/reference/cli/docker/buildx/build/
docker buildx build \
    -t "$IMAGE_NAME" \
    --label "GITHUB_RUN_ID=$GITHUB_RUN_ID" \
    --label "IMAGE_NAME=$IMAGE_NAME" \
    --build-arg GIT_REPOSITORY="$GIT_REPOSITORY" \
    --build-arg GIT_BRANCH="$GIT_BRANCH" \
    --build-arg GIT_COMMIT="$GIT_COMMIT" \
    --build-arg GIT_TAG="$GIT_TAG" \
    --build-arg GITHUB_WORKFLOW="$GITHUB_WORKFLOW" \
    --build-arg GITHUB_RUN_ID="$GITHUB_RUN_ID" \
    --build-arg GITHUB_RUN_NUMBER="$GITHUB_RUN_NUMBER" \
    --platform "$PLATFORM" \
    --pull \
    $OUTPUT \
    .

#Preview matching images
#https://docs.docker.com/reference/cli/docker/image/ls/
docker images "$GIT_REPOSITORY"

read -r -p "Hit ENTER to run the '$IMAGE_NAME' image (Ctrl-C to quit)..."

#Run the container image, Ctrl-C shuts it down cleanly
#https://docs.docker.com/reference/cli/docker/container/run/
docker run --rm -it --name "$GIT_REPOSITORY" "$IMAGE_NAME"
