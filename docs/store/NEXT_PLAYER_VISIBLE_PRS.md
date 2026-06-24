# Next Three Player-Visible PRs

These are product-facing targets, not implementation tickets. A player-visible
PR should change what a reviewer can see, hear, touch, or capture.

## PR 1: Combat Readability And Body Feel

Player-visible goal: a short clip should make attack, dodge, enemy tell, hit,
and enemy defeat understandable without reading the HUD.

Must be visible:

- Player attack has clearer anticipation, active moment, and recovery.
- Enemy windup and strike lane are readable before damage happens.
- Dodge success and failure are distinguishable.
- Hit response changes enemy pose, scale, timing, or effects enough to read.
- Audio timing supports attack, hit, dodge, enemy tell, and player damage.

Evidence to attach:

- 10-20 second clip of enemy tell, dodge, counterattack, and defeat.
- One screenshot at the moment before enemy strike resolves.
- Notes on what is still placeholder.

## PR 2: Room Identity And Objective Clarity

Player-visible goal: the room should look like an intentional prototype space,
not a generated block layout.

Must be visible:

- Stronger player, enemy, altar, and gate silhouettes.
- Clearer path composition from spawn to altar to gate.
- Lighting that separates combat space, altar, and gate.
- Altar heat has a visible room-state effect.
- UI or prompt treatment explains the current objective without debug-log tone.

Evidence to attach:

- Full-room screenshot at spawn.
- Altar charging screenshot.
- Gate-ready screenshot.
- Short note on asset provenance if any non-procedural asset is added.

## PR 3: Loop Closure And Demo Candidate Evidence

Player-visible goal: a reviewer should understand why the room ends and why the
player would continue into the next room.

Must be visible:

- Reward claim has a readable payoff state.
- Room completion is distinct from enemy defeat.
- Reset, retry, or exit behavior is understandable.
- A rough run summary or carry-home placeholder exists if loot is still not
  final.
- Capture can produce a screenshot set and a short clip without manual staging.

Evidence to attach:

- One uninterrupted clip from room entry to reward claim.
- Screenshot set covering room read, combat, altar, gate, and reward.
- Explicit statement that the build is or is not a public demo candidate.

## What Does Not Count

- Internal schema, mediator, or build-system work with no visible artifact.
- A screenshot of an idle room only.
- PR text that promises future art, audio, or loop depth without capture.
- Store-facing claims based on systems that are not playable in the branch.
