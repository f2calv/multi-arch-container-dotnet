#!/usr/bin/env bash

# Reports the language and Kubernetes client versions on container start.

set -euo pipefail

echo "postStartCommand.sh"
echo "-------------------"

dotnet --version
kubectl version --client --output=yaml | head -2

echo "Done"
