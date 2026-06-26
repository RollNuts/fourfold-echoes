# Unity Product Validation

Generated UTC: `2026-06-26T16:53:06.6675710Z`

## Metrics

| Metric | Value |
| --- | ---: |
| Active scene objects | 533 |
| Renderers | 508 |
| AudioSources | 2 |
| LODGroups | 0 |
| Missing material slots | 0 |
| Missing scripts | 0 |
| Negative scale objects | 0 |
| Material assets | 47 |
| Texture assets | 0 |
| AudioClip assets | 9 |
| Mesh assets | 7 |
| Prefab assets | 5 |
| Prefabs with LODGroup | 0 |
| Scene assets | 4 |

## Findings

- **info** `render.pipeline`: Built-in render pipeline is active.
- **info** `save.service`: Versioned local save validated with settings defaults, UI scale/control-hint preferences, settings preservation across reset, roundtrip persistence, backup recovery, and corrupt-save fallback.
- **info** `d020.slice`: D-020 vertical slice generated and validated with one exploration tool, two tool nodes, shortcut route, two normal enemy types, elite guard, boss, basic-attack enemy defeat, enemy-hit failure, retry, title return, shared pause/settings UX, objective marker, progression rail, unbanked relic abandon confirmation, two distinct relic effects, return gate, failed-run reward loss, required SFX, two BGM clips, and full-loop reward banking.
- **info** `hub.crossroads`: Hub Crossroads generated and validated as the playable hub with a D-020 region gate, mission briefing/start confirmation, objective marker, progress initialization, pause/settings UX, reset, and return-to-title persistence.
- **info** `title.entry`: Title scene generated and validated with New Game, Continue, Settings volume persistence, Quit request, and Build Settings order Title -> HubCrossroads -> D020VerticalSlice.
- **info** `steam_deck.readiness`: Title, Hub, and D-020 validated for 1280x800/1080p HUD safe areas, legacy movement axes, and controller-critical bindings.
- **info** `prototype.gate_a`: Legacy Gate A generation skipped. Set FOURFOLD_INCLUDE_LEGACY_GATE_A=1 to validate the old harness explicitly.
- **warn** `texture.assets`: No production texture assets found. Store-quality art pass has not started.
- **warn** `lod.prefabs`: Prefab assets exist but none include LODGroup components.
- **warn** `scene.lod`: Active scene has no LODGroup components. This is acceptable for the reset baseline but not for production outdoor assets.

## Product Interpretation

This report validates technical hygiene only. Title is the product entry point, HubCrossroads is the playable hub, and D020VerticalSlice is the current D-020 evidence path for the one-tool compact action-adventure slice: title entry, hub objective marker, mission briefing/start confirmation, two normal enemy types, one elite guard, one boss, two tool nodes, objective marker, progression rail, two distinct relic effects, unbanked relic abandon confirmation, failed-run reward loss, return banking, required SFX, two BGM clips, pause/settings UX, and full-loop reward persistence. Historical ProductReview evidence is deliberately outside this lane.
