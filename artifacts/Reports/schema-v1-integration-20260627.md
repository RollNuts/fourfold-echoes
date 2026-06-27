# Schema v1 Integration - 2026-06-27

## Result

`artifacts/AssetPipeline/schema_v1/` is now synchronized with the generated model
manifest and can be used as the reference layer for sanitized production prompts.

## Coverage

- Schema records: 154
- Manifest assets: 145
- Prompt contract records: 145
- Matched manifest records: 145
- Manifest-only gaps: 0
- Schema-only records: 9

Schema-only records are planned UI/VFX/modular-kit briefs that do not yet have
model-manifest entries. They remain reference/planning records, not approved
runtime assets.

## Added Pipeline Tools

- `tools/AssetPipeline/sync_schema_v1_from_manifest.mjs`
  - Adds missing schema records from the current model manifest.
  - Rebuilds `assets.schema_v1.json` and `assets.schema_v1.jsonl`.
- `tools/AssetPipeline/harden_schema_v1_prompts.mjs`
  - Adds explicit no-franchise/no-average/no-trade-dress negative policy.
  - Replaces realistic-PBR drift wording with stylized-PBR language.
  - Adds external benchmark-as-metrics-only wording.
- `tools/AssetPipeline/validate_schema_v1.mjs`
  - Checks individual JSON, aggregate JSON, aggregate JSONL, manifest coverage,
    forbidden terms, imitation phrasing, PNG-only naming, and malformed budgets.
- `tools/AssetPipeline/write_commercial_asset_gate.mjs`
  - Writes commercial safety and marketplace/provenance status reports.

## Gate Status

Commercial asset gate:

- Repository-authored manifest pass: true
- Prompt contract safety pass: true
- Third-party candidates blocked: 44
- Market benchmark verdict: `below_market_finish`
- Market benchmark score: 0.63 / 0.8
- Gate status: `blocked_until_market_finish_license_and_human_review`

## Practical Meaning

The generated procedural assets are safe for internal prototype iteration after
validation. They are not market-facing approved. Third-party/Marketplace assets
must not be imported directly until license/provenance evidence and human art/IP
review exist.

