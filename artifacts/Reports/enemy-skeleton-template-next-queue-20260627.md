# Enemy Skeleton Template Next Queue - 2026-06-27

## Completed This Pass

- `ESK-01 Small biped`
- Asset: `FE_ENEMY_TEMPLATE_ESK01_SmallBiped_01`
- Status: neutral enemy skeleton template candidate
- Verified: schema, prompt contract, generated asset validation, Unity import, product validate
- `ESK-03 Quadruped beast`
- Asset: `FE_ENEMY_TEMPLATE_ESK03_Quadruped_01`
- Status: neutral quadruped charger skeleton template candidate
- Verified: schema, prompt contract, generated asset validation, Unity import, product validate

## Next Recommended Templates

1. `ESK-05 Floating caster`
   - Why: covers ranged pressure, support/healer, drone, wisp, and caster variants without needing walk cycles.
   - Must lock: hover height, projectile/cast origin, side attachment sockets, weak core socket.

2. `ESK-09 Dragon/wyvern`
   - Why: user explicitly called out dragons; high value, but needs stronger originality guard before modeling.
   - Must lock: wing sockets, mouth attack origin, tail attack origin, chest weak core, ground/air mode split.
   - Warning: current concept crop is too familiar as a dragon silhouette; redraw/rework before 3D template.

3. `ESK-10 Golem/mech`
   - Why: useful for slow telegraph bruisers and block-field world logic.
   - Must lock: modular limb sockets, crack core, heavy foot contact, slow attack origin.

## Production Rule

Do not create finished enemy art until the neutral template has:

- accepted silhouette
- prefab socket children
- collider profile
- movement mode
- minimum animation clip list
- schema_v1 record
- Unity import pass
