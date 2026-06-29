# Pixel Strategy Board Preview

Status: Unity visual evidence path for the 2D pixel strategy pivot.

This preview turns the deterministic strategy loop model into a small rendered
board read. It is not final art and does not import reusable source assets from
the asset library. Its job is to prove that the core loop can become a
streamer-readable screen:

- seal route;
- lure pressure;
- oath hazard;
- carried relic;
- gate seal;
- relic value, oath heat, chosen debt, and gate-cut HUD.

This early preview predates the world-identity pass. New market-facing captures
must use the Fourfold seal vocabulary in
`docs/Production/PIXEL_STRATEGY_WORLD_IDENTITY.md` and avoid visible
`LOOP BOARD` or `TILE HAND` language.

The preview is intentionally generated from code so the game repository can
validate screen readability before committing to a larger sprite pipeline.
Reusable sprite source, polish passes, PixelOver files, Aseprite files, and
source boards still belong in `RollNuts/unity-game-asset-library`.
