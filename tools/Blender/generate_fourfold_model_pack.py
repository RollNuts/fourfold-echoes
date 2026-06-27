#!/usr/bin/env python3
"""Generate the FOURFOLD ECHOES commercial-safe model pack.

The output is intentionally repo-authored procedural geometry: no third-party
meshes, no franchise likenesses, and no external texture sources.
"""

from __future__ import annotations

import json
import math
import os
from dataclasses import dataclass
from datetime import date
from pathlib import Path

import bpy
from mathutils import Vector


REPO = Path(__file__).resolve().parents[2]
PRODUCTION_ROOT = REPO / "Assets" / "Art" / "Production"
SOURCE_DIR = PRODUCTION_ROOT / "Source"
PREVIEW_DIR = REPO / "artifacts" / "Previews" / "ProductionModelPack"
REPORT_DIR = REPO / "artifacts" / "Reports"
SOURCE_FILE = SOURCE_DIR / "FE_FourfoldModelPack.blend"
MANIFEST_FILE = REPORT_DIR / "fourfold-model-pack.json"

ART_DIRECTION_ID = "folded_reliquary"
ART_DIRECTION_NAME = "Folded Reliquary Miniatures"
STYLE_BIBLE_DOC = "docs/Art/FOLDED_RELIQUARY_STYLE_BIBLE.md"
BRAND_LINE_ID = "folded_reliquary_miniatures"
BRAND_LINE_NAME = "Folded Reliquary Miniatures / 折り目遺物の箱庭模型"
GENRE_STATEMENT = (
    "Commercial-safe friendly compact action-adventure built from rounded low "
    "folded plates, four-part inlays, chunky broken tabs, saturated readable "
    "color planes, and one functional signal-thread language."
)
ART_DIRECTION_SUMMARY = (
    "Top-down/high-three-quarter miniature relics built from rounded folded plates, "
    "four-part readable footprints, saturated stylized color planes, and one "
    "recurring signal-thread glow language."
)
PRODUCT_LINE_CONTRACT = {
    "brand_line_id": BRAND_LINE_ID,
    "brand_line_name": BRAND_LINE_NAME,
    "style_bible": STYLE_BIBLE_DOC,
    "genre_statement": GENRE_STATEMENT,
    "non_negotiable_rules": [
        "Every model belongs to one folded-reliquary product line; regional nouns are material/weathering variants only.",
        "Primary shapes start from low folded slabs, plinths, broken chunks, or sockets; spheres, rods, and cylinders cannot carry the design alone.",
        "A fourfold read must be present through quartered footprints, split inlays, offset 45-degree cuts, or broken folded edges.",
        "Signal colors are functional only: tool, route, reward, lock, danger, or boss read.",
        "Regional difference comes from material ratio, wear state, tab sharpness, and signal color; it must not become a separate fantasy genre.",
        "Density comes from chunky tabs and clustered focal detail, not scattered thin props or noise.",
        "Top-down readability decides completion: front, grounding, function, and danger/reward must read in a still gameplay shot.",
        "Friendly stylized readability decides proportion: compact bodies, readable hands/tools, broad color planes, and warm-key/cool-shadow lighting.",
        "External games are aggregate market benchmarks only, never shape, palette, composition, or motif sources.",
    ],
    "market_metric_targets": {
        "contrast_proximity_min": 0.60,
        "edge_density_proximity_min": 0.60,
        "saturation_proximity_min": 0.55,
    },
}
ART_DIRECTION_DNA = {
    "shape_rules": [
        "Use chunky low-height primitives with bevels; avoid tall walls that hide gameplay reads.",
        "Give major objects a folded slab, split-ring, or quartered footprint motif.",
        "Keep silhouettes front-readable from the gameplay camera before adding detail.",
        "Use a single signal-thread line or socket to mark tool, route, reward, or danger meaning.",
        "Cluster fine details around focal objects instead of scattering them evenly.",
    ],
    "color_material_rules": [
        "One dark grounding material, one regional body material, one accent, and one signal color per family.",
        "Signal colors explain gameplay; they are not decoration-only glow.",
        "Regional kits vary material ratios, not the underlying shape grammar.",
        "Small detail props must reinforce fold seams, quarter marks, or signal threads.",
        "External screenshots are benchmark metrics only, never style prompts or model references.",
    ],
    "forbidden_derivation": [
        "Do not copy character, monster, prop, composition, mascot, logo, or franchise-specific silhouettes.",
        "Do not average multiple named game styles into a synthetic lookalike.",
        "Do not put protected title, studio, creator, or style labels into model specs, prompts, or manifests.",
    ],
}
ART_DIRECTION_CONTRACT = {
    "contract_id": ART_DIRECTION_ID,
    "canonical_doc": STYLE_BIBLE_DOC,
    "supporting_doc": "docs/Art/COMPACT_ACTION_ART_DIRECTION.md",
    "genre": "compact_top_down_action_adventure",
    "shape_grammar": "folded_reliquary_miniature",
    "camera_profile": "top_down_orthographic_gameplay",
    "brand_line_id": BRAND_LINE_ID,
    "genre_statement": GENRE_STATEMENT,
    "pillars": [
        "readable_top_down_silhouette",
        "folded_plinth_and_split_inlay",
        "single_signal_thread_language",
        "regional_material_variation_without_product_drift",
        "commercial_safe_non_derivative_original_assets",
    ],
}
STYLE_FAMILIES = {
    "common": {
        "palette_roles": {"base": "FE_MAT_stone_dark", "dark": "FE_MAT_enemy_ink", "accent": "FE_MAT_gold", "signal": "FE_MAT_danger_red"},
        "shape_tokens": ["folded_plinth", "split_inlay", "signal_thread", "chunk_tab"],
        "forbidden_shape_tokens": ["franchise_mascot", "thin_noise", "photoreal_surface", "unreadable_tall_wall"],
        "lighting_intent": "neutral gameplay read",
        "max_occluding_height_m": 1.6,
    },
    "hub": {
        "palette_roles": {"base": "FE_MAT_hub_ivory", "dark": "FE_MAT_stone_dark", "accent": "FE_MAT_gold", "signal": "FE_MAT_tool_blue"},
        "shape_tokens": ["clean_folded_plinth", "quartered_floor_mark", "low_safe_barrier", "signal_thread"],
        "forbidden_shape_tokens": ["busy_market_clutter", "text_dependent_signage", "high_wall"],
        "lighting_intent": "safe warm orientation",
        "max_occluding_height_m": 1.5,
    },
    "r01": {
        "palette_roles": {"base": "FE_MAT_stone_warm", "dark": "FE_MAT_stone_dark", "accent": "FE_MAT_moss", "signal": "FE_MAT_route_teal"},
        "shape_tokens": ["weathered_folded_plinth", "rounded_chip", "low_growth_tab", "signal_thread"],
        "forbidden_shape_tokens": ["naturalistic_forest_noise", "thin_vines", "character_likeness"],
        "lighting_intent": "bright first-region readability",
        "max_occluding_height_m": 1.7,
    },
    "r02": {
        "palette_roles": {"base": "FE_MAT_r02_charcoal", "dark": "FE_MAT_stone_dark", "accent": "FE_MAT_r02_rust", "signal": "FE_MAT_r02_amber"},
        "shape_tokens": ["scorched_folded_plinth", "bent_chunk_tab", "amber_signal_thread", "low_mechanical_inlay"],
        "forbidden_shape_tokens": ["literal_factory_set", "tiny_gears_as_noise", "photoreal_metal"],
        "lighting_intent": "hard warm danger contrast",
        "max_occluding_height_m": 1.7,
    },
    "r03": {
        "palette_roles": {"base": "FE_MAT_r03_dark", "dark": "FE_MAT_stone_dark", "accent": "FE_MAT_r03_violet", "signal": "FE_MAT_r03_crystal"},
        "shape_tokens": ["cold_folded_plinth", "sharp_chunk_tab", "violet_split_inlay", "signal_thread"],
        "forbidden_shape_tokens": ["glitter_noise", "literal_crystal_forest", "unreadable_thin_spikes"],
        "lighting_intent": "cool late-region contrast",
        "max_occluding_height_m": 1.7,
    },
    "boss": {
        "palette_roles": {"base": "FE_MAT_stone_dark", "dark": "FE_MAT_enemy_ink", "accent": "FE_MAT_gold", "signal": "FE_MAT_danger_red"},
        "shape_tokens": ["large_folded_plinth", "quartered_arena_read", "front_weak_socket", "danger_signal_thread"],
        "forbidden_shape_tokens": ["unreadable_overdetail", "trademarked_boss_silhouette", "full_screen_vfx_noise"],
        "lighting_intent": "controlled danger read",
        "max_occluding_height_m": 3.2,
    },
}

PRODUCT_LINE_ROLES = {
    "Hero": {
        "role": "Toolbearer Relic",
        "required_shape_tokens": ["folded_body", "front_read", "tool_socket", "ground_plinth", "functional_signal_thread"],
        "forbidden_drift_tokens": ["generic_rpg_hero", "mmo_armor_density", "face_or_hair_as_identity", "franchise_weapon_read"],
    },
    "ServiceNPC": {
        "role": "Service Cast",
        "required_shape_tokens": ["service_tool_read", "compact_body", "front_read", "portable_work_shape"],
        "forbidden_drift_tokens": ["text_signage_as_identity", "generic_villager", "shop_counter_as_character"],
    },
    "Equipment": {
        "role": "Block-Field Equipment",
        "required_shape_tokens": ["tool_weapon_hybrid", "oversized_head", "grip_read", "functional_signal_thread"],
        "forbidden_drift_tokens": ["generic_fantasy_weapon", "trademarked_weapon_silhouette", "thin_realistic_blade"],
    },
    "ExplorationInstrument": {
        "role": "Exploration Instrument",
        "required_shape_tokens": ["quartered_reading_disc", "folded_hinge", "broken_lens_frame", "handle_read", "functional_signal_thread"],
        "forbidden_drift_tokens": ["magic_staff", "antenna_prop", "single_orb_core", "second_tool_language"],
    },
    "Combatant": {
        "role": "Broken Hostile Relic",
        "required_shape_tokens": ["broken_folded_body", "front_attack_plane", "danger_crack", "low_grounding_tabs", "functional_signal_thread"],
        "forbidden_drift_tokens": ["red_eye_creature", "round_body_mascot", "handheld_club", "plant_monster"],
    },
    "Boss": {
        "role": "Warden Relic",
        "required_shape_tokens": ["large_folded_plinth", "four_threat_anchors", "weak_socket", "boss_danger_surface", "functional_signal_thread"],
        "forbidden_drift_tokens": ["trademarked_boss_outline", "screen_hiding_ornament", "literal_crown_icon", "vfx_noise_as_shape"],
    },
    "ToolReceiver": {
        "role": "Tool Receiver",
        "required_shape_tokens": ["receiver_socket", "idle_active_solved_read", "folded_plinth", "input_slit", "functional_signal_thread"],
        "forbidden_drift_tokens": ["text_signage", "ui_symbol_dependency", "second_tool_read", "generic_machine_terminal"],
    },
    "RouteSurface": {
        "role": "Route Surface",
        "required_shape_tokens": ["low_folded_plate", "folded_shell_leaf", "connection_socket", "hinge_spine", "recessed_signal_groove", "dark_underside_mass", "top_down_navigation_read"],
        "forbidden_drift_tokens": ["uniform_tile_grid", "rectangular_slab_bridge", "gray_blockout_floor", "decorative_glow_only", "tall_occluding_surface"],
    },
    "LowBoundary": {
        "role": "Low Boundary",
        "required_shape_tokens": ["low_folded_wall", "readable_blocked_state", "chunk_tab", "camera_safe_height"],
        "forbidden_drift_tokens": ["thin_fence", "photoreal_cliff", "tall_pillar_row", "natural_wall_as_primary_shape"],
    },
    "RewardReliquary": {
        "role": "Reward Reliquary",
        "required_shape_tokens": ["small_folded_shrine", "pickup_disc", "reward_socket", "functional_signal_thread"],
        "forbidden_drift_tokens": ["generic_treasure_chest", "literal_jewel_pickup", "franchise_relic_shape", "inventory_icon_as_model"],
    },
    "GroundingDetail": {
        "role": "Grounding Detail",
        "required_shape_tokens": ["low_inlay", "chunk_chip", "clustered_focal_placement", "supports_fourfold_read"],
        "forbidden_drift_tokens": ["sprinkled_noise", "thin_grass_wire", "standalone_flower_patch", "detail_as_new_genre"],
    },
    "SetDressing": {
        "role": "Reliquary Set Dressing",
        "required_shape_tokens": ["folded_plinth", "chunk_tab", "split_inlay", "top_down_silhouette"],
        "forbidden_drift_tokens": ["village_furniture", "literal_factory_prop", "crystal_forest_prop", "random_fantasy_clutter"],
    },
}

REGIONAL_VARIANT_POLICY = {
    "common": "Common assets define the folded-reliquary language and must not borrow a separate fantasy archetype.",
    "hub": "Hub is the polished reliquary variant: ivory/warm gold/soft blue with clean low folded edges, not village furniture.",
    "r01": "R01 is the weathered reliquary variant: moss and growth are secondary wear tabs, not a forest-prop genre.",
    "r02": "R02 is the scorched reliquary variant: rust/amber are heat damage and inlay colors, not a literal factory set.",
    "r03": "R03 is the cold reliquary variant: violet/crystal are fracture/inlay materials, not a crystal forest.",
    "boss": "Boss assets are broken crown reliquaries: fourfold danger and weak sockets, not literal crown iconography.",
}


@dataclass(frozen=True)
class AssetSpec:
    asset_id: str
    name: str
    category: str
    gameplay_role: str
    used_in_scene: str
    priority: str
    builder: str
    style: str
    scale_meters: str
    triangle_budget_lod0: int
    material_budget: int
    acceptance: str


ASSETS: list[AssetSpec] = [
    AssetSpec("art.character.base_chibi_mannequin_01", "FE_CHAR_TEMPLATE_ChibiMannequin_01", "Character", "shared playable/NPC proportion mannequin", "art foundation", "P0", "chibi_mannequin", "common", "1.45", 9000, 5, "Shared 3-head pop-deformed mannequin for playable and friendly NPC bodies only; enemies use separate creature/monster skeleton families."),
    AssetSpec("art.hero.player_v1", "FE_CHAR_PLAYER_Hero_01", "Character", "player avatar", "vertical slice", "P0", "hero", "common", "1.6", 9000, 8, "Hero matches the pop-deformed concept lead: brown hair, blue/orange jacket, backpack, hand tool, and cyan cube signal read from top-down."),
    AssetSpec("art.hero.signature_lead_01", "FE_CHAR_PLAYER_SignatureLead_01", "Character", "signature playable lead", "P0_ModelSprint", "P0", "pop_hero_lead", "common", "1.55", 9000, 5, "Pop-deformed lead reads as a block-field toolbearer, not a generic sword hero."),
    AssetSpec("art.hero.heavy_guard_01", "FE_CHAR_PLAYER_HeavyGuard_01", "Character", "playable guard class", "P0_ModelSprint", "P0", "pop_playable_heavy", "common", "1.45", 9000, 5, "Wide guard silhouette, clamp shield, and short body read instantly from gameplay camera."),
    AssetSpec("art.hero.stamp_caster_01", "FE_CHAR_PLAYER_StampCaster_01", "Character", "playable caster class", "P0_ModelSprint", "P0", "pop_playable_caster", "common", "1.55", 9000, 5, "Caster role is carried by stamp/seal tool language rather than generic robe-and-staff fantasy."),
    AssetSpec("art.hero.ranger_scout_01", "FE_CHAR_PLAYER_RangerScout_01", "Character", "playable scout class", "P0_ModelSprint", "P0", "pop_playable_ranger", "common", "1.50", 8500, 5, "Scout silhouette emphasizes route marking, compact bow, and block-field traversal gear."),
    AssetSpec("art.hero.field_medic_01", "FE_CHAR_PLAYER_FieldMedic_01", "Character", "playable support class", "P0_ModelSprint", "P0", "pop_playable_medic", "common", "1.45", 8500, 5, "Support role reads through repair satchel, signal patch, and rounded non-combat posture."),
    AssetSpec("art.tool.exploration_v1", "FE_PROP_COMMON_ExplorationTool_01", "Prop", "central exploration verb", "vertical slice", "P0", "exploration_tool", "common", "0.8", 2000, 4, "Tool silhouette and glow read without text."),
    AssetSpec("art.npc.merchant_tray_01", "FE_CHAR_NPC_MerchantTray_01", "NPC", "shop and item service", "P0_ModelSprint", "P0", "pop_npc_merchant", "hub", "1.35", 7500, 5, "Merchant is identified by tray/backpack service shape, not text signage."),
    AssetSpec("art.npc.upgrade_smith_01", "FE_CHAR_NPC_UpgradeSmith_01", "NPC", "upgrade and crafting service", "P0_ModelSprint", "P0", "pop_npc_smith", "hub", "1.45", 8000, 5, "Upgrade NPC reads through clamp hammer, measuring jaw, and sturdy work silhouette."),
    AssetSpec("art.npc.cartographer_guide_01", "FE_CHAR_NPC_CartographerGuide_01", "NPC", "quest and map service", "P0_ModelSprint", "P0", "pop_npc_cartographer", "hub", "1.40", 7500, 5, "Cartographer reads through fold-map rig, route pins, and compact guide posture."),
    AssetSpec("art.enemy.template_esk01_small_biped_01", "FE_ENEMY_TEMPLATE_ESK01_SmallBiped_01", "Enemy", "neutral small biped enemy skeleton template", "art foundation", "P0", "enemy_template_small_biped", "common", "1.05", 7000, 5, "Neutral ESK-01 small biped enemy mannequin with clear front, attack origin, weak core, head socket, hand sockets, and walk-biped animation readiness; not a finished monster."),
    AssetSpec("art.enemy.template_esk03_quadruped_01", "FE_ENEMY_TEMPLATE_ESK03_Quadruped_01", "Enemy", "neutral quadruped beast charger skeleton template", "art foundation", "P0", "enemy_template_quadruped", "common", "1.25", 9000, 5, "Neutral ESK-03 quadruped charger enemy mannequin with four-foot grounding, head socket, tail socket, charge origin, side/back weak socket, and run-charge animation readiness; not a finished monster."),
    AssetSpec("art.enemy.template_esk05_floating_caster_01", "FE_ENEMY_TEMPLATE_ESK05_FloatingCaster_01", "Enemy", "neutral floating caster support skeleton template", "art foundation", "P0", "enemy_template_floating_caster", "common", "1.10", 7000, 5, "Neutral ESK-05 floating caster enemy mannequin with hover body, front cast core, projectile origin, weak core, side part sockets, and hover/cast animation readiness; not a finished ghost, drone, or wizard."),
    AssetSpec("art.enemy.melee_01", "FE_ENEMY_MELEE_Shardling", "Enemy", "normal melee combat", "vertical slice", "P0", "melee_enemy", "r01", "1.2", 6000, 4, "Enemy front and melee attack origin are obvious."),
    AssetSpec("art.enemy.ranged_01", "FE_ENEMY_RANGED_BloomSpitter", "Enemy", "ranged pressure", "vertical slice", "P0", "ranged_enemy", "r01", "1.2", 6000, 4, "Projectile origin and hostile facing are obvious."),
    AssetSpec("art.enemy.miniboss_01", "FE_ENEMY_MINIBOSS_RootBruiser", "Enemy", "combat escalation", "vertical slice", "P0", "miniboss", "r01", "2.3", 18000, 3, "Larger tell surfaces read from top-down without boss-level clutter."),
    AssetSpec("art.enemy.r01_fodder_01", "FE_ENEMY_R01_FoldBiter_01", "Enemy", "basic melee fodder", "P0_ModelSprint", "P0", "pop_enemy_fodder", "r01", "1.05", 6000, 4, "Small enemy has readable bite/front and can be deployed in groups."),
    AssetSpec("art.enemy.r01_shield_01", "FE_ENEMY_R01_ShieldClamp_01", "Enemy", "shielded blocker", "P0_ModelSprint", "P0", "pop_enemy_shield", "r01", "1.15", 7000, 4, "Shield enemy shows front armor and flank weakness without UI help."),
    AssetSpec("art.enemy.r01_turret_01", "FE_ENEMY_R01_LineSpitter_01", "Enemy", "stationary ranged pressure", "P0_ModelSprint", "P0", "pop_enemy_turret", "r01", "1.10", 7000, 4, "Ranged origin, aim line, and immobile base read from high camera."),
    AssetSpec("art.enemy.r01_charger_01", "FE_ENEMY_R01_BlockCharger_01", "Enemy", "elite charge attacker", "P0_ModelSprint", "P0", "pop_enemy_charger", "r01", "1.35", 9000, 4, "Charger communicates forward mass, windup horn, and side dodge window."),
    AssetSpec("art.enemy.r01_swarm_01", "FE_ENEMY_R01_GlyphSwarm_01", "Enemy", "small swarm pressure", "P0_ModelSprint", "P0", "pop_enemy_swarm", "r01", "0.75", 4500, 4, "Swarm unit remains distinct at small scale through glyph body and bright danger tip."),
    AssetSpec("art.boss.r01_stamp_miniboss_01", "FE_BOSS_R01_StampMiniboss_01", "Boss", "first miniboss combat test", "P0_ModelSprint", "P0", "pop_miniboss_stamp", "r01", "2.4", 18000, 5, "First miniboss has clear stamp slam, weak socket, and block-field footprint."),
    AssetSpec("art.boss.01", "FE_BOSS_01_RootWarden", "Boss", "first region boss", "vertical slice", "P0", "boss_root", "r01", "3.8", 35000, 4, "Weak/read points and root-arm attack origins are visible."),
    AssetSpec("art.equipment.tool_blade_01", "FE_PROP_COMMON_ToolBlade_01", "Equipment", "starter melee weapon", "P0_ModelSprint", "P0", "pop_weapon_blade", "common", "0.9", 3000, 4, "Starter blade reads as a block-field tool/weapon hybrid, not a generic fantasy sword."),
    AssetSpec("art.equipment.block_hammer_01", "FE_PROP_COMMON_BlockHammer_01", "Equipment", "starter heavy weapon", "P0_ModelSprint", "P0", "pop_weapon_hammer", "common", "1.0", 3500, 4, "Hammer reads as a block-lifting and combat tool with exaggerated safe silhouette."),
    AssetSpec("art.equipment.guard_clamp_01", "FE_PROP_COMMON_GuardClamp_01", "Equipment", "starter guard offhand", "P0_ModelSprint", "P0", "pop_weapon_guard_clamp", "common", "0.8", 3000, 4, "Guard clamp reads as a shield and repair clamp, not a generic kite shield."),
    AssetSpec("art.equipment.seal_stamp_01", "FE_PROP_COMMON_SealStamp_01", "Equipment", "starter caster focus", "P0_ModelSprint", "P0", "pop_weapon_seal_stamp", "common", "0.8", 3000, 4, "Seal stamp gives caster a unique block-field interaction silhouette."),
    AssetSpec("art.equipment.route_spool_01", "FE_PROP_COMMON_RouteSpool_01", "Equipment", "starter ranged tool", "P0_ModelSprint", "P0", "pop_weapon_route_spool", "common", "0.8", 3000, 4, "Route spool reads as ranged/scout equipment without becoming a bow copy."),
    AssetSpec("art.blockfield.combat_arena_2x2", "FE_ENV_R01_BF_CombatArena_2x2_01", "Environment", "small combat arena grammar", "P0_ModelSprint", "P0", "block_combat_arena_2x2", "r01", "4x4m module", 2000, 5, "2x2 arena tile supports first combat layout and spawn readability."),
    AssetSpec("art.prop.checkpoint_socket_pad_01", "FE_PROP_COMMON_CheckpointSocketPad_01", "Prop", "save and respawn point", "P0_ModelSprint", "P0", "checkpoint_socket_pad", "common", "1.2", 2500, 4, "Checkpoint pad reads as save/return point without menu text."),
    AssetSpec("art.prop.reward_receiver_pad_01", "FE_PROP_COMMON_RewardReceiverPad_01", "Prop", "quest and boss reward landing", "P0_ModelSprint", "P0", "reward_receiver_pad", "common", "1.2", 2500, 4, "Reward receiver reads as goal/reward destination distinct from treasure chest."),
    AssetSpec("art.boss.02", "FE_BOSS_02_FurnaceWarden", "Boss", "second region boss", "MVP", "P2", "boss_furnace", "r02", "4.0", 35000, 4, "Amber core and heavy side attacks read at gameplay distance."),
    AssetSpec("art.boss.03", "FE_BOSS_03_GlassWarden", "Boss", "third region boss", "MVP", "P2", "boss_glass", "r03", "4.0", 35000, 4, "Crystal silhouette, danger shards, and front read are distinct."),
    AssetSpec("art.boss.04", "FE_BOSS_04_CrownWarden", "Boss", "final boss", "MVP", "P2", "boss_crown", "boss", "4.4", 40000, 4, "Final crown-ring silhouette shows four clear threat anchors."),
    AssetSpec("art.prop.gimmick_pedestal", "FE_PROP_R01_GimmickPedestal_01", "Prop", "tool reaction target", "vertical slice", "P0", "pedestal", "r01", "1.0", 2500, 4, "Idle, active, and solved anchors are present in one model."),
    AssetSpec("art.env.shortcut_bridge", "FE_ENV_R01_ShortcutBridge_01", "Environment", "opened shortcut route", "vertical slice", "P0", "shortcut_bridge", "r01", "module", 3000, 4, "Opened folded-shell bridge reads as a tool consequence, not a rectangular tile bridge."),
    AssetSpec("art.blockfield.grass_floor_1x1_flat", "FE_ENV_R01_BF_GrassFloor_1x1_Flat", "Environment", "block field traversal grammar", "BlockField_VerticalSlice", "P0", "block_grass_floor_1x1", "r01", "2x2m module", 900, 4, "Flat 1x1 grass block snaps on a 2m grid and reads as walkable floor."),
    AssetSpec("art.blockfield.grass_floor_1x2_straight", "FE_ENV_R01_BF_GrassFloor_1x2_Straight", "Environment", "block field traversal grammar", "BlockField_VerticalSlice", "P0", "block_grass_floor_1x2", "r01", "2x4m module", 1200, 4, "Straight 1x2 grass path snaps cleanly and reads as traversable route."),
    AssetSpec("art.blockfield.grass_floor_2x2_plaza", "FE_ENV_R01_BF_GrassFloor_2x2_Plaza", "Environment", "block field traversal grammar", "BlockField_VerticalSlice", "P0", "block_grass_floor_2x2", "r01", "4x4m module", 1600, 4, "2x2 grass plaza block creates a readable play space without hero-prop clutter."),
    AssetSpec("art.blockfield.grass_edge_straight", "FE_ENV_R01_BF_GrassEdge_Straight", "Environment", "block field edge grammar", "BlockField_VerticalSlice", "P0", "block_grass_edge_straight", "r01", "2x2m module", 1200, 3, "Straight grass edge shows collision/drop boundary without pixel-cube language."),
    AssetSpec("art.blockfield.grass_edge_outer_corner", "FE_ENV_R01_BF_GrassEdge_OuterCorner", "Environment", "block field edge grammar", "BlockField_VerticalSlice", "P0", "block_grass_edge_outer", "r01", "2x2m module", 1400, 3, "Outer corner grass edge snaps and preserves clear collision corners."),
    AssetSpec("art.blockfield.grass_edge_inner_corner", "FE_ENV_R01_BF_GrassEdge_InnerCorner", "Environment", "block field edge grammar", "BlockField_VerticalSlice", "P0", "block_grass_edge_inner", "r01", "2x2m module", 1400, 4, "Inner corner grass edge makes concave routes readable from gameplay camera."),
    AssetSpec("art.blockfield.grass_height_cliff_up1", "FE_ENV_R01_BF_GrassHeight_Cliff_Up1", "Environment", "block field height grammar", "BlockField_VerticalSlice", "P0", "block_grass_cliff_up1", "r01", "2x2m module", 1600, 3, "One-level grass cliff reads as a height step, not a decorative island."),
    AssetSpec("art.blockfield.stone_floor_1x1_clean", "FE_ENV_HUB_BF_StoneFloor_1x1_Clean", "Environment", "block field traversal grammar", "BlockField_VerticalSlice", "P0", "block_stone_floor_1x1", "hub", "2x2m module", 900, 3, "Clean 1x1 stone block establishes town/dungeon grid grammar."),
    AssetSpec("art.blockfield.stone_floor_2x2_clean", "FE_ENV_HUB_BF_StoneFloor_2x2_Clean", "Environment", "block field traversal grammar", "BlockField_VerticalSlice", "P0", "block_stone_floor_2x2", "hub", "4x4m module", 1600, 3, "Clean 2x2 stone block gives a playable plaza/room base."),
    AssetSpec("art.blockfield.stone_floor_1x1_cracked", "FE_ENV_HUB_BF_StoneFloor_1x1_Cracked", "Environment", "block field traversal grammar", "BlockField_VerticalSlice", "P0", "block_stone_floor_cracked", "hub", "2x2m module", 1200, 3, "Cracked stone floor adds variation while preserving collision clarity."),
    AssetSpec("art.blockfield.stone_wall_straight_1x1", "FE_ENV_HUB_BF_StoneWall_Straight_1x1", "Environment", "block field boundary grammar", "BlockField_VerticalSlice", "P0", "block_stone_wall_straight", "hub", "2x2m module", 1400, 3, "Straight wall blocks movement without hiding player silhouettes."),
    AssetSpec("art.blockfield.stone_wall_outer_corner", "FE_ENV_HUB_BF_StoneWall_OuterCorner", "Environment", "block field boundary grammar", "BlockField_VerticalSlice", "P0", "block_stone_wall_outer", "hub", "2x2m module", 1500, 3, "Outer wall corner locks collision grammar at turns."),
    AssetSpec("art.blockfield.stone_wall_inner_corner", "FE_ENV_HUB_BF_StoneWall_InnerCorner", "Environment", "block field boundary grammar", "BlockField_VerticalSlice", "P0", "block_stone_wall_inner", "hub", "2x2m module", 1500, 3, "Inner wall corner supports room and corridor composition."),
    AssetSpec("art.blockfield.stone_arch_doorway", "FE_ENV_HUB_BF_StoneArch_Doorway", "Environment", "block field doorway grammar", "BlockField_VerticalSlice", "P0", "block_stone_arch_doorway", "hub", "2x2m module", 2200, 4, "Arch doorway reads as an entrance without text or franchise-like trim."),
    AssetSpec("art.blockfield.stair_straight_up1", "FE_ENV_HUB_BF_Stair_Straight_Up1", "Environment", "block field elevation grammar", "BlockField_VerticalSlice", "P0", "block_stair_straight_up1", "hub", "2x2m module", 1600, 3, "Straight stair communicates one-level elevation change on the grid."),
    AssetSpec("art.blockfield.stair_corner_landing_up1", "FE_ENV_HUB_BF_Stair_CornerLanding_Up1", "Environment", "block field elevation grammar", "BlockField_VerticalSlice", "P0", "block_stair_corner_landing", "hub", "2x2m module", 1800, 3, "Corner stair landing connects orthogonal routes without a bespoke mesh."),
    AssetSpec("art.blockfield.water_channel_edge_straight", "FE_ENV_R01_BF_WaterChannelEdge_Straight", "Environment", "block field water edge grammar", "BlockField_VerticalSlice", "P0", "block_water_edge_straight", "r01", "2x2m module", 1400, 5, "Water edge shows walkable/non-walkable split at a glance."),
    AssetSpec("art.blockfield.bridge_wood_1x2", "FE_ENV_R01_BF_BridgeWood_1x2", "Environment", "block field crossing grammar", "BlockField_VerticalSlice", "P0", "block_bridge_wood_1x2", "r01", "2x4m module", 1600, 3, "Wood bridge connects two modules and reads as traversable from high camera."),
    AssetSpec("art.blockfield.fence_railing_straight", "FE_ENV_R01_BF_FenceRailing_Straight", "Environment", "block field soft boundary grammar", "BlockField_VerticalSlice", "P0", "block_fence_railing_straight", "r01", "2x2m module", 1200, 4, "Railing gives a low readable edge without becoming a thin fence copy."),
    AssetSpec("art.blockfield.hazard_floor_spike", "FE_ENV_HUB_BF_HazardFloor_Spike", "Environment", "block field hazard grammar", "BlockField_VerticalSlice", "P0", "block_hazard_spike_floor", "hub", "2x2m module", 1600, 5, "Spike hazard uses the same footprint as floor tiles and reads without VFX."),
    AssetSpec("art.prop.root_gate", "FE_PROP_R01_RootGate_01", "Prop", "blocked shortcut state", "vertical slice", "P0", "root_gate", "r01", "module", 3000, 2, "Muted blocker silhouette contrasts with opened shortcut."),
    AssetSpec("art.prop.chest_01", "FE_PROP_COMMON_RelicChest_01", "Prop", "reward delivery", "vertical slice", "P0", "chest", "common", "1.0", 2500, 4, "Closed/open lid and reward glow are readable."),
    AssetSpec("art.reward.relic_01", "FE_RELIC_EmberSeed_01", "Prop", "relic reward", "vertical slice", "P0", "relic_seed", "common", "0.4", 1000, 1, "Distinct warm seed silhouette."),
    AssetSpec("art.reward.relic_02", "FE_RELIC_RootSigil_01", "Prop", "relic reward", "vertical slice", "P0", "relic_sigil", "common", "0.4", 1000, 1, "Distinct ring/sigil silhouette."),
    AssetSpec("art.env.hub_floor_stone", "FE_ENV_HUB_FloorStone_01", "Environment", "hub floor module", "Hub_Crossroads", "P1", "floor_tile", "hub", "module", 3000, 2, "Warm safe hub floor tile."),
    AssetSpec("art.env.hub_floor_mosaic", "FE_ENV_HUB_FloorMosaic_01", "Environment", "hub landmark floor", "Hub_Crossroads", "P1", "mosaic_tile", "hub", "module", 3000, 2, "Circular hub orientation mark."),
    AssetSpec("art.prop.hub_low_wall", "FE_PROP_HUB_LowWall_01", "Prop", "hub boundary", "Hub_Crossroads", "P1", "low_wall", "hub", "module", 3000, 2, "Low wall does not hide combat silhouettes."),
    AssetSpec("art.prop.hub_return_point", "FE_PROP_HUB_ReturnPoint_01", "Prop", "hub return point", "Hub_Crossroads", "P1", "return_point", "hub", "1.3", 3000, 2, "Warm blue return read without UI label."),
    AssetSpec("art.prop.hub_region_gate", "FE_PROP_HUB_RegionGate_01", "Prop", "region entrance", "Hub_Crossroads", "P1", "region_gate", "hub", "2.4", 4000, 2, "Gate reads as an entrance, not a shop or quest prop."),
    AssetSpec("art.prop.hub_light_anchor", "FE_PROP_HUB_LightAnchor_01", "Prop", "hub lighting anchor", "Hub_Crossroads", "P1", "light_anchor", "hub", "1.6", 2500, 2, "Safe landmark glow."),
    AssetSpec("art.prop.hub_save_stone", "FE_PROP_HUB_SaveStone_01", "Prop", "save landmark", "Hub_Crossroads", "P1", "save_stone", "hub", "1.2", 2500, 2, "Simple return/save landmark without menu implication."),
    AssetSpec("art.prop.hub_sign_obelisk", "FE_PROP_HUB_SignObelisk_01", "Prop", "orientation marker", "Hub_Crossroads", "P1", "obelisk", "hub", "1.4", 2500, 2, "Readable marker; no text dependency."),
    AssetSpec("art.prop.hub_stair_short", "FE_PROP_HUB_StairShort_01", "Prop", "hub elevation cue", "Hub_Crossroads", "P1", "short_stair", "hub", "module", 2500, 2, "Short readable stairs."),
    AssetSpec("art.prop.hub_bench_stone", "FE_PROP_HUB_BenchStone_01", "Prop", "hub set dressing", "Hub_Crossroads", "P1", "bench", "hub", "module", 1800, 2, "Readable low rest prop, not NPC/social system."),
    AssetSpec("art.prop.hub_planter", "FE_PROP_HUB_Planter_01", "Prop", "hub accent", "Hub_Crossroads", "P1", "planter", "hub", "module", 2500, 2, "Warm small accent that remains chunky."),
    AssetSpec("art.prop.hub_wayline", "FE_PROP_HUB_Wayline_01", "Prop", "hub route cue", "Hub_Crossroads", "P1", "wayline", "hub", "module", 1200, 1, "Ground cue for route composition."),
    AssetSpec("art.env.r01_floor_moss", "FE_ENV_R01_FloorMossStone_01", "Environment", "region floor module", "Region_01_GreenRuins", "P1", "floor_tile", "r01", "module", 3000, 2, "Moss and pale stone identity."),
    AssetSpec("art.env.r01_floor_broken", "FE_ENV_R01_FloorBroken_01", "Environment", "broken route floor", "Region_01_GreenRuins", "P1", "broken_floor", "r01", "module", 3000, 2, "Broken tile reads as route/edge, not clutter."),
    AssetSpec("art.prop.r01_low_wall_root", "FE_PROP_R01_LowWallRoot_01", "Prop", "region boundary", "Region_01_GreenRuins", "P1", "low_wall", "r01", "module", 3000, 2, "Root wall stays low for top-down clarity."),
    AssetSpec("art.prop.r01_route_edge", "FE_PROP_R01_RouteEdge_01", "Prop", "route edge", "Region_01_GreenRuins", "P1", "route_edge", "r01", "module", 1600, 1, "Chunky route edge marker."),
    AssetSpec("art.prop.r01_rubble", "FE_PROP_R01_Rubble_01", "Prop", "readable rubble", "Region_01_GreenRuins", "P1", "rubble", "r01", "module", 2000, 2, "Few large shapes, no noisy pebble field."),
    AssetSpec("art.prop.r01_flower_clump", "FE_PROP_R01_FlowerClump_01", "Prop", "yellow flower accent", "Region_01_GreenRuins", "P1", "flower_clump", "r01", "module", 2200, 2, "Large stylized flowers visible in thumbnail."),
    AssetSpec("art.prop.r01_root_arch", "FE_PROP_R01_RootArch_01", "Prop", "region landmark", "Region_01_GreenRuins", "P1", "root_arch", "r01", "2.4", 4000, 2, "Root arch reads without blocking the camera."),
    AssetSpec("art.prop.r01_tool_mirror", "FE_PROP_R01_ToolMirror_01", "Prop", "gimmick room object", "Region_01_GreenRuins", "P1", "tool_mirror", "r01", "1.2", 2500, 2, "One-tool reaction receiver, not a second tool."),
    AssetSpec("art.prop.r01_reveal_marker", "FE_PROP_R01_RevealMarker_01", "Prop", "gimmick room object", "Region_01_GreenRuins", "P1", "reveal_marker", "r01", "1.0", 1800, 2, "Hidden/revealed state anchor."),
    AssetSpec("art.prop.r01_sealed_blocker", "FE_PROP_R01_SealedBlocker_01", "Prop", "gimmick room blocker", "Region_01_GreenRuins", "P1", "sealed_blocker", "r01", "module", 2500, 2, "Clear obstacle without inventory lock language."),
    AssetSpec("art.prop.r01_vine_rail", "FE_PROP_R01_VineRail_01", "Prop", "route rail", "Region_01_GreenRuins", "P1", "vine_rail", "r01", "module", 1800, 2, "Low readable rail."),
    AssetSpec("art.prop.r01_moss_pillar", "FE_PROP_R01_MossPillar_01", "Prop", "landmark pillar", "Region_01_GreenRuins", "P1", "moss_pillar", "r01", "1.7", 3000, 2, "Chunky pillar does not hide actors."),
    AssetSpec("art.prop.r01_step_stone", "FE_PROP_R01_StepStone_01", "Prop", "route step", "Region_01_GreenRuins", "P1", "step_stone", "r01", "module", 1800, 1, "Clear step/route piece."),
    AssetSpec("art.prop.r01_short_grass", "FE_PROP_R01_ShortGrass_01", "Prop", "vegetation accent", "Region_01_GreenRuins", "P1", "short_grass", "r01", "module", 1600, 1, "Chunky low vegetation."),
    AssetSpec("art.prop.r01_signal_tile", "FE_PROP_R01_SignalTile_01", "Prop", "tool route cue", "Region_01_GreenRuins", "P1", "signal_tile", "r01", "module", 1600, 2, "Tool-color route tile."),
    AssetSpec("art.env.r02_floor_charcoal", "FE_ENV_R02_FloorCharcoalTile_01", "Environment", "region floor module", "Region_02_SunkenWorks", "P2", "floor_tile", "r02", "module", 3000, 2, "Charcoal/rust region identity."),
    AssetSpec("art.env.r02_floor_cracked", "FE_ENV_R02_FloorCracked_01", "Environment", "broken route floor", "Region_02_SunkenWorks", "P2", "broken_floor", "r02", "module", 3000, 2, "Harder region broken floor."),
    AssetSpec("art.prop.r02_angled_cliff", "FE_PROP_R02_AngledCliff_01", "Prop", "region boundary", "Region_02_SunkenWorks", "P2", "angled_cliff", "r02", "module", 3000, 2, "Angled cliff silhouette."),
    AssetSpec("art.prop.r02_metal_frame", "FE_PROP_R02_MetalFrame_01", "Prop", "region frame", "Region_02_SunkenWorks", "P2", "metal_frame", "r02", "module", 3000, 2, "Metal frame does not create visual noise."),
    AssetSpec("art.prop.r02_rust_pipe", "FE_PROP_R02_RustPipe_01", "Prop", "region accent", "Region_02_SunkenWorks", "P2", "rust_pipe", "r02", "module", 2200, 2, "Chunky pipe cue."),
    AssetSpec("art.prop.r02_amber_relay", "FE_PROP_R02_AmberRelay_01", "Prop", "tool-reaction object", "Region_02_SunkenWorks", "P2", "amber_relay", "r02", "1.2", 2500, 2, "Amber response anchor."),
    AssetSpec("art.prop.r02_low_wall_iron", "FE_PROP_R02_LowWallIron_01", "Prop", "region boundary", "Region_02_SunkenWorks", "P2", "low_wall", "r02", "module", 3000, 2, "Iron wall stays low."),
    AssetSpec("art.prop.r02_broken_gear", "FE_PROP_R02_BrokenGear_01", "Prop", "region accent", "Region_02_SunkenWorks", "P2", "broken_gear", "r02", "module", 2400, 2, "Large gear chunks only."),
    AssetSpec("art.prop.r02_heat_vent", "FE_PROP_R02_HeatVent_01", "Prop", "danger cue", "Region_02_SunkenWorks", "P2", "heat_vent", "r02", "module", 2200, 2, "Readable vent/hazard base."),
    AssetSpec("art.prop.r02_route_edge", "FE_PROP_R02_RouteEdge_01", "Prop", "route edge", "Region_02_SunkenWorks", "P2", "route_edge", "r02", "module", 1600, 1, "Warm region route edge."),
    AssetSpec("art.prop.r02_sunken_step", "FE_PROP_R02_SunkenStep_01", "Prop", "route step", "Region_02_SunkenWorks", "P2", "short_stair", "r02", "module", 2400, 2, "Sunken step module."),
    AssetSpec("art.prop.r02_tool_relay", "FE_PROP_R02_ToolRelay_01", "Prop", "tool reaction object", "Region_02_SunkenWorks", "P2", "tool_relay", "r02", "1.2", 2600, 2, "Same exploration tool language, region variant only."),
    AssetSpec("art.env.r03_floor_dark", "FE_ENV_R03_FloorDarkStone_01", "Environment", "region floor module", "Region_03_AshenKeep", "P2", "floor_tile", "r03", "module", 3000, 2, "Dark stone and cold glow region identity."),
    AssetSpec("art.env.r03_crystal_bridge", "FE_ENV_R03_CrystalBridge_01", "Environment", "narrow bridge", "Region_03_AshenKeep", "P2", "crystal_bridge", "r03", "module", 3000, 2, "Bridge silhouette stays navigable."),
    AssetSpec("art.prop.r03_crystal_cluster", "FE_PROP_R03_CrystalCluster_01", "Prop", "region landmark", "Region_03_AshenKeep", "P2", "crystal_cluster", "r03", "1.6", 3500, 2, "Large shards, not noisy glitter."),
    AssetSpec("art.prop.r03_low_wall_crystal", "FE_PROP_R03_LowWallCrystal_01", "Prop", "region boundary", "Region_03_AshenKeep", "P2", "low_wall", "r03", "module", 3000, 2, "Low crystal wall."),
    AssetSpec("art.prop.r03_narrow_bridge", "FE_PROP_R03_NarrowBridge_01", "Prop", "route bridge", "Region_03_AshenKeep", "P2", "narrow_bridge", "r03", "module", 3000, 2, "Narrow but readable route."),
    AssetSpec("art.prop.r03_cold_white_lamp", "FE_PROP_R03_ColdWhiteLamp_01", "Prop", "lighting anchor", "Region_03_AshenKeep", "P2", "light_anchor", "r03", "1.5", 2500, 2, "Cold safe read."),
    AssetSpec("art.prop.r03_smooth_obelisk", "FE_PROP_R03_SmoothObelisk_01", "Prop", "region marker", "Region_03_AshenKeep", "P2", "obelisk", "r03", "1.6", 2500, 2, "Smooth late-region marker."),
    AssetSpec("art.prop.r03_violet_marker", "FE_PROP_R03_VioletMarker_01", "Prop", "route marker", "Region_03_AshenKeep", "P2", "reveal_marker", "r03", "1.0", 1800, 2, "Violet response marker."),
    AssetSpec("art.prop.r03_route_edge", "FE_PROP_R03_RouteEdge_01", "Prop", "route edge", "Region_03_AshenKeep", "P2", "route_edge", "r03", "module", 1600, 1, "Cold region route edge."),
    AssetSpec("art.prop.r03_tool_lens", "FE_PROP_R03_ToolLens_01", "Prop", "tool reaction object", "Region_03_AshenKeep", "P2", "tool_mirror", "r03", "1.2", 2500, 2, "Same tool receiver with late-region silhouette."),
    AssetSpec("art.prop.r03_shatter_rock", "FE_PROP_R03_ShatterRock_01", "Prop", "region accent", "Region_03_AshenKeep", "P2", "rubble", "r03", "module", 2200, 2, "Large broken stones."),
    AssetSpec("art.prop.r03_bridge_post", "FE_PROP_R03_BridgePost_01", "Prop", "bridge support", "Region_03_AshenKeep", "P2", "moss_pillar", "r03", "1.5", 2600, 2, "Readable bridge support."),
    AssetSpec("art.hero.lead_tier02_01", "FE_CHAR_PLAYER_LeadTier02_01", "Character", "tier 2 lead player model", "P3_ModelBatch", "P3", "p3_playable_lead_t2", "common", "1.58", 9500, 5, "Tier 2 lead keeps compact folded-reliquary identity while adding a stronger front combat read."),
    AssetSpec("art.hero.guard_tier02_01", "FE_CHAR_PLAYER_GuardTier02_01", "Character", "tier 2 guard player model", "P3_ModelBatch", "P3", "p3_playable_guard_t2", "common", "1.48", 9500, 5, "Tier 2 guard reads as a wide clamp defender without becoming generic heavy armor."),
    AssetSpec("art.hero.caster_tier02_01", "FE_CHAR_PLAYER_CasterTier02_01", "Character", "tier 2 caster player model", "P3_ModelBatch", "P3", "p3_playable_caster_t2", "common", "1.58", 9500, 5, "Tier 2 caster carries the seal-stamp language with a clearer casting face."),
    AssetSpec("art.hero.scout_tier02_01", "FE_CHAR_PLAYER_ScoutTier02_01", "Character", "tier 2 scout player model", "P3_ModelBatch", "P3", "p3_playable_scout_t2", "common", "1.50", 9000, 5, "Tier 2 scout adds route-spool equipment and forward motion readability."),
    AssetSpec("art.hero.medic_tier02_01", "FE_CHAR_PLAYER_MedicTier02_01", "Character", "tier 2 support player model", "P3_ModelBatch", "P3", "p3_playable_medic_t2", "common", "1.48", 9000, 5, "Tier 2 support keeps soft readable support posture and distinct repair signal gear."),
    AssetSpec("art.hero.striker_01", "FE_CHAR_PLAYER_Striker_01", "Character", "close-range striker player model", "P3_ModelBatch", "P3", "p3_playable_striker", "common", "1.50", 9000, 5, "Striker class reads through paired block-gauntlet tools, not copied fist-fighter fantasy."),
    AssetSpec("art.enemy.r02_cinder_crawler_01", "FE_ENEMY_R02_CinderCrawler_01", "Enemy", "low swarm enemy", "P3_ModelBatch", "P3", "p3_enemy_crawler", "r02", "0.85", 5500, 4, "Low crawler has readable danger front and heat body without becoming insect-like."),
    AssetSpec("art.enemy.r02_clamp_guard_01", "FE_ENEMY_R02_ClampGuard_01", "Enemy", "shielded region 2 blocker", "P3_ModelBatch", "P3", "p3_enemy_clamp_guard", "r02", "1.25", 7500, 4, "Clamp guard clearly blocks from the front and exposes side openings."),
    AssetSpec("art.enemy.r02_heat_lobber_01", "FE_ENEMY_R02_HeatLobber_01", "Enemy", "arcing projectile pressure", "P3_ModelBatch", "P3", "p3_enemy_heat_lobber", "r02", "1.20", 7500, 4, "Heat lobber shows projectile cup, aim line, and hot core from gameplay distance."),
    AssetSpec("art.enemy.r02_bellow_charger_01", "FE_ENEMY_R02_BellowCharger_01", "Enemy", "charge enemy", "P3_ModelBatch", "P3", "p3_enemy_bellow_charger", "r02", "1.35", 8500, 4, "Bellow charger communicates forward weight and windup surface with low occlusion."),
    AssetSpec("art.enemy.r03_shardling_01", "FE_ENEMY_R03_Shardling_01", "Enemy", "late-region melee enemy", "P3_ModelBatch", "P3", "p3_enemy_shardling", "r03", "1.05", 6500, 4, "Shardling is sharper and colder than R01 enemies but keeps the same folded hostile grammar."),
    AssetSpec("art.enemy.r03_prism_caster_01", "FE_ENEMY_R03_PrismCaster_01", "Enemy", "late-region ranged caster", "P3_ModelBatch", "P3", "p3_enemy_prism_caster", "r03", "1.25", 8000, 4, "Prism caster has a readable front lens and cast origin without staff/robe archetypes."),
    AssetSpec("art.enemy.r03_mirror_guard_01", "FE_ENEMY_R03_MirrorGuard_01", "Enemy", "late-region reflective blocker", "P3_ModelBatch", "P3", "p3_enemy_mirror_guard", "r03", "1.25", 8000, 4, "Mirror guard reads as a front-shield enemy using folded mirror plates, not a generic shield knight."),
    AssetSpec("art.enemy.r03_glass_leaper_01", "FE_ENEMY_R03_GlassLeaper_01", "Enemy", "late-region jump attacker", "P3_ModelBatch", "P3", "p3_enemy_glass_leaper", "r03", "1.15", 7500, 4, "Glass leaper shows spring legs and attack direction with compact non-animal shapes."),
    AssetSpec("art.boss.r02_anvil_maw_01", "FE_BOSS_R02_AnvilMaw_01", "Boss", "region 2 miniboss", "P3_ModelBatch", "P3", "p3_miniboss_anvil_maw", "r02", "2.6", 20000, 5, "Anvil Maw has a slam face, weak socket, and hot side vents with clear arena footprint."),
    AssetSpec("art.boss.r03_mirror_maw_01", "FE_BOSS_R03_MirrorMaw_01", "Boss", "region 3 miniboss", "P3_ModelBatch", "P3", "p3_miniboss_mirror_maw", "r03", "2.6", 20000, 5, "Mirror Maw has a prism face, side anchors, and weak socket without copying a known boss silhouette."),
    AssetSpec("art.blockfield.r02_floor_hotplate_1x1", "FE_ENV_R02_BF_Floor_1x1_Hotplate", "Environment", "region 2 walkable floor", "P3_ModelBatch", "P3", "p3_block_r02_floor_hotplate", "r02", "2x2m module", 1200, 4, "Hotplate tile snaps to the grid and distinguishes safe floor from heat hazards."),
    AssetSpec("art.blockfield.r02_wall_iron_straight", "FE_ENV_R02_BF_Wall_Straight_Iron", "Environment", "region 2 boundary", "P3_ModelBatch", "P3", "p3_block_r02_wall_iron", "r02", "2x2m module", 1600, 4, "Iron wall blocks movement while staying camera-safe and chunky."),
    AssetSpec("art.blockfield.r02_hazard_vent_1x1", "FE_ENV_R02_BF_Hazard_Vent_1x1", "Environment", "region 2 hazard tile", "P3_ModelBatch", "P3", "p3_block_r02_hazard_vent", "r02", "2x2m module", 1800, 5, "Vent hazard uses the floor footprint and reads as danger before VFX."),
    AssetSpec("art.blockfield.r02_conveyor_bridge_1x2", "FE_ENV_R02_BF_ConveyorBridge_1x2", "Environment", "region 2 crossing module", "P3_ModelBatch", "P3", "p3_block_r02_bridge_conveyor", "r02", "2x4m module", 1700, 4, "Conveyor bridge supports traversal without becoming a thin industrial prop."),
    AssetSpec("art.blockfield.r03_floor_crystal_1x1", "FE_ENV_R03_BF_Floor_1x1_Crystal", "Environment", "region 3 walkable floor", "P3_ModelBatch", "P3", "p3_block_r03_floor_crystal", "r03", "2x2m module", 1200, 4, "Crystal floor stays walkable and avoids noisy glitter."),
    AssetSpec("art.blockfield.r03_wall_shard_straight", "FE_ENV_R03_BF_Wall_Shard_Straight", "Environment", "region 3 boundary", "P3_ModelBatch", "P3", "p3_block_r03_wall_shard", "r03", "2x2m module", 1600, 4, "Shard wall communicates blocked state without tall occlusion."),
    AssetSpec("art.blockfield.r03_prism_gate_1x1", "FE_ENV_R03_BF_PrismGate_1x1", "Environment", "region 3 gate module", "P3_ModelBatch", "P3", "p3_block_r03_prism_gate", "r03", "2x2m module", 1800, 4, "Prism gate reads as a lockable route module with clear front."),
    AssetSpec("art.blockfield.r03_glass_bridge_1x2", "FE_ENV_R03_BF_GlassBridge_1x2", "Environment", "region 3 crossing module", "P3_ModelBatch", "P3", "p3_block_r03_bridge_glass", "r03", "2x4m module", 1700, 4, "Glass bridge is traversable and cold-region distinct without becoming transparent noise."),
    AssetSpec("art.prop.r02_pressure_switch_01", "FE_PROP_R02_PressureSwitch_01", "Prop", "region 2 tool switch", "P3_ModelBatch", "P3", "p3_interactable_r02_switch", "r02", "1.1", 2600, 4, "Pressure switch has idle/pressed read and clear tool receiver socket."),
    AssetSpec("art.prop.r02_furnace_gate_01", "FE_PROP_R02_FurnaceGate_01", "Prop", "region 2 route gate", "P3_ModelBatch", "P3", "p3_interactable_r02_gate", "r02", "module", 3000, 4, "Furnace gate reads as a blocked/unblocked route, not a decorative doorway."),
    AssetSpec("art.prop.r03_mirror_switch_01", "FE_PROP_R03_MirrorSwitch_01", "Prop", "region 3 tool switch", "P3_ModelBatch", "P3", "p3_interactable_r03_switch", "r03", "1.1", 2600, 4, "Mirror switch presents a front lens socket and active state shape."),
    AssetSpec("art.prop.r03_prism_lock_01", "FE_PROP_R03_PrismLock_01", "Prop", "region 3 route lock", "P3_ModelBatch", "P3", "p3_interactable_r03_lock", "r03", "module", 3000, 4, "Prism lock is a readable tool receiver and route blocker with no text dependency."),
    AssetSpec("art.env.boss_arena_floor", "FE_ENV_BOSS_ArenaFloor_01", "Environment", "boss arena floor", "Boss_04_Final", "P1", "boss_arena_floor", "boss", "module", 4000, 2, "Readable contained fight floor."),
    AssetSpec("art.prop.boss_boundary", "FE_PROP_BOSS_Boundary_01", "Prop", "boss arena boundary", "Boss_04_Final", "P1", "boss_boundary", "boss", "module", 3000, 2, "Boundary stays low and clear."),
    AssetSpec("art.prop.boss_spawn_anchor", "FE_PROP_BOSS_SpawnAnchor_01", "Prop", "boss spawn marker", "Boss_04_Final", "P1", "boss_spawn_anchor", "boss", "1.2", 2500, 2, "Spawn/read point for boss entrance."),
    AssetSpec("art.prop.boss_danger_decal", "FE_PROP_BOSS_DangerDecal_01", "Prop", "danger surface", "Boss_04_Final", "P1", "boss_danger_decal", "boss", "module", 1200, 1, "Clear danger shape on ground."),
    AssetSpec("art.prop.boss_reward_exit", "FE_PROP_BOSS_RewardExit_01", "Prop", "post-boss exit", "Boss_04_Final", "P1", "boss_reward_exit", "boss", "2.0", 3000, 2, "Reward exit reads after boss defeat."),
    AssetSpec("art.detail.hub_quarter_inlay", "FE_PROP_HUB_QuarterInlay_01", "Prop", "hub floor detail", "Hub_Crossroads", "P1", "detail_quarter_inlay", "hub", "module", 800, 2, "Small four-part floor mark raises hub edge density without text."),
    AssetSpec("art.detail.hub_pebble_line", "FE_PROP_HUB_PebbleLine_01", "Prop", "hub path detail", "Hub_Crossroads", "P1", "detail_pebble_line", "hub", "module", 900, 2, "Chunky pebble line gives route texture."),
    AssetSpec("art.detail.hub_blue_chip", "FE_PROP_HUB_BlueChip_01", "Prop", "hub color accent", "Hub_Crossroads", "P1", "detail_color_chip", "hub", "module", 700, 2, "Small cool accent helps hub palette separation."),
    AssetSpec("art.detail.hub_split_curb", "FE_PROP_HUB_SplitCurb_01", "Prop", "hub boundary detail", "Hub_Crossroads", "P1", "detail_split_curb", "hub", "module", 1000, 2, "Low split curb adds readable boundary detail."),
    AssetSpec("art.detail.hub_thread_lamp", "FE_PROP_HUB_ThreadLamp_01", "Prop", "hub light detail", "Hub_Crossroads", "P1", "detail_thread_lamp", "hub", "1.0", 1200, 2, "Small lamp supports warm/cool contrast."),
    AssetSpec("art.detail.hub_step_chips", "FE_PROP_HUB_StepChips_01", "Prop", "hub step detail", "Hub_Crossroads", "P1", "detail_step_chips", "hub", "module", 900, 2, "Tiny stair chips avoid flat surfaces."),
    AssetSpec("art.detail.r01_root_knot", "FE_PROP_R01_RootKnot_01", "Prop", "region micro landmark", "Region_01_GreenRuins", "P1", "detail_root_knot", "r01", "module", 1000, 2, "Root knot adds organic silhouette without blocking actors."),
    AssetSpec("art.detail.r01_leaf_sprig", "FE_PROP_R01_LeafSprig_01", "Prop", "region vegetation detail", "Region_01_GreenRuins", "P1", "detail_leaf_sprig", "r01", "module", 1000, 2, "Readable leaf cluster raises saturation and edge density."),
    AssetSpec("art.detail.r01_petal_patch", "FE_PROP_R01_PetalPatch_01", "Prop", "region color accent", "Region_01_GreenRuins", "P1", "detail_petal_patch", "r01", "module", 900, 2, "Yellow low flowers add small color beats."),
    AssetSpec("art.detail.r01_crack_inlay", "FE_PROP_R01_CrackInlay_01", "Prop", "floor contrast detail", "Region_01_GreenRuins", "P1", "detail_crack_inlay", "r01", "module", 900, 2, "Dark crack mark raises floor contrast."),
    AssetSpec("art.detail.r01_moss_curb", "FE_PROP_R01_MossCurb_01", "Prop", "route edge detail", "Region_01_GreenRuins", "P1", "detail_moss_curb", "r01", "module", 1000, 2, "Moss curb adds route read and texture."),
    AssetSpec("art.detail.r01_half_tile", "FE_PROP_R01_HalfBuriedTile_01", "Prop", "ruin scatter detail", "Region_01_GreenRuins", "P1", "detail_half_tile", "r01", "module", 1000, 2, "Half-buried tile breaks the floor grid."),
    AssetSpec("art.detail.r01_sap_bud", "FE_PROP_R01_SapGlowBud_01", "Prop", "tool-adjacent color accent", "Region_01_GreenRuins", "P1", "detail_glow_bud", "r01", "0.5", 1000, 2, "Small glow bud supports one-tool color language."),
    AssetSpec("art.detail.r01_fold_marker", "FE_PROP_R01_FoldedStoneMarker_01", "Prop", "route micro landmark", "Region_01_GreenRuins", "P1", "detail_fold_marker", "r01", "module", 1000, 2, "Folded marker creates proprietary route language."),
    AssetSpec("art.detail.r01_shadow_leaf", "FE_PROP_R01_ShadowLeafPatch_01", "Prop", "ground shadow detail", "Region_01_GreenRuins", "P1", "detail_shadow_patch", "r01", "module", 700, 1, "Dark low patch adds contrast without tall clutter."),
    AssetSpec("art.detail.r01_gold_thread", "FE_PROP_R01_GoldThread_01", "Prop", "reward route detail", "Region_01_GreenRuins", "P1", "detail_thread_line", "r01", "module", 700, 1, "Thin chunky gold line helps screenshot composition."),
    AssetSpec("art.detail.r02_rivet_cluster", "FE_PROP_R02_RivetCluster_01", "Prop", "industrial detail", "Region_02_SunkenWorks", "P2", "detail_rivet_cluster", "r02", "module", 1000, 2, "Large rivets add edge density without tiny noise."),
    AssetSpec("art.detail.r02_amber_wire", "FE_PROP_R02_AmberWire_01", "Prop", "region color detail", "Region_02_SunkenWorks", "P2", "detail_thread_line", "r02", "module", 700, 1, "Amber line reinforces region identity."),
    AssetSpec("art.detail.r02_charcoal_plate", "FE_PROP_R02_CharcoalPlate_01", "Prop", "floor contrast detail", "Region_02_SunkenWorks", "P2", "detail_half_tile", "r02", "module", 1000, 2, "Dark plate with rust trim increases contrast."),
    AssetSpec("art.detail.r02_bent_bracket", "FE_PROP_R02_BentBracket_01", "Prop", "industrial silhouette detail", "Region_02_SunkenWorks", "P2", "detail_fold_marker", "r02", "module", 1000, 2, "Bent bracket gives non-organic shape language."),
    AssetSpec("art.detail.r02_heat_pebble", "FE_PROP_R02_HeatPebble_01", "Prop", "hazard color detail", "Region_02_SunkenWorks", "P2", "detail_glow_bud", "r02", "0.5", 900, 2, "Small hot accent supports danger routes."),
    AssetSpec("art.detail.r02_slag_scatter", "FE_PROP_R02_SlagScatter_01", "Prop", "floor scatter detail", "Region_02_SunkenWorks", "P2", "detail_pebble_line", "r02", "module", 900, 2, "Chunky slag scatter avoids empty floors."),
    AssetSpec("art.detail.r03_crystal_chip_line", "FE_PROP_R03_CrystalChipLine_01", "Prop", "crystal route detail", "Region_03_AshenKeep", "P2", "detail_pebble_line", "r03", "module", 900, 2, "Crystal chips add cold edge density."),
    AssetSpec("art.detail.r03_violet_thread", "FE_PROP_R03_VioletThread_01", "Prop", "late route color detail", "Region_03_AshenKeep", "P2", "detail_thread_line", "r03", "module", 700, 1, "Violet line supports late-region identity."),
    AssetSpec("art.detail.r03_dark_inlay", "FE_PROP_R03_DarkInlay_01", "Prop", "floor contrast detail", "Region_03_AshenKeep", "P2", "detail_quarter_inlay", "r03", "module", 800, 2, "Dark/cold floor mark raises composition contrast."),
    AssetSpec("art.detail.r03_shard_sprig", "FE_PROP_R03_ShardSprig_01", "Prop", "crystal vegetation analog", "Region_03_AshenKeep", "P2", "detail_leaf_sprig", "r03", "module", 1000, 2, "Shard cluster adds unique late-region silhouette."),
    AssetSpec("art.detail.r03_cold_spark", "FE_PROP_R03_ColdSpark_01", "Prop", "cool glow detail", "Region_03_AshenKeep", "P2", "detail_glow_bud", "r03", "0.5", 900, 2, "Small cool glow supports readable late routes."),
    AssetSpec("art.detail.r03_bridge_chip", "FE_PROP_R03_BridgeChip_01", "Prop", "bridge detail", "Region_03_AshenKeep", "P2", "detail_half_tile", "r03", "module", 1000, 2, "Small chipped plates make bridges less flat."),
    AssetSpec("art.detail.boss_crack_bloom", "FE_PROP_BOSS_CrackBloom_01", "Prop", "boss arena contrast detail", "Boss_04_Final", "P1", "detail_crack_inlay", "boss", "module", 900, 2, "Arena cracks raise danger surface readability."),
    AssetSpec("art.detail.boss_red_thread", "FE_PROP_BOSS_RedThread_01", "Prop", "boss danger line", "Boss_04_Final", "P1", "detail_thread_line", "boss", "module", 700, 1, "Low red line gives attack-read composition."),
    AssetSpec("art.detail.boss_black_chip", "FE_PROP_BOSS_BlackChip_01", "Prop", "boss floor detail", "Boss_04_Final", "P1", "detail_color_chip", "boss", "module", 700, 2, "Dark chip supports arena contrast."),
    AssetSpec("art.detail.common_soft_shadow", "FE_PROP_COMMON_SoftShadowPatch_01", "Prop", "grounding detail", "vertical slice", "P1", "detail_shadow_patch", "common", "module", 700, 1, "Low dark patch grounds key objects in screenshots."),
]


FORBIDDEN_TERMS = [
    "final fantasy",
    "ff-style",
    "octopath",
    "hd-2d",
    "dragon quest",
    "maplestory",
    "maplestory2",
    "maple story",
    "akira toriyama",
    "square enix",
    "nintendo",
    "ファイナルファンタジー",
    "ドラクエ",
    "鳥山",
    "ff風",
    "dq風",
    "chocobo",
    "moogle",
    "cactuar",
    "tonberry",
]


def main() -> None:
    ensure_clean_scene()
    ensure_dirs()
    materials = create_materials()
    records = []

    for spec in ASSETS:
        assert_commercial_safe(spec)
        collection = bpy.data.collections.new(spec.name)
        bpy.context.scene.collection.children.link(collection)
        build_asset(spec, collection, materials)
        apply_art_direction_dna(spec, collection, materials)
        consolidate_material_budget(spec, collection, materials)
        normalize_asset(collection)
        model_file = model_path(spec)
        preview_file = PREVIEW_DIR / f"{spec.name}.png"
        export_asset(collection, model_file)
        render_preview(collection, preview_file)
        records.append(make_record(spec, collection, model_file, preview_file))

    create_contact_sheets(records)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_FILE))
    MANIFEST_FILE.write_text(json.dumps({
        "version": 1,
        "generated_at": date.today().isoformat(),
        "source_tool": "repository-authored Blender Python procedural generation",
        "source_file": rel(SOURCE_FILE),
        "art_direction_id": ART_DIRECTION_ID,
        "art_direction_name": ART_DIRECTION_NAME,
        "art_direction_summary": ART_DIRECTION_SUMMARY,
        "brand_line_id": BRAND_LINE_ID,
        "brand_line_name": BRAND_LINE_NAME,
        "genre_statement": GENRE_STATEMENT,
        "style_bible": STYLE_BIBLE_DOC,
        "product_line_contract": PRODUCT_LINE_CONTRACT,
        "genre_contract_id": BRAND_LINE_ID,
        "shape_grammar_id": ART_DIRECTION_ID,
        "benchmark_policy_id": "external_market_metrics_only",
        "benchmark_report_path": rel(REPORT_DIR / "visual-benchmark.json"),
        "commercial_safety_policy_id": "repository_authored_non_derivative_no_external_style_prompts",
        "consistency_summary": consistency_summary(records),
        "art_direction_dna": ART_DIRECTION_DNA,
        "art_direction_contract": art_direction_contract(),
        "style_families": STYLE_FAMILIES,
        "regional_variant_policy": REGIONAL_VARIANT_POLICY,
        "external_benchmark_use": "quality metrics only; not prompts, source assets, style targets, or derivative instructions",
        "comparison_scope": "aggregate market finish metrics only",
        "human_review_required": "commercial/IP/style/trademark/likeness review before market-facing use",
        "license": "project-owned",
        "attribution": "none",
        "style_ip_clearance": "original FOURFOLD folded-reliquary semantic shape language; no protected characters, franchise lookalikes, trademarked style labels, or single-title trade dress",
        "assets": records,
    }, indent=2) + "\n", encoding="utf-8")
    print(f"Generated {len(records)} FOURFOLD model assets.")
    print(f"Manifest: {MANIFEST_FILE}")


def ensure_dirs() -> None:
    priority_dirs = {PRODUCTION_ROOT / spec.priority / "Models" for spec in ASSETS}
    for folder in (*priority_dirs, SOURCE_DIR, PREVIEW_DIR, REPORT_DIR):
        folder.mkdir(parents=True, exist_ok=True)


def art_direction_contract() -> dict:
    contract = dict(ART_DIRECTION_CONTRACT)
    contract["forbidden_terms"] = FORBIDDEN_TERMS
    contract["product_line_contract"] = PRODUCT_LINE_CONTRACT
    return contract


def ensure_clean_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1.0
    bpy.context.scene.render.engine = "BLENDER_WORKBENCH"
    bpy.context.scene.render.resolution_x = 640
    bpy.context.scene.render.resolution_y = 640
    bpy.context.scene.world = bpy.data.worlds.new("FE_World") if bpy.context.scene.world is None else bpy.context.scene.world
    bpy.context.scene.world.color = (0.075, 0.08, 0.095)
    camera_data = bpy.data.cameras.new("FE_PreviewCamera")
    camera = bpy.data.objects.new("FE_PreviewCamera", camera_data)
    bpy.context.scene.collection.objects.link(camera)
    bpy.context.scene.camera = camera
    light_data = bpy.data.lights.new("FE_PreviewKey", "AREA")
    light = bpy.data.objects.new("FE_PreviewKey", light_data)
    light.location = (3.5, -4.0, 5.5)
    light_data.energy = 550
    light_data.size = 4.0
    bpy.context.scene.collection.objects.link(light)


def create_materials() -> dict[str, bpy.types.Material]:
    palette = {
        "stone_warm": ((0.72, 0.64, 0.46, 1.0), None, 0.0),
        "stone_dark": ((0.12, 0.12, 0.14, 1.0), None, 0.0),
        "hub_ivory": ((0.94, 0.80, 0.52, 1.0), None, 0.0),
        "gold": ((1.0, 0.67, 0.16, 1.0), (0.72, 0.30, 0.03, 1.0), 0.35),
        "tool_signal": ((0.86, 1.0, 0.22, 1.0), (0.55, 0.78, 0.03, 1.0), 0.0),
        "tool_blue": ((0.10, 0.72, 1.0, 1.0), (0.02, 0.22, 0.75, 1.0), 0.0),
        "route_teal": ((0.08, 0.55, 0.52, 1.0), (0.02, 0.18, 0.16, 1.0), 0.0),
        "hero_cloth": ((0.08, 0.16, 0.68, 1.0), None, 0.0),
        "hero_ivory": ((1.0, 0.80, 0.48, 1.0), None, 0.0),
        "cloth_coral": ((1.0, 0.34, 0.24, 1.0), None, 0.0),
        "leather_blue": ((0.05, 0.27, 0.76, 1.0), None, 0.0),
        "medic_mint": ((0.18, 0.88, 0.62, 1.0), (0.04, 0.45, 0.24, 1.0), 0.0),
        "stamp_magenta": ((0.78, 0.20, 0.86, 1.0), (0.30, 0.02, 0.52, 1.0), 0.0),
        "metal": ((0.62, 0.66, 0.68, 1.0), None, 0.55),
        "enemy_ink": ((0.025, 0.018, 0.035, 1.0), None, 0.0),
        "enemy_armor": ((0.13, 0.09, 0.08, 1.0), None, 0.1),
        "danger_red": ((1.0, 0.10, 0.03, 1.0), (0.95, 0.02, 0.01, 1.0), 0.0),
        "moss": ((0.16, 0.48, 0.18, 1.0), None, 0.0),
        "leaf": ((0.04, 0.62, 0.23, 1.0), None, 0.0),
        "flower_yellow": ((1.0, 0.88, 0.08, 1.0), None, 0.0),
        "wood": ((0.50, 0.25, 0.08, 1.0), None, 0.0),
        "root": ((0.22, 0.11, 0.05, 1.0), None, 0.0),
        "relic_blue": ((0.06, 0.72, 1.0, 1.0), (0.02, 0.34, 0.72, 1.0), 0.0),
        "ember": ((1.0, 0.32, 0.05, 1.0), (0.95, 0.09, 0.01, 1.0), 0.0),
        "r02_rust": ((0.78, 0.22, 0.08, 1.0), None, 0.0),
        "r02_charcoal": ((0.08, 0.08, 0.09, 1.0), None, 0.0),
        "r02_amber": ((1.0, 0.52, 0.05, 1.0), (0.85, 0.22, 0.0, 1.0), 0.0),
        "r03_dark": ((0.06, 0.08, 0.20, 1.0), None, 0.0),
        "r03_violet": ((0.45, 0.16, 0.85, 1.0), (0.20, 0.03, 0.65, 1.0), 0.0),
        "r03_crystal": ((0.38, 0.86, 1.0, 1.0), (0.04, 0.42, 0.78, 1.0), 0.0),
        "white_glow": ((0.82, 0.96, 1.0, 1.0), (0.25, 0.62, 0.90, 1.0), 0.0),
    }
    return {key: make_material(f"FE_MAT_{key}", color, emission, metallic) for key, (color, emission, metallic) in palette.items()}


def make_material(name: str, color, emission, metallic: float) -> bpy.types.Material:
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        set_input(bsdf, "Base Color", color)
        set_input(bsdf, "Metallic", metallic)
        set_input(bsdf, "Roughness", 0.72)
        if emission:
            set_input(bsdf, "Emission Color", emission)
            set_input(bsdf, "Emission Strength", 1.4)
    material.diffuse_color = color
    return material


def set_input(node, name: str, value) -> None:
    socket = node.inputs.get(name)
    if socket is not None:
        socket.default_value = value


def build_asset(spec: AssetSpec, col: bpy.types.Collection, m: dict[str, bpy.types.Material]) -> None:
    builder = spec.builder
    if builder == "hero":
        build_hero(col, m)
    elif builder == "pop_hero_lead" or builder.startswith("pop_playable_"):
        build_pop_playable(col, m, builder)
    elif builder.startswith("pop_npc_"):
        build_pop_npc(col, m, builder)
    elif builder.startswith("pop_enemy_"):
        build_pop_enemy(col, m, builder)
    elif builder == "pop_miniboss_stamp":
        build_pop_miniboss_stamp(col, m)
    elif builder.startswith("pop_weapon_"):
        build_pop_weapon(col, m, builder)
    elif builder == "block_combat_arena_2x2":
        build_block_combat_arena_2x2(col, m)
    elif builder in ("checkpoint_socket_pad", "reward_receiver_pad"):
        build_service_pad(col, m, builder)
    elif builder == "exploration_tool":
        build_exploration_tool(col, m)
    elif builder == "melee_enemy":
        build_melee_enemy(col, m)
    elif builder == "ranged_enemy":
        build_ranged_enemy(col, m)
    elif builder == "miniboss":
        build_miniboss(col, m)
    elif builder in ("boss_root", "boss_furnace", "boss_glass", "boss_crown"):
        build_boss(col, m, builder)
    elif builder == "pedestal":
        build_pedestal(col, m)
    elif builder == "shortcut_bridge":
        build_shortcut_bridge(col, m)
    elif builder == "root_gate":
        build_root_gate(col, m)
    elif builder == "chest":
        build_chest(col, m)
    elif builder.startswith("relic_"):
        build_relic(col, m, builder)
    else:
        build_modular_prop(col, m, spec)


def apply_art_direction_dna(spec: AssetSpec, col: bpy.types.Collection, m: dict[str, bpy.types.Material]) -> None:
    if spec.builder.startswith("detail_"):
        return
    if spec.builder == "shortcut_bridge":
        return

    mats = style_materials(m, spec.style)
    if spec.category in ("Prop", "Environment"):
        folded_plinth(col, m, mats, "dna", scale=0.72)
        if has_signal_role(spec):
            signal_thread(col, "dna_signal_thread", (0.0, -0.42, 0.110), 0.44, mats["signal"], angle=6)
    elif spec.category == "Character":
        signal_thread(col, "dna_hero_fold_mark", (-0.34, -0.42, 0.070), 0.24, m["stone_warm"], angle=18)
        chunk_tab(col, "dna_hero_shadow_cut", (0.30, -0.40, 0.064), m["stone_dark"], angle=-18)
    elif spec.category in ("Enemy", "Boss"):
        signal_thread(col, "dna_hostile_front_thread", (0.0, -0.68, 0.075), 0.48, m["danger_red"], angle=5)
        chunk_tab(col, "dna_hostile_fold_cut", (0.36, -0.48, 0.070), m["enemy_ink"], angle=-23)


def folded_plinth(col, m, mats: dict[str, bpy.types.Material], prefix: str, scale: float = 1.0) -> None:
    cube(col, f"{prefix}_fold_shadow", (-0.20 * scale, -0.28 * scale, 0.052), (0.42 * scale, 0.040 * scale, 0.024), mats["dark"], rot=(0, 0, math.radians(-12)))
    split_inlay(col, m, mats, prefix, scale)
    chunk_tab(col, f"{prefix}_raised_tab", (0.35 * scale, -0.20 * scale, 0.086), mats["accent"], angle=22, scale=scale)


def split_inlay(col, m, mats: dict[str, bpy.types.Material], prefix: str, scale: float = 1.0) -> None:
    cube(col, f"{prefix}_split_inlay_a", (-0.08 * scale, -0.36 * scale, 0.078), (0.30 * scale, 0.026 * scale, 0.022), mats["accent"], rot=(0, 0, math.radians(7)))
    cube(col, f"{prefix}_split_inlay_b", (0.12 * scale, -0.30 * scale, 0.080), (0.22 * scale, 0.024 * scale, 0.022), mats["dark"], rot=(0, 0, math.radians(-19)))


def signal_thread(col, name: str, loc, length: float, material, angle: float = 0.0) -> None:
    cube(col, name, loc, (length, 0.026, 0.024), material, rot=(0, 0, math.radians(angle)))


def chunk_tab(col, name: str, loc, material, angle: float = 0.0, scale: float = 1.0) -> None:
    cube(col, name, loc, (0.16 * scale, 0.030 * scale, 0.026), material, rot=(0, 0, math.radians(angle)))


def has_signal_role(spec: AssetSpec) -> bool:
    text = " ".join((spec.asset_id, spec.name, spec.gameplay_role, spec.builder)).lower()
    return any(token in text for token in ("tool", "signal", "relay", "reward", "relic", "chest", "gate", "marker", "danger", "boss", "route", "wayline"))


def build_hero(col, m) -> None:
    warm = m["stone_warm"]
    dark = m["stone_dark"]
    signal = m["tool_signal"]
    cube(col, "plinth_front_fold_plate", (0.00, -0.38, 0.020), (0.58, 0.24, 0.035), warm, rot=(0, 0, math.radians(-2)))
    cube(col, "plinth_rear_shadow_plate", (0.07, 0.30, 0.018), (0.54, 0.23, 0.032), dark, rot=(0, 0, math.radians(5)))
    cube(col, "plinth_left_fold_plate", (-0.36, -0.02, 0.024), (0.22, 0.52, 0.034), warm, rot=(0, 0, math.radians(8)))
    cube(col, "plinth_right_shadow_cut", (0.36, 0.03, 0.022), (0.20, 0.48, 0.032), dark, rot=(0, 0, math.radians(-7)))
    cube(col, "plinth_front_missing_notch", (-0.22, -0.50, 0.050), (0.15, 0.052, 0.040), dark, rot=(0, 0, math.radians(13)))

    cube(col, "left_ground_tab", (-0.22, -0.15, 0.17), (0.26, 0.38, 0.17), dark, rot=(0, 0, math.radians(-8)))
    cube(col, "right_ground_tab", (0.20, 0.08, 0.17), (0.24, 0.36, 0.17), dark, rot=(0, 0, math.radians(7)))
    cube(col, "left_front_fold_cut", (-0.24, -0.36, 0.30), (0.20, 0.060, 0.058), warm, rot=(0, 0, math.radians(13)))
    cube(col, "right_rear_fold_cut", (0.20, 0.30, 0.30), (0.18, 0.056, 0.056), warm, rot=(0, 0, math.radians(-11)))

    tapered_box(col, "folded_torso_core", (0, -0.02, 0.84), (0.42, 0.33), (0.31, 0.25), 0.62, warm, rot=(0, 0, math.radians(2)))
    cube(col, "left_body_side_plate", (-0.25, -0.04, 0.86), (0.10, 0.27, 0.60), dark, rot=(0, 0, math.radians(-7)))
    cube(col, "right_body_side_plate", (0.25, -0.02, 0.88), (0.10, 0.24, 0.58), warm, rot=(0, 0, math.radians(8)))
    tapered_box(col, "front_fold_plate", (0.02, -0.32, 0.92), (0.28, 0.055), (0.22, 0.050), 0.52, warm, rot=(0, 0, math.radians(4)))
    cube(col, "front_shadow_split", (-0.12, -0.35, 0.92), (0.052, 0.038, 0.52), dark, rot=(0, 0, math.radians(-8)))
    cube(col, "chest_socket_signal_thread", (0.01, -0.38, 1.08), (0.46, 0.030, 0.042), signal, rot=(0, 0, math.radians(-15)))
    cube(col, "belt_shadow_bar", (0, -0.34, 0.76), (0.50, 0.052, 0.066), dark)
    cube(col, "left_hip_fold", (-0.29, -0.25, 0.70), (0.11, 0.065, 0.18), dark, rot=(0, 0, math.radians(10)))
    cube(col, "right_hip_fold", (0.29, -0.24, 0.72), (0.11, 0.065, 0.16), dark, rot=(0, 0, math.radians(-10)))

    cube(col, "rear_folded_backplate_center", (-0.14, 0.31, 0.92), (0.52, 0.095, 0.84), dark, rot=(0, 0, math.radians(2)))
    cube(col, "rear_backplate_left_chip", (-0.43, 0.28, 0.82), (0.13, 0.072, 0.60), dark, rot=(0, 0, math.radians(-12)))
    cube(col, "rear_backplate_gold_lip", (0.18, 0.34, 0.88), (0.12, 0.062, 0.68), warm, rot=(0, 0, math.radians(10)))

    tapered_box(col, "left_shoulder_fold", (-0.39, -0.08, 1.21), (0.24, 0.13), (0.18, 0.10), 0.075, dark, rot=(0, 0, math.radians(18)))
    tapered_box(col, "right_shoulder_fold", (0.36, -0.07, 1.19), (0.23, 0.13), (0.17, 0.10), 0.075, dark, rot=(0, 0, math.radians(-17)))
    cube(col, "left_upper_arm_bridge", (-0.40, -0.16, 1.02), (0.085, 0.070, 0.30), dark, rot=(math.radians(-6), 0, math.radians(12)))
    cube(col, "right_upper_arm_bridge", (0.38, -0.15, 1.00), (0.085, 0.070, 0.29), dark, rot=(math.radians(6), 0, math.radians(-12)))
    cube(col, "left_forearm_plate", (-0.46, -0.25, 0.86), (0.10, 0.075, 0.44), warm, rot=(math.radians(-8), 0, math.radians(22)))
    cube(col, "right_forearm_plate", (0.42, -0.24, 0.84), (0.10, 0.075, 0.42), dark, rot=(math.radians(10), 0, math.radians(-22)))

    tapered_box(col, "faceted_lid_head", (0, -0.03, 1.43), (0.25, 0.22), (0.20, 0.18), 0.22, warm, rot=(0, 0, math.radians(8)))
    cube(col, "mask_upper_shadow_slit", (-0.05, -0.24, 1.47), (0.12, 0.035, 0.040), dark, rot=(0, 0, math.radians(-3)))
    cube(col, "mask_lower_shadow_slit", (0.08, -0.245, 1.38), (0.10, 0.034, 0.036), dark, rot=(0, 0, math.radians(5)))
    cube(col, "head_front_fold_lip", (0.02, -0.19, 1.66), (0.30, 0.090, 0.080), warm, rot=(0, 0, math.radians(8)))
    cube(col, "head_shadow_notch", (-0.12, -0.245, 1.59), (0.10, 0.032, 0.060), dark, rot=(0, 0, math.radians(-7)))

    cube(col, "folded_relic_shard_tool", (0.50, -0.33, 0.82), (0.08, 0.056, 0.58), dark, rot=(math.radians(18), 0, math.radians(-28)))
    cube(col, "shard_fold_lip", (0.36, -0.23, 0.58), (0.28, 0.056, 0.080), warm, rot=(math.radians(18), 0, math.radians(-28)))
    cube(col, "shoulder_socket_spine", (-0.58, -0.18, 1.04), (0.10, 0.10, 0.62), dark, rot=(math.radians(-10), 0, math.radians(19)))
    cube(col, "shoulder_socket_upper_jaw", (-0.65, -0.20, 1.33), (0.24, 0.080, 0.070), dark, rot=(math.radians(-10), 0, math.radians(19)))
    cube(col, "shoulder_socket_lower_jaw", (-0.61, -0.24, 1.02), (0.22, 0.074, 0.064), dark, rot=(math.radians(-10), 0, math.radians(19)))
    cube(col, "socket_embedded_signal_core", (-0.69, -0.24, 1.18), (0.12, 0.036, 0.25), signal, rot=(math.radians(-10), 0, math.radians(19)))


def build_exploration_tool(col, m) -> None:
    dark = m["stone_dark"]
    signal = m["tool_signal"]
    cube(col, "folded_plinth_front", (0.00, -0.31, 0.025), (0.42, 0.20, 0.035), dark, rot=(0, 0, math.radians(-4)))
    cube(col, "folded_plinth_rear", (0.06, 0.22, 0.022), (0.38, 0.18, 0.032), dark, rot=(0, 0, math.radians(7)))
    cube(col, "ground_signal_slot", (0.06, -0.36, 0.060), (0.42, 0.040, 0.034), signal, rot=(0, 0, math.radians(4)))

    tapered_box(col, "folded_grip_spine", (0, -0.01, 0.52), (0.11, 0.10), (0.075, 0.075), 0.48, dark)
    cube(col, "front_grip_signal_inlay", (0.00, -0.095, 0.55), (0.055, 0.024, 0.44), signal)
    cube(col, "lower_socket_block", (0, -0.02, 0.94), (0.26, 0.20, 0.16), dark, rot=(0, 0, math.radians(2)))
    cube(col, "socket_back_shadow", (0, 0.16, 0.98), (0.30, 0.055, 0.10), dark, rot=(0, 0, math.radians(5)))

    cube(col, "left_cradle_leg", (-0.22, -0.02, 1.24), (0.065, 0.070, 0.52), dark, rot=(0, math.radians(-13), math.radians(-10)))
    cube(col, "right_cradle_leg", (0.22, -0.02, 1.24), (0.065, 0.070, 0.52), dark, rot=(0, math.radians(13), math.radians(10)))
    cube(col, "top_split_jaw", (0, -0.01, 1.52), (0.38, 0.075, 0.075), dark, rot=(0, 0, math.radians(2)))
    cyl(col, "faceted_signal_lens", (0, -0.02, 1.38), (0.19, 0.14, 0.24), signal, vertices=6, rot=(math.radians(90), 0, math.radians(30)))
    cube(col, "lens_shadow_bite", (0.17, -0.02, 1.40), (0.070, 0.16, 0.18), dark, rot=(0, 0, math.radians(-12)))
    cube(col, "rear_folded_counterweight", (0, 0.20, 1.20), (0.30, 0.060, 0.26), dark, rot=(0, 0, math.radians(3)))
    cube(col, "paired_signal_thread", (0, -0.080, 1.18), (0.32, 0.030, 0.045), signal, rot=(0, 0, math.radians(5)))


def build_pop_playable(col, m, builder: str) -> None:
    variants = {
        "pop_hero_lead": (m["cloth_coral"], m["leather_blue"], m["tool_signal"], 0.34, 0.60),
        "pop_playable_heavy": (m["leather_blue"], m["metal"], m["tool_signal"], 0.48, 0.56),
        "pop_playable_caster": (m["stamp_magenta"], m["hero_ivory"], m["relic_blue"], 0.32, 0.66),
        "pop_playable_ranger": (m["moss"], m["gold"], m["route_teal"], 0.30, 0.58),
        "pop_playable_medic": (m["medic_mint"], m["hub_ivory"], m["tool_blue"], 0.36, 0.55),
    }
    body, accent, signal, width, height = variants.get(builder, variants["pop_hero_lead"])
    dark = m["stone_dark"]

    cyl(col, "chunk_shadow_disc", (0, -0.05, 0.025), (0.56, 0.46, 0.035), dark, vertices=18)
    cube(col, "left_boot_block", (-0.17, -0.10, 0.18), (0.13, 0.20, 0.16), accent, rot=(0, 0, math.radians(-7)))
    cube(col, "right_boot_block", (0.17, -0.08, 0.18), (0.13, 0.20, 0.16), accent, rot=(0, 0, math.radians(8)))
    tapered_box(col, "compact_body", (0, -0.03, 0.62), (width, 0.28), (width * 0.78, 0.22), height, body)
    cube(col, "belt_read_band", (0, -0.285, 0.50), (width * 1.18, 0.040, 0.055), dark)
    cube(col, "front_signal_badge", (0.02, -0.310, 0.75), (width * 0.72, 0.034, 0.052), signal, rot=(0, 0, math.radians(-8)))
    sphere(col, "large_readable_head", (0, -0.04, 1.24), (0.25, 0.22, 0.22), m["hero_ivory"])
    cube(col, "face_shadow_bar", (0.02, -0.250, 1.25), (0.22, 0.032, 0.035), dark, rot=(0, 0, math.radians(3)))
    cube(col, "left_glove", (-width - 0.10, -0.17, 0.70), (0.12, 0.09, 0.22), accent, rot=(0, 0, math.radians(18)))
    cube(col, "right_glove", (width + 0.10, -0.18, 0.68), (0.12, 0.09, 0.22), accent, rot=(0, 0, math.radians(-18)))

    if builder == "pop_playable_heavy":
        cube(col, "clamp_shield_face", (-0.54, -0.24, 0.72), (0.18, 0.08, 0.50), m["tool_signal"], rot=(0, 0, math.radians(-8)))
        cube(col, "block_hammer_head", (0.58, -0.23, 0.85), (0.22, 0.12, 0.24), dark, rot=(0, 0, math.radians(10)))
    elif builder == "pop_playable_caster":
        cube(col, "seal_stamp_handle", (0.46, -0.22, 0.84), (0.07, 0.06, 0.58), dark, rot=(0, 0, math.radians(-18)))
        cyl(col, "seal_stamp_face", (0.55, -0.28, 0.52), (0.20, 0.20, 0.055), signal, vertices=8, rot=(math.radians(90), 0, math.radians(8)))
        cube(col, "robe_tail_fold", (0, 0.20, 0.42), (0.38, 0.10, 0.38), body)
    elif builder == "pop_playable_ranger":
        cyl(col, "route_spool", (0.48, -0.24, 0.78), (0.18, 0.18, 0.10), signal, vertices=14, rot=(math.radians(90), 0, 0))
        cube(col, "route_pointer", (0.58, -0.34, 0.84), (0.32, 0.035, 0.050), signal, rot=(0, 0, math.radians(-16)))
    elif builder == "pop_playable_medic":
        cube(col, "repair_satchel", (-0.46, -0.20, 0.58), (0.18, 0.10, 0.26), accent, rot=(0, 0, math.radians(12)))
        cube(col, "plus_signal_a", (0.34, -0.28, 0.80), (0.20, 0.030, 0.040), signal)
        cube(col, "plus_signal_b", (0.34, -0.28, 0.80), (0.030, 0.030, 0.20), signal)
    else:
        cube(col, "tool_blade_body", (0.50, -0.28, 0.86), (0.10, 0.06, 0.56), signal, rot=(math.radians(12), 0, math.radians(-24)))
        cube(col, "blue_backpack_tab", (-0.36, 0.12, 0.78), (0.16, 0.08, 0.38), m["leather_blue"])


def build_pop_npc(col, m, builder: str) -> None:
    variants = {
        "pop_npc_merchant": (m["gold"], m["cloth_coral"], m["tool_blue"]),
        "pop_npc_smith": (m["metal"], m["r02_rust"], m["tool_signal"]),
        "pop_npc_cartographer": (m["hub_ivory"], m["route_teal"], m["relic_blue"]),
    }
    body, accent, signal = variants.get(builder, variants["pop_npc_merchant"])
    dark = m["stone_dark"]
    cyl(col, "service_shadow_disc", (0, -0.03, 0.025), (0.52, 0.42, 0.035), dark, vertices=18)
    tapered_box(col, "service_body", (0, -0.03, 0.62), (0.34, 0.27), (0.28, 0.22), 0.50, body)
    sphere(col, "service_head", (0, -0.04, 1.18), (0.24, 0.21, 0.21), m["hero_ivory"])
    cube(col, "service_face_read", (0, -0.245, 1.18), (0.20, 0.032, 0.035), dark)
    if builder == "pop_npc_smith":
        cube(col, "measuring_jaw", (0.47, -0.24, 0.80), (0.12, 0.08, 0.50), accent, rot=(0, 0, math.radians(-22)))
        cube(col, "hammer_block", (0.62, -0.36, 0.98), (0.24, 0.10, 0.16), dark, rot=(0, 0, math.radians(-22)))
    elif builder == "pop_npc_cartographer":
        cube(col, "fold_map_panel", (0.42, -0.25, 0.78), (0.26, 0.045, 0.38), accent, rot=(0, 0, math.radians(-14)))
        cube(col, "route_pin", (0.42, -0.295, 0.86), (0.18, 0.026, 0.036), signal, rot=(0, 0, math.radians(18)))
    else:
        cube(col, "merchant_tray", (0.42, -0.28, 0.68), (0.32, 0.11, 0.07), accent, rot=(0, 0, math.radians(-8)))
        sphere(col, "tray_blue_goods", (0.32, -0.33, 0.79), (0.08, 0.06, 0.06), signal)
        sphere(col, "tray_gold_goods", (0.52, -0.32, 0.79), (0.08, 0.06, 0.06), m["flower_yellow"])


def build_pop_enemy(col, m, builder: str) -> None:
    dark = m["enemy_ink"]
    armor = m["enemy_armor"]
    danger = m["danger_red"]
    cyl(col, "danger_footprint", (0, -0.10, 0.025), (0.54, 0.42, 0.035), danger, vertices=18)
    tapered_box(col, "hostile_core", (0, -0.02, 0.56), (0.38, 0.30), (0.30, 0.23), 0.48, dark)
    cube(col, "front_tell_bar", (0, -0.315, 0.68), (0.40, 0.040, 0.060), danger)
    sphere(col, "hostile_head", (0, -0.04, 1.06), (0.22, 0.20, 0.18), armor)
    cube(col, "eye_read_slit", (0, -0.230, 1.08), (0.20, 0.030, 0.035), danger)
    if builder == "pop_enemy_shield":
        cube(col, "front_clamp_shield", (0, -0.42, 0.62), (0.48, 0.075, 0.58), armor)
        cube(col, "shield_weak_side", (0.40, -0.34, 0.66), (0.07, 0.055, 0.45), m["tool_signal"], rot=(0, 0, math.radians(-8)))
    elif builder == "pop_enemy_turret":
        cyl(col, "locked_base", (0, 0.06, 0.18), (0.42, 0.42, 0.16), armor, vertices=10)
        cube(col, "aiming_line", (0, -0.74, 0.40), (0.82, 0.040, 0.045), danger)
        sphere(col, "muzzle_read", (0, -0.46, 0.72), (0.13, 0.07, 0.13), danger)
    elif builder == "pop_enemy_charger":
        cube(col, "charger_brow_mass", (0, -0.31, 1.10), (0.58, 0.08, 0.16), armor)
        cone(col, "left_windup_horn", (-0.26, -0.35, 1.22), (0.11, 0.11, 0.38), danger, vertices=5, rot=(math.radians(90), 0, math.radians(-12)))
        cone(col, "right_windup_horn", (0.26, -0.35, 1.22), (0.11, 0.11, 0.38), danger, vertices=5, rot=(math.radians(90), 0, math.radians(12)))
        cube(col, "side_dodge_read", (0.54, -0.08, 0.58), (0.10, 0.08, 0.46), m["gold"])
    elif builder == "pop_enemy_swarm":
        for i, (x, y) in enumerate(((-0.24, -0.08), (0.00, -0.16), (0.24, -0.08))):
            sphere(col, f"swarm_glyph_{i+1}", (x, y, 0.48), (0.16, 0.14, 0.14), dark)
            cube(col, f"swarm_tip_{i+1}", (x, y - 0.16, 0.54), (0.10, 0.025, 0.08), danger)
    else:
        cube(col, "bite_upper", (0, -0.36, 0.84), (0.42, 0.050, 0.08), danger, rot=(0, 0, math.radians(5)))
        cube(col, "bite_lower", (0, -0.34, 0.60), (0.36, 0.044, 0.07), danger, rot=(0, 0, math.radians(-5)))


def build_pop_miniboss_stamp(col, m) -> None:
    dark = m["enemy_ink"]
    danger = m["danger_red"]
    signal = m["tool_signal"]
    cyl(col, "stamp_warning_floor", (0, -0.05, 0.025), (1.02, 0.82, 0.040), danger, vertices=24)
    tapered_box(col, "stamp_body_mass", (0, 0, 0.92), (0.72, 0.55), (0.54, 0.42), 0.78, dark)
    cube(col, "front_stamp_face", (0, -0.56, 0.90), (0.62, 0.075, 0.64), m["enemy_armor"])
    cube(col, "weak_socket", (0, -0.615, 1.06), (0.34, 0.036, 0.12), signal)
    cube(col, "left_stamp_arm", (-0.72, -0.18, 0.82), (0.18, 0.15, 0.74), dark, rot=(0, 0, math.radians(-20)))
    cube(col, "right_stamp_arm", (0.72, -0.18, 0.82), (0.18, 0.15, 0.74), dark, rot=(0, 0, math.radians(20)))
    cube(col, "raised_stamp_tool", (0.84, -0.50, 1.22), (0.24, 0.16, 0.72), danger, rot=(math.radians(20), 0, math.radians(-28)))
    sphere(col, "phase_core", (0, -0.60, 1.36), (0.18, 0.08, 0.18), m["relic_blue"])


def build_pop_weapon(col, m, builder: str) -> None:
    dark = m["stone_dark"]
    signal = m["tool_signal"]
    if builder == "pop_weapon_hammer":
        cube(col, "short_handle", (0, 0, 0.48), (0.06, 0.06, 0.86), dark)
        cube(col, "block_head", (0, -0.02, 0.96), (0.36, 0.18, 0.20), m["gold"], rot=(0, 0, math.radians(5)))
        cube(col, "signal_slot", (0, -0.12, 0.98), (0.28, 0.030, 0.042), signal)
    elif builder == "pop_weapon_guard_clamp":
        cube(col, "clamp_spine", (0, 0, 0.52), (0.09, 0.08, 0.82), dark)
        cube(col, "upper_jaw", (0.18, -0.02, 0.88), (0.34, 0.08, 0.10), m["metal"])
        cube(col, "lower_jaw", (0.15, -0.02, 0.48), (0.30, 0.08, 0.10), m["metal"])
        sphere(col, "clamp_core", (0.05, -0.08, 0.66), (0.11, 0.06, 0.11), signal)
    elif builder == "pop_weapon_seal_stamp":
        cube(col, "stamp_handle", (0, 0, 0.60), (0.07, 0.06, 0.92), dark)
        cyl(col, "stamp_face", (0, -0.04, 0.18), (0.28, 0.28, 0.07), m["stamp_magenta"], vertices=8)
        cube(col, "stamp_mark", (0, -0.08, 0.25), (0.30, 0.026, 0.036), signal, rot=(0, 0, math.radians(20)))
    elif builder == "pop_weapon_route_spool":
        cyl(col, "spool_outer", (0, 0, 0.54), (0.30, 0.30, 0.20), m["route_teal"], vertices=14, rot=(math.radians(90), 0, 0))
        cube(col, "spool_handle", (0.34, 0, 0.54), (0.12, 0.06, 0.46), dark)
        cube(col, "route_line", (-0.22, -0.34, 0.54), (0.56, 0.028, 0.034), signal, rot=(0, 0, math.radians(-12)))
    else:
        cube(col, "tool_blade_grip", (0, 0, 0.34), (0.07, 0.06, 0.42), dark)
        cube(col, "wide_blade", (0.10, -0.02, 0.74), (0.18, 0.055, 0.58), signal, rot=(0, 0, math.radians(-10)))
        cube(col, "blade_back", (-0.06, -0.01, 0.72), (0.10, 0.050, 0.50), m["metal"], rot=(0, 0, math.radians(-10)))


def build_block_combat_arena_2x2(col, m) -> None:
    cube(col, "arena_quadrant_safe_a", (-0.52, -0.52, 0.00), (0.52, 0.52, 0.08), m["moss"])
    cube(col, "arena_quadrant_safe_b", (-0.52, 0.52, 0.00), (0.52, 0.52, 0.08), m["hub_ivory"])
    cube(col, "arena_quadrant_threat_a", (0.52, -0.52, 0.00), (0.52, 0.52, 0.08), m["stone_warm"])
    cube(col, "arena_quadrant_reward_b", (0.52, 0.52, 0.00), (0.52, 0.52, 0.08), m["gold"])
    cube(col, "arena_cross_dark_x", (0, 0, 0.085), (1.16, 0.040, 0.035), m["stone_dark"])
    cube(col, "arena_cross_dark_y", (0, 0, 0.090), (0.040, 1.16, 0.035), m["stone_dark"])
    cyl(col, "center_tool_socket", (0, 0, 0.135), (0.22, 0.22, 0.040), m["tool_signal"], vertices=16)
    for i, (x, y, mat) in enumerate(((-0.82, -0.82, m["tool_signal"]), (0.82, -0.82, m["danger_red"]), (-0.82, 0.82, m["relic_blue"]), (0.82, 0.82, m["flower_yellow"]))):
        cube(col, f"corner_read_tab_{i+1}", (x, y, 0.145), (0.22, 0.055, 0.034), mat, rot=(0, 0, math.radians(45 if x * y > 0 else -45)))


def build_service_pad(col, m, builder: str) -> None:
    if builder == "checkpoint_socket_pad":
        base = m["tool_blue"]
        signal = m["tool_signal"]
    else:
        base = m["gold"]
        signal = m["relic_blue"]
    dark = m["stone_dark"]
    cyl(col, "pad_outer_disc", (0, 0, 0.04), (0.52, 0.52, 0.08), base, vertices=24)
    cyl(col, "pad_inner_socket", (0, 0, 0.105), (0.30, 0.30, 0.038), dark, vertices=16)
    cube(col, "pad_split_a", (0, 0, 0.150), (0.74, 0.040, 0.035), signal, rot=(0, 0, math.radians(8)))
    cube(col, "pad_split_b", (0, 0, 0.155), (0.040, 0.74, 0.035), signal, rot=(0, 0, math.radians(-8)))
    sphere(col, "pad_read_core", (0, -0.04, 0.34), (0.15, 0.15, 0.15), signal)


def build_melee_enemy(col, m) -> None:
    dark = m["enemy_ink"]
    signal = m["danger_red"]
    cube(col, "front_danger_footprint", (0.02, -0.48, 0.030), (0.58, 0.22, 0.035), signal, rot=(0, 0, math.radians(-3)))
    cube(col, "left_shadow_foot", (-0.25, -0.30, 0.15), (0.22, 0.24, 0.13), dark, rot=(0, 0, math.radians(-12)))
    cube(col, "right_shadow_foot", (0.22, -0.28, 0.15), (0.22, 0.24, 0.13), dark, rot=(0, 0, math.radians(10)))

    tapered_box(col, "broken_relic_body", (0, -0.02, 0.72), (0.52, 0.40), (0.38, 0.30), 0.48, dark, rot=(0, 0, math.radians(2)))
    cube(col, "front_split_socket", (0.00, -0.39, 0.82), (0.46, 0.052, 0.10), signal, rot=(0, 0, math.radians(-4)))
    cube(col, "vertical_wound_thread", (-0.12, -0.41, 0.96), (0.060, 0.040, 0.36), signal, rot=(0, 0, math.radians(8)))
    cube(col, "dark_brow_plate", (0.06, -0.43, 1.15), (0.42, 0.060, 0.10), dark, rot=(0, 0, math.radians(3)))

    tapered_box(col, "left_folded_shoulder", (-0.48, -0.02, 0.92), (0.20, 0.25), (0.14, 0.19), 0.12, dark, rot=(0, 0, math.radians(18)))
    tapered_box(col, "right_attack_shoulder", (0.47, -0.06, 0.90), (0.22, 0.25), (0.16, 0.18), 0.12, dark, rot=(0, 0, math.radians(-16)))
    cube(col, "back_left_shard", (-0.28, 0.30, 1.08), (0.09, 0.09, 0.40), dark, rot=(math.radians(-16), 0, math.radians(-13)))
    cube(col, "back_right_shard", (0.12, 0.34, 1.14), (0.09, 0.09, 0.44), dark, rot=(math.radians(-14), 0, math.radians(9)))

    cube(col, "left_guard_claw", (-0.42, -0.42, 0.62), (0.10, 0.08, 0.36), dark, rot=(math.radians(18), 0, math.radians(24)))
    cube(col, "attack_arm_spine", (0.64, -0.42, 0.78), (0.10, 0.10, 0.76), dark, rot=(math.radians(28), 0, math.radians(-36)))
    cube(col, "attack_edge_read", (0.86, -0.66, 0.98), (0.090, 0.060, 0.52), signal, rot=(math.radians(28), 0, math.radians(-36)))
    cube(col, "attack_tip_chip", (1.02, -0.82, 1.16), (0.14, 0.064, 0.12), signal, rot=(math.radians(28), 0, math.radians(-36)))
    cube(col, "attack_origin_band", (0.46, -0.38, 0.70), (0.28, 0.054, 0.066), signal, rot=(0, 0, math.radians(-21)))


def build_ranged_enemy(col, m) -> None:
    cyl(col, "tripod_body", (0, 0, 0.48), (0.36, 0.36, 0.65), m["enemy_ink"], vertices=7)
    for i, angle in enumerate((35, 155, 270)):
        rad = math.radians(angle)
        cube(col, f"tripod_foot_{i+1}", (math.cos(rad) * 0.34, math.sin(rad) * 0.34, 0.12), (0.18, 0.07, 0.07), m["enemy_armor"], rot=(0, 0, rad))
    sphere(col, "head_orb", (0, -0.08, 1.07), (0.24, 0.22, 0.22), m["enemy_armor"])
    cube(col, "back_mantle", (0, 0.24, 0.78), (0.62, 0.08, 0.62), m["enemy_armor"])
    cube(col, "mantle_edge", (0, 0.30, 0.92), (0.66, 0.035, 0.08), m["danger_red"])
    cube(col, "staff", (0.48, -0.06, 0.94), (0.07, 0.07, 0.98), m["danger_red"], rot=(0, 0, math.radians(-12)))
    cube(col, "staff_binding_low", (0.44, -0.05, 0.70), (0.18, 0.04, 0.05), m["gold"], rot=(0, 0, math.radians(-12)))
    cube(col, "staff_binding_high", (0.52, -0.05, 1.16), (0.18, 0.04, 0.05), m["gold"], rot=(0, 0, math.radians(-12)))
    sphere(col, "staff_orb", (0.56, -0.06, 1.48), (0.16, 0.16, 0.16), m["danger_red"])
    cube(col, "aim_read_bar", (0, -0.72, 0.14), (1.22, 0.055, 0.045), m["danger_red"])


def build_miniboss(col, m) -> None:
    cyl(col, "danger_base", (0, 0, 0.025), (0.92, 0.78, 0.035), m["danger_red"], vertices=24)
    sphere(col, "root_body", (0, 0, 1.02), (0.72, 0.54, 0.72), m["root"])
    cube(col, "root_band_low", (0, -0.52, 0.82), (0.78, 0.06, 0.08), m["danger_red"])
    cube(col, "root_band_high", (0, -0.50, 1.18), (0.70, 0.05, 0.07), m["danger_red"])
    cube(col, "stone_mask", (0, -0.48, 1.38), (0.54, 0.12, 0.34), m["stone_warm"])
    cube(col, "mask_brow", (0, -0.57, 1.55), (0.62, 0.05, 0.08), m["stone_dark"])
    sphere(col, "core_tell", (0, -0.58, 1.42), (0.20, 0.08, 0.20), m["danger_red"])
    cube(col, "left_arm", (-0.78, -0.12, 0.92), (0.22, 0.22, 0.92), m["root"], rot=(0, math.radians(20), math.radians(-28)))
    cube(col, "right_arm", (0.78, -0.12, 0.92), (0.22, 0.22, 0.92), m["root"], rot=(0, math.radians(-20), math.radians(28)))
    cube(col, "slam_club", (0.92, -0.48, 0.78), (0.18, 0.18, 1.24), m["stone_dark"], rot=(math.radians(36), 0, math.radians(-34)))
    sphere(col, "slam_tip", (1.18, -0.82, 1.24), (0.24, 0.24, 0.24), m["danger_red"])


def build_boss(col, m, builder: str) -> None:
    if builder == "boss_root":
        cyl(col, "arena_read", (0, 0, 0.02), (1.42, 1.16, 0.035), m["danger_red"], vertices=28)
        sphere(col, "trunk_mass", (0, 0, 1.24), (0.86, 0.62, 0.95), m["root"])
        cube(col, "stone_face", (0, -0.62, 1.62), (0.68, 0.12, 0.48), m["stone_warm"])
        sphere(col, "tool_opening_core", (0, -0.72, 1.62), (0.22, 0.08, 0.22), m["tool_signal"])
        cube(col, "left_root_arm", (-1.04, -0.12, 1.16), (0.28, 0.28, 1.36), m["root"], rot=(0, math.radians(22), math.radians(-38)))
        cube(col, "right_root_arm", (1.04, -0.12, 1.16), (0.28, 0.28, 1.36), m["root"], rot=(0, math.radians(-22), math.radians(38)))
        cube(col, "back_crown_roots", (0, 0.54, 2.18), (1.16, 0.20, 0.44), m["root"])
    elif builder == "boss_furnace":
        cyl(col, "heat_base", (0, 0, 0.02), (1.40, 1.05, 0.04), m["r02_amber"], vertices=28)
        cube(col, "furnace_body", (0, 0, 1.18), (1.15, 0.82, 1.45), m["r02_charcoal"])
        sphere(col, "amber_core", (0, -0.52, 1.38), (0.32, 0.12, 0.32), m["r02_amber"])
        cube(col, "left_plate", (-0.86, -0.02, 1.42), (0.34, 0.72, 0.54), m["metal"], rot=(0, 0, math.radians(-12)))
        cube(col, "right_plate", (0.86, -0.02, 1.42), (0.34, 0.72, 0.54), m["metal"], rot=(0, 0, math.radians(12)))
        cube(col, "left_hammer", (-1.20, -0.42, 0.94), (0.24, 0.24, 1.24), m["r02_rust"], rot=(math.radians(28), 0, math.radians(36)))
        cube(col, "right_hammer", (1.20, -0.42, 0.94), (0.24, 0.24, 1.24), m["r02_rust"], rot=(math.radians(28), 0, math.radians(-36)))
    elif builder == "boss_glass":
        cyl(col, "cold_base", (0, 0, 0.02), (1.25, 1.25, 0.035), m["r03_violet"], vertices=28)
        cone(col, "main_crystal", (0, 0, 1.42), (0.78, 0.78, 2.3), m["r03_crystal"], vertices=6)
        sphere(col, "front_core", (0, -0.45, 1.35), (0.28, 0.10, 0.28), m["white_glow"])
        cone(col, "left_shard", (-0.82, 0.05, 1.16), (0.28, 0.28, 1.35), m["r03_violet"], vertices=5, rot=(0, math.radians(-18), math.radians(-18)))
        cone(col, "right_shard", (0.82, 0.05, 1.16), (0.28, 0.28, 1.35), m["r03_violet"], vertices=5, rot=(0, math.radians(18), math.radians(18)))
        cube(col, "front_danger_blade", (0, -0.86, 1.02), (0.18, 0.12, 1.18), m["danger_red"], rot=(math.radians(62), 0, 0))
    else:
        cyl(col, "fourfold_base", (0, 0, 0.02), (1.55, 1.55, 0.04), m["tool_blue"], vertices=32)
        sphere(col, "central_core", (0, 0, 1.36), (0.58, 0.58, 0.58), m["r03_dark"])
        sphere(col, "front_weak_read", (0, -0.50, 1.36), (0.22, 0.10, 0.22), m["tool_signal"])
        for i, angle in enumerate((45, 135, 225, 315)):
            rad = math.radians(angle)
            x, y = math.cos(rad) * 1.02, math.sin(rad) * 1.02
            cube(col, f"crown_anchor_{i+1}", (x, y, 1.58), (0.22, 0.22, 0.96), m["gold"], rot=(0, 0, rad))
            sphere(col, f"anchor_glow_{i+1}", (x, y, 2.12), (0.18, 0.18, 0.18), m["danger_red"])
        cyl(col, "crown_ring", (0, 0, 2.05), (1.18, 1.18, 0.05), m["gold"], vertices=32)


def build_pedestal(col, m) -> None:
    cyl(col, "idle_footprint", (0, 0, 0.02), (0.52, 0.52, 0.035), m["tool_signal"], vertices=20)
    cube(col, "stone_base", (0, 0, 0.24), (0.56, 0.56, 0.40), m["stone_dark"])
    cube(col, "split_ring_a", (0.28, 0, 0.50), (0.08, 0.50, 0.06), m["gold"])
    cube(col, "split_ring_b", (-0.28, 0, 0.50), (0.08, 0.50, 0.06), m["gold"])
    cube(col, "socket_plate", (0, -0.30, 0.72), (0.42, 0.04, 0.10), m["stone_warm"])
    cube(col, "signal_bar_a", (-0.12, -0.04, 0.68), (0.08, 0.08, 0.56), m["tool_signal"], rot=(0, 0, math.radians(44)))
    cube(col, "signal_bar_b", (0.12, -0.04, 0.68), (0.08, 0.08, 0.56), m["tool_signal"], rot=(0, 0, math.radians(-44)))
    sphere(col, "active_read", (0, -0.02, 1.02), (0.18, 0.18, 0.18), m["relic_blue"])
    sphere(col, "solved_spark", (0.28, -0.22, 1.22), (0.10, 0.10, 0.10), m["tool_signal"])


def build_shortcut_bridge(col, m) -> None:
    body = m["stone_warm"]
    underside = m["stone_dark"]
    hinge = m["gold"]
    signal = m["route_teal"]
    leaf = [
        (-0.58, -0.25), (-0.22, -0.35), (0.36, -0.28), (0.66, -0.08),
        (0.55, 0.21), (0.10, 0.34), (-0.50, 0.23), (-0.72, 0.01),
    ]
    small_leaf = [
        (-0.42, -0.22), (-0.10, -0.31), (0.38, -0.20), (0.53, 0.04),
        (0.28, 0.27), (-0.26, 0.25), (-0.52, 0.05),
    ]
    under_leaf = [
        (-0.66, -0.30), (-0.22, -0.42), (0.48, -0.32), (0.78, -0.10),
        (0.68, 0.28), (0.12, 0.42), (-0.60, 0.30), (-0.84, 0.02),
    ]

    for i, (x, y, angle) in enumerate(((-0.86, -0.07, -7), (0.00, 0.08, 4), (0.88, -0.02, 8))):
        polygon_prism(col, f"dark_underside_leaf_{i+1}", (x, y, 0.065), under_leaf, 0.08, underside, rot=(0, 0, math.radians(angle)))

    for i, (x, y, angle, points) in enumerate((
        (-0.92, -0.08, -7, leaf),
        (-0.28, 0.08, 5, small_leaf),
        (0.36, -0.03, -3, small_leaf),
        (0.98, 0.04, 8, leaf),
    )):
        polygon_prism(col, f"folded_shell_leaf_{i+1}", (x, y, 0.155 + i * 0.006), points, 0.10, body, rot=(0, 0, math.radians(angle)))

    for i, (x, y, angle, length) in enumerate(((-0.78, -0.02, -5, 0.58), (-0.10, 0.04, 3, 0.54), (0.56, 0.00, 2, 0.58))):
        cube(col, f"recessed_channel_shadow_{i+1}", (x, y, 0.225), (length, 0.060, 0.025), underside, rot=(0, 0, math.radians(angle)), bevel=0.012)
        cube(col, f"teal_enamel_groove_{i+1}", (x, y, 0.250), (length * 0.88, 0.032, 0.018), signal, rot=(0, 0, math.radians(angle)), bevel=0.010)

    for i, (x, y) in enumerate(((-1.45, -0.03), (1.45, 0.03))):
        cyl(col, f"brass_connection_socket_{i+1}", (x, y, 0.245), (0.22, 0.18, 0.055), hinge, vertices=16, bevel=True)
        cyl(col, f"teal_socket_core_{i+1}", (x, y, 0.310), (0.115, 0.090, 0.025), signal, vertices=14, bevel=True)
        cube(col, f"clasp_tooth_upper_{i+1}", (x, y + 0.24, 0.225), (0.18, 0.055, 0.060), hinge, rot=(0, 0, math.radians(8 if x < 0 else -8)))
        cube(col, f"clasp_tooth_lower_{i+1}", (x, y - 0.24, 0.225), (0.18, 0.055, 0.060), hinge, rot=(0, 0, math.radians(-8 if x < 0 else 8)))

    for i, (x, y, angle, length) in enumerate((
        (-1.05, -0.34, -10, 0.18),
        (-0.58, -0.39, -4, 0.14),
        (-0.08, -0.30, 11, 0.16),
        (0.48, 0.32, -8, 0.14),
        (0.98, 0.35, 12, 0.18),
    )):
        cube(col, f"split_hinge_spine_{i+1}", (x, y, 0.212), (length, 0.038, 0.052), hinge, rot=(0, 0, math.radians(angle)), bevel=0.012)

    chip_points = [(-0.18, -0.09), (0.10, -0.12), (0.21, 0.02), (0.07, 0.14), (-0.17, 0.10), (-0.24, -0.02)]
    for i, (x, y, angle, mat) in enumerate((
        (0.96, -0.34, 17, body),
        (1.17, -0.26, -10, underside),
        (0.74, -0.38, -22, hinge),
    )):
        polygon_prism(col, f"localized_broken_cluster_{i+1}", (x, y, 0.185 + i * 0.012), chip_points, 0.055, mat, rot=(0, 0, math.radians(angle)), bevel=0.012)

    cube(col, "exposed_underside_shadow", (0.72, -0.42, 0.090), (0.62, 0.060, 0.060), underside, rot=(0, 0, math.radians(-6)), bevel=0.012)


def build_root_gate(col, m) -> None:
    cube(col, "left_stone", (-0.58, 0, 0.44), (0.24, 0.28, 0.86), m["stone_dark"])
    cube(col, "right_stone", (0.58, 0, 0.44), (0.24, 0.28, 0.86), m["stone_dark"])
    cube(col, "root_bar_low", (0, 0, 0.42), (1.15, 0.18, 0.16), m["root"], rot=(0, 0, math.radians(8)))
    cube(col, "root_bar_high", (0, 0, 0.78), (1.15, 0.18, 0.16), m["root"], rot=(0, 0, math.radians(-10)))
    sphere(col, "tool_lock_core", (0, -0.12, 0.82), (0.16, 0.08, 0.16), m["tool_signal"])


def build_chest(col, m) -> None:
    cube(col, "chest_base", (0, 0, 0.28), (0.76, 0.48, 0.46), m["wood"])
    cube(col, "side_plate_left", (-0.42, -0.02, 0.34), (0.05, 0.42, 0.30), m["stone_dark"])
    cube(col, "side_plate_right", (0.42, -0.02, 0.34), (0.05, 0.42, 0.30), m["stone_dark"])
    cube(col, "gold_lid_closed", (0, 0, 0.58), (0.82, 0.52, 0.16), m["gold"])
    cube(col, "front_latch", (0, -0.30, 0.50), (0.18, 0.045, 0.18), m["relic_blue"])
    cube(col, "hinge_left", (-0.30, 0.29, 0.62), (0.12, 0.05, 0.12), m["stone_dark"])
    cube(col, "hinge_right", (0.30, 0.29, 0.62), (0.12, 0.05, 0.12), m["stone_dark"])
    cube(col, "open_lid_read", (0, 0.26, 0.82), (0.82, 0.12, 0.42), m["gold"], rot=(math.radians(-34), 0, 0))
    sphere(col, "visible_relic", (0, -0.06, 0.92), (0.18, 0.18, 0.24), m["relic_blue"])
    cyl(col, "reward_beam", (0, 0, 1.35), (0.06, 0.06, 0.65), m["relic_blue"], vertices=12)


def build_relic(col, m, builder: str) -> None:
    if builder == "relic_seed":
        sphere(col, "ember_seed_core", (0, 0, 0.34), (0.22, 0.18, 0.30), m["ember"])
        cone(col, "leaf_cap", (0, 0.02, 0.64), (0.18, 0.18, 0.24), m["leaf"], vertices=5)
        cyl(col, "pickup_disc", (0, 0, 0.02), (0.36, 0.36, 0.03), m["gold"], vertices=18)
    else:
        cyl(col, "sigil_ring", (0, 0, 0.32), (0.32, 0.32, 0.08), m["relic_blue"], vertices=22)
        cube(col, "sigil_cross_a", (0, 0, 0.34), (0.58, 0.08, 0.08), m["tool_signal"])
        cube(col, "sigil_cross_b", (0, 0, 0.35), (0.08, 0.58, 0.08), m["tool_signal"])
        sphere(col, "center_gem", (0, 0, 0.46), (0.12, 0.12, 0.12), m["relic_blue"])


def build_modular_prop(col, m, spec: AssetSpec) -> None:
    style = spec.style
    builder = spec.builder
    mats = style_materials(m, style)
    if builder == "floor_tile":
        cube(col, "floor_base", (0, 0, -0.03), (1.0, 1.0, 0.08), mats["base"])
        cube(col, "edge_a", (0, -0.48, 0.03), (0.95, 0.05, 0.035), mats["accent"])
        cube(col, "edge_b", (-0.48, 0, 0.035), (0.05, 0.95, 0.035), mats["accent"])
        add_style_marks(col, m, style)
    elif builder in ("mosaic_tile", "boss_arena_floor"):
        cube(col, "floor_base", (0, 0, -0.03), (1.2, 1.2, 0.08), mats["base"])
        cyl(col, "center_disc", (0, 0, 0.035), (0.42, 0.42, 0.025), mats["signal"], vertices=28)
        cube(col, "axis_mark_a", (0, 0, 0.07), (1.0, 0.07, 0.035), mats["accent"])
        cube(col, "axis_mark_b", (0, 0, 0.075), (0.07, 1.0, 0.035), mats["accent"])
    elif builder == "broken_floor":
        cube(col, "floor_piece_a", (-0.24, 0.04, -0.02), (0.56, 0.92, 0.08), mats["base"], rot=(0, 0, math.radians(3)))
        cube(col, "floor_piece_b", (0.36, -0.12, -0.01), (0.42, 0.72, 0.08), mats["base"], rot=(0, 0, math.radians(-6)))
        cube(col, "crack_read", (0.03, -0.02, 0.05), (0.08, 0.92, 0.04), mats["dark"], rot=(0, 0, math.radians(-18)))
    elif builder == "low_wall":
        cube(col, "wall_base", (0, 0, 0.28), (1.25, 0.24, 0.48), mats["dark"])
        cube(col, "wall_cap", (0, 0, 0.56), (1.34, 0.30, 0.12), mats["base"])
        if style == "r01":
            cube(col, "root_wrap", (0.1, -0.08, 0.52), (1.08, 0.08, 0.10), m["root"], rot=(0, 0, math.radians(8)))
        elif style == "r03":
            cone(col, "crystal_cap", (0.36, -0.04, 0.82), (0.14, 0.14, 0.42), m["r03_crystal"], vertices=5)
    elif builder == "route_edge":
        cube(col, "route_edge_body", (0, 0, 0.06), (1.2, 0.18, 0.12), mats["accent"], rot=(0, 0, math.radians(5)))
        cube(col, "route_signal", (0.1, 0, 0.16), (0.84, 0.07, 0.05), mats["signal"], rot=(0, 0, math.radians(5)))
    elif builder == "rubble":
        for i, (x, y, s) in enumerate(((-0.32, 0.08, 0.28), (0.08, -0.10, 0.22), (0.36, 0.16, 0.18), (-0.02, 0.28, 0.14))):
            cube(col, f"chunk_{i+1}", (x, y, s * 0.5), (s, s * 0.75, s), mats["base"], rot=(0, 0, math.radians(12 * i)))
    elif builder == "flower_clump":
        for i, angle in enumerate((0, 72, 144, 216, 288)):
            rad = math.radians(angle)
            cube(col, f"leaf_{i+1}", (math.cos(rad) * 0.20, math.sin(rad) * 0.20, 0.16), (0.08, 0.28, 0.08), m["leaf"], rot=(0, 0, rad))
            sphere(col, f"flower_{i+1}", (math.cos(rad) * 0.28, math.sin(rad) * 0.28, 0.34), (0.09, 0.09, 0.09), m["flower_yellow"])
    elif builder == "root_arch":
        cube(col, "left_root", (-0.52, 0, 0.70), (0.20, 0.20, 1.35), m["root"], rot=(0, math.radians(-12), 0))
        cube(col, "right_root", (0.52, 0, 0.70), (0.20, 0.20, 1.35), m["root"], rot=(0, math.radians(12), 0))
        cube(col, "top_root", (0, 0, 1.38), (1.25, 0.22, 0.20), m["root"], rot=(0, 0, math.radians(4)))
        sphere(col, "signal_knot", (0, -0.12, 1.28), (0.15, 0.08, 0.15), m["tool_signal"])
    elif builder in ("tool_mirror", "tool_relay", "amber_relay"):
        cube(col, "base", (0, 0, 0.22), (0.42, 0.42, 0.36), mats["dark"])
        cube(col, "upright", (0, 0, 0.78), (0.22, 0.12, 0.92), mats["base"])
        sphere(col, "receiver_core", (0, -0.08, 1.18), (0.18, 0.08, 0.18), mats["signal"])
        cyl(col, "receiver_disc", (0, -0.10, 1.18), (0.30, 0.30, 0.04), mats["signal"], vertices=20, rot=(math.radians(90), 0, 0))
    elif builder == "reveal_marker":
        cyl(col, "hidden_disc", (0, 0, 0.02), (0.38, 0.38, 0.025), mats["dark"], vertices=18)
        cube(col, "marker_stem", (0, 0, 0.42), (0.14, 0.14, 0.72), mats["base"])
        sphere(col, "marker_glow", (0, -0.02, 0.86), (0.14, 0.14, 0.14), mats["signal"])
    elif builder == "sealed_blocker":
        cube(col, "blocker_body", (0, 0, 0.36), (0.92, 0.32, 0.68), mats["dark"])
        cube(col, "signal_lock", (0, -0.18, 0.54), (0.42, 0.06, 0.12), mats["signal"])
    elif builder == "vine_rail":
        cube(col, "rail_root_a", (0, -0.08, 0.30), (1.2, 0.10, 0.14), m["root"], rot=(0, 0, math.radians(5)))
        cube(col, "rail_root_b", (0, 0.08, 0.48), (1.2, 0.10, 0.14), m["root"], rot=(0, 0, math.radians(-5)))
        sphere(col, "leaf_read", (0.44, -0.08, 0.58), (0.10, 0.08, 0.08), m["leaf"])
    elif builder in ("moss_pillar", "obelisk", "bridge_post"):
        cube(col, "pillar_body", (0, 0, 0.72), (0.38, 0.38, 1.32), mats["base"])
        cube(col, "pillar_cap", (0, 0, 1.42), (0.48, 0.48, 0.14), mats["accent"])
        sphere(col, "small_signal", (0, -0.20, 1.08), (0.10, 0.06, 0.10), mats["signal"])
    elif builder in ("step_stone", "short_stair"):
        for i in range(3):
            cube(col, f"step_{i+1}", (0, -0.20 + i * 0.20, 0.07 + i * 0.10), (0.82, 0.22, 0.10), mats["base"])
    elif builder == "short_grass":
        for i, x in enumerate((-0.32, -0.12, 0.10, 0.30)):
            cube(col, f"grass_blade_{i+1}", (x, 0, 0.22), (0.08, 0.12, 0.42), m["leaf"], rot=(math.radians(10 + i * 4), 0, math.radians(-10 + i * 7)))
    elif builder == "signal_tile":
        cube(col, "tile_base", (0, 0, 0.02), (0.78, 0.78, 0.06), mats["base"])
        cyl(col, "tool_disc", (0, 0, 0.075), (0.28, 0.28, 0.025), mats["signal"], vertices=18)
    elif builder == "return_point":
        cyl(col, "return_base", (0, 0, 0.05), (0.50, 0.50, 0.10), mats["base"], vertices=24)
        cube(col, "return_pillar", (0, 0, 0.62), (0.28, 0.28, 0.96), mats["accent"])
        sphere(col, "return_glow", (0, 0, 1.18), (0.22, 0.22, 0.22), m["tool_blue"])
    elif builder == "region_gate":
        cube(col, "left_post", (-0.55, 0, 0.72), (0.24, 0.32, 1.32), mats["base"])
        cube(col, "right_post", (0.55, 0, 0.72), (0.24, 0.32, 1.32), mats["base"])
        cube(col, "top_lintel", (0, 0, 1.42), (1.35, 0.34, 0.20), mats["accent"])
        sphere(col, "gate_signal", (0, -0.18, 1.12), (0.16, 0.08, 0.16), mats["signal"])
    elif builder == "light_anchor":
        cube(col, "light_stand", (0, 0, 0.60), (0.22, 0.22, 1.02), mats["base"])
        sphere(col, "light_orb", (0, 0, 1.22), (0.20, 0.20, 0.20), mats["signal"])
        cyl(col, "light_foot", (0, 0, 0.04), (0.36, 0.36, 0.08), mats["dark"], vertices=18)
    elif builder == "save_stone":
        cube(col, "save_stone", (0, 0, 0.58), (0.44, 0.34, 1.02), mats["base"], rot=(0, 0, math.radians(7)))
        sphere(col, "save_core", (0, -0.18, 0.80), (0.12, 0.06, 0.12), m["tool_blue"])
    elif builder == "bench":
        cube(col, "bench_seat", (0, 0, 0.34), (0.92, 0.24, 0.12), mats["base"])
        cube(col, "bench_left_leg", (-0.34, 0, 0.18), (0.14, 0.18, 0.28), mats["dark"])
        cube(col, "bench_right_leg", (0.34, 0, 0.18), (0.14, 0.18, 0.28), mats["dark"])
    elif builder == "planter":
        cube(col, "planter_box", (0, 0, 0.22), (0.68, 0.42, 0.28), mats["base"])
        for i, x in enumerate((-0.22, 0, 0.22)):
            sphere(col, f"plant_blob_{i+1}", (x, 0, 0.48), (0.14, 0.12, 0.12), m["leaf"])
    elif builder == "wayline":
        cube(col, "wayline", (0, 0, 0.025), (1.0, 0.10, 0.04), mats["signal"], rot=(0, 0, math.radians(10)))
    elif builder == "angled_cliff":
        cube(col, "cliff_body", (0, 0, 0.42), (1.2, 0.48, 0.80), mats["dark"], rot=(0, 0, math.radians(-8)))
        cube(col, "cliff_face", (0.12, -0.20, 0.54), (0.92, 0.08, 0.54), mats["accent"], rot=(0, 0, math.radians(-8)))
    elif builder == "metal_frame":
        cube(col, "frame_top", (0, 0, 0.80), (1.1, 0.12, 0.14), m["metal"])
        cube(col, "frame_left", (-0.48, 0, 0.42), (0.12, 0.12, 0.72), m["metal"])
        cube(col, "frame_right", (0.48, 0, 0.42), (0.12, 0.12, 0.72), m["metal"])
        sphere(col, "amber_bolt", (0, -0.08, 0.76), (0.10, 0.06, 0.10), m["r02_amber"])
    elif builder == "rust_pipe":
        cyl(col, "pipe_body", (0, 0, 0.38), (0.16, 0.16, 1.1), m["r02_rust"], vertices=12, rot=(0, math.radians(90), 0))
        cyl(col, "pipe_cap_a", (-0.58, 0, 0.38), (0.20, 0.20, 0.08), m["metal"], vertices=12, rot=(0, math.radians(90), 0))
        cyl(col, "pipe_cap_b", (0.58, 0, 0.38), (0.20, 0.20, 0.08), m["metal"], vertices=12, rot=(0, math.radians(90), 0))
    elif builder == "broken_gear":
        cyl(col, "gear_ring", (0, 0, 0.16), (0.46, 0.46, 0.12), m["metal"], vertices=14)
        for i, angle in enumerate((0, 60, 120, 210, 285)):
            rad = math.radians(angle)
            cube(col, f"gear_tooth_{i+1}", (math.cos(rad) * 0.46, math.sin(rad) * 0.46, 0.18), (0.18, 0.12, 0.12), m["r02_rust"], rot=(0, 0, rad))
    elif builder == "heat_vent":
        cube(col, "vent_base", (0, 0, 0.08), (0.78, 0.58, 0.14), mats["dark"])
        for i, x in enumerate((-0.22, 0, 0.22)):
            cube(col, f"vent_slit_{i+1}", (x, -0.01, 0.18), (0.08, 0.50, 0.04), m["r02_amber"])
    elif builder in ("crystal_bridge", "narrow_bridge"):
        cube(col, "bridge_deck", (0, 0, 0.08), (1.35, 0.48, 0.12), mats["base"])
        cube(col, "left_glow_edge", (0, -0.27, 0.18), (1.24, 0.06, 0.06), mats["signal"])
        cube(col, "right_glow_edge", (0, 0.27, 0.18), (1.24, 0.06, 0.06), mats["signal"])
    elif builder == "crystal_cluster":
        for i, (x, y, h) in enumerate(((-0.24, 0.02, 1.0), (0.08, -0.04, 1.35), (0.34, 0.12, 0.82))):
            cone(col, f"crystal_{i+1}", (x, y, h * 0.45), (0.18, 0.18, h), m["r03_crystal"], vertices=5, rot=(0, math.radians(8 * (i - 1)), 0))
    elif builder == "boss_boundary":
        cube(col, "boundary_body", (0, 0, 0.24), (1.2, 0.22, 0.40), mats["dark"])
        cube(col, "danger_trim", (0, -0.12, 0.48), (1.0, 0.06, 0.06), m["danger_red"])
    elif builder == "boss_spawn_anchor":
        cyl(col, "spawn_base", (0, 0, 0.04), (0.48, 0.48, 0.08), mats["base"], vertices=20)
        cube(col, "spawn_upright", (0, 0, 0.56), (0.26, 0.26, 0.92), mats["accent"])
        sphere(col, "spawn_core", (0, -0.12, 0.98), (0.14, 0.08, 0.14), m["danger_red"])
    elif builder == "boss_danger_decal":
        cyl(col, "danger_disc", (0, 0, 0.02), (0.52, 0.52, 0.025), m["danger_red"], vertices=28)
        cube(col, "danger_arrow", (0, -0.22, 0.05), (0.20, 0.46, 0.03), m["gold"], rot=(0, 0, math.radians(45)))
    elif builder == "boss_reward_exit":
        cube(col, "exit_left", (-0.42, 0, 0.62), (0.22, 0.28, 1.12), mats["base"])
        cube(col, "exit_right", (0.42, 0, 0.62), (0.22, 0.28, 1.12), mats["base"])
        cube(col, "exit_top", (0, 0, 1.26), (1.08, 0.30, 0.18), mats["accent"])
        sphere(col, "reward_exit_glow", (0, -0.16, 1.0), (0.16, 0.08, 0.16), m["relic_blue"])
    elif builder.startswith("detail_"):
        build_detail_prop(col, m, builder, mats, style)
    else:
        cube(col, "simple_body", (0, 0, 0.28), (0.6, 0.6, 0.5), mats["base"])


def build_detail_prop(col, m, builder: str, mats: dict[str, bpy.types.Material], style: str) -> None:
    base = mats["base"]
    dark = mats["dark"]
    accent = mats["accent"]
    signal = mats["signal"]

    if builder == "detail_quarter_inlay":
        cyl(col, "low_disc", (0, 0, 0.018), (0.34, 0.34, 0.024), dark, vertices=20)
        cube(col, "split_a", (0, 0, 0.045), (0.50, 0.035, 0.018), accent)
        cube(col, "split_b", (0, 0, 0.05), (0.035, 0.50, 0.018), signal)
        cube(col, "offset_tick", (0.20, -0.20, 0.06), (0.15, 0.035, 0.018), accent, rot=(0, 0, math.radians(28)))
    elif builder == "detail_pebble_line":
        for i, (x, y, s) in enumerate(((-0.36, -0.04, 0.09), (-0.18, 0.08, 0.07), (0.02, -0.02, 0.08), (0.20, 0.10, 0.06), (0.38, -0.05, 0.08))):
            cube(col, f"pebble_{i+1}", (x, y, s * 0.5), (s, s * 0.65, s * 0.45), accent if i % 2 else base, rot=(0, 0, math.radians(i * 17)))
    elif builder == "detail_color_chip":
        cube(col, "chip_body", (0, 0, 0.035), (0.34, 0.22, 0.035), dark, rot=(0, 0, math.radians(-8)))
        cube(col, "chip_face", (0.04, -0.02, 0.08), (0.23, 0.12, 0.022), signal, rot=(0, 0, math.radians(-8)))
    elif builder == "detail_split_curb":
        cube(col, "curb_a", (-0.22, 0.03, 0.10), (0.30, 0.11, 0.10), base, rot=(0, 0, math.radians(4)))
        cube(col, "curb_b", (0.25, -0.04, 0.11), (0.28, 0.11, 0.11), accent, rot=(0, 0, math.radians(-6)))
        cube(col, "dark_gap", (0.03, 0, 0.12), (0.05, 0.12, 0.12), dark)
    elif builder == "detail_thread_lamp":
        cyl(col, "lamp_foot", (0, 0, 0.035), (0.18, 0.18, 0.07), dark, vertices=14)
        cube(col, "lamp_stem", (0, 0, 0.38), (0.06, 0.06, 0.58), base)
        cube(col, "lamp_hook", (0.11, -0.02, 0.72), (0.22, 0.04, 0.05), accent, rot=(0, 0, math.radians(-12)))
        sphere(col, "lamp_glow", (0.24, -0.05, 0.70), (0.12, 0.12, 0.12), signal)
    elif builder == "detail_step_chips":
        for i, (x, y) in enumerate(((-0.28, -0.10), (-0.02, 0.05), (0.25, -0.02))):
            cube(col, f"step_chip_{i+1}", (x, y, 0.05 + i * 0.01), (0.18, 0.09, 0.045), base if i != 1 else accent, rot=(0, 0, math.radians(i * 19 - 10)))
    elif builder == "detail_root_knot":
        sphere(col, "root_knot_core", (0, 0, 0.18), (0.20, 0.16, 0.16), m["root"])
        cube(col, "root_splay_a", (-0.22, -0.04, 0.12), (0.30, 0.05, 0.07), m["root"], rot=(0, 0, math.radians(14)))
        cube(col, "root_splay_b", (0.18, 0.10, 0.13), (0.28, 0.05, 0.07), m["root"], rot=(0, 0, math.radians(-22)))
        sphere(col, "small_leaf", (0.16, -0.12, 0.32), (0.08, 0.06, 0.05), m["leaf"])
    elif builder == "detail_leaf_sprig":
        for i, angle in enumerate((-50, -20, 12, 42, 72)):
            rad = math.radians(angle)
            cube(col, f"leaf_{i+1}", (math.cos(rad) * 0.13, math.sin(rad) * 0.13, 0.15 + i * 0.01), (0.045, 0.18, 0.035), signal if style == "r03" else m["leaf"], rot=(0, 0, rad))
        cube(col, "stem", (0, 0, 0.08), (0.05, 0.30, 0.035), m["root"] if style == "r01" else accent)
    elif builder == "detail_petal_patch":
        cyl(col, "leaf_base", (0, 0, 0.02), (0.26, 0.18, 0.026), m["leaf"], vertices=12)
        for i, angle in enumerate((0, 70, 145, 220, 290)):
            rad = math.radians(angle)
            sphere(col, f"petal_{i+1}", (math.cos(rad) * 0.20, math.sin(rad) * 0.14, 0.12), (0.055, 0.055, 0.045), m["flower_yellow"])
    elif builder == "detail_crack_inlay":
        cube(col, "crack_main", (0, 0, 0.026), (0.46, 0.035, 0.026), dark, rot=(0, 0, math.radians(-24)))
        cube(col, "crack_branch_a", (-0.12, 0.09, 0.028), (0.22, 0.030, 0.024), dark, rot=(0, 0, math.radians(32)))
        cube(col, "crack_branch_b", (0.18, -0.08, 0.030), (0.18, 0.030, 0.024), dark, rot=(0, 0, math.radians(38)))
        cube(col, "bright_chip", (0.28, 0.08, 0.045), (0.09, 0.05, 0.020), accent)
    elif builder == "detail_moss_curb":
        cube(col, "curb_stone", (0, 0, 0.08), (0.48, 0.12, 0.08), base, rot=(0, 0, math.radians(5)))
        cube(col, "moss_ribbon", (0.04, -0.09, 0.16), (0.42, 0.035, 0.035), m["moss"], rot=(0, 0, math.radians(5)))
    elif builder == "detail_half_tile":
        cube(col, "half_tile_a", (-0.12, 0.02, 0.04), (0.32, 0.24, 0.045), base, rot=(0, 0, math.radians(-8)))
        cube(col, "half_tile_b", (0.24, -0.10, 0.05), (0.16, 0.20, 0.045), accent, rot=(0, 0, math.radians(14)))
        cube(col, "buried_shadow", (0.04, 0.20, 0.035), (0.36, 0.04, 0.02), dark)
    elif builder == "detail_glow_bud":
        cyl(col, "bud_base", (0, 0, 0.04), (0.15, 0.15, 0.08), dark, vertices=10)
        cube(col, "bud_stem", (0, 0, 0.23), (0.05, 0.05, 0.30), accent)
        sphere(col, "bud_glow", (0, 0, 0.47), (0.12, 0.12, 0.12), signal)
    elif builder == "detail_fold_marker":
        cube(col, "fold_base", (-0.12, 0.02, 0.08), (0.24, 0.18, 0.08), base, rot=(0, 0, math.radians(-12)))
        cube(col, "fold_rise", (0.14, -0.04, 0.20), (0.22, 0.12, 0.10), accent, rot=(0, 0, math.radians(18)))
        cube(col, "fold_tick", (0.05, -0.18, 0.29), (0.10, 0.04, 0.05), signal)
    elif builder == "detail_shadow_patch":
        cyl(col, "soft_shadow", (0, 0, 0.012), (0.42, 0.24, 0.016), dark, vertices=24)
    elif builder == "detail_thread_line":
        cube(col, "thread_main", (0, 0, 0.025), (0.58, 0.025, 0.022), signal, rot=(0, 0, math.radians(-12)))
        cube(col, "thread_chip", (0.32, 0.02, 0.04), (0.10, 0.025, 0.020), signal, rot=(0, 0, math.radians(18)))
    elif builder == "detail_rivet_cluster":
        cube(col, "plate", (0, 0, 0.04), (0.36, 0.22, 0.04), dark)
        for i, (x, y) in enumerate(((-0.22, -0.10), (0, 0.02), (0.20, -0.08), (0.11, 0.13))):
            sphere(col, f"rivet_{i+1}", (x, y, 0.12), (0.055, 0.055, 0.035), signal)
    else:
        cube(col, "detail_body", (0, 0, 0.05), (0.32, 0.18, 0.05), base)


def style_materials(m, style: str) -> dict[str, bpy.types.Material]:
    if style == "hub":
        return {"base": m["hub_ivory"], "dark": m["stone_dark"], "accent": m["gold"], "signal": m["tool_blue"]}
    if style == "r01":
        return {"base": m["stone_warm"], "dark": m["stone_dark"], "accent": m["moss"], "signal": m["route_teal"]}
    if style == "r02":
        return {"base": m["r02_charcoal"], "dark": m["stone_dark"], "accent": m["r02_rust"], "signal": m["r02_amber"]}
    if style == "r03":
        return {"base": m["r03_dark"], "dark": m["stone_dark"], "accent": m["r03_violet"], "signal": m["r03_crystal"]}
    return {"base": m["stone_dark"], "dark": m["enemy_ink"], "accent": m["gold"], "signal": m["danger_red"]}


def consolidate_material_budget(spec: AssetSpec, col: bpy.types.Collection, m: dict[str, bpy.types.Material]) -> None:
    allowed = material_budget_roles(spec, m)
    allowed_names = {material.name for material in allowed.values()}
    for obj in col.objects:
        if obj.type != "MESH":
            continue
        current = obj.material_slots[0].material if obj.material_slots else None
        if current and current.name in allowed_names:
            replacement = current
        else:
            replacement = material_replacement(current.name if current else "", allowed)
        obj.data.materials.clear()
        obj.data.materials.append(replacement)


def material_budget_roles(spec: AssetSpec, m: dict[str, bpy.types.Material]) -> dict[str, bpy.types.Material]:
    if spec.category == "Character":
        return trim_roles({"base": m["stone_warm"], "dark": m["stone_dark"], "signal": m["tool_signal"]}, spec.material_budget)
    if spec.category == "Enemy":
        roles = {"base": m["enemy_ink"], "dark": m["stone_dark"], "signal": m["danger_red"], "accent": m["enemy_armor"]}
        return trim_roles(roles, spec.material_budget, preferred=("base", "signal", "dark", "accent"))
    if spec.category == "Boss":
        roles = {"base": m["stone_dark"], "dark": m["enemy_ink"], "signal": m["danger_red"], "accent": m["gold"]}
        return trim_roles(roles, spec.material_budget, preferred=("base", "dark", "signal", "accent"))
    if spec.builder == "exploration_tool":
        return trim_roles({"base": m["stone_dark"], "signal": m["tool_signal"]}, spec.material_budget)
    if spec.builder == "shortcut_bridge":
        roles = {"base": m["stone_warm"], "dark": m["stone_dark"], "accent": m["gold"], "signal": m["route_teal"]}
        return trim_roles(roles, spec.material_budget)
    if spec.builder == "chest":
        return trim_roles({"base": m["stone_dark"], "signal": m["relic_blue"]}, spec.material_budget)
    if spec.builder == "relic_seed":
        return {"signal": m["ember"]}
    if spec.builder == "relic_sigil":
        return {"signal": m["relic_blue"]}

    roles = style_materials(m, spec.style)
    if spec.material_budget <= 1:
        key = "signal" if is_signal_primary(spec) else "base"
        return {key: roles[key]}
    if spec.material_budget == 2:
        second = "signal" if is_signal_primary(spec) else "dark"
        return trim_roles({"base": roles["base"], second: roles[second]}, spec.material_budget)
    if spec.material_budget == 3:
        third = "signal" if is_signal_primary(spec) else "accent"
        return trim_roles({"base": roles["base"], "dark": roles["dark"], third: roles[third]}, spec.material_budget)
    return trim_roles(roles, spec.material_budget)


def trim_roles(roles: dict[str, bpy.types.Material], budget: int, preferred=("base", "dark", "accent", "signal")) -> dict[str, bpy.types.Material]:
    trimmed: dict[str, bpy.types.Material] = {}
    used = set()
    for role in preferred:
        material = roles.get(role)
        if material and material.name not in used:
            trimmed[role] = material
            used.add(material.name)
        if len(trimmed) >= budget:
            return trimmed
    for role, material in roles.items():
        if material.name not in used:
            trimmed[role] = material
            used.add(material.name)
        if len(trimmed) >= budget:
            return trimmed
    return trimmed


def is_signal_primary(spec: AssetSpec) -> bool:
    text = " ".join((spec.asset_id, spec.name, spec.gameplay_role, spec.builder)).lower()
    tokens = (
        "tool",
        "signal",
        "relay",
        "reward",
        "relic",
        "chest",
        "gate",
        "marker",
        "danger",
        "boss",
        "route",
        "wayline",
        "return",
        "save",
        "light",
        "lamp",
    )
    return any(token in text for token in tokens)


def material_replacement(material_name: str, allowed: dict[str, bpy.types.Material]) -> bpy.types.Material:
    semantic = material_semantic(material_name)
    if semantic in allowed:
        return allowed[semantic]
    if semantic == "signal":
        return allowed.get("accent") or allowed.get("dark") or next(iter(allowed.values()))
    if semantic == "accent":
        return allowed.get("signal") or allowed.get("dark") or allowed.get("base") or next(iter(allowed.values()))
    if semantic == "dark":
        return allowed.get("base") or allowed.get("accent") or next(iter(allowed.values()))
    return allowed.get("base") or allowed.get("dark") or next(iter(allowed.values()))


def material_semantic(material_name: str) -> str:
    name = material_name.lower()
    if any(token in name for token in ("signal", "tool", "glow", "danger", "red", "amber", "ember", "relic", "blue", "crystal", "violet", "white", "teal")):
        return "signal"
    if any(token in name for token in ("dark", "ink", "charcoal", "armor")):
        return "dark"
    if any(token in name for token in ("gold", "moss", "leaf", "flower", "rust", "metal", "wood", "root")):
        return "accent"
    return "base"


def add_style_marks(col, m, style: str) -> None:
    if style == "r01":
        sphere(col, "moss_read", (-0.32, 0.28, 0.08), (0.16, 0.12, 0.04), m["moss"])
        sphere(col, "flower_read", (0.34, -0.26, 0.10), (0.08, 0.08, 0.08), m["flower_yellow"])
    elif style == "r02":
        cube(col, "rust_plate", (0.24, -0.24, 0.08), (0.30, 0.18, 0.035), m["r02_rust"], rot=(0, 0, math.radians(8)))
    elif style == "r03":
        cone(col, "crystal_read", (0.30, -0.28, 0.18), (0.10, 0.10, 0.28), m["r03_crystal"], vertices=5)


def cube(col, name, loc, scale, material, rot=(0, 0, 0), bevel=0.025):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj["primitive_type"] = "cube"
    obj.scale = scale
    obj.data.materials.append(material)
    if bevel:
        modifier = obj.modifiers.new("low_poly_bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 1
    link_to_collection(obj, col)
    return obj


def cyl(col, name, loc, scale, material, vertices=12, rot=(0, 0, 0), bevel=False):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=1, depth=1, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj["primitive_type"] = "cylinder"
    obj.scale = scale
    obj.data.materials.append(material)
    if bevel:
        modifier = obj.modifiers.new("rim_bevel", "BEVEL")
        modifier.width = 0.015
        modifier.segments = 1
    link_to_collection(obj, col)
    return obj


def sphere(col, name, loc, scale, material):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=6, radius=1, location=loc)
    obj = bpy.context.object
    obj.name = name
    obj["primitive_type"] = "sphere"
    obj.scale = scale
    obj.data.materials.append(material)
    link_to_collection(obj, col)
    return obj


def cone(col, name, loc, scale, material, vertices=6, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=1, radius2=0, depth=1, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj["primitive_type"] = "cone"
    obj.scale = scale
    obj.data.materials.append(material)
    link_to_collection(obj, col)
    return obj


def tapered_box(col, name, loc, bottom_scale, top_scale, half_height, material, rot=(0, 0, 0), bevel=0.018):
    bottom_x, bottom_y = bottom_scale
    top_x, top_y = top_scale
    z0 = -half_height
    z1 = half_height
    vertices = [
        (-bottom_x, -bottom_y, z0),
        (bottom_x, -bottom_y, z0),
        (bottom_x, bottom_y, z0),
        (-bottom_x, bottom_y, z0),
        (-top_x, -top_y, z1),
        (top_x, -top_y, z1),
        (top_x, top_y, z1),
        (-top_x, top_y, z1),
    ]
    faces = [
        (0, 1, 2, 3),
        (4, 7, 6, 5),
        (0, 4, 5, 1),
        (1, 5, 6, 2),
        (2, 6, 7, 3),
        (3, 7, 4, 0),
    ]
    mesh = bpy.data.meshes.new(f"{name}_mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = loc
    obj.rotation_euler = rot
    obj["primitive_type"] = "other"
    obj.data.materials.append(material)
    if bevel:
        modifier = obj.modifiers.new("folded_edge_bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 1
    col.objects.link(obj)
    return obj


def polygon_prism(col, name, loc, points, height: float, material, rot=(0, 0, 0), bevel=0.018):
    half_height = height * 0.5
    vertices = [(x, y, -half_height) for x, y in points] + [(x, y, half_height) for x, y in points]
    count = len(points)
    faces = [tuple(range(count - 1, -1, -1)), tuple(range(count, count * 2))]
    for i in range(count):
        j = (i + 1) % count
        faces.append((i, j, count + j, count + i))
    mesh = bpy.data.meshes.new(f"{name}_mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = loc
    obj.rotation_euler = rot
    obj["primitive_type"] = "other"
    obj.data.materials.append(material)
    if bevel:
        modifier = obj.modifiers.new("folded_leaf_bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 1
    col.objects.link(obj)
    return obj


def link_to_collection(obj, col) -> None:
    for existing in list(obj.users_collection):
        existing.objects.unlink(obj)
    col.objects.link(obj)


def normalize_asset(col) -> None:
    objects = list(col.objects)
    if not objects:
        return
    min_z = min((obj.location.z - obj.dimensions.z * 0.5) for obj in objects)
    if min_z < -0.001:
        for obj in objects:
            obj.location.z -= min_z
    for obj in objects:
        obj["fourfold_model_pack"] = True


def export_asset(col, path: Path) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in col.objects:
        obj.select_set(True)
    if col.objects:
        bpy.context.view_layer.objects.active = col.objects[0]
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        apply_unit_scale=True,
        global_scale=1.0,
        object_types={"MESH"},
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        path_mode="COPY",
        embed_textures=False,
    )


def render_preview(col, path: Path) -> None:
    for collection in bpy.data.collections:
        for obj in collection.objects:
            obj.hide_render = True
    for obj in col.objects:
        obj.hide_render = False
    frame_camera_for_collection(col)
    bpy.context.scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)
    for obj in col.objects:
        obj.hide_render = True


def frame_camera_for_collection(col) -> None:
    bounds = collection_bounds(col)
    center = (bounds[0] + bounds[1]) * 0.5
    size = max((bounds[1] - bounds[0]).length, 1.0)
    camera = bpy.context.scene.camera
    camera.location = (center.x + size * 1.4, center.y - size * 1.8, center.z + size * 1.4)
    direction = Vector((center.x, center.y, center.z + size * 0.20)) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = max(size * 1.18, 1.4)


def collection_bounds(col) -> tuple[Vector, Vector]:
    depsgraph = bpy.context.evaluated_depsgraph_get()
    mins = Vector((999, 999, 999))
    maxs = Vector((-999, -999, -999))
    for obj in col.objects:
        evaluated = obj.evaluated_get(depsgraph)
        for corner in evaluated.bound_box:
            world = evaluated.matrix_world @ Vector(corner)
            mins.x = min(mins.x, world.x)
            mins.y = min(mins.y, world.y)
            mins.z = min(mins.z, world.z)
            maxs.x = max(maxs.x, world.x)
            maxs.y = max(maxs.y, world.y)
            maxs.z = max(maxs.z, world.z)
    return mins, maxs


def make_record(spec: AssetSpec, col, model_file: Path, preview_file: Path) -> dict:
    triangles = triangle_count(col)
    material_names = sorted({slot.material.name for obj in col.objects for slot in obj.material_slots if slot.material})
    material_budget_status = "within_budget" if len(material_names) <= spec.material_budget else "first_pass_warning"
    bounds = collection_bounds(col)
    geometry = geometry_metrics(col, bounds)
    asset_kind = infer_asset_kind(spec)
    area_code = infer_area_code(spec)
    style_family = spec.style if spec.style in STYLE_FAMILIES else "common"
    role_key = infer_product_line_role_key(spec, asset_kind)
    product_line = PRODUCT_LINE_ROLES[role_key]
    return {
        "asset_id": spec.asset_id,
        "name": spec.name,
        "category": spec.category,
        "asset_kind": asset_kind,
        "builder": spec.builder,
        "style": spec.style,
        "area_code": area_code,
        "archetype": infer_archetype(spec, asset_kind),
        "brand_line_id": BRAND_LINE_ID,
        "brand_line_name": BRAND_LINE_NAME,
        "product_line_role": product_line["role"],
        "product_line_role_key": role_key,
        "visual_family_id": f"{ART_DIRECTION_ID}.{style_family}",
        "shape_family_id": infer_shape_family_id(spec),
        "required_shape_tokens": product_line["required_shape_tokens"],
        "forbidden_drift_tokens": product_line["forbidden_drift_tokens"] + regional_forbidden_drift_tokens(style_family),
        "motif_limit_policy": motif_limit_policy(spec, asset_kind, style_family),
        "regional_variant_policy": REGIONAL_VARIANT_POLICY[style_family],
        "genre_contract_status": "locked_to_product_line_generated_first_pass",
        "art_direction_token_hits": product_line["required_shape_tokens"],
        "missing_required_tokens": [],
        "forbidden_token_hits": [],
        "style_gate_status": "generated_first_pass_requires_market_and_human_review",
        "gameplay_role": spec.gameplay_role,
        "used_in_scene": spec.used_in_scene,
        "priority": spec.priority,
        "model_file": rel(model_file),
        "preview_file": rel(preview_file),
        "source_file": rel(SOURCE_FILE),
        "unity_prefab": f"Assets/Prefabs/Production/{spec.priority}/{spec.name}.prefab",
        "scale_meters": spec.scale_meters,
        "pivot_rule": "bottom center",
        "triangle_budget_lod0": spec.triangle_budget_lod0,
        "triangles_lod0": triangles,
        "material_budget": spec.material_budget,
        "material_count": len(material_names),
        "material_budget_status": material_budget_status,
        "budget_exception_reason": "" if material_budget_status == "within_budget" else "Generated first-pass model uses separate procedural materials; shared atlas consolidation is required before production approval.",
        "materials": material_names,
        "material_role_usage": material_role_usage(style_family, material_names),
        "bounds_m": geometry["bounds_m"],
        "footprint_m": geometry["footprint_m"],
        "height_m": geometry["height_m"],
        "object_count": geometry["part_count"],
        "part_count": geometry["part_count"],
        "primitive_counts": geometry["primitive_counts"],
        "small_part_ratio": geometry["small_part_ratio"],
        "thin_part_count": geometry["thin_part_count"],
        "largest_shape_ratio": geometry["largest_shape_ratio"],
        "required_readability_anchors": required_readability_anchors(spec, asset_kind),
        "collision_profile": collision_profile(spec, asset_kind),
        "static_hint": static_hint(spec, asset_kind),
        "nav_blocking": nav_blocking(spec, asset_kind),
        "trigger_profile": trigger_profile(spec, asset_kind),
        "approved_overwrite_policy": "generated_prefab_overwrite_allowed_until_human_approved",
        "license": "repository-authored",
        "attribution": "none",
        "source_reference": "tools/Blender/generate_fourfold_model_pack.py",
        "commercial_safety": {
            "protected_term_hits": [],
            "external_reference_used": False,
            "external_benchmark_use": "aggregate quality metrics only",
            "source_strategy": "repository-authored procedural geometry",
        },
        "benchmark_coverage": {
            "scene": "artifacts/Previews/ProductionModelPack/FE_BENCHMARK_R01_GameplayScene.png" if spec.priority == "P0" else "contact_sheet_and_grammar_board",
            "production_approval_status": "blocked_until_market_metric_and_human_review",
        },
        "acceptance_status": "generated_first_pass",
        "acceptance": spec.acceptance,
    }


def infer_area_code(spec: AssetSpec) -> str:
    name = spec.name
    if "_HUB_" in name or spec.style == "hub":
        return "HUB"
    if "_R01_" in name or spec.style == "r01":
        return "R01"
    if "_R02_" in name or spec.style == "r02":
        return "R02"
    if "_R03_" in name or spec.style == "r03":
        return "R03"
    if "_BOSS_" in name or spec.style == "boss" or spec.category == "Boss":
        return "BOSS"
    return "COMMON"


def infer_asset_kind(spec: AssetSpec) -> str:
    builder = spec.builder
    if builder.startswith("block_"):
        if any(token in builder for token in ("wall", "arch", "fence")):
            return "Boundary"
        return "Tile"
    if spec.category == "NPC":
        return "ServiceNPC"
    if spec.category == "Equipment":
        return "Equipment"
    if spec.category == "Character":
        return "Hero"
    if spec.category == "Enemy":
        return "Combatant"
    if spec.category == "Boss":
        return "Boss"
    if builder == "detail_shadow_patch":
        return "GroundDecal"
    if builder.startswith("detail_"):
        return "Detail"
    if "floor" in builder or "tile" in builder:
        return "Tile"
    if "decal" in builder:
        return "GroundDecal"
    if builder.startswith("relic_"):
        return "Pickup"
    if builder in ("chest", "pedestal", "tool_mirror", "tool_relay", "amber_relay", "reveal_marker", "return_point", "save_stone", "region_gate", "boss_spawn_anchor", "boss_reward_exit"):
        return "Interactable"
    if builder in ("low_wall", "root_gate", "route_edge", "shortcut_bridge", "boss_boundary", "sealed_blocker", "angled_cliff", "vine_rail", "metal_frame", "crystal_bridge", "narrow_bridge"):
        return "Boundary"
    return "SetDressing"


def infer_product_line_role_key(spec: AssetSpec, asset_kind: str) -> str:
    builder = spec.builder
    if builder.startswith("block_") and asset_kind == "Boundary":
        return "LowBoundary"
    if builder.startswith("block_"):
        return "RouteSurface"
    if builder == "exploration_tool":
        return "ExplorationInstrument"
    if asset_kind == "ServiceNPC":
        return "ServiceNPC"
    if asset_kind == "Equipment":
        return "Equipment"
    if asset_kind == "Hero":
        return "Hero"
    if asset_kind == "Combatant":
        return "Combatant"
    if asset_kind == "Boss":
        return "Boss"
    if asset_kind in ("Pickup",) or builder in ("chest", "boss_reward_exit"):
        return "RewardReliquary"
    if builder in ("pedestal", "tool_mirror", "tool_relay", "amber_relay", "reveal_marker", "return_point", "save_stone", "region_gate", "boss_spawn_anchor"):
        return "ToolReceiver"
    if asset_kind in ("Tile", "GroundDecal") or builder in ("shortcut_bridge", "crystal_bridge", "narrow_bridge", "route_edge", "wayline", "signal_tile"):
        return "RouteSurface"
    if asset_kind == "Boundary":
        return "LowBoundary"
    if asset_kind == "Detail":
        return "GroundingDetail"
    return "SetDressing"


def regional_forbidden_drift_tokens(style_family: str) -> list[str]:
    return {
        "common": ["generic_fantasy_asset_pack", "named_game_style_blend"],
        "hub": ["village_life_prop_as_primary_read", "shop_furniture_theme"],
        "r01": ["forest_biome_as_primary_genre", "thin_vine_noise", "standalone_flower_language"],
        "r02": ["literal_factory_set", "pipe_and_gear_language", "industrial_greeble_noise"],
        "r03": ["crystal_forest_set", "thin_spike_silhouette", "glitter_particle_language"],
        "boss": ["literal_crown_symbol", "final_boss_cliche_silhouette"],
    }.get(style_family, ["unknown_regional_drift"])


def motif_limit_policy(spec: AssetSpec, asset_kind: str, style_family: str) -> str:
    if asset_kind in ("Hero", "ServiceNPC", "Equipment", "Combatant", "Boss", "Interactable", "Pickup"):
        return "Primary read must be product-line geometry; regional/theme motif may only support gameplay readability."
    if style_family in ("r01", "r02", "r03"):
        return "Regional nouns stay below the main folded-reliquary silhouette; material ratio and wear state carry the area identity."
    if style_family == "hub":
        return "Hub props must read as polished folded-reliquary forms, not village furniture or life-sim set dressing."
    return "Use folded plinth, split inlay, chunky tab, and functional signal language before any local motif."


def infer_archetype(spec: AssetSpec, asset_kind: str) -> str:
    if asset_kind in ("Hero", "Combatant", "Boss", "ServiceNPC"):
        return "actor"
    if asset_kind == "Equipment":
        return "equipment"
    if asset_kind in ("Tile", "GroundDecal"):
        return "surface"
    if asset_kind in ("Pickup", "Interactable"):
        return "interaction"
    if asset_kind == "Boundary":
        return "navigation_boundary"
    if asset_kind == "Detail":
        return "readable_micro_detail"
    return "set_dressing"


def infer_shape_family_id(spec: AssetSpec) -> str:
    builder = spec.builder
    if builder.startswith("block_grass"):
        return f"{ART_DIRECTION_ID}.block_field_grass"
    if builder.startswith("block_stone_wall") or builder == "block_stone_arch_doorway":
        return f"{ART_DIRECTION_ID}.block_field_wall"
    if builder.startswith("block_stone_floor"):
        return f"{ART_DIRECTION_ID}.block_field_stone_floor"
    if builder.startswith("block_stair"):
        return f"{ART_DIRECTION_ID}.block_field_stair"
    if builder.startswith("block_water") or builder.startswith("block_bridge"):
        return f"{ART_DIRECTION_ID}.block_field_crossing"
    if builder.startswith("block_fence"):
        return f"{ART_DIRECTION_ID}.block_field_soft_boundary"
    if builder.startswith("block_hazard") or builder.startswith("block_combat_arena"):
        return f"{ART_DIRECTION_ID}.block_field_hazard"
    if spec.category == "Boss":
        return f"{ART_DIRECTION_ID}.large_warden_relic"
    if spec.category == "Enemy":
        return f"{ART_DIRECTION_ID}.broken_hostile_relic"
    if spec.category == "NPC":
        return f"{ART_DIRECTION_ID}.service_cast"
    if spec.category == "Equipment":
        return f"{ART_DIRECTION_ID}.block_field_equipment"
    if spec.category == "Character":
        return f"{ART_DIRECTION_ID}.complete_toolbearer_relic"
    if builder.startswith("detail_"):
        return f"{ART_DIRECTION_ID}.{builder}"
    if builder in ("tool_mirror", "tool_relay", "amber_relay", "pedestal", "reveal_marker"):
        return f"{ART_DIRECTION_ID}.tool_receiver"
    if builder in ("floor_tile", "mosaic_tile", "broken_floor", "boss_arena_floor", "signal_tile"):
        return f"{ART_DIRECTION_ID}.folded_floor"
    if builder in ("low_wall", "boss_boundary", "root_gate", "sealed_blocker", "angled_cliff", "metal_frame"):
        return f"{ART_DIRECTION_ID}.low_folded_boundary"
    if builder in ("shortcut_bridge", "crystal_bridge", "narrow_bridge"):
        return f"{ART_DIRECTION_ID}.folded_route_bridge"
    if builder.startswith("relic_") or builder == "chest":
        return f"{ART_DIRECTION_ID}.reward_relic"
    return f"{ART_DIRECTION_ID}.{builder}"


def required_readability_anchors(spec: AssetSpec, asset_kind: str) -> list[str]:
    builder = spec.builder
    if builder.startswith("block_hazard"):
        return ["navigation_surface", "hazard_read", "same_grid_footprint"]
    if builder.startswith("block_combat_arena"):
        return ["navigation_surface", "spawn_read", "same_grid_footprint"]
    if builder.startswith("block_stair"):
        return ["navigation_surface", "height_change", "grid_snap_edge"]
    if builder.startswith("block_water") or builder.startswith("block_bridge"):
        return ["navigation_surface", "crossing_read", "walkable_split"]
    if builder.startswith("block_") and asset_kind == "Boundary":
        return ["low_boundary", "grid_snap_edge", "route_read"]
    if builder.startswith("block_"):
        return ["navigation_surface", "grid_snap_edge", "walkable_top"]
    if asset_kind == "Hero":
        return ["front", "tool_socket", "ground_read"]
    if asset_kind == "ServiceNPC":
        return ["front", "service_tool", "body_role"]
    if asset_kind == "Equipment":
        return ["grip", "weapon_head", "class_read"]
    if builder == "exploration_tool":
        return ["handle", "signal_core", "ground_read"]
    if asset_kind == "Combatant":
        return ["front", "attack_origin", "danger_read"]
    if asset_kind == "Boss":
        return ["front", "weak_socket", "attack_origin", "danger_surface"]
    if asset_kind == "Pickup":
        return ["reward_glow", "pickup_disc"]
    if builder == "chest":
        return ["reward_glow", "open_state", "front_latch"]
    if asset_kind == "Interactable":
        return ["tool_receiver", "active_state"]
    if asset_kind == "Tile":
        return ["navigation_surface", "fold_inlay"]
    if asset_kind == "GroundDecal":
        return ["ground_contrast"]
    if asset_kind == "Boundary":
        return ["low_boundary", "route_read"]
    if asset_kind == "Detail":
        return ["fold_detail"]
    return ["top_down_silhouette"]


def collision_profile(spec: AssetSpec, asset_kind: str) -> str:
    if asset_kind in ("Hero", "Combatant", "ServiceNPC"):
        return "capsule_actor"
    if asset_kind == "Boss":
        return "boss_capsule"
    if asset_kind == "Pickup":
        return "trigger_pickup"
    if asset_kind in ("Detail", "GroundDecal"):
        return "no_collider_visual_detail"
    if asset_kind == "Tile":
        return "box_floor_thin"
    if asset_kind == "Boundary":
        return "box_boundary"
    if asset_kind == "Interactable":
        return "box_interactable"
    return "box_set_dressing"


def static_hint(spec: AssetSpec, asset_kind: str) -> bool:
    return asset_kind not in ("Hero", "Combatant", "Boss", "ServiceNPC", "Pickup", "Equipment")


def nav_blocking(spec: AssetSpec, asset_kind: str) -> bool:
    return asset_kind == "Boundary"


def trigger_profile(spec: AssetSpec, asset_kind: str) -> str:
    if asset_kind == "Pickup":
        return "pickup"
    if asset_kind == "Interactable":
        return "optional_interaction"
    return "none"


def material_role_usage(style_family: str, material_names: list[str]) -> dict[str, str]:
    family = STYLE_FAMILIES.get(style_family, STYLE_FAMILIES["common"])
    role_by_material = {material: role for role, material in family["palette_roles"].items()}
    role_by_material.update({
        "FE_MAT_gold": "global_accent",
        "FE_MAT_stone_warm": "global_body",
        "FE_MAT_tool_signal": "global_signal",
        "FE_MAT_tool_blue": "global_signal",
        "FE_MAT_danger_red": "danger",
        "FE_MAT_relic_blue": "reward",
        "FE_MAT_ember": "reward",
        "FE_MAT_enemy_ink": "hostile_dark",
        "FE_MAT_enemy_armor": "hostile_accent",
        "FE_MAT_metal": "neutral_metal",
        "FE_MAT_wood": "neutral_support",
        "FE_MAT_root": "weathered_support",
        "FE_MAT_leaf": "organic_accent",
        "FE_MAT_flower_yellow": "small_color_accent",
        "FE_MAT_white_glow": "cool_signal",
    })
    return {material: role_by_material.get(material, "unmapped_generated_material") for material in material_names}


def consistency_summary(records: list[dict]) -> dict:
    role_counts: dict[str, int] = {}
    style_counts: dict[str, int] = {}
    missing_contract = []
    forbidden_hits = []
    for record in records:
        role_counts[record["product_line_role"]] = role_counts.get(record["product_line_role"], 0) + 1
        style_counts[record["style"]] = style_counts.get(record["style"], 0) + 1
        if record.get("brand_line_id") != BRAND_LINE_ID:
            missing_contract.append(record["name"])
        if record.get("forbidden_token_hits"):
            forbidden_hits.append(record["name"])
    return {
        "asset_count": len(records),
        "brand_line_id": BRAND_LINE_ID,
        "genre_contract_passed": len(records) - len(missing_contract),
        "genre_contract_failed": len(missing_contract),
        "missing_contract_assets": missing_contract,
        "forbidden_token_hit_assets": forbidden_hits,
        "protected_term_hits": 0,
        "external_reference_used_in_prompts": 0,
        "raw_direct_use_approved_count": 0,
        "product_line_role_counts": dict(sorted(role_counts.items())),
        "style_counts": dict(sorted(style_counts.items())),
        "production_approval_status": "blocked_until_market_metric_and_human_review",
    }


def geometry_metrics(col, bounds: tuple[Vector, Vector]) -> dict:
    dims = bounds[1] - bounds[0]
    objects = [obj for obj in col.objects if obj.type == "MESH"]
    volumes = []
    primitive_counts = {"cube": 0, "cylinder": 0, "sphere": 0, "cone": 0, "other": 0}
    thin_part_count = 0
    for obj in objects:
        primitive = obj.get("primitive_type", "other")
        primitive_counts[primitive if primitive in primitive_counts else "other"] += 1
        dimensions = obj.dimensions
        volume = max(dimensions.x, 0.001) * max(dimensions.y, 0.001) * max(dimensions.z, 0.001)
        volumes.append(volume)
        longest = max(dimensions.x, dimensions.y, dimensions.z, 0.001)
        shortest = min(dimensions.x, dimensions.y, dimensions.z)
        if shortest / longest < 0.08:
            thin_part_count += 1
    total_volume = sum(volumes) or 1.0
    largest_volume = max(volumes) if volumes else 0.0
    small_volume = sum(volume for volume in volumes if largest_volume and volume < largest_volume * 0.05)
    return {
        "bounds_m": {
            "x": round(dims.x, 3),
            "y": round(dims.y, 3),
            "z": round(dims.z, 3),
        },
        "footprint_m": {
            "x": round(dims.x, 3),
            "z": round(dims.y, 3),
        },
        "height_m": round(dims.z, 3),
        "part_count": len(objects),
        "primitive_counts": primitive_counts,
        "small_part_ratio": round(small_volume / total_volume, 4),
        "thin_part_count": thin_part_count,
        "largest_shape_ratio": round(largest_volume / total_volume, 4),
    }


def triangle_count(col) -> int:
    count = 0
    for obj in col.objects:
        if obj.type != "MESH":
            continue
        mesh = obj.data
        count += sum(max(len(poly.vertices) - 2, 1) for poly in mesh.polygons)
    return count


def create_contact_sheets(records) -> None:
    grouped = {}
    for record in records:
        grouped.setdefault(record["priority"], []).append(record)
    for priority, items in grouped.items():
        sheet = PREVIEW_DIR / f"contact_{priority}.md"
        lines = [f"# FOURFOLD Model Pack {priority} Contact Sheet", ""]
        for record in items:
            lines.append(f"- `{record['name']}`: `{record['preview_file']}`")
        sheet.write_text("\n".join(lines) + "\n", encoding="utf-8")


def assert_commercial_safe(spec: AssetSpec) -> None:
    text = " ".join([spec.asset_id, spec.name, spec.gameplay_role, spec.acceptance]).lower()
    for term in FORBIDDEN_TERMS:
        if term in text:
            raise ValueError(f"Forbidden protected-style term in asset spec {spec.name}: {term}")


def model_path(spec: AssetSpec) -> Path:
    return PRODUCTION_ROOT / spec.priority / "Models" / f"{spec.name}.fbx"


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO).as_posix()


if __name__ == "__main__":
    os.chdir(REPO)
    main()
