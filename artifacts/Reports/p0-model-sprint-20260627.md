# P0 Model Sprint - 2026-06-27

## Decision

Stop broad image generation for now. The priority moved to Unity-usable first-pass
3D assets that make the game loop visible: playable cast, service NPCs, enemies,
equipment, block-field combat space, checkpoint, and reward destination.

## Output

- Total generated model assets: 145
- P0 generated model assets: 54
- P0 category split:
  - Character: 6
  - NPC: 3
  - Enemy: 8
  - Boss: 2
  - Equipment: 5
  - Environment: 22
  - Prop: 8

## Added P0 Sprint Assets

| Model | Role |
| --- | --- |
| `FE_CHAR_PLAYER_SignatureLead_01` | signature playable lead |
| `FE_CHAR_PLAYER_HeavyGuard_01` | guard/tank playable role |
| `FE_CHAR_PLAYER_StampCaster_01` | caster/tool playable role |
| `FE_CHAR_PLAYER_RangerScout_01` | ranged/scout playable role |
| `FE_CHAR_PLAYER_FieldMedic_01` | support playable role |
| `FE_CHAR_NPC_MerchantTray_01` | shop/item service NPC |
| `FE_CHAR_NPC_UpgradeSmith_01` | upgrade/crafting NPC |
| `FE_CHAR_NPC_CartographerGuide_01` | quest/map guide NPC |
| `FE_ENEMY_R01_FoldBiter_01` | basic melee fodder |
| `FE_ENEMY_R01_ShieldClamp_01` | shield/blocker enemy |
| `FE_ENEMY_R01_LineSpitter_01` | stationary ranged pressure |
| `FE_ENEMY_R01_BlockCharger_01` | elite charge attacker |
| `FE_ENEMY_R01_GlyphSwarm_01` | small swarm pressure |
| `FE_BOSS_R01_StampMiniboss_01` | first miniboss test |
| `FE_PROP_COMMON_ToolBlade_01` | starter melee weapon |
| `FE_PROP_COMMON_BlockHammer_01` | starter heavy weapon |
| `FE_PROP_COMMON_GuardClamp_01` | guard offhand |
| `FE_PROP_COMMON_SealStamp_01` | caster focus |
| `FE_PROP_COMMON_RouteSpool_01` | scout/ranged tool |
| `FE_ENV_R01_BF_CombatArena_2x2_01` | small combat arena block |
| `FE_PROP_COMMON_CheckpointSocketPad_01` | save/respawn point |
| `FE_PROP_COMMON_RewardReceiverPad_01` | reward/goal landing |

## Review Notes

- This is a playable-loop asset pass, not final art.
- The new enemy and equipment silhouettes are more immediately useful than the
  current lead character shape.
- The lead/NPC bodies need a second pass for hands, feet, face simplification,
  and tool density.
- Avoid drifting back into brown/stone/gold as the default palette. The sprint
  deliberately adds coral, cyan, plum, mint, magenta, lime, and blue accents.
- `RelicChest` remains usable only as a temporary reward prop. Prefer
  `RewardReceiverPad` for the original reward language.

## Verification

- `python3 tools/AssetPipeline/generate_production_model_pack.py`
- `node tools/AssetPipeline/build_prompt_contract.mjs`
- `node tools/AssetPipeline/validate_generated_assets.mjs --phase=preimport`

