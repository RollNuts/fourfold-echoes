# Feature Report: D020VerticalSlice

Date: 2026-06-27

## Goal Support

This lane moves `D020VerticalSlice` from a single static proof interaction toward a controllable Region 01 test room by adding a second player-visible response that uses the same ExplorationTool. It keeps the MVP boundary intact: one tool, one reward, no inventory, no new broad systems.

## Systems Touched

- D020 generated scene content
- ExplorationTool node binding
- Relic reward unlock logic
- Minimal HUD prompt/progress readout
- D020 editor smoke validation
- Package-free EditMode smoke under `Assets/Tests/EditMode`

## Files Added Or Changed

- `Assets/Scripts/D020RelicReward.cs`
- `Assets/Scripts/D020HudController.cs`
- `Assets/Editor/FourfoldD020SliceSceneBuilder.cs`
- `Assets/Editor/FourfoldD020PlayableSmoke.cs`
- `Assets/Scenes/D020VerticalSlice.unity`
- `Assets/Tests.meta`
- `Assets/Tests/EditMode.meta`
- `Assets/Tests/EditMode/D020RelicRewardEditModeSmoke.cs`
- `Assets/Tests/EditMode/D020RelicRewardEditModeSmoke.cs.meta`
- `reports/feature-D020VerticalSlice.md`

## Implementation

- Added optional `D020RelicReward.requiredNodes` so a reward can require every configured one-tool node without changing the existing `requiredNode` field.
- Added optional `D020HudController.nodes` so HUD prompts stay on "Use tool on sigil" until all configured nodes are solved.
- Updated the D020 scene builder to generate:
  - `D020 Exploration Tool Node` -> `D020 Shortcut Route`
  - `D020 Reward Lens Node` -> `D020 Reward Lens Response`
- Bound both nodes to the same `ExplorationTool` and to `D020ProgressSave`.
- Required the reward to wait for enemy defeat plus both one-tool responses before pickup.

## Tests And Validation

- `git diff --check -- <changed exact paths>`: passed
- `node Scripts/Validation/validate_repo.mjs`: passed
- `node check_public_repo_hygiene.mjs` against this worktree: passed
- Secret/private path scan over changed exact paths: passed, no matches
- Unity `FourfoldEchoes.Editor.FourfoldD020SliceSceneBuilder.BuildAndValidate`: exit code 0
- Unity `FourfoldEchoes.Editor.FourfoldD020PlayableSmoke.Run`: exit code 0
- Unity `FourfoldEchoes.Tests.D020RelicRewardEditModeSmoke.Run`: exit code 0

## Public API Impact

- Added optional public fields:
  - `D020RelicReward.requiredNodes`
  - `D020HudController.nodes`
- Existing fields and methods remain compatible. Existing one-node setups keep working because the new arrays are optional and null-safe.

## Acceptance Conditions

- Player-visible second one-tool response exists in the generated D020 room.
- The same ExplorationTool solves both the shortcut response and reward-lens response.
- Reward remains locked after enemy defeat plus only the shortcut response.
- Reward unlocks after enemy defeat plus both one-tool responses.
- Progress save/load restores both solved nodes and the collected reward as `Progress S2 R1`.

## Notes

- Unity import generated an untracked `Assets/Audio.meta`; it was left unstaged as outside this lane.
- Raw Unity logs, local absolute paths, tokens, and private URLs are not included in this report.

## Next Smallest Useful Task

Add a small camera/evidence capture angle for the reward-lens response so PR reviewers can inspect both one-tool gimmick reads without opening the scene manually.
