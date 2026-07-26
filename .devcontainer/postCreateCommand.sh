#!/bin/sh

echo "postCreateCommand.sh"
echo "--------------------"

sudo chmod +x .devcontainer/postStartCommand.sh

pre-commit install --install-hooks
