# Veripsa Lane Plan

Status: prepared for Veripsa Core review with live GitHub signal checked on
2026-06-26 JST.

## Current Core Access

No local `veripsa` CLI was found in this environment, and the previously known
`aicompany` PostgreSQL database is not present. GitHub PR checks are visible,
so this document combines local lane planning with the limited live Core signal
available from open PRs.

When Veripsa Core is available, submit these lanes separately and use Core's
file-overlap and land-order verdict as the coordination source.

## Live Core Observations

- PR #17, `codex/store-readiness-pack`, has Veripsa `SUCCESS`. It is docs-only:
  `README.md` and `docs/store/*`.
- PR #14, `codex/gate-a-evidence-harness`, has Veripsa `NEUTRAL`. The Core
  comment says the PR reserves these paths, but they were not analyzed because
  they are not in main's graph yet:
  `Assets/Editor/FourfoldGateAEvidenceReport.cs`,
  `Assets/Editor/FourfoldGateAEvidenceReport.cs.meta`, and
  `tools/unity_gate_a_evidence.sh`.
- Treat that exact pattern as the expected behavior for new Unity editor tools,
  scripts, and generated scene files: `UNKNOWN` means "unverified", not "safe".
- New D-020 runtime files such as `Assets/Scripts/ExplorationTool.cs` and
  `Assets/Scripts/ExplorationNode.cs` may similarly produce `UNKNOWN` until the
  graph indexes them. Treat that as coordination uncertainty, not correctness.

## Current Open PR Stack

Current open PRs are intentionally stacked from guardrails to Unity evidence:

| Land Order | PR | Branch | Scope | Status |
| ---: | --- | --- | --- | --- |
| 1 | #40 | `codex/public-required-checks` | public hygiene, required-check docs, validation runners | draft, clean |
| 2 | #42 | `codex/d020-objective-guardrails` | D-020 objective guardrails, output section validation, hard-exclude checks | draft, clean |
| 3 | #41 | `codex/d020-readable-room-pass` | D-020 Unity scene readability, editor command flow, capture evidence | draft, clean |

Do not merge #41 before #42. Do not merge #42 before #40. If Core reports
`Pause`, read the overlapping files and confirm whether the overlap is caused by
the intentional stacked base or an actual semantic conflict.

## Next Lane After Current Stack

After #40, #42, and #41 land, the next player-visible lane should be
`production-art-p0`.

| Lane | Purpose | Primary Files | Required Evidence |
| --- | --- | --- | --- |
| `production-art-p0` | replace generated read-target silhouettes with production-intent hero, exploration tool, first enemy, gimmick pedestal, and reward chest/relic | `docs/Art/NEXT_ASSET_BATCH.md`, `Assets/Art/Production/P0/*`, `Assets/Editor/*Art*`, `artifacts/Previews/*` | Unity import validation, gameplay-camera screenshots, no missing materials, no local paths in PR |

This lane must work on the biggest on-screen reads first. It must not spend a
PR on decorative props, region quantity, or optional pipeline polish before the
hero/tool/enemy/reward reads improve.

## Recommended PR Split

| PR | Lane | Purpose | Primary Files | Land Order |
| --- | --- | --- | --- | --- |
| PR-A | `product-canon` | D-020 spec reset and source-of-truth docs | `README.md`, `AGENTS.md`, `docs/DECISIONS.md`, `docs/Product/*`, `game-spec/project.yaml`, `game-spec/scenes/d020-vertical-slice.yaml`, `game-spec/entities/d020-vertical-slice.yaml`, `game-spec/scenarios/d020-tool-room-read.yaml` | 1 |
| PR-B | `art-audio-direction` | art/audio budgets, naming, cue registers, quality gates | `docs/Art/*`, `docs/Audio/*`, `docs/ASSET_RIGHTS.md`, `tools/AudioPipeline/*`, `Assets/Audio/Generated/*` | 2 |
| PR-C | `production-release-plans` | vertical slice, scope control, QA, store/release/tech plan | `docs/Production/*`, `docs/QA/*`, `docs/Marketing/*`, `docs/Release/*`, `docs/Legal/*`, `docs/Tech/*` | 3 |
| PR-D | `validation-sync` | update static checks and Core split reports to reject old scope | `Scripts/Validation/validate_repo.mjs`, `Scripts/Validation/write_veripsa_split_report.mjs`, `tools/forge/check.mjs`, `commands/samples/inspect-d020-slice.json`, `artifacts/Reports/veripsa-current-split.*`, `artifacts/.gitignore` | 4 |
| PR-E1 | `d020-tool-runtime` | smallest runtime contract for the one exploration tool | `Assets/Scripts/ExplorationTool.cs`, `Assets/Scripts/ExplorationTool.cs.meta`, `Assets/Scripts/ExplorationNode.cs`, `Assets/Scripts/ExplorationNode.cs.meta` | after A-D |
| PR-E2 | `d020-scene-evidence` | D-020 generated scene and editor builder only | `Assets/Editor/FourfoldD020SliceSceneBuilder.cs`, `Assets/Editor/FourfoldD020SliceSceneBuilder.cs.meta`, `Assets/Scenes/D020VerticalSlice.unity`, `Assets/Scenes/D020VerticalSlice.unity.meta`, `Assets/Art/Generated/D020/*` | after E1 |
| PR-E3 | `d020-capture-build` | capture/build/validation wiring for the D-020 slice | `Assets/Editor/FourfoldUnityEvidenceCapture.cs`, `Assets/Editor/FourfoldUnityBuild.cs`, `Scripts/Build/build_current.sh`, `Scripts/Validation/run_all.sh`, `Scripts/Validation/write_market_reports.mjs`, `tools/unity_capture_d020_slice.sh`, `artifacts/Previews/d020-*.png`, `artifacts/Reports/*` | after E2 |
| PR-F | `historical-proof-cleanup` | remove or archive legacy product-review proof outside active production scope | `Assets/Scripts/FourfoldProductReviewController.cs`, `Assets/Scripts/EchoPhaseState.cs`, `Assets/Editor/FourfoldProductReviewSceneBuilder.cs`, `Assets/Editor/FourfoldProductValidator.cs`, `Assets/Scenes/ProductReviewSandbox.unity`, `Assets/Art/Generated/{Materials,Meshes,Prefabs,Textures}/*ProductReview*`, `artifacts/Previews/*product-review*`, `tools/unity_capture_product_review.sh` | after D-020 proof lands |
| PR-G | `forge-mediator-sync` | keep Forge command entry points aligned with current D-020 spec | `Assets/Editor/Mediator/*`, `tools/unity_forge_command.sh`, `ProjectSettings/PackageManagerSettings.asset` | after D |
| PR-H | `asset-pipeline-pilot-optional` | land reusable Blender/asset pipeline tooling only if it is still useful after D-020 reset | `tools/Blender/*`, `tools/AssetPipeline/*`, `Assets/Art/Generated/BlenderPilot/*`, `artifacts/Previews/BlenderPilot/*` | optional; do not block D-020 |

## Hotspots

- `AGENTS.md` is a high-impact source-of-truth file. Only PR-A should edit it.
- `README.md` is a high-impact entry point. Only PR-A should change canonical
  product direction there.
- `docs/Product/*` should not be edited in art/audio or QA PRs unless Core says
  the dependency is safe.
- `game-spec/*d020*` belongs with PR-A because it tells Forge and future agents
  what scene is current. Historical ProductReview Game IR should not remain in
  the active Game IR tree.
- Static validation scripts should land after canonical docs, otherwise reports
  may lock old wording. Unity execution/report scripts stay in PR-E3.
- `Scripts/Validation/validate_repo.mjs` now owns the D-020 stale-scope guard.
  It should reject current-scope reintroductions of old open-world/Echo Phase
  language in entry docs, Tech architecture, Game IR, Forge checks, and market
  report generation.
- `docs/Audio/ASSET_REGISTER.csv` and `docs/Audio/SFX_REGISTER.csv` belong to
  PR-B. `Scripts/Validation/validate_repo.mjs` belongs to PR-D and only enforces
  that the D-020 required cue rows remain registered. Do not mix production WAV
  generation into either lane.
- `Assets/*` should be kept out of the first spec PRs unless runtime work starts.
- `Assets/Scripts/ExplorationTool.cs` and `Assets/Scripts/ExplorationNode.cs`
  are PR-E1 runtime proof files. Do not mix them into docs-only PRs or scene
  generation PRs.
- `Assets/Editor/FourfoldD020SliceSceneBuilder.cs` is PR-E2. It depends on the
  runtime contract but should not also change validation reports or build
  scripts.
- `Assets/Editor/FourfoldUnityEvidenceCapture.cs`,
  `Assets/Editor/FourfoldUnityBuild.cs`, `Scripts/Build/build_current.sh`,
  `Scripts/Validation/run_all.sh`, `Scripts/Validation/write_market_reports.mjs`,
  and `tools/unity_capture_d020_slice.sh` are PR-E3. They should land after the
  scene exists so Core can reason about the smaller reservation.
- `ProductReviewSandbox` and `EchoPhaseState.cs` are outside active D-020
  production. If they remain at all, they must be archived or removed in PR-F,
  not polished as product evidence.
- `BlenderPilot` files and generated ProductReview art are not D-020 product
  assets. Land BlenderPilot only as optional tooling evidence or remove it in a
  dedicated cleanup lane; do not let it define the current art direction.

## Current Local Split Recommendation

Keep the local worktree clean between PRs. Rejected historical prototypes should
stay out of the active branch unless a dedicated cleanup PR intentionally
removes tracked files.

1. Land #40, #42, then #41 in that order.
2. Rebase or merge later branches after each lower-order PR lands.
3. Keep stale ProductReview, Echo Phase, BlenderPilot, or open-world artifacts
   out of D-020 product PRs.
4. Start `production-art-p0` only after the current stack lands or Core confirms
   it can reserve non-overlapping art paths safely.
5. If local experiments are needed, keep them ignored or backed up outside the
   public PR path until their license, purpose, and quality gate are clear.

## Core Questions To Ask

1. Are PR-A and PR-B independent enough to review in parallel?
2. Should `docs/QA/STEAM_RELEASE_PLAN.md` land with production planning or
   release readiness?
3. Does `game-spec/project.yaml` belong in PR-A or a later Game IR sync PR?
4. Which open PRs or branches touch `AGENTS.md`, `docs/Product/*`, or validation
   scripts?

## ACK Rule

Do not ACK a Core Pause blindly. Read the overlapping files, confirm whether the
new D-020 direction or older open-world/Echo Phase direction is authoritative,
then record the reason in the PR.
