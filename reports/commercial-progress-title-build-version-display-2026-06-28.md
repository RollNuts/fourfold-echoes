# Commercial Progress: Title Build Version Display

Date: 2026-06-28

## Goal Support

This lane closes part of the Steam release QA plan by making the current build
version and commit identifier visible from the player-facing title screen. It
supports bug reports, hotfix triage, clean-launch smoke evidence, and store QA
without adding a new gameplay system.

## Systems Touched

- Title UI
- Build automation
- D022 product contract validation

## Files Added Or Changed

- `Assets/Scripts/FourfoldBuildInfo.cs`
- `Assets/Scripts/FourfoldBuildInfo.cs.meta`
- `Assets/Scripts/TitleSceneController.cs`
- `Assets/Editor/FourfoldUnityBuild.cs`
- `Assets/Editor/FourfoldD022ProductContractVerifier.cs`
- `reports/commercial-progress-title-build-version-display-2026-06-28.md`

## Implementation

- Added a small title-screen build line: `Build {version} / commit {shortSha}`.
- Added `FourfoldBuildInfo`, which reads build metadata from a generated
  Resources text asset and falls back to safe local-dev values in editor runs.
- Updated `FourfoldUnityBuild` to write the version and commit metadata before
  build, include it in the player, and restore the local project file state
  after the build.
- Extended the D022 product contract so the version/commit display cannot
  disappear silently.

## Tests

- `git diff --check` on the exact changed paths: passed.
- `node Scripts/Validation/validate_repo.mjs`: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed.
- `node tools/forge/check.mjs`: passed.
- Secret/private-path scan on the exact changed text paths: passed.
- Unity 6000.3.18f1 batchmode
  `FourfoldEchoes.Editor.FourfoldD022ProductContractVerifier.VerifyD022Contract`:
  exit code 0.

## Acceptance Conditions

- The title screen displays build version and commit information inside the
  1280x800 and 1920x1080 safe area.
- Product builds can inject commit metadata without committing generated
  Resources files.
- D022 validation fails if the title build identifier path is removed.

## Next Smallest Useful Task

Use the same build identifier in the clean-launch smoke report so QA evidence,
bug reports, and store-capture manifests can be tied to the exact build.
