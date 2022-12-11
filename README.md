# Multi-Architecture Container Image w/.NET

## Introduction

I've been developing a service orientated smart home system which consists of a number of separate containerised workloads all running on an edge Kubernetes cluster (via [Microk8s](https://github.com/canonical/microk8s)), the "cluster" itself is a sole Raspberry Pi 4 (ARMv8).

As well as running multiple workloads on the Pi 4 in addition I sometimes run single workloads on another Raspberry Pi 2b (ARMv7) - a very old Pi but very power efficient. And finally I need to run general tests of the workloads on my local Windows development machine prior to deployment to "Production".

Although I could acheive my goal of deploying the same application to multiple architectures using seperate Dockerfiles (i.e. Dockerfile.amd64, Dockerfile.arm64, etc...) in my view that is messy and makes the CI/CD overly complex.

This repository contains my learnings and provides a fully working example of building a .NET application container image that is capable of targetting multiple platform architectures - all from a single Dockerfile.

If you find this repository of use then please massage my ego by giving this repository a :star: ... :wink:

## Goals

- Construct a .NET multi-architecture container image via a single Dockerfile using the `docker buildx` command.
- Create GitHub Actions workflow to;

  - Push finished multi-architecture container images to GitHub packages.
  - Push packaged Helm chart to GitHub packages.

## Runtime Identifiers

RID is short for [Runtime Identifier](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog), these are the RID's which identify the target platforms I am interested in deploying my .NET application to;

- linux-x64 (Most desktop distributions like CentOS, Debian, Fedora, Ubuntu, and derivatives)
- linux-arm64 (Linux distributions running on 64-bit ARM like Ubuntu Server 64-bit on Raspberry Pi Model 3+)
- linux-arm (Linux distributions running on ARM like Raspbian on Raspberry Pi Model 2+)
  - Note: this architecture was not _plain sailing_, but I used a [great solution here](https://github.com/dotnet/dotnet-docker/issues/1537#issuecomment-755351628).

## Run Demo Build

The .NET workload is an ultra simple worker process (i.e. a console application) which outputs a number of environment variables in a loop for debugging.

First clone the repository (ideally by opening it as [vscode devcontainer](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)) and then from the root of the repository execute either;

  ```powershell
  #demo script PowerShell version
  ./launch.ps1
  ```
  
  Or
  
  ```bash
  #demo script Shell version (also below)
  . launch.sh
  ```

### Shell Demo Script
```shell
#!/bin/sh

#set variables to emulate running in the workflow/pipeline
GIT_REPO=$(basename `git rev-parse --show-toplevel`)
GIT_BRANCH=$(git branch --show-current)
GIT_COMMIT=$(git rev-parse HEAD)
GIT_TAG="dev"
GITHUB_WORKFLOW="n/a"
GITHUB_RUN_ID=0
GITHUB_RUN_NUMBER=0
IMAGE_NAME="$GIT_REPO:$GIT_TAG"
#Note: you cannot export a buildx container image with manifests, so you have to select just a single architecture
#$PLATFORM="linux/amd64,linux/arm64,linux/arm/v7"
PLATFORM="linux/amd64"

#Create a new builder instance
#https://github.com/docker/buildx/blob/master/docs/reference/buildx_create.md
docker buildx create --name multiarchcontainerdotnet --use

#Start a build
#https://github.com/docker/buildx/blob/master/docs/reference/buildx_build.md
docker buildx build \
    -t $IMAGE_NAME \
    -t "$GIT_REPO:latest" \
    --label "GITHUB_RUN_ID=$GITHUB_RUN_ID" \
    --label "IMAGE_NAME=$IMAGE_NAME" \
    --build-arg GIT_REPO=$GIT_REPO \
    --build-arg GIT_TAG=$GIT_TAG \
    --build-arg GIT_BRANCH=$GIT_BRANCH \
    --build-arg GIT_COMMIT=$GIT_COMMIT \
    --build-arg GITHUB_WORKFLOW=$GITHUB_WORKFLOW \
    --build-arg GITHUB_RUN_ID=$GITHUB_RUN_ID \
    --build-arg GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER \
    --platform $PLATFORM \
    --pull \
    -o type=docker \
    .

#Preview matching images
#https://docs.docker.com/engine/reference/commandline/images/
docker images $GIT_REPO

read -p "Hit ENTER to run the '$IMAGE_NAME' image..."

#Run the multi-architecture container image
#https://docs.docker.com/engine/reference/commandline/run/
docker run --rm -it --name $GIT_REPO $IMAGE_NAME
```

## Docker, Container & .NET Resources

- I highly recommend reading the official Docker blog posts about multi-arch images;

  - https://www.docker.com/blog/multi-arch-images/
  - https://www.docker.com/blog/multi-arch-build-and-images-the-simple-way/
  - https://www.docker.com/blog/faster-multi-platform-builds-dockerfile-cross-compilation-guide/

- Official Docker documentation about support/implementation for multi-arch images;

  - https://github.com/docker/buildx
  - https://docs.docker.com/desktop/multi-arch/
  - https://docs.docker.com/buildx/working-with-buildx/
  - https://docs.docker.com/engine/reference/commandline/buildx_build/

- Official Microsoft documentation useful for multi-arch .NET application builds;

  - https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build
  - https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
  - https://docs.microsoft.com/en-us/azure/container-registry/push-multi-architecture-images
  - https://dotnet.microsoft.com/en-us/download/dotnet/7.0
