# Asset Pipeline

## Principle

Final assets must be commercially usable, registered, validated, and visually reviewed. A model file existing on disk is not enough.

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

Use the same uppercase area-coded naming as
`docs/Art/COMPACT_ACTION_ART_DIRECTION.md`.

| Category | Pattern | Example |
| --- | --- | --- |
| Hero | `FE_HERO_<NAME>` | `FE_HERO_Bearer` |
| Props | `FE_PROP_<AREA>_<NAME>_##` | `FE_PROP_R01_RootGate_01` |
| Terrain | `FE_ENV_<AREA>_<TYPE>_##` | `FE_ENV_R02_FloorCracked_03` |
| Enemy | `FE_ENEMY_<ROLE>_<NAME>` | `FE_ENEMY_MELEE_Shardling` |
| Boss | `FE_BOSS_##_<NAME>` | `FE_BOSS_01_RootWarden` |
| UI/world icon | `FE_UI_<PURPOSE>_<STATE>` | `FE_UI_Tool_Ready` |
| VFX mesh | `FE_VFX_<SOURCE>_<ACTION>` | `FE_VFX_Tool_Pulse` |
| Material | `FE_MAT_<AREA_OR_ROLE>_<SURFACE>` | `FE_MAT_R03_CrystalGlow` |
| Texture | `FE_TEX_<AREA_OR_ROLE>_<SURFACE>_<MAP>` | `FE_TEX_R01_Stone_ALB` |

Area codes are `HUB`, `R01`, `R02`, `R03`, `BOSS`, and `COMMON`.

## Validation

Run:

```sh
node Scripts/Validation/validate_repo.mjs
node tools/AssetPipeline/validate_generated_assets.mjs
```

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
