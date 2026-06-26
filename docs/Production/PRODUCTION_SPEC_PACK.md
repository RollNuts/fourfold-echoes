# Production Spec Pack

Status: current completion and scope-control pack for D-020.

This pack is for the single-player Steam-first MVP only. Art production is
handled in its own lane; this document defines the game-completion boundary that
art, audio, QA, store copy, and implementation must obey.

## Specification Memo

FOURFOLD ECHOES is a compact, single-player, top-down classic
action-adventure. The MVP is built around one hub, at most three handcrafted
regions, at most four bosses, and one exploration tool that gains depth through
room layout, enemy placement, shortcuts, boss patterns, and player mastery.

Hard limits:

| Area | Limit |
| --- | --- |
| Hub | 1 |
| Regions | 3 maximum |
| Bosses | 4 maximum |
| Exploration tools | 1 |
| Inventory/crafting/quest/social systems | 0 |
| Open world/day-night/fishing/farming/base building | 0 |
| Multiplayer/online/live service/backend dependency | 0 |

Production rule: improve clarity, feel, readability, and completion before
adding content. New systems are rejected unless an accepted decision removes
existing scope first, and no decision may exceed the limits above.

## Scene List

Allowed production scenes:

| Scene | Purpose | Scope Notes |
| --- | --- | --- |
| `Bootstrap` | startup and service creation | no gameplay rules |
| `Title_Menu` | new game, continue, settings, quit | no feature marketing beyond shipped scope |
| `PersistentSystems` | input, save, audio, scene flow | loaded once |
| `UI_Game` | HUD, pause, settings | no quest log, inventory, or map economy |
| `Hub_Crossroads` | single hub, region gates, return point | 1 hub only |
| `Region_01_GreenRuins` | first region, tool introduction, boss 1 | vertical-slice target |
| `Region_02_SunkenWorks` | second region, tool mastery, boss 2 | no second tool |
| `Region_03_AshenKeep` | third region, late-game pressure, boss 3 | final region cap |
| `Boss_04_Final` | final boss arena | boss 4 maximum |

Proof or preview scenes such as `D020VerticalSlice` and art preview scenes may
exist only as validation scaffolding. They must not expand the product scene
list or justify extra systems.

## Script Responsibility List

Runtime scripts should stay small and purpose-bound:

| Script Area | Owns | Must Not Own |
| --- | --- | --- |
| Bootstrap/scene flow | startup, title flow, hub/region/boss transitions | open-world streaming |
| Input/camera | keyboard/controller input, readable top-down framing | gameplay decisions, cinematic framework |
| Player motor | movement, facing, dodge | damage rules, progression |
| Player combat | normal attack, hit stun, damage intake, death/retry | combo trees, loot builds |
| Exploration tool | the single tool input, pulse/aim/use state | multiple abilities, inventory, crafting |
| Exploration nodes | reveal, activate, expose responses | quest logic or world-state switching |
| Room controller | entry lock, enemy clear, reward spawn, shortcut open | procedural world generation |
| Enemies | movement, telegraph, attack, death | boss phases or global progression |
| Bosses | boss patterns, openings, defeat events | save storage or economy |
| Progress/save | local flags, current scene, shortcuts, bosses, regions | account, cloud, backend, inventory |
| UI | health, tool state, prompts, boss health, pause/settings | quest log, minimap economy, social menus |
| Audio routing | cue playback and BGM state | composition tools or asset generation |

Do not touch gameplay scripts from this lane unless a separate implementation
task explicitly owns that work.

## Data Design

Keep data authored and finite. No procedural world, inventory economy, or quest
database is part of the MVP.

| Data | Required Fields | Cap/Rule |
| --- | --- | --- |
| `RegionDefinition` | `id`, `sceneName`, `unlockFlag`, `hubGateId`, `bossId` | 3 maximum |
| `BossDefinition` | `id`, `sceneName`, `hp`, `attackSetId`, `defeatFlag` | 4 maximum |
| `EnemyDefinition` | `id`, `hp`, `moveSpeed`, `telegraphTime`, `attackId` | small authored set |
| `ExplorationNodeDefinition` | `id`, `nodeType`, `requiredFlag`, `responseTargetId`, `sfxId`, `vfxId` | reveal/activate/expose only |
| `RoomDefinition` | `id`, `sceneName`, `enemySet`, `rewardId`, `shortcutFlag` | authored rooms only |
| `RelicRewardDefinition` | `id`, `displayName`, `effectId`, `presentationId` | no inventory UI |
| `SaveData` | `version`, `currentScene`, `hubSpawnId`, `bossDefeated`, `shortcutsOpened`, `regionsUnlocked`, `toolOwned`, `settings` | one local save path |
| `AudioCueDefinition` | `id`, `category`, `clipPath`, `volume`, `priority` | cues registered before market capture |

## Implementation Order

1. Lock the D-020 scope documents and reject stale open-world, Echo Phase, loot,
   co-op, or extraction references as historical.
2. Build the playable base: bootstrap, persistent systems, title flow, hub stub,
   one region test room, movement, camera, normal attack, dodge, damage,
   death/retry.
3. Prove the one exploration tool with one node that visibly changes a route or
   reveals an interactable.
4. Add room flow: room gates, two tool gimmick rooms, one shortcut, two relic
   reward presentations, and local save/load for flags.
5. Complete combat slice: two normal enemies, one miniboss, one boss, readable
   tells, retry flow, combat SFX hooks.
6. Replace blockout in the slice with production-readable hero, tool, enemy,
   reward, room, and boss visuals from the art lane.
7. Integrate BGM and required SFX for movement, attack, hit, dodge, tool,
   reward, UI, miniboss, and boss.
8. Stabilize UI, controller feel, 1280x800 readability, save roundtrip, and
   Windows build output.
9. Capture market evidence from real gameplay: screenshots, short trailer beats,
   and a 20-30 minute hands-on path.

## Scope-Out List

Reject these for MVP and vertical-slice work:

| Scope-Out Item | Reason |
| --- | --- |
| Inventory, crafting, equipment economy | adds UI/economy scope beyond one-tool mastery |
| Quest log or task tracker | implies broader content structure |
| Social systems, accounts, online backend | contradicts offline single-player scope |
| Multiplayer, co-op, PvP | multiplies design and QA cost |
| Open world or seamless large-map streaming | contradicts compact authored regions |
| Day-night cycle | adds content, lighting, save, and QA burden |
| Fishing, farming, base building, survival loops | different genre promises |
| Second exploration tool | dilutes the one-tool mastery pillar |
| More than 1 hub, 3 regions, or 4 bosses | exceeds MVP ceiling |
| Procedural world generation | weakens authored room readability |
| Hack-and-slash loot/builds or extraction loop | stale historical direction |
| Placeholder art/audio in market captures | invalidates market evidence |

## Art Scope Summary

Art lane may improve production readability for the existing MVP targets only:
hero, one exploration tool, first enemy set, boss/miniboss silhouettes, reward
props, hub, three-region visual language, room landmarks, shortcut readability,
tool VFX, hit/tell VFX, and screenshot composition.

Art lane must not add extra regions, tool variants that behave like new tools,
decorative-only content that delays gameplay readability, or market-facing
placeholder media.

## Audio Scope Summary

Audio scope is BGM and SFX that clarify the existing loop: movement, attack,
hit, dodge, death/retry, tool use, node response, reward, UI confirm/cancel,
room clear, miniboss, boss, and region ambience.

Audio must not add systems such as adaptive music middleware dependencies,
voice pipelines, radio/dialogue content, licensed music risk, or placeholder
cues in validation captures.

## QA Scope Summary

QA validates completion, not breadth:

| Check | Acceptance |
| --- | --- |
| Scope cap | 1 hub, 3 regions max, 4 bosses max, 1 tool |
| First 30 minutes | objective, danger, reward, retry, and tool use are clear |
| Controls | controller-first movement, attack, dodge, tool, pause |
| Save/load | boss, shortcut, region, tool, and settings flags roundtrip locally |
| UI readability | health, tool state, prompts, boss health, pause/settings read at 1280x800 |
| Audio/visual evidence | no gray-box or placeholder audio in market captures |
| Stability | no crash, softlock, save corruption, or stuck room in the slice path |

## Store Scope Summary

Store copy may claim only the locked product promise:

- single-player top-down classic action-adventure
- compact handcrafted adventure
- one hub, up to three regions, up to four bosses
- one exploration tool used for repeated mastery
- readable combat, shortcuts, boss fights, and relic rewards
- Steam-first premium buy-to-own game

Store copy must not claim open world, online play, co-op, crafting, inventory
depth, farming, fishing, base building, day-night simulation, live-service
updates, procedural worlds, or features that are not implemented or locked.
