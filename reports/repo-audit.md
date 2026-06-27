# Repository Audit

作成日: 2026-06-27
対象: FOURFOLD ECHOES Unity ローカル checkout
方針: 監査としては読み取り中心。今回の成果物として `reports/repo-audit.md`, `reports/dependency-map.md`, `reports/task-split.md` のみを更新する。

## 監査時点の注意

監査中に、私が作成した3つの report 以外にも未コミットの code/config/test/CI 変更がワークツリー上に現れた。現時点の worktree を正として監査するが、これらの authorship はこの監査では断定しない。

確認時点の代表的な未コミット領域:

```text
AGENTS.md
Packages/manifest.json
Packages/packages-lock.json
Assets/Scripts/FourfoldEchoes.Product.asmdef
Assets/Scripts/Combat/*
Assets/Scripts/Enemies/*
Assets/Scripts/ProductionCombatSliceController.cs
Assets/Scripts/ProductionCombatSliceUi.cs
Assets/Tests/*
.github/workflows/*
Assets/Scenes/ProductionCombatSlice.unity
reports/*
```

## 必須確認結果

| 対象 | 事実 | 備考 |
| --- | --- | --- |
| `ProjectSettings/ProjectVersion.txt` | 存在。`m_EditorVersion: 6000.3.18f1` | revision は `5ebeb53e4c07` |
| `Packages/manifest.json` | 存在 | `com.unity.test-framework` が direct dependency。Input System、Cinemachine、Addressables、URP/HDRP は direct dependency なし |
| `Assets/**/*.asmdef` | 3 件 | runtime 1、EditMode tests 1、PlayMode tests 1 |
| `Assets/**/*.unity` | 3 件 | `ProductionCombatSlice`, `D020VerticalSlice`, `AshenThresholdSpike` |
| `Assets/**/Tests*` | 存在 | `Assets/Tests/EditMode`, `Assets/Tests/PlayMode` |
| `.github/workflows/*` | 3 件 | `validate.yml`, `build.yml`, `security.yml` |
| `README*` | 存在 | root `README.md` のほか tools/artifacts 配下に README あり |
| `LICENSE*` | root には不在 | `docs/Legal/LICENSES.md`, `docs/Art/LICENSES.md` は存在 |
| `AGENTS.md` | 存在 | 現行 product direction と storage hygiene を定義 |

## Unity とレンダーパイプライン

### 事実

- Unity editor target は `6000.3.18f1`。
- `ProjectSettings/GraphicsSettings.asset` は `m_CustomRenderPipeline: {fileID: 0}`。
- `ProjectSettings/QualitySettings.asset` の各 quality level も `customRenderPipeline: {fileID: 0}`。
- `Packages/manifest.json` に URP/HDRP package はない。
- `Assets/Editor/FourfoldD020SliceSceneBuilder.cs` は `Shader.Find("Standard")` で material を作る。
- `Assets/Editor/FourfoldUnitySpikeBuilder.cs` には `Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")` が残る。

### 推測

- 現在の実行前提は Built-in Render Pipeline。
- Spike builder の URP shader 探索は legacy fallback であり、SRP 採用の根拠にはならない。

### 未確認

- Unity Editor 起動による compile、scene open、material validation。
- CI 上の Unity test/build 成功。

## Packages

### 直接依存

```text
Project
  -> com.unity.ai.assistant 2.13.0-pre.1
  -> com.unity.ai.inference 2.6.1
  -> com.unity.test-framework 1.6.0
  -> com.unity.modules.ai
  -> com.unity.modules.animation
  -> com.unity.modules.audio
  -> com.unity.modules.imgui
  -> com.unity.modules.physics
  -> com.unity.modules.uielements
```

### 主要な間接依存

`Packages/packages-lock.json` では `com.unity.burst`, `com.unity.collections`, `com.unity.dt.app-ui`, `com.unity.mathematics`, `com.unity.test-framework.performance`, `com.unity.nuget.newtonsoft-json` などを確認した。

### 不在を確認した package

- `com.unity.inputsystem`
- `com.unity.cinemachine`
- `com.unity.addressables`
- URP/HDRP package

## asmdef と assembly 境界

### 事実

```text
Assets/Scripts/FourfoldEchoes.Product.asmdef
Assets/Tests/EditMode/FourfoldEchoes.EditModeTests.asmdef
Assets/Tests/PlayMode/FourfoldEchoes.PlayModeTests.asmdef
```

`FourfoldEchoes.Product.asmdef` は `Assets/Scripts` 直下にあるため、現在は `Assets/Scripts` 配下のすべての runtime script を含む。つまり assembly 名は `FourfoldEchoes.Product` だが、`FourfoldEchoes.Spike` namespace の legacy scripts も同じ assembly に入る。

Test assemblies は `FourfoldEchoes.Product` を参照し、`optionalUnityReferences` に `TestAssemblies` を持つ。

### 未確認

- Unity import 後に scene YAML が asmdef 後の assembly name へ再シリアライズされるか。
- 現 scene YAML の `m_EditorClassIdentifier` はまだ `Assembly-CSharp::...` 表記を含む。

## シーン一覧と役割

### Build Settings

`ProjectSettings/EditorBuildSettings.asset` で enabled:

```text
1. Assets/Scenes/ProductionCombatSlice.unity
2. Assets/Scenes/D020VerticalSlice.unity
```

### Assets 配下の scene

| Scene | Build Settings | 実構造 | 役割 |
| --- | --- | --- | --- |
| `Assets/Scenes/ProductionCombatSlice.unity` | enabled | GameObject 19、MonoBehaviour 4、Camera 1、Light 4、PrefabInstance 159 | production prefab を使った戦闘/探索ツール/報酬 proof。UI/runtime state 変更が入っている |
| `Assets/Scenes/D020VerticalSlice.unity` | enabled | GameObject 232、MonoBehaviour 2、Camera 1、Light 4、PrefabInstance 0 | D-020 の generated proof。primitive/generated material 中心 |
| `Assets/Scenes/AshenThresholdSpike.unity` | not enabled | GameObject 278、MonoBehaviour 1、Camera 1、Light 6、PrefabInstance 0 | 旧 Gate A / spike proof。README/AGENTS 上は historical 扱い |

### Scene to MonoBehaviour

```text
ProductionCombatSlice.unity
  -> FourfoldEchoes.Product.ExplorationTool
  -> FourfoldEchoes.Product.ExplorationNode
  -> FourfoldEchoes.Product.ProductionCombatSliceController
  -> FourfoldEchoes.Spike.FourfoldProofAudio

D020VerticalSlice.unity
  -> FourfoldEchoes.Product.ExplorationNode
  -> FourfoldEchoes.Product.ExplorationTool

AshenThresholdSpike.unity
  -> FourfoldEchoes.Spike.FourfoldUnitySpikeController
```

## スクリプト配置

### Runtime

| Folder | 主な namespace/type | 事実 |
| --- | --- | --- |
| `Assets/Scripts` | `ExplorationTool`, `ExplorationNode`, `ProductionCombatSliceController`, `ProductionCombatSliceUi` | product proof runtime と UI Toolkit runtime UI |
| `Assets/Scripts/Combat` | `Damageable`, `DamageInfo` | health/damage event の小さい境界 |
| `Assets/Scripts/Enemies` | `EnemyController`, `EnemyDefinition`, `EnemyMotor`, `EnemySensor`, `EnemyAttackDriver`, `EnemyAnimatorBridge` | common enemy AI baseline |
| `Assets/Scripts` | `FourfoldUnitySpikeController`, `FourfoldProofAudio` | legacy Gate A / procedural audio proof |

### Editor

| Folder | 主な files | 役割 |
| --- | --- | --- |
| `Assets/Editor` | scene builders | D-020、production combat、legacy spike、enemy AI verification scene の生成/検証 |
| `Assets/Editor` | importers/postprocessor | generated model pack、mass asset、Art import policy |
| `Assets/Editor` | validator/build/capture | product validation、Unity build、screenshot evidence |
| `Assets/Editor/Mediator` | Forge mediator/menu/inbox | command file based editor automation |

## 入力系

### 事実

- `ProjectSettings/ProjectSettings.asset` は `activeInputHandler: 0`。
- `Packages/manifest.json` に `com.unity.inputsystem` はない。
- `.inputactions` asset は未検出。
- runtime は `Input.GetAxisRaw`, `Input.GetKeyDown`, `Input.GetKey`, `Input.GetMouseButtonDown`, `KeyCode.JoystickButton*` を直接使用。
- `ProductionCombatSliceUi` も legacy input polling を使う runtime UI flow。

### 推測

- 現状は legacy Input Manager 前提。
- controller-first 方向に進むには、Input System 導入前でも `InputReader` などの action abstraction が必要。

### 未確認

- Input Manager axis の詳細設定。
- 実機 controller の操作感。

## カメラ系

### 事実

- `com.unity.cinemachine` は package にない。
- `Cinemachine` 文字列は current runtime/editor/scene から検出されなかった。
- 各 scene は `Camera` component を 1 つ持つ。
- D-020 と ProductionCombatSlice の builder は top-down 固定 `Camera` を作成し、`tag = "MainCamera"` を付ける。

### 推測

- 現状は Cinemachine なしの固定 top-down camera proof。
- 最初の camera hardening は package 追加ではなく、Built-in Camera の小型 `TopDownCameraRig` が安全。

## アセットロード

### 事実

- Addressables package と `AddressableAssetsData` は未検出。
- `Resources` folder と `Resources.Load` は未検出。
- `StreamingAssets` folder は未検出。
- runtime は serialized direct reference と scene/prefab references 中心。
- editor automation は `AssetDatabase` と `PrefabUtility` を多用。

### 推測

- docs の方針どおり、Addressables は現時点では deferred。
- production slice は direct prefab instance と scene wiring の検証 lane。

## テスト

### 事実

EditMode:

```text
Assets/Tests/EditMode/BuildSettingsScopeTests.cs
Assets/Tests/EditMode/ExplorationNodeTests.cs
Assets/Tests/EditMode/ExplorationToolTests.cs
```

PlayMode:

```text
Assets/Tests/PlayMode/EnemyControllerPlayModeTests.cs
Assets/Tests/PlayMode/SliceSceneSmokeTests.cs
```

Coverage summary:

- Build Settings に current slice scenes が入り、retired scenes が入らないこと。
- `ExplorationNode` の range、activation、presentation、reset。
- `ExplorationTool` の audio source setup、nearest usable node、miss cooldown。
- D-020 scene load と shortcut activation smoke。
- ProductionCombatSlice scene load、combat/gate/reward wiring、shortcut reaction smoke。
- Enemy AI FSM、leash、line of sight、obstacle fallback movement。

### 未確認

- Unity Editor / batchmode での test 実行結果。既存 report では別 Unity instance により実行が blocked された形跡がある。
- Full production combat completion path、input simulation、save integration、HUD/controller UI interaction。

## CI

### 事実

```text
.github/workflows/validate.yml
.github/workflows/build.yml
.github/workflows/security.yml
```

`validate.yml`:

- repo guards: public hygiene、repo validation、Forge contract check、Node syntax check、diff whitespace。
- Unity EditMode/PlayMode tests: `game-ci/unity-test-runner`、`UNITY_LICENSE` secret。

`build.yml`:

- push/main または manual。
- `game-ci/unity-builder` で D-020 macOS slice build。
- `FourfoldEchoes.Editor.FourfoldUnityBuild.BuildCurrentD020Slice` を呼ぶ。

`security.yml`:

- CodeQL C#。

### 未確認

- GitHub remote 上で workflow が実際に成功するか。
- `UNITY_LICENSE` secret の有無。
- Unity Linux runner で StandaloneOSX build target が期待どおり動くか。

## 現状のモジュール境界

```text
FourfoldEchoes.Product assembly
  -> Product runtime proof
  -> Combat/Damageable
  -> Enemy AI baseline
  -> UI Toolkit runtime UI
  -> Legacy Spike namespace scriptsも含む

FourfoldEchoes.EditModeTests assembly
  -> FourfoldEchoes.Product

FourfoldEchoes.PlayModeTests assembly
  -> FourfoldEchoes.Product

Assembly-CSharp-Editor or editor compile context
  -> Assets/Editor/*
  -> runtime types
  -> UnityEditor APIs
```

## 後続ロールが編集してよいフォルダ境界案

### 原則として編集可

| 領域 | 条件 |
| --- | --- |
| `Assets/Scripts/Combat`, `Assets/Scripts/Enemies` | current enemy/combat baseline の follow-up |
| `Assets/Scripts/ExplorationTool` または既存 `ExplorationTool/Node` | one-tool scope を守る |
| `Assets/Scripts/Player`, `Assets/Scripts/Core`, `Assets/Scripts/Rooms`, `Assets/Scripts/Save`, `Assets/Scripts/UI`, `Assets/Scripts/Audio` | 新規 runtime 境界を追加する場合 |
| `Assets/Tests/*` | runtime 変更に対応する test |
| `Assets/Editor/*` | editor validation/build/import scene builder |
| `.github/workflows/*` | CI worker のみ |
| `reports/*` | audit/report worker のみ |

### 原則として避ける

| 領域 | 理由 |
| --- | --- |
| `ProjectSettings/*` | editor version、input backend、render pipeline、build settings への影響が大きい |
| `Packages/*` | package 追加は tech lead decision。Input System/Cinemachine/Addressables は特に要承認 |
| `Assets/Scripts/FourfoldUnitySpikeController.cs` | historical proof。現行 product から依存させない |
| `Assets/Scenes/AshenThresholdSpike.unity` | historical proof。build settings に戻さない |
| `Library`, `Temp`, `Logs` | generated local cache |
| `.meta` 単独編集 | GUID/Importer 破壊リスク |

## 主なリスク

### 事実

- asmdef は導入済みだが、`Assets/Scripts` 直下なので Product と Spike namespace が同一 assembly に残っている。
- scene YAML は `Assembly-CSharp::...` の class identifier 表記をまだ含む。
- 入力は legacy static API 直呼び。
- Tests/CI は追加されているが、実行成功は未確認。

### 推測

- 次の事故ポイントは、asmdef 境界の誤解、scene serialized reference、Unity CI の license/build target、legacy input の testability。
- 実装を進める前に、Unity Editor で compile と EditMode/PlayMode test を通して現在の未コミット差分を固めるべき。

## 監査で未確認のもの

- Unity Editor compile。
- EditMode / PlayMode tests の実実行。
- CI green。
- Build artifact 作成。
- 実機 controller。
- save integration と full room completion path。
