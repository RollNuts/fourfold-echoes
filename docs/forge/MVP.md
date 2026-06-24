# Forge MVP

The MVP is not a general Unity automation system.

The MVP proves that FOURFOLD ECHOES Gate A can be generated, verified, captured,
and reviewed through GitHub-centered desired state.

## Target Scene

`scene.ashen_threshold`

## Required Content

- Block room.
- Player spawn.
- Hollow enemy.
- Ember altar.
- Claim gate.
- Proof camera.
- Proof lighting.
- Collider intent.
- Ember material intent.
- Minimal audio cue intent.
- Gate A clear scenario.

## Required Commands

```sh
forge inspect project
forge inspect scene scene.ashen_threshold
forge plan changes/samples/ember-altar.yaml
forge apply changes/samples/ember-altar.yaml --dry-run
forge verify changes/samples/ember-altar.yaml
forge capture scene scene.ashen_threshold
```

The first implementation can map these commands to repository scripts before a
full standalone CLI exists.

The first Unity-facing implementation should be the file-contract mediator:

```sh
tools/forge/forge run commands/samples/run-room-spike.json
```

That wrapper may call Unity `-batchmode -executeMethod` internally.

## Required Evidence

- Schema validation result.
- Unity version result.
- Scene generation result.
- Console error result.
- Screenshot from `camera.proof_room`.
- Scenario summary for `scenario.gate_a.clear_room`.

## First Runtime Slice

The first runtime slice is not a full build pipeline. It is:

1. Generate or open `scene.ashen_threshold`.
2. Run the room spike command.
3. Capture proof camera screenshot.
4. Write JSONL events and Unity logs.
5. Return artifact paths that can be attached to a PR.

## Director Acceptance

The director should be able to review without Unity knowledge:

- A PR summary.
- A screenshot.
- A short note on controls.
- A playable artifact when available.
- A simple prompt: keep, change, or reject.

## Deferred

- Windows build automation.
- Official Unity MCP custom tool wiring.
- Asset import pipeline.
- Reverse export from manual Unity edits.
- Performance regression dashboard.
- Multi-engine adapters.
- Automatic PR comments with rich media.
