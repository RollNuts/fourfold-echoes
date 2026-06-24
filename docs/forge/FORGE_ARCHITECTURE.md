# Veripsa Forge Architecture

Forge is split into a control plane and an execution plane.

The first implementation uses two layers:

1. Stable layer: CLI plus file-contract mediator.
2. Acceleration layer: Unity's official MCP bridge, later, as a wrapper over the
   same file contract.

The stable layer must work without MCP.

## Control Plane

```text
Game IR
Operation manifest
Forge CLI
GitHub PR
Veripsa Core
```

The control plane is reviewable, diffable, and safe for AI agents to edit.

## Execution Plane

```text
Unity project
Unity Adapter
File-contract mediator
Generated scene and prefab region
EditMode / PlayMode tests
Capture / replay / build output
```

The execution plane turns desired state into runtime evidence.

## Repository Shape

```text
game-spec/
  project.yaml
  scenes/
  entities/
  scenarios/
contracts/
  schemas/
changes/
  samples/
unity/
  Assets/
    Manual/
    Generated/VeripsaForge/
    Shared/
tools/
  forge/
docs/forge/
evidence/
```

This repository currently keeps the Unity project at the repository root. The
Forge shape should be introduced gradually, starting with `game-spec/`,
`contracts/`, `changes/`, and adapter scripts that target the existing Unity
layout.

## Manual And Generated Regions

Manual region:

- Human-authored art.
- Polished animation.
- Final lighting.
- Hand-made VFX.
- Final prefabs.

Generated region:

- Graybox rooms.
- Proof prefabs.
- Component wiring.
- Stable ID manifests.
- Test fixtures.
- Screenshots and temporary evidence.

Forge must not rewrite manual assets. It may reference manual assets by stable
semantic IDs after those assets are registered.

## Director Review Loop

Forge should make the review loop independent of Unity Editor knowledge:

1. AI opens a PR with semantic changes.
2. Veripsa Core reports coordination state.
3. Unity Adapter generates the scene.
4. Evidence is attached: screenshot, replay summary, console status, and build
   status when relevant.
5. The reviewer judges game feel from playable artifacts and evidence, not from
   Unity Inspector steps.

The normal director workflow should end in a screen, sound, replay, or playable
artifact. It should not end in "open Unity and click this Inspector field."

## Stable Mediator First

The initial bridge is a file contract:

```text
commands/*.json
  -> Unity -batchmode -executeMethod ...
  -> events/*.jsonl
  -> artifacts/screenshots/
  -> artifacts/logs/
  -> artifacts/test-results.xml
```

Unity exposes one narrow entrypoint that receives a command file path. AI agents
write command JSON and read generated evidence files. This is more stable than
GUI automation and continues to work even if MCP is unavailable.

Official Unity MCP can later expose a custom tool such as
`run_contract_command`, but that tool should call the same mediator rather than
becoming a second behavior path.

## First Adapter Boundary

The first Unity Adapter only needs to support `scene.ashen_threshold`.

Required operations:

- Inspect project.
- Validate Game IR.
- Plan a scene/entity change.
- Generate a proof scene.
- Capture a proof camera.
- Run a deterministic scenario.

Deferred:

- Windows build.
- Asset import automation.
- Reverse export from manual Unity edits.
- Multi-engine adapters.
