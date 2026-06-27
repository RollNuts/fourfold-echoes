# Game Asset Priority

Status: active production priority.

## Decision

Small props alone do not make the game. The asset pipeline must prioritize the
minimum set needed for a playable ARPG loop:

1. player can move
2. player can attack/use a tool
3. enemies can be fought
4. field can be assembled
5. reward/goal can be read
6. UI can show state

Decorative props, landmarks, and polish come after this loop exists.

## P0 Playable Loop Assets

Build or concept first:

- base player body
- 4 playable role silhouettes
- shared player rig/socket plan
- player weapon/tool set: melee, ranged, magic/tool, shield/guard
- common enemy melee
- common enemy ranged
- tank/blocker enemy
- charger enemy
- hazard/trap enemy
- support/healer enemy
- first miniboss
- first boss
- block-field terrain first 20 modules
- reward vessel / reward pad
- checkpoint/save pad
- interaction target
- core UI icons: HP, stamina/MP, attack, skill, item, interact, map, quest
- VFX markers: hit, heal, danger telegraph, reward pickup, interact pulse

## Current P0 Model Sprint

The 2026-06-27 model sprint moved from broad concept generation to first-pass
3D output. Current generated P0 coverage is:

- 54 P0 model assets total
- 6 playable/hero models
- 3 service NPC models
- 8 enemy models
- 2 boss/miniboss models
- 5 equipment models
- 22 block-field/environment models
- 8 reward/interact/utility props

Sprint report: `artifacts/Reports/p0-model-sprint-20260627.md`.

## First 12 Character/Encounter Targets

These are the first character-side targets to regenerate and later model:

1. `SignatureHeroLead`
2. `PlayableHeavyTank`
3. `PlayableCaster`
4. `PlayableRangerScout`
5. `TownMerchantNPC`
6. `BlacksmithUpgradeNPC`
7. `CartographerQuestGuide`
8. `BasicMeleeFodderEnemy`
9. `ShieldEnemy`
10. `RangedTurretHazard`
11. `EliteCharger`
12. `FirstMiniboss`

Current broad sheets are part-mining sources, not final character approval.
Enemies and bosses are closer to useful production seeds than the playable hero
sheet. Playables need an originality pass before modeling.

## P1 Content Expansion

- NPC service cast: shop, blacksmith, cook, healer, quest, map/transport,
  appraiser, upgrade vendor, training instructor
- equipment families: simple sword, bow, staff/tool, shield, hammer, armor,
  boots, cape, bag
- item/material icons
- block-field biome variants: grass, village, water, cave/dungeon, snow
- village props
- dungeon props

## P2 Later Polish

- decorative props
- hero landmarks
- extra biome variants
- rare equipment variants
- optional NPC body variants
- non-critical VFX
- high-detail boss phase variants

## Style Direction

Characters and enemies should be more pop/deformed than realistic:

- 2.8 to 3.2 heads tall
- large heads and hands
- chunky boots
- simple broad color regions
- low/mid-poly friendly shapes
- readable roles from camera distance

Avoid:

- ultra-real humans
- dense MMO armor
- small filigree
- famous-character silhouettes
- average fantasy clones
- realism that cannot be modeled quickly

## Keep-All Concept Policy

All generated sheets are kept in the private art vault staging area. Weak full
designs may still contain useful parts: hair, hats, boots, backpacks, weapon
scale, color blocking, attack reads, weak points, service props, or silhouettes.
