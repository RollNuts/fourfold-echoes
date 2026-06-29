# Pixel Strategy Steam Screen Preview

Status: visual evidence layer for the pixel strategy pivot.

This preview adds a 16:9 Unity capture target that aims at a Steam screenshot
read instead of a UI component sheet. It keeps the board as the main object:
hero token, reward route, pressure trail, lair threat, extraction gate, and
three bottom decision cards.

## Why It Exists

The prior board preview proved that the loop model could render, but it still
read more like a compact debug board plus explanatory HUD. The new direction
needs a game screen that can sell the decision at thumbnail size:

- bag value and pressure are visible at the edge;
- the route, gate, hero, reward, and danger are visible on the board;
- the three decisions read as different tactical outcomes;
- the selected safe extraction card visibly connects to the gate.

The screen also bakes in the latest premise read: a "chosen" adventurer starts
with a wood stick and a few coins, then gets shoved toward an absurd threat.
That unfair classic-fantasy setup should feel funny, readable, and dangerous
without copying any protected game, character, UI, or quest text.

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
