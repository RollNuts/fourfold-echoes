# Repo Timeline Audit

Audit date: 2026-06-25
Repo: `RollNuts/fourfold-echoes` private repository
Base commit inspected: `e3d12e41952fc25752e6e5dfe4a226032a8a0f79`

## Current State Summary

Confirmed: The repo is a Unity project targeting Unity `6000.3.18f1`.
Confirmed: The active branch at audit start was `main`, clean after reclone.
Confirmed at audit start: The committed runtime path pointed at a generated Gate A prototype, not a production slice.
Confirmed after D-020 branch changes: default Game IR now points at
`scene.d020_vertical_slice`; normal validation generates D-020 evidence instead
of treating Gate A or ProductReviewSandbox as current product content.
Confirmed: No third-party production art, texture, animation, audio, font, or model assets are committed.
Confirmed: Package dependencies are Unity built-in modules only: AI, animation, audio, IMGUI, physics, UIElements.
Confirmed: Render pipeline is Built-in; `ProjectSettings/GraphicsSettings.asset` and all quality levels have `customRenderPipeline: 0`.
Confirmed: There is no committed networking, transport, lobby, relay, account, server, matchmaking, or custom backend code found by repo search.
Confirmed: Before D-020, co-op appeared only in product wording, not as implemented runtime architecture.

## Timeline Evidence

| Evidence | Period / Commit | What It Was Trying To Prove | Classification |
| --- | --- | --- | --- |
| `1521f78 Initialize FOURFOLD ECHOES Unity project` | Initial Unity seed | Establish a private commercial Unity repo. | canonical infrastructure |
| `5c89b4e Add Unity Gate A validation harness` | Gate A start | Generate and validate one playable room from editor code. | prototype |
| `5dbb589 Document Unity MCP security gate (#3)` | Automation security | Keep MCP/editor automation constrained and documented. | canonical infrastructure |
| `a8dd4c3 Seed Veripsa Forge architecture` and Forge docs | GitOps / Forge experiment | Explore stable file contracts for AI-to-Unity generation. | prototype infrastructure, partially reusable |
| `488093b Stabilize Unity project metadata` | Unity hygiene | Commit project metadata and make Unity repo behavior predictable. | canonical infrastructure |
| `46dcedc Polish Gate A visual scene generation` | Gate A presentation | Improve generated primitive room readability. | prototype |
| `0b937b0 Clean Unity Gate A capture warnings` | Evidence tooling | Reduce warnings around capture path. | prototype infrastructure |
| `ec6ddff Add Gate A playable build tooling` | Build gate | Build the generated prototype as macOS/Windows standalone target. | canonical infrastructure if generalized |
| `a468234 Improve Gate A combat feel` | Prototype feel | Movement, attack, dodge, enemy timing, feedback. | prototype |
| `51d2c27 Add procedural Gate A audio proof` and `e142511 Add Gate A procedural ambient bed` | Audio proof | Runtime synthesized placeholder cues and ambience. | prototype; rights discipline is canonical |
| `f47f3da Simplify Gate A arena readability (#15)` | Readability pass | Camera/arena clarity around Gate A. | prototype |
| `e3d12e4 Improve Gate A action feel (#16)` | Latest main | Additional action-feel polish on old Gate A. | prototype |
| Branch `origin/codex/store-readiness-pack` | Store thinking | Early store-facing documentation. | prototype; may inform but not authoritative |
| Branch `origin/codex/gate-a-evidence-harness` | Evidence harness | Report/capture automation around Gate A. | prototype infrastructure, worth mining |

## Co-op / Multiplayer / Online Trace

Previously confirmed before the product reset: README said "solo-first co-op-ready structure."
Confirmed: No runtime code implements multiplayer, lobby, relay, transport, online sync, or server.
Confirmed: `ProjectSettings/MultiplayerManager.asset` exists as Unity project settings, but no multiplayer package is present in `Packages/manifest.json`.
Assumed: Co-op was an early aspiration, not an implemented product pillar.
Decision: v1 canonical scope removes co-op/multiplayer. Treat it as historical
only; do not keep it as a post-v1 promise or roadmap candidate.

## Canonical / Prototype / Discard

### Canonical

- Private commercial repo separation from public `veripsa-games`.
- Unity 6.3 LTS as current engine.
- Built-in render pipeline as current project reality until a tested migration proves value.
- Commercial-use-only asset discipline.
- CLI/build/capture as required verification paths.
- Local save / no custom backend / no always-online requirement.

### Prototype

- Legacy Gate A generated room and scripts, only as explicit harness evidence.
- Ember altar and claim gate flow.
- Primitive block diorama visuals.
- Procedural runtime tones.
- Forge file contracts and Unity mediator, until product relevance is revalidated.
- Fixed-angle camera, unless current product tests prove controlled camera is better.

### Discard

- Treating Gate A as the product core.
- Co-op-ready as v1 requirement.
- Environmental puzzle pressure as the main selling hook.
- Hack-and-slash/loot carry-home as the primary genre promise.
- Altar/gate room structure as first-screen product identity.
- Any "minimum viable" visual direction that cannot carry Steam screenshots.

## Final Repo Interpretation

Confirmed: The repository currently contains useful Unity automation and legacy prototype harness code, but it does not yet contain the canonical sellable game.
Assumed: The next highest-leverage work is not more Gate A/ProductReview polish,
but product reset toward a compact, single-player, premium-indie top-down
classic action-adventure centered on one exploration tool.
Unknown: The final traversal verbs, world layout, hero asset style, music identity, and demo path are not yet proven in runtime.
