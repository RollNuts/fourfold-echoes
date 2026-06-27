# Enemy Template ESK-05 Floating Caster - 2026-06-27

## Asset

- Asset: `FE_ENEMY_TEMPLATE_ESK05_FloatingCaster_01`
- Model: `Assets/Art/Production/P0/Models/FE_ENEMY_TEMPLATE_ESK05_FloatingCaster_01.obj`
- Prefab: `Assets/Prefabs/Production/P0/FE_ENEMY_TEMPLATE_ESK05_FloatingCaster_01.prefab`
- Preview: `artifacts/Previews/ProductionModelPack/FE_ENEMY_TEMPLATE_ESK05_FloatingCaster_01.png`
- Turnaround: `artifacts/Previews/ProductionModelPack/FE_ENEMY_TEMPLATE_ESK05_FloatingCaster_01_Turnaround.png`
- Socket report: `artifacts/Reports/enemy-template-esk05-floating-caster-sockets.json`
- Concept seed crop: `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-05_floating_caster.png`

## Scope

- Neutral floating caster enemy skeleton template only.
- Not a finished ghost, wizard, drone, angel, floating-eye enemy, or playable caster.
- Future variants should scale or swap side parts, crest, back plate, core, aura, and optional shield/focus modules.

## Geometry

- Triangles: `2176`
- Primitive counts: `{'capsule': 3, 'ellipsoid': 16, 'prism': 5}`
- Collider: `capsule_actor`
- Movement mode: `Hover caster`

## Sockets

- `SOCKET_Ground`: (0.0, 0.0, 0.0) - bottom-center pivot and hover height reference; not a foot contact
- `SOCKET_ChestCore`: (0.0, 0.322, 0.91) - front exposed casting core
- `SOCKET_Mouth`: (0.0, 0.322, 0.91) - mouth-equivalent casting face on the front core
- `SOCKET_AttackOrigin`: (0.0, 0.42, 0.91) - projectile and beam origin; visibly in front of the core
- `SOCKET_Cast`: (0.0, 0.42, 0.91) - charge/release VFX origin tied to the same front core
- `SOCKET_WeakPoint`: (0.0, 0.0, 1.205) - top weak core exposed during stagger or cast recovery
- `SOCKET_HitVfx`: (0.0, 0.0, 0.9) - central body impact VFX
- `SOCKET_Back`: (0.0, -0.275, 0.94) - rear support socket for capes, fins, back plates, or status VFX
- `SOCKET_Head`: (0.0, -0.018, 1.155) - optional top mask, crest, or antenna swap; not a humanoid head
- `SOCKET_LeftHand`: (-0.385, 0.005, 0.9) - left side-part socket for shield, winglet, or spell focus
- `SOCKET_RightHand`: (0.385, 0.005, 0.9) - right side-part socket for shield, winglet, or spell focus

## Minimum Animation Clips

- `idle_loop`: 60 frames - slow hover bob; ground socket stays fixed and body never touches ground
- `move_loop`: 36 frames - drift loop with body lean and side-part lag
- `turn_in_place`: 24 frames - rotate around SOCKET_Ground while front core remains readable
- `attack_a`: 36 frames - short projectile: 10f charge, release from SOCKET_AttackOrigin, 12f recoil
- `attack_b`: 48 frames - long cast or radial pulse: clear charge, release, and recovery phases
- `hit_light`: 14 frames - small body wobble; side parts overshoot and settle
- `hit_heavy`: 24 frames - strong recoil exposing SOCKET_WeakPoint
- `stagger`: 36 frames - top weak core exposed; casting disabled
- `death`: 48 frames - body collapses downward into dissipating fragments without gore
- `spawn`: 36 frames - appears from hover anchor, then lifts into idle clearance
