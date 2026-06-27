# Single Hero Polish - 2026-06-27

## Target

- Asset: `FE_CHAR_PLAYER_Hero_01`
- Existing prefab path: `Assets/Prefabs/Production/P0/FE_CHAR_PLAYER_Hero_01.prefab`
- Existing scene use: `ProductionCombatSlice` player object

## What Changed

- Rebuilt only this one model, not a new batch.
- Kept the same asset name, prefab path, collision profile, and scene wiring.
- Replaced the previous fallback primitive model with a smoother custom OBJ mesh.
- Added a dedicated single-asset rebuild script: `tools/AssetPipeline/polish_single_hero.py`
- Updated the shared generator `hero()` path so future rebuilds use the same polished construction.

## Current Model Files

- Model: `Assets/Art/Production/P0/Models/FE_CHAR_PLAYER_Hero_01.obj`
- Material: `Assets/Art/Production/P0/Models/FE_CHAR_PLAYER_Hero_01.mtl`
- Prefab: `Assets/Prefabs/Production/P0/FE_CHAR_PLAYER_Hero_01.prefab`
- Preview: `artifacts/Previews/ProductionModelPack/FE_CHAR_PLAYER_Hero_01.png`
- Manifest: `artifacts/Reports/fourfold-model-pack.json`

## Geometry Delta

- Previous manifest triangle count: 508
- Current triangle count: 6040
- Current primitive counts:
  - capsule: 6
  - ellipsoid: 23
  - prism: 18
- Current status: `single_asset_polished_candidate_requires_human_art_review`

## Verification

- `python3 tools/AssetPipeline/polish_single_hero.py`
- `node tools/AssetPipeline/build_prompt_contract.mjs`
- `node tools/AssetPipeline/write_unity_prefab_stubs.mjs`
- `node tools/AssetPipeline/validate_generated_assets.mjs --phase=postimport`
- `node Scripts/Validation/validate_repo.mjs`
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
- `git diff --check`
- Unity inbox: `production_art.import_model_pack`
- Unity inbox: `production_slice.build_and_validate`
- Unity inbox: `product.validate`

## Notes

Blender crashed in local background mode during this pass, so the model was rebuilt with repository-authored procedural OBJ geometry instead of the Blender generator. This is still a single-asset polish pass, not batch filler. The next model should not start until this hero is visually accepted or revised.
