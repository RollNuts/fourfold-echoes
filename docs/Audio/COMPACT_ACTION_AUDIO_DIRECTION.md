# Compact Action Audio Direction

Status: canonical after D-020.

## オーディオピラー

1. **Readable Before Beautiful**
   - Enemy tells, hit confirm, damage, guard/dodge, and tool response must be
     understood before music polish.

2. **Few Themes, Strong Memory**
   - Use a small number of memorable loops and stingers.
   - Do not build a complex dynamic music system for MVP.

3. **Tool-Centered Feedback**
   - The exploration tool must have a clear sound family: ready, pulse, near
     response, success, fail, cooldown.

## 必須SE一覧

| Category | Required Cues |
| --- | --- |
| UI | select, confirm, back, error, pause |
| Player movement | footstep common, dodge, landing |
| Combat | normal swing, hit enemy, hit shield/armor, player damage, enemy death |
| Defense | guard start, guard hit, parry/success if implemented |
| Enemy | enemy notice, telegraph, attack, damage, death |
| Boss | boss intro hit, boss tell, boss impact, boss transition, boss defeat |
| Exploration tool | equip/ready, pulse, near response, target hit, fail, cooldown/ready |
| Gimmick | pedestal wake, mechanism move, shortcut open |
| Reward | chest open, relic appear, pickup, discovery stinger |
| System | save, load/continue, settings apply |

## 不要カテゴリ

- Voice acting.
- Ambient dialogue.
- Multiple weapon sound families before the first slice.
- Music stems for systems outside MVP.
- Dozens of UI variations.
- Online/social notification sounds.
- Crafting/inventory sounds.

## BGM一覧

| Track | Role | MVP Requirement |
| --- | --- | --- |
| `BGM_Hub` | safety and return | loopable, memorable motif |
| `BGM_Region01` | first exploration area | can share motif with hub but more motion |
| `BGM_Region02` | pressure region | planned, not needed in first slice |
| `BGM_Region03` | late mystery region | planned, not needed in first slice |
| `BGM_NormalCombat` | optional combat layer or short loop | keep simple |
| `BGM_Boss` | boss identity and readable escalation | required in slice |
| `BGM_DiscoveryReward` | short stinger | required in slice |

## 探索ツール音設計

| State | Sound Design |
| --- | --- |
| Idle ready | very low loop or periodic glint, optional |
| Equip/raise | short bright gesture |
| Pulse | clear mid-frequency pulse synced to VFX |
| Near response | repeated soft ping, faster only if needed |
| Target hit | upward resolved chime plus object reaction |
| Fail/no target | short low dry drop, not punitive |
| Cooldown/ready | small return tick |

Design principle: the game must remain playable without sound, but sound should
double confidence and satisfaction. Visuals communicate the rule; audio confirms
timing, proximity, success, and reward.

## 実装優先順位

1. Tool pulse, target hit, fail, and gimmick activation.
2. Attack, hit confirm, enemy tell, enemy death, player damage.
3. Boss tell, boss impact, boss transition, boss defeat.
4. UI select/confirm/back/error and save cue.
5. Hub/exploration track and boss track.
6. Reward/discovery stinger.
7. Region 02/03 loops after vertical slice direction is proven.

## マイルストーン完成条件

| Milestone | Audio Completion |
| --- | --- |
| M1 movement/combat | attack, hit, damage, dodge, enemy tell are audible |
| M2 tool proof | tool pulse, success, fail, and object activation are synced |
| M3 room proof | shortcut, chest, reward, and save cues exist |
| M4 boss proof | boss tell/impact/transition/defeat and boss BGM exist |
| M5 market slice | no placeholder SFX in capture, two BGM tracks mixed, loudness pass done |

## Mix Rules

- Enemy and boss tells outrank music.
- Player damage outranks ambience.
- Tool success briefly outranks exploration music.
- Reward stinger can briefly duck ambience.
- UI should be soft but readable.
