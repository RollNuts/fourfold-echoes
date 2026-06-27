# FE_ENEMY_MELEE_Shardling Modeling Brief

Status: hold; usable only with guardrails.

## Source Concept

Primary:

- `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_MeleeShardling_SealedLockRelic_v001.png`

Supporting:

- `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_03_HostileRelics_18Thumbs.png`
- `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_FoldedReliquary_StyleBoard_v001.png`

## Asset Role

Hostile broken reliquary object.

Despite the name `Shardling`, this should not be modeled as a creature. It is a
sealed lock-relic that crawls, slides, pivots, or lunges through mechanical
motion and pressure-release animation.

## Gameplay Read

From top-down/high three-quarter camera, the player should read:

- dangerous locked state
- red seam energy under cracked shell-stone plates
- a heavy trap-like object with a forward attack wedge
- no face, eyes, mouth, claws, or animal anatomy

## Required Construction Tokens

- broken sealed lock-relic core
- layered stone plates with chipped seams
- dull red wax/fire seams for hostility
- aged brass side locks and collars
- low heavy base blocks
- front locking wedge that indicates attack direction
- readable weak point or pressure seam
- distinct idle/active state silhouette

## Forbidden Shapes

- animal head
- eyes, jaws, teeth, horns, skull, or face
- claws, paws, legs, arms, or biological limbs
- goblin, slime, dragon, mascot, insect, or familiar monster archetype
- readable mouth opening
- cute creature proportions

## Material Palette

- body: warm ivory weathered shell-stone
- base: dark teal-black support blocks
- hardware: aged brass
- danger state: dull red wax/fire seams
- no blood, gore, bone, or horror anatomy

## Modeling Targets

Do not start production modeling until the concept is reworked or approved by
the camera-distance silhouette gate.

- LOD0 under 6000 tris
- LOD1 preserves lock-relic mass, red seams, and front wedge
- LOD2 preserves hostile silhouette without creature features
- pivot at bottom center
- collider: capsule or convex hull plus forward hitbox
- rig: Generic only if animation requires plates/hinges; otherwise animate as
  segmented object
- suggested sockets: `SOCKET_RedSeamVFX`, `SOCKET_ForwardHit`

## Screenshot Acceptance

Reject the model if the gameplay render reads as:

- an animal or monster
- a cute mascot enemy
- a rock beast with a head
- a generic fantasy creature

Accept only if it reads as a dangerous broken reliquary mechanism.

## Camera-Distance Gate

Before production modeling approval, render a flat black three-quarter
silhouette at 128 px height. It must read as a hostile lock-relic with a clear
attack direction, not as a rock egg, turret pod, animal, or generic monster.
