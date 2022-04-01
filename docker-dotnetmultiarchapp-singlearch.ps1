. ./.scripts/Invoke-Setup.ps1

& "docker" build -t "$($REPOSITORY):$($GIT_TAG)" `
    -f src/$IMAGE_NAME/Dockerfile `
    --label "GITHUB_RUN_ID=${GITHUB_RUN_ID}" `
    --label "IMAGE_NAME=$IMAGE_NAME" `
    --build-arg GIT_REPO=$GIT_REPO `
    --build-arg GIT_TAG=$GIT_TAG `
    --build-arg GIT_BRANCH=$GIT_BRANCH `
    --build-arg GIT_COMMIT=$GIT_COMMIT `
    --build-arg GITHUB_WORKFLOW=$GITHUB_WORKFLOW `
    --build-arg GITHUB_RUN_ID=$GITHUB_RUN_ID `
    --build-arg GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER `
    --pull `
    .

& "docker" push "$($REPOSITORY):$($GIT_TAG)"

Write-Host "Hit any key to run the image..." 
pause

docker run --rm -it --name $IMAGE_NAME "$($REPOSITORY):$($GIT_TAG)"
