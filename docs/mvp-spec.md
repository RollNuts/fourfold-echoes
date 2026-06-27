# FOURFOLD ECHOES MVP Spec

Status: MVP planning spec, 2026-06-27.

This document defines the implementable MVP for the private `fourfold-echoes`
Unity repository. It treats the current canonical direction as binding:
FOURFOLD ECHOES is a Steam-first, buy-to-own, single-player, top-down classic
action-adventure built around one exploration tool.

## Audit Basis

The requested file `reports/repo-audit.md` is not present in this checkout.
This spec therefore uses the closest repo-local audit and evidence sources:
`docs/Product/REPO_TIMELINE_AUDIT.md`,
`artifacts/Reports/production-combat-slice-pass-20260627.md`,
`artifacts/Reports/unity-product-validation.md`,
`artifacts/Reports/generated-asset-consistency.md`,
`artifacts/Reports/audio-inventory.md`, and the canonical product documents
under `docs/Product`, `docs/Art`, `docs/Audio`, `docs/Production`,
`docs/QA`, and `docs/Marketing`.

The important repo facts are these. The project targets Unity 6.3 LTS
`6000.3.18f1`, uses the Built-in Render Pipeline, and does not contain runtime
networking, multiplayer, account, backend, quest, inventory, crafting, or open
world systems. The current strongest playable evidence is
`Assets/Scenes/ProductionCombatSlice.unity`, generated and validated with
production-prefab hero, two enemies, a boss read target, a block-field room,
one exploration node, a shortcut reveal, a gate, and a reward. The canonical
runtime hook for the product direction is the pair
`Assets/Scripts/ExplorationTool.cs` and `Assets/Scripts/ExplorationNode.cs`.
The old Gate A / Echo Phase prototype remains useful as feel and feedback
evidence, but it is not the MVP product structure.

## MVP Definition

The MVP is the smallest content-complete version that can honestly sell the
game's promise on Steam: the player leaves one small hub, enters three compact
hand-authored regions, masters one exploration tool through repeated room
rules, fights readable enemies and bosses, opens shortcuts, earns a small
number of relic rewards, and defeats four bosses including the final boss.

The MVP is not a gray-box demo and not an open-ended systems sandbox. It may
start implementation from the current `ProductionCombatSlice`, but it is
accepted only when the shipped loop is playable through the hub, three regions,
four boss clears, local save/load, minimal UI, and non-placeholder gameplay
audio. The vertical-slice milestone is a narrower proof inside this MVP: one
hub entrance, one Region 01 segment, two gimmick rooms, two normal enemy reads,
one miniboss, one boss, one shortcut, and two relic rewards.

## Scope Frame

| Area | MVP commitment | Current repo leverage | Explicit boundary |
| --- | --- | --- | --- |
| Hub | One small return hub named `Hub_Crossroads`. | P1 hub prefabs include save stone, region gate, return point, low walls, lamps, inlays, and wayline props. | No town simulation, shops, relationship system, quest board, or open-world map. |
| Regions | Three compact regions: Region 01 Green Ruins, Region 02 Sunken Works, Region 03 Ashen Keep. | P0/P1 Region 01 props, P2/P3 Region 02 and Region 03 floors, hazards, gates, bridges, switches, and set dressing already exist as production prefabs. | No fourth region, biome system, procedural world generation, or seamless streaming requirement. |
| Bosses | Four bosses total: one boss per region plus final boss. | `FE_BOSS_01_RootWarden`, `FE_BOSS_02_FurnaceWarden`, `FE_BOSS_03_GlassWarden`, `FE_BOSS_04_CrownWarden`, and P3 boss variants exist as prefab candidates. | Bosses reuse the same movement, damage, tell, room, save, and tool-opening primitives; no separate boss game framework. |
| Exploration tool | One tool with pulse, target response, fail response, cooldown, and solved-state persistence. | `ExplorationTool` and `ExplorationNode` already implement range, cooldown, nearest-node selection, activation, idle/active reads, and response targets. | No second tool, no ability tree, no inventory of tools, no world-state phase switching. |
| Combat | Readable top-down normal attack, dodge, damage, death, retry, two enemy reads, miniboss escalation, boss patterns. | `ProductionCombatSliceController` and legacy spike code prove movement, attack, dodge, enemy chase/strike, hit feedback, and simple health bars. | No combo tree, loot build, weapon spreadsheet, lock-on RPG system, or guard/parry unless a later accepted decision removes something else. |
| Rewards | Small relic rewards that change existing player or world variables without an inventory screen. | Current prefabs include `FE_RELIC_EmberSeed_01`, `FE_RELIC_RootSigil_01`, reward chest, reward receiver pad, and discovery SFX. | No inventory, crafting materials, equipment comparison, random loot, or quest reward list. |
| Save | One local save path for progress flags and settings. | Product docs already define flag-based progress; no backend exists. | No cloud dependency, account login, save-slot management UI, or platform APIs in gameplay code. |
| UI | Title menu, in-game HUD, interaction prompt, boss health, pause/settings basics, save indicator. | IMGUI HUD exists only as proof in `ProductionCombatSliceController`; UIElements module is available in manifest. | No minimap, quest log, inventory, codex, social menu, online menu, or mouse-only interface. |
| Audio | Non-placeholder required cues for tool, combat, reward, UI, save, and boss readability plus two BGM loops for the slice. | Seven generated pilot SFX exist and are registered; audio inventory records the remaining needed cues. | No voice acting, dynamic stem system, ambient dialogue, or large music implementation before core cues work. |

## Core Loop

The core loop begins in the hub. The player checks their health and tool state,
chooses an unlocked region gate, enters a compact authored route, and reads the
room from the top-down camera. Each room asks the player to do some mix of
positioning, normal attacking, dodging, interacting, and using the one
exploration tool. The player resolves the room by defeating enemies, activating
or revealing a tool target, avoiding a simple hazard, or opening a shortcut.
The room then gives a visible consequence: a route appears, a gate opens, a
reward chest becomes reachable, a boss path unlocks, or a return shortcut folds
the route back toward the hub.

Progression is primarily player mastery and persistent flags, not system
quantity. The player improves by learning enemy tells, room layouts, boss
timing, and how the tool marks hidden or dormant objects. The game saves flags
such as defeated bosses, opened shortcuts, claimed relics, unlocked regions,
current return point, and basic settings. The loop never asks the player to
manage an inventory, craft, grind loot, accept quests, or maintain social or
online state.

## One Session Flow

A normal session starts at the title menu. New Game creates a clean local save
and places the player at the hub return point. Continue loads the last saved
hub or region checkpoint. The hub presents the player with clear region gates,
a save stone, a return landmark, and no active combat. From there, the player
enters the currently available region.

Inside a region, the session is structured as a short chain of rooms. A first
room confirms movement, camera, attack, dodge, and one enemy read. A second
room demonstrates the exploration tool by making an inactive object or blocked
route respond to the pulse. A third room combines enemies with the same tool
rule. A shortcut room folds the route back or opens a permanent return. A
miniboss or boss approach tests the same combat and tool language at higher
pressure. The region boss clear sets a defeated flag, awards or reveals a relic
reward, unlocks the next region gate when appropriate, and returns the player
to the hub or to a safe post-boss exit.

The first vertical-slice session should last 20 to 30 minutes and prove the
entire loop with one region segment. The full MVP session target is a compact
premium adventure with short, readable play chunks: the player should always
know the next physical route, the next visible lock, or the next boss gate
without reading a quest log.

## Player Actions

| Action | Runtime behavior | Implementation note |
| --- | --- | --- |
| Move | The player moves on the X/Z plane with responsive top-down acceleration and no required mouse input. Facing updates from movement or attack direction. | Start by extracting the movement behavior proven in the slice/spike into a `TopDownPlayerMotor`-style component rather than expanding the monolithic proof controller. |
| Aim / face | Facing determines attack preference and tool readability. The MVP can use movement-facing first; right-stick or mouse aim is optional only after controller-first movement is stable. | Do not add lock-on as a separate system for MVP. |
| Normal attack | A short melee attack checks a readable range in front of the player, applies damage, plays hit confirm or whiff feedback, and has recovery. | Use simple hit volumes or range checks before adding animation-event complexity. |
| Dodge | Dodge moves the player in a committed direction, grants a brief invulnerability window, has cooldown/recovery, and is readable through animation, VFX, and SFX. | This is the main survival verb. Guard/parry remains out of MVP unless explicitly re-scoped. |
| Exploration tool | The player triggers a pulse. If an unsolved node is in range, the nearest valid node activates and its response target changes state. If no node is valid, the tool plays a fail response and enters cooldown. | Existing `ExplorationTool` and `ExplorationNode` are the canonical starting point. Expand node response types carefully without creating multiple tools. |
| Interact | Interact is used for region gates, save stones, reward chests, post-boss exits, and simple confirmed pickups. | Interaction never opens an inventory or quest log. Context prompts must be concise and controller-friendly. |
| Pause | Pause freezes gameplay, exposes resume/settings/quit-to-title, and returns focus predictably to controller navigation. | Settings begin with volume, display/window mode if implemented, and input prompt mode. |

## Exploration Tool Specification

The exploration tool is the product hook and the only systemic exploration
verb. It does three things across the MVP: it reveals a hidden route or object,
activates a dormant mechanism, or exposes a vulnerable state. These are not
separate abilities; they are node response types authored per room. The player
always performs the same action: approach, read the tool target, pulse, watch
the response, then act on the new room state.

Each valid node has an idle read, an active read, a solved flag, a range, an
optional highlighted response target, and one semantic audio cue set. Region 01
uses the node to reveal bridge/route pieces and open simple gates. Region 02
uses the same pulse to wake relays, stop or redirect a hazard, and make furnace
routes safe for a short or permanent window. Region 03 uses the same pulse to
expose crystal bridges, mirror locks, or boss weak reads. The underlying code
should stay the same: a node changes scene objects and persists its solved
state when the room requires permanence.

Tool mastery comes from placement, timing, and consequence. The player should
not wonder which tool to equip or which ability tree to inspect. If a room is
about the tool, the target must be visible through silhouette, glow, material,
audio, or environmental framing without relying on explanatory text.

## Rooms And Regions

Rooms are authored, bounded spaces that can be understood from the gameplay
camera. A room may contain enemies, one or more tool nodes, a hazard, a locked
gate, a shortcut, or a reward. The room controller owns entry state, active
enemy count, gate lock/unlock state, reward spawn, shortcut state, and room
completion events. It must not own global story, quest logic, or open-world
streaming.

Region 01 Green Ruins is the first proof region and should be built from the
existing moss, root, grass, stone, bridge, gate, and reward prefabs. It teaches
the tool with low-risk route reveals, introduces melee and ranged reads, opens
one shortcut, gives two simple relic rewards in the vertical slice, and ends
with RootWarden.

Region 02 Sunken Works reuses the same loop with higher pressure. Existing
amber, furnace, vent, charcoal, iron, relay, pressure switch, and bridge assets
should create rooms where the tool activates relays or makes hazard timing
readable. The region should feel like heat and machinery have altered the same
folded reliquary family, not like a separate genre.

Region 03 Ashen Keep or late crystal region reuses the loop again with colder
contrast and more precise timing. Existing crystal bridge, prism gate, mirror
switch, cold lamp, violet thread, and crystal wall assets should support rooms
where the tool exposes narrow paths, mirror locks, or boss openings. It must
not introduce a second traversal verb.

The final boss scene is a focused arena. It tests movement, dodge, attack
timing, and tool-opening recognition from the whole game. It does not require
a new region, new UI layer, or cinematic-only mechanics.

## Enemy, Obstacle, And Interaction Specification

Normal enemies exist to pressure position and timing, not to create an RPG
stat contest. A melee enemy approaches, telegraphs, strikes at close range,
recovers, and can be interrupted or punished. A ranged enemy creates a visible
line or area threat, then either repositions or recovers; the MVP should prefer
telegraphed line/area strikes over a full projectile framework unless the
projectile implementation is already proven. Both enemy types share common
health, damage, tell, hit, death, and room-count behavior.

The miniboss is a larger readability test. It reuses the normal enemy state
shape with more health, larger tells, one extra cadence change, and a room
layout that makes dodge direction matter. It must not require a new combat
system. The boss framework extends the same primitives into named arenas:
intro lock, health bar, two or three readable attacks, one tool-related opening
or exposed state, defeat event, reward/exit event, save flag.

Obstacles are simple physical or stateful blockers. Gates open, bridges appear,
hazard floors or vents become safe, relays wake, locks expose weak states, and
reward chests open. Interactions are either direct Interact prompts or
exploration-node responses. Physics puzzles, movable object chains, inventory
keys, multi-step quest flags, and crafting recipes are out of MVP.

## UI Specification

The HUD contains only what the player needs during action: health, tool
readiness/cooldown, context prompt, temporary save indicator, and boss health
when a boss is active. The vertical-slice IMGUI HUD proves debug readability
only; MVP UI should be implemented as a controller-first production HUD using
available Unity UI capability already in the project. No new UI package is a
requirement.

The title menu contains New Game, Continue, Settings, and Quit. Continue is
disabled or clearly unavailable if no save exists. The pause menu contains
Resume, Settings, Quit to Title, and Quit Game where platform-appropriate.
Settings are intentionally shallow for MVP: master/music/SFX volume and display
options only when they are tested. The UI must be readable at 1280x800 and
1920x1080, navigable without a mouse, and free of debug labels in store
captures.

The MVP does not ship a minimap, quest log, inventory screen, equipment screen,
social menu, online menu, crafting page, codex, or achievement browser.

## Clear And Failure Conditions

A region is cleared when its boss defeat flag is saved and the player can
return to the hub or safe post-boss exit. The full MVP is cleared when all
three region bosses and the final boss are defeated, the final clear flag is
saved, and the player reaches the end state without losing local progress.

Failure occurs when player health reaches zero. The game presents a quick retry
from the last valid checkpoint or room start, preserving already saved global
progress and resetting only local room combat state as appropriate. There is no
permadeath. A room can also be abandoned by pausing and returning to title, but
that is not a game failure; it must not corrupt progress.

Soft-locks are treated as release-blocking failures. If a tool node is solved,
a gate opens, a boss dies, a reward is claimed, or a region unlocks, the save
state must reload to a playable configuration.

## Current Asset Use Plan

The first implementation step should not import large external asset packs.
Use the existing production prefabs, generated material library, and generated
audio pilot cues to prove gameplay wiring, while keeping the documented
approval gate for production art and audio. The `ProductionCombatSlice` scene
is the working asset yard and behavior proof. The MVP should split its
monolithic controller behavior into production components only when the
behavior is needed by at least two concrete rooms or enemy cases.

The production-prefab asset families already cover the MVP's required shape:
hero and player variants, NPC/service cast candidates, melee and ranged enemy
candidates, Region 01 root/grass/stone rooms, Region 02 furnace/amber/iron
rooms, Region 03 crystal/prism rooms, four boss candidates, relics, reward
chests, gates, bridges, relays, switches, low walls, floors, and hazards. These
assets are useful for implementation and internal validation, but
`generated-asset-consistency` marks production approval as blocked until human
art/IP review. That review is part of the acceptance path before any
market-facing capture.

## Nice-To-Have Parking Lot

The following are intentionally outside MVP unless a later accepted decision
removes existing scope to pay for them: guard/parry, lock-on, minimap, quest
log, inventory, equipment, crafting, fishing/farming/base building, procedural
rooms, open-world traversal, additional exploration tools, day/night cycle,
co-op, online services, achievements, deep localization pipeline, advanced
accessibility beyond the first readability pass, complex dynamic music, voice
acting, cutscene-heavy storytelling, and additional regions or bosses.

Nice-to-have items may be prototyped only after the vertical slice proves the
core loop with production-intent art and audio. They must never be used to
explain missing MVP clarity.
