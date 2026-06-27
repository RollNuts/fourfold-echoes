# Enemy Template ESK-03 Quadruped - 2026-06-27

## Asset

- Asset: `FE_ENEMY_TEMPLATE_ESK03_Quadruped_01`
- Model: `Assets/Art/Production/P0/Models/FE_ENEMY_TEMPLATE_ESK03_Quadruped_01.obj`
- Prefab: `Assets/Prefabs/Production/P0/FE_ENEMY_TEMPLATE_ESK03_Quadruped_01.prefab`
- Preview: `artifacts/Previews/ProductionModelPack/FE_ENEMY_TEMPLATE_ESK03_Quadruped_01.png`
- Turnaround: `artifacts/Previews/ProductionModelPack/FE_ENEMY_TEMPLATE_ESK03_Quadruped_01_Turnaround.png`
- Socket report: `artifacts/Reports/enemy-template-esk03-quadruped-sockets.json`
- Concept seed crop: `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-03_quadruped_beast.png`

## Scope

- Neutral quadruped enemy skeleton template only.
- Not a finished beast, wolf, boar, lizard, dragon, or crystal monster.
- Future variants should scale or swap head, tail, back, paw/claw, armor, and weak-core parts.

## Geometry

- Triangles: `6800`
- Primitive counts: `{'box': 1, 'capsule': 11, 'ellipsoid': 43, 'prism': 6}`
- Collider: `capsule_actor`
- Movement mode: `Run/charge quadruped`

## Sockets

- `SOCKET_Ground`: (0.0, 0.0, 0.0) - bottom-center pivot and four-foot grounding reference
- `SOCKET_Head`: (0.0, 0.73, 0.705) - head, mask, tusk, or crest swap
- `SOCKET_Mouth`: (0.0, 0.73, 0.925) - bite/spit point; separate from charge hitbox
- `SOCKET_AttackOrigin`: (0.0, 0.735, 1.03) - charge/ram hitbox origin
- `SOCKET_ChestCore`: (0.0, 0.625, 0.42) - front chest core and charger read
- `SOCKET_WeakPoint`: (0.225, 0.68, -0.075) - side/back targetable weak point
- `SOCKET_Back`: (0.0, 0.69, -0.08) - saddle armor, mane, crest, or shell attachment
- `SOCKET_Tail`: (0.0, 0.645, -0.69) - tail, tail club, or rear VFX attachment
- `SOCKET_LeftHand`: (-0.21, 0.12, 0.52) - front-left paw/claw attachment
- `SOCKET_RightHand`: (0.21, 0.12, 0.52) - front-right paw/claw attachment
- `SOCKET_Cast`: (0.0, 0.76, 0.875) - telegraph marker during charge windup
- `SOCKET_HitVfx`: (0.0, 0.6, 0.1) - body impact center

## Minimum Animation Clips

- `idle_loop`: 54 frames - subtle breathing; four paws remain planted
- `move_loop`: 28 frames - trot loop with stable foot contacts
- `turn_in_place`: 24 frames - short quadruped pivot without foot sliding
- `charge_windup`: 24 frames - front mass lowers and braces; attack tell visible by frame 8
- `charge`: 20 frames - active hitbox from SOCKET_AttackOrigin
- `skid_recovery`: 26 frames - charge end skid; no instant turn
- `attack_b`: 30 frames - short bite/body bump alternate attack
- `hit_light`: 12 frames - small side recoil, paws keep readable contact
- `hit_heavy`: 22 frames - strong recoil, weak point flashes
- `stagger`: 38 frames - brief exposed weak-point state
- `death`: 46 frames - collapse onto side without gore
- `spawn`: 32 frames - enter from crouch/ground socket
