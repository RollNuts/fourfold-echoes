# Commercial Gap Map

Date: 2026-06-27

## Purpose

This map tracks the gap between the current FOURFOLD ECHOES checkout and the
commercial target: Steam-first, buy-to-own, single-player top-down classic
action-adventure with one hub, three handcrafted regions, four bosses, and one
exploration tool.

The worktree is currently dirty with staged runtime/UI/test/report work and
untracked workflow files. This report treats the current files on disk as
evidence, but it does not assume those changes are merged or accepted.

## Current Evidence Snapshot

| Area | Evidence | Status |
| --- | --- | --- |
| Canonical scope | `docs/Product/MVP_BLUEPRINT.md`, `docs/Product/PROJECT_SPEC.md`, `docs/Product/CORE_SYSTEMS.md`, `docs/Production/VERTICAL_SLICE_PLAN.md` | Strong |
| Active slice scene | `Assets/Scenes/ProductionCombatSlice.unity` | Present, EditMode and PlayMode assemblies pass in a temporary Unity project copy |
| Title/pause/retry UI | `Assets/Scripts/ProductionCombatSliceUi.cs`, `reports/ui-title-pause-retry.md` | Implemented with UI Toolkit and Legacy Input; PlayMode route passes |
| Title -> Game -> Pause -> Retry -> Clear proof | `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs`, `reports/commercial-progress-title-pause-clear-2026-06-27.md` | Deterministic PlayMode route passes and asserts saved reward flags |
| Combat slice controller | `Assets/Scripts/ProductionCombatSliceController.cs` | Present; narrow deterministic progression hooks added, still input-coupled for live play |
| Exploration tool | `Assets/Scripts/ExplorationTool.cs`, `Assets/Scripts/ExplorationNode.cs` | Present, basic test coverage exists |
| Enemy framework | `Assets/Scripts/Enemies/*`, `Assets/Scripts/Combat/Damageable.cs`, `reports/ai-EnemyController.md` | Present, integration maturity unclear |
| Local save | `Assets/Scripts/Save/*`, `Assets/Tests/EditMode/LocalSaveServiceTests.cs`, `Assets/Tests/EditMode/ProductionCombatSliceProgressTests.cs`, `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs` | Present, service and slice flag conversion have focused coverage; live route and scene reload restore assertions pass |
| Build target | `Assets/Editor/FourfoldUnityBuild.cs`, `.github/workflows/build.yml`, `reports/commercial-mvp-progress-2026-06-27.md` | Windows-first intent present, build artifact missing |
| QA automation | `Assets/Tests/EditMode/*`, `Assets/Tests/PlayMode/*`, `reports/qa-gap-analysis.md` | EditMode and PlayMode test assemblies pass in serialized Unity batchmode against a temporary project copy |
| Art/audio evidence | Production assets and generated audio exist under `Assets/Art` / `Assets/Audio` | Quality/readiness not proven |
| Asset library reuse | `unity-game-asset-library` reference repository and current candidate art/audio/model packs in the worktree | Candidate assets are visible, but adoption still needs read-only provenance, readiness, and gameplay-bind classification |
| Steam/release docs | `docs/QA/STEAM_RELEASE_PLAN.md`, `docs/Marketing/STEAM_STORE_PLAN.md`, `docs/Release/STEAM_READINESS.md` | Planning present, checklist not green |

## Commercial Requirements

| Requirement | Current State | Gap |
| --- | --- | --- |
| Windows / Steam Deck-oriented build passes | Windows-first build defaults appear in current worktree | No current Windows artifact or Unity build result |
| 20-30 minute vertical slice is playable | ProductionCombatSlice exists; D-020 proof exists | Not yet 20-30 minutes, lacks hub/region arc proof |
| Title -> Game -> Pause -> Retry -> Clear works | Runtime UI/controller states exist; deterministic PlayMode smoke route covers state transitions and saved reward flags | Passed in Unity PlayMode; controller input remains static Legacy Input |
| One exploration tool is the product hook | Tool/node exists and opens a shortcut | Needs two-room mastery proof and trailer-readable presentation |
| Combat, boss, room, shortcut, reward loop works | Combat slice has enemies, boss, gate, reward; deterministic completion hooks and PlayMode route pass | Manual controller-device proof still pending |
| Local save/load viable | Save service, flags, production slice progress conversion tests, live route save assertions, and scene reload restore proof pass | Full app restart/save-load proof still pending |
| No placeholder art/audio in claimed validation footage | Production assets exist | Market-readiness and audio replacement not proven |
| NullReference / missing reference blockers absent | Static reports found no obvious missing script/prefab pattern; EditMode and PlayMode assemblies pass | Dedicated scene-builder validation still pending |
| Steam demo/store/release checklist green or risk-managed | Release docs exist | No final readiness report and no current store/demo gate result |

## Risk-Ordered Backlog

1. **Windows build smoke**: produce a current Windows artifact or record why the editor module is unavailable.
2. **Fresh app-start save proof**: verify the same shortcut, boss, and reward flags after a fresh application start.
3. **Controller-first input proof**: current Legacy Input path exists, but controller completion through UI/combat/tool/reward is not proven by device input.
4. **Production scene-builder validation**: run the dedicated slice verifier when available and record exit code/results.
5. **Hub/region structure**: canonical product requires one hub and three regions; current evidence is still slice-scene centered.
6. **Second exploration-tool room**: the one-tool mastery thesis needs at least two different room uses before adding systems.
7. **Art/audio readiness**: identify every placeholder and decide which assets/audio are accepted for market-validation capture.
8. **Steam evidence**: produce Windows build artifact, Steam Deck-oriented UI/readability pass, screenshot/trailer beat list, and final release risk table.

## Next Smallest Useful Task

Add a fresh app-start save proof for `ProductionCombatSlice`, then run a
Windows build smoke.
