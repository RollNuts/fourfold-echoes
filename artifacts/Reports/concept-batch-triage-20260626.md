# Concept Batch Triage

Generated: 2026-06-26

## Verdict

The workflow should stay image-first for now.

Do not polish one model deeply until the asset family has enough concept
variants to sort. The useful gate is:

1. batch concept images
2. triage into accept / hold / reject
3. write a short modeling brief only for accepted candidates
4. model the accepted candidate

## Batch Sheets

| Sheet | Path | Triage |
| --- | --- | --- |
| Product line 24 thumbs | `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_01_ProductLine_24Thumbs.png` | accept parts / hold low walls |
| First Fold rooms 12 thumbs | `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_02_FirstFoldRooms_12Thumbs.png` | accept a few asymmetric layouts / reject circular arena-heavy layouts |
| Hostile relics 18 thumbs | `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_03_HostileRelics_18Thumbs.png` | strong accept pool |
| Reward reliquaries 20 thumbs | `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_04_RewardReliquaries_20Thumbs.png` | strong accept pool / reject box-like rewards |
| Tool receiver pairs 16 thumbs | `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_05_ToolReceiverPairs_16Thumbs.png` | receivers useful / handheld tools too staff-like |
| Non-staff exploration tools 20 thumbs | `artifacts/Concepts/FoldedReliquary/Batch_20260626/FE_CONCEPT_BATCH_06_NonStaffExplorationTools_20Thumbs.png` | accept pool for handheld tool redesign |

## Accept Pool

Route surfaces:

- use the center/right top-row route modules from the product-line sheet
- prefer socketed leaf plates and asymmetric folded profiles
- avoid the first two straight slab-like route modules

Rooms:

- use the more asymmetric broken layouts from the room sheet
- avoid circular arena-first rooms unless heavily broken and de-centered
- keep route logic visible through signal grooves, not UI labels

Hostile relics:

- accept floating sealed relics, vertical broken lock forms, and non-creature
  trap relics
- hold legged forms for review because they drift toward animal/monster reads
- reject anything that needs eyes, faces, skulls, or limbs to explain hostility

Rewards:

- accept open petal vessels, low socket bowls, triangular folded vessels, and
  small solved-state reliquaries
- reject box-like rewards, cylinder canisters, coin/loot implications, and
  anything that returns to treasure chest language

Exploration tool:

- use the non-staff sheet, not the first tool/receiver sheet, as the source for
  handheld tool redesign
- accept compact pucks, clamp devices, folding lens jaws, and two-handed compact
  mechanisms
- reject long handles, wand/staff silhouettes, literal keys, weapons, guns, and
  antenna-like devices

Tool receiver:

- accept socket bowls, petal receivers, and vertical cradle receivers from both
  receiver sheets
- hold tall tower-like receivers until they are shortened or widened for the
  gameplay camera

## Reject Rules

Reject a candidate before modeling if it reads as:

- average fantasy prop
- rectangular tile bridge
- circular arena by default
- generic treasure chest or loot box
- staff, wand, literal key, gun, sword, shield, or antenna
- animal, goblin, slime, dragon, skull, or mascot creature
- decorative stone wall without folded-reliquary function

## Next Modeling Candidates

The next candidates should be modeled one at a time from accepted image pools:

1. `FE_PROP_COMMON_ExplorationTool_01`
   Source: `FE_CONCEPT_BATCH_06_NonStaffExplorationTools_20Thumbs.png`
   Direction: compact folded lens/clamp device, not staff.

2. `FE_PROP_COMMON_RelicChest_01`
   Source: `FE_CONCEPT_BATCH_04_RewardReliquaries_20Thumbs.png`
   Direction: replace chest language with an open petal vessel or low socket bowl.

3. `FE_ENEMY_MELEE_Shardling`
   Source: `FE_CONCEPT_BATCH_03_HostileRelics_18Thumbs.png`
   Direction: broken sealed relic/trap object, not creature.

4. `FE_ENV_R01_ShortcutBridge_01`
   Source: `FE_CONCEPT_BATCH_01_ProductLine_24Thumbs.png` plus
   `FE_CONCEPT_RouteSurface_ModelSheet_v002.png`
   Direction: socketed folded leaf route, not rectangular slab bridge.

## Process Change

Image batch triage now precedes model polish. A model is not worth deep work
until the concept pool proves the family has enough non-derivative, non-average
options.
