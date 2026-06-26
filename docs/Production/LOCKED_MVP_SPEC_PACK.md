# Locked MVP Spec Pack

Status: required production brief.

This document supersedes older direction documents for current implementation.
Treat older notes about open worlds, extraction, co-op, loot economies, phase
systems, crafting, or live-service features as historical unless this file is
explicitly replaced.

## 仕様固定メモ

- Game: FOURFOLD ECHOES.
- Platform: Steam-first Windows PC. Steam Deck support is a launch target.
  Console ports may be prepared structurally, but are not launch scope.
- Business model: premium buy-to-play, offline single-player, no live service.
- Genre: top-down classic action-adventure.
- Scope: compact authored adventure. No open world.
- Fun source: repeated mastery of a small rule set.
- Core hook: one exploration tool used for navigation, gimmicks, shortcuts,
  combat openings, rewards, and boss reads.
- MVP cap: 1 hub, 3 regions, 4 bosses, 1 exploration tool.
- Forbidden from day one: inventory, crafting, quest log, social systems,
  multiplayer, online dependency, open world, day-night cycle, fishing, farming,
  base building, procedural world generation, second exploration tool.
- Visual target: stylized 3D, readable from top-down camera, not photoreal.
- Audio target: BGM and SFX are gameplay readability. They are not a late
  decoration pass.

## シーン一覧

| Scene | Purpose | MVP Rule |
| --- | --- | --- |
| `Title` | New game, continue, settings, quit | no feature claims beyond shipped scope |
| `Hub_Crossroads` | one hub, return point, three region gates, final gate | 1 hub maximum |
| `Region_01` | first region, combat basics, tool introduction, boss 1 | compact authored region |
| `Region_02` | tool mastery, shortcut pressure, boss 2 | no second tool |
| `Region_03` | combined combat/gimmick pressure, boss 3 | final normal region |
| `Final_Boss` | boss 4 and ending route | only after three regions clear |

Boss 1-3 may live inside their region scenes. `Final_Boss` may be separate for
loading, QA, and store capture clarity. Do not add separate towns, dungeons,
challenge modes, arena modes, social scenes, crafting rooms, or open-world maps.

## スクリプト責務一覧

| Script / Area | Owns | Must Not Own |
| --- | --- | --- |
| `GameFlowController` | title, hub, region, final boss scene transitions | streaming open-world logic |
| `SaveService` | versioned local save, flags, settings, corruption fallback | inventory/economy/account data |
| `PlayerController` | movement, facing, dodge, basic attack input | combo trees or build systems |
| `PlayerCombat` | hit detection, damage intake, invulnerability, death/retry | progression storage |
| `ExplorationToolController` | one tool use, cooldown, target query, success/fail event | multiple abilities |
| `ExplorationTarget` | target state, visual/audio response, solved flag | quest logic |
| `RoomController` | enemy locks, shortcut open, reward spawn | procedural world generation |
| `EnemyController` | normal enemy movement, tell, attack, hit, death | boss phases |
| `BossController` | boss health, pattern phases, openings, defeat event | save implementation |
| `RegionGate` | unlock condition and destination scene | quest log or map economy |
| `RewardRelic` | two vertical-slice rewards, visible pickup event | inventory screen |
| `CameraController` | top-down framing and region bounds | cinematic system |
| `UIController` | HUD, prompts, boss HP, pause/settings, save indicator | quest log, inventory, social UI |
| `AudioController` | BGM switching, required SFX routing, volume settings | music generation or middleware dependency |

Keep scripts direct and readable. Use interfaces only when a second real
implementation exists. Do not build a framework before the slice proves play.

## データ設計

| Data | Required Fields | Cap |
| --- | --- | --- |
| `SaveData` | `version`, `currentScene`, `spawnId`, `regionCleared[3]`, `bossDefeated[4]`, `shortcutOpened`, `toolOwned`, `settings` | one local save file |
| `RegionDefinition` | `id`, `sceneName`, `hubGateId`, `unlockFlag`, `bossId`, `colorScriptId` | 3 |
| `BossDefinition` | `id`, `sceneName`, `hp`, `patternSetId`, `defeatFlag`, `musicId` | 4 |
| `EnemyDefinition` | `id`, `role`, `hp`, `moveSpeed`, `telegraphSeconds`, `damage` | authored small set |
| `ExplorationTargetDefinition` | `id`, `targetType`, `responseType`, `solvedFlag`, `sfxId`, `vfxId` | one tool language |
| `RoomDefinition` | `id`, `sceneName`, `enemySetId`, `targetIds`, `shortcutFlag`, `rewardId` | authored rooms only |
| `RelicRewardDefinition` | `id`, `displayName`, `effectKind`, `presentationId` | 2 in vertical slice |
| `AudioCueDefinition` | `id`, `category`, `clipPath`, `volume`, `priority` | required categories only |

No item database, crafting recipe table, quest database, social data, loot
rarity table, or procedural biome data in MVP.

## 実装順

1. Player movement, camera, basic attack, damage, death/retry.
2. Two normal enemies with readable tells and deaths.
3. Hub to one region transition and local save/load.
4. One exploration tool with a visible target, fail feedback, success feedback,
   and one shortcut.
5. Two gimmick rooms using the same tool in different spatial layouts.
6. Two visible relic rewards without inventory UI.
7. One miniboss and one boss in Region 01.
8. Required UI: health, tool state, prompt, boss HP, pause/settings, save state.
9. Required audio: BGM 2 tracks and minimum SFX set.
10. Replace gray box in the vertical slice with stylized production-readable art.
11. Steam Deck readability and controller pass.
12. Windows build, store screenshots, 45-second trailer footage.

## スコープ外一覧

- Inventory, equipment collection, crafting, rarity loot, build economy.
- Quest log, branching dialogue, companions, social systems.
- Open world, seamless streaming, procedural map generation.
- Day-night cycle, weather simulation, fishing, farming, base building.
- Multiplayer, co-op, PvP, lobbies, online accounts, live events.
- Second exploration tool, skill trees, class/job system.
- More than 1 hub, 3 regions, or 4 bosses.
- Store claims for console release, online play, open world, or infinite freedom.

## アートピラー

1. **Readable Stylized 3D**: shapes, color blocks, lighting, and motion read at
   top-down camera distance. No gray capsules or primitive piles in public media.
2. **One Tool, One Visual Hook**: the exploration tool is the most recognizable
   non-character object. Valid targets, active use, success, and cooldown share
   one visual language.
3. **Compact Regional Contrast**: hub and three regions differ by color, shape,
   lighting, and prop families without implying open-world scale.

## 禁止事項

- Photorealism, MMO armor density, AI-looking detail noise.
- Tall walls that hide combat, tiny clutter that reads as false interaction.
- Important mechanics communicated only by text labels.
- VFX that covers enemy tells, boss weak points, or player position.
- Region identity solved only by global tint.
- Placeholder art, gray blockout, stick figures, or default materials in store
  captures.

## 予算表

| Asset Class | Vertical Slice Cap | Material Cap | Texture Cap | VFX Cap | Animation Cap |
| --- | ---: | ---: | ---: | ---: | ---: |
| Hero | 1 | 3 | 1024 | 2 | 10 clips |
| Exploration tool | 1 | 2 | 512 | 4 | 5 states |
| Normal enemy | 2 | 2 each | 512 | 2 each | 6 clips each |
| Miniboss | 1 | 3 | 1024 | 3 | 8 clips |
| Boss | 1 in slice / 4 MVP | 4 | 2048 max | 5 each | 12 clips each |
| Hub kit | 12-18 props | shared atlas + 2 accents | 1024 atlas | 2 ambient | 2 moving props |
| Region kit | 16-24 props | shared atlas + 3 accents | 1024 atlas | 3 regional | 3 moving props |
| Gimmick/chest/reward | 6-10 objects | 2 each | 512 | 3 total | 4 states |
| UI icons | 12 max | n/a | 256 | 2 UI pulses | 2 states/icon |

## 命名規則

| Category | Pattern | Example |
| --- | --- | --- |
| Prop | `FE_PROP_<AREA>_<NAME>_##` | `FE_PROP_R01_ToolPedestal_01` |
| Terrain | `FE_ENV_<AREA>_<TYPE>_##` | `FE_ENV_R02_FloorCracked_03` |
| Enemy | `FE_ENEMY_<ROLE>_<NAME>` | `FE_ENEMY_MELEE_Shardling` |
| Boss | `FE_BOSS_##_<NAME>` | `FE_BOSS_01_RootWarden` |
| UI | `FE_UI_<PURPOSE>_<STATE>` | `FE_UI_Tool_Ready` |
| VFX | `FE_VFX_<SOURCE>_<ACTION>` | `FE_VFX_Tool_Pulse` |
| Material | `FE_MAT_<AREA_OR_ROLE>_<SURFACE>` | `FE_MAT_R03_CrystalGlow` |
| Texture | `FE_TEX_<AREA_OR_ROLE>_<SURFACE>_<MAP>` | `FE_TEX_R01_Stone_ALB` |

Area codes: `HUB`, `R01`, `R02`, `R03`, `BOSS`, `COMMON`.

## 地域別ルック表

| Area | Color | Shape | Lighting | Read Goal |
| --- | --- | --- | --- | --- |
| Hub | ivory, warm gold, soft blue | circular plaza, low walls, repaired stone | warm safe key, soft shadows | return, safety, orientation |
| Region 01 | moss green, pale stone, yellow accent | roots, rounded ruins, shallow slopes | clear adventure light | basic tool and enemy learning |
| Region 02 | rust red, charcoal, amber | broken tile, angled cliffs, metal frames | harder side light | shortcut pressure and combat |
| Region 03 | deep blue, violet, cold white | crystals, narrow bridges, dark smooth stone | cool rim glow | late route reads and final pressure |

## 最低品質基準

| Object | Acceptance |
| --- | --- |
| Enemy 1 | readable body/front/attack source; idle, move, tell, attack, hit, death |
| Room 1 | walkable floor, boundary, exit, target, reward route visible without labels |
| Gimmick pedestal 1 | idle/active/solved states visually different and tool-linked |
| Chest 1 | closed/open states, reward glow, not confused with scenery |
| Hero | facing and tool socket readable at gameplay camera distance |
| Tool | silhouette, glow, target response, success burst readable in 30-second trailer |

## 制作フロー

1. **灰色ブロックアウト**: prove camera, room scale, route, enemy spacing,
   tool target. Internal only.
2. **スタイル化**: replace major silhouettes, lock color/value hierarchy.
3. **ライティング**: prove hub and Region 01 at 1280x800 and 1920x1080.
4. **VFX**: add tool pulse, target response, hit confirm, damage, reward, boss tell.
5. **最終磨き**: remove clutter, fix contrast, capture store candidates, remove placeholders.

省略可能: tiny decoration, costume variants, cloth sim, high-frequency texture
detail, purely decorative ambient VFX. 削ってはいけない: player readability,
enemy tells, tool target state, reward visibility, boss tell, route/shortcut read.

## オーディオピラー

1. **Readable Before Beautiful**: tells, damage, tool response, and rewards must
   be readable in play.
2. **Compact Palette, Strong Motif**: few memorable loops and reusable stingers.
3. **One Tool Sound Family**: ready, pulse, near response, success, fail, ready return.

## 必須SE一覧

| Category | Required Cues |
| --- | --- |
| UI | select, confirm, back, error, pause, save |
| Player | step, attack, dodge, hit, damage, death/retry |
| Enemy | notice, telegraph, attack release, hit, death |
| Tool | ready, pulse, near response, fail, success, cooldown ready |
| Gimmick | pedestal wake, mechanism move, lock release, shortcut open |
| Reward | discovery stinger, chest open, relic pickup |
| Boss | intro, tell, impact, phase transition, stun/break, defeat |

不要: voice acting, co-op notifications, loot-rarity explosions, crafting/trade
sounds, large ambience libraries, adaptive middleware dependency.

## BGM一覧

| Track | Use | Requirement |
| --- | --- | --- |
| `BGM_Hub` | safety, return, planning | required |
| `BGM_Region01` | first exploration | vertical slice required |
| `BGM_Region02` | danger/shortcut pressure | MVP |
| `BGM_Region03` | final-region tension | MVP |
| `BGM_NormalCombat` | reusable combat lift | optional if Region loop carries combat |
| `BGM_Boss` | boss pressure and escalation | vertical slice required |
| `ST_DiscoveryReward` | reward/shortcut stinger | required |

## 探索ツール音設計

| State | Sound | Purpose |
| --- | --- | --- |
| Ready | short bright lift | confirms availability |
| Pulse/use | mid-frequency pulse synced to VFX | confirms action timing |
| Near response | restrained ping | confidence, not puzzle solution |
| Success | resolved chime + target reaction | correct use and reward |
| Fail | short dry drop | experimentation feedback |
| Ready return | tiny tick/glint | cooldown clarity |

Principle: muted play must still work. Audio doubles confidence and feel; it
must not be the only way to understand a mechanic.

## 実装優先順位

1. Tool pulse/response/success/fail.
2. Combat attack/hit/tell/death/damage/dodge.
3. Boss tell/impact/transition/defeat and boss BGM.
4. UI/select/save/pause.
5. Shortcut/reward/chest cues.
6. Hub, Region 01, boss music for market capture.

## マイルストーン完成条件

| Milestone | Audio Done Means |
| --- | --- |
| M1 Combat | attack, hit, enemy tell, enemy death, player damage, dodge implemented |
| M2 Tool | ready, pulse, near, success, fail, cooldown synced to VFX |
| M3 Room | mechanism, shortcut, discovery, reward, save cues exist |
| M4 Boss | intro, tell, impact, transition, break, defeat, boss BGM exist |
| M5 Market Slice | hub, Region 01, boss BGM, core SFX, reward stinger mixed |

## 完成条件チェックリスト

- [ ] Hub 1 exists and starts the game loop.
- [ ] Exploration area 1 exists and is not gray-box in market capture.
- [ ] Two normal enemies work.
- [ ] One miniboss works.
- [ ] One boss works.
- [ ] One exploration tool works in movement/gimmick and combat-opening context.
- [ ] One shortcut opens and persists.
- [ ] Two gimmick rooms use the same tool differently.
- [ ] Two relic rewards can be claimed and persisted.
- [ ] Minimal UI: HP, tool state, prompt, boss HP, pause/settings, save indicator.
- [ ] BGM 2 tracks are in-game.
- [ ] Minimum SFX set is in-game.
- [ ] Save/load roundtrip works.
- [ ] 30 minutes or less communicates the core fun.
- [ ] 30 seconds of footage communicates the game without voiceover.
- [ ] Optimization and bug table exist before market test.

## 実装順序

| Week | Goal | Output |
| --- | --- | --- |
| 1 | player, enemy, hub-region flow, save skeleton | playable loop without art polish |
| 2 | one tool, two gimmick rooms, shortcut, reward | tool mastery proof |
| 3 | miniboss, boss, death/retry, UI | combat/adventure slice |
| 4 | production-readable art for slice | no gray-box capture |
| 5 | BGM/SFX minimum and mix pass | no placeholder audio capture |
| 6 | controller, Deck layout, save corruption, build | release-candidate proof |
| 7 | store screenshots, trailer beats, bug fixing | market test package |

## 担当表

| Category | Owns |
| --- | --- |
| Engineering | player, enemies, boss logic, tool, save, scene flow |
| Design | room layouts, boss patterns, reward placement, difficulty |
| Art/TA | hero, tool, enemy, boss, room, chest, VFX, lighting |
| Audio | BGM, required SFX, mix, loudness, missing-cue checks |
| QA/Release | regression, Deck, controller, build, crash/save tests |
| Marketing | screenshots, trailer, Steam copy, localization priority |

## リスク表

| Risk | Impact | Countermeasure |
| --- | --- | --- |
| One tool feels shallow | core hook fails | require movement, gimmick, shortcut, and boss-use cases |
| Boss count expands | production stalls | 4 boss hard cap, 1 slice boss first |
| Regions grow too large | compact promise breaks | authored room budget per region |
| Art/audio delayed | market test invalid | no placeholder capture gate |
| Save bugs | launch blocker | versioned save and corruption tests |
| Controller friction | Steam Deck failure | controller-first weekly checks |
| Store overpromises | refund/review risk | claim check against implemented features |

## 市場検証可能性メモ

The slice can test saleability because it shows the actual promise: compact
top-down action-adventure, one clear exploration tool, readable combat, a
shortcut, a reward, and a boss. It avoids unverifiable scope promises. The
market question is not "is there enough content yet"; it is "does this small
rule set feel good enough that players want a polished 3-region version?"

## 変更管理テンプレート

```text
Change:
Reason:
Trailer value:
Play value:
Implementation cost:
What two things are removed if accepted:
Affected cap:
Decision: accept / reject / defer
Owner:
Review date:
```

## 上限定義

| Element | Hard Cap |
| --- | ---: |
| Hub | 1 |
| Regions | 3 |
| Bosses | 4 |
| Exploration tools | 1 |
| Normal enemies in vertical slice | 2 |
| Minibosses in vertical slice | 1 |
| Relic rewards in vertical slice | 2 |
| Gimmick rooms in vertical slice | 2 |
| BGM tracks in vertical slice | 2 full loops + reward stinger |

## 却下基準

Reject if the proposal:

- adds a forbidden system;
- needs a new UI screen resembling inventory/crafting/quest/social;
- increases region, boss, hub, or tool caps;
- cannot explain trailer value in one sentence;
- improves breadth while art/audio/readability remain unfinished;
- cannot name two deletions that pay for the addition.

## 週次レビュー表

| Week | Complete First | Improve Next | Delete Candidate | Evidence |
| --- | --- | --- | --- | --- |
| W1 | movement/combat/save | camera feel | extra menus | playable capture |
| W2 | tool/gimmick/shortcut | tool feedback | second mechanic | room capture |
| W3 | boss/retry/UI | tells and tuning | enemy variant | boss capture |
| W4 | art/audio slice pass | lighting/mix | decorative clutter | store screenshot |

## 3分類表

| 完成優先 | 改善優先 | 削除候補 |
| --- | --- | --- |
| hub, Region 01, one tool, two enemies, miniboss, boss, rewards, save | combat feel, camera, tells, lighting, audio mix, controller | extra enemies, second tool, broad map, optional modes, decorative-only props |

## Steam発売前チェックリスト

- [ ] Windows build launches from clean install.
- [ ] Steam Deck 1280x800 UI is readable.
- [ ] Controller can complete game.
- [ ] Keyboard can complete game.
- [ ] Save/load survives restart.
- [ ] Corrupted save falls back safely.
- [ ] Language switch does not break critical UI.
- [ ] No crash in launch, region transition, boss retry, quit.
- [ ] No boss progression softlock.
- [ ] No placeholder art/audio in store captures.
- [ ] Licenses and credits are complete.
- [ ] Store copy matches implemented features.

## Steam Deckテスト表

| Test | Pass |
| --- | --- |
| 1280x800 gameplay | no clipped HUD or unreadable prompts |
| default controller | all actions reachable |
| suspend/resume | state and input recover |
| offline launch | no online dependency |
| save roundtrip | progress survives restart |
| 30-minute thermal/stability | no crash or major input loss |

## 回帰テスト表

| Area | Cases |
| --- | --- |
| Controller | move, attack, dodge, tool, interact, pause, disconnect/reconnect |
| Resolution | 1280x800, 1920x1080, fullscreen/windowed |
| Save | missing, old, truncated, quit during save |
| Language | Japanese/English/Simplified Chinese critical UI |
| Crash | launch, scene transition, retry loop, quit |
| Boss | defeat, retry, simultaneous death, reload after defeat |
| Tool | no target, valid target, solved target, reload solved |

## コンソール事前対応表

| Concern | Prepare Now |
| --- | --- |
| Input | action IDs independent from device glyphs |
| Save | versioned local save service |
| Suspend/resume | pause-safe scene flow |
| Performance | texture/material/VFX budgets |
| UI | safe-area and 1280x800-first layouts |
| Platform APIs | wrappers only; gameplay never calls Steam directly |

## 30日運用計画

| Period | Focus |
| --- | --- |
| Day 0-3 | P0 launch/save/progression/crash fixes |
| Day 4-7 | controller, Deck, boss retry, major readability |
| Day 8-14 | P1/P2 clusters and known-issues updates |
| Day 15-30 | stability patch, no new scope |

## ユーザー告知テンプレート

```text
Update <version> is live. This patch focuses on stability, saves, controller
play, and progression fixes. We are prioritizing launch failures, save issues,
crashes, and boss/region blockers before balance or polish requests.
```

## バグ優先度表

| Priority | Definition |
| --- | --- |
| P0 | cannot launch, save corruption, progression impossible |
| P1 | crash, boss/tool blocker, controller lockout |
| P2 | serious readability, UI clipping, missing key SFX, common collision issue |
| P3 | typo, rare visual issue, minor polish |

## 短文紹介3案

1. A compact top-down action-adventure about mastering one tool, opening hidden
   routes, and defeating four handcrafted bosses.
2. Explore three stylized regions, solve readable tool-based rooms, and fight
   bosses in a focused single-player adventure.
3. One hub. Three regions. One tool. Four bosses. A small adventure built around
   clean combat, shortcuts, and mastery.

## 長文紹介

FOURFOLD ECHOES is a compact single-player top-down action-adventure. From one
hub, explore three handcrafted regions, learn a single exploration tool, open
shortcuts, uncover relic rewards, and defeat four bosses. The game avoids
crafting, inventory sprawl, open-world bloat, and online requirements. It is
built around readable combat, clear routes, tool-based room solutions, and a
small adventure polished enough to finish.

## タグ優先順

1. Action-Adventure
2. Singleplayer
3. Top-Down
4. Adventure
5. Action
6. Controller
7. Boss Fight
8. Exploration
9. Stylized
10. Puzzle

## スクリーンショット計画

| # | Shot |
| --- | --- |
| 1 | hub view with player, region gate, tool readable |
| 2 | Region 01 traversal with tool target |
| 3 | normal enemy combat with clear tell |
| 4 | first gimmick room before tool use |
| 5 | shortcut opening after tool use |
| 6 | relic reward/chest moment |
| 7 | miniboss or boss tell/read moment |
| 8 | boss defeat or region completion screen |

## トレーラー絵コンテ

### 45秒版

| Time | Shot |
| --- | --- |
| 0-5s | hub, player, three region gates |
| 5-10s | move/attack/dodge against normal enemy |
| 10-18s | tool pulse reveals or activates route |
| 18-25s | shortcut opens, chest/reward appears |
| 25-35s | miniboss/boss tells and player response |
| 35-42s | boss defeat, hub return, progress visible |
| 42-45s | title and Steam demo/wishlist CTA |

### 75秒版

Add clearer region contrast shots, one extra gimmick room, one death/retry beat,
and one before/after route shot. Do not add unimplemented features.

## 告知文

### Demo

FOURFOLD ECHOES demo is a focused slice of a compact top-down action-adventure:
enter from the hub, master one exploration tool, open a shortcut, claim relic
rewards, and fight a boss. No online account required.

### Launch

FOURFOLD ECHOES is out now on Steam. Explore three handcrafted regions from one
hub, master one exploration tool, open shortcuts, and defeat four bosses in a
single-player adventure built for clear combat and tight scope.

## 翻訳優先リスト

| Priority | Text |
| --- | --- |
| P0 | Steam short description, capsule tagline, CTA |
| P0 | controls, prompts, pause/settings, save messages |
| P0 | boss/region/reward names shown in UI |
| P1 | long description, update posts, demo announcement |
| P1 | trailer captions and screenshot captions |
| P2 | support FAQ and known-issues templates |

Languages: Japanese, English, Simplified Chinese. Translate only shipped claims.
