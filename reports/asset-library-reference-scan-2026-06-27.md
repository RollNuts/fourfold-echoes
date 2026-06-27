# Asset Library Reference Scan - 2026-06-27

## Purpose

This report records a read-only asset scan for the commercial MVP goal. The
current objective allows Unity assets from the `unity-game-asset-library`
reference repository when they help the compact Steam-first action-adventure.
This scan does not import, bind, delete, or approve any asset. It only records
what is visible in the current checkout and how it should be treated before a
gameplay PR uses it.

## Library Access

The `private-assets` remote is configured and has a local `private-assets/main`
ref available for read-only comparison. No network fetch was performed. No
separate adjacent checkout was found, so this scan uses only the local ref and
the current game worktree.

The asset-library ref contains reusable runtime-style folders such as art,
animations, audio, project settings, commands, contracts, docs, reports, and
tools. That matches the repository rule that reusable/bulky source and
reference material should live in the asset library while this game repository
keeps only active runtime assets and slice work.

## Current Candidate Packs

| Candidate | Current size | Visible evidence | Classification | Reason |
| --- | ---: | --- | --- | --- |
| `Assets/Art/ModelRigAnimation` | 9.0M | Enemy/MeleeShardling preview tree and meta files are present. | Preview only | It is an untracked local candidate. Do not gameplay-bind skeleton or mannequin-like material until AnimatorController, curves, root motion policy, event binding, import validation, and license/provenance notes are present. |
| `Assets/Art/UI/Icons/Generated/ART_FoldedReliquary_ToolGlyphs_v0.1.0` | 1.0M | `asset.json` says repository-authored proprietary, production-intent draft, transparent no-text exploration-tool state glyphs. | Gameplay bind ready after validation | It is aligned with the one-tool HUD/readability goal, but should still land in a focused UI/art asset lane with Unity import validation and screenshot readability evidence before store capture. |
| `Assets/Art/VFX/Generated/VFX_Telegraph_DangerCircle_Enemy_v1.0.0` | 256K | `asset.json` says Built-in Render Pipeline, enemy danger circle, partially verified, corrected prefabs pending root-project Unity import pass. | Preview only | The visual purpose is aligned with enemy tell readability, but the manifest itself says root-project Unity import is still pending. Do not gameplay-bind until the prefab imports cleanly and is validated in the production scene. |
| `Assets/Audio/SFX/SFXPack_v0_1_0` | 3.2M | Categorized WAVs for ambient, combat, footsteps, interact, magic, pickup, and UI are present. | Preview only | Useful as cue candidates, but gameplay binding needs cue-ID mapping, license/provenance registration, loudness/mix review, and event routing. Do not claim non-placeholder audio readiness from file presence alone. |

## Adoption Rules

Asset adoption should stay lane-separated. Gameplay PRs may reference already
approved runtime assets, but they should not mix large P0/P1/P2/P3 asset
batches, generated preview packs, model rigs, SFX packs, and scene gameplay
logic in one change.

For every candidate selected from the asset library or current local candidate
packs, record:

1. Source/provenance and license status.
2. Runtime path and, when useful, proposed asset-library path.
3. Ready / Not ready / Preview only / Gameplay bind ready classification.
4. Unity import validation result.
5. Gameplay-camera or UI screenshot evidence when the asset affects player
   readability.
6. Whether it is store-capture approved or internal-only.

## Recommendation

The lowest-risk candidate to bind next is the exploration-tool glyph family,
because it supports the existing single-tool HUD without expanding scope. The
enemy danger-circle VFX and SFX pack should remain preview-only until Unity
import and cue-routing evidence exists. Model rig/animation material should
remain preview-only until animation controllers and gameplay event binding are
explicitly validated.
