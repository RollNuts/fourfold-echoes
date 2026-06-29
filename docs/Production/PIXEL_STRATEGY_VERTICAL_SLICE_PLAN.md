# Pixel Strategy Vertical Slice Plan

Status: prototype plan for the 2D pixel strategy pivot.

The first vertical slice should prove the game is compelling as a tiny board
before it tries to become a large content pipeline.

## Slice Goal

Create a playable board loop where a viewer can understand:

- the route the adventurer will follow;
- which placed tiles create danger or reward;
- how much value is currently carried;
- why taking one more loop is tempting;
- why extracting now might be correct.

## PR Lanes

| Lane | Purpose | Output |
| --- | --- | --- |
| Product | lock the pivot and scope ceiling | product docs and acceptance gates |
| Core Model | deterministic route, pressure, loot, extraction simulation | runtime model plus tests |
| Art Seed | original board tiles and token sprites | asset-library seed with preview |
| Unity Preview | one readable board screenshot | generated scene/capture evidence |
| UI Read | pressure, bag value, loop count, extraction odds | HUD contract and tests |

Keep these lanes split so review and Veripsa merge ordering can stay clear.

## First Playable Board

- Board size: 8x6 or smaller.
- Route: one closed loop with one optional branch.
- Token: one adventurer or party marker.
- Tiles:
  - path;
  - wall/block;
  - lair;
  - hazard;
  - relic/reward;
  - extraction gate;
  - pressure modifier.
- Meters:
  - loop count;
  - carried loot;
  - threat/pressure;
  - extraction status.

## Visual Bar

The slice cannot finish as gray boxes. The board must use original pixel icons
that are small, readable, and charming enough to support Steam screenshots.

The desired read is closer to compact board-game pixel strategy than animated
action spectacle. Tile silhouettes, value contrast, and icon personality matter
more than high-frame-count character animation.

## Test Bar

Core strategy logic should be deterministic and covered by tests:

- route traversal is stable;
- tile effects trigger in order;
- threat grows with loops and danger tiles;
- reward grows with risk;
- extraction preview does not mutate state;
- extraction/defeat resolves carried loot clearly.

## Next Smallest Useful Slice

Implement the deterministic route loop model with tests, then bind it to one
Unity preview scene using the asset-library board seed.
