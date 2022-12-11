Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

#set variables to emulate running in the workflow/pipeline
$REPO_ROOT = git rev-parse --show-toplevel
$GIT_REPO = $REPO_ROOT | Split-Path -Leaf
$GIT_TAG = "dev"
$GIT_BRANCH = $(git branch --show-current)
$GIT_COMMIT = $(git rev-parse HEAD)
$GITHUB_WORKFLOW = "n/a"
$GITHUB_RUN_ID = 0
$GITHUB_RUN_NUMBER = 0
$IMAGE_NAME = "$($GIT_REPO):$($GIT_TAG)"

#Create a new builder instance
#https://github.com/docker/buildx/blob/master/docs/reference/buildx_create.md
docker buildx create --name multiarchcontainerdotnet --use

#Start a build
#https://github.com/docker/buildx/blob/master/docs/reference/buildx_build.md
docker buildx build `
    -t $IMAGE_NAME `
    --build-arg GIT_REPO=$GIT_REPO `
    --build-arg GIT_TAG=$GIT_TAG `
    --build-arg GIT_BRANCH=$GIT_BRANCH `
    --build-arg GIT_COMMIT=$GIT_COMMIT `
    --build-arg GITHUB_WORKFLOW=$GITHUB_WORKFLOW `
    --build-arg GITHUB_RUN_ID=$GITHUB_RUN_ID `
    --build-arg GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER `
    --platform linux/amd64,linux/arm64,linux/arm/v7 `
    --pull `
    -o type-docker `
    .

#Preview matching images
#https://docs.docker.com/engine/reference/commandline/images/
docker images $GIT_REPO

Write-Host "Hit any key to run the '$IMAGE_NAME' image..." 
pause

#Run the multi-architecture container image
#https://docs.docker.com/engine/reference/commandline/run/
docker run --rm -it --name $GIT_REPO $IMAGE_NAME