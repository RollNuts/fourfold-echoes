# Next Three Player-Visible PRs

These are product-facing targets, not implementation tickets. A player-visible
PR should change what a reviewer can see, hear, touch, or capture.

Hard PO constraint: the immediate visible roadmap must fix fun first. Do not
spend the next three visible PRs adding puzzle symbols, lore glyphs, extra phase
markers, or more objective rules. The viewer should see a faster, clearer ARPG
loop before seeing more systems.

## PR 1: Movement, Dodge, And Hit Feel

Player-visible goal: a 10-20 second clip should make the basic ARPG loop look
responsive and satisfying: move into danger, dodge a fair threat, land a hit,
see the enemy react.

Must be visible:

- Movement to contact is quick and intentional.
- Dodge has clear start, invulnerable window, recovery, and success feedback.
- Attack has anticipation, active moment, recovery, and impact.
- Enemy hit response changes pose, scale, timing, effects, or audio enough to
  feel rewarding.
- The clip does not depend on the player reading puzzle symbols or HUD text.

Evidence to attach:

- 10-20 second clip of move, dodge, counterattack, hit reaction.
- One screenshot at the moment of hit impact.
- Notes on what is still placeholder.

## PR 2: Enemy Pressure And Time-To-Fun

Player-visible goal: the first 20 seconds should contain a clear decision and a
payoff. The player should not look like they are walking across an empty proof
room or waiting for a system to become interesting.

Must be visible:

- Enemy starts pressuring the player quickly and fairly.
- The room path pushes the player toward action, not wandering.
- Enemy tell, miss, hit, stagger, and defeat are readable without debug labels.
- Basic audio makes threat and impact easier to read.
- Any objective state is simplified enough that combat remains the main event.

Evidence to attach:

- One uninterrupted first-20-seconds clip.
- Screenshot of the enemy tell before impact.
- Screenshot of the enemy defeated or staggered after a successful exchange.
- Short note on asset provenance if any non-procedural asset is added.

## PR 3: Simple Room Payoff

Player-visible goal: a reviewer should understand why the room ends and why the
player would want another room, without needing symbolic puzzle explanation.

Must be visible:

- The room objective is reduced to one readable action or state change.
- Reward claim has a readable payoff state.
- Room completion is distinct from enemy defeat but does not slow the combat
  loop.
- Reset, retry, or exit behavior is understandable.
- A rough run summary or carry-home placeholder exists if loot is still not
  final.
- Capture can produce a short clip without manual staging.

Evidence to attach:

- One uninterrupted clip from room entry to reward claim.
- Screenshot set covering room read, combat, simple objective, gate, and reward.
- Explicit statement that the build is or is not a public demo candidate.

## What Does Not Count

- Internal schema, mediator, or build-system work with no visible artifact.
- A screenshot of an idle room only.
- More symbols, phase markers, UI labels, or lore tokens that do not improve the
  move-dodge-hit-defeat-claim loop.
- PR text that promises future art, audio, or loop depth without capture.
- Store-facing claims based on systems that are not playable in the branch.
