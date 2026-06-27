# FOURFOLD ECHOES Test Plan

Last updated: 2026-06-27

## Goal

Keep QA focused on the current product truth: a compact, Steam-first,
single-player top-down action-adventure built around one exploration tool.
Automation should prove stable contracts cheaply, while manual smoke checks
cover feel, readability, art, and audio.

## Automated Coverage

| Layer | Location | Coverage | Spec link |
| --- | --- | --- | --- |
| EditMode | `Assets/Tests/EditMode/ExplorationNodeTests.cs` | Exploration node range, reveal/activate state, reset state | `CORE_EXPLORATION_NODE`, `MVP` one exploration tool |
| EditMode | `Assets/Tests/EditMode/ExplorationToolTests.cs` | Missing-node safety, nearest usable node selection, miss cooldown | `CORE_EXPLORATION_TOOL` |
| EditMode | `Assets/Tests/EditMode/CombatEnemyContractTests.cs` | Damage clamping/death events, enemy attack arc filtering, self-hit and duplicate-collider prevention | `CORE_DAMAGEABLE`, `CORE_ENEMY_ATTACK` |
| EditMode | `Assets/Tests/EditMode/LocalSaveServiceTests.cs` | New-game defaults, progress flag roundtrip, settings normalization, missing/corrupt save fallback | `CORE_SAVE`, `MVP` local progress |
| EditMode | `Assets/Tests/EditMode/ProductionCombatSliceProgressTests.cs` | Production slice shortcut, boss, and reward flags map to save data and cascade safely on read/write | `CORE_SAVE`, `SLICE_PRODUCTION` |
| EditMode | `Assets/Tests/EditMode/BuildSettingsScopeTests.cs` | Build settings stay on current slice scenes and exclude retired Gate A/ProductReviewSandbox paths | `MVP_SCOPE` |
| PlayMode | `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs` | D-020 scene loads, has one tool node, opens shortcut, keeps readable camera/renderers | `SLICE_D020` |
| PlayMode | `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs` | Production combat slice loads with player, two enemies, boss, reward, gate, and exploration shortcut wiring | `SLICE_PRODUCTION` |

## Run Commands

From the repository root, prefer Unity Test Runner in the editor. For batchmode,
use the project editor target from `ProjectSettings/ProjectVersion.txt`:

```bash
/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults reports/editmode-results.xml -logFile reports/unity-editmode.log
/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults reports/playmode-results.xml -logFile reports/unity-playmode.log
```

If the Unity editor is already open on this checkout, use the Test Runner window
instead of launching a second editor process.

Do not add `-quit` to these commands. Unity Test Framework 1.6 exits the editor
itself after the run; with `-quit`, batchmode can close before the test runner
starts.

## Test Design Rules

- Core logic stays in EditMode and uses public component contracts.
- Scene wiring and actual slice behavior stay in PlayMode.
- Tests do not use external network, services, package registries, or time of day.
- Tests avoid seconds-based waiting. `yield return null` is only used after scene
  load or after an explicit scene action to allow Unity lifecycle callbacks.
- Tests do not make production members public solely for QA.
- Current project uses legacy `UnityEngine.Input`, not the Input System package.
  Input simulation is therefore deferred until an input abstraction or
  `com.unity.inputsystem` is added.

## Manual QA Boundary

Automated tests do not decide whether combat feels good, enemy tells read, audio
is satisfying, art is store-ready, or controller play feels responsive. Those
must be checked with `docs/smoke-checklist.md` and captured evidence.
