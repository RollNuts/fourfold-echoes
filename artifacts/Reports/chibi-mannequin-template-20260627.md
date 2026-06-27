# Chibi Mannequin Template - 2026-06-27

## Decision

The previous hero model pass is not accepted as a concept match. The next correct step is a shared body mannequin before costume, hair, backpack, or tool modeling.

This mannequin is scoped to playable characters and friendly NPCs only. Enemies, monsters, minibosses, and bosses must use separate skeleton families.

## Asset

- Asset: `FE_CHAR_TEMPLATE_ChibiMannequin_01`
- Model: `Assets/Art/Production/P0/Models/FE_CHAR_TEMPLATE_ChibiMannequin_01.obj`
- Material: `Assets/Art/Production/P0/Models/FE_CHAR_TEMPLATE_ChibiMannequin_01.mtl`
- Prefab: `Assets/Prefabs/Production/P0/FE_CHAR_TEMPLATE_ChibiMannequin_01.prefab`
- Preview: `artifacts/Previews/ProductionModelPack/FE_CHAR_TEMPLATE_ChibiMannequin_01.png`
- Turnaround: `artifacts/Previews/ProductionModelPack/FE_CHAR_TEMPLATE_ChibiMannequin_01_Turnaround.png`
- Builder: `tools/AssetPipeline/build_chibi_mannequin.py`

## Template Rules

- 3-head pop-deformed proportion.
- Large blank head, short torso, chunky hands and feet.
- Visible shoulder, elbow, wrist, hip, knee, and ankle landmarks.
- No hair, no costume, no weapon, no backpack, no enemy anatomy.
- Front orientation marks are only for modeling alignment.

## Geometry

- Triangles: `7860`
- Primitive counts:
  - capsule: `9`
  - ellipsoid: `40`
  - prism: `5`
- Material count: `4`

## Verification

- `python3 -m py_compile tools/AssetPipeline/generate_production_model_pack.py tools/AssetPipeline/build_chibi_mannequin.py`
- `python3 tools/AssetPipeline/build_chibi_mannequin.py`
- `node tools/AssetPipeline/build_prompt_contract.mjs`
- `node tools/AssetPipeline/write_unity_prefab_stubs.mjs`
- `node tools/AssetPipeline/validate_generated_assets.mjs --phase=postimport`
- Unity inbox: `production_art.import_model_pack` -> `ok: true`
- Unity inbox: `product.validate` -> `ok: true`
- `node Scripts/Validation/validate_repo.mjs`
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
- `git diff --check`

## Next Step

Use this mannequin as the body reference for the top-left blue/orange hero concept. Do not start from the previous rejected hero mesh.
