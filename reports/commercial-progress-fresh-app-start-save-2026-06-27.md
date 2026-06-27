# Commercial Progress: Fresh App-Start Save Proof

Date: 2026-06-27

## 1. Final Goal Support

This pass tightens the local-save and build-readiness contract for the
commercial vertical slice. `ProductionCombatSlice` now keeps restored reward
state visible in runtime event text, and the repo has a production-slice build
entry that defaults to the Windows-first target.

## 2. Systems Touched

- Runtime combat slice controller feedback
- PlayMode smoke coverage
- Production slice save restoration evidence
- Unity editor build automation
- Production scene validation
- Test plan and commercial progress reports

No package, ProjectSettings, render pipeline, inventory, quest, open-world,
network, or backend scope was added.

## 3. Files Added / Changed

- Changed: `Assets/Editor/FourfoldUnityBuild.cs`
- Changed: `Assets/Scripts/ProductionCombatSliceController.cs`
- Changed: `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs`
- Changed: `docs/forge/EDITOR_AUTOMATION.md`
- Changed: `reports/commercial-gap-map.md`
- Changed: `reports/commercial-progress-fresh-app-start-save-2026-06-27.md`

Existing dirty files outside this lane were left untouched.

## 4. Implementation

Added the production-slice build method and tightened the saved-progress
feedback path.

Runtime changes:

- restored slice progress now sets player-visible event text
- restored completed reward progress reports `Saved reward restored`
- reward claiming accepts the same gamepad use button as the Echo Tool

Build automation changes:

- `FourfoldUnityBuild.BuildProductionCombatSlice` targets
  `ProductionCombatSlice`
- the method defaults to Windows and validates the generated scene before build

PlayMode coverage now includes
`SLICE_PRODUCTION_FreshAppStartRestoresSavedRewardFromDisk`.

The test:

- completes the production slice reward route
- verifies `Progress saved`
- verifies the save file exists
- unloads `ProductionCombatSlice` by loading `D020VerticalSlice`
- verifies the old production controller is gone
- reloads `ProductionCombatSlice`
- begins a fresh run and verifies completed reward state restores from disk
- verifies the restored event text is `Saved reward restored`

This is an automated fresh-start-equivalent proof. It does not claim a full
application relaunch or controller-device playthrough.

## 5. Tests

Static checks executed for this lane:

- `git diff --check` on the exact changed paths
- `node Scripts/Validation/validate_repo.mjs`
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
- sanitization scan over changed exact paths

All static checks passed.

Unity rerun status:

- Isolated production scene validation passed with exit code 0.
- Production scene validation reported 159 prefab instances, 87 distinct
  prefabs, and 156 renderers.
- PlayMode was attempted twice in the isolated worktree. Both attempts imported
  and compiled the project, then Unity shut down without producing Test Runner
  XML. No PlayMode pass is claimed for the latest test.
- Windows build smoke reached `BuildProductionCombatSlice`, then exited 1
  because this editor does not have `StandaloneWindows64` support installed.
  No build artifact was produced.

Warnings handled:

- Unity CoreBusinessMetrics SQLite cache warnings appeared during all batchmode
  runs and are treated as local metrics-cache noise.
- Unity licensing entitlement lookup messages appeared but did not block scene
  validation or the build-support check.
- Empty-folder `.meta` warnings were emitted for tracked asset folders during
  import; they are outside this lane.
- .NET build-server and ADB shutdown messages appeared after Unity shutdown and
  are treated as local environment noise.

## 6. Acceptance Conditions

Accept this pass when:

- the new PlayMode test compiles
- the production scene validator passes in serialized Unity
- static validation passes
- no raw Unity logs, local paths, credentials, or non-public links are included

Remaining acceptance blockers before release-candidate build readiness:

- regenerate PlayMode Test Runner XML and get the updated suite to exit 0
- install/use Windows Standalone build support and produce a current artifact

## 7. Next Smallest Useful Task

Restore reliable PlayMode Test Runner XML generation in batchmode, then rerun
the updated suite and the production Windows build smoke on an editor with
`StandaloneWindows64` support.
