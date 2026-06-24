# One-Room Prototype Critique

This critique is intentionally direct. The current room proves the end-to-end
shape of the loop, but it does not yet look sellable to a player who has no
context.

## What Works Now

- The loop has a beginning, pressure point, objective state change, and finish.
- Fixed-angle framing can show the room, enemy, altar, and gate together.
- Phase colors give the first hint of a readable combat identity.
- Enemy windup, strike lane, dodge feedback, and player invulnerability are
  visible enough to tune.
- Altar heat and gate claim provide concrete non-combat room goals.
- Procedural cue coverage proves where audio feedback belongs.

## What Hurts Sellability

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

## Art Pass Priorities

1. Replace proof silhouettes with distinct player, enemy, altar, and gate shapes.
2. Give each interactable a readable material family and lighting role.
3. Add floor/wall detail that supports navigation without cluttering combat.
4. Make phase effects visible on the character and attack, not only in UI.
5. Make the gate claim moment look like a reward beat, not a validation flag.

## Engineering And UX Priorities

1. Add a capture-friendly HUD mode that keeps objective clarity without debug
   event text.
2. Keep combat feedback legible at thumbnail size: tell, dodge, hit, stagger,
   death, gate ready.
3. Make the first 20 seconds self-teaching through room layout and prompts.
4. Add evidence clips to player-visible PRs, not just screenshots.
5. Preserve placeholder honesty in docs and PR copy until final assets exist.

## Main Risk

The team can keep proving technical systems while the product still reads as a
test room. The next visible work should make the same loop easier to understand,
better to watch, and more satisfying to finish.
