# Audio Bible

Status: D-020 compact single-player direction.

This file defines audio as part of the game value, not as final decoration.
Detailed cue lists and milestone gates are maintained in:

- `docs/Audio/COMPACT_ACTION_AUDIO_DIRECTION.md`

## オーディオピラー

1. **Readability First**
   - Sound confirms danger, contact, success, and state change in a compact
     top-down action-adventure.
   - Enemy tells, player damage, boss phase movement, tool response, and
     shortcut opening must be readable before music polish.

2. **Small Set, High Memory**
   - MVP uses a small number of strong loops and reusable stingers.
   - Do not create a large adaptive score, weapon families, biome variants, or
     online/social audio.

3. **Tool And Reward Feel**
   - The single exploration tool needs a recognizable sound family.
   - Discoveries, rewards, mechanisms, and shortcuts must feel deliberate and
     satisfying without adding systems.

## 必須SE一覧

| Group | Minimum Cues |
| --- | --- |
| UI | select, confirm, back/cancel, error, pause/menu open, save confirm |
| 探索成功 | tool target hit, discovery stinger, chest/reward reveal, pickup |
| 被弾 | player damage, heavy damage, invulnerability/recover tick |
| 仕掛け起動 | tool pulse response, pedestal/mechanism wake, mechanism move, lock release |
| ショートカット開通 | shortcut unlock, door/gate open, route-confirm stinger |
| ボス移行 | boss intro hit, phase transition, stun/break moment, defeat resolve |

## 不要SEカテゴリ

- Voice acting and ambient dialogue.
- Co-op, online, lobby, ping, invite, or social notification sounds.
- Hack-and-slash loot bursts, extraction alarms, open-world map reveal sweeps,
  or other rejected direction cues.
- Multiple weapon sound families before MVP validation.
- Large ambient creature sets or region-specific one-off foley libraries.
- Crafting, trading, party management, or complex inventory audio.
- Dense UI variation packs beyond the minimum readable set.

## BGM一覧

| Track | Role | Scope |
| --- | --- | --- |
| `BGM_Hub` | safe return, preparation, home motif | MVP |
| `BGM_Region01` | first field/dungeon region | MVP |
| `BGM_Region02` | second region contrast | planned within D-020 cap |
| `BGM_Region03` | final region tension/mystery | planned within D-020 cap |
| `BGM_NormalCombat` | short combat loop or lightweight layer | MVP if combat reads better with music, otherwise post-slice |
| `BGM_Boss` | boss identity and escalation | MVP |
| `ST_DiscoveryReward` | short discovery/reward stinger | MVP stinger, not a full song |

Keep the MVP music set compact. Region tracks may share motifs, palettes, and
instrumentation; the goal is identity and flow, not volume.

## 探索ツール音設計

The single exploration tool has one compact sound family:

| State | Cue |
| --- | --- |
| Ready/equip | short bright raise cue |
| Pulse/use | clear mid-frequency pulse synced to the tool VFX |
| Near response | restrained ping that supports proximity without solving it alone |
| Success | resolved chime plus object reaction |
| Fail/no target | short dry drop that is readable but not punitive |
| Cooldown/ready return | small tick or glint |

## 「無音でも成立、音があると気持ちよさが倍増」の原則

Every mechanic must remain understandable through animation, UI, camera, and
level feedback without sound. Audio then adds confidence, timing, texture, and
reward. If a cue is required to understand the rule, the visual design is not
finished. If a cue does not improve confidence or satisfaction, it is cut.

## 実装優先順位

1. Exploration tool pulse, success, fail, and mechanism reaction.
2. Player attack/contact, enemy tell, enemy hit/death, player damage.
3. Boss tells, impacts, transition, defeat resolve, and boss loop.
4. UI minimum set, save confirmation, reward/discovery stinger.
5. Hub, Region01, and optional normal combat loop for market slice.
6. Region02 and Region03 loops after the compact slice proves the direction.

## マイルストーン完成条件

| Milestone | Completion Gate |
| --- | --- |
| Combat proof | hit confirm, enemy tell, player damage, and dodge/movement are audible and mixed |
| Tool proof | pulse, near response, success, fail, and mechanism reaction are synced |
| Exploration proof | discovery, reward, shortcut opening, and save cues are implemented |
| Boss proof | boss intro, tell, impact, transition, defeat, and boss BGM are implemented |
| Market slice | no placeholder audio in capture, compact BGM set mixed, no rejected-direction cue categories present |
