# FE_PROP_COMMON_RelicChest_01 Modeling Brief

Status: ready for concept-driven 3D rebuild.

## Source Concept

Primary:

- `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_RelicChest_PetalReliquary_v001.png`

Supporting:

- `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_04_RewardReliquaries_20Thumbs.png`
- `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_FoldedReliquary_StyleBoard_v001.png`

## Asset Role

Reward reliquary.

The asset keeps the legacy runtime name `RelicChest`, but the visual language
must not be a chest. It should read as an opened folded vessel that presents a
reward from a teal socket.

## Gameplay Read

From top-down/high three-quarter camera, the player should read:

- this is a solved or reward state
- the center socket is the reward focus
- the object opened by folding shell-stone petals outward
- it belongs to the folded reliquary product line

## Required Construction Tokens

- low circular or slightly oval base
- opened shell-stone petals with thick beveled rims
- deliberately uneven petal heights and spacing
- teal inner reward socket or glow bowl
- aged brass hinge collars and front clasp
- visible hinge/pivot logic for how the vessel opened
- dark teal underside blocks
- sparse moss staining in cracks, below 10% of visible area

## Forbidden Shapes

- rectangular treasure chest
- hinged wooden box
- loot crate
- coin container
- obvious lock plate as the main read
- skull, dragon, sword, shield, or franchise-like reward motif

## Material Palette

- body: warm ivory weathered shell-stone
- underside: dark teal-black support material
- signal/reward socket: teal glow
- hardware: aged brass
- accent: very limited dull gold and moss stains

## Modeling Targets

- LOD0 under 2500 tris
- LOD1 preserves opened petal silhouette and teal center
- LOD2 preserves circular base, petal rim, and glow read
- pivot at bottom center
- simple convex or cylinder-like collider
- material slots: stone, underside, brass, emissive socket
- closed/idle pose is optional; open reward pose is the production default

## Screenshot Acceptance

Reject the model if the gameplay render reads as:

- a generic treasure chest
- a flower decoration without reward function
- a random magic bowl outside the product line
- a polished lotus shrine with no mechanical opening logic

Accept only if the model reads as a folded reliquary vessel with a clear reward
socket.

## Camera-Distance Gate

Before production modeling approval, render a flat black three-quarter
silhouette at 128 px height. It must still read as an opened reward reliquary,
not a generic bowl, shrine, flower, or chest.
