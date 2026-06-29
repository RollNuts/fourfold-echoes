# Pixel Strategy Art Direction

Status: visual direction for the 2D pixel strategy pivot.

The art direction should favor compact, iconic pixel pieces over expensive 3D
models or large animation sets.

## Desired Feel

- bright classic fantasy readability;
- dry, slightly mischievous dungeon-board humor;
- small hero and monster silhouettes with personality;
- tile icons that feel collectible and tactical;
- warm 2D-HD lighting only after the base pixels read clearly.

Do not copy existing JRPG heroes, monsters, UI frames, fonts, tile sets, or
specific color arrangements. The game needs original motifs.

## Original Motifs

Use FOURFOLD ECHOES-specific visual language:

- folded-route seams;
- echo crystals;
- relic locks;
- paper-lantern adventurer shapes;
- lairs that look like tiny board pieces;
- extraction gates that read as a risky exit, not a generic door.

## Sprite Targets

| Asset | Size | Purpose |
| --- | ---: | --- |
| route tile | 32x32 | readable path role |
| lair/hazard tile | 32x32 | immediate threat read |
| reward/relic tile | 32x32 | greed trigger |
| adventurer token | 32x32 or 48x48 | board piece personality |
| monster token | 32x32 or 48x48 | threat personality |
| extraction marker | 32x32 | retreat decision |
| HUD glyph | 64x64 | pressure/bag/extract status |

## Asset Storage

Reusable source art, SVGs, previews, PixelOver files, Aseprite files, and
license notes belong in `RollNuts/unity-game-asset-library`.

The game repository should only receive reviewed runtime sprites or small
prototype samples required for the active playable slice.
