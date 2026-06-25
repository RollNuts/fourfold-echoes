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

## Recommended PR Split

| PR | Lane | Purpose | Primary Files | Land Order |
| --- | --- | --- | --- | --- |
| PR-A | `product-canon` | D-020 spec reset and source-of-truth docs | `README.md`, `AGENTS.md`, `docs/DECISIONS.md`, `docs/Product/*`, `game-spec/project.yaml`, `game-spec/scenes/d020-vertical-slice.yaml`, `game-spec/entities/d020-vertical-slice.yaml`, `game-spec/scenarios/d020-tool-room-read.yaml` | 1 |
| PR-B | `art-audio-direction` | art/audio budgets, naming, cue registers, quality gates | `docs/Art/*`, `docs/Audio/*`, `docs/ASSET_RIGHTS.md`, `tools/AudioPipeline/*`, `Assets/Audio/Generated/*` | 2 |
| PR-C | `production-release-plans` | vertical slice, scope control, QA, store/release/tech plan | `docs/Production/*`, `docs/QA/*`, `docs/Marketing/*`, `docs/Release/*`, `docs/Legal/*`, `docs/Tech/*` | 3 |
| PR-D | `validation-sync` | update generated reports and checks to reject old scope | `Scripts/Validation/*`, `tools/forge/check.mjs`, `commands/samples/inspect-d020-slice.json`, `artifacts/Reports/*`, `.gitignore`, `artifacts/.gitignore` | 4 |
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
- `Scripts/Validation/*` should land after canonical docs, otherwise generated
  reports may lock old wording.
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

The current worktree is intentionally dirty because it contains the D-020 reset
plus generated Unity evidence. If branches/PRs are created from this state,
split by semantic ownership, not by convenience:

1. Land `product-canon` first so every later PR reads the same source of truth.
2. Land `art-audio-direction` and `production-release-plans` only after PR-A is
   reviewed, because those docs interpret the canon.
3. Land `validation-sync` after canonical docs so reports do not preserve old
   Echo Phase/open-world wording.
4. Land `d020-tool-runtime` as the smallest code PR. Expect Veripsa `UNKNOWN`
   for new C# paths until they enter main's graph.
5. Land `d020-scene-evidence` after E1. This reserves generated scene/material
   paths separately from runtime code.
6. Land `d020-capture-build` after E2. This is where screenshot/build/report
   automation belongs.
7. Remove or archive `ProductReviewSandbox` and its multi-state support files in
   a dedicated cleanup lane. Do not mix that cleanup into the canonical D-020
   runtime PR.
8. Decide separately whether `asset-pipeline-pilot-optional` still earns its
   keep. If not, leave those generated pilot assets out of D-020 entirely.

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
