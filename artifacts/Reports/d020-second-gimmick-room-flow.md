# D-020 Second Gimmick Room Flow

Date: 2026-06-28

## Goal Support

This lane moves the D-020 reward-lens proof from a single response marker toward a compact authored second gimmick room. It supports the vertical-slice acceptance item that two room reads reuse the same ExplorationTool differently without adding a second tool, inventory, crafting, quest log, or open-world scope.

## Systems Touched

- D-020 generated scene content
- D-020 playable smoke validation
- D-020 evidence capture framing

## Files Added Or Changed

- `Assets/Editor/FourfoldD020SliceSceneBuilder.cs`
- `Assets/Editor/FourfoldD020PlayableSmoke.cs`
- `Assets/Editor/FourfoldUnityEvidenceCapture.cs`
- `Assets/Scenes/D020VerticalSlice.unity`
- `artifacts/Previews/d020-reward-lens-read.png`
- `artifacts/Reports/d020-second-gimmick-room-flow.md`

## Implementation

- Added `D020 Reward Lens Chamber`, a small readable room frame around the reward-lens node and relic route.
- Added a closed/idle read under `D020 Reward Lens Chamber Idle Read` that is visible before the tool solve.
- Added `D020 Reward Lens Chamber Path`, a solved route-flow response that appears when the same ExplorationTool solves the reward-lens node.
- Extended D-020 smoke to require the chamber, verify the path starts hidden, verify the closed read starts visible, and verify the same tool flips those states.
- Adjusted the reward-lens evidence capture pose to include the new chamber flow.

## Tests And Validation

- Unity D-020 scene generation and validation
  - Method: `FourfoldEchoes.Editor.FourfoldD020SliceSceneBuilder.BuildAndValidate`
  - Result: passed, exit code 0.
- Unity D-020 playable smoke
  - Method: `FourfoldEchoes.Editor.FourfoldD020PlayableSmoke.Run`
  - Result: passed, exit code 0.
- Unity D-020 evidence capture
  - Method: `FourfoldEchoes.Editor.FourfoldUnityEvidenceCapture.CaptureD020Slice`
  - Result: passed, exit code 0.
- Market report generation
  - Method: `node Scripts/Validation/write_market_reports.mjs`
  - Result: passed.
- PNG inspection
  - `d020-reward-lens-read.png`: valid 1280x800 PNG.
  - `d020-hud-reward-save.png`: valid 1280x800 PNG.
- Visual QA
  - `d020-reward-lens-read.png` was inspected and shows the reward-lens chamber rails plus the opened one-tool route path.
- `git diff --check -- <changed exact paths>`
  - Result: passed.
- `node Scripts/Validation/validate_repo.mjs`
  - Result: passed. Required reset files present: 52.
- Public repo hygiene scan
  - Result: passed. Scanned tracked/untracked files: 226.
- Secret/private path scan over changed exact paths
  - Result: passed. No matches.

## Public API Impact

- No runtime public API was changed.

## Acceptance Conditions

- D-020 generated scene contains a visible second gimmick chamber.
- The second chamber starts with a closed/idle read and hidden path response.
- The same ExplorationTool reveals the chamber path and hides the closed/idle read.
- Reward remains gated by enemy defeat plus both one-tool responses.
- Updated reward-lens evidence remains a valid runtime capture.

## Remaining Risk

- This is still a compact generated room proof, not final production art.
- D-020 still needs the second relic reward, boss/miniboss proof, additional runtime screenshot candidates, and final BGM/SFX approval.

## Next Smallest Useful Task

Add the second relic reward as a small flag-based reward with no inventory UI, then extend save/load smoke and HUD evidence to prove both rewards roundtrip.
