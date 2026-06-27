# Asset Pipeline Validation

Generated model validation checks the model pack manifest, exported FBX files,
preview files, source file, Unity `.meta` files, prefab creation, triangle
budgets, material budgets, commercial-safety wording, and the folded-reliquary
art-direction contract.

Run from the repository root:

```sh
blender --background --factory-startup --python tools/Blender/generate_fourfold_model_pack.py
# Then run Unity import:
# FOURFOLD/Assets/Import Generated Model Pack
node tools/AssetPipeline/sync_schema_v1_from_manifest.mjs
node tools/AssetPipeline/harden_schema_v1_prompts.mjs
node tools/AssetPipeline/validate_schema_v1.mjs
node tools/AssetPipeline/build_prompt_contract.mjs
node tools/AssetPipeline/write_commercial_asset_gate.mjs
node tools/AssetPipeline/validate_generated_assets.mjs
```

Use `--phase=preimport` immediately after Blender generation when Unity has not
created `.meta` files or prefabs yet.

`generate_production_model_pack.py` is a dependency-light fallback for OBJ
evidence only. Do not use it as the preferred Unity import path while Blender
FBX generation is available.

If the validator reports missing Unity `.meta` or prefabs, run the Unity import
step first.

## Prompt Contract

`artifacts/AssetPipeline/schema_v1/` is a reference prompt bundle only. Do not
send those raw prompts directly to a generator or vendor. First sync and harden
the schema from the current model manifest, then build the sanitized contract:

```sh
node tools/AssetPipeline/sync_schema_v1_from_manifest.mjs
node tools/AssetPipeline/harden_schema_v1_prompts.mjs
node tools/AssetPipeline/validate_schema_v1.mjs
node tools/AssetPipeline/build_prompt_contract.mjs
```

This writes `artifacts/Reports/fourfold-asset-prompt-contract.jsonl`, a
sanitized prompt contract aligned to the current folded-reliquary
art direction, manifest names, collision profiles, budgets, and commercial/IP
safety gates.

Current expected coverage after the P0 model sprint:

- schema records: 154
- manifest assets: 145
- prompt contract records: 145
- manifest-only schema gaps: 0

## Commercial Gate

Run:

```sh
node tools/AssetPipeline/write_commercial_asset_gate.mjs
```

This writes `artifacts/Reports/commercial-asset-gate.json` and `.md`.
Repository-authored procedural assets may proceed to internal prototype use
after validation. Marketplace/third-party candidates remain blocked until
license/provenance evidence and human art/IP review are captured. Market-facing
approval also remains blocked while the visual benchmark verdict is below the
production threshold.
