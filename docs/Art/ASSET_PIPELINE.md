# Asset Pipeline

## Principle

Final assets must be commercially usable, registered, validated, and visually reviewed. A model file existing on disk is not enough.

All production model work must obey the current art-direction contract:

- art axis: **Folded Reliquary Miniatures** / **折り目遺物の箱庭模型**
- shared DNA: folded plinth, split inlay, signal thread, chunky tabs, top-down readability
- region variation: material ratio and wear-state variation only; do not turn each region into a separate product family
- external comparison: aggregate quality metrics only, never prompt/reference/model/style instructions

## Free / Low-Cost Tool Stack

- Blender: modeling, rig blockout, export, preview renders.
- Krita: paintover, concept sheets, texture edits.
- Material Maker: procedural materials where appropriate.
- Unity Built-in renderer for current repo; do not migrate render pipeline without a tested branch.

## Intake Path

1. Register asset in `docs/Art/ASSET_REGISTER.csv`.
2. Confirm license in `docs/Legal/LICENSES.md`.
3. Place source under a future `Assets/Manual/Source/` or keep confidential source outside repo if license requires.
4. Export Unity-ready asset with stable naming.
5. Import through Unity, preserving `.meta`.
6. Create prefab and simplified collider.
7. Validate material, scale, pivot, bounds, LOD, and missing references.
8. Capture preview into `artifacts/Previews/`.

## Naming

- Hero: `FE_Hero_*`
- Enemy: `FE_Enemy_*`
- Boss: `FE_Boss_*`
- Environment: `FE_Env_*`
- VFX mesh: `FE_VFX_*`
- UI/world icon: `FE_UI_*`

## Validation

Run:

```sh
node Scripts/Validation/validate_repo.mjs
node tools/AssetPipeline/validate_generated_assets.mjs
```

## Production Model Pack Generator

The current repeatable production-art first pass is:

```sh
blender --background --factory-startup --python tools/Blender/generate_fourfold_model_pack.py
node tools/AssetPipeline/validate_generated_assets.mjs
```

It generates:

- 103 original model FBX files under `Assets/Art/Production/P0/Models`,
  `Assets/Art/Production/P1/Models`, and `Assets/Art/Production/P2/Models`
- Blender source at `Assets/Art/Production/Source/FE_FourfoldModelPack.blend`
- Unity prefabs under `Assets/Prefabs/Production/P0`, `P1`, and `P2`
- preview PNGs under `artifacts/Previews/ProductionModelPack/`
- manifest at `artifacts/Reports/fourfold-model-pack.json`
- market comparison report at `artifacts/Reports/visual-benchmark.md`

Acceptance status is `generated_first_pass`. The pack is commercially
repo-authored and manifest-gated by the art-direction contract, but the latest
external screenshot benchmark still marks it as below market finish. Treat it
as a complete model inventory and first visual iteration, not final
Steam-facing art.

## Blender Pilot Generator

The first reproducible art factory pass is:

```sh
blender --background --python tools/Blender/generate_pilot_assets.py
node tools/AssetPipeline/validate_generated_assets.mjs
```

It currently generates a pilot hero, first guardian boss, and ruin kit:

- `Assets/Art/Generated/BlenderPilot/Models/FE_Hero_Pilot.fbx`
- `Assets/Art/Generated/BlenderPilot/Models/FE_Boss_Guardian_Pilot.fbx`
- `Assets/Art/Generated/BlenderPilot/Models/FE_Env_RuinKit_Pilot.fbx`
- `Assets/Art/Generated/BlenderPilot/Source/FE_BlenderPilotAssets.blend`
- preview PNGs under `artifacts/Previews/BlenderPilot/`
- manifest at `artifacts/Reports/blender-pilot-assets.json`

Acceptance status is `pilot_visual_only`. These files prove that Blender CLI
can produce visible, registered, previewed, commercially owned source assets.
They are not approved final hero, boss, or terrain art.

Unity-specific validation should later add:

- missing material scanner
- missing reference scanner
- mesh bounds validator
- LODGroup scanner
- texture size scanner
- collider complexity scanner
