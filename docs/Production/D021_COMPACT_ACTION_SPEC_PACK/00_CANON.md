# D021 Canon

Status: current D021 source of truth.

This folder supersedes D020 planning for future product decisions. Existing
D020 scenes and scripts are historical proof material until they are migrated or
deleted by a dedicated implementation PR.

## Fixed Product Memo

FOURFOLD ECHOES is a Steam-first, buy-to-play, single-player top-down classic
action-adventure.

The game is compact by design:

- 1 hub.
- 3 handcrafted regions.
- 4 bosses.
- 1 exploration tool.
- Local save only.
- Controller-first UI.
- Offline play.
- No live service.

The first public product promise is not open-world scale, co-op, extraction, or
hack-and-slash loot. The promise is a focused adventure where a small number of
rules become satisfying through readable rooms, shortcuts, enemy placement, boss
openings, reward beats, and mastery of one exploration tool.

## Product Feel

- Classic action-adventure pacing: prepare in hub, enter a region, solve rooms,
  fight readable enemies, open shortcuts, defeat a boss, return with progress.
- Simple attack and dodge. No combo-tree requirement.
- One tool that reveals, activates, or exposes. It must be visually and
  sonically recognizable before text explains it.
- Rewards are concrete but small. They improve play feel without creating an
  inventory, loot economy, crafting table, or spreadsheet build game.
- UI/UX is product scope, not a debug overlay. Title, continue, pause, settings,
  save feedback, retry, boss HP, prompts, and result screens must be shippable.

## Required Player-Facing Loop

1. Start from `Title_Menu`.
2. Continue or start a new local save.
3. Move in `Hub_Crossroads`.
4. Enter the available region gate.
5. Learn the exploration tool in a room.
6. Fight at least one readable enemy group.
7. Open one shortcut or route change.
8. Earn one reward beat.
9. Defeat the region boss.
10. Return to hub with confirmed progress.

## Scene List

| Scene | Required | Role |
| --- | --- | --- |
| `Bootstrap` | Yes | startup and persistent service load |
| `Title_Menu` | Yes | new game, continue, settings, quit |
| `PersistentSystems` | Yes | input, save, audio, scene flow |
| `UI_Game` | Yes | HUD, pause, settings, prompts |
| `Hub_Crossroads` | Yes | one safe hub and region gates |
| `Region_01_GreenRuins` | Yes | first region and vertical-slice target |
| `Region_02_SunkenWorks` | Yes | second region after Region 01 works |
| `Region_03_AshenKeep` | Yes | final region before finale |
| `Boss_04_Final` | Yes | final boss arena |

## Non-Negotiable Caps

| Category | Cap |
| --- | ---: |
| Hub | 1 |
| Regions | 3 |
| Bosses | 4 |
| Exploration tools | 1 |
| Save slots | 1 local save |
| Online systems | 0 |
| Inventory/crafting/quest systems | 0 |
| Open-world streaming systems | 0 |

## Historical Terms

The following words may appear in old files, but must not guide new D021
product work or player-facing copy:

- Gate A
- D020 as product name
- extraction
- unbanked reward loss
- hack-and-slash loot economy
- Echo Phase world-state switching
- co-op-ready
- compact open world
- MMO-style class or equipment systems
