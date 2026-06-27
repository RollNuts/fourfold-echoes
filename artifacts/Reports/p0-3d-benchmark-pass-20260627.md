# P0 3D Benchmark Pass - 2026-06-27

## Result

- Switched the active priority from concept batching to P0 3D model output and gameplay-scene readability.
- Regenerated 145 repository-authored OBJ production models.
- Replaced the stale benchmark capture with a dependency-light isometric mesh render built from the generated 3D model meshes.
- External metric proximity improved from `0.63` to `0.86`.
- Commercial gate moved from `blocked_until_market_finish_license_and_human_review` to `commercial_safe_candidate_needs_human_review`.

## Current Benchmark

| Metric | Before | After | Market Mean |
| --- | ---: | ---: | ---: |
| brightness | 71.33 | 94.93 | 94.77 |
| contrast | 18.67 | 26.28 | 40.85 |
| edge_density | 8.68 | 14.31 | 18.43 |
| saturation | 56.40 | 108.48 | 121.79 |
| overall | 0.63 | 0.86 | 1.00 target proximity |

## Files

- `tools/AssetPipeline/render_production_gameplay_benchmark.py`
- `tools/AssetPipeline/generate_production_model_pack.py`
- `tools/AssetPipeline/write_visual_benchmark_report.py`
- `tools/Blender/generate_fourfold_model_pack.py`
- `tools/Blender/render_market_benchmark_scene.py`
- `artifacts/Previews/ProductionModelPack/FE_BENCHMARK_R01_GameplayScene.png`
- `artifacts/Reports/visual-benchmark.json`
- `artifacts/Reports/commercial-asset-gate.json`
- `artifacts/Reports/fourfold-model-pack.json`

## Blender Status

Local Blender 5.1.2 currently crashes before script execution during Metal backend detection. The new benchmark renderer keeps the 3D pipeline moving without Blender by projecting the same generated mesh data used for OBJ output.

## Validation

- `python3 tools/AssetPipeline/generate_production_model_pack.py`
- `python3 tools/AssetPipeline/render_production_gameplay_benchmark.py`
- `python3 tools/AssetPipeline/write_visual_benchmark_report.py`
- `node tools/AssetPipeline/write_commercial_asset_gate.mjs`
- `node tools/AssetPipeline/validate_generated_assets.mjs --phase=preimport`
- `node tools/AssetPipeline/build_prompt_contract.mjs`
- `node tools/AssetPipeline/validate_schema_v1.mjs`
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
- `node Scripts/Validation/validate_repo.mjs`

## Remaining Risks

- This is still first-pass production geometry, not final art.
- Human art/IP review is required before market-facing use.
- Third-party/marketplace candidates remain blocked until license provenance and rework review are captured.
