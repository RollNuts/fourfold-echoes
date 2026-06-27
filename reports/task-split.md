# Task Split for Follow-up Codex Workers

作成日: 2026-06-27
目的: 後続ロールが現ワークツリーの asmdef/tests/CI/enemy/UI 変更を踏まえ、FOURFOLD ECHOES の MVP 方向へ安全に実装できる分担を定義する。

## Current Baseline

### 事実

- Unity は `6000.3.18f1`。
- Built-in Render Pipeline。
- `FourfoldEchoes.Product.asmdef` が `Assets/Scripts` 直下にある。
- EditMode / PlayMode test assemblies がある。
- `.github/workflows` に validate/build/security がある。
- Input System、Cinemachine、Addressables は入っていない。
- Runtime は legacy `UnityEngine.Input` 直呼び。
- `Damageable` と `EnemyController` 系の baseline が追加されている。
- `ProductionCombatSliceUi` と title/pause/retry/completion flow が追加されている。
- Unity test/build の成功は未確認。

### 推測

- 今すぐ新機能を重ねるより、まず Unity compile/test/scene validation を通して current baseline を固定するべき。
- `FourfoldEchoes.Product` assembly は便利だが粗い。Product と Spike の分離は未完了。

## Recommended Order

```text
T1 Baseline verification
  -> T2 Assembly boundary cleanup
    -> T3 Production slice integration hardening
      -> T4 Input abstraction
        -> T5 Enemy AI integration
          -> T6 Room/save progression
            -> T7 Camera/HUD/audio polish
              -> T8 CI hardening
```

## T1 Baseline Verification

### Role

Unity verification worker

### Goal

現在の未コミット baseline が Unity で compile し、tests と scene smoke が通るかを確定する。

### Allowed folders

```text
reports/
artifacts/test-results/
```

テスト修正が必要な場合のみ:

```text
Assets/Tests/
Assets/Scripts/
Assets/Editor/
```

### Tasks

- Unity Editor が開いているなら競合しない方法で EditMode / PlayMode tests を実行する。
- `ProductionCombatSlice` と `D020VerticalSlice` を scene load する。
- asmdef 後の script reference / missing script / compile error を確認する。
- CI workflows の expected secrets と failure mode を確認する。

### Acceptance

- EditMode と PlayMode の結果が report に残る。
- 失敗がある場合、compile error、test failure、Unity instance lock、license/CI failure を分けて記録する。
- runtime code を広げずに baseline の健康状態が分かる。

## T2 Assembly Boundary Cleanup

### Role

Unity architecture worker

### Goal

`FourfoldEchoes.Product` asmdef の境界を実態に合わせ、後続が誤解しない状態にする。

### Allowed folders

```text
Assets/Scripts/
Assets/Tests/
Assets/Editor/
reports/
```

### Avoid

```text
Assets/Scenes/
ProjectSettings/
Packages/
```

### Tasks

- Product assembly に Spike namespace が含まれる現状を維持するか分離するか決める。
- 分離する場合は scene GUID/reference と editor builders の参照を確認する。
- Test asmdef references を最小化する。
- asmdef `.meta` を単独で壊さない。

### Acceptance

- Assembly names と folder ownership が一致する。
- `FourfoldEchoes.Spike` を current product runtime の依存先にしない。
- Unity compile/test が通る。

## T3 Production Slice Integration Hardening

### Role

Production slice worker

### Goal

`ProductionCombatSliceController`, `ProductionCombatSliceUi`, scene wiring の現在変更を安定させる。

### Allowed folders

```text
Assets/Scripts/ProductionCombatSliceController.cs
Assets/Scripts/ProductionCombatSliceUi.cs
Assets/Scenes/ProductionCombatSlice.unity
Assets/Tests/PlayMode/
reports/
```

### Tasks

- Title -> Playing -> Pause -> Retry -> Completed flow を Play Mode で確認する。
- `showDebugOverlay` と UI Toolkit HUD の責務を分ける。
- `ExplorationTool` が non-playing state で disabled になる挙動を確認する。
- Full combat completion path の smoke test を追加するか、現 controller の private/input coupling を先に薄くする。

### Acceptance

- Production scene が missing script/reference なしで開く。
- Start/Pause/Retry/Reward completion の UI flow が console error なし。
- Scene smoke test が current UI/run state と矛盾しない。

## T4 Input Abstraction

### Role

Input worker

### Goal

legacy static input を gameplay/UI から隔離し、controller-first の testable action layer を作る。

### Allowed folders

```text
Assets/Scripts/Core/
Assets/Scripts/Input/
Assets/Scripts/Player/
Assets/Scripts/UI/
Assets/Tests/
```

### Avoid

```text
Packages/manifest.json
ProjectSettings/ProjectSettings.asset
```

Input System package 追加は別 decision。

### Tasks

- `InputReader` または action snapshot を追加する。
- Move/Attack/Dodge/Interact/Tool/Pause/Retry を semantic action 化する。
- `ProductionCombatSliceController`, `ProductionCombatSliceUi`, `ExplorationTool` の static input 直呼びを段階的に置き換える。

### Acceptance

- 現操作を変えずに、test では action を注入できる。
- 新規 gameplay code は `Input.Get*` を直接呼ばない。

## T5 Enemy AI Integration

### Role

Enemy/combat worker

### Goal

追加済み `EnemyController` baseline を production slice または prefabs に統合し、重複した hostile logic を減らす。

### Allowed folders

```text
Assets/Scripts/Combat/
Assets/Scripts/Enemies/
Assets/Scripts/Rooms/
Assets/Prefabs/Production/
Assets/Scenes/ProductionCombatSlice.unity
Assets/Tests/PlayMode/
Assets/Editor/FourfoldEnemyAiVerificationSceneBuilder.cs
```

### Tasks

- `EnemyDefinition` assets の保存先と生成/手編集の方針を決める。
- Production prefabs または scene instances に `EnemyController` stack を接続する。
- `ProductionCombatSliceController` の enemies health/move/attack duplicate を段階的に削る。
- melee/ranged の readable tells と leash/LOS を PlayMode で確認する。

### Acceptance

- Enemy AI tests が通る。
- Production slice が少なくとも2体の normal enemies と boss gate flow を保つ。
- Behavior tree や複雑な combat system に広げない。

## T6 Room and Save Progression

### Role

Room/save worker

### Goal

shortcut、boss defeated、reward claimed、region unlock を local progress flags に接続する。

### Allowed folders

```text
Assets/Scripts/Rooms/
Assets/Scripts/Save/
Assets/Scripts/ExplorationTool/
Assets/Tests/
```

### Avoid

```text
Inventory/Crafting/Quest/Social folders
platform SDK direct calls
```

### Tasks

- `RoomController` を小さく作る。
- local save path と versioned save data を MVP flags に限定する。
- Scene load 時に shortcut/gate/reward state が復元されるようにする。

### Acceptance

- Save/load tests が progress flag roundtrip を証明する。
- Production/D020 scene flow を壊さない。
- inventory/quest log を作らない。

## T7 Camera, HUD, Audio Polish

### Role

Player experience worker

### Goal

読みやすい top-down camera、minimum HUD、semantic audio cues を整える。

### Allowed folders

```text
Assets/Scripts/UI/
Assets/Scripts/Audio/
Assets/Scripts/Player/
Assets/Audio/
Assets/Scenes/
Assets/Tests/
```

### Avoid

```text
com.unity.cinemachine package addition
large UI/settings scope
quest/minimap/social UI
```

### Tasks

- Built-in Camera の `TopDownCameraRig` を作る。
- UI Toolkit HUD が Steam Deck 16:10 でも収まることを確認する。
- `AudioCueRouter` で semantic cue を鳴らす。
- procedural/prototype audio と production-approved audio を分ける。

### Acceptance

- Camera は player/enemy/route/tool target が読める。
- HUD は title/pause/retry/completion flow と衝突しない。
- Audio は raw clip path 直叩きではなく cue 経由に寄る。

## T8 CI Hardening

### Role

CI/build worker

### Goal

追加済み workflows を実際に回る水準にする。

### Allowed folders

```text
.github/workflows/
Scripts/Validation/
tools/forge/
Assets/Editor/
reports/
```

### Tasks

- `UNITY_LICENSE` secret 不在時の failure を分かりやすくする。
- game-ci action versions と Unity version auto 解決を確認する。
- D-020 macOS build が ubuntu runner で成立するか検証する。
- Artifacts retention と Library cache key を見直す。

### Acceptance

- validate/build/security の各 workflow の expected green path と expected blocked path が document される。
- CI failure が code regression と infra/license 問題で判別できる。

## Folder Boundary Proposal

### Safe by default

```text
reports/
Assets/Tests/
Assets/Scripts/Combat/
Assets/Scripts/Enemies/
Assets/Scripts/Core/
Assets/Scripts/Input/
Assets/Scripts/Rooms/
Assets/Scripts/Save/
Assets/Scripts/UI/
Assets/Scripts/Audio/
```

### Task-owned only

```text
Assets/Scenes/
Assets/Prefabs/
Assets/Art/
Assets/Audio/
Assets/Editor/
.github/workflows/
Packages/
ProjectSettings/
```

### Do not use for source work

```text
Library/
Temp/
Logs/
Build/
```

## Acceptance Conditions for Future Work

Substantial implementation should report:

1. final-goal support
2. systems touched
3. files added/changed
4. implementation
5. tests
6. acceptance conditions
7. next smallest useful task

## Open Questions

- Should `FourfoldEchoes.Spike` be split into its own legacy assembly?
- Should `EnemyController` become the production slice enemy authority now, or after input abstraction?
- Should `ProductionCombatSlice` remain generated, become hand-authored, or keep a hybrid builder/hand-edit policy?
- Should Input System be adopted later, or should a custom abstraction over legacy input carry the vertical slice?
- What command is the canonical local verification gate: Unity Test Runner, `FourfoldProductValidator.RunAll`, Forge mediator, or a wrapper script?
