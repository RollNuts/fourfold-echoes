# Feature Report: D020VerticalSlice

Date: 2026-06-28

## Goal Support

This lane moves `D020VerticalSlice` closer to the controllable Region 01 test room target by adding the second relic reward required by the D-020 vertical slice contract. It keeps the MVP boundary intact: one tool, two rewards, no inventory, no crafting, and no new broad systems.

## Systems Touched

- D020 generated scene content
- ExplorationTool node binding
- Relic reward unlock logic
- Minimal HUD prompt/progress readout
- D020 editor smoke validation
- Package-free EditMode smoke under `Assets/Tests/EditMode`
- Meshwright asset readiness triage

## Files Added Or Changed

- `Assets/Scripts/D020HudController.cs`
- `Assets/Editor/FourfoldD020SliceSceneBuilder.cs`
- `Assets/Editor/FourfoldD020PlayableSmoke.cs`
- `Assets/Editor/FourfoldUnityEvidenceCapture.cs`
- `Assets/Scenes/D020VerticalSlice.unity`
- `Assets/Tests/EditMode/D020RelicRewardEditModeSmoke.cs`
- `Scripts/Validation/write_market_reports.mjs`
- `artifacts/Previews/d020-*.png`
- `artifacts/Reports/final-status-report.*`
- `artifacts/Reports/performance-snapshot.*`
- `reports/feature-D020VerticalSlice.md`

## Implementation

- Added `D020 Shortcut Relic Cache` as a second generated reward near the shortcut route.
- Kept `D020 Relic Chest` as the chamber reward gated by enemy defeat plus both one-tool responses.
- Bound both rewards to the existing `D020ProgressSave.rewards` array and reward ids so progress reads and save/load restore `Progress S2 R2`.
- Added optional `D020HudController.rewards` so the HUD shows aggregate two-relic progress while keeping the existing single `reward` field compatible.
- Updated camera evidence capture so the HUD proof captures both relics as collected.

## Meshwright Triage

- Ran Meshwright state, queue, pro-audit, delivery-manifest, game-integration, game-sync, one `unity_prefab` attempt for the character enemy candidate, and unity-health.
- Verdict for this lane: `Preview only`, not gameplay-bind ready.
- Reason: source/model/texture/preflight evidence is present, but required Unity Material/Prefab outputs are still missing and the prefab builder reported a blocked build.
- Meshwright command results: pro-audit exit 0 with `useful_internal_beta`; delivery-manifest exit 2 with `incomplete`; game-integration exit 0 with `needs_prefabs`; game-sync dry-run exit 0 with missing Material/Prefab outputs; one `unity_prefab` attempt exit 2; unity-health exit 2 with `prefab_builder_failed`.
- No Meshwright generated asset was copied into this runtime PR.

## Tests And Validation

- `git diff --check -- <changed exact paths>`: passed
- `node Scripts/Validation/validate_repo.mjs`: passed
- `node check_public_repo_hygiene.mjs` against this worktree: passed
- Secret/private path scan over changed exact paths: passed, no matches
- Unity `FourfoldEchoes.Editor.FourfoldD020SliceSceneBuilder.BuildAndValidate`: exit code 0
- Unity `FourfoldEchoes.Editor.FourfoldD020PlayableSmoke.Run`: exit code 0
- Unity `FourfoldEchoes.Tests.D020RelicRewardEditModeSmoke.Run`: exit code 0
- Unity `FourfoldEchoes.Editor.FourfoldUnityEvidenceCapture.CaptureD020Slice`: exit code 0

## Public API Impact

- Added optional public fields:
  - `D020HudController.rewards`
- Existing fields and methods remain compatible. Existing one-reward setups keep working because the new array is optional and null-safe.

## Acceptance Conditions

- The same ExplorationTool solves both the shortcut response and reward-lens response.
- Shortcut relic cache unlocks after the shortcut response.
- Chamber relic reward remains locked until enemy defeat plus both one-tool responses.
- HUD exposes two-relic state as `Relics 0/2` through `Relics 2/2`.
- Progress save/load restores both solved nodes and both collected rewards as `Progress S2 R2`.

## Notes

- Unity import generated an untracked `Assets/Audio.meta`; it was left unstaged as outside this lane.
- Raw Unity/Meshwright logs, machine-local paths, and credentials are not included in this report.

## Next Smallest Useful Task

Fix Meshwright prefab generation for one production-intent runtime asset, then reassess it as gameplay-bind ready before importing it into D020.
