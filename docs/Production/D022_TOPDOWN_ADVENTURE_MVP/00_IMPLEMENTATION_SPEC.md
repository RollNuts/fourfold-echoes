# D022 Implementation Spec

Status: current sole product specification.

Use this pack as the source of truth for new work. Earlier directions are
historical evidence only.

## 仕様固定メモ

FOURFOLD ECHOES is a Steam-first, buy-to-own, single-player top-down classic
action-adventure.

| Item | Fixed Scope |
| --- | ---: |
| Hub | 1 |
| Regions | 3 |
| Bosses | 4 |
| Exploration tools | 1 |
| Save | local save |
| Online systems | 0 |
| Inventory / crafting / quest log / social systems | 0 |

The fun comes from repeated mastery of a small rule set: movement, basic attack,
dodge, one exploration tool, readable rooms, shortcuts, enemy tells, boss
openings, reward beats, UI clarity, and audio feedback.

No open world, day/night cycle, fishing, farming, base building, social system,
online dependency, live service, or extra exploration tool may be introduced for
MVP.

The MVP promise is exactly 1 hub, 3 regions, 4 bosses, and 1 exploration tool.

Music and SFX are part of the playable product from the first market slice. A
milestone that claims market validation cannot ship gray boxes or placeholder
sound.

### New Project Folder Proposal

```text
Assets/
  Scenes/
    FES_Boot.unity
    FES_Title.unity
    FES_Runtime.unity
    FES_GameHud.unity
    FES_Hub_Anchor.unity
    FES_R01_VerdantSteps.unity
    FES_R02_CinderCanal.unity
    FES_R03_MoonlitVault.unity
    FES_Boss_FinalCore.unity
  Scripts/
    Core/
    Input/
    Player/
    Combat/
    ExplorationTool/
    Rooms/
    Enemies/
    Bosses/
    Save/
    UI/
    Audio/
  Prefabs/
    Player/
    Enemies/
    Bosses/
    Rooms/
    Interactables/
    UI/
  Art/
    Characters/
    Enemies/
    Environment/
    Props/
    VFX/
    UI/
  Audio/
    BGM/
    SFX/
  Settings/
```

Support folders:

```text
docs/
game-spec/
commands/
Scripts/Validation/
tools/Blender/
tools/AssetPipeline/
tools/AudioPipeline/
artifacts/Reports/
artifacts/Previews/
```

Do not add folders for inventory, crafting, quest, social, multiplayer, live
service, open-world streaming, fishing, farming, or base building.

## シーン一覧

| Scene | Role | Notes |
| --- | --- | --- |
| `FES_Boot` | startup and persistent system load | no gameplay content |
| `FES_Title` | new game, continue, settings, quit | product UI only |
| `FES_Runtime` | input, save, audio, scene flow | loaded once |
| `FES_GameHud` | HUD, pause, settings, prompts | no quest log |
| `FES_Hub_Anchor` | only hub, region gates, return point | hub cap is 1 |
| `FES_R01_VerdantSteps` | first region, tool introduction, boss 1 | vertical-slice target |
| `FES_R02_CinderCanal` | second region, shortcut mastery, boss 2 | no second tool |
| `FES_R03_MoonlitVault` | final region pressure, boss 3 | region cap is 3 |
| `FES_Boss_FinalCore` | boss 4 finale | boss cap is 4 |

## スクリプト責務一覧

| Script | Owns | Must Not Own |
| --- | --- | --- |
| `GameBoot` | startup and persistent system creation | combat/progression rules |
| `SceneFlow` | title, hub, region, boss transitions | open-world streaming |
| `InputReader` | keyboard/controller input | gameplay decisions |
| `TopDownCamera` | readable camera follow and room framing | cinematic camera suite |
| `PlayerMotor` | move, face, dodge | damage rules |
| `PlayerCombat` | basic attack, damage intake, hit stun, death/retry | combo tree |
| `SingleToolController` | one tool input, cooldown, target query | ability system |
| `ToolTarget` | reveal/activate/expose reactions | quest or inventory gates |
| `RoomGateController` | room lock, enemy clear, reward beat, shortcut | procedural generation |
| `EnemyController` | common enemy move, tell, attack, death | boss phases |
| `BossController` | boss pattern, opening, transition, defeat event | save ownership |
| `ProgressState` | flags for boss, shortcut, regions, tool | item list or quest log |
| `SaveService` | local save/load, backup, migration | Steam API direct calls |
| `HudController` | HP, tool state, boss HP, prompts, save feedback | minimap or quest list |
| `AudioRouter` | mapped SFX/BGM cues and priorities | music generation |

## データ設計

| Data | Fields | Constraint |
| --- | --- | --- |
| `SaveData` | `version`, `currentScene`, `spawnId`, `bossDefeated[4]`, `regionsUnlocked[3]`, `shortcutsOpened`, `toolOwned`, `settings` | no inventory/crafting/quests |
| `RegionDef` | `id`, `sceneName`, `hubGateId`, `unlockFlag`, `bossId` | 3 records |
| `BossDef` | `id`, `sceneName`, `hp`, `patternSetId`, `defeatFlag`, `musicId` | 4 records |
| `EnemyDef` | `id`, `role`, `hp`, `moveSpeed`, `telegraphSeconds`, `damage` | small handcrafted roster |
| `ToolTargetDef` | `id`, `targetType`, `responseType`, `solvedFlag`, `sfxId`, `vfxId` | reveal/activate/expose only |
| `RoomDef` | `id`, `sceneName`, `enemySetId`, `targetIds`, `shortcutFlag`, `rewardBeatId` | handcrafted rooms only |
| `RewardBeatDef` | `id`, `presentationId`, `effectKind`, `saveFlag` | reward presentation, no inventory UI |
| `AudioCueDef` | `id`, `category`, `clipPath`, `volume`, `priority` | required cues only |

## 実装順

1. Create `FES_Boot`, `FES_Runtime`, `FES_Title`, `FES_Hub_Anchor`, and one empty
   room in `FES_R01_VerdantSteps`.
2. Implement movement, top-down camera, basic attack, dodge, hit, damage,
   death, and retry.
3. Implement one normal enemy with readable tell, attack, hit, and death.
4. Implement one exploration tool and one `ToolTarget` that visibly opens a path
   or exposes an object.
5. Implement room gates, one shortcut, reward beat, progress flags, and local
   save/load.
6. Build region 1 slice: 2 gimmick rooms, 2 normal enemies, 1 miniboss, 1 boss,
   2 reward relics, minimal UI, 2 BGM tracks, minimum SFX.
7. Replace debug UI with product title, HUD, pause, settings, boss HP, prompts,
   save feedback, death/retry, and result screens.
8. Replace placeholder audio with required SFX/BGM for capture.
9. Replace gray-box visuals with stylized 3D assets that pass the art bar.
10. Add regions 2, 3, and final boss using the same systems without adding new
    systems.
11. Verify Windows, controller, 1280x800, save roundtrip, crash-free 30-minute
    route, screenshots, and trailer beats.

## スコープ外一覧

| Out of Scope | Reason |
| --- | --- |
| Inventory, equipment spreadsheets, crafting | shifts game into economy UI |
| Quest log or task list | requires broad content tracking |
| Social, account, backend, online, co-op, PvP | violates single-player v1 |
| Open world or seamless streaming | violates compact handcrafted scope |
| Day/night cycle | multiplies lighting, save, QA, and art states |
| Fishing, farming, base building, survival | different product promise |
| Second exploration tool | breaks one-tool mastery |
| More than 1 hub, 3 regions, 4 bosses | exceeds MVP cap |
| Procedural world generation | undermines readable handcrafted rooms |
| Loot economy or hack-and-slash reward spam | expands UI and balance scope |
| Placeholder art/audio in public captures | cannot validate market appeal |
