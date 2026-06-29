# Pixel Strategy Seal Core

Status: prototype model for the current Steam indie direction.

This branch pivots away from the prior construction-action stack and adds a
small code-first model for an original 2D pixel strategy run. The target feel is
a readable top-down seal board about route pressure, placed oath marks, relic
temptation, and gate-cut decisions.

## Inspiration Boundary

Use references such as underground-management strategy, repeating adventure
pressure, and loss-aversion tension only as broad market shorthand. Do not copy
any game's IP, characters, monsters, UI, names, exact mechanics, board rules, or
visual identity.

Market-facing captures must follow
`docs/Production/PIXEL_STRATEGY_WORLD_IDENTITY.md`: the visible read is a
Fourfold seal board about chosen debt and oath pressure, not a road-loop card
expedition.

## Runtime Model

The Unity-facing model lives in
`Assets/Scripts/StrategyLoop/PixelStrategyLoopCore.cs`.

It currently proves:

- a closed orthogonal seal route that can be simulated as repeated pressure;
- placed lairs that influence adjacent route cells;
- on-route hazards and reward caches;
- chosen-pawn path simulation across one or more echoes;
- threat, relic value, debt pressure, health, and step accounting;
- run-boundary decisions to continue, cut the gate, spare the chosen, or lose.

The model intentionally does not add UI, art, live scene generation, inventory,
networking, or a broad content system. Those should wait until this tiny loop is
fun and legible under tests.

## Why It Matters

This gives the prototype a testable strategic spine: the player can make simple
placement choices that alter seal-route risk and relic temptation, while the
chosen run creates streamer-readable "spare or exploit the debt" pressure. It
is small enough to iterate without locking the project into a copied genre
implementation.
