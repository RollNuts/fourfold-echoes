# MVP Blueprint

Status: canonical after D-020.

## 仕様固定メモ

- FOURFOLD ECHOES is a single-player top-down classic action-adventure.
- First platform is Steam Windows. Keep architecture portable for later console
  work, but do not build console-specific systems yet.
- Business model is premium buy-to-play.
- No always-online requirement, live service, accounts, multiplayer, or backend.
- Scope is compact. Do not make an open world.
- The MVP limit is 1 hub, 3 regions, 4 bosses, and 1 exploration tool.
- The core fun is repeated mastery of a small rule set.
- The one exploration tool is the central verb. It can reveal, activate, or
  expose, but it must not become an inventory, crafting, or ability-tree system.
- Combat is readable and positional. Do not add combo strings as the core.
- Art is stylized 3D. Photorealism, MMO clutter, and gray-box final output are
  invalid.
- Music and SFX are part of the product feel from the first playable milestone.

## 新規プロジェクトのフォルダ構成

Keep the Unity project shallow, readable, and PR-friendly. Use folders by
ownership and gameplay purpose, not by broad future engine abstractions.

```text
Assets/
  Scenes/
    Bootstrap.unity
    Hub_Crossroads.unity
    Region_01_GreenRuins.unity
    Region_02_SunkenWorks.unity
    Region_03_AshenKeep.unity
    Boss_04_Final.unity
  Scripts/
    Core/
    Player/
    Combat/
    ExplorationTool/
    Rooms/
    Enemies/
    Bosses/
    Save/
    UI/
    Audio/
  Art/
    Characters/
    Enemies/
    Environment/
    Props/
    VFX/
    UI/
    Generated/
  Audio/
    BGM/
    SFX/
    Generated/
  Prefabs/
    Player/
    Enemies/
    Bosses/
    Rooms/
    Interactables/
    UI/
  Settings/
```

Repository support folders:

```text
docs/
game-spec/
commands/
Scripts/Validation/
Scripts/Build/
tools/Blender/
tools/AssetPipeline/
tools/AudioPipeline/
artifacts/Reports/
artifacts/Previews/
```

Rules:

- `Assets/Scripts/*` owns runtime gameplay only.
- `Assets/Editor/*` owns generation, validation, capture, and build automation.
- `Assets/Art/Generated/D020/*` is evidence for the current D-020 proof only,
  not final production art.
- `Assets/Art/Generated/BlenderPilot/*` is optional pipeline evidence and must
  not become the product look by accident.
- `Assets/Audio/Generated/*` is prototype or pipeline evidence unless the audio
  register explicitly accepts it for production.
- Do not add folders for inventory, crafting, quest, social, multiplayer, live
  service, or open-world streaming systems.

## シーン一覧

| Scene | Purpose | Notes |
| --- | --- | --- |
| `Bootstrap` | startup, service creation, first load | no gameplay content |
| `PersistentSystems` | input, save, audio, scene flow | loaded once |
| `UI_Game` | HUD, pause, settings | no quest log |
| `Hub_Crossroads` | hub, save return, region gates | 1 hub only |
| `Region_01_GreenRuins` | tutorial region, exploration tool introduction, boss 1 | vertical-slice target |
| `Region_02_SunkenWorks` | second region, tool mastery, boss 2 | no new core tool |
| `Region_03_AshenKeep` | final region before finale, harder rooms, boss 3 | no new region systems |
| `Boss_04_Final` | final boss arena | entered from hub or final gate |

## スクリプト責務一覧

| Script | Responsibility | Must Not Own |
| --- | --- | --- |
| `GameBootstrap` | load persistent systems and starting scene | gameplay rules |
| `SceneFlowController` | hub/region/boss transitions | streaming open-world logic |
| `InputReader` | keyboard/controller input mapping | gameplay decisions |
| `TopDownPlayerMotor` | movement, facing, dodge movement | combat damage |
| `TopDownCameraRig` | readable top-down camera and room framing | cinematic camera system |
| `PlayerCombat` | normal attack, damage intake, hit stun, death/retry | combo trees |
| `ExplorationTool` | one tool input, pulse/aim/use state | multiple ability systems |
| `ExplorationNode` | tool-reactive objects, reveal/activate/expose responses | quest logic |
| `RoomController` | room entry, enemy gates, reward spawn, shortcut open | world streaming |
| `EnemyController` | common enemy movement, telegraph, attack, death | boss-specific phases |
| `BossController` | boss state machine and defeat event | global progression storage |
| `Damageable` | health and damage response | AI decisions |
| `Hitbox` | attack volumes and timing windows | owner health |
| `ProgressState` | boss defeated, shortcut opened, region unlocked | inventory or quest log |
| `SaveService` | local save/load roundtrip | direct Steam API calls |
| `HudController` | health, tool readiness, boss health, prompts | minimap/quest list |
| `PauseMenuController` | resume, settings, quit to title | social/system menus |
| `AudioCueRouter` | play mapped cues for combat, UI, tool, boss, reward | music composition logic |

## データ設計

| Data | Fields | Notes |
| --- | --- | --- |
| `RegionDefinition` | `id`, `sceneName`, `unlockFlag`, `hubGateId`, `bossId` | max 3 |
| `BossDefinition` | `id`, `sceneName`, `hp`, `attackSetId`, `defeatFlag` | max 4 |
| `EnemyDefinition` | `id`, `hp`, `moveSpeed`, `telegraphTime`, `attackId` | keep small |
| `ExplorationNodeDefinition` | `id`, `nodeType`, `requiredFlag`, `responseTargetId`, `sfxId`, `vfxId` | reveal/activate/expose only |
| `RoomDefinition` | `id`, `sceneName`, `enemySet`, `rewardId`, `shortcutFlag` | no procedural room generation |
| `RelicRewardDefinition` | `id`, `displayName`, `effectId`, `presentationId` | vertical slice uses 2 |
| `SaveData` | `version`, `currentScene`, `hubSpawnId`, `bossDefeated`, `shortcutsOpened`, `regionsUnlocked`, `toolOwned`, `settings` | no inventory/crafting/quests |
| `AudioCueDefinition` | `id`, `category`, `clipPath`, `volume`, `priority` | required from first playable |

## 実装順

1. Lock this D-020 specification and remove old open-world/Echo Phase guidance
   from canonical docs.
2. Create `Bootstrap`, `PersistentSystems`, `Hub_Crossroads`, and one test room
   inside `Region_01_GreenRuins`.
3. Implement top-down movement, camera, normal attack, dodge, hit confirm, damage,
   death/retry, and one enemy.
4. Implement the single `ExplorationTool` and one `ExplorationNode` that visibly
   changes a route or reveals an interactable.
5. Add `RoomController`, shortcut opening, two simple gimmick rooms, and local
   save/load for progress flags.
6. Build vertical-slice content: hub 1, exploration area 1, enemy 2 types,
   miniboss 1, boss 1, shortcut 1, gimmick room 2, relic reward 2, minimal UI,
   BGM 2, minimum SFX.
7. Replace gray-box visuals in the vertical slice with stylized 3D art that
   meets the art direction doc.
8. Replace placeholder audio with milestone-quality BGM and SFX from the audio
   direction doc.
9. Run Windows/Steam Deck-oriented QA passes, capture screenshots/trailer beats,
   and decide whether the game has market pull.

## スコープ外一覧

- Open world or seamless world streaming.
- Day/night cycle.
- Fishing, farming, base building, survival loops.
- Inventory, crafting, equipment spreadsheets, quest log, social systems.
- Multiplayer, co-op, PvP, matchmaking, accounts, dedicated servers.
- Live service, daily tasks, paid currency, gacha.
- More than 1 hub, 3 regions, 4 bosses, or 1 exploration tool in MVP.
- Multiple exploration tools or Echo Phase/world-state switching.
- Procedural world generation as a production dependency.
- Complex combo strings as the core combat expression.
- Placeholder art/audio in market-validation captures.
