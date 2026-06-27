# Next Asset Batch

Status: D-020 draft.

This file records the next production-intent art work after the D-020 direction
reset. Historical prototype scenes prove that Unity generation, capture,
validation, and build can work, but they are not the visual target.

User style-lock input from the ARPG prompt-design report is accepted only as
safe abstract production guidance: friendly stylized proportions, saturated but
controlled color planes, rounded readable silhouettes, warm-key/cool-shadow
lighting, ornament hierarchy, FBX-first Unity delivery, LOD/pivot/import QA.
Named game labels and "in the style of" language are not allowed in prompts,
manifest records, or asset specs.

Do not treat inventory count as success. P1/P2 quantity is blocked as a
production claim until P0 hero, exploration tool, first combatant, first room
kit, reward read, and boss read pass the market benchmark and human art/IP
review.

## P0: Replace The Biggest Reads

Status: first-pass generated model pack exists in
`Assets/Art/Production/P0/Models` and `Assets/Prefabs/Production/P0`. Market
benchmark still reports `below_market_finish`, so these are not final
Steam-facing assets.

1. Production hero silhouette
   - Purpose: make the player readable within one second.
   - Requirements: friendly stylized fantasy, compact body, readable hands/tool,
     clear front, broad color planes, rig-ready proportions, no named-game hero
     silhouette and no generic MMO armor.

2. Exploration tool model
   - Purpose: communicate the game's central verb.
   - Requirements: readable in hand, visible glow, clear active state, works
     from top-down camera.

3. First normal enemy
   - Purpose: prove readable top-down combat.
   - Requirements: broken hostile relic rather than creature mascot; visible
     front, clear attack origin, red danger crack, simple rig.

4. Gimmick pedestal
   - Purpose: first tool target.
   - Requirements: idle/active/solved states, clear VFX anchor, simple collider.

5. Reward chest and relic pair
   - Purpose: reward read without inventory bloat.
   - Requirements: chest open state, two distinct relic silhouettes.

## P1: Vertical Slice Environment

Status: first-pass Hub, Region 01, and boss arena modular kits exist in
`Assets/Art/Production/P1/Models` and `Assets/Prefabs/Production/P1`.

1. Hub kit
   - low walls, gate markers, save/return point, warm lighting anchors.

2. Region 01 kit
   - floor, low wall, broken floor, route edge, tool-reactive object, rubble,
     vegetation accent.

3. Boss arena kit
   - readable boundary, boss spawn anchor, danger decal surface, reward exit.

## P2: Reduce Repetition

Status: first-pass Region 02, Region 03, and Bosses 02-04 exist in
`Assets/Art/Production/P2/Models` and `Assets/Prefabs/Production/P2`.

1. Region 02 kit.
2. Region 03 kit.
3. Additional enemy polish.
4. Additional reward variants.

## Manual / External Queue

No paid asset purchase is approved yet.

Use free or repository-authored production attempts first. Third-party assets
must enter through `docs/Legal/LICENSES.md` and the asset register before Unity
import.
