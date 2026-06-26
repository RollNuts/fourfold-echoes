# D022 Art And Audio Direction

## アートピラー

1. **遠距離で読めるスタイライズ3D**
   - Top-down camera distance decides the art. Shape, color planes, glow, and
     movement matter more than small surface detail.
   - Low-poly leaning is valid; unfinished primitives are not.

2. **1探索ツールが主役**
   - The exploration tool is the clearest non-character object.
   - Its idle, ready, active, success, fail, and cooldown states must read in a
     still screenshot.

3. **3地域+1ハブの即時識別**
   - Hub and three regions differ by color, shape language, lighting, floor
     material, and prop family.
   - Region identity cannot be solved by a post-process tint.

4. **無音トレーラー可読性**
   - Tool use, target success, shortcut opening, reward, damage, and boss
     transition must be understood without audio.

5. **低予算の完成感**
   - Fewer assets, better finished.
   - No market-facing gray boxes, capsules, stick figures, or raw kitbash.

## 禁止事項

- Photorealism.
- MMO armor/detail density.
- Hack-and-slash loot spray visuals.
- Open-world vistas, mount-scale landmarks, or distant biome promises.
- Important information communicated only by text labels.
- VFX that hides enemy tells or hazard areas.
- Tall walls that hide combat.
- Thin decorative clutter that cannot be read from top-down view.
- Region palettes that differ only by full-screen tint.
- Final player, enemy, boss, tool, reward, or UI assets made from raw primitives.
- Art language implying additional exploration systems.

## 予算表

| Class | MVP Upper Bound | Geometry / Animation | Material / Texture | VFX / Audio Hook |
| --- | ---: | --- | --- | --- |
| Player | 1 model | <= 8k tris, <= 10 core clips | 3 materials, 1024px set | footstep/dodge/hit sync |
| Exploration tool | 1 tool | <= 2k tris, <= 6 states | 2 materials, 512px | 4 VFX, 6 SFX |
| Normal enemies | 4 max MVP, 2 in slice | <= 5k tris each, <= 6 clips | 2 materials, 512px | tell SFX required |
| Bosses | 4 max MVP, 1 in slice | <= 16k tris each, <= 12 clips | 4 materials, 1024px; final boss may use 2048px | transition SFX required |
| Hub kit | <= 15 props | low walls, plaza, gates | 1024px atlas + 2 accents | small ambience only |
| Region kit | <= 18 props per region | shared base shapes | 1024px atlas + 3 accents | 1 BGM per region |
| Interactables/rewards | <= 12 objects | shape changes by state | 512px centered | success/open/reward SFX |
| VFX | <= 16 slice effects | tool, hit, hazard, reward | 256-512px flipbooks | short lifetime |
| BGM | 5 loops + optional combat | hub/R01/R02/R03/boss | 60-120 sec loops | no room-by-room BGM |
| SFX | <= 41 cue IDs | <= 65 files with variants | 48kHz WAV source | priority mix |

## 命名規則

| Type | Pattern | Example |
| --- | --- | --- |
| Common assets | `FE_<TYPE>_<AREA>_<NAME>_<STATE_OR_##>` | `FE_PROP_R01_RootGate_01` |
| Tool | `FE_TOOL_COMMON_<NAME>_<STATE>` | `FE_TOOL_COMMON_PulseLantern_Active` |
| Enemy | `FE_ENEMY_<ROLE>_<NAME>` | `FE_ENEMY_MELEE_Shardling` |
| Boss | `FE_BOSS_##_<NAME>` | `FE_BOSS_01_RootWarden` |
| VFX | `FE_VFX_<SOURCE>_<ACTION>` | `FE_VFX_Tool_Pulse` |
| SFX | `FE_SFX_<GROUP>_<ACTION>_##` | `FE_SFX_Tool_Pulse_01` |
| BGM | `FE_BGM_<AREA>_<ROLE>_Loop` | `FE_BGM_R02_Pressure_Loop` |
| Stinger | `FE_ST_<EVENT>_<NAME>` | `FE_ST_Reward_Discovery` |

Area codes: `HUB`, `R01`, `R02`, `R03`, `COMMON`, `BOSS`.

State suffixes: `_Idle`, `_Ready`, `_Active`, `_Solved`, `_Open`, `_Closed`,
`_Hit`, `_Break`, `_Loop`.

Filenames must be ASCII, no spaces, and must not include prototype names,
deprecated feature names, local paths, or tool prompts.

## 地域別ルック表

| Area | Color | Shape | Lighting | Read |
| --- | --- | --- | --- | --- |
| HUB | ivory, warm gold, soft blue | circular plaza, low walls, repaired stone | soft warm light | safety, return, preparation |
| R01 | moss green, pale stone, yellow flowers | roots, round ruins, shallow water | bright adventure light | first tool targets, basic enemies |
| R02 | rust red, charcoal, amber | cracked tile, angled cliffs, metal frames | hard side light | shortcut and combat pressure |
| R03 | deep blue, violet, cold white | crystals, narrow bridges, smooth dark stone | cool rim light | late route reads, boss foreshadowing |

Tool targets keep one shared accent across all regions. Floors must be lower
contrast than enemies, hazards, rewards, and tool targets.

## 最低品質基準

| Target | Acceptance |
| --- | --- |
| Enemy | front, attack source, tell, hit, and death read from gameplay distance |
| Room | entrance, exit, hazard, target, reward, and route change read without labels |
| Gimmick pedestal | idle, active, solved states differ by shape/light/motion |
| Chest | closed/open state and reward identity visible from camera distance |
| Tool | idle, use, success, fail, and ready return are readable visually |
| Boss | danger source, opening, phase/transition, defeat are visible without audio |
| Trailer capture | 30 seconds muted communicates tool, room change, combat, reward, boss |

## 制作フロー

1. **灰色ブロックアウト**
   - Validate camera distance, room scale, routes, combat spacing, and tool
     targets. Not allowed for public capture.
2. **スタイル化**
   - Replace major shapes with authored silhouettes and regional color planes.
3. **ライティング**
   - Establish hub/R01 readability at 1280x800 and 1920x1080.
4. **VFX**
   - Add tool pulse, target reaction, hit confirm, hazard tell, reward, boss
     transition. Effects support readability; they do not cover it.
5. **最終磨き**
   - Remove clutter, tune contrast, capture screenshots, mute-review video, fix
     unclear beats.

## 省略可能な表現

- Tiny decoration.
- Extra costumes.
- Cloth simulation.
- High-frequency texture detail.
- Decorative ambient VFX.
- Unique mesh for every small prop.
- Non-gameplay animals or background animation.
- Cinematic-only lighting that differs from playable rooms.

## 絶対に削ってはいけない表現

- Player readability.
- Tool silhouette, glow, response VFX, and state language.
- Enemy tells.
- Boss danger shape and opening.
- Shortcut opening visual.
- Reward/chest readability.
- Hub/R01/R02/R03 color and lighting distinction.
- Hit confirm and damage feedback.
- One polished representative enemy, room, gimmick pedestal, and chest before
  market capture.

## オーディオピラー

1. **音はゲーム価値**
   - Audio reads action, contact, danger, discovery, and achievement.
2. **美しさより可読性**
   - Enemy tell, boss tell, and player damage outrank music.
3. **小さく強い音色設計**
   - Use fewer memorable cues. Do not build a large room-specific audio library.
4. **1探索ツール=1音響ファミリー**
   - Ready, pulse, near, success, fail, cooldown use one sound language.
5. **無音でも成立、音ありで倍増**
   - Mechanics cannot require audio, but audio should double confidence and feel.

## 必須SE一覧

| Group | Cue IDs | Count |
| --- | --- | ---: |
| UI | select, confirm, back, error, pause, save_confirm | 6 |
| Player | footstep_common, dodge, landing | 3 |
| Combat contact | player_swing, enemy_hit, armor_hit, player_damage_light, player_damage_heavy, enemy_death | 6 |
| Enemy tell | enemy_notice, enemy_tell, enemy_attack_release, enemy_damage | 4 |
| Exploration tool | tool_ready, tool_pulse, tool_near_response, tool_target_hit, tool_fail, tool_cooldown_ready | 6 |
| Mechanism | pedestal_wake, mechanism_move, lock_release, shortcut_open | 4 |
| Reward | chest_open, relic_appear, pickup, discovery_stinger | 4 |
| Boss | boss_intro_hit, boss_tell, boss_impact, boss_phase_transition, boss_stun_break, boss_defeat | 6 |
| System | load_continue, settings_apply | 2 |

Total cap: 41 cue IDs. Variants max 3 per important cue; low-priority UI/system
cues use 1 variant.

## BGM一覧

| Track | Length | Role | Required |
| --- | ---: | --- | --- |
| `FE_BGM_HUB_Return_Loop` | 60-90 sec | return, safety, preparation | yes |
| `FE_BGM_R01_Field_Loop` | 75-105 sec | first exploration | yes |
| `FE_BGM_R02_Pressure_Loop` | 75-105 sec | combat pressure and shortcut | yes |
| `FE_BGM_R03_FinalRoute_Loop` | 75-105 sec | late tension | yes |
| `FE_BGM_BOSS_Core_Loop` | 90-120 sec | boss foundation for 4 bosses | yes |
| `FE_BGM_COMMON_Combat_Loop` | 45-60 sec | optional normal combat lift | optional |
| `FE_ST_Reward_Discovery` | 2-4 sec | reward/discovery stinger | yes |

## 探索ツール音設計

Tool working name: `PulseLantern`. Sound palette: mid-frequency pulse, light
metal/glass, restrained low end.

| State | Sound | Visual Sync |
| --- | --- | --- |
| Ready | short rising glint | tool small glow |
| Pulse | clear one-shot pulse | circular/sector glow |
| Near | restrained ping, >= 1.2 sec spacing | target weak blink |
| Success | resolved upward chime | target shape/light change |
| Fail | short dry downbeat | pulse dissipates |
| Cooldown ready | small tick/glint | tool reglow |

No loud scanning loops. No audio-only hidden target design.

## 実装優先順位

1. Tool VFX, 6 tool SFX, and target state change.
2. Player action SFX, enemy tell, hit, damage, death.
3. One room mechanism, shortcut, reward, discovery stinger.
4. Hub/R01 look and BGM.
5. One boss tell, transition, defeat, and boss BGM.
6. R02/R03 kits and BGM.
7. Four-boss differences, final mix, trailer capture.

## マイルストーン完成条件

| Milestone | Audio/Art Completion |
| --- | --- |
| M1 direction lock | caps, naming, regional palette, cue list fixed |
| M2 tool proof | tool states readable muted; 6 SFX synced |
| M3 room proof | mechanism, shortcut, reward read without labels and placeholder sound |
| M4 combat proof | 2 normal enemies read tells/contact/death |
| M5 boss proof | 1 boss has intro, tell, transition, opening, defeat, BGM |
| M6 MVP | 1 hub, 3 regions, 4 bosses, 1 tool; no placeholders |
| M7 market capture | 60 sec trailer reads muted and feels better with audio |
