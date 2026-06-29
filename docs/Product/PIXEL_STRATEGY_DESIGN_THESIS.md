# Pixel Strategy Design Thesis

Status: design guardrail for the 2D pixel strategy pivot.

FOURFOLD ECHOES should become a compact board strategy game about making a
hero too valuable, too wounded, and too late to leave cleanly.

The player is not a traditional hero. The player is the quiet hand shaping the
board: placing lairs, rewards, route pressure, and extraction windows until the
adventurer's next loop becomes an argument.

## Market Shorthand

Useful references:

- underground mischief strategy: small monsters, board ecology, and the joy of
  watching a confident hero walk into a bad plan;
- loop-route strategy: repeated traversal, predictable timing, and the pressure
  of one more pass;
- extraction tension: carried value matters only if the run leaves safely.

These are reference lenses, not implementation templates. Do not copy any
characters, monsters, UI frames, names, boards, combat rules, or story setups
from existing games.

## Core Fantasy

The best version of the game feels like a tiny fantasy board that slowly turns
against the adventurer.

The player should think:

- "If I place this reward here, the bag gets heavy before the safe gate."
- "If I add this lair now, the next loop is richer but the retreat line breaks."
- "The hero can survive, but only if I stop being greedy."

The audience should understand that thought from a single screenshot: route,
hero, bag value, pressure, and the next obvious disaster.

## The Greed Triangle

Every important choice should move at least one corner of this triangle:

| Corner | Meaning | Good Tension |
| --- | --- | --- |
| Hero | health, timing, courage, wounds | can keep going, but not forever |
| Board | lairs, hazards, route shape, gates | readable plan, then visible trap |
| Bag | loot, relics, burden, extraction value | value rises faster than safety |

If a proposed feature does not affect Hero, Board, or Bag, it probably does not
belong in the vertical slice.

## Moment-To-Moment Loop

1. Preview two placement options with visible deltas.
2. Place one tile on or near the route.
3. Watch the adventurer complete a short loop.
4. Read the bag, pressure, health, and extraction state.
5. Choose whether the board should stay greedy or become safer.

This keeps the MVP away from menus, inventories, crafting, large content trees,
and RPG party management. Depth should come from repeated tile reads and route
timing, not from system count.

## Original Visual Language

The game should borrow the readability of classic pixel fantasy while using its
own motifs:

- lantern-shaped adventurers instead of copied warrior silhouettes;
- folded-stone route seams that make the board feel like a paper relic;
- echo crystals as loot and pressure anchors;
- tiny lairs shaped like board charms, not realistic caves;
- extraction gates that look tempting, fragile, and slightly suspicious;
- pressure marks that crawl along the route as a readable countdown.

Use 32x32 and 48x48 pixel pieces first. Add 2D-HD lighting only after the base
sprite reads clearly at small size.

## Streamer-Readable Beats

The game needs moments that viewers can name quickly:

- Greedy: more loot now, worse extraction later.
- Safe: lower reward, cleaner gate timing.
- Doom Loop: continuing is technically possible but obviously foolish.
- Bag Panic: carried value is high enough that losing it hurts.
- Gate Miss: the board created a clear escape, then the hero missed it.

These beats should appear in UI labels, tests, previews, and screenshots before
the game grows a larger campaign wrapper.

## Slice Boundaries

For the first playable proof, keep this hard ceiling:

- one board;
- one route loop;
- one adventurer token;
- one pressure model;
- three tile families: danger, reward, and route control;
- one extraction decision;
- no inventory;
- no crafting;
- no party roster;
- no town management;
- no procedural campaign map;
- no meta economy beyond what is needed to replay the board.

The commercial version can grow only after this board produces repeatable
"one more loop" tension in tests, captures, and hands-on play.

## Acceptance Checks

A change supports this thesis if it makes at least one of these true:

- two placement choices can be compared without reading hidden rules;
- the safer choice and greedier choice are both defensible;
- a screenshot clearly shows route, hero, bag value, pressure, and gate state;
- a lost run feels caused by visible greed or visible bad timing;
- the art becomes more iconic without copying an existing fantasy franchise.
