# FE_PROP_COMMON_ExplorationTool_01 Modeling Brief

Status: hold; regenerate before production modeling.

## Source Concept

Primary:

- `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_ExplorationTool_ClampLens_v001.png`

Supporting:

- `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_06_NonStaffExplorationTools_20Thumbs.png`
- `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_FoldedReliquary_StyleBoard_v001.png`

## Asset Role

Handheld exploration tool.

This is the player's compact relic instrument for reading sockets and routes.
It must not become a staff, wand, key, gun, sword, or antenna.

The current image is a useful direction but is too visually close to
`FE_PROP_COMMON_RelicChest_01`. Do not model it as-is.

## Gameplay Read

From top-down/high three-quarter camera, the player should read:

- a compact tool, not a weapon
- a central teal lens used for reading/activating reliquary mechanisms
- folded jaws that can clasp or focus around a socket
- a short loop grip for holding, not a long handle

## Required Construction Tokens

- C-shaped folded shell-stone jaws, reduced relative to ergonomic grip mass
- central teal lens set in aged brass
- short leather loop or compact grip with clear hand scale
- brass hinge barrels at the jaw base
- dark teal underside cavity
- one obvious activation face
- visible active end distinct from the reward reliquary's teal socket

## Forbidden Shapes

- staff
- wand
- literal key
- sword, gun, shield, or weapon silhouette
- antenna or tuning fork
- long handle longer than the head assembly
- modern flashlight, scanner, radio, or sci-fi gadget read
- miniature petal reliquary vessel
- same top silhouette as `FE_PROP_COMMON_RelicChest_01`

## Material Palette

- shell body: warm ivory weathered stone
- grip: dark leather
- hardware: aged brass
- lens and channel: oxidized teal / teal glow
- underside: dark teal-black

## Modeling Targets

Do not start production modeling until the concept is reworked and passes the
camera-distance silhouette gate.

- LOD0 under 2000 tris
- LOD1 preserves C-jaw silhouette and central lens
- LOD2 preserves clamp/lens read from the gameplay camera
- pivot at grip center or hand socket attachment point
- optional socket: `SOCKET_ToolLens`
- material slots: shell, brass, leather, emissive lens

## Screenshot Acceptance

Reject the model if the gameplay render reads as:

- a staff or wand
- a key
- a weapon
- a generic magical gadget unrelated to the folded reliquary product line

Accept only if the silhouette remains a compact clamp/lens device.

## Camera-Distance Gate

Before production modeling approval, render a flat black three-quarter
silhouette at 128 px height. It must read as a player-held tool, not as a small
reward reliquary, key, staff, or weapon.
