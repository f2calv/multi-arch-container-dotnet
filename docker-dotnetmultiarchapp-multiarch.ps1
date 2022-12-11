. ./.scripts/Invoke-Setup.ps1

docker buildx create --name multiarchtestdotnet --use

& "docker" buildx build -t "$($REPOSITORY):$($GIT_TAG)" `
    -f Dockerfile.multiarch `
    --label "GITHUB_RUN_ID=${GITHUB_RUN_ID}" `
    --label "IMAGE_NAME=$IMAGE_NAME" `
    --build-arg GIT_REPO=$GIT_REPO `
    --build-arg GIT_TAG=$GIT_TAG `
    --build-arg GIT_BRANCH=$GIT_BRANCH `
    --build-arg GIT_COMMIT=$GIT_COMMIT `
    --build-arg GITHUB_WORKFLOW=$GITHUB_WORKFLOW `
    --build-arg GITHUB_RUN_ID=$GITHUB_RUN_ID `
    --build-arg GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER `
    --platform linux/amd64,linux/arm64,linux/arm/v7 `
    --push `
    --pull `
    .
