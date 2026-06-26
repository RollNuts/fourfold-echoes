# D021 Art, Audio, And UI

Status: current presentation contract.

Art production may happen in a separate lane, but game systems must reserve room
for finished art, animation, VFX, audio, and UI from the start.

## Art Pillars

1. Readable from gameplay camera.
2. One tool, one visual language.
3. Compact regional contrast.

## Forbidden Visuals

- Photorealism.
- MMO armor/detail density.
- Gray blockout or default primitive art in public capture.
- Tall walls that hide combat.
- Thin clutter that looks interactable.
- Text labels as the only explanation.
- VFX covering enemy tells or player position.
- Open-world vista language.
- Co-op ownership rings or social markers.
- Region identity based only on post-process tint.
- Visuals implying extra tools, classes, crafting, or inventory.

## Art Budgets

| Asset Class | Cap | Materials | Textures | VFX | Animation Density |
| --- | ---: | ---: | ---: | ---: | ---: |
| Hero | 1 | 3 | 1024 max | 2 | 10 clips |
| Exploration tool | 1 | 2 | 512 max | 4 | 5 states |
| Normal enemy | 2 slice models | 2 each | 512 max | 2 each | 6 clips each |
| Bosses | 4 MVP max | 4 each | 2048 max | 5 each | 12 clips each |
| Hub kit | 12-18 props | shared atlas + 2 accents | 1024 atlas | 2 ambient | 2 moving props |
| Region kit | 16-24 props/region | shared atlas + 3 accents | 1024 atlas | 3 regional | 3 moving props |
| Gimmick objects | 6-10 total | 2 each | 512 max | 3 total | 4 states |
| Chest/reward | 1 chest family, 2 rewards | 2 each | 512 max | 2 | 3 clips |
| UI icons | 12 max | n/a | 256 max | 2 pulses | 2 states/icon |

## Naming Rules

| Type | Pattern | Example |
| --- | --- | --- |
| Prop | `FE_PROP_<AREA>_<NAME>_##` | `FE_PROP_R01_ToolPedestal_01` |
| Terrain | `FE_ENV_<AREA>_<TYPE>_##` | `FE_ENV_R02_FloorCracked_03` |
| Enemy | `FE_ENEMY_<ROLE>_<NAME>` | `FE_ENEMY_MELEE_Shardling` |
| Boss | `FE_BOSS_##_<NAME>` | `FE_BOSS_01_RootWarden` |
| VFX | `FE_VFX_<SOURCE>_<ACTION>` | `FE_VFX_Tool_Pulse` |
| Material | `FE_MAT_<AREA_OR_ROLE>_<SURFACE>` | `FE_MAT_R03_CrystalGlow` |
| Texture | `FE_TEX_<AREA_OR_ROLE>_<SURFACE>_<MAP>` | `FE_TEX_R01_Stone_ALB` |
| UI | `FE_UI_<PURPOSE>_<STATE>` | `FE_UI_Tool_Ready` |

Area codes: `HUB`, `R01`, `R02`, `R03`, `BOSS`, `COMMON`.

## Regional Color Script

| Area | Palette | Shapes | Lighting | Gameplay Read |
| --- | --- | --- | --- | --- |
| Hub | ivory, warm gold, soft blue | circular plaza, low walls, repaired stone | warm safe key, soft shadows | safety and orientation |
| Region 01 | moss green, pale stone, yellow flower accent | roots, rounded ruins, shallow slopes | bright adventure light | first tool use |
| Region 02 | rust red, charcoal, amber | broken tile, angled cliffs, metal frames | hard side light | shortcut pressure |
| Region 03 | deep blue, violet, cold white | crystals, narrow bridges, smooth dark stone | cool rim glow | late pressure |

## Minimum Quality Bar

- One enemy reads front, mass, attack origin, and danger tell from gameplay
  camera.
- One room shows floor, boundary, exit, hazard, interactable, and reward route
  without text labels.
- One tool pedestal has idle, active, and solved states.
- One chest or reward object has closed/open or hidden/revealed states.
- No public capture may use graybox or missing audio when claiming market value.

## Audio Pillars

1. Readable before beautiful.
2. Small palette, strong motif.
3. One tool sound family.
4. Reward without noise.

## Required SFX

| Category | Required Cues |
| --- | --- |
| UI | select, confirm, back, error, pause/menu open, save confirm |
| Player | footstep, attack, dodge, landing, light damage, heavy damage, death/retry |
| Combat | player swing, enemy hit, solid hit, enemy death |
| Enemy | notice, telegraph, attack release, damage |
| Tool | ready, pulse, near response, success, fail, cooldown ready |
| Gimmick | pedestal wake, mechanism move, lock release |
| Shortcut | shortcut unlock, gate open, route-confirm stinger |
| Reward | discovery stinger, reward reveal, pickup |
| Boss | intro, tell, impact, phase transition, stun/break, defeat resolve |

## BGM List

| Track | Role | Requirement |
| --- | --- | --- |
| `BGM_Hub` | safe return and preparation | required |
| `BGM_Region01` | first exploration region | required |
| `BGM_Region02` | danger and shortcut pressure | MVP |
| `BGM_Region03` | late mystery and tension | MVP |
| `BGM_NormalCombat` | optional reusable combat layer | optional |
| `BGM_Boss` | boss pressure foundation | required |
| `ST_DiscoveryReward` | reward stinger | required |

## Tool Audio Design

No continuous scanner loop. Visuals solve readability; audio improves
confidence.

| State | Sound |
| --- | --- |
| Ready/equip | short bright raise |
| Pulse/use | clear mid-frequency pulse synced to VFX |
| Near response | restrained ping/shimmer |
| Success | resolved upward chime plus object reaction |
| Fail/no target | short dry drop |
| Cooldown ready | small tick/glint |

## UI/UX Completion Bar

The vertical slice is not complete until the following can be used with keyboard
and controller:

- Title: new game, continue, settings, quit.
- Continue: resume region or return to hub safely.
- Hub: region gate prompt, mission briefing, result summary.
- HUD: HP, tool ready/cooldown, objective, boss HP, reward/progress beat.
- Pause: resume, settings, retry or return to title.
- Settings: master/music/SFX volume, UI scale, language, control hints.
- Save: visible confirmation after progress changes.
- Failure: clear retry path without losing input focus.
