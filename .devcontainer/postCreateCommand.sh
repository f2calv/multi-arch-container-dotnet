#!/bin/sh

echo "postCreateCommand.sh"
echo "--------------------"

sudo chmod +x .devcontainer/postStartCommand.sh

# Note: the pre-commit git hook is deliberately NOT installed here. Its cost is fixed
# interpreter start-up per hook rather than per file, so even a one-file commit pays the
# full price - which is painful on slower hardware. Run it manually instead:
#
#     pre-commit run --all-files
#
# Or opt in to a once-per-push hook, far cheaper than once-per-commit:
#
#     pre-commit install --hook-type pre-push --install-hooks
#
# Either way the `lint` job in .github/workflows/ci.yml is the authoritative gate.
