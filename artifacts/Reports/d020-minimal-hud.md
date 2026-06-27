# D-020 Minimal HUD

Date: 2026-06-27

## Goal Support

This pass moves `D020VerticalSlice` closer to a commercial vertical-slice loop by
adding a player-visible HUD for the room's current playable proof: tool state,
reward state, progress counts, and the next compact prompt.

## Systems Touched

- D-020 runtime HUD.
- D-020 generated scene wiring.
- D-020 automated editor smoke validation.

## Files Added Or Changed

- `Assets/Scripts/D020HudController.cs`
- `Assets/Editor/FourfoldD020SliceSceneBuilder.cs`
- `Assets/Editor/FourfoldD020PlayableSmoke.cs`
- `Assets/Scenes/D020VerticalSlice.unity`
- `artifacts/Reports/d020-minimal-hud.md`

## Implementation

- Added `D020HudController`, a narrow runtime HUD component that displays:
  tool readiness/cooldown, relic lock/ready/claimed state, saved shortcut/reward
  counts, and the next room prompt.
- Kept the HUD scoped to the D-020 proof and avoided inventory, quest log,
  minimap, extra tool, or new package dependencies.
- Updated the D-020 scene builder to generate a `D020 HUD` object wired to the
  existing player, ExplorationTool, ExplorationNode, reward, and progress save.
- Extended the automated smoke to assert the HUD reads initial tool/reward
  state, tool cooldown after use, reward claimed state, and saved/loaded
  progress counts.

## Tests

- `git diff --check -- <changed exact paths>`
  - Result: passed.
- `node Scripts/Validation/validate_repo.mjs`
  - Result: passed. Required reset files present: 52.
- Public repo hygiene scan
  - Result: passed. Scanned tracked/untracked files: 216.
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

- Added new runtime component `D020HudController` under
  `FourfoldEchoes.Product`.
- No existing public method signatures were changed.

## Acceptance Conditions

- The room now has player-visible UI for tool state, reward state, progress, and
  prompt information.
- The HUD is covered by the automated D-020 smoke.
- The implementation stays within the one-tool/no-inventory/no-quest-log scope.

## Remaining Risk

- This is a D-020 proof HUD, not the final art-directed HUD skin.
- Health and boss health are still outside this single-room D-020 proof and
  belong to the broader vertical-slice combat/boss lanes.

## Next Smallest Useful Task

Add one lightweight D-020 capture candidate that shows the HUD during the
reward-save moment, then fold that evidence into the D-020 validation report.
