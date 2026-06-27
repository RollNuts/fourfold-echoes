# Unity Product Validation

Generated UTC: `2026-06-25T19:03:22.2483550Z`

## Metrics

| Metric | Value |
| --- | ---: |
| Active scene objects | 118 |
| Renderers | 105 |
| AudioSources | 1 |
| LODGroups | 0 |
| Missing material slots | 0 |
| Missing scripts | 0 |
| Negative scale objects | 0 |
| Material assets | 10 |
| Texture assets | 0 |
| AudioClip assets | 7 |
| Mesh assets | 0 |
| Prefab assets | 0 |
| Prefabs with LODGroup | 0 |
| Scene assets | 1 |

## Findings

- **info** `render.pipeline`: Built-in render pipeline is active.
- **info** `d020.slice`: D-020 vertical slice evidence scene generated and validated with player, one tool node, shortcut route, enemy, and relic chest.
- **info** `d020.playable_smoke`: D-020 playable smoke passed for movement, fixed top-down camera, dodge, normal attack, enemy defeat, one-tool shortcut response, visible reward, and imported core SFX assets.
- **info** `prototype.gate_a`: Legacy Gate A generation skipped. Set FOURFOLD_INCLUDE_LEGACY_GATE_A=1 to validate the old harness explicitly.
- **warn** `prefab.assets`: No prefab assets found. Production content is not prefabbed yet.
- **warn** `texture.assets`: No production texture assets found. Store-quality art pass has not started.
- **warn** `scene.lod`: Active scene has no LODGroup components. This is acceptable for the reset baseline but not for production outdoor assets.

## Product Interpretation

This report validates technical hygiene only. D020VerticalSlice is the current D-020 evidence path for the first controllable Region 01 test room, now including a batchmode playable smoke for movement, fixed top-down camera, dodge, normal attack, enemy defeat, one ExplorationTool response, one shortcut response, one visible reward, and imported non-placeholder core SFX assets. Historical ProductReview evidence is deliberately outside this lane.

See `artifacts/Reports/d020-playable-room-milestone.md` for the 2026-06-27
milestone verification pass and remaining risk.
