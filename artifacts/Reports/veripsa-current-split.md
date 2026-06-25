# Veripsa Current Split Report

Generated UTC: `2026-06-25T18:23:31.438Z`

Core source: GitHub Veripsa checks; local veripsa CLI unavailable

## Core-Derived Rules

- PR #17 codex/store-readiness-pack: Veripsa `SUCCESS`. Docs-only changes with a narrow path set are a Veripsa-friendly landing unit.
- PR #14 codex/gate-a-evidence-harness: Veripsa `NEUTRAL`. New Unity editor/tool files were not in main's graph, so Core treated them as unknown. Split new C# runtime, editor scene generation, and capture/build tooling into separate PRs instead of landing them with docs.

Dirty files: 144

## Recommended Lanes

### PR-A - product-canon

- None

### PR-B - art-audio-direction

- `??` Assets/Audio.meta

### PR-C - production-release-plans

- None

### PR-D - validation-sync

- `A` Scripts/Validation/validate_repo.mjs
- `AM` Scripts/Validation/write_veripsa_split_report.mjs
- `M` artifacts/.gitignore
- `A` artifacts/Reports/.gitkeep
- `A` artifacts/Reports/veripsa-current-split.json
- `A` artifacts/Reports/veripsa-current-split.md
- `A` commands/samples/inspect-d020-slice.json
- `M` tools/forge/check.mjs

### PR-E1 - d020-tool-runtime

- `??` Assets/Scripts/ExplorationNode.cs
- `??` Assets/Scripts/ExplorationNode.cs.meta
- `??` Assets/Scripts/ExplorationTool.cs
- `??` Assets/Scripts/ExplorationTool.cs.meta

### PR-E2 - d020-scene-evidence

- `M` ProjectSettings/EditorBuildSettings.asset
- `??` Assets/Art.meta
- `??` Assets/Art/Generated.meta
- `??` Assets/Art/Generated/D020.meta
- `??` Assets/Art/Generated/D020/Materials.meta
- `??` Assets/Art/Generated/D020/Materials/D020_ChestWood.mat
- `??` Assets/Art/Generated/D020/Materials/D020_ChestWood.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_EnemyInk.mat
- `??` Assets/Art/Generated/D020/Materials/D020_EnemyInk.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_EnemyTell.mat
- `??` Assets/Art/Generated/D020/Materials/D020_EnemyTell.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_Floor.mat
- `??` Assets/Art/Generated/D020/Materials/D020_Floor.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_FloorDark.mat
- `??` Assets/Art/Generated/D020/Materials/D020_FloorDark.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_PlayerCape.mat
- `??` Assets/Art/Generated/D020/Materials/D020_PlayerCape.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_PlayerIvory.mat
- `??` Assets/Art/Generated/D020/Materials/D020_PlayerIvory.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_RelicBlue.mat
- `??` Assets/Art/Generated/D020/Materials/D020_RelicBlue.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_RouteGold.mat
- `??` Assets/Art/Generated/D020/Materials/D020_RouteGold.mat.meta
- `??` Assets/Art/Generated/D020/Materials/D020_ToolSignal.mat
- `??` Assets/Art/Generated/D020/Materials/D020_ToolSignal.mat.meta
- `??` Assets/Editor/FourfoldD020SliceSceneBuilder.cs
- `??` Assets/Editor/FourfoldD020SliceSceneBuilder.cs.meta
- `??` Assets/Scenes/D020VerticalSlice.unity
- `??` Assets/Scenes/D020VerticalSlice.unity.meta

### PR-E3 - d020-capture-build

- `M` Assets/Editor/FourfoldUnityBuild.cs
- `M` Assets/Editor/FourfoldUnityEvidenceCapture.cs
- `??` Scripts/Validation/run_all.sh
- `??` Scripts/Validation/write_market_reports.mjs
- `??` artifacts/Reports/audio-inventory.json
- `??` artifacts/Reports/audio-inventory.md
- `??` artifacts/Reports/final-status-report.json
- `??` artifacts/Reports/final-status-report.md
- `??` artifacts/Reports/performance-snapshot.json
- `??` artifacts/Reports/performance-snapshot.md
- `??` artifacts/Reports/unity-product-validation.json
- `??` artifacts/Reports/unity-product-validation.md
- `??` tools/unity_capture_d020_slice.sh

### PR-F - historical-proof-cleanup

- `D` events/.gitkeep
- `??` Assets/Art/Generated/Materials.meta
- `??` Assets/Art/Generated/Materials/ProductReview_Bloom.mat
- `??` Assets/Art/Generated/Materials/ProductReview_Bloom.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_Ember.mat
- `??` Assets/Art/Generated/Materials/ProductReview_Ember.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_EnemyObsidian.mat
- `??` Assets/Art/Generated/Materials/ProductReview_EnemyObsidian.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_EnemyThreat.mat
- `??` Assets/Art/Generated/Materials/ProductReview_EnemyThreat.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_HeroCloak.mat
- `??` Assets/Art/Generated/Materials/ProductReview_HeroCloak.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_HeroIvory.mat
- `??` Assets/Art/Generated/Materials/ProductReview_HeroIvory.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_HeroSteel.mat
- `??` Assets/Art/Generated/Materials/ProductReview_HeroSteel.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_Moss.mat
- `??` Assets/Art/Generated/Materials/ProductReview_Moss.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_Prism.mat
- `??` Assets/Art/Generated/Materials/ProductReview_Prism.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_RewardPrism.mat
- `??` Assets/Art/Generated/Materials/ProductReview_RewardPrism.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_RouteGold.mat
- `??` Assets/Art/Generated/Materials/ProductReview_RouteGold.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_Stone.mat
- `??` Assets/Art/Generated/Materials/ProductReview_Stone.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_StoneDark.mat
- `??` Assets/Art/Generated/Materials/ProductReview_StoneDark.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_Telegraph.mat
- `??` Assets/Art/Generated/Materials/ProductReview_Telegraph.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_Tide.mat
- `??` Assets/Art/Generated/Materials/ProductReview_Tide.mat.meta
- `??` Assets/Art/Generated/Materials/ProductReview_TideWater.mat
- `??` Assets/Art/Generated/Materials/ProductReview_TideWater.mat.meta
- `??` Assets/Art/Generated/Meshes.meta
- `??` Assets/Art/Generated/Meshes/product_review_crystal.asset
- `??` Assets/Art/Generated/Meshes/product_review_crystal.asset.meta
- `??` Assets/Art/Generated/Prefabs.meta
- `??` Assets/Art/Generated/Prefabs/BossReadTarget.prefab
- `??` Assets/Art/Generated/Prefabs/BossReadTarget.prefab.meta
- `??` Assets/Art/Generated/Prefabs/EnemyReadTarget.prefab
- `??` Assets/Art/Generated/Prefabs/EnemyReadTarget.prefab.meta
- `??` Assets/Art/Generated/Prefabs/PlayerReadTarget.prefab
- `??` Assets/Art/Generated/Prefabs/PlayerReadTarget.prefab.meta
- `??` Assets/Art/Generated/Prefabs/RewardReadTarget.prefab
- `??` Assets/Art/Generated/Prefabs/RewardReadTarget.prefab.meta
- `??` Assets/Art/Generated/Prefabs/WorldLandmark.prefab
- `??` Assets/Art/Generated/Prefabs/WorldLandmark.prefab.meta
- `??` Assets/Art/Generated/Textures.meta
- `??` Assets/Art/Generated/Textures/product_review_stone_trim.png
- `??` Assets/Art/Generated/Textures/product_review_stone_trim.png.meta
- `??` Assets/Editor/FourfoldProductReviewSceneBuilder.cs
- `??` Assets/Editor/FourfoldProductReviewSceneBuilder.cs.meta
- `??` Assets/Editor/FourfoldProductValidator.cs
- `??` Assets/Editor/FourfoldProductValidator.cs.meta
- `??` Assets/Scenes/ProductReviewSandbox.unity
- `??` Assets/Scenes/ProductReviewSandbox.unity.meta
- `??` Assets/Scripts/EchoPhaseState.cs
- `??` Assets/Scripts/EchoPhaseState.cs.meta
- `??` Assets/Scripts/FourfoldProductReviewController.cs
- `??` Assets/Scripts/FourfoldProductReviewController.cs.meta
- `??` tools/unity_capture_product_review.sh

### PR-G - forge-mediator-sync

- `M` Assets/Editor/Mediator/FourfoldForgeMediator.cs
- `M` Assets/Editor/Mediator/FourfoldForgeMenuItems.cs
- `M` tools/unity_forge_command.sh
- `??` ProjectSettings/PackageManagerSettings.asset

### PR-H - asset-pipeline-pilot-optional

- `??` Assets/Art/Generated/BlenderPilot.meta
- `??` Assets/Art/Generated/BlenderPilot/Models.meta
- `??` Assets/Art/Generated/BlenderPilot/Models/FE_Boss_Guardian_Pilot.fbx
- `??` Assets/Art/Generated/BlenderPilot/Models/FE_Boss_Guardian_Pilot.fbx.meta
- `??` Assets/Art/Generated/BlenderPilot/Models/FE_Env_RuinKit_Pilot.fbx
- `??` Assets/Art/Generated/BlenderPilot/Models/FE_Env_RuinKit_Pilot.fbx.meta
- `??` Assets/Art/Generated/BlenderPilot/Models/FE_Hero_Pilot.fbx
- `??` Assets/Art/Generated/BlenderPilot/Models/FE_Hero_Pilot.fbx.meta
- `??` Assets/Art/Generated/BlenderPilot/Prefabs.meta
- `??` Assets/Art/Generated/BlenderPilot/Prefabs/FE_Boss_Guardian_Pilot.prefab
- `??` Assets/Art/Generated/BlenderPilot/Prefabs/FE_Boss_Guardian_Pilot.prefab.meta
- `??` Assets/Art/Generated/BlenderPilot/Prefabs/FE_Env_RuinKit_Pilot.prefab
- `??` Assets/Art/Generated/BlenderPilot/Prefabs/FE_Env_RuinKit_Pilot.prefab.meta
- `??` Assets/Art/Generated/BlenderPilot/Prefabs/FE_Hero_Pilot.prefab
- `??` Assets/Art/Generated/BlenderPilot/Prefabs/FE_Hero_Pilot.prefab.meta
- `??` Assets/Art/Generated/BlenderPilot/Source.meta
- `??` Assets/Art/Generated/BlenderPilot/Source/FE_BlenderPilotAssets.blend
- `??` Assets/Art/Generated/BlenderPilot/Source/FE_BlenderPilotAssets.blend.meta
- `??` artifacts/Reports/blender-pilot-assets.json
- `??` tools/AssetPipeline/README.md
- `??` tools/AssetPipeline/validate_generated_assets.mjs
- `??` tools/Blender/README.md
- `??` tools/Blender/generate_pilot_assets.py

## Unknown / Needs Manual Lane

- None

## Use

- Keep PR-A through PR-D reviewable before any Unity scene/build lane lands.
- Treat new Unity C# and generated scene paths as Veripsa UNKNOWN until main indexes them.
- Do not ACK a Veripsa Pause without reading the overlapping files and recording why the D-020 lane is authoritative.
