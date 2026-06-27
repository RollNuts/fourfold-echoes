# P0 Model Quality And Unity Risk Pass - 2026-06-27

## Result

- P0 model-density audit now passes: `54 / 54`.
- Generated model manifest remains at `145` repository-authored assets.
- External visual benchmark remains passing: `0.86` overall, `near_market_metric_range`.
- Commercial gate remains `commercial_safe_candidate_needs_human_review`.
- Unity importer policy was corrected, but local Unity batch import could not complete because Unity licensing initialization failed.

## Model Quality Changes

- Raised legacy P0 hero, enemy, boss, pedestal, chest, and block-field model density.
- Added role finish geometry for playable cast, NPCs, enemies, route blocks, and boss read points.
- Reclassified shortcut/bridge route assets as walkable `Tile` assets instead of nav-blocking `Boundary` assets.
- Added `no_collider_equipment` so held equipment no longer becomes solid set dressing.
- Added `p0-model-quality-audit` gate to stop low-density P0 assets from silently passing.

## Unity Import Changes

- `FourfoldGeneratedModelPackImporter` now sizes colliders from manifest `bounds_m` / `footprint_m`.
- Static flags are now applied from `static_hint`.
- Navigation static is only applied when `nav_blocking` is true.
- Validation now rejects nav-blocking bridges and blocking equipment colliders.
- `Scripts/Validation/run_all.sh` now runs model-pack validation and calls the generated model importer before postimport validation.

## Current Blocker

Unity batch import was attempted:

```text
/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity
  -batchmode
  -quit
  -projectPath <repo-root>
  -executeMethod FourfoldEchoes.Editor.FourfoldGeneratedModelPackImporter.ImportGeneratedModelPack
```

It did not reach project import because Unity licensing failed:

```text
Licensing initialization failed after 74.85s
```

Therefore `postimport` is intentionally not claimed as passing yet. The existing missing prefab/meta issue remains a Unity execution task, not a generator pass.

## Verified

- `python3 tools/AssetPipeline/generate_production_model_pack.py`
- `node tools/AssetPipeline/write_p0_model_quality_audit.mjs`
- `node tools/AssetPipeline/validate_generated_assets.mjs --phase=preimport`
- `python3 tools/AssetPipeline/render_production_gameplay_benchmark.py`
- `python3 tools/AssetPipeline/write_visual_benchmark_report.py`
- `node tools/AssetPipeline/build_prompt_contract.mjs`
- `node tools/AssetPipeline/validate_schema_v1.mjs`
- `node tools/AssetPipeline/write_commercial_asset_gate.mjs`
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
- `node Scripts/Validation/validate_repo.mjs`

## Not Yet Verified

- Unity-generated prefab count equals manifest count.
- Unity `.obj.meta` / `.mtl.meta` generated for all current assets.
- Static flags and generated colliders in prefab YAML after reimport.
- Full `Scripts/Validation/run_all.sh`, because it now correctly depends on Unity import.
