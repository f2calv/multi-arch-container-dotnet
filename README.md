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

## Demo

The .NET workload is an ultra simple worker process (i.e. a console application) which outputs a number of environment variables in a loop for debugging.

Clone the repository (ideally open it as devcontainer) and execute the following PowerShell script from the root of the repository;

```powershell
#build the multi-arch image locally with Docker Desktop;
docker buildx create --name multiarchtest --use
docker buildx build -t multi-arch-container-dotnet:dev -f Dockerfile.multiarch --platform linux/arm/v7 --pull -o type=docker .
#check the image exists locally
docker images multi-arch-container-dotnet:dev
#run the multi-arch image on your local Docker Desktop installation (this will use the AMD64 image, assuming you built on an AMD64 CPU architecture!);
docker run --rm -it --name multi-arch-container-dotnet multi-arch-container-dotnet:dev

#run a pre-built multi-arch image on a Kubernetes cluster (this will use the AMD64, ARM64 or ARM32 image depending on your cluster CPU architecture);
kubectl run -i --tty --attach multi-arch-container-dotnet --image=ghcr.io/f2calv/multi-arch-container-dotnet --image-pull-policy='Always'
#watch for successful pod creation
kubectl get po -w
#attach to view the pod logs
kubectl logs -f multi-arch-container-dotnet
```

For local execution there are two PowerShell scripts you can try, which will build and push the images to your own Docker Hub account;

```bash
#build/push/run a single architecture image using a 'vanilla' Dockerfile.
. docker-build.sh

#multi-build/multi-push/run a multi-architecture image, using a heavily customised Dockerfile.
. docker-build-multiarch.sh
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
