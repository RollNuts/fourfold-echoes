# Core Systems

Status: canonical after D-020. This is a scope-control document, not a wish list.

## 必須

| System | Purpose | MVP Acceptance |
| --- | --- | --- |
| Top-down movement | basic feel | responsive on controller and keyboard |
| Camera | readability | player, enemies, route, and tool target stay visible |
| Normal attack | core combat | hit confirm and recovery are clear |
| Dodge | survival | timing and recovery are readable |
| Basic damage/death/retry | fail loop | player can fail and quickly retry |
| One exploration tool | product hook | reveals, activates, or exposes room solutions |
| Exploration nodes | tool targets | one node type can open/reveal a route or object |
| Room controller | room flow | enter, fight/solve, reward, shortcut |
| Enemy AI | combat pressure | at least melee and ranged/basic variant in slice |
| Miniboss and boss framework | vertical slice proof | readable tells and phase-like health thresholds without new systems |
| Shortcut unlock | adventure structure | one shortcut folds route back to hub/earlier area |
| Relic reward | reward feel | two rewards in slice, no inventory spreadsheet |
| Minimal HUD | readability | health, tool state, prompt, boss health |
| Local save/load | product viability | progress flags roundtrip safely |
| BGM/SFX routing | product feel | required cues play from first playable milestone |

## 不要

| System | Reason |
| --- | --- |
| Open-world streaming | contradicts compact non-open-world scope |
| Echo Phase/world-state switching | adds multiple systems instead of one tool mastery |
| Inventory | expands UI and economy with low MVP value |
| Crafting | pulls focus away from action-adventure rooms |
| Quest log | implies content scale not in MVP |
| Social systems | outside single-player compact scope |
| Multiplayer/co-op | multiplies QA and design cost |
| Live service/backend | contradicts buy-to-play offline promise |
| Farming/fishing/base building | unrelated loops |
| Procedural world generation | weakens handcrafted room clarity |

## 後回し

| System | Revisit Only After |
| --- | --- |
| Settings depth | core slice controls and audio pass |
| Localization pipeline | stable UI text set |
| Platform achievements | Steam release scope lock |
| Advanced accessibility | basic controls/UI/audio are stable |
| Additional enemy variants | two enemies + miniboss + boss prove fun |
| Additional relics | two slice relics prove reward value |

## Implementation Boundary

Code should be clean enough to extend, but not abstract enough to hide the game.
Prefer direct, testable components for the vertical slice. Introduce shared
abstractions only when two concrete rooms or enemies already need the same
behavior.
