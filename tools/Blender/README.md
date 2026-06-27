# Blender Model Generation

This folder owns repeatable, repository-authored model generation for FOURFOLD
ECHOES. The current generator creates original folded-reliquary miniature
meshes for the D-020/vertical-slice/MVP asset inventory.

Every generated asset must use the same product-family DNA: folded plinth,
split inlay, signal thread, chunky tabs, and top-down readability. Region
variation is expressed through material ratios and wear-state, not by turning
each region into a different genre.

Run from the repository root:

```sh
blender --background --factory-startup --python tools/Blender/generate_fourfold_model_pack.py
node tools/AssetPipeline/validate_generated_assets.mjs
```

If Blender is launched from a sandboxed command context on macOS and crashes
during Metal startup, rerun it from the normal shell/editor automation context.
The fallback generator can still write OBJ evidence, but FBX from Blender is the
production import path:

```sh
python3 tools/AssetPipeline/generate_production_model_pack.py
```

Generated outputs:

- `Assets/Art/Production/P0/Models/*.fbx`
- `Assets/Art/Production/P1/Models/*.fbx`
- `Assets/Art/Production/P2/Models/*.fbx`
- `Assets/Art/Production/Source/FE_FourfoldModelPack.blend`
- `artifacts/Previews/ProductionModelPack/*.png`
- `artifacts/Reports/fourfold-model-pack.json`

The geometry is procedural and project-owned, but still requires human visual
review before it should be treated as final market-facing art.
