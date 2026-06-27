# Individual Candidate Triage

Generated: 2026-06-27

## Verdict

The concept-first workflow is the right next step.

Do not deep-polish production 3D until a family has multiple image candidates
and the useful parts have been sorted. The current image pool is not all bad:
several candidates have strong product-line value, but each accepted direction
needs explicit guardrails so modeling does not drift back to average fantasy
props.

## Candidate Results

| Candidate | Concept Path | Status | Modeling Priority |
| --- | --- | --- | --- |
| `FE_PROP_COMMON_RelicChest_01` | `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_RelicChest_PetalReliquary_v001.png` | accept primary | P0 |
| `FE_PROP_COMMON_ExplorationTool_01` | `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_ExplorationTool_ClampLens_v001.png` | hold / rework | P1 concept rework |
| `FE_ENEMY_MELEE_Shardling` | `artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_MeleeShardling_SealedLockRelic_v001.png` | hold with guardrails | P2 concept rework |

## Accepted Reads

`FE_PROP_COMMON_RelicChest_01`:

- use it as an opened petal reliquary vessel, not a box
- keep the low circular base, stone petal shells, teal inner socket, and aged
  brass hinge/collar logic
- it can replace treasure-chest language for rewards across the product line
- before 3D, exaggerate petal asymmetry and visible opening mechanics so it
  does not collapse into a generic lotus shrine container

`FE_PROP_COMMON_ExplorationTool_01`:

- hold the current image as useful reference, but do not model it as-is
- it is too close to the reward vessel's stone-petal, teal-lens, brass-ring
  language
- the rework needs stronger tool identity: hand ergonomics, active end, compact
  scale cues, and a silhouette that reads as player-used equipment before it
  reads as a miniature reliquary

`FE_ENEMY_MELEE_Shardling`:

- use the sealed broken lock-relic idea, not a creature
- keep the stone plates, red wax/fire seams, low crawlable mass, and front
  locking wedge
- model as an animated trap/relic object with no face, eyes, jaws, claws, or
  biological limbs
- do not production-model until the attack direction, weak point, and idle/active
  state are readable without material detail

## Reject Rules Before Modeling

Reject before 3D work if a generated variant reads as:

- generic treasure chest or loot box
- long-handled staff, wand, key, weapon, or tool-as-sword
- animal head, mascot enemy, goblin, slime, skull, dragon, or creature with
  eyes
- circular arena prop without a clear function
- familiar franchise silhouette, named-game motif, or copied palette structure

## Modeling Order

1. Model `FE_PROP_COMMON_RelicChest_01` from the petal reliquary image.
2. Re-generate `FE_PROP_COMMON_ExplorationTool_01` with stronger tool silhouette
   separation before modeling.
3. Re-generate or heavily guardrail `FE_ENEMY_MELEE_Shardling` before rigging.

## Camera-Distance Silhouette Gate

Before any accepted concept is modeled, reduce it to a flat black three-quarter
view at 128 px height. The gameplay category must still be identifiable without
color, material, cracks, bevels, glow, or labels.

Required reads:

- `RelicChest`: reward vessel / opened reliquary
- `ExplorationTool`: player-held tool, not a small reliquary
- `Shardling`: hostile lock-relic with attack direction, not a rock egg

If the three silhouettes cannot be separated from each other at this size, the
concept fails triage.

## Process Rule

The next pass should create more image candidates, not fewer. The useful loop is
batch generate, classify, then model only the winners.
