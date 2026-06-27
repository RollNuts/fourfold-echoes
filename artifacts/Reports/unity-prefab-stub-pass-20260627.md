# Unity Prefab Integration Pass

- Scope: 145 generated production model assets from `artifacts/Reports/fourfold-model-pack.json`
- Generator: `tools/AssetPipeline/write_unity_prefab_stubs.mjs`
- Prefab output: `Assets/Prefabs/Production/P0`, `P1`, `P2`
- Model output: `Assets/Art/Production/P0`, `P1`, `P2`
- Result: deterministic prefab/meta generation passed, then the already-open Unity Editor imported the full 145-prefab model pack through `FourfoldForgeCommandInbox`

## What Changed

- Added missing Unity `.obj.meta`, `.mtl.meta`, `.prefab`, and `.prefab.meta` files for the generated model pack.
- Preserved existing GUIDs and rewrote importer settings only with the same GUIDs.
- Generated root wrapper prefabs with the model instance renamed to `Visual`.
- Applied collision profiles from the manifest:
  - character/enemy actors use capsule colliders
  - bosses use larger capsule colliders
  - pickups use trigger box colliders
  - floor/bridge/tile assets use thin floor box colliders
  - equipment/detail/decal assets have no scene collider
- Applied static flags from the manifest, with NavigationStatic only on true boundary assets.

## Verification

- `node tools/AssetPipeline/write_unity_prefab_stubs.mjs`
- `node tools/AssetPipeline/validate_generated_assets.mjs --phase=preimport`
- `node tools/AssetPipeline/validate_generated_assets.mjs --phase=postimport`
- Unity inbox command: `import.production_art_model_pack.prefab_stubs.202606270419`
- Unity event result: `ok=true`, `code=OK`
- Unity Editor log: `FOURFOLD generated model pack imported: 145 prefabs created from artifacts/Reports/fourfold-model-pack.json`
- Post-Unity static validation: `node tools/AssetPipeline/validate_generated_assets.mjs --phase=postimport`

The postimport-style check now validates prefab GUID references, root names, `Visual` rename, collider type, collider dimensions, trigger settings, static/nav flags, model importer settings, and required `.meta` GUID uniqueness.

## Notes

Direct batchmode was blocked because the project was already open in a normal Unity Editor instance. The active Editor was used through the existing inbox mediator instead. A first retry failed because Unity processed the command before the updated Editor assembly recompiled; after Unity recompiled `Assembly-CSharp-Editor.dll`, the same import action completed successfully.
