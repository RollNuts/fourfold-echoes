# FOURFOLD ECHOES MVP Backlog By Role

Status: MVP planning backlog, 2026-06-27.

This backlog translates `docs/mvp-spec.md` and
`docs/acceptance-criteria.md` into role-owned work. The order is intentionally
practical: turn the existing `ProductionCombatSlice` and `ExplorationTool`
proofs into reusable production pieces, prove one Region 01 vertical slice,
then extend the same rule set to the full one-hub, three-region, four-boss MVP.

Priority meanings are simple. P0 work blocks the vertical slice. P1 work blocks
the full MVP. P2 work is important polish or scale work after the loop proves
itself. Nice-to-have work is parked until the acceptance gate says it can enter.

## Game Design

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Define the Region 01 vertical-slice route as a playable paper map: hub entrance, first combat room, tool reveal room, mixed combat/tool room, shortcut room, reward room, miniboss escalation, and boss arena. | Engineering and art can build each room without asking what the player does next, and every room uses only move, attack, dodge, interact, and the one tool. |
| P0 | Specify the two vertical-slice tool rooms so they use the same pulse differently. | One room is a route/bridge or blocker reveal, the other is a timing or combat-pressure activation; neither requires another ability or inventory item. |
| P0 | Write enemy behavior cards for melee, ranged line-threat, miniboss, and RootWarden. | Each card describes tell, range, damage window, recovery, health tier, room role, and counterplay in implementation-ready language. |
| P1 | Define Region 02 and Region 03 room rule variations using existing relay, vent, switch, crystal bridge, mirror lock, and prism lock assets. | Each region deepens the same tool rule through layout and pressure, not a new system. |
| P1 | Define the four-boss progression and region unlock flags. | Boss defeat, reward, next gate unlock, hub return, and save behavior are unambiguous for all four bosses. |
| P2 | Tune relic reward effects as small modifiers to existing variables. | Rewards feel meaningful in playtest and never require an inventory or equipment UI. |

## Lead / Gameplay Programming

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Split `ProductionCombatSliceController` behavior into production components for player motor, player combat, health/damage, enemy controller, room controller, and boss controller only as concrete rooms require them. | The Region 01 slice no longer depends on one monolithic proof controller, and the old controller can remain as evidence without owning product logic. |
| P0 | Keep `ExplorationTool` and `ExplorationNode` as the canonical tool path, then add minimal response typing for reveal, activate, and expose. | Two rooms use the same tool input and node logic with different authored response targets, and solved states can be queried by room/save code. |
| P0 | Implement room flow for enter, lock, enemy count, solve, shortcut, reward, and exit. | A room can be reset after death and can persist permanent shortcut/reward state after save/load. |
| P0 | Implement death/retry and checkpoint behavior. | Player death restarts from the valid room or checkpoint without losing saved boss, shortcut, region, or relic flags. |
| P0 | Add semantic audio cue routing for player, enemy, tool, reward, UI, save, and boss events. | Gameplay code requests cue IDs or semantic events rather than hardcoding random clip paths. |
| P1 | Implement simple scene flow for title, persistent systems, hub, region, boss arena, and UI. | The player can start, enter a region, clear boss content, return to hub, quit, and continue from save. |
| P1 | Implement local versioned save service for progress flags and settings. | Save roundtrip and save-corruption fallback pass the acceptance criteria. |
| P1 | Implement boss framework for four bosses using shared attack primitives. | Each boss has intro lock, health UI, readable attacks, tool opening, defeat flag, and post-boss exit/reward. |
| P2 | Add automated smoke tests for movement, attack, dodge, tool activation, room clear, save roundtrip, and cue firing where practical. | CI or local validation creates evidence under `artifacts/test-results/` without requiring manual scene inspection for every regression. |

## UI / UX

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Replace debug IMGUI HUD in product scenes with a minimal controller-first HUD. | Health, tool readiness/cooldown, prompt, save indicator, and boss health are readable at 1280x800 and 1920x1080. |
| P0 | Define and implement prompt behavior for interact, save, region gate, reward, and invalid tool use. | Prompts appear only when actionable, do not explain missing gameplay clarity, and do not require mouse input. |
| P1 | Implement title and pause menu flows. | New Game, Continue, Settings, Quit, Resume, and Quit to Title work with controller focus and keyboard. |
| P1 | Add capture-safe UI mode. | Debug labels and prototype indicators can be hidden for store screenshots and trailer capture. |
| P2 | Add localization-ready string IDs for shipped UI text after the UI surface stabilizes. | Text can be exported or reviewed without redesigning the UI. |

## Art Direction / 3D Art

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Review existing production prefabs for vertical-slice use and mark approved, needs edit, or internal-only. | Hero, tool, two enemies, miniboss, RootWarden, Region 01 room kit, shortcut, gate, and reward assets have a clear production-readiness label. |
| P0 | Produce or polish production-intent silhouettes for hero, exploration tool, melee enemy, ranged enemy, miniboss, RootWarden, shortcut, and reward chest. | A top-down screenshot communicates the object role without debug labels or explanatory UI. |
| P0 | Build the Region 01 visual kit from existing root, moss, grass, stone, bridge, gate, low wall, flower, and signal-thread assets. | The vertical slice no longer reads as primitive blockout and meets the folded reliquary style direction. |
| P1 | Build hub identity using P1 hub save stone, return point, region gates, lamps, low walls, inlays, benches, and wayline props. | The hub is readable as safe return space and visually distinguishes three region gates. |
| P1 | Extend Region 02 and Region 03 kits from existing P2/P3 furnace, amber, iron, crystal, prism, mirror, lamp, and bridge assets. | Each region is distinct in screenshot while still belonging to the same folded reliquary product family. |
| P1 | Establish human art/IP approval evidence for generated production assets used in store-facing capture. | The current `blocked_until_human_art_ip_review` status no longer blocks market screenshots. |
| P2 | Add LODGroup coverage or documented exceptions for repeated outdoor props and landmarks. | Product validation no longer carries unresolved production-risk LOD warnings for MVP scenes. |

## Technical Art / VFX

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Create gameplay-first VFX for tool pulse, target hit, fail, shortcut reveal, hit confirm, damage, enemy tell, reward, and boss opening. | Effects improve readability and never cover attack tells or tool targets. |
| P0 | Set material and emission conventions for idle, active, solved, danger, reward, and boss weak states. | Tool nodes and obstacles share a readable state language across Region 01 rooms. |
| P1 | Build regional lighting presets for hub, Region 01, Region 02, Region 03, and boss arenas. | Screenshots at 1280x800 and 1920x1080 retain player, enemy, route, and tool readability. |
| P1 | Create import and prefab validation rules for colliders, renderers, missing materials, and scale. | New production prefabs cannot silently enter scenes with broken renderer/collider basics. |
| P2 | Capture frame-time and overdraw/perf snapshots for the vertical slice and later MVP scenes. | Performance risks are measured before store capture and before Steam Deck checks. |

## Audio

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Replace or formally accept pilot SFX for tool pulse, target hit, fail, shortcut/mechanism, attack, hit confirm, dodge, player damage, enemy tell, enemy death, reward, and discovery. | Required cues fire from gameplay events and are not merely files in `Assets/Audio/Generated`. |
| P0 | Create vertical-slice hub/exploration BGM and boss/combat BGM. | The slice has two loopable tracks mixed under critical gameplay cues. |
| P1 | Complete boss cue set for intro, tell, impact, transition, defeat, reward, and exit. | Bosses remain readable with music playing and no critical cue is masked. |
| P1 | Complete UI and save cue set. | Select, confirm, back, error, pause, save, load/continue, and settings apply are audible and routed. |
| P2 | Perform loudness and mix pass for store capture. | Trailer and screenshot capture sessions have no placeholder or missing audio markers. |

## QA / Release

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Create manual smoke checklist for the Region 01 vertical slice. | A tester can verify movement, attack, dodge, tool, interact, room clear, shortcut, reward, death/retry, boss clear, pause, and save/load in one pass. |
| P0 | Run validation after slice builds and record evidence. | `product.validate`, missing-script/material checks, and current scene validation have repo-local reports. |
| P1 | Build Windows clean-launch, controller-only, save-roundtrip, and save-corruption test evidence. | The build can be started and completed through the MVP path without editor-only assumptions. |
| P1 | Run 1280x800 and 1920x1080 UI/readability checks. | HUD, menus, prompts, enemy tells, and tool targets are readable and unclipped. |
| P1 | Start Steam Deck-oriented compatibility checks when a Windows build and controller path are stable. | Deck readability, input, suspend/resume, save, audio, and frame-time evidence exist. |
| P2 | Maintain bug priority table focused on launch, save, progression, controller, crash, UI clipping, and missing required cues. | P0/P1 issues are not hidden behind polish backlog. |

## Marketing / Store

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Define the first eight screenshot candidates around real gameplay: hub, tool use, room before/after, enemy combat, miniboss, boss, reward. | Every shot maps to implemented content and rejects gray-box or debug UI. |
| P1 | Capture 30 to 45 seconds of current gameplay footage after art/audio acceptance. | Silent footage communicates hero, tool, room problem, solution, reward, and boss threat. |
| P1 | Keep store copy aligned with implemented scope. | Copy promises compact top-down action-adventure, one tool, three regions, four bosses, and no filler systems; it avoids open world, co-op, loot, crafting, and quest language. |
| P2 | Prepare demo announcement copy only after the vertical slice is playable. | The announcement lists only content that exists in the build. |

## Production / Scope / Legal

| Priority | Work | Done when |
| --- | --- | --- |
| P0 | Maintain scope gate for every proposed new feature. | New systems are rejected unless they replace existing scope and pass the accepted decision process. |
| P0 | Track asset rights for generated and any newly imported assets. | `docs/ASSET_RIGHTS.md`, `docs/Legal/LICENSES.md`, and relevant asset registers cover anything used in store-facing capture. |
| P1 | Keep this game repo focused on runtime assets and active slice work. | Large reusable source/reference packs are stored through the asset-library repository process instead of this repo. |
| P1 | Maintain milestone evidence index. | Each milestone links to validation, screenshots/captures, QA notes, known blockers, and next smallest useful task. |

## Nice-To-Have Parking Lot

| Item | Revisit condition |
| --- | --- |
| Guard or parry | Only after attack, dodge, enemy tells, and boss timing are accepted, and only if one existing combat feature or content item is cut. |
| Lock-on | Only if controller-first facing fails playtests and simpler aim/facing tuning cannot solve it. |
| Minimap | Only if authored hub/region landmarks and shortcuts fail orientation tests without adding quest-log-like UI. |
| Achievements | Only after full MVP progression flags are stable and Steam release planning begins. |
| Advanced localization | Only after UI text surface stabilizes and no layout-critical text is still changing. |
| Dynamic music stems | Only after two core BGM loops and required gameplay cues are mixed and accepted. |
| Extra enemies, relics, rooms, or bosses | Only after the one-hub, three-region, four-boss MVP path is playable and accepted. |
