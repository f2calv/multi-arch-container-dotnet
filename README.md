# Multi-Architecture Container Image for a .NET 7.0 app

The back story here is that I started playing around with a Kubernetes cluster running on my Raspberry Pi 4. I wished to deploy a workload running [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) and it turned out I had skill-up on building/deploying containers on CPU architectures other than AMD64 so I've built a demo repository to document the process in code along with related resource links.

Key lessons learned;

- .NET multi-architecture container image builds using the `docker buildx` command.
- Using GitHub container registry for container images+helm charts.
- GitHub actions workflow for container image+chart construction/publishing.

The .NET 7.0 workload is a simple worker process which outputs a number of environment variables in a loop for debugging.

I'd highly recommend reading the official Docker blog posts and if you find this repository useful please star it :)

## Runtime Identifiers

RID is short for [Runtime Identifier](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog), these are the RID's which identify the target platforms I am interested in deploying my .NET application to;

- linux-x64 (Most desktop distributions like CentOS, Debian, Fedora, Ubuntu, and derivatives)
- linux-arm64 (Linux distributions running on 64-bit ARM like Ubuntu Server 64-bit on Raspberry Pi Model 3+)
- linux-arm (Linux distributions running on ARM like Raspbian on Raspberry Pi Model 2+)
  - Note: this architecture was not _plain sailing_, but I used a [great solution here](https://github.com/dotnet/dotnet-docker/issues/1537#issuecomment-755351628).

## Demo

Clone the repo and execute the following from the root of the repository;

```pwsh
#build the multi-arch image with Docker Desktop;
docker buildx create --name multiarchtest --use
docker buildx build -t dotnetmultiarchapp:dev -f src/dotnetmultiarchapp/Dockerfile.multiarch --platform linux/amd64,linux/arm64,linux/arm/v7 --pull .

#run a pre-built multi-arch image on your local Docker Desktop installation (this will use the AMD64 image);
docker run --rm -it --name dotnetmultiarchapp ghcr.io/f2calv/dotnetmultiarchapp

#run a pre-built multi-arch image on a Kubernetes cluster (this will use the AMD64 or ARM64 or ARM32 image depending on your cluster);
kubectl create deployment --image=ghcr.io/f2calv/dotnetmultiarchapp dotnetmultiarchapp
#watch for successful pod creation
kubectl get po -w
#attach to view the pod logs
kubectl logs -f dotnetmultiarchapp-????? #<---enter the full pod name here
```

For local execution there are two PowerShell scripts you can play with, which will push the images to your own Docker Hub account;

```pwsh
$DOCKERHUB_USERNAME = "????" #<------------ populate this variable

#build/push/run a single architecture image using a 'vanilla' Dockerfile.
./docker-dotnetmultiarchapp-singlearch.ps1

#multi-build/multi-push/run a multi-architecture image, using a customised Dockerfile.
./docker-dotnetmultiarchapp-multiarch.ps1
```

## Docker, Container & .NET Resources

- Official Docker blog posts about multi-arch images;

  - https://www.docker.com/blog/multi-arch-images/
  - https://www.docker.com/blog/multi-arch-build-and-images-the-simple-way/
  - https://www.docker.com/blog/faster-multi-platform-builds-dockerfile-cross-compilation-guide/

- Official Docker documentation about support/implementation for multi-arch images;

  - https://docs.docker.com/desktop/multi-arch/
  - https://docs.docker.com/buildx/working-with-buildx/
  - https://docs.docker.com/engine/reference/commandline/buildx_build/

- Official Microsoft documentation useful for multi-arch .NET application builds;

  - https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build
  - https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
  - https://docs.microsoft.com/en-us/azure/container-registry/push-multi-architecture-images
  - https://dotnet.microsoft.com/en-us/download/dotnet/7.0
