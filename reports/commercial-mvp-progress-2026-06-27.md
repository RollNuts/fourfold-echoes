# Commercial MVP Progress Cycle

Date: 2026-06-27

## 1. Final Goal Support

This cycle moves FOURFOLD ECHOES toward the Steam-first commercial MVP by
aligning default build evidence with the first shipping platform: Steam
Windows. The project can still keep macOS as local development evidence, but
the CI/default D-020 slice build should prove the platform named by the product
spec and Steam release plan.

## 2. Systems Touched

- Build automation: D-020 slice build target selection.
- GitHub Actions: default branch build workflow.
- Production documentation: CI/build notes and platform notes.
- Scope control: no gameplay system, content cap, package, scene reference,
  input package, networking, inventory, quest, or live-service behavior added.

## 3. Files Added/Changed

- Changed: `Assets/Editor/FourfoldUnityBuild.cs`
- Changed: `.github/workflows/build.yml`
- Changed: `docs/ci.md`
- Changed: `docs/Tech/PLATFORM_NOTES.md`
- Changed: `docs/forge/EDITOR_AUTOMATION.md`
- Added: `reports/commercial-mvp-progress-2026-06-27.md`

## 4. Implementation

- Changed `BuildCurrentD020Slice` so its no-argument default target is
  `windows`.
- Kept the historical `BuildGateA` default target unchanged by making
  `GetRequestedTarget` accept an optional default value.
- Updated the default branch GitHub Actions build from `StandaloneOSX` to
  `StandaloneWindows64`.
- Updated build artifact naming and custom build parameters from `macos` to
  `windows`.
- Updated CI and platform docs so Windows evidence is the default commercial
  path and macOS remains development evidence unless a later product decision
  changes that.

## 5. Tests

Static checks run:

- `ruby -e "require 'psych'; Psych.load_file('.github/workflows/build.yml'); Psych.load_file('.github/workflows/validate.yml'); Psych.load_file('.github/workflows/security.yml'); puts 'yaml ok'"`
  - Result: passed.
- `git diff --check`
  - Result: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
  - Result: passed; scanned tracked and untracked files.
- `node Scripts/Validation/validate_repo.mjs`
  - Result: passed.
- `node tools/forge/check.mjs`
  - Result: passed.
- `rg -n "D-020 macOS|macos-build|StandaloneOSX|fourfoldBuildTarget macos|default-branch D-020 macOS|Windows builds are intentionally" .github docs Assets/Editor`
  - Result: only intentional legacy `StandaloneOSX` support remains in
    `FourfoldUnityBuild` for explicit macOS/Gate A builds.

Unity build/test execution was not run in this cycle. The previous QA gap note
already records that local Unity batch tests were blocked by an open editor
instance. The next verification should run through the active editor or after
closing the open Unity process.

## 6. Acceptance Conditions

Accepted for this cycle when:

- The default D-020 CI build targets `StandaloneWindows64`.
- `BuildCurrentD020Slice` defaults to Windows when no explicit target is passed.
- Historical macOS build support remains available through explicit target
  selection.
- Documentation no longer describes the commercial CI path as macOS-first.
- No new runtime gameplay scope or package dependency is introduced by the
  change.

## 7. Next Smallest Useful Task

Run the Unity Test Runner through the open editor, or close the open editor and
run batchmode EditMode plus PlayMode tests. Record the results under `reports/`
or `artifacts/test-results/`, then decide whether the current enemy/UI/save
changes are ready to be staged or need a focused compile/runtime fix.

## Launch-Blocker Notes

- Launch blocker: no current Windows build artifact has been produced from this
  checkout.
- Launch blocker: controller-first completion of the production slice is not
  verified.
- Non-blocker for this cycle: macOS build support can stay as development
  evidence, provided store/release readiness continues to measure Windows first.
