# Unity 6 CI/CD Status

## Current Situation

**Unity 6 (6000.x) is not yet supported by GameCI** - the standard Unity CI/CD solution.

GameCI provides Docker images and GitHub Actions for Unity, but they haven't released support for Unity 6 yet since it's very new (released October 2024).

## What We're Using Instead

We've created a **simplified CI/CD workflow** (`unity-ci-simple.yml`) that:

✅ Validates project structure
✅ Checks for C# scripts and tests
✅ Performs basic code quality checks
✅ Validates documentation
✅ Checks Git LFS configuration

This doesn't run actual Unity tests or builds, but it catches common issues.

## When Will Full CI/CD Work?

GameCI typically adds support for new Unity versions within 1-3 months of release. You can track progress here:
- https://github.com/game-ci/docker
- https://github.com/game-ci/unity-actions

## Options

### Option 1: Wait for GameCI Support (Recommended)
- Keep using Unity 6
- Use simplified workflow for now
- Switch to full CI/CD when GameCI adds support
- **Timeline**: Likely by Q1 2025

### Option 2: Downgrade to Unity 2022 LTS
- Unity 2022 LTS has full GameCI support
- Stable and well-tested for CI/CD
- Can upgrade to Unity 6 later
- **Downside**: Lose Unity 6 features

### Option 3: Custom Unity Installation
- Install Unity manually in workflow
- Complex and slow (~20-30 minutes per run)
- Not recommended unless urgent need
- **Downside**: Expensive in CI minutes

## Current Workflow Status

- ✅ `unity-ci-simple.yml` - Active (project validation)
- ⚠️ `unity-ci.yml` - Disabled (waiting for Unity 6 support)

## Recommendation

**Stick with Unity 6 and use the simplified workflow for now.** GameCI will add support soon, and Unity 6 has significant improvements worth keeping.

In the meantime:
1. Run Unity tests locally before pushing
2. Use the simplified workflow to catch structural issues
3. Monitor GameCI for Unity 6 support announcements

## Checking for Updates

To check if Unity 6 is supported yet:

```bash
# Check available Unity versions in GameCI
curl -s https://hub.docker.com/v2/repositories/unityci/editor/tags?page_size=100 | grep "6000"
```

If you see tags like `ubuntu-6000.2.10f1-base-1`, Unity 6 support is available!
