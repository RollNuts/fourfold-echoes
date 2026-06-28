# D-020 Two-Gimmick Evidence Capture

Date: 2026-06-28

## Goal Support

This lane strengthens D-020 vertical-slice evidence by making the second one-tool response visible in runtime capture output. It supports commercial review by proving the same ExplorationTool can expose both the shortcut route and the reward-lens response without adding a second tool, inventory, crafting, or quest UI.

## Systems Touched

- D-020 Unity evidence capture
- D-020 market report generation
- D-020 preview manifest

## Files Added Or Changed

- `Assets/Editor/FourfoldUnityEvidenceCapture.cs`
- `Scripts/Validation/write_market_reports.mjs`
- `tools/unity_capture_d020_slice.sh`
- `artifacts/Previews/README.md`
- `artifacts/Previews/d020-slice-camera.png`
- `artifacts/Previews/d020-tool-node-read.png`
- `artifacts/Previews/d020-reward-lens-read.png`
- `artifacts/Previews/d020-reward-read.png`
- `artifacts/Previews/d020-playable-attack-read.png`
- `artifacts/Previews/d020-hud-reward-save.png`
- `artifacts/Reports/performance-snapshot.json`
- `artifacts/Reports/performance-snapshot.md`
- `artifacts/Reports/final-status-report.json`
- `artifacts/Reports/final-status-report.md`
- `artifacts/Reports/d020-two-gimmick-evidence.md`

## Implementation

- Added a dedicated `d020-reward-lens-read.png` capture candidate focused on the second one-tool response.
- Updated D-020 HUD reward/save capture setup so both one-tool nodes are solved before saving, making the captured progress read `S2 R1`.
- Updated market report generation to include the reward-lens capture in visual evidence counts.
- Updated final status blockers to reflect the current state: two one-tool responses exist, but a complete authored second gimmick room and 8-shot runtime set remain open.

## Tests And Validation

- Meshwright check
  - `$meshwright` skill/tool was requested by the user but is not available in this Codex session and has no install candidate exposed here.
  - Applied the repo-local Meshwright guidance fallback: visual QA on generated evidence images plus Unity CLI validation.
- Unity D-020 evidence capture
  - Method: `FourfoldEchoes.Editor.FourfoldUnityEvidenceCapture.CaptureD020Slice`
  - Result: passed, exit code 0.
- Unity D-020 playable smoke
  - Method: `FourfoldEchoes.Editor.FourfoldD020PlayableSmoke.Run`
  - Result: passed, exit code 0.
- PNG inspection
  - `d020-reward-lens-read.png`: valid 1280x800 PNG.
  - `d020-hud-reward-save.png`: valid 1280x800 PNG with `Progress S2 R1`.
- Visual QA
  - `d020-reward-lens-read.png` was inspected and shows the second one-tool response near the relic area.
  - `d020-hud-reward-save.png` was inspected and shows both one-tool nodes solved and the relic claimed.
- `node Scripts/Validation/write_market_reports.mjs`
  - Result: passed.
- `git diff --check -- <changed exact paths>`
  - Result: passed.
- `node Scripts/Validation/validate_repo.mjs`
  - Result: passed. Required reset files present: 52.
- Public repo hygiene scan
  - Result: passed. Scanned tracked/untracked files: 225.
- Secret/private path scan over changed exact paths
  - Result: passed. No matches.

## Public API Impact

- No runtime public API was changed.

## Acceptance Conditions

- Unity capture emits a valid 1280x800 PNG for the second one-tool response.
- Market reports count the new reward-lens capture.
- Existing D-020 evidence captures still generate.
- No raw Unity logs, local absolute paths, tokens, secrets, or private URLs are committed.

## Remaining Risk

- This is still technical gameplay evidence, not store-approved screenshot art.
- D-020 still needs a complete second authored gimmick room, full boss/miniboss proof, and at least 8 runtime screenshot candidates.

## Next Smallest Useful Task

Promote the reward-lens response into a compact authored second gimmick room with clear route flow, while keeping one ExplorationTool and no inventory.
