# Builder Prototype 2D-HD Pixel Readability

Status: prototype contract for Steam-viability testing.

This slice tests whether the builder ARPG prototype can move toward a 2D-HD
pixel-art presentation without losing the combat, building, loot, and extraction
reads already proven by the Unity prototype.

## Runtime Contract

The authoritative Unity-side contract is
`Assets/Scripts/BuilderPrototype/BuilderPrototypePixelReadabilityContract.cs`.
It fixes these prototype rules:

- 32 pixels per Unity unit.
- 32x32 pixel tiles, mapping one tile to one world unit.
- Build blocks occupy exactly one 32x32 tile.
- The avatar stays inside one tile at 24x28 pixels in the default spec, with
  enough mass to read facing and enough margin to read collision.
- Combat telegraphs require at least 3:1 contrast against the playfield.
- Extraction threshold text uses a short stable label such as `EXT 68%`.
- Extraction readouts reserve at least 56 pixels so `100%` does not resize the
  HUD during pressure changes.

## Steam Read

This is not a request to add a second art system to the final game. It is a
narrow evidence layer for the builder prototype: the same combat/build/extract
information must remain clear if production chooses 2D-HD/pixel art to avoid a
costly custom 3D model pipeline.

The contract deliberately avoids source asset imports. Reusable sprites,
references, or bulky source packs should stay in the asset-library repository
until a production art decision accepts them.
