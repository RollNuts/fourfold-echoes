# FOURFOLD ECHOES Agent Rules

This repository is the shared source for FOURFOLD ECHOES development. Keep the
public surface clean: do not commit secrets, private URLs, personal local paths,
unknown-license assets, or tool credentials.

## Product Direction

FOURFOLD ECHOES is a Steam-first, buy-to-own, single-player top-down
action-adventure. Current production work should stay focused on a compact MVP:

- one hub
- three handcrafted regions
- four bosses
- one visually clear exploration tool
- local save
- controller-first UI/UX
- no networking, live service, gacha, MMO, open world, base building, or
  co-op dependency for v1

## Git And PR Discipline

- Work in branches and PRs; do not push directly to `main`.
- Keep each PR to one purpose and one reviewable behavior slice.
- Do not mix art imports, gameplay systems, CI, and docs in one PR.
- Preserve unrelated worktree changes. Do not reset, clean, stash, or revert
  another agent's work unless explicitly instructed.
- Required evidence belongs in PR descriptions: checks run, Unity validation,
  screenshots/builds when relevant, and known gaps.

## Unity Validation

- Unity target: 6000.3 LTS.
- Prefer command-line or editor-command validation over manual claims.
- Do not call a feature playable unless Unity scene load, input flow, console
  state, and relevant screenshots/build evidence were actually checked.
- If Unity is already busy in another worktree, report that and run the smallest
  safe static checks instead of starting competing editor jobs.

## Art And Mesh Pipeline

For art, mesh, Blender, skeleton, animation planning, Unity prefab, or visual QA
work, first try to load the Codex skill named `$meshwright`.

Use it for:

- image-first low-poly asset planning
- skeleton and motion planning before modeling
- Blender batch generation/postprocess
- Unity prefab/material import checks
- review bundles, delivery manifests, and pro-audit reports

If `$meshwright` is not available in the current Codex session, state that
clearly and continue with repository-local scripts and documented checks. Do not
write the local skill installation path or any personal filesystem path into
repo files, PR bodies, screenshots, reports, or generated manifests.

## Required Output Shape

For substantial tasks, report:

1. how the task supports the final game
2. systems touched
3. files added or changed
4. implementation summary
5. checks run
6. acceptance evidence
7. next smallest useful task
