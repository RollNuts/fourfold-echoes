# Unity Product Validation

Generated UTC: `2026-06-27T01:30:56.5308970Z`

## Metrics

| Metric | Value |
| --- | ---: |
| Active scene objects | 488 |
| Renderers | 158 |
| AudioSources | 1 |
| LODGroups | 0 |
| Missing material slots | 0 |
| Missing scripts | 0 |
| Negative scale objects | 0 |
| Material assets | 357 |
| Texture assets | 2024 |
| AudioClip assets | 29 |
| Mesh assets | 283 |
| Prefab assets | 178 |
| Prefabs with LODGroup | 0 |
| Scene assets | 3 |

## Findings

- **info** `render.pipeline`: Built-in render pipeline is active.
- **info** `d020.slice`: D-020 vertical slice evidence scene generated and validated with player, one tool node, shortcut route, enemy, and relic chest.
- **info** `prototype.gate_a`: Legacy Gate A generation skipped. Set FOURFOLD_INCLUDE_LEGACY_GATE_A=1 to validate the old harness explicitly.
- **info** `production.prefab_slice`: Production combat slice generated and validated with real Production prefabs for hero, enemies, boss, block field, exploration node, gate, bridge, and reward.
- **warn** `lod.prefabs`: Prefab assets exist but none include LODGroup components.
- **warn** `scene.lod`: Active scene has no LODGroup components. This is acceptable for the reset baseline but not for production outdoor assets.

## Product Interpretation

This report validates technical hygiene and scene wiring. D020VerticalSlice remains the focused ExplorationTool + ExplorationNode evidence path, while ProductionCombatSlice is the current production-prefab combat/readability lane with hero, enemies, boss, block field, exploration node, gate, and reward wiring. Art quality, animation quality, LOD coverage, and store-level feel still require separate review.
