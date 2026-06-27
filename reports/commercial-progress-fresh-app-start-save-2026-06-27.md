# Commercial Progress: Fresh App-Start Save Proof

Date: 2026-06-27

## 1. Final Goal Support

This pass tightens the local-save contract for the commercial vertical slice.
`ProductionCombatSlice` now has automated coverage for restoring completed
reward progress from disk after the production scene has been unloaded.

## 2. Systems Touched

- PlayMode smoke coverage
- Production slice save restoration evidence
- Test plan and commercial progress reports

No runtime feature, package, ProjectSettings, render pipeline, inventory,
quest, open-world, network, or backend scope was added.

## 3. Files Added / Changed

- Changed: `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs`
- Changed: `docs/test-plan.md`
- Changed: `reports/commercial-gap-map.md`
- Changed: `reports/commercial-progress-title-pause-clear-2026-06-27.md`
- Changed: `reports/production-combat-feedback-save-status.md`
- Added: `reports/commercial-progress-fresh-app-start-save-2026-06-27.md`

Existing dirty files outside this lane were left untouched.

## 4. Implementation

Added `SLICE_PRODUCTION_FreshAppStartRestoresSavedRewardFromDisk`.

The test:

- completes the production slice reward route
- verifies `Progress saved`
- verifies the save file exists
- unloads `ProductionCombatSlice` by loading `D020VerticalSlice`
- verifies the old production controller is gone
- reloads `ProductionCombatSlice`
- begins a fresh run and verifies completed reward state restores from disk

This is an automated fresh-start-equivalent proof. It does not claim a full
application relaunch or controller-device playthrough.

## 5. Tests

Static checks executed for this lane:

- `git diff --check` on the exact changed paths
- `git diff --cached --check` on the exact staged paths
- `node Scripts/Validation/validate_repo.mjs`
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
- sanitization scan over changed exact paths

All static checks passed.

Unity rerun status:

- Not completed in this pass. The checkout currently has Unity lockfile
  evidence, and a later isolated-worktree Unity attempt failed during licensing
  initialization before Test Runner XML was produced.
- Do not start another Unity validation against this project until the active
  editor/session state is clear and the licensing channel has recovered.

## 6. Acceptance Conditions

Accept this pass when:

- the new PlayMode test compiles
- the updated PlayMode suite passes in a serialized Unity run
- static validation passes
- no raw Unity logs, local paths, credentials, or non-public links are included

## 7. Next Smallest Useful Task

Run the updated PlayMode suite in serialized Unity, then run a Windows build
smoke for the playable slice.
