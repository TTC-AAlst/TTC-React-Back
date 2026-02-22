# Install git hooks for this repository
Write-Host "Installing git hooks..."
git config core.hooksPath hooks
Write-Host "Git hooks installed. Pre-commit and pre-push hooks are now active."
