# Concept-First Asset Pipeline

Status: mandatory gate before production-intent 3D modeling.

FOURFOLD ECHOES does not start production modeling from broad fantasy labels.
Every important 3D family must first have an approved concept image and a
short modeling brief derived from that image. The purpose is to avoid averaged
RPG output, derivative silhouettes, and assets that look usable in isolation
but fail as one product line.

## Rule

The asset order is:

1. Batch concept art for the family.
2. Sort concepts into accept / hold / reject.
3. Review accepted concepts for originality, market finish, and product-line
   consistency.
4. Run the camera-distance silhouette gate.
5. Convert the accepted image into a modeling brief.
6. Model the asset in Blender or another DCC.
7. Render it from the gameplay camera and compare it back to the concept.
8. Import only after the screenshot still preserves the concept's shape
   language.

No production-intent 3D model is accepted when its only source is a text prompt,
a generic RPG trope, or an old test mesh.

Do not over-polish a single model before the image pool has enough options to
prove the family direction. More images first, then sorting, then modeling.

During the broad concept phase, keep every generated sheet. A weak full design
may still be useful for part mining: hair, hats, boots, backpacks, weapon scale,
enemy attack reads, boss weak points, NPC role props, or color blocking.

## Anti-Average Gate

Reject the concept before modeling if it depends on any of these shortcuts:

- generic treasure chest, crystal tower, castle wall, sword, shield, wand, slime,
  goblin, dragon, or mascot silhouette
- plain square tile grid, medieval cobblestone, or decorative modular floor
- symmetrical average fantasy forms that could fit many existing games
- named-game style prompts, franchise motifs, recognizable IP shapes, or
  copied palette/composition sets
- tiny scattered noise used as a substitute for structure

Use the user's taste only as abstract production traits: friendly readability,
layered costume/object hierarchy, saturated but controlled color, warm key
light, cool shadow, clear gameplay silhouettes, and polished current-game
presentation.

## Accepted Visual Language

The current product line is **Folded Reliquary Miniatures**.

Core forms:

- folded shell-stone leaves
- thick beveled lips
- brass hinge spines and caps
- teal enamel signal grooves
- dull red wax seams for danger or locked state
- dark underside support masses
- asymmetrical chipped tabs and clasp teeth

The image may be beautiful, but the model must preserve the construction logic:
what moves, what connects, what locks, what opens, and what the player reads
from the gameplay camera.

## Current Reference Set

Use these images as the current concept-first reference set:

- `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_FoldedReliquary_StyleBoard_v001.png`
- `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_FirstFold_KeyArt_v001.png`
- `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_RouteSurface_ModelSheet_v002.png`
- `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_01_ProductLine_24Thumbs.png`
- `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_03_HostileRelics_18Thumbs.png`
- `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_04_RewardReliquaries_20Thumbs.png`
- `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_06_NonStaffExplorationTools_20Thumbs.png`
- `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_RelicChest_PetalReliquary_v001.png`
- `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_ExplorationTool_ClampLens_v001.png`
- `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_MeleeShardling_SealedLockRelic_v001.png`

Keep this image as a rejected comparison example:

- `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_RouteSurface_ModelSheet_v001.png`

`RouteSurface_ModelSheet_v001` is useful as a warning because it remains too
close to a standard rectangular modular floor. `RouteSurface_ModelSheet_v002`
is the modeling source for the next route-surface pass.

## Modeling Brief Template

Before Blender work starts, create a brief with:

- concept image path
- asset family and role
- silhouette read from gameplay camera
- required construction tokens
- forbidden familiar shapes
- material palette
- pivot and connection logic
- LOD target
- screenshot acceptance notes

If the brief cannot name the original construction tokens, the concept is not
specific enough to model.

## Camera-Distance Silhouette Gate

Before modeling approval, flatten the candidate to a black three-quarter
silhouette at 128 px height. The category must remain identifiable without
color, material, cracks, bevels, glow, labels, or surface polish.

Reject the concept if:

- reward, tool, enemy, and route silhouettes cannot be separated from each other
- the design only works because of teal glow, brass trim, cracks, or render
  finish
- the candidate looks familiar once reduced to its large shapes

## Next Modeling Targets

The next 3D polish target remains `FE_ENV_R01_ShortcutBridge_01` because it
sets the route-surface language:

Model it from:

- primary reference: `FE_CONCEPT_RouteSurface_ModelSheet_v002.png`
- supporting reference: `FE_CONCEPT_FoldedReliquary_StyleBoard_v001.png`

It should become an asymmetrical folded-shell shortcut bridge, not a rectangular
stone tile bridge.

After that route-surface pass, use the individual candidate triage order:

1. `FE_PROP_COMMON_RelicChest_01`
   Source: `IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_RelicChest_PetalReliquary_v001.png`
   Direction: opened petal reliquary vessel, not a treasure chest. Accepted,
   but must exaggerate asymmetry and opening mechanics.

2. `FE_PROP_COMMON_ExplorationTool_01`
   Source: `IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_ExplorationTool_ClampLens_v001.png`
   Direction: hold/rework. The current image is too close to the reward
   reliquary; regenerate with stronger tool ergonomics and active-end silhouette
   before modeling.

3. `FE_ENEMY_MELEE_Shardling`
   Source: `IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_MeleeShardling_SealedLockRelic_v001.png`
   Direction: hold/rework. It needs attack direction, weak point, and idle/active
   state before production modeling.
