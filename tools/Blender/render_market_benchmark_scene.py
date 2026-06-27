#!/usr/bin/env python3
"""Render a production-art benchmark scene from generated model-pack pieces."""

from __future__ import annotations

import importlib.util
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


REPO = Path(__file__).resolve().parents[2]
GENERATOR_PATH = REPO / "tools" / "Blender" / "generate_fourfold_model_pack.py"
OUTPUT = REPO / "artifacts" / "Previews" / "ProductionModelPack" / "FE_BENCHMARK_R01_GameplayScene.png"
GRAMMAR_BOARD_OUTPUT = REPO / "artifacts" / "Previews" / "ProductionModelPack" / "FE_BENCHMARK_FoldedReliquaryGrammar.png"


def main() -> None:
    gen = load_generator()
    gen.ensure_clean_scene()
    configure_render_surface()
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    materials = gen.create_materials()

    scene_col = bpy.data.collections.new("FE_BENCHMARK_R01_GameplayScene")
    bpy.context.scene.collection.children.link(scene_col)

    build_floor(gen, scene_col, materials)
    add_value_zones(gen, scene_col, materials)
    place_asset(gen, scene_col, materials, "FE_ENV_R01_BF_CombatArena_2x2_01", (0.55, -0.15, 0.055), 1.30, math.radians(0))
    for location, scale in [
        ((-3.55, -2.55, 0.018), 1.25),
        ((-2.35, -2.06, 0.018), 0.98),
        ((-1.30, -1.22, 0.018), 0.92),
        ((0.55, 0.35, 0.018), 1.35),
        ((2.48, -1.28, 0.018), 1.65),
        ((4.05, 2.20, 0.018), 1.20),
    ]:
        place_asset(gen, scene_col, materials, "FE_PROP_COMMON_SoftShadowPatch_01", location, scale, math.radians(-14))

    place_asset(gen, scene_col, materials, "FE_PROP_COMMON_CheckpointSocketPad_01", (-4.55, -2.75, 0), 1.05, math.radians(8))
    place_asset(gen, scene_col, materials, "FE_CHAR_PLAYER_SignatureLead_01", (-3.55, -2.55, 0), 1.08, math.radians(32))
    place_asset(gen, scene_col, materials, "FE_CHAR_PLAYER_HeavyGuard_01", (-2.45, -2.00, 0), 0.92, math.radians(26))
    place_asset(gen, scene_col, materials, "FE_CHAR_PLAYER_StampCaster_01", (-1.45, -1.28, 0), 0.88, math.radians(22))
    place_asset(gen, scene_col, materials, "FE_PROP_COMMON_ToolBlade_01", (-3.92, -1.86, 0.04), 0.78, math.radians(-32))
    place_asset(gen, scene_col, materials, "FE_PROP_COMMON_SealStamp_01", (-1.05, -0.78, 0.04), 0.72, math.radians(-18))

    place_asset(gen, scene_col, materials, "FE_PROP_R01_GimmickPedestal_01", (-1.72, -0.70, 0), 1.12, math.radians(-10))
    place_asset(gen, scene_col, materials, "FE_ENV_R01_ShortcutBridge_01", (-4.55, 0.55, 0), 1.18, math.radians(23))
    place_asset(gen, scene_col, materials, "FE_PROP_R01_RootGate_01", (-4.95, -0.45, 0), 1.22, math.radians(90))
    place_asset(gen, scene_col, materials, "FE_ENEMY_R01_FoldBiter_01", (0.24, 0.38, 0), 1.05, math.radians(210))
    place_asset(gen, scene_col, materials, "FE_ENEMY_R01_ShieldClamp_01", (1.08, -0.36, 0), 1.03, math.radians(210))
    place_asset(gen, scene_col, materials, "FE_ENEMY_R01_LineSpitter_01", (2.42, 1.12, 0), 0.96, math.radians(225))
    place_asset(gen, scene_col, materials, "FE_ENEMY_R01_BlockCharger_01", (2.48, -1.28, 0), 1.08, math.radians(205))
    place_asset(gen, scene_col, materials, "FE_BOSS_R01_StampMiniboss_01", (3.58, -0.22, 0), 0.72, math.radians(210))
    place_asset(gen, scene_col, materials, "FE_PROP_COMMON_RewardReceiverPad_01", (4.05, 2.20, 0), 1.08, math.radians(-20))
    place_asset(gen, scene_col, materials, "FE_PROP_COMMON_RelicChest_01", (4.62, 2.70, 0), 0.92, math.radians(-20))
    place_asset(gen, scene_col, materials, "FE_RELIC_EmberSeed_01", (4.47, 2.36, 0.85), 1.08, 0)
    add_functional_signal_geometry(gen, scene_col, materials)
    add_block_field_polish(gen, scene_col, materials)

    add_detail_clusters(gen, scene_col, materials)
    add_foreground_frame(gen, scene_col, materials)

    add_light("Benchmark Warm Key", (4.8, -5.2, 8.0), (1.0, 0.78, 0.54), 600)
    add_light("Benchmark Tool Fill", (-2.4, -1.5, 3.0), (0.72, 0.95, 0.25), 260)
    add_light("Benchmark Reward Fill", (4.2, 2.6, 3.0), (0.2, 0.58, 1.0), 220)
    add_light("Benchmark Hostile Rim", (1.6, 1.0, 2.8), (1.0, 0.16, 0.06), 160)

    camera = bpy.context.scene.camera
    camera.location = (7.0, 11.2, 8.4)
    target = Vector((0.35, -0.20, 0.60))
    direction = target - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 6.55

    bpy.context.scene.render.engine = "BLENDER_WORKBENCH"
    configure_workbench_shading()
    bpy.context.scene.render.resolution_x = 1280
    bpy.context.scene.render.resolution_y = 720
    bpy.context.scene.render.filepath = str(OUTPUT)
    bpy.ops.render.render(write_still=True)
    print(f"Rendered benchmark scene: {OUTPUT}")
    render_grammar_board(gen, materials)


def load_generator():
    spec = importlib.util.spec_from_file_location("fourfold_model_pack_generator", GENERATOR_PATH)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Could not load generator at {GENERATOR_PATH}")
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


def configure_render_surface() -> None:
    world = bpy.context.scene.world
    if world:
        world.color = (0.060, 0.070, 0.085)


def configure_workbench_shading() -> None:
    shading = bpy.context.scene.display.shading
    shading.light = "STUDIO"
    shading.show_shadows = True
    if hasattr(shading, "show_cavity"):
        shading.show_cavity = True
    if hasattr(shading, "cavity_valley_factor"):
        shading.cavity_valley_factor = 1.75
    if hasattr(shading, "cavity_ridge_factor"):
        shading.cavity_ridge_factor = 0.85
    if hasattr(shading, "color_type"):
        shading.color_type = "MATERIAL"
    if hasattr(shading, "background_type"):
        shading.background_type = "VIEWPORT"
    if hasattr(shading, "background_color"):
        shading.background_color = (0.062, 0.070, 0.086)


def build_floor(gen, scene_col, m) -> None:
    room = bpy.data.collections.new("Room")
    scene_col.children.link(room)
    for x in range(-6, 7):
        for z in range(-4, 5):
            edge = abs(x) == 6 or abs(z) == 4
            diagonal_band = -1 <= (x - z) <= 1
            threat_zone = -1 <= x <= 3 and -2 <= z <= 2
            reward_zone = x >= 3 and z >= 1
            hero_zone = x <= -2 and z <= -1
            mat = m["hub_ivory"] if diagonal_band else (m["stone_warm"] if (x + z) % 2 else m["moss"])
            if edge:
                mat = m["stone_dark"]
            elif threat_zone:
                mat = m["stone_warm"] if (x + z) % 2 else m["route_teal"]
            if reward_zone:
                mat = m["gold"] if (x + z) % 2 else m["relic_blue"]
            if hero_zone:
                mat = m["hub_ivory"] if (x + z) % 2 else m["tool_signal"]
            if not edge and not threat_zone and (x + z) % 4 == 0:
                mat = m["flower_yellow"]
            plate_scale = 0.52 if diagonal_band else 0.47
            gen.cube(room, f"floor_{x}_{z}", (x, z, -0.045), (plate_scale, plate_scale, 0.05), mat, rot=(0, 0, math.radians(((x * 7 + z * 11) % 9) - 4)))
            if not edge and (x + z) % 3 == 0:
                line_mat = m["danger_red"] if threat_zone and (x + z) % 2 == 0 else m["stone_dark"]
                gen.cube(room, f"fold_line_{x}_{z}", (x + 0.14, z - 0.15, 0.020), (0.34, 0.034, 0.024), line_mat, rot=(0, 0, math.radians((x * 13 + z * 7) % 36 - 18)))
            if not edge and (x * 2 + z) % 7 == 0:
                gen.sphere(room, f"flower_read_{x}_{z}", (x - 0.20, z + 0.18, 0.10), (0.08, 0.08, 0.08), m["flower_yellow"])
    for x in range(-6, 7, 2):
        gen.cube(room, f"north_wall_{x}", (x, 4.55, 0.34), (0.68, 0.15, 0.34), m["stone_dark"], rot=(0, 0, math.radians(-4)))
        gen.cube(room, f"south_wall_{x}", (x, -4.55, 0.34), (0.68, 0.15, 0.34), m["stone_dark"], rot=(0, 0, math.radians(4)))
    for z in range(-2, 5, 2):
        gen.cube(room, f"east_wall_{z}", (6.55, z, 0.34), (0.15, 0.66, 0.34), m["stone_dark"])
    for x, y, angle in [(-1.2, 2.2, 18), (1.7, -3.1, -16), (3.8, 0.2, 13), (-4.2, 2.6, -24)]:
        gen.cube(room, f"low_fold_{x}_{y}", (x, y, 0.22), (0.74, 0.10, 0.10), m["root"], rot=(0, 0, math.radians(angle)))
        gen.cube(room, f"fold_inlay_{x}_{y}", (x + 0.08, y - 0.06, 0.32), (0.52, 0.030, 0.026), m["tool_signal"], rot=(0, 0, math.radians(angle + 5)))
    for x, y in [(-3.1, -1.05), (-2.35, -1.12), (-1.58, -1.10), (-0.82, -0.96), (-0.08, -0.72)]:
        gen.cube(room, f"tool_route_{x}_{y}", (x, y, 0.035), (0.40, 0.035, 0.025), m["tool_signal"], rot=(0, 0, math.radians(16)))
    for x, y, angle in [(-3.5, 1.6, -22), (-0.6, 2.75, 18), (2.8, 2.95, -16), (3.55, -0.65, 23), (-4.55, -2.05, -26)]:
        gen.cube(room, f"warm_route_{x}_{y}", (x, y, 0.04), (0.44, 0.04, 0.025), m["gold"], rot=(0, 0, math.radians(angle)))


def add_value_zones(gen, scene_col, m) -> None:
    zones = bpy.data.collections.new("AuthoredValueZones")
    scene_col.children.link(zones)
    for name, loc, scale, material, angle in [
        ("hero_warm_island", (-3.05, -2.34, -0.010), (1.92, 1.20, 0.035), m["hub_ivory"], 12),
        ("checkpoint_blue_island", (-4.58, -2.72, -0.008), (1.10, 0.86, 0.034), m["tool_blue"], -8),
        ("tool_signal_pool", (-1.70, -0.88, -0.006), (1.38, 0.92, 0.032), m["tool_signal"], -10),
        ("enemy_shadow_island", (0.95, 0.02, -0.004), (2.04, 1.36, 0.040), m["enemy_ink"], -8),
        ("combat_teal_island", (1.02, 0.72, -0.003), (1.52, 0.80, 0.034), m["route_teal"], 13),
        ("miniboss_shadow_island", (2.62, -1.18, -0.006), (1.84, 1.26, 0.038), m["stone_dark"], 15),
        ("reward_gold_island", (4.10, 2.26, -0.004), (1.58, 1.04, 0.036), m["gold"], -18),
        ("foreground_dark_band", (-0.80, -3.88, -0.002), (4.90, 0.38, 0.052), m["enemy_ink"], 4),
    ]:
        gen.cube(zones, name, loc, scale, material, rot=(0, 0, math.radians(angle)), bevel=0.035)
    for index, (x, y, angle, length) in enumerate([
        (-3.65, -2.82, 18, 0.72),
        (-2.85, -2.32, 18, 0.62),
        (-2.20, -1.82, 18, 0.62),
        (-1.55, -1.34, 18, 0.54),
        (-0.92, -0.88, 18, 0.50),
        (0.05, -0.15, 18, 0.44),
        (1.18, 0.40, -18, 0.50),
        (2.25, 1.05, -18, 0.54),
        (3.35, 1.72, -18, 0.62),
        (4.02, 2.18, -18, 0.70),
    ]):
        material = m["tool_signal"] if index < 6 else m["gold"]
        gen.cube(zones, f"diagonal_signal_path_{index:02}", (x, y, 0.060), (length, 0.045, 0.030), material, rot=(0, 0, math.radians(angle)), bevel=0.012)
    for index, (x, y, angle) in enumerate([(0.30, 0.15, -22), (0.82, -0.20, 18), (1.18, 0.66, 31), (2.20, -1.18, -12), (2.90, -1.72, 20)]):
        gen.cube(zones, f"hostile_red_crack_{index:02}", (x, y, 0.068), (0.62, 0.052, 0.034), m["danger_red"], rot=(0, 0, math.radians(angle)), bevel=0.010)
    for index, (x, y) in enumerate([(-3.45, -2.05), (-2.82, -2.95), (-1.30, -0.85), (3.62, 2.02), (4.45, 2.76), (4.10, 1.72)]):
        gen.sphere(zones, f"high_saturation_beat_{index:02}", (x, y, 0.120), (0.10, 0.10, 0.075), m["flower_yellow" if index < 3 else "relic_blue"])


def add_block_field_polish(gen, scene_col, m) -> None:
    polish = bpy.data.collections.new("BlockFieldReadabilityPolish")
    scene_col.children.link(polish)
    for index, (x, y, angle, material, length) in enumerate([
        (-3.35, -2.10, 18, m["tool_signal"], 0.86),
        (-2.55, -1.74, 18, m["tool_signal"], 0.78),
        (-1.68, -1.28, 18, m["tool_signal"], 0.70),
        (-0.72, -0.72, 18, m["tool_signal"], 0.66),
        (0.12, -0.20, -18, m["danger_red"], 0.86),
        (0.72, 0.16, -18, m["danger_red"], 0.78),
        (1.36, 0.48, -18, m["danger_red"], 0.74),
        (2.40, -0.64, 20, m["danger_red"], 0.86),
        (3.24, -0.98, 20, m["danger_red"], 0.76),
        (3.78, 1.74, -22, m["relic_blue"], 0.76),
        (4.36, 2.18, -22, m["relic_blue"], 0.72),
    ]):
        gen.cube(polish, f"wide_gameplay_read_band_{index:02}", (x, y, 0.182), (length, 0.105, 0.044), material, rot=(0, 0, math.radians(angle)), bevel=0.018)
        gen.cube(polish, f"dark_backing_for_band_{index:02}", (x + 0.05, y - 0.06, 0.152), (length * 0.72, 0.040, 0.030), m["enemy_ink"], rot=(0, 0, math.radians(angle - 7)), bevel=0.010)
    for index, (x, y, material) in enumerate([
        (-3.86, -2.72, m["tool_blue"]),
        (-2.98, -2.32, m["flower_yellow"]),
        (-2.12, -1.82, m["tool_signal"]),
        (-1.12, -1.18, m["stamp_magenta"]),
        (0.28, 0.60, m["danger_red"]),
        (1.12, -0.30, m["danger_red"]),
        (2.36, 1.14, m["danger_red"]),
        (2.96, -1.48, m["danger_red"]),
        (3.86, 2.18, m["relic_blue"]),
        (4.58, 2.70, m["flower_yellow"]),
    ]):
        gen.cyl(polish, f"large_color_socket_{index:02}", (x, y, 0.220), (0.155, 0.155, 0.040), material, vertices=12, bevel=True)
        gen.cyl(polish, f"socket_dark_cut_{index:02}", (x + 0.05, y - 0.04, 0.192), (0.110, 0.110, 0.030), m["enemy_ink"], vertices=10, bevel=True)
    for index, (x, y, angle) in enumerate([
        (-4.86, -2.98, 42),
        (-4.18, -3.22, -20),
        (-3.10, -3.05, 15),
        (-0.20, 1.00, -28),
        (1.72, 0.96, 18),
        (2.94, 0.34, -10),
        (3.70, -1.56, 26),
        (4.76, 1.84, -18),
    ]):
        gen.cube(polish, f"thick_fold_edge_{index:02}", (x, y, 0.205), (0.44, 0.070, 0.040), m["stone_dark"], rot=(0, 0, math.radians(angle)), bevel=0.014)
        gen.cube(polish, f"bright_edge_lip_{index:02}", (x - 0.03, y + 0.04, 0.238), (0.30, 0.034, 0.030), m["hub_ivory"], rot=(0, 0, math.radians(angle + 6)), bevel=0.010)


def add_functional_signal_geometry(gen, scene_col, m) -> None:
    signals = bpy.data.collections.new("FunctionalSignalGeometry")
    scene_col.children.link(signals)
    for index, (x, y, angle, length, width) in enumerate([
        (-3.05, -2.18, 21, 0.78, 0.070),
        (-2.38, -1.76, 19, 0.70, 0.065),
        (-1.78, -1.35, 15, 0.62, 0.060),
        (-1.16, -0.94, 13, 0.54, 0.058),
    ]):
        gen.cube(signals, f"tool_reaction_band_{index:02}", (x, y, 0.105), (length, width, 0.035), m["tool_signal"], rot=(0, 0, math.radians(angle)), bevel=0.016)
        gen.cube(signals, f"tool_reaction_shadow_cut_{index:02}", (x + 0.08, y - 0.08, 0.086), (length * 0.68, 0.030, 0.026), m["stone_dark"], rot=(0, 0, math.radians(angle - 7)), bevel=0.010)
    for index, (x, y, angle, length) in enumerate([
        (0.45, 0.16, -18, 0.86),
        (0.92, -0.10, 12, 0.70),
        (1.35, 0.44, 32, 0.62),
        (2.45, -1.15, -15, 0.80),
        (2.86, -1.72, 24, 0.62),
    ]):
        gen.cube(signals, f"enemy_tell_wedge_{index:02}", (x, y, 0.120), (length, 0.080, 0.040), m["danger_red"], rot=(0, 0, math.radians(angle)), bevel=0.018)
        gen.cube(signals, f"enemy_tell_dark_back_{index:02}", (x - 0.05, y + 0.05, 0.092), (length * 0.52, 0.050, 0.030), m["enemy_ink"], rot=(0, 0, math.radians(angle + 7)), bevel=0.012)
    for index, (x, y, angle) in enumerate([(3.72, 2.12, -21), (4.22, 2.44, 12), (4.68, 2.18, 28), (4.10, 2.94, -8)]):
        gen.cube(signals, f"reward_quarter_beam_{index:02}", (x, y, 0.135), (0.54, 0.070, 0.040), m["relic_blue"], rot=(0, 0, math.radians(angle)), bevel=0.016)
        gen.cube(signals, f"reward_gold_lip_{index:02}", (x + 0.05, y - 0.04, 0.166), (0.36, 0.040, 0.032), m["gold"], rot=(0, 0, math.radians(angle + 8)), bevel=0.012)


def add_detail_clusters(gen, scene_col, materials) -> None:
    details = [
        ("FE_PROP_R01_CrackInlay_01", (-2.55, -1.85, 0.01), 1.05, -18),
        ("FE_PROP_R01_GoldThread_01", (-2.20, -1.62, 0.01), 1.2, 12),
        ("FE_PROP_R01_PetalPatch_01", (-3.05, -2.05, 0.01), 1.0, 33),
        ("FE_PROP_R01_CrackInlay_01", (-3.52, -2.92, 0.01), 1.15, 8),
        ("FE_PROP_R01_MossCurb_01", (-3.76, -2.30, 0.01), 1.05, -15),
        ("FE_PROP_R01_LeafSprig_01", (-2.72, -2.72, 0.01), 0.95, 24),
        ("FE_PROP_R01_HalfBuriedTile_01", (-1.12, -1.78, 0.01), 1.0, -9),
        ("FE_PROP_R01_SapGlowBud_01", (-1.38, -0.75, 0.01), 1.0, 7),
        ("FE_PROP_R01_FoldedStoneMarker_01", (-0.72, -0.38, 0.01), 1.05, -26),
        ("FE_PROP_R01_GoldThread_01", (-1.95, -0.92, 0.01), 1.30, 19),
        ("FE_PROP_R01_PetalPatch_01", (-1.10, -1.32, 0.01), 1.05, -30),
        ("FE_PROP_R01_CrackInlay_01", (-0.34, -0.62, 0.01), 1.20, 12),
        ("FE_PROP_R01_RootKnot_01", (0.05, 0.95, 0.01), 1.05, 17),
        ("FE_PROP_R01_LeafSprig_01", (0.92, 1.12, 0.01), 1.0, -14),
        ("FE_PROP_R01_CrackInlay_01", (1.20, 0.04, 0.01), 1.1, 28),
        ("FE_PROP_BOSS_RedThread_01", (0.32, 0.58, 0.01), 1.12, -20),
        ("FE_PROP_BOSS_CrackBloom_01", (0.94, 0.74, 0.01), 1.05, 16),
        ("FE_PROP_BOSS_RedThread_01", (1.46, -0.18, 0.01), 1.20, 22),
        ("FE_PROP_R01_ShadowLeafPatch_01", (1.98, -1.10, 0.012), 1.7, 4),
        ("FE_PROP_R01_MossCurb_01", (2.82, -0.86, 0.01), 1.1, 18),
        ("FE_PROP_R01_HalfBuriedTile_01", (3.28, -1.98, 0.01), 1.15, -20),
        ("FE_PROP_BOSS_BlackChip_01", (2.12, -1.88, 0.01), 1.15, -8),
        ("FE_PROP_BOSS_RedThread_01", (2.82, -1.24, 0.01), 1.22, 25),
        ("FE_PROP_R01_CrackInlay_01", (3.10, -0.42, 0.01), 1.05, -31),
        ("FE_PROP_R01_PetalPatch_01", (3.65, 2.10, 0.01), 0.95, 18),
        ("FE_PROP_R01_GoldThread_01", (3.68, 2.82, 0.01), 1.18, -10),
        ("FE_PROP_R01_SapGlowBud_01", (4.45, 2.05, 0.01), 0.95, 30),
        ("FE_PROP_HUB_BlueChip_01", (3.55, 2.48, 0.01), 1.20, 11),
        ("FE_PROP_HUB_QuarterInlay_01", (4.62, 2.86, 0.01), 1.10, -12),
        ("FE_PROP_BOSS_BlackChip_01", (5.05, 1.92, 0.01), 1.15, 24),
    ]
    for name, loc, scale, angle in details:
        place_asset(gen, scene_col, materials, name, loc, scale, math.radians(angle))


def add_foreground_frame(gen, scene_col, materials) -> None:
    place_asset(gen, scene_col, materials, "FE_PROP_R01_RootArch_01", (-4.85, -3.72, 0), 1.38, math.radians(18))
    place_asset(gen, scene_col, materials, "FE_PROP_R01_LowWallRoot_01", (-1.05, -4.20, 0), 1.28, math.radians(-9))
    place_asset(gen, scene_col, materials, "FE_PROP_R01_LowWallRoot_01", (3.85, -3.95, 0), 1.35, math.radians(11))


def render_grammar_board(gen, materials) -> None:
    gen.ensure_clean_scene()
    configure_render_surface()
    board_col = bpy.data.collections.new("FE_BENCHMARK_FoldedReliquaryGrammar")
    bpy.context.scene.collection.children.link(board_col)
    rows = [
        ("FE_CHAR_PLAYER_SignatureLead_01", "FE_CHAR_PLAYER_HeavyGuard_01", "FE_CHAR_PLAYER_StampCaster_01", "FE_PROP_COMMON_CheckpointSocketPad_01"),
        ("FE_ENEMY_R01_FoldBiter_01", "FE_ENEMY_R01_ShieldClamp_01", "FE_ENEMY_R01_BlockCharger_01", "FE_BOSS_R01_StampMiniboss_01"),
        ("FE_ENV_R01_BF_CombatArena_2x2_01", "FE_PROP_COMMON_ToolBlade_01", "FE_PROP_COMMON_SealStamp_01", "FE_PROP_COMMON_RewardReceiverPad_01"),
        ("FE_CHAR_PLAYER_Hero_01", "FE_PROP_COMMON_ExplorationTool_01", "FE_PROP_R01_GimmickPedestal_01", "FE_PROP_COMMON_RelicChest_01"),
        ("FE_ENEMY_MELEE_Shardling", "FE_ENEMY_RANGED_BloomSpitter", "FE_ENEMY_MINIBOSS_RootBruiser", "FE_BOSS_01_RootWarden"),
        ("FE_ENV_HUB_FloorStone_01", "FE_PROP_HUB_LowWall_01", "FE_PROP_HUB_QuarterInlay_01", "FE_PROP_HUB_ThreadLamp_01"),
        ("FE_ENV_R01_FloorMossStone_01", "FE_PROP_R01_LowWallRoot_01", "FE_PROP_R01_FoldedStoneMarker_01", "FE_PROP_R01_CrackInlay_01"),
    ]
    for row_index, row in enumerate(rows):
        y = 3.25 - row_index * 1.22
        for col_index, name in enumerate(row):
            x = -3.45 + col_index * 2.3
            scale = 0.72 if row_index < 2 else 0.92
            place_asset(gen, board_col, materials, name, (x, y, 0), scale, math.radians(-8 + col_index * 8))
    add_light("Grammar Board Key", (3.0, -4.0, 7.0), (1.0, 0.82, 0.62), 520)
    add_light("Grammar Board Fill", (-3.0, 1.0, 3.0), (0.55, 0.75, 1.0), 180)
    camera = bpy.context.scene.camera
    camera.location = (6.4, 8.8, 7.4)
    target = Vector((0, 0, 0.35))
    camera.rotation_euler = (target - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 8.6
    bpy.context.scene.render.engine = "BLENDER_WORKBENCH"
    configure_workbench_shading()
    bpy.context.scene.render.resolution_x = 1280
    bpy.context.scene.render.resolution_y = 720
    bpy.context.scene.render.filepath = str(GRAMMAR_BOARD_OUTPUT)
    bpy.ops.render.render(write_still=True)
    print(f"Rendered grammar board: {GRAMMAR_BOARD_OUTPUT}")


def place_asset(gen, scene_col, materials, name: str, location, scale: float, rotation_z: float) -> None:
    source_spec = next(spec for spec in gen.ASSETS if spec.name == name)
    col = bpy.data.collections.new(name)
    scene_col.children.link(col)
    gen.build_asset(source_spec, col, materials)
    gen.normalize_asset(col)
    cos_z = math.cos(rotation_z)
    sin_z = math.sin(rotation_z)
    for obj in col.objects:
        local_x = obj.location.x * scale
        local_y = obj.location.y * scale
        obj.location.x = local_x * cos_z - local_y * sin_z + location[0]
        obj.location.y = local_x * sin_z + local_y * cos_z + location[1]
        obj.location.z = obj.location.z * scale + location[2]
        obj.scale = (obj.scale.x * scale, obj.scale.y * scale, obj.scale.z * scale)
        obj.rotation_euler.z += rotation_z


def add_light(name: str, location, color, energy: float) -> None:
    data = bpy.data.lights.new(name, "POINT")
    data.color = color
    data.energy = energy
    data.shadow_soft_size = 3.0
    obj = bpy.data.objects.new(name, data)
    obj.location = location
    bpy.context.scene.collection.objects.link(obj)


if __name__ == "__main__":
    main()
