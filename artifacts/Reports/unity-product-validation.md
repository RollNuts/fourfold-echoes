# Unity Product Validation

Generated UTC: `2026-06-28T00:46:13.1154260Z`

## Metrics

| Metric | Value |
| --- | ---: |
| Active scene objects | 517 |
| Renderers | 492 |
| AudioSources | 2 |
| LODGroups | 0 |
| Missing material slots | 0 |
| Missing scripts | 0 |
| Negative scale objects | 0 |
| Material assets | 75 |
| Texture assets | 0 |
| AudioClip assets | 11 |
| Mesh assets | 35 |
| Prefab assets | 33 |
| Prefabs with LODGroup | 0 |
| Scene assets | 4 |

## Findings

- **info** `d022.contract`: D022 product contract validated: current top-down adventure MVP pack is present, AGENTS points to it, UI/UX layouts fit 1280x800/1080p, Title/Hub/Region menu SFX hooks are present, Region tool target-state HUD is present, and stale player-facing copy is blocked.
- **info** `render.pipeline`: Built-in render pipeline is active.
- **info** `save.service`: Versioned local save validated with settings defaults, language preference, UI scale/control-hint preferences, settings preservation across reset, roundtrip persistence, backup recovery, and corrupt-save fallback.
- **info** `r01.verdant_steps`: R01 Verdant Steps evidence path generated and validated with one exploration tool, sealed-route and shortcut interactions, explicit tool target/no-target/cooldown feedback, two normal enemy types, elite guard, boss, boss tool-opening attack window, combat feedback text, basic-attack enemy defeat, enemy-hit failure, failure result/retry/hub-return UX, title return, shared pause/settings/language UX with menu SFX, objective marker, progression rail, build-slot HUD, dodge state HUD, reward-effect notice UX, confirmation before abandoning unsaved rewards, two distinct saved reward skills, Lumen Link combined-skill recovery, return gate, required SFX, two BGM clips, and hub-return reward persistence.
- **info** `art.production_p3`: Production P3 model pack imported and validated with 28 prefabs, renderer/mesh/material references, and sane bounds.
- **info** `hub.crossroads`: Hub Crossroads generated and validated as the playable hub with an R01 region gate, staged R02 future gate, mission briefing/start confirmation, reward-skill synergy and loss-risk briefing, returned-run summary/replay UX with last-clear/new-best timing, failed-return summary UX, objective marker, progress initialization, pause/settings/language UX with menu SFX, reset confirmation, and return-to-title persistence.
- **info** `title.entry`: Title scene generated and validated with New Game overwrite confirmation, Continue resume-or-hub choice for in-progress runs, Settings volume/language persistence, menu SFX hooks, Quit request, and Build Settings order Title -> HubCrossroads -> R01.
- **info** `steam_deck.readiness`: Title, Hub, and R01 validated for 1280x800/1080p HUD safe areas, legacy movement axes, and controller-critical bindings.
- **info** `prototype.gate_a`: Legacy Gate A generation skipped. Set FOURFOLD_INCLUDE_LEGACY_GATE_A=1 to validate the old harness explicitly.
- **warn** `texture.assets`: No production texture assets found. Store-quality art pass has not started.
- **warn** `lod.prefabs`: Prefab assets exist but none include LODGroup components.
- **warn** `scene.lod`: Active scene has no LODGroup components. This is acceptable for the reset baseline but not for production outdoor assets.

## Product Interpretation

This report validates technical hygiene only. D022 is the current product contract: Steam-first, buy-to-play, single-player, compact top-down classic action-adventure, one hub, three regions, four bosses, and one exploration tool. Title is the product entry point, HubCrossroads is the playable hub, and R01 Verdant Steps is the first playable evidence path for D022 player-facing language and UI/UX. Required product evidence includes title flow, hub objective marker, mission briefing, readable combat, exploration tool target response, boss clear, reward save-on-hub-return, local save, required SFX/BGM, pause/settings/language UX, and 1280x800 readability. Historical ProductReview, Gate A, D020, and D021 evidence are deliberately outside the active implementation lane unless explicitly migrated into D022.
