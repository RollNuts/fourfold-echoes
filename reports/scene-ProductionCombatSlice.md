# scene-ProductionCombatSlice

作成日: 2026-06-27

## 1. Final Goal Support

`ProductionCombatSlice` を、既存 Production Prefab を使った top-down combat / exploration-tool proof scene として扱いやすくするため、Hierarchy / Inspector で追跡しやすいランドマークを読みやすく調整した。

対象シーン名は依頼文では `{{SCENE_NAME}}` のままだったため、既存シーン構造、Prefab 使用量、現在の production evidence に最も合う `Assets/Scenes/ProductionCombatSlice.unity` を対象と仮定した。

## 2. Systems Touched

- Scene: `Assets/Scenes/ProductionCombatSlice.unity`
- Prefab usage: existing `Assets/Prefabs/Production/P1/FE_PROP_R01_RevealMarker_01.prefab`
- Camera: existing `PCS Top Down Camera` / `MainCamera` tag preserved
- Audio listener: added to `PCS Top Down Camera`
- Lighting: existing dynamic lights preserved
- Input / EventSystem: unchanged
- C# scripts: no scene-implementation script change was required

## 3. Files Added / Changed

- Changed: `Assets/Scenes/ProductionCombatSlice.unity`
- Added: `reports/scene-ProductionCombatSlice.md`

Existing unrelated work was preserved:

- `AGENTS.md` was already modified.
- `reports/review-findings.md` was already untracked.

## 4. Implementation

Existing Prefab instance under `PCS Interactables`:

- `PCS Inspector Landmark - Tool Shortcut Reward`
- Source Prefab: `Assets/Prefabs/Production/P1/FE_PROP_R01_RevealMarker_01.prefab`
- Position: `x=-0.85, y=0.16, z=-2.35`
- Rotation: `y=45`
- Scale: `0.78`
- Collider override: disabled, so it does not alter player movement, combat, or tool interaction.

The landmark was kept on the existing reward/shortcut read line, with scale increased from `0.62` to `0.78` and local height raised from `0.12` to `0.16` so it reads more clearly from the top-down camera.

Existing `AudioListener` component on `PCS Top Down Camera` was preserved.

The scene now has:

- 159 prefab instances
- 87 distinct prefab assets
- 1 explicit reveal-marker landmark for inspection and scene-read tracking
- 1 scene audio listener on the existing top-down camera

## 5. Prefab / ScriptableObject / Lightmap Settings

Required Prefab:

- `Assets/Prefabs/Production/P1/FE_PROP_R01_RevealMarker_01.prefab`

No new ScriptableObject was required.

No new lightmap or LightingSettings asset was required. The scene still uses existing dynamic lighting and has `m_LightingSettings: {fileID: 0}`.

Cinemachine was not used because `Packages/manifest.json` does not include Cinemachine.

Addressables settings were not added because Addressables is not present in `Packages/manifest.json`.

## 6. Tests

Static checks completed:

- `git diff --check -- Assets/Scenes/ProductionCombatSlice.unity reports/scene-ProductionCombatSlice.md`: passed.
- `node Scripts/Validation/validate_repo.mjs`: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed.
- Confirmed `Packages/manifest.json` has no Cinemachine or Addressables dependency. Unity Test Framework is present in the current worktree, from separate changes.
- Confirmed the scene still has `PCS Top Down Camera` tagged `MainCamera`.
- Confirmed `PCS Top Down Camera` has an `AudioListener`.
- Confirmed no static missing-script pattern: `m_Script: {fileID: 0`
- Confirmed no static missing-prefab pattern: `m_SourcePrefab: {fileID: 0`
- Confirmed the added Prefab instance resolves to `FE_PROP_R01_RevealMarker_01.prefab`.
- Confirmed prefab instance count increased to 159 and distinct Prefab count to 87.

Unity execution checks:

- Unity batchmode scene-builder verification was run against a temporary project copy to avoid launching against the open working checkout.
- `FourfoldEchoes.Editor.FourfoldProductionCombatSliceSceneBuilder.ValidateGeneratedScene`: exit code 0.
- Verifier pass line: `prefabInstances=159`, `distinctPrefabs=87`, `renderers=156`.
- A dedicated 30-second Play Mode smoke was run against the same temporary project copy. Result line: `nullReferences=0`, `errors=0`, `ok=True`.
- The smoke verifier emitted Unity Editor service noise after Play Mode entry, including QuickSearch indexing and UI Toolkit theme warnings. No `NullReferenceException` was counted during the 30-second smoke window.

## 7. Acceptance Conditions

Current evidence:

- Target scene changed: yes, `Assets/Scenes/ProductionCombatSlice.unity`.
- Required Prefab documented: yes.
- ScriptableObject / lightmap settings documented: yes, none required.
- Report added: yes, `reports/scene-ProductionCombatSlice.md`.
- EventSystem / Input settings preserved: yes, no related files or scene objects were changed.
- Cinemachine / Addressables constraints preserved: yes, neither was added.
- Audio listener warning addressed: yes, `PCS Top Down Camera` now has one `AudioListener`.
- Player-visible improvement: yes, the existing reveal-marker landmark now reads larger and slightly higher from the top-down camera, while keeping collision disabled.
- Major objects remain traceable:
  - `Production Combat Slice World`
  - `PCS Block Field Arena`
  - `PCS Combatants`
  - `PCS Interactables`
  - `PCS Exploration Tool Runtime`
  - `PCS Exploration Tool Node`
  - `PCS Runtime Hook`
  - `PCS Top Down Camera`
  - `PCS Inspector Landmark - Tool Shortcut Reward`

Remaining runtime caveat:

- The 30-second pass was automated batchmode smoke verification, not a manual controller playthrough in the live Unity viewport.

## 8. Next Smallest Useful Task

Next run:

- A 30-second manual controller playthrough for `ProductionCombatSlice` in the live Unity viewport.

If `{{SCENE_NAME}}` was intended to mean a different scene, redo this pass against the explicitly named scene and keep this change as a small production-slice traceability improvement only if desired.
