# Pixel Strategy Decision Scorer

Status: prototype helper layered on top of the pixel strategy loop core.

The decision scorer compares candidate A and candidate B placement sets by
running the existing `PixelStrategyLoopSimulator` for each side. It is pure
runtime logic: no scene state, no UI dependency, no board preview dependency,
and no new content system.

## What It Adds

- short ASCII judgment labels such as `Greedier`, `SaferExtract`,
  `HigherThreat`, `ExtractReady`, and `DesperateRetreat`;
- numeric B-vs-A deltas for loot, threat, bag pressure, health, completed
  loops, extract readiness, and score;
- a compact summary string that can fit in a small UI hint or streamer-facing
  debug overlay;
- support for comparing a single placement or placement sets on top of shared
  base placements.

## Why It Fits The Pivot

The new direction needs quick readable pressure: the player should see that a
tile choice makes the route greedier, safer, more dangerous, or extraction-ready
without adding a large planner UI. This supports the loop-based strategy feel
and underground-pressure management shorthand while staying original and
avoiding named IP, copied rules, characters, or terminology.

## Scope Control

This does not expand the MVP beyond the compact board loop. It does not add
inventory, crafting, quest logs, procedural content, new enemy AI, rendering,
or the board preview layer. It only scores candidate placements against the
existing loop model so future UI can display a small decision hint.

## Acceptance

- `PixelStrategyDecisionScorer` compares candidate A/B with existing core
  simulation data.
- Labels and summaries remain short, English, and ASCII.
- PlayMode tests cover greed, extraction readiness, safer extract, higher
  threat, desperate retreat, and route validation.
