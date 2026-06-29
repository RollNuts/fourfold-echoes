# Pixel Strategy Steam Screen Preview

Status: visual evidence layer for the pixel strategy pivot.

This preview adds a 16:9 Unity capture target that aims at a Steam screenshot
read instead of a UI component sheet. It keeps the board as the main object:
chosen pawn, relic route, oath-heat trail, lure threat, gate seal, and
three bottom decision cards.

## Why It Exists

The prior board preview proved that the loop model could render, but it still
read more like a compact debug board plus explanatory HUD. The new direction
needs a game screen that can sell the decision at thumbnail size:

- chosen debt and oath heat are visible at the edge;
- the seal route, gate, chosen pawn, relic, and danger are visible on the board;
- the three decisions read as different tactical outcomes;
- the selected gate-cut card visibly connects to the gate.
- the selected `CUT TO GATE` decision visibly opens a short gate route and
  seals pressure cells on the board, so the choice reads as a board change
  rather than only a highlighted UI card.
- the board carries a FOURFOLD-specific visual read: four corner sigils, echo
  shadows behind changed cells, a carried-relic rail, gate danger frames, and a
  four-beat seal counter.
- each A/B/C card has a one-step tradeoff forecast: baiting the chosen hero
  raises relic value and oath heat, cutting to the gate opens a spare route
  while cracking pressure, and claiming relics spikes debt while pushing the
  gate away.
- a cracked pressure beat raises the chosen hero level and queues a next-step
  accident, so the same profitable line becomes more dangerous after repeated
  pressure spikes.

The screen also bakes in the latest premise read: a "chosen" adventurer starts
with a wood stick and a few coins, then gets shoved toward an absurd threat.
That unfair classic-fantasy setup should feel funny, readable, and dangerous
without copying any protected game, character, UI, or quest text.

## World Identity Guardrail

Use `docs/Production/PIXEL_STRATEGY_WORLD_IDENTITY.md` as the guardrail before
adding more rules. The market-facing screen must read as a Fourfold seal board
about chosen debt, oath pressure, and gate cuts. It must not lead with visible
`LOOP`, `TILE HAND`, road-expedition, camp, deck, or dungeon-management language.

## Scope Control

This does not add a runtime scene, inventory, campaign, meta economy, imported
art pack, or UI system. The capture remains code-generated visual evidence so
the team can judge composition before committing to a larger sprite pipeline.

Reusable polished source art still belongs in
`RollNuts/unity-game-asset-library`.

## Verification Target

Generate with:

```bash
/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath <worktree> \
  -executeMethod FourfoldEchoes.Editor.StrategyLoop.PixelStrategySteamScreenPreviewCapture.Capture \
  -logFile <temp>/fourfold-pixel-strategy-steam-screen-preview.log
```

Expected output:

`<temp>/fourfold-pixel-strategy-steam-screen-preview-20260629/pixel-strategy-steam-screen-preview.png`
