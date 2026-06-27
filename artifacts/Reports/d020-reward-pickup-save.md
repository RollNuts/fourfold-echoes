# D-020 Reward Pickup And Save

Date: 2026-06-27

## Goal Support

This pass advances `D020VerticalSlice` from a controllable room proof toward a
commercial vertical-slice loop by turning the visible relic reward into a
collectible pickup and persisting the shortcut/reward progress flags through a
local save file.

## Systems Touched

- D-020 reward pickup runtime.
- D-020 local progress save runtime.
- D-020 generated scene wiring.
- D-020 automated editor smoke validation.

## Files Added Or Changed

- `Assets/Scripts/D020RelicReward.cs`
- `Assets/Scripts/D020ProgressSave.cs`
- `Assets/Editor/FourfoldD020SliceSceneBuilder.cs`
- `Assets/Editor/FourfoldD020PlayableSmoke.cs`
- `Assets/Scenes/D020VerticalSlice.unity`
- `artifacts/Reports/d020-reward-pickup-save.md`

## Implementation

- Added `D020RelicReward` as a tiny runtime component for one relic pickup, with
  enemy/tool-solution gating, explicit idle/collected reads, and the existing
  generated relic pickup SFX.
- Added `D020ProgressSave` as a narrow JSON local-save component for D-020
  progress flags: solved `ExplorationNode` ids and collected reward ids.
- Updated the D-020 scene builder so the generated room contains the reward
  component, pickup feedback, SFX wiring, and a runtime progress-save hook.
- Extended the automated smoke to collect the reward, save shortcut/reward
  state, reset the scene state, reload it, and assert both flags restore.

## Tests

- `git diff --check -- <changed exact paths>`
  - Result: passed.
- `node Scripts/Validation/validate_repo.mjs`
  - Result: passed. Required reset files present: 52.
- Public repo hygiene scan
  - Result: passed. Scanned tracked/untracked files: 213.
- Secret/private path scan over changed exact paths
  - Result: passed. No personal local path, credential assignment, private key,
    credentialed URL, or database URL pattern found.
- Unity D-020 scene validation
  - Method:
    `FourfoldEchoes.Editor.FourfoldD020SliceSceneBuilder.BuildAndValidate`
  - Result: passed, exit code 0.
- Unity D-020 playable smoke
  - Method: `FourfoldEchoes.Editor.FourfoldD020PlayableSmoke.Run`
  - Result: passed, exit code 0.

## Warnings

- Unity emitted non-failing CoreBusinessMetrics SQLite cache warnings.
- Unity emitted non-failing shutdown noise from licensing, ADB, .NET SDK
  discovery, and debugger cleanup after the validation methods passed.
- No compile errors or smoke failures were present in the final Unity results.

## Public API Impact

- Added new runtime components `D020RelicReward` and `D020ProgressSave` under
  `FourfoldEchoes.Product`.
- No existing public method signatures were changed.

## Acceptance Conditions

- Reward is no longer only a readable chest; the relic can be collected.
- The generated room now has a persisted shortcut flag and reward flag.
- The smoke remains automated through Unity batchmode and does not require
  scene-dependent manual clicking.

## Remaining Risk

- This is still the first single-room D-020 proof, not the full two-reward
  vertical slice.
- Save data is deliberately narrow to D-020 flags and should later be folded
  into the final MVP save service once that architecture is accepted.
- The SFX are existing generated pilot clips and still need final audio
  direction approval before market-facing capture.

## Next Smallest Useful Task

Add a minimal in-room HUD read for tool cooldown, reward collected state, and
save confirmation without introducing inventory or quest-log scope.
