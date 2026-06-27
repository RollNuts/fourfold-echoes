# QA Gap Analysis

Date: 2026-06-27

## Summary

The repository now has a minimal Unity Test Framework surface for the active
runtime path: EditMode tests for the exploration tool/node contract and PlayMode
smoke tests for the D-020 and production combat scenes. This is enough to catch
broken scene wiring, missing slice objects, and regressions in the one-tool
shortcut activation loop without adding network, timing, or service dependency.

## Covered

- `ExplorationNode` range, activation, presentation toggles, and reset behavior.
- `ExplorationTool` missing-node safety, nearest-node selection, and miss cooldown.
- `Damageable` health clamping, death event dispatch, and dead-target guard.
- `EnemyAttackDriver` forward-arc filtering, self-hit exclusion, dead-target
  exclusion, and duplicate-collider prevention.
- `LocalSaveService` missing/corrupt fallback, versioned new-game defaults, progress flag roundtrip, and settings normalization.
- `ProductionCombatSliceProgress` maps shortcut, boss, and reward progress to
  local save flags with cascade rules: reward implies boss and shortcut; boss
  implies shortcut.
- Build settings exclude retired historical scenes and include current slice scenes.
- `D020VerticalSlice` loads, exposes one exploration node, opens its shortcut, and has a readable camera/render surface.
- `ProductionCombatSlice` loads with two enemies, boss, gate/reward wiring, and exploration shortcut reaction.
- `ProductionCombatSlice` restores a saved claimed reward as an opened
  shortcut, unlocked/open boss gate, visible reward pad, and completed run state
  without reading the user's default local save during PlayMode tests.
- Runtime/test assembly layout is normalized to a single gameplay assembly:
  `Assets/Scripts/FourfoldEchoes.Product.asmdef`. EditMode and PlayMode test
  assemblies both reference `FourfoldEchoes.Product`.

## Not Yet Covered

- Full combat completion path: enemies defeated, shortcut solved, boss unlocked,
  boss defeated, gate opened, reward claimed.
- Keyboard/controller input simulation.
- Hub/region scene-flow integration that applies saved region flags to live
  routing. `ProductionCombatSlice` can now apply and write its shortcut, boss,
  and reward flags when `useLocalSave` is enabled.
- HUD, pause/settings, and controller remapping.
- Audio correctness beyond manual smoke.
- Art quality, animation quality, camera composition, and market-readiness.

## Untestable Or High-Cost Areas

- Input simulation: the project does not include `com.unity.inputsystem` and the
  runtime reads legacy static `UnityEngine.Input` directly. Unity's legacy input
  path is not reliably injectable in tests, so the current PlayMode tests invoke
  public gameplay methods instead of faking keypresses.
- Combat progression: `ProductionCombatSliceController` currently mixes input,
  health state, enemy movement, gate state, reward state, and presentation in one
  `MonoBehaviour`. The required actions are mostly private and driven by static
  input polling, so fully deterministic progression tests would need reflection
  or frame-sensitive input tricks. That would be brittle, so this pass limits
  automation to public state and scene wiring.
- Save/load integration: the local save service now has EditMode coverage and
  `ProductionCombatSlice` can write its local progress when enabled, but hub and
  region gates do not yet consume saved region progress.
- Generated scene validation: editor builders already validate generated scenes,
  but invoking them inside regular tests would rewrite scene/assets and make the
  test suite expensive and noisy. Keep builder validation as an explicit Unity
  validation command.

## Recommended Next QA Refactor

1. Add a small `InputReader` or install/adopt Unity Input System so EditMode or
   PlayMode tests can simulate attack, tool, reset, and reward interaction.
2. Extract combat room state into a small domain object or service that exposes
   deterministic methods for damage, defeat, boss unlock, gate open, and reward
   claim.
3. Wire `LocalSaveService` into hub/region scene flow so saved progress affects
   routing after reload.
4. Add one minimal PlayMode reproduction scene only when a real scene bug is
   found and the production scenes are too large to isolate it.

## Risk

This suite is intentionally small. It should run cheaply and catch broken core
contracts, but it is not a replacement for manual feel/readability QA or full
Unity validation before milestone evidence capture.

## 2026-06-27 Verification Pass

Implementation:

- Removed the duplicate `Assets/Scripts/FourfoldEchoes.Runtime.asmdef` assembly
  definition from the same folder as `FourfoldEchoes.Product.asmdef`.
- Updated `Assets/Tests/EditMode/FourfoldEchoes.EditModeTests.asmdef` to
  reference `FourfoldEchoes.Product`, matching the runtime namespace used by the
  exploration, combat, enemy, and production-slice scripts.
- Normalized the remaining asmdef `.meta` files with `AssemblyDefinitionImporter`
  blocks so Unity does not need to rewrite them on the next import.
- Added `Assets/Tests/EditMode/LocalSaveServiceTests.cs` for missing-save
  defaults, save/load roundtrip, corrupt JSON fallback, progress flag
  normalization, and settings clamping.
- Added `Assets/Scripts/Save/ProductionCombatSliceProgress.cs` plus
  `Assets/Tests/EditMode/ProductionCombatSliceProgressTests.cs` to keep the
  production slice's shortcut, boss, and reward save mapping out of the large
  `MonoBehaviour`.
- Updated `ProductionCombatSliceController` with public `ApplySavedProgress`,
  `WriteSavedProgress`, and `ConfigureSaveService` hooks. When `useLocalSave` is
  enabled, the slice loads saved progress on reset/start and writes changed
  shortcut, boss, or reward flags.
- Enabled `useLocalSave` by default in `ProductionCombatSliceController`, set it
  explicitly in `FourfoldProductionCombatSliceSceneBuilder`, and marked it on
  the current `Assets/Scenes/ProductionCombatSlice.unity` controller instance.
- `BeginRun`, `RetryRun`, and `ReturnToTitle` reset transient state before
  applying saved progress, so a saved completed reward can restore the completed
  slice state instead of being flattened into a fresh run.

Public API impact:

- `ProductionCombatSliceController` now exposes save hooks for tests and future
  scene-flow integration.
- `ProductionCombatSliceProgress` exposes public constants for the current
  production-slice save IDs.

Static checks:

- `rg --files Assets -g '*.asmdef'`
  - Result: only `FourfoldEchoes.Product`, `FourfoldEchoes.EditModeTests`, and
    `FourfoldEchoes.PlayModeTests` remain.
- `rg -n "FourfoldEchoes\\.Runtime|FourfoldEchoes\\.Product" Assets/Tests Assets/Scripts`
  - Result: no remaining `FourfoldEchoes.Runtime` references; test assemblies
    reference `FourfoldEchoes.Product`.
- `git diff --check`
  - Result: passed with no whitespace errors.
- ProductionCombatSliceProgress tests cover write/read cascade behavior and
  null-save fallback.

Unity test command attempted:

```sh
/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath <repo> \
  -runTests -testPlatform EditMode \
  -testResults <tmp>/fourfold-editmode-results.xml \
  -logFile -
```

Result:

- Failed before compilation/test execution because another Unity instance has
  this checkout open.
- Unity did not write `<tmp>/fourfold-editmode-results.xml`.

Follow-up verification:

- Close the open Unity editor instance or run tests through the active editor
  automation path, then rerun EditMode and PlayMode tests.
- Current follow-up status: `Temp/UnityLockfile` still exists, so Unity CLI tests
  were not rerun in this pass.

## 2026-06-27 Production Save-Restore Smoke

Purpose:

- Player-visible improvement: production-slice local progress can restore a
  claimed reward as a completed room state, so a returned player sees the
  shortcut/gate/reward state they already earned.
- Commercial readiness support: this closes one soft-lock risk from
  `docs/acceptance-criteria.md`, where a solved node, opened gate, defeated boss,
  or claimed reward must not reload into an impossible blocked state.

Systems touched:

- Production slice runtime save wiring.
- PlayMode scene smoke coverage.
- QA gap report only.

Files changed:

- `Assets/Scripts/ProductionCombatSliceController.cs`
- `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs`
- `reports/qa-gap-analysis.md`

Implementation:

- Added `ProductionCombatSliceController.SaveServiceFactory` as a narrow test
  seam. Default runtime behavior still uses `LocalSaveService.CreateDefault`.
- PlayMode smoke tests now override the save service factory with a temporary
  save path for every test, preventing tests from reading or overwriting the
  user's real local save.
- Added a production-slice PlayMode test that writes a saved claimed reward,
  loads `ProductionCombatSlice`, starts the run, and verifies the restored
  completed state, shortcut node, opened gate, reward flag, and reward pad.

Public API impact:

- `ProductionCombatSliceController.SaveServiceFactory` is a new public static
  property intended for test/runtime host injection of `LocalSaveService`.
  Existing scene behavior remains unchanged because its default value is
  `LocalSaveService.CreateDefault`.

Acceptance conditions:

- Existing production scene smoke still starts from a clean one-shot temporary
  save and verifies initial boss/gate/reward state.
- Saved reward smoke verifies `Completed`, `ShortcutOpen`, `BossUnlocked`,
  `GateOpen`, `RewardClaimed`, the solved node response target, and visible
  reward pad after restore.
- Validation output and PR notes must remain sanitized: no raw Unity logs, no
  token/secret/private URL, and no personal local path.

Veripsa status:

- Local Veripsa CLI is unavailable.
- Latest `artifacts/Reports/veripsa-current-split.md` says the Core source is
  GitHub Veripsa checks, with observed `SUCCESS` for a docs-only PR and
  `NEUTRAL` for a broad new Unity tool PR.
- No current Core `Pause` entry targets this small production save/test lane, so
  no ACK was issued.

Unity verification commands planned:

```sh
{UNITY_EDITOR} -batchmode -quit -projectPath {REPO_ROOT} -runTests -testPlatform PlayMode -logFile {SANITIZED_TMP_LOG}
```

If the project is already open in Unity, batchmode must wait until the active
editor releases the project lock.
