# Development Charter

Status: canonical after D-020.

## Purpose

FOURFOLD ECHOES is being built as a Steam-first, buy-to-own, single-player
top-down classic action-adventure.

Every task must improve at least one of these product truths:

- top-down movement and camera become clearer
- the one exploration tool becomes more readable or more satisfying
- a room becomes better at teaching or testing the same tool
- combat becomes easier to read and better to feel
- a shortcut, boss, or reward becomes clearer
- art becomes more stylized, readable, and marketable
- audio improves timing, feedback, or memory
- production becomes more repeatable without adding systems

## Product North Star

The player should be able to say:

> I left a small hub, explored compact fantasy regions, mastered one exploration
> tool, opened shortcuts, found relic rewards, and defeated readable bosses.

If a proposed feature does not support that sentence, it is suspect.

## Default Decision Rule

When uncertain, choose:

- one strong tool over many weak systems
- compact room mastery over map size
- readable action over complex simulation
- stylized clarity over realism
- finished art/audio over broad gray-box coverage
- controller-first play over editor convenience
- local single-player completion over future platform dreams

## Cost Gate For Additions

Any new feature proposal must answer:

| Question | Required Answer |
| --- | --- |
| Does it make the one-tool loop stronger? | yes/no |
| Does it improve the first 30 minutes? | yes/no |
| Does it improve trailer readability? | none/weak/strong |
| What two things are removed to pay for it? | required |
| Implementation cost | low/medium/high |
| QA cost | low/medium/high |

High-cost and weak-trailer-value proposals are rejected by default.

## Milestone Rules

### Phase A: Playable Base

Allowed:

- movement
- camera
- attack
- dodge
- one enemy
- damage/death/retry
- one gray-box room
- temporary assets and sounds

Disallowed:

- region quantity
- second exploration tool
- inventory/crafting/quest UI
- open-world layout

### Phase B: One-Tool Room Proof

Allowed:

- one exploration tool
- one tool-reactive route
- one tool-reactive object
- one shortcut
- first non-placeholder tool SFX/VFX

Disallowed:

- additional tools
- extra regions
- economy systems

### Phase C: Vertical Slice

Allowed:

- hub 1
- region segment 1
- enemy types 2
- miniboss 1
- boss 1
- gimmick rooms 2
- relic rewards 2
- BGM 2
- minimum SFX
- save/load
- styled art

Disallowed:

- gray-box market capture
- placeholder audio in validation capture
- new systems before the slice is fun

### Phase D: Production

Allowed:

- expand to 3 regions and 4 bosses using proven slice standards
- polish art, animation, audio, UI, QA, localization

Disallowed:

- new primary systems
- open world
- multiplayer
- late inventory/crafting expansion

## Non-Negotiables

- No open world.
- No live service.
- No mandatory online.
- No server cost.
- No co-op in MVP.
- No inventory, crafting, quest log, social systems, fishing, farming, or base
  building.
- No second exploration tool in MVP.
- No final placeholder art/audio.
- No unclear asset rights.
- No direct gameplay dependency on platform-specific APIs.
