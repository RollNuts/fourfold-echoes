# Veripsa Current Split Report

Generated UTC: `2026-06-25T19:33:56.908Z`

Core source: GitHub Veripsa checks; local veripsa CLI unavailable

## Core-Derived Rules

- PR #17 codex/store-readiness-pack: Veripsa `SUCCESS`. Docs-only changes with a narrow path set are a Veripsa-friendly landing unit.
- PR #14 codex/gate-a-evidence-harness: Veripsa `NEUTRAL`. New Unity editor/tool files were not in main's graph, so Core treated them as unknown. Split new C# runtime, editor scene generation, and capture/build tooling into separate PRs instead of landing them with docs.

Dirty files: 25

## Recommended Lanes

### PR-A - product-canon

- None

### PR-B - art-audio-direction

- `??` Assets/Audio.meta

### PR-C - production-release-plans

- None

### PR-D - validation-sync

- `M` artifacts/Reports/veripsa-current-split.json
- `M` artifacts/Reports/veripsa-current-split.md

### PR-E1 - d020-tool-runtime

- `M` Assets/Scripts/ExplorationTool.cs

### PR-E2 - d020-scene-evidence

- `M` Assets/Editor/FourfoldD020SliceSceneBuilder.cs
- `M` Assets/Scenes/D020VerticalSlice.unity

### PR-E3 - d020-capture-build

- `M` Assets/Editor/FourfoldUnityEvidenceCapture.cs
- `M` Scripts/Validation/write_market_reports.mjs
- `M` artifacts/Previews/d020-playable-attack-read.png
- `M` artifacts/Previews/d020-reward-read.png
- `M` artifacts/Previews/d020-slice-camera.png
- `M` artifacts/Previews/d020-tool-node-read.png
- `M` artifacts/Reports/audio-inventory.json
- `M` artifacts/Reports/audio-inventory.md
- `M` artifacts/Reports/final-status-report.json
- `M` artifacts/Reports/final-status-report.md
- `M` artifacts/Reports/performance-snapshot.json
- `M` artifacts/Reports/performance-snapshot.md
- `M` artifacts/Reports/unity-product-validation.json
- `M` artifacts/Reports/unity-product-validation.md
- `??` artifacts/Previews/d020-second-gimmick-before.png
- `??` artifacts/Previews/d020-second-gimmick-room-read.png
- `??` artifacts/Previews/d020-second-gimmick-solved.png

### PR-F - historical-proof-cleanup

- `M` Assets/Editor/FourfoldProductValidator.cs

### PR-G - forge-mediator-sync

- None

### PR-H - asset-pipeline-pilot-optional

- None

## Unknown / Needs Manual Lane

- `M` Assets/Editor/FourfoldD020PlayableSmoke.cs

## Use

- Keep PR-A through PR-D reviewable before any Unity scene/build lane lands.
- Treat new Unity C# and generated scene paths as Veripsa UNKNOWN until main indexes them.
- Do not ACK a Veripsa Pause without reading the overlapping files and recording why the D-020 lane is authoritative.
