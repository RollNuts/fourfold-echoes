# Enemy Template ESK-01 Small Biped - 2026-06-27

## Asset

- Asset: `FE_ENEMY_TEMPLATE_ESK01_SmallBiped_01`
- Model: `Assets/Art/Production/P0/Models/FE_ENEMY_TEMPLATE_ESK01_SmallBiped_01.obj`
- Prefab: `Assets/Prefabs/Production/P0/FE_ENEMY_TEMPLATE_ESK01_SmallBiped_01.prefab`
- Preview: `artifacts/Previews/ProductionModelPack/FE_ENEMY_TEMPLATE_ESK01_SmallBiped_01.png`
- Turnaround: `artifacts/Previews/ProductionModelPack/FE_ENEMY_TEMPLATE_ESK01_SmallBiped_01_Turnaround.png`
- Socket report: `artifacts/Reports/enemy-template-esk01-small-biped-sockets.json`
- Concept seed crop: `artifacts/Concepts/EnemySkeletonTaxonomy/Batch_20260627/Crops_v001/ESK-01_small_biped.png`

## Scope

- Neutral enemy skeleton template only.
- Not a finished monster.
- Not a playable/NPC body.
- Future variants should scale or swap parts without changing the skeleton unless gameplay requires it.

## Geometry

- Triangles: `5900`
- Primitive counts: `{'box': 1, 'capsule': 10, 'ellipsoid': 38, 'prism': 3}`
- Collider: `capsule_actor`
- Movement mode: `Walk biped`

## Sockets

- `SOCKET_Ground`: (0.0, 0.0, 0.0) - bottom-center pivot and spawn grounding
- `SOCKET_Head`: (0.0, 1.12, 0.03) - head or mask swap
- `SOCKET_Mouth`: (0.0, 1.12, 0.225) - short bite/spit origin for variants
- `SOCKET_ChestCore`: (0.0, 0.8, 0.283) - front core and main readability mark
- `SOCKET_WeakPoint`: (0.0, 0.82, 0.31) - targetable weak point, separate from root collider
- `SOCKET_Back`: (0.0, 0.85, -0.205) - backpack, shell, or rear weak attachment
- `SOCKET_AttackOrigin`: (0.0, 0.83, 0.39) - forward attack hitbox/VFX origin
- `SOCKET_LeftHand`: (-0.335, 0.47, 0.205) - left hand weapon/tool/claw attachment
- `SOCKET_RightHand`: (0.335, 0.47, 0.205) - right hand weapon/tool/claw attachment
- `SOCKET_Cast`: (0.0, 0.9, 0.36) - cast warning and projectile startup
- `SOCKET_HitVfx`: (0.0, 0.78, 0.12) - impact and damage VFX center

## Minimum Animation Clips

- `idle_loop`: 48 frames - small weight shift; feet stay grounded
- `move_loop`: 24 frames - walk biped loop; no sliding
- `turn_in_place`: 18 frames - short readable turn for target reacquire
- `attack_a`: 22 frames - fast forward swipe from hand/attack origin
- `attack_b`: 30 frames - heavier lunge, windup visible by frame 8
- `hit_light`: 12 frames - small recoil, keeps feet under collider
- `hit_heavy`: 20 frames - strong recoil, weak-point flash
- `stagger`: 34 frames - temporary exposed weak-point state
- `death`: 42 frames - collapse without gore
- `spawn`: 30 frames - rise/pop-in from ground socket
