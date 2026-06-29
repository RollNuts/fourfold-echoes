# Pixel Strategy Pivot

Status: active prototype direction as of 2026-06-29.

FOURFOLD ECHOES is pivoting toward a Steam-first, buy-to-own, single-player
2D pixel strategy game built around an addictive board loop, readable placement,
and extraction pressure.

See `docs/Product/PIXEL_STRATEGY_DESIGN_THESIS.md` for the tighter design
guardrail: shape the board so the adventurer becomes more valuable, more
pressured, and harder to extract safely.

The target feel is:

- small, charming pixel pieces with the immediate readability of classic JRPG
  battle/field icons;
- board-like route planning and placement pressure;
- run-by-run greed, loss, and retreat decisions;
- spectator-readable outcomes that make viewers argue about the next tile,
  risk, or extraction call.

References such as classic JRPG pixel art, dungeon-management strategy games,
and loop-route strategy games are inspiration for readability and tension only.
Do not copy their characters, monsters, UI, names, story, or exact mechanics.

## Core Loop

1. The player shapes a compact route board with tiles, lairs, hazards, rewards,
   and extraction points.
2. A small party or lone adventurer token traverses the route automatically or
   semi-automatically.
3. Placed tiles create predictable interactions: monster pressure, loot, relics,
   shortcuts, wounds, corruption, and tempo changes.
4. The player decides when to keep looping for better rewards and when to
   extract before the carried value is lost.
5. Between runs, the player unlocks tighter tile synergies, sharper board
   control, and better risk reads rather than broad live-service progression.

## Strategic Pillars

- **Readable Board**: every tile must communicate role at a glance: route,
  lair, hazard, reward, modifier, extraction, or lock.
- **Greed Clock**: each loop should make the next loop more tempting and more
  dangerous.
- **Placement Mastery**: depth comes from tile adjacency, route timing,
  monster ecology, reward routing, and extraction windows.
- **Small Cast, Strong Icons**: use memorable silhouettes instead of large
  character sheets.
- **Streamer Tension**: a viewer should understand the bag value, current
  pressure, and next bad outcome without reading a manual.

## MVP Ceiling

The first playable proof should stay compact:

- one board biome;
- one route loop;
- one adventurer token;
- three tile families: route, danger, reward;
- one extraction marker;
- one pressure meter;
- no crafting, quest log, open world, base building, multiplayer, gacha, or
  live-service systems.

## What This Replaces

This direction supersedes work that assumes the prototype must be a
Dragon-Quest-Builders-like ARPG or a 3D model production problem. Existing code
and assets may be reused only if they help the 2D pixel strategy loop become
clearer, more testable, or more Steam-readable.

## Acceptance For Current Prototype Work

A change supports this pivot only if it makes at least one of these more true:

- the route board is easier to read;
- tile placement creates an interesting decision;
- loop pressure increases tension;
- extraction or retreat has a visible cost;
- the pixel art direction becomes more distinctive and more sellable;
- a PR produces reusable assets or testable mechanics without copying existing
  IP.
