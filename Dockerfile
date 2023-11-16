#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo
COPY ["src/multi-arch-container-dotnet/multi-arch-container-dotnet.csproj", "src/multi-arch-container-dotnet/"]
RUN dotnet restore "src/multi-arch-container-dotnet/multi-arch-container-dotnet.csproj"
COPY . .
ARG TARGETPLATFORM
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=linux-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=linux-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
    RID=linux-arm ; \
    fi \
    && dotnet publish "src/multi-arch-container-dotnet/multi-arch-container-dotnet.csproj" -c Release -o /app/publish -r $RID --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ARG GIT_REPO
ENV GIT_REPO=$GIT_REPO
ARG GIT_TAG
ENV GIT_TAG=$GIT_TAG
ARG GIT_BRANCH
ENV GIT_BRANCH=$GIT_BRANCH
ARG GIT_COMMIT
ENV GIT_COMMIT=$GIT_COMMIT

ARG GITHUB_WORKFLOW=n/a
ENV GITHUB_WORKFLOW=$GITHUB_WORKFLOW
ARG GITHUB_RUN_ID=0
ENV GITHUB_RUN_ID=$GITHUB_RUN_ID
ARG GITHUB_RUN_NUMBER=0
ENV GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER

ENTRYPOINT ["dotnet", "multi-arch-container-dotnet.dll"]