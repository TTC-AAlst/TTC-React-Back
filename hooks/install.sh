#!/bin/sh
# Install git hooks for this repository
echo "Installing git hooks..."
git config core.hooksPath hooks
echo "Git hooks installed. Pre-commit and pre-push hooks are now active."
