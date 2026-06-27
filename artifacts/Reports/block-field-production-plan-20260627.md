# Block Field Production Plan

Generated: 2026-06-27

## Verdict

Move field production to a block/chunk-based kit.

The goal is not to make one beautiful terrain piece. The goal is to make enough
snap modules to build playable fields quickly, then improve art direction after
the game can run through a real route.

External review note: the current image sheets are exploration references, not
direct modeling references. They still read too much like cute voxel diorama
icons and marketplace thumbnails. The production pass must start from terrain
grammar, not decorative scenelets.

## Concept Sheets

| Sheet | Path | Use |
| --- | --- | --- |
| Core terrain | `artifacts/Concepts/BlockField/Batch_20260627/FE_BLOCKFIELD_BATCH_01_CoreTerrain_48Thumbs.png` | core snap vocabulary |
| Biome variants | `artifacts/Concepts/BlockField/Batch_20260627/FE_BLOCKFIELD_BATCH_02_BiomeVariants_42Thumbs.png` | palette and biome expansion |
| Town/dungeon kit | `artifacts/Concepts/BlockField/Batch_20260627/FE_BLOCKFIELD_BATCH_03_TownDungeonKit_48Thumbs.png` | practical playable town/dungeon modules |

## Production Order

Build the first playable block kit in this order. These are the first 20 modules
to model before portals, crystals, trees, wells, shops, or decorative biome
props:

1. `BF_GRASS_Floor_1x1_Flat`
2. `BF_GRASS_Floor_1x2_Straight`
3. `BF_GRASS_Floor_2x2_Plaza`
4. `BF_GRASS_Edge_Straight`
5. `BF_GRASS_Edge_OuterCorner`
6. `BF_GRASS_Edge_InnerCorner`
7. `BF_GRASS_Height_Cliff_Up1`
8. `BF_STONE_Floor_1x1_Clean`
9. `BF_STONE_Floor_2x2_Clean`
10. `BF_STONE_Floor_1x1_Cracked`
11. `BF_STONE_Wall_Straight_1x1`
12. `BF_STONE_Wall_OuterCorner`
13. `BF_STONE_Wall_InnerCorner`
14. `BF_STONE_Arch_Doorway`
15. `BF_STAIR_Straight_Up1`
16. `BF_STAIR_CornerLanding_Up1`
17. `BF_WATER_ChannelEdge_Straight`
18. `BF_BRIDGE_Wood_1x2`
19. `BF_FENCE_Railing_Straight`
20. `BF_HAZARD_Floor_Spike`

This gives enough pieces for a playable route, village hub test, dungeon room,
water crossing, height change, and hazard.

## Modeling Rules

- Model fast and simple.
- Start greybox-to-final, not final-detail-first.
- Use block footprints first, polish second.
- Every module must snap on a 2m grid.
- Every module gets a simple collider immediately.
- Every module gets a top-down gameplay screenshot.
- Material count should stay low: top, side, trim, detail/emissive if needed.
- Avoid final texture polish until at least 25 modules exist.
- Separate kit families before decoration: grass floors, stone floors, walls,
  stairs, water/bridge, fence, hazard.

## Visual Guardrails

Use block construction logic, but avoid block-game copying:

- no pixel textures
- no cubic dirt/grass copy language
- no exact voxel cube presentation
- no all-brown terrain
- no famous franchise palette/silhouette
- no cute voxel diorama island as the production read
- no marketplace icon-pack composition as direct modeling source

The model should read as a stylized ARPG diorama block, not a Minecraft block.

Biome variants need structural differences, not only color swaps:

- desert: eroded slabs and sand-sunk edges
- swamp: sunken rounded stones and waterlogged margins
- snow: softened caps and compressed paths
- lava: fractured basalt and glow cracks
- town: cut-stone precision and curbs
- jungle: root invasion and broken edges

## First Vertical Slice Goal

Build one short playable route:

- grass spawn
- path bend
- bridge over water
- ramp to raised platform
- pressure plate
- dungeon doorway
- checkpoint pad
- reward pad

The route is successful when it can be assembled from repeated modules without
unique hand-placed terrain.
