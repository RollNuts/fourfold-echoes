# D022 Core Systems

## 必須

| System | MVP Requirement |
| --- | --- |
| Top-down movement | responsive walking, facing, collision, controller-first |
| Basic attack | one simple repeated attack with hit confirm |
| Dodge | short readable repositioning, no input complexity |
| Damage/death/retry | clear damage reason and fast retry |
| One exploration tool | reveal/activate/expose only, visually obvious |
| Tool target | idle, valid, active, solved, fail states |
| Room gate | enter room, lock/pressure, clear, reward, open route |
| Shortcut | one route change per slice, saved to local progress |
| Enemy tells | clear anticipation before damage |
| Boss openings | player learns where/when to attack |
| Reward relic beats | 2 slice rewards as presentation and small effect only |
| Local save/load | new, continue, clear flags, settings, backup/corrupt fallback |
| UI/UX | title, HUD, boss HP, prompts, pause, settings, retry, result |
| Audio | BGM 2 minimum, required SFX, mix priorities |
| Validation | 1280x800, controller, save, crash, boss progression |

## 不要

| System | Decision |
| --- | --- |
| Inventory | do not implement |
| Crafting | do not implement |
| Quest log | do not implement |
| Social systems | do not implement |
| Multiplayer/co-op/PvP | do not implement |
| Open-world streaming | do not implement |
| Day/night cycle | do not implement |
| Fishing/farming/base building | do not implement |
| Extra exploration tools | do not implement for MVP |
| Loot rarity/economy | do not implement |
| Procedural world generation | do not implement as production dependency |

## 後回し

| Item | When Allowed |
| --- | --- |
| Steam achievements | after internal progress events exist |
| Cloud save wrapper | after local save is stable |
| Console platform wrappers | after Steam slice proves market pull |
| Additional localization | after JP/EN/ZH-Hans store/UI priority text stabilizes |
| Extra enemy variants | only after the 2 slice enemies and 4 bosses are solid |
| Polish VFX variants | only after core readability VFX pass mute-review |
