if ([string]::IsNullOrEmpty($DOCKERHUB_USERNAME)) {
    Write-Error "Please set DOCKERHUB_USERNAME!"
    return 1
}

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

& "docker" login

$IMAGE_NAME = "multi-arch-container-dotnet"
$REPOSITORY = "$DOCKERHUB_USERNAME/$IMAGE_NAME"
$REPO_ROOT = git rev-parse --show-toplevel
$GIT_REPO = $REPO_ROOT | Split-Path -Leaf
$GIT_TAG = "latest-dev"
$GIT_BRANCH = $(git branch --show-current)
$GIT_COMMIT = $(git rev-parse HEAD)
$GITHUB_WORKFLOW = "n/a"
$GITHUB_RUN_ID = 0
$GITHUB_RUN_NUMBER = 0
