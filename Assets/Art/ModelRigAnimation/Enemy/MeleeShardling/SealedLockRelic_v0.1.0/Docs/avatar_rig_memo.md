# Avatar / Rig Memo

Asset: MDL_Enemy_MeleeShardling_SealedLockRelic_v0.1.0

## Rig

- Unity rig type: mechanical transform hierarchy.
- Rig style: generated `.anim` clips drive named transforms; no Humanoid Avatar and no skin weights.
- ROOT remains at origin for every clip.
- Scale: 1 Blender unit = 1 Unity meter.
- Axis: authored Blender Z-up, forward along Blender -Y; exported with FBX axis_forward=-Z and axis_up=Y for Unity Y-up/+Z-forward import.
- Root motion: off; movement is controller-driven.
- Locomotion contact uses base compression frames because the model has no feet.

## Control Hierarchy

- `ROOT`
- `CTRL_Base`
- `CTRL_Body`
- `CTRL_FrontWedge`
- `CTRL_TopPlate_L`
- `CTRL_TopPlate_R`
- `CTRL_RedSeam`
- `CTRL_LeftLock`
- `CTRL_RightLock`

## Sockets

- `SOCKET_Ground` parent `ROOT` at (0.0, 0.0, 0.0): ground pivot and spawn point
- `SOCKET_ChestCore` parent `CTRL_Body` at (0.0, -0.02, 0.54): central red seam and cast charge core
- `SOCKET_WeakPoint` parent `CTRL_Body` at (0.0, -0.18, 0.65): targetable pressure seam
- `SOCKET_Back` parent `CTRL_Base` at (0.0, 0.48, 0.42): rear impact/VFX attachment
- `SOCKET_AttackOrigin` parent `CTRL_FrontWedge` at (0.0, -0.82, 0.33): forward wedge attack origin
- `SOCKET_ForwardHit` parent `CTRL_FrontWedge` at (0.0, -0.96, 0.3): forward hitbox center
- `SOCKET_RedSeamVFX` parent `CTRL_RedSeam` at (0.0, -0.18, 0.67): red seam VFX emission point
- `SOCKET_Cast` parent `CTRL_RedSeam` at (0.0, -0.34, 0.72): cast charge/release origin
- `SOCKET_HitVfx` parent `CTRL_Body` at (0.0, -0.08, 0.45): main impact VFX center

## Optimization

- LOD0 triangles: `538` / 6000.
- Material slots: `4` / 4.
- LOD1/LOD2 are not required for this first tiny technical pack, but proposed targets are LOD1 <= 3000 tris and LOD2 <= 1000 tris if the model gains detail.
- Keep swinging/loose parts transform-driven; avoid cloth/physics until a later optimization pass approves it.

## Unity Import

- Import model FBX with scale factor 1.0.
- The model FBX may import with Animation Type None because it is a mechanical transform hierarchy, not a skinned Avatar.
- Use the generated `.anim` AnimationClips for runtime playback; the source `ANM_*.fbx` files remain as named motion source exports.
- Enable Loop Time only for Idle, Walk, Run, AttackLoop, and ChannelLoop; the generated clips already carry those loop flags.
- Keep Bake Into Pose/original root transform settings so ROOT remains at origin.

## Sample Scene Setup

- Generated AnimatorController: `Runtime/Animator/AC_Enemy_MeleeShardling_SealedLockRelic_v0.1.0.controller`.
- Generated runtime prefab: `Runtime/Prefabs/PF_Enemy_MeleeShardling_SealedLockRelic_v0.1.0.prefab`.
- Generated preview scene: `Runtime/Scenes/SCN_Enemy_MeleeShardling_SealedLockRelic_RuntimePreview_v0.1.0.unity`.
- Prefab root starts at `(0, 0, 0)` and keeps `Animator.applyRootMotion = false`.
- The prefab includes a root BoxCollider and a trigger hitbox at `SOCKET_ForwardHit/HITBOX_ForwardPreview`.
- The prefab root includes `MeleeShardlingAnimationEventRelay` so AnimationEvents such as `hit_active_start`, `hit_active_end`, and `projectile_release` have receiver methods.
- `HITBOX_ForwardPreview` starts disabled and is enabled only between `hit_active_start` and `hit_active_end`.
- Attach red seam VFX to `SOCKET_RedSeamVFX`, projectile/cast VFX to `SOCKET_Cast`, and impact VFX to `SOCKET_HitVfx`.
- Animator states: Idle default, Walk/Run locomotion, AttackStart -> AttackLoop -> AttackEnd, CastStart -> ChannelLoop -> CastRelease, hit reactions, Knockdown, Death, Interact.
