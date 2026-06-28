# Builder Prototype Asset Library Sync Plan

Status: draft for PR review.

## Goal

Keep reusable, bulky, source, reference, and prototype asset groups in
`RollNuts/unity-game-asset-library`, while this game repository keeps only
runtime-ready assets that directly support the Builder Prototype slice.

This plan supports an original Steam indie direction: block building,
Diablo-like buildcraft, FF14-like positional telegraph combat, Tarkov-like local
run tension, satisfying loot reveals, and streamer-friendly moments. It does not
authorize copied Dragon Quest, Diablo, FF14, or Tarkov assets, names, monsters,
UI, audio, or visual identity.

## Repository Boundary

| Location | Owns | Must Not Own |
| --- | --- | --- |
| `unity-game-asset-library` | Source files, bulky references, reusable prototype kits, license notes, previews, import notes, rejected variants | Shipping Unity scenes, gameplay code, slice-only runtime commits |
| `fourfold-echoes` | Curated runtime assets, small docs/previews, Unity `.meta` files for imported runtime assets, active slice evidence | Asset warehouse folders, source packs, large unused variants, ambiguous license material |

Runtime assets move into `fourfold-echoes` only after they pass acceptance checks
and have an import note linking back to the asset-library source batch.

## Asset-Library Taxonomy

```text
FourfoldEchoes/
  BuilderPrototype/
    README.md
    00_Manifests/
      asset_index.csv
      batch_status.md
      runtime_export_log.md
    01_BlockPalette/BP001_FoldedRelicBlocks/
    02_TelegraphDecals/TD001_ReadableCombatShapes/
    03_RewardPresentation/RP001_BeamsAndCards/
    04_RunStakes/RS001_ExtractAndRiskSignals/
    05_CharactersEnemies/CE001_TopDownPrototypeCast/
    06_SFX/SX001_ReadabilityCore/
    90_References/
    99_Deprecated/
```

Each batch uses:

```text
Source/
UnityRuntime/
Previews/
License/
ImportNotes/
```

`00_Manifests/asset_index.csv` should include:

```text
asset_id,batch_id,category,name,status,source_path,runtime_path,license_status,preview_path,unity_target_path,notes
```

## Naming Conventions

| Category | Pattern | Example |
| --- | --- | --- |
| Block mesh | `FE_ENV_COMMON_BLOCK_<NAME>_PROTO_##` | `FE_ENV_COMMON_BLOCK_FoldedPlinth_PROTO_01` |
| Telegraph decal | `FE_TEX_COMMON_Telegraph_<SHAPE>_<STATE>_##` | `FE_TEX_COMMON_Telegraph_Circle_Warn_01` |
| Reward visual | `FE_VFX_COMMON_Reward_<NAME>_##` | `FE_VFX_COMMON_Reward_BeamGold_01` |
| Run-stakes signal | `FE_UI_RUN_<NAME>_<STATE>_##` | `FE_UI_RUN_ExtractMarker_Ready_01` |
| Character prototype | `FE_CHAR_<ROLE>_<NAME>_PROTO_##` | `FE_CHAR_PLAYER_HeroBlockout_PROTO_01` |
| Enemy prototype | `FE_ENEMY_<ROLE>_<NAME>_PROTO_##` | `FE_ENEMY_MELEE_ShieldRead_PROTO_01` |
| SFX | `FE_SFX_<CATEGORY>_<ACTION>_##` | `FE_SFX_Loot_Reveal_01` |

## License And Source Notes

Every batch must include `License/SOURCE_NOTES.md` before runtime import.

Required fields:

```text
Batch:
Owner:
Created date:
Source type: original / purchased / generated / mixed / recording
Commercial use: yes / no / restricted / unknown
Redistribution allowed: yes / no / restricted / unknown
Attribution required:
Third-party sources:
Generator or tool used:
Prompt or transformation notes:
Human edits:
Do not ship before:
Unity import notes:
Runtime destination:
```

Unknown or restricted commercial-use status blocks runtime import.

## Preview Requirements

| Asset Type | Required Preview |
| --- | --- |
| Block kit | Top-down contact sheet, scale grid, pivot/collision note |
| Telegraph decals | Dark-floor and light-floor test, readable-at-1280 screenshot |
| Reward beams/cards | Reveal/pickup sequence or contact sheet |
| Run-stakes signals | Extract marker states, danger-tier states, carried-loot warnings |
| Characters/enemies | Top-down silhouette, facing, attack-origin screenshot |
| SFX | WAV list, loudness notes, cue sync note |

## First Asset Batches

### BP001 Folded Relic Block Palette

Floor tiles, low walls, route blockers, pedestals, gates, and shared materials.
Acceptance focus: scale consistency, pivots, collision notes, readable
floor/wall/exit separation, and no gray-box market read.

### TD001 Readable Combat Telegraph Decals

Circle, cone, line, arc, ring, warn, active, fade, boss-danger, and
accessibility variants. Acceptance focus: readable danger shapes that never hide
the player or reward effects.

### RP001 Reward Beams And Cards

Loot/reward beams, rarity frames, pickup sparkles, and affix-card placeholders.
Acceptance focus: fast reaction value for streams without implying copied UI.

### RS001 Extract And Risk Signals

Extract markers, danger-tier decals, carried-loot warnings, and pressure pips.
Acceptance focus: Tarkov-like tension as a local push-or-extract decision, not a
multiplayer extraction-shooter clone.

### CE001 Top-Down Prototype Cast

Hero, tool proxy, melee enemy, ranged enemy, elite, and boss-threat blockouts.
Acceptance focus: top-down silhouette, attack origin, and telegraph readability.

### SX001 Readability Core SFX

Block hit/break/place, telegraph warn/impact, loot reveal, extract ready,
extract success/failure, danger escalation, UI confirm/cancel. Acceptance focus:
short, readable, streamer-friendly cues with license/source notes.

## Import Workflow

1. Create or update a batch folder in `unity-game-asset-library`.
2. Add source files, license notes, previews, and import notes.
3. Stage only clean Unity-ready files under `UnityRuntime/`.
4. Validate naming, license status, preview coverage, and import notes.
5. Open an asset-library PR for the batch.
6. Copy only selected runtime files into `fourfold-echoes`.
7. Commit Unity-generated `.meta` files with runtime imports.
8. In the game PR, list source batch ID, asset IDs, destinations, and why each
   asset is needed.

## PR Draft

Title: `Builder Prototype: add asset-library sync plan`

Body:

```md
## Summary
- Adds the Builder Prototype asset-library sync plan.
- Defines source/runtime boundaries, taxonomy, naming, license notes, previews,
  and first asset batches.
- Covers block, combat telegraph, loot, run-stakes, character/enemy, and SFX
  asset families.

## Validation
- [ ] Confirmed this PR is docs-only.
- [ ] Confirmed PR comments are addressed before merge.
```
