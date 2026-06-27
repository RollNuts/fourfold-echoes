# Block Field Direction

Status: active field-production direction.

## Decision

The field should be built from connected modular blocks/chunks.

This is the right production direction for the current phase because it lets the
game move forward with many reusable pieces instead of slow bespoke terrain.
The construction logic can be Minecraft-like: snap blocks, repeat chunks,
swap biomes, and compose playable routes quickly.

The visual result must not copy Minecraft. No pixel-cube texture language, no
literal voxel terrain, and no famous block-game silhouette. The target is a
stylized ARPG block-field kit: chunky, beveled, colorful, and readable from a
high three-quarter camera.

The current concept sheets are exploration sheets only. Production modeling
starts from traversal and collision grammar, not from decorative scenelets.

## Core Module Set

First-pass field modules:

- `BF_GRASS_Floor_1x1_Flat`
- `BF_GRASS_Floor_1x2_Straight`
- `BF_GRASS_Floor_2x2_Plaza`
- `BF_GRASS_Edge_Straight`
- `BF_GRASS_Edge_OuterCorner`
- `BF_GRASS_Edge_InnerCorner`
- `BF_GRASS_Height_Cliff_Up1`
- `BF_STONE_Floor_1x1_Clean`
- `BF_STONE_Floor_2x2_Clean`
- `BF_STONE_Floor_1x1_Cracked`
- `BF_STONE_Wall_Straight_1x1`
- `BF_STONE_Wall_OuterCorner`
- `BF_STONE_Wall_InnerCorner`
- `BF_STONE_Arch_Doorway`
- `BF_STAIR_Straight_Up1`
- `BF_STAIR_CornerLanding_Up1`
- `BF_WATER_ChannelEdge_Straight`
- `BF_BRIDGE_Wood_1x2`
- `BF_FENCE_Railing_Straight`
- `BF_HAZARD_Floor_Spike`

## Initial Biomes

Do not build every biome at once. The first playable set should cover:

- grassland
- village plaza
- water edge
- cave/dungeon
- snow field

Later expansion sets:

- desert
- lava cavern
- crystal cave
- swamp
- sky island
- mushroom grove

## Scale Rules

- 1 Unity unit = 1 meter.
- Base tile footprint: 2m x 2m.
- Half tile footprint: 1m x 2m or 2m x 1m.
- Platform height step: 0.5m increments.
- Walkable top surface must be mostly flat.
- Snap origin should sit at the bottom center of the module footprint.
- Module edges must be readable without relying on tiny trim detail.

## Visual Rules

Use:

- rounded bevels
- thick readable sides
- broad color regions by biome
- simple top surfaces
- large flowers/grass/ice/crystal/lava marks only where they help category read
- clear socket edges for snapping

Avoid:

- brown/stone/gold/teal dominance
- pixel-art cube textures
- tiny noise as decoration
- one-off hero terrain that cannot tile
- generic medieval floor-only kits
- recognizable franchise shapes
- cute voxel diorama islands as the production read
- marketplace icon-pack scenelets as direct modeling sources

## Current Concept Sheets

Use these as exploration sheets, not final accepted art:

- `artifacts/Concepts/BlockField/Batch_20260627/FE_BLOCKFIELD_BATCH_01_CoreTerrain_48Thumbs.png`
- `artifacts/Concepts/BlockField/Batch_20260627/FE_BLOCKFIELD_BATCH_02_BiomeVariants_42Thumbs.png`
- `artifacts/Concepts/BlockField/Batch_20260627/FE_BLOCKFIELD_BATCH_03_TownDungeonKit_48Thumbs.png`

## Production Rule

Model many simple modules before polishing any single module. A field block is
accepted when it snaps correctly, reads from the gameplay camera, and can be
recolored or biome-swapped without rebuilding the mesh.
