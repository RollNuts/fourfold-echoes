# Veripsa Current Split Report

Generated UTC: `2026-06-26T05:40:41.299Z`

Core source: GitHub Veripsa checks; local veripsa CLI unavailable

## Core-Derived Rules

- PR #17 codex/store-readiness-pack: Veripsa `SUCCESS`. Docs-only changes with a narrow path set are a Veripsa-friendly landing unit.
- PR #14 codex/gate-a-evidence-harness: Veripsa `NEUTRAL`. New Unity editor/tool files were not in main's graph, so Core treated them as unknown. Split new C# runtime, editor scene generation, and capture/build tooling into separate PRs instead of landing them with docs.

Dirty files: 5

## Recommended Lanes

### PR-A - product-canon

- None

### PR-B - art-audio-direction

- None

### PR-C - production-release-plans

- None

### PR-D - validation-sync

- `M` Scripts/Validation/write_veripsa_split_report.mjs
- `M` artifacts/Reports/veripsa-current-split.json
- `M` artifacts/Reports/veripsa-current-split.md

### PR-E1 - d020-tool-runtime

- `??` Assets/Scripts/D020ProgressSave.cs
- `??` Assets/Scripts/D020ProgressSave.cs.meta

### PR-E2 - d020-scene-evidence

- None

### PR-E3 - d020-capture-build

- None

### PR-F - historical-proof-cleanup

- None

### PR-G - forge-mediator-sync

- None

### PR-H - asset-pipeline-pilot-optional

- None

## Unknown / Needs Manual Lane

- None

## Use

- Keep PR-A through PR-D reviewable before any Unity scene/build lane lands.
- Treat new Unity C# and generated scene paths as Veripsa UNKNOWN until main indexes them.
- Do not ACK a Veripsa Pause without reading the overlapping files and recording why the D-020 lane is authoritative.
