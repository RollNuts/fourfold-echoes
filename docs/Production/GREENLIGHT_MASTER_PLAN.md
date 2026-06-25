# Greenlight Master Plan

Status: D-020 greenlight source.

This document is the one-stop production gate for the current FOURFOLD ECHOES
direction. It does not add features. It consolidates the current canonical
scope into the exact evidence needed to decide whether the game is worth taking
from prototype into full production.

External release constraints checked on 2026-06-26:

- Steam store page work requires store assets, screenshots, trailers, and other
  page data to be prepared before submission. Source:
  https://partner.steamgames.com/doc/store/page
- Steam graphical assets include required capsule assets, screenshots at
  minimum 1920x1080 16:9, and library assets. Source:
  https://partner.steamgames.com/doc/store/assets
- Steam trailer guidance recommends gameplay first, fast comprehension, and
  silent readability. Source:
  https://partner.steamgames.com/doc/store/trailer
- Steam Deck compatibility requires default controller access to all content,
  matching glyphs, a playable default configuration, and Deck resolution/text
  readability. Source:
  https://partner.steamgames.com/doc/steamhardware/compat

## 仕様固定メモ

FOURFOLD ECHOES is a single-player, Steam-first, buy-to-play top-down classic
action-adventure.

MVP hard ceiling:

| Item | Limit |
| --- | ---: |
| Hub | 1 |
| Handcrafted regions | 3 |
| Bosses | 4 |
| Exploration tools | 1 |
| Vertical-slice normal enemies | 2 |
| Vertical-slice minibosses | 1 |
| Vertical-slice bosses | 1 |
| Vertical-slice gimmick rooms | 2 |
| Vertical-slice relic rewards | 2 |
| Vertical-slice BGM | 2 tracks |

Rules:

- No inventory, crafting, quest log, social system, multiplayer, live service,
  backend dependency, survival loop, fishing, farming, or base-building.
- No open-map sprawl. Regions are compact authored spaces.
- The core fun is repeated mastery of movement, normal attack, dodge, and one
  exploration tool.
- Art and audio are part of the product, not polish debt.
- The first market slice must be understandable in 30 seconds of silent footage
  and fun enough to judge within 30 minutes.

## シーン一覧

| Scene | Purpose | Greenlight Requirement |
| --- | --- | --- |
| `Bootstrap` | starts app and persistent systems | loads without platform-specific services |
| `PersistentSystems` | input, save, audio, settings, scene flow | exists once; no gameplay content |
| `UI_Game` | HUD, pause, settings, controller prompts | no quest log or inventory screen |
| `Hub_Crossroads` | safe hub, region gates, save return | shows the whole game structure quickly |
| `Region_01_GreenRuins` | vertical-slice exploration area | includes two gimmick rooms and one shortcut |
| `Boss_01_GreenRuins` | slice boss arena | readable tells, retry, reward flag |
| `Region_02_SunkenWorks` | later region | same tool, different room layout language |
| `Region_03_AshenKeep` | final region before finale | same systems, higher pressure |
| `Boss_04_Final` | final boss | no new verbs introduced here |

Do not add separate scenes for systems outside the MVP.

## スクリプト責務一覧

| Script or Module | Responsibility | Must Not Own |
| --- | --- | --- |
| `GameBootstrap` | startup and first scene load | gameplay rules |
| `SceneFlowController` | hub/region/boss transitions | seamless-world streaming |
| `InputReader` | keyboard/controller action state | combat decisions |
| `TopDownPlayerMotor` | movement, facing, dodge displacement | damage and rewards |
| `TopDownCameraRig` | readable view, room framing, collision-safe angle | cinematic-only camera work |
| `PlayerCombat` | normal attack timing, damage intake, death/retry | combo trees |
| `ExplorationTool` | one tool input, pulse, active/cooldown state | multiple ability system |
| `ExplorationNode` | reveal/activate/expose response | quest or inventory logic |
| `RoomController` | room start, clear, reward, shortcut unlock | world progression storage |
| `EnemyController` | common enemy locomotion, tell, attack, death | boss state machine |
| `BossController` | boss attack states, health thresholds, defeat event | save serialization |
| `Damageable` | health, hit response, invulnerability windows | AI |
| `Hitbox` | attack volume timing and owner reference | health ownership |
| `ProgressState` | boss defeated, shortcut open, region unlock flags | item economy |
| `SaveService` | local versioned save/load | direct Steam or console APIs |
| `HudController` | health, tool state, prompt, boss health | minimap, quest log |
| `PauseMenuController` | resume, settings, quit | social or online UI |
| `AudioCueRouter` | route cue IDs to clips/mixer groups | composition system |

Keep implementations direct until two concrete rooms or enemies require shared
behavior. Do not abstract for hypothetical systems.

## データ設計

| Data Type | Required Fields | Limit |
| --- | --- | --- |
| `RegionDefinition` | `id`, `sceneName`, `unlockFlag`, `hubGateId`, `bossId` | 3 |
| `BossDefinition` | `id`, `sceneName`, `hp`, `attackSetId`, `defeatFlag`, `rewardId` | 4 |
| `EnemyDefinition` | `id`, `hp`, `moveSpeed`, `tellSeconds`, `attackId`, `sfxSetId` | slice uses 2 |
| `ExplorationNodeDefinition` | `id`, `nodeType`, `responseTargetId`, `vfxId`, `sfxId`, `saveFlag` | reveal/activate/expose only |
| `RoomDefinition` | `id`, `sceneName`, `enemySetId`, `nodeIds`, `rewardId`, `shortcutFlag` | slice uses 2 gimmick rooms |
| `RelicRewardDefinition` | `id`, `displayNameKey`, `effectId`, `presentationId`, `sfxId` | slice uses 2 |
| `AudioCueDefinition` | `id`, `category`, `clipPath`, `volume`, `priority`, `cooldownMs` | required cues only |
| `SaveData` | `version`, `currentScene`, `hubSpawnId`, `bossDefeated`, `shortcutsOpened`, `regionsUnlocked`, `relicFlags`, `settings` | no inventory records |

Data must be stable-ID based. Unity object names are not save identifiers.

## 実装順

| Order | Work | Acceptance |
| ---: | --- | --- |
| 1 | Product and scope lock | stale directions rejected by validation |
| 2 | Bootstrap, hub shell, Region 01 test room | app starts and loads controlled scenes |
| 3 | Player motor, camera, normal attack, dodge | controller and keyboard smoke pass |
| 4 | One enemy and hit loop | tell, hit confirm, death, retry are readable |
| 5 | Exploration tool and one node | tool visibly changes one route/object |
| 6 | Room controller, shortcut, save flags | shortcut state survives reload |
| 7 | Second gimmick room using same tool | proves depth without new systems |
| 8 | Enemy 2, miniboss, boss | 30-minute slice has escalation |
| 9 | Minimal UI and settings | Deck-readable HUD/pause/settings |
| 10 | BGM 2 and required SFX | no placeholder cues in slice capture |
| 11 | Art pass for hub/region/tool/enemy/boss | no gray-box market capture |
| 12 | QA/build/capture/store package | greenlight review can use real evidence |

## アートピラー

1. **Readable Stylized 3D**
   - Top-down silhouettes, color blocks, and lighting beat detail.
   - Low-poly leaning is allowed; primitive-looking construction is not.

2. **One Tool, One Iconic Read**
   - The exploration tool must be the strongest non-character silhouette.
   - Its idle, pulse, hit, fail, and solved states must read without captions.

3. **Compact Regional Contrast**
   - Hub and three regions must differ by color, shape, lighting, and prop
     families, not by post-process tint alone.

## 禁止事項

- Photorealism and noisy realism.
- MMO-level armor and prop detail density.
- Final or market-facing gray boxes.
- High walls that hide combat from top-down view.
- Important interactables that require text labels to identify.
- VFX that hides enemy tells or tool targets.
- New art language for systems outside the MVP.
- AI-looking generated fantasy noise without edited silhouettes and rights
  review.

## 予算表

| Category | Material Upper Bound | Texture Upper Bound | VFX Upper Bound | Animation Density |
| --- | ---: | ---: | ---: | --- |
| Hero | 3 | 1024 | tool glow only | idle, move, attack, dodge, hit, death |
| Exploration tool | 2 | 512 | 5 states | raise, pulse, hit, fail, ready |
| Normal enemy | 2 each | 512 | tell + hit + death | idle, move, tell, attack, hit, death |
| Miniboss | 3 | 1024 | 3 tells + defeat | 2-3 attacks only |
| Boss | 4 | 1024-2048 | 4 tells + transition + defeat | readable states, no cinematic excess |
| Room kit | shared atlas | 1024 | room-specific only | static or simple mechanical movement |
| Chest/relic | 2 each | 512 | beam + pickup | closed/open/pickup |
| UI | n/a | 256 icons | simple state pulses | no decorative animation loops |

## 命名規則

| Type | Pattern | Example |
| --- | --- | --- |
| Prop | `FE_PROP_<AREA>_<NAME>_##` | `FE_PROP_R01_RootGate_01` |
| Terrain | `FE_ENV_<AREA>_<TYPE>_##` | `FE_ENV_R02_FloorCracked_03` |
| Enemy | `FE_ENEMY_<ROLE>_<NAME>` | `FE_ENEMY_RANGED_GlintMote` |
| Boss | `FE_BOSS_##_<NAME>` | `FE_BOSS_01_RootWarden` |
| UI | `FE_UI_<PURPOSE>_<STATE>` | `FE_UI_Tool_Ready` |
| VFX | `FE_VFX_<SOURCE>_<ACTION>` | `FE_VFX_Tool_Pulse` |
| Material | `FE_MAT_<AREA_OR_ROLE>_<SURFACE>` | `FE_MAT_R03_CrystalGlow` |
| Texture | `FE_TEX_<AREA_OR_ROLE>_<SURFACE>_<MAP>` | `FE_TEX_R01_Stone_ALB` |
| Audio | `FE_AUD_<CATEGORY>_<ACTION>` | `FE_AUD_Tool_TargetHit` |

Area codes: `HUB`, `R01`, `R02`, `R03`, `BOSS`, `COMMON`.

## 地域別ルック表

| Area | Color Script | Shape Language | Lighting | Gameplay Read |
| --- | --- | --- | --- | --- |
| Hub | ivory, warm gold, soft blue | circular plaza, low walls, clean stone | soft warm key | safe return and orientation |
| Region 01 | moss green, pale stone, yellow flowers | roots, rounded ruins, shallow slopes | bright adventure light | first tool reads |
| Region 02 | rust red, charcoal, amber | broken tiles, angled cliffs, metal frames | hard side light | pressure and shortcut reads |
| Region 03 | deep blue, violet, cold white | crystals, bridges, smooth dark stone | cool contrast | late route reads and boss foreshadow |

## 最低品質基準

| Asset | Minimum Finish |
| --- | --- |
| 1 enemy | front, attack origin, tell, hit, and death state read at gameplay distance |
| 1 room | entrance, exit, enemy, tool target, hazard, reward route visible without labels |
| 1 gimmick pedestal | idle, targeted, activated, solved states visible and synced to tool |
| 1 chest | reward silhouette, open state, pickup feedback, not confused with scenery |
| Hero | readable facing, tool socket, non-capsule body, no generic mannequin feel |
| Exploration tool | iconic silhouette and glow visible in thumbnail |

## 制作フロー

1. **Gray Blockout**
   - Prove camera, scale, route, enemy spacing, and tool target.
   - Exit quickly; blockout cannot pass market validation.
2. **Style Pass**
   - Replace primary silhouettes, establish regional colors, remove clutter.
3. **Lighting Pass**
   - Lock region identity and 1280x800 readability.
4. **VFX Pass**
   - Add only gameplay-explaining effects first: tool, hit, tell, reward, boss.
5. **Final Polish**
   - Capture, review silently, fix ambiguous objects before adding more content.

省略可能:

- Tiny decorative props.
- Alternate costumes.
- Cloth simulation.
- High-frequency texture detail.
- Decorative VFX not tied to gameplay.

絶対に削らない:

- Hero read.
- Tool read and tool response.
- Enemy and boss tells.
- Shortcut opening visual.
- Reward/chest read.
- Region color/lighting distinction.
- Hit confirm and damage feedback.

## オーディオピラー

1. **Readable Before Beautiful**
   - Tells, hit confirm, damage, dodge, and tool responses outrank ambience.
2. **Few Themes, Strong Memory**
   - Small track list; memorable motif; no complex dynamic score in MVP.
3. **Tool-Centered Confidence**
   - Tool audio communicates ready, pulse, near response, hit, fail, success,
     and cooldown/ready.

## 必須SE一覧

| Category | Required Cues |
| --- | --- |
| UI | select, confirm, back, error, pause, settings apply |
| Player | footstep common, dodge, landing, damage, death/retry |
| Combat | normal swing, enemy hit, armor/shield hit, enemy death |
| Enemy | notice, telegraph, attack, damage, death |
| Boss | intro, tell, impact, transition, defeat |
| Exploration tool | equip/ready, pulse, near response, target hit, fail, cooldown ready |
| Gimmick | pedestal wake, mechanism move, shortcut open |
| Reward | chest open, relic appear, pickup, discovery stinger |
| Save | save success, load/continue |

Unneeded for MVP: voice acting, ambient dialogue, multiple weapon families,
crafting/inventory sounds, online/social sounds, region-specific UI variants.

## BGM一覧

| Track | Role | Slice Requirement |
| --- | --- | --- |
| `BGM_Hub` | safe return and motif | required |
| `BGM_Region01` | exploration motion | required or combined with hub if distinct enough |
| `BGM_Region02` | pressure | planned after slice |
| `BGM_Region03` | late mystery | planned after slice |
| `BGM_NormalCombat` | short combat layer or loop | optional if mix remains clear |
| `BGM_Boss` | boss identity/escalation | required |
| `BGM_DiscoveryReward` | short reward stinger | required as cue, not full track |

## 探索ツール音設計

| Tool State | Audio Rule |
| --- | --- |
| Ready | soft bright tick; avoid constant annoyance |
| Raise/equip | short confident gesture |
| Pulse | mid-frequency pulse synced to VFX |
| Near response | gentle ping, not a radar minigame |
| Target hit | upward resolved chime plus object reaction |
| Fail | dry low response, quick recovery |
| Cooldown ready | small return tick |

Principle: the game must be playable without sound, but audio should double
confidence, timing, and satisfaction.

## オーディオ実装優先順位

1. Tool pulse, target hit, fail, pedestal/shortcut response.
2. Normal attack, hit confirm, enemy tell, enemy death, player damage.
3. Boss tell, impact, transition, defeat.
4. UI select/confirm/back/error, pause, save.
5. Hub/exploration track and boss track.
6. Reward/discovery stinger.
7. Later region loops.

## マイルストーン完成条件

| Milestone | Done Means |
| --- | --- |
| M1 movement/combat | attack, hit, damage, dodge, enemy tell cues fire |
| M2 tool proof | pulse, success, fail, object activation are synced |
| M3 room proof | shortcut, chest, reward, save cues exist |
| M4 boss proof | boss tell/impact/transition/defeat and boss BGM exist |
| M5 market slice | no placeholder SFX, two BGM tracks mixed, loudness checked |

## 完成条件チェックリスト

- [ ] Hub 1 styled and playable.
- [ ] Exploration area 1 styled and playable.
- [ ] Normal enemies 2 types playable.
- [ ] Miniboss 1 playable.
- [ ] Boss 1 playable.
- [ ] Exploration tool modeled, animated, audible, and usable.
- [ ] Shortcut 1 opens and is saved.
- [ ] Gimmick rooms 2 use the same tool differently.
- [ ] Relic rewards 2 awarded without inventory complexity.
- [ ] Minimal UI: health, tool state, prompt, boss health, pause/settings.
- [ ] BGM 2 tracks integrated.
- [ ] Minimum SFX set integrated.
- [ ] Local save/load works for progress flags.
- [ ] 30 minutes or less communicates the core fun.
- [ ] 30 seconds of silent footage communicates the game.
- [ ] No gray-box art in market captures.
- [ ] No placeholder SFX in audio captures.
- [ ] Optimization and bug table updated.

## 1週間単位の実装順序

| Week | Goal | Output |
| ---: | --- | --- |
| 1 | Scope lock and playable base | movement, camera, attack, dodge, one enemy in a controlled room |
| 2 | Exploration tool proof | one node, one route/object change, tool VFX/SFX first pass |
| 3 | Two-room loop | two gimmick rooms, one shortcut, one reward, save flag |
| 4 | Combat escalation | enemy 2, damage/death/retry, combat SFX, readable tells |
| 5 | Miniboss and boss | one miniboss, one boss, boss BGM, retry and reward |
| 6 | Art replacement | hub/region/tool/enemy/chest/room meet minimum quality |
| 7 | Audio/UI/save hardening | two BGM tracks, required SFX, HUD/pause/settings, save regression |
| 8 | Greenlight package | Windows build, Deck-oriented capture, screenshots, trailer capture plan |

## 担当表

| Category | Owner | Must Deliver |
| --- | --- | --- |
| Programming | Lead Programmer | player, combat, tool, room flow, save, build |
| Game Design | Designer | room rules, enemy/boss reads, one-tool depth |
| Art Direction | Art Director | art pillars, style checks, screenshot readability |
| Technical Art | Tech Artist | budgets, materials, VFX, import validation |
| Audio | Audio Director | cue list, BGM 2, SFX gate, mix priorities |
| QA/Release | QA Lead | Deck, controller, save, boss blocker, crash tests |
| Marketing | Store Lead | copy, screenshots, trailers, announcement text |

## リスク表

| Risk | Severity | Mitigation |
| --- | --- | --- |
| One tool is not fun enough | High | prove two different rooms before adding systems |
| Combat reads as generic | High | boss/miniboss must test positioning, dodge, and tool timing |
| Art remains blockout | High | blockout cannot pass greenlight |
| Audio slips late | High | tool/combat SFX required before room acceptance |
| Scope creep | High | one in/two out; no new systems |
| Steam Deck failure | Medium | 1280x800, controller, glyph, and text checks from week 1 |
| Save corruption | Medium | versioned save, backup, corruption tests before capture |
| Trailer unclear | Medium | first 10 seconds must show hub/tool/combat/route consequence |

## 市場検証可能性メモ

This slice is market-testable because it produces the same evidence players see
first on Steam:

- screenshots that show hero, tool, room, enemy, boss, reward, shortcut, and
  region identity;
- a gameplay-first trailer that communicates the rule without audio;
- a 20-30 minute playable slice that tests whether one-tool mastery has depth;
- final-enough art and audio to avoid false negatives from placeholder feel.

Greenlight fails if a viewer cannot describe the game as a compact top-down
action-adventure built around one exploration tool after a silent 30-second
clip.

## 変更管理テンプレート

| Field | Entry |
| --- | --- |
| Change title |  |
| Problem solved |  |
| Trailer value | none / weak / strong |
| Play value | none / weak / strong |
| Implementation cost | low / medium / high |
| QA cost | low / medium / high |
| Existing item removed |  |
| Second item removed |  |
| Decision | accept / defer / reject |
| Decision owner |  |
| Date |  |

## 上限定義

| Element | Hard Limit |
| --- | ---: |
| Hub | 1 |
| Regions | 3 |
| Bosses | 4 |
| Exploration tools | 1 |
| Vertical-slice normal enemies | 2 |
| Vertical-slice minibosses | 1 |
| Vertical-slice bosses | 1 |
| Vertical-slice gimmick rooms | 2 |
| Vertical-slice relic rewards | 2 |
| New systems per milestone | 0 |

## 却下基準

Reject when any is true:

- It adds a prohibited system.
- It adds a second exploration tool.
- It does not improve trailer clarity.
- It does not improve the first 30 minutes.
- It increases QA scope more than play value.
- It lets art or audio remain placeholder longer.
- It delays the vertical slice.
- It solves a problem the one-tool loop should solve.

## 週次レビュー表

| Review | Question | Outcome |
| --- | --- | --- |
| 15-minute play | Can a player understand and improve? | keep / fix / cut |
| Screenshot | Can one image explain room/tool/enemy? | pass / fail |
| Audio | Are new interactions audible and non-placeholder? | pass / fail |
| Scope | Did anything exceed caps? | accept / cut |
| Bugs | Top 5 player-facing blockers? | next-week priority |
| Trailer beat | Did the week create footage value? | yes / no |

## 3分類表

| Classification | Items |
| --- | --- |
| 完成優先 | movement, camera, normal attack, dodge, one tool, two rooms, shortcut, save/load, required SFX |
| 改善優先 | enemy readability, boss tells, tool VFX, hub/region lighting, UI readability, BGM mix |
| 削除候補 | any second tool, extra regions, optional systems, decorative-only props, extra UI screens |

## Steam発売前チェックリスト

| Gate | Pass Evidence |
| --- | --- |
| Windows build launches from clean install | launch test log |
| Controller can access all gameplay and menus | controller test log |
| No launcher required | release checklist |
| 1280x800 and 1920x1080 layouts are readable | screenshot set |
| Save/load survives restart | save roundtrip test |
| Save corruption fallback works | corruption test |
| No placeholder art/audio in store captures | art/audio gate report |
| Store screenshots are real gameplay | screenshot manifest |
| Trailer is gameplay-first | trailer manifest |
| Licenses documented | legal/audio/art registers |
| Crash/support paths documented | support page |
| Build version visible | version capture |

## Steam Deckテスト表

| Item | Test | Pass |
| --- | --- | --- |
| Controller | physical controls access all content by default | no settings change required |
| Glyphs | active controller glyphs match input | no keyboard glyphs during controller play |
| Resolution | 1280x800 preferred, 1280x720 fallback | no HUD clipping |
| Text | minimum practical text size | target 12px+, never below compatibility minimum |
| Performance | default settings at 800p | target 30fps minimum for compatibility, higher for quality |
| Launcher | no external launcher | game starts directly |
| Suspend/resume | pause, suspend, resume | input and save state recover |
| Proton | Windows build on Deck | no platform warning or unsupported dialog |

## 回帰テスト表

| Area | Cases |
| --- | --- |
| Controller | move, attack, dodge, tool, interact, pause, settings |
| Resolution | 1280x800, 1280x720, 1920x1080, ultrawide safe check |
| Save corruption | missing save, old version, truncated save, invalid flags |
| Language | English/Japanese switch, missing string fallback |
| Crash | launch, scene load, boss retry, quit |
| Boss progression | death/retry, defeat flag, reward, reload after defeat |
| Tool | no target, valid target, solved target, reload solved state |
| Audio | required cues fire once in slice |
| UI | prompts, tool state, boss health, pause, save indicator |

## コンソール事前対応表

| Concern | Prepare Now |
| --- | --- |
| Input | action IDs and controller-first UI |
| Save | versioned save and platform-agnostic wrapper |
| Suspend/resume | clean pause/recover in all playable scenes |
| Performance | material, texture, mesh, VFX budgets |
| Display | safe area and 1280x800-first checks |
| Glyphs | glyph data separate from input action |
| Platform services | achievements/cloud behind adapters only |
| Localization | string IDs, font fallback, no baked text images |

## 30日運用計画

| Period | Focus | Rule |
| --- | --- | --- |
| Day 0-3 | P0 launch, crash, save, progression blockers | hotfix from shipped tag |
| Day 4-7 | controller, Deck, boss retry, readability | batch P1 fixes after smoke |
| Day 8-14 | frequent P1/P2 bugs, FAQ, small QoL | patch notes before upload |
| Day 15-30 | stability patch and known issues | no scope additions |

## ユーザー告知テンプレート

### Known Issue

We are investigating an issue where `<brief issue>`. Current status:
`investigating / fixed internally / fix scheduled`. If you encounter it, please
send your build version, input device, save file if possible, and the last room
or boss you entered.

### Hotfix Shipped

Hotfix `<version>` is live. This update fixes `<P0/P1 summary>`. Please restart
Steam to receive the patch. We continue prioritizing saves, progression,
crashes, controller play, and boss blockers first.

### Hotfix Delayed

The fix for `<issue>` needs more verification because it touches saves or
progression. We will not ship it until it passes regression. Current workaround:
`<workaround or none>`.

## バグ優先度表

| Priority | Definition | Examples |
| --- | --- | --- |
| P0 | cannot launch, save corruption, impossible progression | game cannot start, save deleted, boss door never opens |
| P1 | crash, boss/tool blocker, controller lockout | crash on region load, soft-lock, pause traps controller |
| P2 | serious readability/UI/audio/collision issue | Deck HUD clipped, required cue missing |
| P3 | typo, rare visual issue, minor polish | text typo, prop overlap |

## 短文紹介3案

1. A compact top-down fantasy action-adventure built around one mysterious tool,
   readable combat, shortcuts, relic rewards, and four bosses.
2. Explore three stylized regions from a small hub, solve tool-based rooms, open
   shortcuts, and defeat readable bosses.
3. A focused single-player adventure: one hub, one exploration tool, three
   regions, four bosses, and no filler systems.

## 長文紹介

FOURFOLD ECHOES is a compact single-player top-down fantasy action-adventure.

Leave a small hub, explore three handcrafted regions, master one mysterious
exploration tool, open shortcuts, claim relic rewards, and defeat four readable
bosses. The game is built around a small rule set used well: visible dangers,
clear rooms, positional combat, and secrets that can be understood from what
you see on screen.

No map sprawl, no live service, no crafting treadmill. The goal is a focused
premium adventure that feels good to play and is easy to read in motion.

## タグ優先順

1. Action-Adventure
2. Singleplayer
3. Adventure
4. Action
5. Exploration
6. Top-Down
7. Fantasy
8. Controller
9. Stylized
10. Indie
11. Puzzle
12. Atmospheric

Do not use tags that imply unsupported systems.

## スクリーンショット計画

| Shot | Content | Purpose |
| ---: | --- | --- |
| 1 | hub overview with three region gates | structure |
| 2 | hero holding/using exploration tool | core hook |
| 3 | Region 01 room before solution | readable problem |
| 4 | same room after tool response/shortcut | consequence |
| 5 | normal enemy combat with tell and dodge | action readability |
| 6 | miniboss tell and player positioning | escalation |
| 7 | boss arena with large attack tell | spectacle |
| 8 | chest/relic reward with minimal UI | reward feel |

All screenshots must be actual gameplay, 16:9, minimum 1920x1080 for Steam
store use, and free of gray-box art, debug UI, and unimplemented features.

## トレーラー絵コンテ

### 45秒版

| Time | Beat |
| --- | --- |
| 0-5s | hub, hero, region gates, title |
| 5-10s | exploration tool pulse changes a route/object |
| 10-17s | normal enemy fight: tell, dodge, hit |
| 17-24s | second room uses the same tool differently |
| 24-32s | shortcut opening and relic reward |
| 32-40s | boss tell, dodge, counterattack |
| 40-45s | title, Steam wishlist, demo/date if true |

### 75秒版

| Time | Beat |
| --- | --- |
| 0-8s | hub and three region mood shots |
| 8-16s | tool model, pulse VFX, response |
| 16-26s | two room problems, same tool |
| 26-38s | two enemies and miniboss escalation |
| 38-50s | shortcut and relic reward |
| 50-65s | boss pattern: tell, dodge, tool opening, punish |
| 65-75s | recap: compact adventure, one tool, three regions, four bosses |

Trailer rule: first listed trailer must be mostly gameplay from the actual
camera perspective. It must teach the game in under 10 seconds and still make
sense with no audio.

## 告知文

### Demo Announcement

FOURFOLD ECHOES is a focused top-down fantasy action-adventure built around one
exploration tool, readable combat, shortcuts, relic rewards, and handcrafted
boss rooms. The first demo will show the hub, one region, two gimmick rooms,
two enemy types, one miniboss, and one boss.

### Launch Announcement

FOURFOLD ECHOES is out now on Steam. Explore three compact fantasy regions from
a small hub, master one mysterious exploration tool, open shortcuts, find relic
rewards, and defeat four bosses in a focused single-player adventure.

## 翻訳優先リスト

| Priority | Locale Code | Language | Scope |
| ---: | --- | --- | --- |
| 1 | `en` | English | store and in-game source review |
| 2 | `ja` | Japanese | full store and in-game text, native review |
| 3 | `zh-CN` | Simplified Chinese | store first, UI next, font coverage check |
| 4 | `zh-TW` | Traditional Chinese | store if demand appears |
| 5 | `ko` | Korean | store if demand appears |
| 6 | `fr` | French | store later |
| 7 | `de` | German | store later |
| 8 | `es` | Spanish | store later |

Priority glossary: exploration tool, shortcut, relic reward, boss tell,
handcrafted region, top-down action-adventure.

## MVP範囲外一覧

- Any second exploration tool.
- Inventory, crafting, quest log, social systems.
- Online/co-op/PvP/matchmaking/accounts/backend/live service.
- Day/night cycle.
- Fishing, farming, base building, survival mechanics.
- Seamless large-world streaming.
- Procedural world generation as a product pillar.
- More than one hub, three regions, four bosses, or two slice relics.
- Complex combo strings as core combat.
- Market captures with gray-box art or placeholder SFX.
