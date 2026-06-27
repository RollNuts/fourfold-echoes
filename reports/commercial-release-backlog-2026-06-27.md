# Commercial Release Backlog

Date: 2026-06-27

## Purpose

This gap map defines the path from the current FOURFOLD ECHOES repository state
to a Steam-first, buy-to-own, single-player top-down classic action-adventure
release candidate.

It uses the current repository, canonical docs, Unity scenes, Prefabs, tests,
open GitHub PRs, and local Veripsa-lane evidence as inputs. It intentionally
keeps the MVP ceiling intact:

- 1 hub
- 3 regions
- 4 bosses
- 1 exploration tool
- no inventory
- no crafting
- no quest log
- no open world
- no social, online, live-service, gacha, farming, fishing, or base-building
  systems

## Current Evidence Snapshot

| Area | Current Evidence | Commercial Read |
| --- | --- | --- |
| Product canon | `docs/Product/MVP_BLUEPRINT.md`, `docs/Product/PROJECT_SPEC.md`, `docs/Product/CORE_SYSTEMS.md`, `docs/Product/SCOPE_BOUNDARIES.md` | Strong direction: compact premium top-down action-adventure, one tool, strict MVP cap. |
| Release and store docs | `docs/Release/STEAM_READINESS.md`, `docs/QA/STEAM_RELEASE_PLAN.md`, `docs/Marketing/STEAM_STORE_PLAN.md` | Planning exists, but gates are not green. |
| Scenes | `Assets/Scenes/D020VerticalSlice.unity`, `Assets/Scenes/ProductionCombatSlice.unity`, `Assets/Scenes/AshenThresholdSpike.unity`, VFX preview scene | Production slice exists, D-020 proof exists, but final hub/3-region/4-boss structure is not fully proven. |
| Production Prefabs | `Assets/Prefabs/Production/P0`, `P1`, `P2`, `P3` | Large production snapshot exists; gameplay readiness and LOD/material acceptance need gates. |
| Tests | `Assets/Tests/EditMode/*`, `Assets/Tests/PlayMode/*` | Useful EditMode/PlayMode coverage exists; current worktree has staged/dirty test changes from other lanes. |
| Build/validation | `Assets/Editor/FourfoldProductValidator.cs`, `FourfoldUnityBuild.cs`, validation scripts | Validation infrastructure exists; Windows/Steam launch artifact is not current evidence. |
| PR state | #44, #45, #46 ready; #43 and older D-020 stack mostly draft | Ready PRs can land in order; large product-loop PR should remain draft or be split. |
| Veripsa state | No local `veripsa` CLI found; local PostgreSQL access blocked; existing split report is stale | Use GitHubApp PR routing and lane docs. Do not blind ACK Veripsa pauses. |
| Dirty worktree | Current checkout has mixed staged/unstaged C#, tests, reports, art, VFX, audio, and tooling work | Treat as other-agent work. Do not stage broadly. |

## Commercial Gaps

| Requirement | Current State | Gap |
| --- | --- | --- |
| Steam Windows build launches cleanly | Build tooling exists; PR #43 reports macOS product-loop smoke and Windows module missing | No current Windows clean-launch artifact. |
| Steam Deck-oriented readability | QA plan exists and UI safe-area checks are mentioned in PR #43 | No measured Steam Deck or 1280x800 player-window evidence in current accepted stack. |
| 20-30 minute vertical slice | D-020 and ProductionCombatSlice evidence exist | Not yet a cohesive 20-30 minute commercial slice. |
| Hub + 3 regions + 4 bosses | Canonical docs define it | Production content proof is slice-centered; full structure not built. |
| One exploration tool mastery | Tool/node exists; D-020 stack has two-room proof in older draft PRs | Needs accepted production path and market-readable presentation. |
| Combat + enemy baseline | PR #46 adds EnemyController AI baseline | Needs Veripsa routing and integration into production slice without mixing lanes. |
| Save/load and restart proof | Tests and reports indicate local save progress work in dirty lanes and PR #43 | Fresh app-start save proof is not an accepted ready PR yet. |
| Art/audio ready for store captures | Production asset snapshot and generated audio/VFX exist | Acceptance, licensing, LOD/material budgets, and no-placeholder gate remain open. |
| Required checks | PR #40 defines public required checks but is draft and workflow scope is constrained | Enforcement and workflow landing remain unresolved. |
| PR traffic | Many open stacked PRs exist | Need land order discipline to avoid merging large or stale stacks into main. |

## Commercial Release Backlog

### P0 - Release Blockers

| ID | Task | Why It Blocks Commercial Progress | Ready PR Shape |
| --- | --- | --- | --- |
| P0-01 | Land the current ready production slice routing stack: #44 then #45 | #45 depends on the title-guidance stack from #44; landing them clarifies the current production slice evidence base. | One PR per existing ready branch; no new files. |
| P0-02 | Route #46 EnemyController AI baseline through Veripsa/GitHubApp | Normal enemy behavior is core to commercial play feel; it must not be mixed with scene/UI/art work. | Existing ready PR, review as runtime/test lane. |
| P0-03 | Split or refresh #43 product-loop PR | #43 carries major release proof but is too large and still draft; it should become either validated ready or smaller PRs. | First split should target one accepted release proof, not the whole loop. |
| P0-04 | Produce a current Windows clean-launch smoke result | Steam-first means Windows first; macOS evidence is not enough. | Build/QA PR with artifact summary, no raw logs. |
| P0-05 | Add fresh app-start save proof for ProductionCombatSlice | Save persistence and restart reliability are commercial blockers. | Focused test/report PR proving shortcut/boss/reward flags survive restart. |
| P0-06 | Establish store-capture no-placeholder gate | Store images/trailer cannot use gray-box or placeholder audio while claiming market validation. | Report plus validator/check update; no new asset pack import. |
| P0-07 | Resolve required-check workflow lane (#40 or successor) | Release branches need stable validation gates even if branch protection is not enforceable yet. | Dedicated workflow/validation PR; separate from gameplay. |

### P1 - Vertical Slice Completion

| ID | Task | Commercial Value | Ready PR Shape |
| --- | --- | --- | --- |
| P1-01 | Build accepted hub-to-region slice path | Proves the customer-facing loop starts from the intended hub instead of isolated test rooms. | Scene/UI flow PR with Unity verifier. |
| P1-02 | Confirm two-room one-tool mastery in the production path | Proves depth comes from repeated use of the one exploration tool. | Scene/test PR; no second tool or inventory. |
| P1-03 | Integrate two normal enemy reads into ProductionCombatSlice | Makes combat variety visible without expanding scope. | Scene/runtime binding PR after #46 lands. |
| P1-04 | Add miniboss and boss readability proof | Four-boss product promise needs a first accepted boss-pattern proof. | One boss/miniboss lane at a time, with telegraph validation. |
| P1-05 | Replace or accept hero/tool/enemy/reward production art | Prevents market captures from reading as placeholder or anonymous assets. | Asset-preview first, gameplay binding only after import policy passes. |
| P1-06 | Accept minimum SFX/BGM cue set | Audio is part of feel, not late decoration. | Audio register + scene cue validation PR. |
| P1-07 | Add controller-first manual QA evidence | Steam players need controller-safe menus and gameplay. | QA report with exact build/version evidence, no raw logs. |
| P1-08 | Add UI player-window screenshot harness | Current editor capture may miss UI overlays; store/readability proof needs UI included. | QA/tooling PR, separate from UI design changes. |

### P2 - Release Polish And Market Readiness

| ID | Task | Commercial Value | Ready PR Shape |
| --- | --- | --- | --- |
| P2-01 | Steam screenshot manifest with accepted captures | Turns the store plan into concrete production evidence. | Marketing report PR after no-placeholder gate passes. |
| P2-02 | 45-second trailer source-capture plan | Prevents trailer from promising unimplemented systems. | Store/trailer report PR with exact implemented beats. |
| P2-03 | Steam Deck measurement pass | Confirms handheld readability and controller expectations. | QA report after Windows build exists. |
| P2-04 | License and asset-rights audit | Prevents shipping blocked or unclear assets/audio. | Docs/legal register PR, no asset import. |
| P2-05 | Performance and asset-budget report | Prevents late memory/material/VFX collapse. | Technical-art report plus validator checks. |
| P2-06 | Support, known-issues, and crash-report docs | Release operations need a stable support path. | Release docs PR. |
| P2-07 | Localization extraction and glossary pass | Store/UI text needs future translation safety. | Text/glossary report, no new UI system. |

## Recommended PR Land Order

1. #44 `production slice title guidance`
2. #45 `ProductionCombatSlice scene validation report`
3. #46 `EnemyController AI baseline`
4. #43 split decision: refresh as ready only if still one coherent PR; otherwise
   split the smallest release proof from it.
5. P0-04 Windows clean-launch smoke
6. P0-05 fresh app-start save proof
7. P0-06 store-capture no-placeholder gate
8. P0-07 required-check workflow lane

Older D-020 draft PRs (#27-#36 and #40-#42) should stay parked unless the team
explicitly chooses the D-020 prototype stack as the active landing path again.

## Veripsa Operating Rule

Use Veripsa as traffic control, not as a substitute for reading the files.

- Local Veripsa CLI was not available in this pass.
- Local PostgreSQL-backed Veripsa access was blocked in this environment.
- Existing `artifacts/Reports/veripsa-current-split.md` is stale relative to
  the current dirty worktree.
- Do not ACK a Pause without reading the pause reason and the overlapping files.
- Treat new Unity C# and generated scene paths as coordination-unknown until
  indexed.

## Dirty Worktree Classification

Current dirty work is mixed and must not be committed as one unit:

| Lane | Examples | Handling |
| --- | --- | --- |
| Runtime/UI/tests | `Assets/Scripts/ProductionCombat*`, `Assets/Tests/*` | Other-agent work; split only with explicit scope. |
| Editor validation | `Assets/Editor/Fourfold*` | Requires Unity verifier evidence before ready PR. |
| Reports | `reports/commercial-gap-map.md`, progress/status reports | Do not overwrite; create narrow reports when needed. |
| Art/VFX/audio | generated model rig, VFX, UI icon, SFX folders | Asset-preview lane only until license/import/readiness is clear. |
| Workflow/tooling | `.github`, `tools/*`, task matrices | Separate validation/tooling lane; workflow scope may block push. |

## Next P0 PR Unit

After this backlog report, the next smallest useful PR is P0-04:

Produce a current Windows clean-launch smoke result, or record the exact Unity
module blocker in a sanitized release-risk report if the Windows build module is
not installed.
