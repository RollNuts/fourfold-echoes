# Enemy Skeleton Taxonomy

Status: active enemy art foundation.

## Decision

Enemy production starts with skeleton categories, not finished monsters.

The correct sequence is:

1. draw many enemy concepts
2. cluster them by skeleton and gameplay job
3. build neutral enemy mannequins per skeleton category
4. add family parts: head, horn, jaw, wing, tail, shell, armor, weak core
5. add biome/material variants
6. make final encounter models

The playable/NPC chibi mannequin is not an enemy template. Enemies, monsters,
minibosses, bosses, and dragons use separate skeleton families.

## Skeleton Categories

| ID | Template | Use | First Modeling Target | Part Kit |
|---|---|---|---|---|
| ESK-01 | Small biped | fodder, thief, goblin-like jobs without copied goblin styling | waist-high attacker | head swaps, ears/horns, small weapon/tool, backpack, shoulder pads |
| ESK-02 | Heavy biped | blocker, brute, elite guard, ogre-like jobs | shield blocker | chest shell, fist/club arm, front armor, weak back socket |
| ESK-03 | Quadruped beast | wolf/boar/cat/lizard-like jobs | charger | head plates, tusks, mane, tail, saddle armor, paw/claw swaps |
| ESK-04 | Slime/blob | simple mascot enemy, split enemy, elemental puddle | bounce fodder | core, crown cap, eye plates, liquid shell, elemental insert |
| ESK-05 | Floating caster | orb, ghost, drone, wisp, support enemy | ranged/support hazard | halo ring, mask face, side fins, hanging cloth, projectile core |
| ESK-06 | Winged flyer | bat, bird, insect flyer, imp flyer | dive attacker | wing pairs, talons, tail rudder, beak/mask, back core |
| ESK-07 | Insect/arachnid | swarm, crawler, trap layer, poison enemy | ground crawler | leg count variants, shell plates, mandibles, abdomen, spike tail |
| ESK-08 | Serpent/long body | snake, eel, burrower, segmented hazard | line hazard | head, tail, spine segments, dorsal fins, weak belly plates |
| ESK-09 | Dragon/wyvern | rare elite, miniboss, boss seed | small wyvern template | wings, horn crown, jaw, tail club, back spines, chest core |
| ESK-10 | Golem/mech | stone guardian, construct, block-field enemy | slow telegraph bruiser | modular blocks, arm heads, crack core, shoulder slabs, leg feet |
| ESK-11 | Plant/root | root snare, spitter, healer, field hazard | rooted turret | blossom mouth, vine arms, bulb core, root base, leaf armor |
| ESK-12 | Boss multi-anchor | major boss, arena setpiece | first boss mannequin | 4 anchors, weak sockets, phase plates, breakable limbs |

## Gameplay Jobs

Each skeleton category can host multiple encounter jobs, but the job must be
readable before color or UI:

- fodder: small, fast, disposable, simple front read
- blocker: protected front and obvious flank/back weakness
- charger: low forward mass and clear windup posture
- ranged: visible projectile origin
- support: visible buff/heal core or repair appendage
- hazard: body doubles as floor/line/area danger
- elite: same skeleton, larger silhouette and stronger tell surfaces
- boss: readable weak points and phase parts

## Concept Sheet Requirement

Draw concept sheets by skeleton category, not by final enemy name.

Each sheet should include:

- 12 to 16 silhouette thumbnails
- neutral mannequin pose and one attack pose
- same camera angle as gameplay
- no final texture detail
- no famous monster silhouettes
- no franchise dragon head, goblin, slime, or skull defaults
- one clean selected template at the end

## Template Modeling Requirement

For every accepted skeleton category, create a neutral 3D mannequin before
final enemy art:

- no biome material
- no final armor
- no named character identity
- pivot at ground center
- clear front direction
- visible joint landmarks
- simple sockets for head, arms, wings, tail, core, weak point
- turnarounds before costume/parts

## First Template Order

Do not build all twelve at once. Build the templates in gameplay priority:

1. ESK-01 Small biped
2. ESK-03 Quadruped beast
3. ESK-05 Floating caster
4. ESK-09 Dragon/wyvern
5. ESK-10 Golem/mech
6. ESK-04 Slime/blob
7. ESK-07 Insect/arachnid
8. ESK-12 Boss multi-anchor

## Rejection Conditions

- Enemy uses the playable/NPC mannequin body.
- Finished detail is added before the skeleton template works.
- A dragon, slime, goblin, bat, wolf, or golem reads like a known default
  instead of this game's original block-field/action-adventure language.
- Only color separates enemy types.
- The front, attack origin, and weak point cannot be read from the game camera.
