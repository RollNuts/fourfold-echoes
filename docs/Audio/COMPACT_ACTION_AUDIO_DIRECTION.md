# Compact Action Audio Direction

Status: canonical after D-020.

Scope: single-player, Steam-first, premium compact top-down action-adventure.
The MVP cap is one hub, three regions, four bosses, and one exploration tool.
Audio must support that shape only. Echo Phase, co-op, hack-and-slash loot,
extraction, and open-world audio language are out of scope.

## オーディオピラー

1. **Readable Before Beautiful**
   - Audio is a gameplay readability layer: danger, hit contact, damage,
     tool response, mechanism state, shortcut completion, and boss transitions
     must be understandable in play.
   - Music polish never outranks enemy tells, boss tells, or player damage.

2. **Compact Palette, Strong Identity**
   - Use a small number of memorable loops, shared motifs, and reusable
     stingers.
   - Do not build a large adaptive music system or one-off cue library for
     every room, enemy, or prop.

3. **One Tool, One Sound Family**
   - The exploration tool needs one consistent sound family: ready, pulse,
     near response, success, fail, and ready return.
   - The tool sound should make discovery feel intentional without making
     audio the only way to solve exploration.

4. **Reward Without Noise**
   - Exploration success, shortcut opening, boss transitions, and rewards get
     clear emphasis.
   - Routine actions stay short and mixable so the game remains clean during
     repeated play.

## 必須SE一覧

| Group | Required Cues | Notes |
| --- | --- | --- |
| UI | select, confirm, back/cancel, error, pause/menu open, save confirm | Keep soft and short; no large UI pack. |
| 探索成功 | tool target hit, discovery stinger, chest/reward reveal, pickup | The success chime and object reaction can share layers. |
| 被弾 | player light damage, player heavy damage, recover/invulnerability tick | Must cut through BGM without being harsh. |
| 仕掛け起動 | tool pulse response, pedestal/mechanism wake, mechanism move, lock release | Mechanism sounds should state "changed" more than "large machine." |
| ショートカット開通 | shortcut unlock, door/gate open, route-confirm stinger | One clear route-open identity reused across shortcut types. |
| ボス移行 | boss intro hit, phase transition, stun/break moment, defeat resolve | Phase transition must be audible even under combat noise. |

Supporting gameplay cues also required for MVP readability:

| Group | Required Cues | Notes |
| --- | --- | --- |
| Player movement | common footstep, dodge, landing | Minimal shared set. |
| Combat contact | player swing, enemy hit, solid/armor hit, enemy death | Prioritize timing and contact feel. |
| Enemy tell | notice, telegraph, attack release, enemy damage | Tells sit above music in the mix. |
| Exploration tool | equip/ready, pulse/use, near response, fail/no target, cooldown/ready return | See tool section. |

## 不要カテゴリ

- Voice acting and ambient dialogue.
- Co-op, online, lobby, emote, ping, invite, revive, or social notification
  sounds.
- Hack-and-slash loot sprays, extraction alarms, rarity explosions, or
  open-world map-reveal sweeps.
- Echo Phase or multi-state transformation cue sets.
- Multiple weapon families, defense variants, or character-class cue sets before
  MVP validation.
- Large region-specific ambience libraries, dozens of room loops, or bespoke
  prop foley for non-critical decoration.
- Crafting, trading, party management, complex inventory, or live-service
  reward audio.
- Advanced dynamic music stems beyond simple loops and short stingers.

## BGM一覧

| Track | Role | MVP Requirement |
| --- | --- | --- |
| `BGM_Hub` | safety, preparation, return motif | required |
| `BGM_Region01` | first exploration region | required for market slice |
| `BGM_Region02` | second region contrast | planned within D-020 cap |
| `BGM_Region03` | final region tension/mystery | planned within D-020 cap |
| `BGM_NormalCombat` | short normal combat loop or simple layer | use only if combat needs musical lift; keep reusable |
| `BGM_Boss` | boss identity, pressure, and readable escalation | required |
| `ST_DiscoveryReward` | discovery/reward stinger | required stinger, not counted as a full BGM track |

Do not inflate the soundtrack. The compact target is hub, three region loops,
one reusable normal combat cue if needed, one reusable boss foundation that can
support four bosses with small mix or intro variations, and one discovery
stinger. Region tracks may share motif and instrumentation.

## 探索ツール音設計

The exploration tool is the main non-combat audio identity. Its cue family must
be small, consistent, and cheap to implement.

| State | Sound Design | Gameplay Value |
| --- | --- | --- |
| Ready/equip | short bright raise cue | Confirms the tool is available. |
| Pulse/use | clear mid-frequency pulse synced to VFX | Confirms timing and direction of the action. |
| Near response | restrained ping or shimmer | Supports proximity without becoming the puzzle solution. |
| Success/target hit | resolved upward chime plus target reaction | Makes discovery and correct use feel intentional. |
| Fail/no target | short dry drop | Communicates no result without punishing experimentation. |
| Cooldown/ready return | small tick or glint | Confirms the player can act again. |

Avoid continuous loud loops, complex scanning layers, or musical systems tied
to hidden-object searching. Visuals carry the rule; sound increases confidence.

## 「無音でも成立、音があると気持ちよさが倍増」の原則

The game must remain playable with audio muted. Enemy tells, interactable
states, tool results, boss transitions, damage, rewards, and shortcut openings
must all have sufficient visual or timing feedback.

Audio then doubles the feel: it makes successful timing more satisfying,
mistakes clearer, discoveries more memorable, and repeated actions less flat.
If a sound is required to understand a mechanic, the mechanic needs better
visual feedback. If a sound does not improve confidence, timing, or reward, it
should be cut before it adds implementation cost.

## 実装優先順位

1. Exploration tool pulse, near response, success, fail, and mechanism reaction.
2. Player swing/contact, enemy tell, enemy damage/death, player damage, dodge.
3. Boss tell, boss impact, boss phase transition, boss defeat resolve, boss BGM.
4. UI select/confirm/back/error, pause, save confirm.
5. Shortcut opening, discovery/reward stinger, chest/reward pickup.
6. `BGM_Hub`, `BGM_Region01`, and `BGM_Boss` for market-facing capture.
7. `BGM_NormalCombat`, `BGM_Region02`, and `BGM_Region03` after the slice proves
   the direction and budget remains stable.

## マイルストーン完成条件

| Milestone | Audio Completion |
| --- | --- |
| M1 combat readability | attack, hit confirm, enemy tell, enemy death, player damage, and dodge are implemented and mixed |
| M2 tool proof | tool ready, pulse, near response, success, fail, and cooldown/ready return are synced to VFX and object response |
| M3 exploration room proof | mechanism activation, shortcut opening, discovery, reward pickup, and save cues exist with no placeholder audio |
| M4 boss proof | boss intro, tell, impact, phase transition, stun/break, defeat resolve, and boss BGM are present |
| M5 market slice | hub, Region01, boss music, core SFX, and reward stinger are mixed; capture has no rejected-direction audio |
| M6 full MVP cap | hub, three regions, reusable normal combat if kept, four bosses supported by the compact boss approach, and final loudness pass complete |

## Mix Rules

- Enemy and boss tells outrank music.
- Player damage outranks ambience and routine combat.
- Tool success briefly outranks exploration music.
- Boss transition briefly owns the mix, then returns control quickly.
- Shortcut and reward stingers may duck ambience but should not feel like loot
  explosions.
- UI should be soft, readable, and never louder than player damage or tells.
