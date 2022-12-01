FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /repo
COPY ["src/dotnetmultiarchapp/dotnetmultiarchapp.csproj", "src/dotnetmultiarchapp/"]
RUN dotnet restore "src/dotnetmultiarchapp/dotnetmultiarchapp.csproj"
COPY . .
RUN dotnet publish "src/dotnetmultiarchapp/dotnetmultiarchapp.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
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

ARG GITHUB_WORKFLOW
ENV GITHUB_WORKFLOW=$GITHUB_WORKFLOW
ARG GITHUB_RUN_ID
ENV GITHUB_RUN_ID=$GITHUB_RUN_ID
ARG GITHUB_RUN_NUMBER
ENV GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER

EXPOSE 80

ENTRYPOINT ["dotnet", "dotnetmultiarchapp.dll"]