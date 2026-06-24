# Game IR

Game IR is the stable semantic layer edited by AI agents and reviewed in Git.

AI agents should not author `.unity` or `.prefab` files directly. The Unity
Adapter owns the conversion from Game IR to Unity objects.

## ID Rules

Use stable semantic IDs:

- `scene.ashen_threshold`
- `entity.player.solo`
- `entity.enemy.hollow_grunt`
- `entity.object.ember_altar`
- `entity.object.claim_gate`
- `material.phase.ember`
- `scenario.gate_a.clear_room`

Do not use Unity GameObject names, scene paths, or generated GUIDs as the only
source of identity.

## Initial Types

- Project
- Scene
- Entity
- Transform
- Component
- Material reference
- Camera
- Light
- Terrain block
- Interaction
- Puzzle state
- Audio cue
- Scenario

## Entity Shape

```yaml
id: entity.object.ember_altar
archetype: interactable.altar
scene: scene.ashen_threshold
transform:
  position: { x: 1.3, y: 0.24, z: 0 }
  rotation: { x: 0, y: 0, z: 0 }
  scale: { x: 0.72, y: 0.22, z: 0.72 }
components:
  - type: heat_receiver
    accepted_phase: ember
    threshold: 100
  - type: puzzle_state_emitter
    grants_state: state.gate_open
presentation:
  primitive: cylinder
  material: material.phase.ember
  readable_outline: true
```

## Scenario Shape

```yaml
id: scenario.gate_a.clear_room
scene: scene.ashen_threshold
seed: 42
commands:
  - tick: 1
    actor: entity.player.solo
    action: move
    vector: { x: 1, z: 0 }
  - tick: 40
    actor: entity.player.solo
    action: attack
  - tick: 120
    actor: entity.player.solo
    action: phase_action
    target: entity.object.ember_altar
expected:
  - entity_exists: entity.object.claim_gate
  - event_emitted: event.gate_opened
  - screenshot: camera.proof_room
```

## Unity Leakage To Avoid

- GameObject hierarchy indexes.
- Inspector field order.
- `.meta` GUIDs as user-facing IDs.
- Scene YAML patches.
- Material asset paths as the only identity.

The Adapter may store Unity mappings internally, but Game IR remains semantic.
