# Pixel Strategy Loop Core

Status: prototype model for the current Steam indie direction.

This branch pivots away from the prior construction-action stack and adds a
small code-first model for an original 2D pixel strategy loop. The target feel is a
readable board-like top-down game about route pressure, tile placement, loot
tension, and extraction decisions.

## Inspiration Boundary

Use references such as underground-management strategy, loop-based adventure,
and extraction tension only as broad market shorthand. Do not copy any game's
IP, characters, monsters, UI, names, exact mechanics, board rules, or visual
identity.

## Runtime Model

The Unity-facing model lives in
`Assets/Scripts/StrategyLoop/PixelStrategyLoopCore.cs`.

It currently proves:

- a closed orthogonal tile route that can be simulated as a repeating loop;
- placed lairs that influence adjacent route cells;
- on-route hazards and reward caches;
- hero path simulation across one or more loops;
- threat, loot, bag pressure, health, and step accounting;
- loop-boundary decisions to continue, extract, retreat, or lose.

The model intentionally does not add UI, art, live scene generation, inventory,
networking, or a broad content system. Those should wait until this tiny loop is
fun and legible under tests.

## Why It Matters

This gives the prototype a testable strategic spine: the player can make simple
placement choices that alter route risk and reward, while the hero run creates
streamer-friendly "one more loop or extract now" pressure. It is small enough to
iterate without locking the project into a copied genre implementation.
