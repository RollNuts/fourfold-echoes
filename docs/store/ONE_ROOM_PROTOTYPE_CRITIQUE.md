# One-Room Prototype Critique

This critique is intentionally direct. The current room proves the end-to-end
shape of the loop, but it does not yet look sellable to a player who has no
context.

Hard PO read: the current prototype looks tedious and not fun. Treat this as the
primary product risk, ahead of new puzzle symbols, lore markers, or additional
phase decoration.

## What Works Now

- The loop has a beginning, pressure point, objective state change, and finish.
- Fixed-angle framing can show the room, enemy, altar, and gate together.
- Phase colors give the first hint of a readable combat identity.
- Enemy windup, strike lane, dodge feedback, and player invulnerability are
  visible enough to tune.
- Altar heat and gate claim provide concrete non-combat room goals.
- Procedural cue coverage proves where audio feedback belongs.

## What Hurts Sellability

- The first impression is still "technical proof," not "I want to play this."
- The room asks the player to manage symbols and objective state before the
  basic action feels rewarding.
- Movement to contact can look like crossing a test room instead of entering a
  fight.
- Primitive silhouettes still read as proof objects, not characters and props.
- The player does not yet have motion style, attack weight, hit pause, recovery
  body language, or readable animation intent.
- The enemy is a single pressure source and does not yet sell behavior variety.
- The HUD is useful for debugging but too literal and tool-like for store-facing
  capture.
- Phase switching is visible as color, but not yet as a strong gameplay promise.
- The altar interaction currently reads as a hold-meter unless the surrounding
  room state reinforces why it matters.
- The reward claim does not yet communicate loot value, extraction value, or why
  the player should want another room.
- The room lacks authored environmental story, prop density, damage, material
  contrast, and focal lighting.
- Placeholder audio proves event coverage but does not yet carry mood, impact,
  or identity.

## Fun-First Priorities

1. Make movement, dodge, attack, hit reaction, and enemy defeat feel good before
   expanding puzzle or phase complexity.
2. Shorten the time from room start to first meaningful combat decision.
3. Make the enemy pressure fair but urgent; tedious safety is worse than a rough
   but readable threat.
4. Make one successful hit visibly satisfying through timing, effects, audio,
   enemy reaction, or camera-independent staging.
5. Simplify the room objective until it supports the fight instead of competing
   with it.

## Art Pass Priorities

1. Replace proof silhouettes with distinct player, enemy, altar, and gate shapes.
2. Give each interactable a readable material family and lighting role.
3. Add floor/wall detail that supports navigation without cluttering combat.
4. Make phase effects support combat feel, not puzzle-symbol reading.
5. Make the gate claim moment look like a reward beat, not a validation flag.

## Engineering And UX Priorities

1. Tune input response, attack cadence, dodge recovery, enemy timing, and hit
   feedback before adding new room rules.
2. Add a capture-friendly HUD mode that keeps objective clarity without debug
   event text.
3. Keep combat feedback legible at thumbnail size: tell, dodge, hit, stagger,
   death, gate ready.
4. Make the first 20 seconds self-teaching through action, not labels.
5. Add evidence clips to player-visible PRs, not just screenshots.
6. Preserve placeholder honesty in docs and PR copy until final assets exist.

## Main Risk

The team can keep proving technical systems and symbolic mechanics while the
product still reads as a slow test room. The next visible work should make the
same loop faster, clearer, and more satisfying to play.
