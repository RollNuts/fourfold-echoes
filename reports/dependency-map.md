# Dependency Map

作成日: 2026-06-27
対象: FOURFOLD ECHOES Unity checkout

## 読み方

- `A -> B` は A が B に依存、参照、または生成時に利用する方向。
- `A -x-> B` は現状その依存がない、または避けるべき方向。
- 「事実」は現ファイルから確認したこと。「推測」は設計意図や安全な次手。「未確認」は Unity 実行や外部状態が必要なもの。

## Package Graph

### 事実

```text
Project
  -> com.unity.ai.assistant
  -> com.unity.ai.inference
  -> com.unity.test-framework
  -> com.unity.modules.ai
  -> com.unity.modules.animation
  -> com.unity.modules.audio
  -> com.unity.modules.imgui
  -> com.unity.modules.physics
  -> com.unity.modules.uielements
```

```text
com.unity.ai.assistant
  -> com.unity.mathematics
  -> com.unity.nuget.mono-cecil
  -> com.unity.nuget.newtonsoft-json
  -> com.unity.modules.unitywebrequest

com.unity.ai.inference
  -> com.unity.burst
  -> com.unity.collections
  -> com.unity.dt.app-ui
  -> com.unity.nuget.newtonsoft-json

com.unity.test-framework
  -> com.unity.ext.nunit
  -> com.unity.modules.imgui
  -> com.unity.modules.jsonserialize
```

```text
Project -x-> com.unity.inputsystem
Project -x-> com.unity.cinemachine
Project -x-> com.unity.addressables
Project -x-> URP/HDRP packages
```

## Assembly Graph

### 事実

```text
FourfoldEchoes.Product
  -> Assets/Scripts/*.cs
  -> Assets/Scripts/Combat/*.cs
  -> Assets/Scripts/Enemies/*.cs
  -> UnityEngine
  -> UnityEngine.AI
  -> UnityEngine.UIElements
```

`FourfoldEchoes.Product` は `Assets/Scripts` 直下の asmdef なので、以下を同じ assembly に含む:

```text
FourfoldEchoes.Product namespace
  -> ExplorationTool
  -> ExplorationNode
  -> ProductionCombatSliceController
  -> ProductionCombatSliceUi
  -> Damageable
  -> EnemyController family

FourfoldEchoes.Spike namespace
  -> FourfoldUnitySpikeController
  -> FourfoldProofAudio
```

```text
FourfoldEchoes.EditModeTests
  -> FourfoldEchoes.Product
  -> TestAssemblies
  -> NUnit
  -> UnityEditor for BuildSettingsScopeTests

FourfoldEchoes.PlayModeTests
  -> FourfoldEchoes.Product
  -> TestAssemblies
  -> NUnit
  -> UnityEngine.TestTools
```

```text
Editor compile context
  -> Assets/Editor/*.cs
  -> Assets/Editor/Mediator/*.cs
  -> UnityEditor
  -> UnityEditor.SceneManagement
  -> UnityEditor.Build.Reporting
  -> FourfoldEchoes.Product runtime types
```

### リスク

```text
FourfoldEchoes.Product asmdef at Assets/Scripts root
  -> Product and Spike are still compile-coupled
  -> assembly name suggests a cleaner boundary than actually exists
  -> future split needs explicit migration plan
```

## Runtime Dependency Graph

### Exploration

```text
ExplorationTool
  -> ExplorationNode[]
  -> UnityEngine.Input legacy API
  -> AudioSource / AudioClip
  -> serialized player Transform
  -> serialized pulse GameObject

ExplorationNode
  -> serialized responseTarget GameObject
  -> serialized idle/active reads
  -> serialized Renderer highlights
```

### Production slice

```text
ProductionCombatSliceController
  -> ExplorationTool
  -> ExplorationNode
  -> ProductionCombatSliceUi
  -> UnityEngine.Input legacy API
  -> serialized player/enemy/boss/gate/reward references
  -> simple internal health arrays
  -> gate/reward/shortcut state

ProductionCombatSliceUi
  -> ProductionCombatSliceController
  -> UnityEngine.UIElements
  -> legacy input polling for menu navigation
  -> runtime-created UIDocument / PanelSettings
```

### Combat and enemy AI

```text
Damageable
  -> DamageInfo
  -> Damaged event
  -> Died event

EnemyDefinition
  -> EnemyArchetype
  -> EnemyState
  -> tuning data for sensing/movement/attack/retreat

EnemyController
  -> EnemyDefinition
  -> EnemyMotor
  -> EnemySensor
  -> EnemyAttackDriver
  -> EnemyAnimatorBridge
  -> Damageable

EnemyMotor
  -> optional NavMeshAgent
  -> fallback transform movement
  -> obstacle sphere cast

EnemySensor
  -> target Transform
  -> Damageable viability check
  -> Physics overlap/raycast

EnemyAttackDriver
  -> Damageable targets
  -> Physics overlap

EnemyAnimatorBridge
  -> optional Animator parameters
```

### Legacy spike

```text
FourfoldUnitySpikeController
  -> FourfoldProofAudio
  -> EchoPhase
  -> UnityEngine.Input legacy API
  -> runtime primitive indicators

FourfoldProofAudio
  -> EchoPhase
  -> procedural AudioClip generation
  -> AudioSource
```

## Scene Graph

### Build Settings

```text
EditorBuildSettings
  -> Assets/Scenes/ProductionCombatSlice.unity
  -> Assets/Scenes/D020VerticalSlice.unity
```

### Scene to script

```text
ProductionCombatSlice.unity
  -> ExplorationTool
  -> ExplorationNode
  -> ProductionCombatSliceController
  -> FourfoldProofAudio
  -> Assets/Prefabs/Production/* prefab instances

D020VerticalSlice.unity
  -> ExplorationTool
  -> ExplorationNode
  -> generated primitive scene objects

AshenThresholdSpike.unity
  -> FourfoldUnitySpikeController
  -> generated primitive scene objects
```

### 未確認

Scene YAML の `m_EditorClassIdentifier` は `Assembly-CSharp::...` 表記を含む。Unity は script GUID で参照を解決するはずだが、asmdef 後の import/compile 成功は未確認。

## Editor Automation Graph

### Scene builders

```text
FourfoldD020SliceSceneBuilder
  -> ExplorationTool
  -> ExplorationNode
  -> Assets/Scenes/D020VerticalSlice.unity
  -> Assets/Art/Generated/D020/Materials

FourfoldProductionCombatSliceSceneBuilder
  -> ExplorationTool
  -> ExplorationNode
  -> ProductionCombatSliceController
  -> FourfoldProofAudio
  -> Assets/Prefabs/Production/*
  -> Assets/Scenes/ProductionCombatSlice.unity

FourfoldUnitySpikeBuilder
  -> FourfoldUnitySpikeController
  -> FourfoldProofAudio
  -> Assets/Scenes/AshenThresholdSpike.unity

FourfoldEnemyAiVerificationSceneBuilder
  -> EnemyDefinition
  -> EnemyController
  -> Damageable
  -> Assets/Scenes/AI_EnemyController_Verification.unity
  -> Assets/Generated/AI/Definitions
  -> Assets/Generated/AI/Materials
```

`AI_EnemyController_Verification.unity` は builder の出力先として定義されているが、現時点の `Assets/**/*.unity` には存在しない。

### Validation / capture / build

```text
FourfoldProductValidator
  -> D020 builder
  -> ProductionCombatSlice builder
  -> optional legacy Gate A builder
  -> AssetDatabase scans
  -> artifacts/Reports/unity-product-validation.*

FourfoldUnityEvidenceCapture
  -> D020/GateA builders
  -> Camera.main or first Camera
  -> RenderTexture screenshot capture

FourfoldUnityBuild
  -> D020 builder
  -> legacy Gate A builder
  -> BuildPipeline.BuildPlayer()
```

### Asset import and Forge

```text
FourfoldGeneratedModelPackImporter
  -> artifacts/Reports/fourfold-model-pack.json
  -> AssetDatabase.LoadAssetAtPath()
  -> PrefabUtility.SaveAsPrefabAsset()

FourfoldMassAssetImporter
  -> asset_manifest.json
  -> Assets/Prefabs/MassProduction/*
  -> artifacts/Reports/mass-asset-unity-import.json

FourfoldArpgAssetPostprocessor
  -> Assets/Art/*
  -> model / texture / audio import settings

Forge command files or inbox
  -> FourfoldForgeMediator / FourfoldForgeCommandInbox
  -> builders / validators / importers / capture
```

## Test Graph

```text
EditMode tests
  -> BuildSettingsScopeTests
    -> EditorBuildSettings
  -> ExplorationNodeTests
    -> ExplorationNode
  -> ExplorationToolTests
    -> ExplorationTool
    -> ExplorationNode

PlayMode tests
  -> SliceSceneSmokeTests
    -> SceneManager.LoadScene(D020VerticalSlice)
    -> SceneManager.LoadScene(ProductionCombatSlice)
    -> ExplorationTool / ExplorationNode / ProductionCombatSliceController
  -> EnemyControllerPlayModeTests
    -> EnemyDefinition
    -> EnemyController
    -> EnemyMotor
    -> EnemySensor
    -> EnemyAttackDriver
    -> Damageable
```

## CI Graph

```text
.github/workflows/validate.yml
  -> repo-guards
    -> Scripts/Validation/check_public_repo_hygiene.mjs
    -> Scripts/Validation/validate_repo.mjs
    -> tools/forge/check.mjs
    -> node --check
    -> git diff --check
  -> unity-tests
    -> game-ci/unity-test-runner
    -> UNITY_LICENSE secret
    -> EditMode / PlayMode

.github/workflows/build.yml
  -> game-ci/unity-builder
  -> UNITY_LICENSE secret
  -> FourfoldEchoes.Editor.FourfoldUnityBuild.BuildCurrentD020Slice
  -> StandaloneOSX D-020 build

.github/workflows/security.yml
  -> CodeQL C#
```

## Asset Loading Graph

### Runtime

```text
Scenes
  -> serialized references
    -> Transform
    -> GameObject
    -> Material
    -> Camera
    -> AudioClip
    -> ExplorationNode[]
    -> Prefab instances

Runtime scripts
  -x-> Resources.Load
  -x-> Addressables
  -x-> StreamingAssets
```

### Editor

```text
Editor scripts
  -> AssetDatabase.FindAssets()
  -> AssetDatabase.LoadAssetAtPath()
  -> AssetDatabase.CreateAsset()
  -> PrefabUtility.InstantiatePrefab()
  -> PrefabUtility.SaveAsPrefabAsset()
  -> EditorSceneManager.SaveScene()
```

## Product Constraint Graph

### 事実

```text
AGENTS.md / README.md
  -> docs/Product/MVP_BLUEPRINT.md
  -> docs/Product/CORE_SYSTEMS.md
  -> docs/Product/SCOPE_BOUNDARIES.md
  -> docs/Tech/TECHNICAL_ARCHITECTURE.md

MVP ceiling
  -> 1 hub
  -> 3 regions
  -> 4 bosses
  -> 1 exploration tool
  -> no inventory / crafting / quest log / multiplayer / open world
```

## Recommended Direction

### 推奨

```text
Core
  -> no UI / UnityEditor dependency

Input abstraction
  -> wraps legacy Input first
  -> can later swap to Input System by decision

Player
  -> Core
  -> Input
  -> Combat

Combat
  -> Damageable / hit rules

Enemies
  -> Combat
  -> EnemyDefinition data

Rooms
  -> Player
  -> Combat
  -> Enemies
  -> ExplorationTool

UI
  -> read public state from controllers
  -> does not own gameplay state

Save
  -> progress flags only
  -> no inventory/quest expansion
```

### 避ける

```text
Runtime -x-> UnityEditor
Gameplay -x-> direct platform APIs
Gameplay -x-> direct file I/O for save
ExplorationTool -x-> inventory / ability tree systems
Rooms -x-> open-world streaming service
Product runtime -x-> Spike namespace
```

## Unknowns

- Unity compile after asmdef/test/CI/runtime additions.
- Scene references after asmdef import.
- CI green with available secrets.
- Whether `EnemyController` will replace duplicated hostile logic in `ProductionCombatSliceController`.
- Whether UI Toolkit runtime flow works with mouse/keyboard/controller in Play Mode.
