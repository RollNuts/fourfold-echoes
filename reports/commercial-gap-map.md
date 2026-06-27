# Commercial Gap Map

Date: 2026-06-27

## Purpose

This map tracks the gap between the current FOURFOLD ECHOES checkout and the
commercial target: Steam-first, buy-to-own, single-player top-down classic
action-adventure with one hub, three handcrafted regions, four bosses, and one
exploration tool.

The worktree is currently dirty with many untracked runtime, test, CI, and
report files. This report treats the current files on disk as evidence, but it
does not assume those changes are staged or accepted.

## Current Evidence Snapshot

| Area | Evidence | Status |
| --- | --- | --- |
| Canonical scope | `docs/Product/MVP_BLUEPRINT.md`, `docs/Product/PROJECT_SPEC.md`, `docs/Product/CORE_SYSTEMS.md`, `docs/Production/VERTICAL_SLICE_PLAN.md` | Strong |
| Active slice scene | `Assets/Scenes/ProductionCombatSlice.unity` | Present, runtime validation pending |
| Title/pause/retry UI | `Assets/Scripts/ProductionCombatSliceUi.cs`, `reports/ui-title-pause-retry.md` | Implemented, Play Mode validation pending |
| Combat slice controller | `Assets/Scripts/ProductionCombatSliceController.cs` | Present, still broad and input-coupled |
| Exploration tool | `Assets/Scripts/ExplorationTool.cs`, `Assets/Scripts/ExplorationNode.cs` | Present, basic test coverage exists |
| Enemy framework | `Assets/Scripts/Enemies/*`, `Assets/Scripts/Combat/Damageable.cs`, `reports/ai-EnemyController.md` | Present, integration maturity unclear |
| Local save | `Assets/Scripts/Save/*`, `Assets/Tests/EditMode/LocalSaveServiceTests.cs`, `Assets/Tests/EditMode/ProductionCombatSliceProgressTests.cs` | Present, service and slice flag conversion have focused coverage; Unity execution pending |
| Build target | `Assets/Editor/FourfoldUnityBuild.cs`, `.github/workflows/build.yml`, `reports/commercial-mvp-progress-2026-06-27.md` | Windows-first intent present, build artifact missing |
| QA automation | `Assets/Tests/EditMode/*`, `Assets/Tests/PlayMode/*`, `reports/qa-gap-analysis.md` | Test surface present, Unity execution blocked by open editor |
| Art/audio evidence | Production assets and generated audio exist under `Assets/Art` / `Assets/Audio` | Quality/readiness not proven |
| Steam/release docs | `docs/QA/STEAM_RELEASE_PLAN.md`, `docs/Marketing/STEAM_STORE_PLAN.md`, `docs/Release/STEAM_READINESS.md` | Planning present, checklist not green |

## Commercial Requirements

| Requirement | Current State | Gap |
| --- | --- | --- |
| Windows / Steam Deck-oriented build passes | Windows-first build defaults appear in current worktree | No current Windows artifact or Unity build result |
| 20-30 minute vertical slice is playable | ProductionCombatSlice exists; D-020 proof exists | Not yet 20-30 minutes, lacks hub/region arc proof |
| Title -> Game -> Pause -> Retry -> Clear works | Runtime UI and controller states exist | Not verified in Play Mode; controller input is static Legacy Input |
| One exploration tool is the product hook | Tool/node exists and opens a shortcut | Needs two-room mastery proof and trailer-readable presentation |
| Combat, boss, room, shortcut, reward loop works | Combat slice has enemies, boss, gate, reward | Full progression path automation missing |
| Local save/load viable | Save service, flags, and production slice progress conversion tests exist | Live scene save/load proof still pending |
| No placeholder art/audio in claimed validation footage | Production assets exist | Market-readiness and audio replacement not proven |
| NullReference / missing reference blockers absent | Static reports found no obvious missing script/prefab pattern | Unity Play Mode and builder validation blocked |
| Steam demo/store/release checklist green or risk-managed | Release docs exist | No final readiness report and no current store/demo gate result |

## Risk-Ordered Backlog

1. **Unverified Unity execution**: close or use the active Unity editor, then run EditMode, PlayMode, production slice validation, and a Windows build smoke.
2. **Live save integration proof**: verify that saved shortcut, boss, and reward flags affect `ProductionCombatSlice` after reload.
3. **Full slice progression proof**: expose deterministic controller hooks or a small domain layer so tests can complete enemy defeat -> shortcut -> boss -> reward without fake input.
4. **Controller-first input proof**: current Legacy Input path exists, but controller completion through UI/combat/tool/reward is not proven.
5. **Hub/region structure**: canonical product requires one hub and three regions; current evidence is still slice-scene centered.
6. **Second exploration-tool room**: the one-tool mastery thesis needs at least two different room uses before adding systems.
7. **Art/audio readiness**: identify every placeholder and decide which assets/audio are accepted for market-validation capture.
8. **Steam evidence**: produce Windows build artifact, Steam Deck-oriented UI/readability pass, screenshot/trailer beat list, and final release risk table.

## Next Smallest Useful Task

Run Unity EditMode and PlayMode tests through the active editor or after closing
the open editor. If Unity remains blocked, add deterministic public hooks around
`ProductionCombatSliceController` progression so the full room loop can be
tested without fake Legacy Input.
