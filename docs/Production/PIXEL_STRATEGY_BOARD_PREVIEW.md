# Pixel Strategy Board Preview

Status: Unity visual evidence path for the 2D pixel strategy pivot.

This preview turns the deterministic strategy loop model into a small rendered
board read. It is not final art and does not import reusable source assets from
the asset library. Its job is to prove that the core loop can become a
streamer-readable screen:

- route loop;
- lair pressure;
- on-route hazard;
- carried reward;
- extraction gate;
- loot, threat, bag pressure, and extract decision HUD.

The preview is intentionally generated from code so the game repository can
validate screen readability before committing to a larger sprite pipeline.
Reusable sprite source, polish passes, PixelOver files, Aseprite files, and
source boards still belong in `RollNuts/unity-game-asset-library`.
