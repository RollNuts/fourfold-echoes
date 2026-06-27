# Enemy Skeleton Taxonomy v001

## Source

- Sheet: `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/FE_ENEMY_SKELETON_TAXONOMY_16Thumbs_v001.png`
- Crops: `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001`
- Scope: enemy, monster, miniboss, and boss skeletons only
- Excluded: playable and friendly NPC mannequin

## Classification

| Slot | ID | Template | Status | Crop | Note |
|---:|---|---|---|---|---|
| 1 | ESK-01 | Small biped | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-01_small_biped.png` | Good base for fodder. Strip final panel details before mannequin modeling. |
| 2 | ESK-02 | Heavy biped | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-02_heavy_biped.png` | Good blocker/brute mass. Needs neutral joint landmarks and less finished armor. |
| 3 | ESK-03 | Quadruped beast | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-03_quadruped_beast.png` | Strong charger base. Remove crystal color identity for neutral template. |
| 4 | ESK-04 | Slime/blob | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-04_slime_blob.png` | Good non-biped body category. Core socket is readable. |
| 5 | ESK-05 | Floating caster | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-05_floating_caster.png` | Good ranged/support hover skeleton. Keep side shields as optional parts. |
| 6 | ESK-06 | Winged flyer | revise_before_template | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-06_winged_flyer.png` | Useful silhouette, but too close to familiar bat/imp defaults. Needs more original head language. |
| 7 | ESK-07 | Insect/arachnid | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-07_insect_arachnid.png` | Good crawler template. Simplify shell into neutral body mannequin. |
| 8 | ESK-08 | Serpent/long body | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-08_serpent_long_body.png` | Useful segmented line-hazard body. Head should become swappable. |
| 9 | ESK-09 | Dragon/wyvern | revise_before_template | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-09_dragon_wyvern.png` | Useful dragon category, but the head/wing read is too familiar. Must be reworked before modeling. |
| 10 | ESK-10 | Golem/mech | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-10_golem_mech.png` | Good slow telegraph bruiser. Works as a separate construct skeleton. |
| 11 | ESK-11 | Plant/root | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-11_plant_root.png` | Good rooted turret/support body. Keep as plant hazard skeleton, not NPC body. |
| 12 | ESK-12 | Boss multi-anchor | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-12_boss_multi-anchor.png` | Useful multi-limb socket boss seed. Needs clearer weak-point hierarchy. |
| 13 | ESK-13 | Crab shell | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-13_crab_shell.png` | Good alternate low crawler. Good for shielded side-step enemy. |
| 14 | ESK-14 | Rolling shell | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-14_rolling_shell.png` | Good roll/charge hazard. Needs unfolded attack pose concept later. |
| 15 | ESK-15 | Mimic/tool construct | accept_template_seed | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-15_mimic_tool_construct.png` | Good object-enemy skeleton. Keep because ARPG needs surprise/interact enemies. |
| 16 | ESK-16 | Tall support caster | revise_before_template | `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-16_tall_support_caster.png` | Readable support silhouette, but too humanoid/staff-like. Rework to avoid generic caster. |

## Next Template Order

1. ESK-01
2. ESK-03
3. ESK-05
4. ESK-09
5. ESK-10
6. ESK-04
7. ESK-07
8. ESK-12

## Production Note

These are not final enemies. Accepted slots become neutral 3D mannequins first, then receive heads, wings, horns, tails, shells, armor, weak cores, materials, and biome variants.
