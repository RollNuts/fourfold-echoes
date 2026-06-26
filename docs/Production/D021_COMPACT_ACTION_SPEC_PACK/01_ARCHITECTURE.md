# D021 Architecture

Status: current D021 implementation target.

## Script Responsibilities

| Script | Owns | Must Not Own |
| --- | --- | --- |
| `GameBootstrap` | startup and persistent load | gameplay rules |
| `TitleMenuController` | new game, continue, settings, quit | gameplay progression |
| `SceneFlowController` | title, hub, region, boss transitions | open-world streaming |
| `InputReader` | keyboard/controller input | gameplay decisions |
| `TopDownPlayerMotor` | movement, facing, dodge | combat damage |
| `TopDownCameraRig` | readable top-down framing | cinematic mode sprawl |
| `PlayerCombat` | normal attack, hit confirm, damage, death/retry | combo trees |
| `ExplorationToolController` | the one tool state and use | second ability system |
| `ExplorationTarget` | reveal, activate, expose responses | quest or inventory logic |
| `RoomController` | room gates, enemy clear, shortcut, reward spawn | world generation |
| `EnemyController` | normal enemy tell, movement, attack, death | boss phases |
| `BossController` | boss pattern set and defeat event | save ownership |
| `ProgressState` | flags only | inventory or quest log |
| `SaveService` | one versioned local save | direct platform API calls |
| `HudController` | HP, tool state, prompt, boss HP, save indicator | minimap or quest list |
| `PauseMenuController` | resume, settings, title, quit | social menus |
| `AudioCueRouter` | BGM/SFX cue routing | composing music logic |

## Data Design

| Data | Fields | Constraint |
| --- | --- | --- |
| `SaveData` | `version`, `currentScene`, `spawnId`, `bossDefeated[4]`, `regionsUnlocked[3]`, `shortcutsOpened`, `toolOwned`, `settings` | no inventory, crafting, or quest entries |
| `RegionDefinition` | `id`, `sceneName`, `hubGateId`, `unlockFlag`, `bossId` | max 3 |
| `BossDefinition` | `id`, `sceneName`, `hp`, `patternSetId`, `defeatFlag`, `musicId` | max 4 |
| `EnemyDefinition` | `id`, `role`, `hp`, `moveSpeed`, `telegraphSeconds`, `damage` | small authored set |
| `ExplorationTargetDefinition` | `id`, `targetType`, `responseType`, `solvedFlag`, `sfxId`, `vfxId` | reveal, activate, expose only |
| `RoomDefinition` | `id`, `sceneName`, `enemySetId`, `targetIds`, `shortcutFlag`, `rewardId` | handcrafted, not procedural |
| `RelicRewardDefinition` | `id`, `displayName`, `effectKind`, `presentationId` | reward beat only, no inventory UI |
| `AudioCueDefinition` | `id`, `category`, `clipPath`, `volume`, `priority` | required cue must be mapped |

## Implementation Order

1. Lock D021 canon and mark D020 documents as historical.
2. Build bootstrap, title, persistent systems, hub, and Region 01 test room.
3. Implement movement, camera, dodge, basic attack, damage, death/retry.
4. Add one enemy with readable tell and death.
5. Add one exploration tool plus one target response.
6. Add room gates, shortcut, reward spawn, save/load flags.
7. Add two tool rooms, two enemy types, one miniboss, and Boss 01.
8. Replace debug UI with shippable HUD, pause, settings, boss HP, prompts, and
   save indicator.
9. Add required BGM/SFX routing and replace temporary cues for the slice.
10. Bring Region 01 to production-readable art.
11. Run controller, 1280x800, and 1920x1080 readability passes.
12. Produce Windows build, capture path, and Steam-facing evidence.

## Implementation Boundary

Prefer direct gameplay components over framework-first architecture. Add shared
abstractions only after two concrete rooms, enemies, or bosses require the same
behavior.

Do not add a new system unless the change proposal also removes two lower-value
items and stays inside the D021 caps.
