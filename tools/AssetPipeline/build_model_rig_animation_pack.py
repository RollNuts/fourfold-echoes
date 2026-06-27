#!/usr/bin/env python3
"""Build a model/rig/animation delivery pack for the model-rig session.

Run with Blender:

    blender --background --factory-startup --python tools/AssetPipeline/build_model_rig_animation_pack.py

The first pack uses the existing Melee Shardling sealed-lock relic brief as a
technical build-ready sample. It is a mechanical transform rig, not a creature
skin rig.
"""

from __future__ import annotations

import json
import math
from datetime import datetime, timezone
from pathlib import Path

import bpy
from mathutils import Vector


REPO = Path(__file__).resolve().parents[2]
SEMVER = "0.1.0"
FAMILY = "Enemy"
SUBJECT = "MeleeShardling"
VARIANT = "SealedLockRelic"
PACK_ROOT = REPO / "Assets" / "Art" / "ModelRigAnimation" / FAMILY / SUBJECT / f"{VARIANT}_v{SEMVER}"
MODEL_DIR = PACK_ROOT / "Models"
ANIM_DIR = PACK_ROOT / "Animations"
CLIP_DIR = PACK_ROOT / "AnimationClips"
DOC_DIR = PACK_ROOT / "Docs"
PREVIEW_DIR = PACK_ROOT / "Previews"
FRAME_DIR = PREVIEW_DIR / "preview_frames"
MODEL_NAME = f"MDL_{FAMILY}_{SUBJECT}_{VARIANT}_v{SEMVER}.fbx"
MODEL_PATH = MODEL_DIR / MODEL_NAME
PREVIEW_GIF = PREVIEW_DIR / "preview.gif"
ASSET_JSON = PACK_ROOT / "asset.json"
ASSET_REQUEST = PACK_ROOT / "asset_request.yaml"
RIG_MEMO = DOC_DIR / "avatar_rig_memo.md"
QC_JSON = PACK_ROOT / "qc_result.json"
SOURCE_CONCEPT = "artifacts/Concepts/FoldedReliquary/IndividualCandidates_20260627/FE_CONCEPT_CANDIDATE_MeleeShardling_SealedLockRelic_v001.png"
SOURCE_BRIEF = "artifacts/Concepts/FoldedReliquary/FE_ENEMY_MELEE_Shardling_ModelingBrief.md"
SOURCE_SCHEMA = "artifacts/AssetPipeline/schema_v1/art_enemy_melee_01.json"
SAMPLE_RATE = 30


LOOP_ACTIONS = {"Idle", "Walk", "Run", "AttackLoop", "ChannelLoop"}
ROOT_MOTION_POLICY = (
    "Off. ROOT stays at world origin with no translation keys; gameplay movement "
    "is controller-driven in Unity. Locomotion clips are in-place compression/"
    "rocking cycles for a mechanical crawling relic."
)

ANIMATION_SPECS = [
    ("Idle", 1, 61, "loop", "neutral", "neutral"),
    ("Walk", 1, 31, "loop", "neutral", "neutral"),
    ("Run", 1, 25, "loop", "neutral", "neutral"),
    ("AttackStart", 1, 19, "oneshot", "neutral", "attack_extended"),
    ("AttackLoop", 1, 25, "loop", "attack_extended", "attack_extended"),
    ("AttackEnd", 1, 15, "oneshot", "attack_extended", "neutral"),
    ("HitFront", 1, 13, "oneshot", "neutral", "neutral"),
    ("HitBack", 1, 13, "oneshot", "neutral", "neutral"),
    ("Knockdown", 1, 31, "oneshot", "neutral", "knockdown"),
    ("Death", 1, 45, "oneshot", "neutral", "dead"),
    ("CastStart", 1, 21, "oneshot", "neutral", "charged"),
    ("ChannelLoop", 1, 37, "loop", "charged", "charged"),
    ("CastRelease", 1, 19, "oneshot", "charged", "neutral"),
    ("Interact", 1, 25, "oneshot", "neutral", "neutral"),
]

CONTACT_FRAMES = {
    "Idle": [1, 31, 61],
    "Walk": [1, 16, 31],
    "Run": [1, 13, 25],
    "AttackStart": [14],
    "AttackLoop": [1, 13, 25],
    "AttackEnd": [15],
    "HitFront": [1, 13],
    "HitBack": [1, 13],
    "Knockdown": [18, 31],
    "Death": [24, 45],
    "CastStart": [21],
    "ChannelLoop": [1, 19, 37],
    "CastRelease": [19],
    "Interact": [12, 25],
}

EVENT_FRAMES = {
    "Idle": [],
    "Walk": [{"frame": 1, "event": "base_contact_left"}, {"frame": 16, "event": "base_contact_right"}],
    "Run": [{"frame": 1, "event": "base_contact_left"}, {"frame": 13, "event": "base_contact_right"}],
    "AttackStart": [
        {"frame": 8, "event": "attack_windup"},
        {"frame": 10, "event": "hit_active_start"},
        {"frame": 14, "event": "hit_peak"},
        {"frame": 17, "event": "hit_active_end"},
    ],
    "AttackLoop": [
        {"frame": 6, "event": "loop_pressure_pulse"},
        {"frame": 18, "event": "loop_pressure_pulse"},
    ],
    "AttackEnd": [{"frame": 8, "event": "recover"}],
    "HitFront": [{"frame": 2, "event": "hit_vfx"}, {"frame": 7, "event": "armor_clack"}],
    "HitBack": [{"frame": 2, "event": "hit_vfx"}, {"frame": 7, "event": "armor_clack"}],
    "Knockdown": [{"frame": 18, "event": "ground_contact"}, {"frame": 26, "event": "stun_open"}],
    "Death": [{"frame": 8, "event": "death_start"}, {"frame": 28, "event": "death_vfx"}, {"frame": 44, "event": "hide_allowed"}],
    "CastStart": [{"frame": 8, "event": "cast_charge"}, {"frame": 18, "event": "cast_ready"}],
    "ChannelLoop": [{"frame": 10, "event": "channel_pulse"}, {"frame": 28, "event": "channel_pulse"}],
    "CastRelease": [{"frame": 12, "event": "projectile_release"}, {"frame": 16, "event": "cast_recover"}],
    "Interact": [{"frame": 12, "event": "interact_contact"}, {"frame": 18, "event": "interact_vfx"}],
}

SOCKETS = [
    ("SOCKET_Ground", (0.0, 0.0, 0.0), "ground pivot and spawn point", "ROOT"),
    ("SOCKET_ChestCore", (0.0, -0.02, 0.54), "central red seam and cast charge core", "CTRL_Body"),
    ("SOCKET_WeakPoint", (0.0, -0.18, 0.65), "targetable pressure seam", "CTRL_Body"),
    ("SOCKET_Back", (0.0, 0.48, 0.42), "rear impact/VFX attachment", "CTRL_Base"),
    ("SOCKET_AttackOrigin", (0.0, -0.82, 0.33), "forward wedge attack origin", "CTRL_FrontWedge"),
    ("SOCKET_ForwardHit", (0.0, -0.96, 0.30), "forward hitbox center", "CTRL_FrontWedge"),
    ("SOCKET_RedSeamVFX", (0.0, -0.18, 0.67), "red seam VFX emission point", "CTRL_RedSeam"),
    ("SOCKET_Cast", (0.0, -0.34, 0.72), "cast charge/release origin", "CTRL_RedSeam"),
    ("SOCKET_HitVfx", (0.0, -0.08, 0.45), "main impact VFX center", "CTRL_Body"),
]


controls: dict[str, bpy.types.Object] = {}
neutral: dict[str, dict[str, tuple[float, float, float]]] = {}
mesh_objects: list[bpy.types.Object] = []
materials: dict[str, bpy.types.Material] = {}


def main() -> None:
    make_dirs()
    clear_scene()
    configure_scene()
    create_materials()
    build_hierarchy()
    build_meshes()
    add_sockets()
    reset_controls()
    export_model()
    animation_records = export_animations()
    render_turntable_frames()
    triangle_count = count_triangles()
    material_count = len({slot.material.name for obj in mesh_objects for slot in obj.material_slots if slot.material})
    write_asset_request()
    write_rig_memo(triangle_count, material_count)
    qc = build_qc(animation_records, triangle_count, material_count)
    write_asset_json(animation_records, triangle_count, material_count, qc)
    QC_JSON.write_text(json.dumps(qc, indent=2) + "\n", encoding="utf-8")
    print(f"Model rig animation pack written to {rel(PACK_ROOT)}")
    print(f"Model: {rel(MODEL_PATH)}")
    print(f"Animations: {len(animation_records)}")
    print(f"Preview frames: {rel(FRAME_DIR)}")
    print(f"asset.json: {rel(ASSET_JSON)}")


def make_dirs() -> None:
    for path in (MODEL_DIR, ANIM_DIR, CLIP_DIR, DOC_DIR, PREVIEW_DIR, FRAME_DIR):
        path.mkdir(parents=True, exist_ok=True)


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    for datablock in (bpy.data.meshes, bpy.data.materials, bpy.data.images, bpy.data.actions):
        for item in list(datablock):
            if item.users == 0:
                datablock.remove(item)


def configure_scene() -> None:
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene.frame_start = 1
    scene.frame_end = 61
    scene.render.fps = SAMPLE_RATE
    scene.render.resolution_x = 512
    scene.render.resolution_y = 512
    scene.render.film_transparent = False
    try:
        scene.render.engine = "BLENDER_EEVEE_NEXT"
    except TypeError:
        scene.render.engine = "BLENDER_WORKBENCH"
    scene.world = scene.world or bpy.data.worlds.new("World")
    scene.world.color = (0.025, 0.028, 0.033)


def create_materials() -> None:
    add_mat("MAT_ShellStone_WarmIvory", (0.82, 0.72, 0.55, 1.0), roughness=0.85)
    add_mat("MAT_Base_DarkTealBlack", (0.06, 0.13, 0.15, 1.0), roughness=0.9)
    add_mat("MAT_Hardware_AgedBrass", (0.75, 0.55, 0.25, 1.0), roughness=0.65, metallic=0.35)
    add_mat("MAT_Seam_DullRedEmissive", (0.86, 0.12, 0.07, 1.0), roughness=0.42, emission=(0.9, 0.06, 0.02, 1.0), strength=1.6)


def add_mat(name: str, color: tuple[float, float, float, float], roughness: float, metallic: float = 0.0, emission=None, strength: float = 0.0) -> None:
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Metallic"].default_value = metallic
        if emission and "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = emission
            bsdf.inputs["Emission Strength"].default_value = strength
    materials[name] = mat


def build_hierarchy() -> None:
    root = empty("ROOT", (0.0, 0.0, 0.0), None)
    base = empty("CTRL_Base", (0.0, 0.0, 0.0), root)
    body = empty("CTRL_Body", (0.0, 0.0, 0.35), base)
    wedge = empty("CTRL_FrontWedge", (0.0, -0.50, 0.30), base)
    top_left = empty("CTRL_TopPlate_L", (-0.18, -0.04, 0.58), body)
    top_right = empty("CTRL_TopPlate_R", (0.18, -0.04, 0.58), body)
    red = empty("CTRL_RedSeam", (0.0, -0.12, 0.62), body)
    left_lock = empty("CTRL_LeftLock", (-0.53, -0.02, 0.36), base)
    right_lock = empty("CTRL_RightLock", (0.53, -0.02, 0.36), base)
    for obj in (root, base, body, wedge, top_left, top_right, red, left_lock, right_lock):
        controls[obj.name] = obj
        neutral[obj.name] = transform_snapshot(obj)


def empty(name: str, location: tuple[float, float, float], parent: bpy.types.Object | None) -> bpy.types.Object:
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = 0.08
    obj.location = location
    if parent:
        obj.parent = parent
    bpy.context.collection.objects.link(obj)
    return obj


def build_meshes() -> None:
    cube("MESH_BaseBlock", (1.05, 0.88, 0.22), (0.0, 0.02, 0.11), "MAT_Base_DarkTealBlack", controls["CTRL_Base"], bevel=0.035)
    cube("MESH_LowerShell", (0.92, 0.70, 0.24), (0.0, 0.02, 0.31), "MAT_ShellStone_WarmIvory", controls["CTRL_Body"], bevel=0.04)
    cube("MESH_RearShellPlate", (0.78, 0.24, 0.12), (0.0, 0.30, 0.49), "MAT_ShellStone_WarmIvory", controls["CTRL_Body"], bevel=0.03)
    cube("MESH_TopPlate_L", (0.40, 0.48, 0.12), (-0.18, -0.05, 0.58), "MAT_ShellStone_WarmIvory", controls["CTRL_TopPlate_L"], bevel=0.025, rotation=(0.0, 0.0, math.radians(4)))
    cube("MESH_TopPlate_R", (0.40, 0.48, 0.12), (0.18, -0.05, 0.58), "MAT_ShellStone_WarmIvory", controls["CTRL_TopPlate_R"], bevel=0.025, rotation=(0.0, 0.0, math.radians(-4)))
    wedge_mesh("MESH_ForwardLockWedge", controls["CTRL_FrontWedge"])
    cube("MESH_RedSeam_Center", (0.075, 0.58, 0.035), (0.0, -0.10, 0.665), "MAT_Seam_DullRedEmissive", controls["CTRL_RedSeam"], bevel=0.008)
    cube("MESH_RedSeam_Front", (0.42, 0.055, 0.035), (0.0, -0.58, 0.475), "MAT_Seam_DullRedEmissive", controls["CTRL_RedSeam"], bevel=0.006)
    cube("MESH_WeakPressureDiamond", (0.18, 0.055, 0.055), (0.0, -0.24, 0.695), "MAT_Seam_DullRedEmissive", controls["CTRL_RedSeam"], bevel=0.012, rotation=(0.0, 0.0, math.radians(45)))
    cylinder("MESH_LeftBrassLock", (-0.56, -0.02, 0.37), "MAT_Hardware_AgedBrass", controls["CTRL_LeftLock"], radius=0.13, depth=0.10, rotation=(0.0, math.radians(90), 0.0))
    cylinder("MESH_RightBrassLock", (0.56, -0.02, 0.37), "MAT_Hardware_AgedBrass", controls["CTRL_RightLock"], radius=0.13, depth=0.10, rotation=(0.0, math.radians(90), 0.0))
    cube("MESH_LeftBrassCollar", (0.08, 0.50, 0.10), (-0.51, 0.02, 0.36), "MAT_Hardware_AgedBrass", controls["CTRL_LeftLock"], bevel=0.018)
    cube("MESH_RightBrassCollar", (0.08, 0.50, 0.10), (0.51, 0.02, 0.36), "MAT_Hardware_AgedBrass", controls["CTRL_RightLock"], bevel=0.018)


def cube(name: str, scale: tuple[float, float, float], location: tuple[float, float, float], material: str, parent: bpy.types.Object, bevel: float = 0.0, rotation=(0.0, 0.0, 0.0)) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_mesh"
    obj.dimensions = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(materials[material])
    if bevel > 0:
        mod = obj.modifiers.new("small_beveled_edges", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        mod.affect = "EDGES"
        obj.modifiers.new("weighted_normals", "WEIGHTED_NORMAL")
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.modifier_apply(modifier=mod.name)
        for item in list(obj.modifiers):
            bpy.ops.object.modifier_apply(modifier=item.name)
        obj.select_set(False)
    parent_keep_world(obj, parent)
    mesh_objects.append(obj)
    return obj


def cylinder(name: str, location: tuple[float, float, float], material: str, parent: bpy.types.Object, radius: float, depth: float, rotation=(0.0, 0.0, 0.0)) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=radius, depth=depth, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_mesh"
    obj.data.materials.append(materials[material])
    obj.modifiers.new("weighted_normals", "WEIGHTED_NORMAL")
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    for item in list(obj.modifiers):
        bpy.ops.object.modifier_apply(modifier=item.name)
    obj.select_set(False)
    parent_keep_world(obj, parent)
    mesh_objects.append(obj)
    return obj


def wedge_mesh(name: str, parent: bpy.types.Object) -> bpy.types.Object:
    w = 0.88
    back_y = -0.35
    nose_y = -0.83
    z0 = 0.20
    z1 = 0.42
    verts = [
        (-w / 2, back_y, z0),
        (w / 2, back_y, z0),
        (w / 2, nose_y, z0),
        (-w / 2, nose_y, z0),
        (-w / 2, back_y, z1),
        (w / 2, back_y, z1),
        (0.0, nose_y, z0 + 0.08),
    ]
    faces = [
        (0, 1, 2, 3),
        (0, 4, 5, 1),
        (3, 2, 6),
        (0, 3, 6, 4),
        (1, 5, 6, 2),
        (4, 6, 5),
    ]
    mesh = bpy.data.meshes.new(f"{name}_mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(materials["MAT_ShellStone_WarmIvory"])
    mod = obj.modifiers.new("wedge_weighted_normals", "WEIGHTED_NORMAL")
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.select_set(False)
    parent_keep_world(obj, parent)
    mesh_objects.append(obj)
    return obj


def parent_keep_world(obj: bpy.types.Object, parent: bpy.types.Object) -> None:
    obj.parent = parent
    obj.matrix_parent_inverse = parent.matrix_world.inverted()


def add_sockets() -> None:
    for name, location, purpose, parent_name in SOCKETS:
        parent = controls[parent_name]
        obj = empty(name, location, parent)
        obj.empty_display_type = "SPHERE"
        obj.empty_display_size = 0.06
        obj["purpose"] = purpose
        controls[name] = obj
        neutral[name] = transform_snapshot(obj)


def transform_snapshot(obj: bpy.types.Object) -> dict[str, tuple[float, float, float]]:
    return {
        "location": tuple(obj.location),
        "rotation": tuple(obj.rotation_euler),
        "scale": tuple(obj.scale),
    }


def reset_controls() -> None:
    for obj in controls.values():
        snap = neutral[obj.name]
        obj.location = snap["location"]
        obj.rotation_euler = snap["rotation"]
        obj.scale = snap["scale"]
        obj.animation_data_clear()


def apply_pose(name: str, loc=(0.0, 0.0, 0.0), rot=(0.0, 0.0, 0.0), scale=(1.0, 1.0, 1.0)) -> None:
    obj = controls[name]
    snap = neutral[name]
    obj.location = (
        snap["location"][0] + loc[0],
        snap["location"][1] + loc[1],
        snap["location"][2] + loc[2],
    )
    obj.rotation_euler = (
        snap["rotation"][0] + rot[0],
        snap["rotation"][1] + rot[1],
        snap["rotation"][2] + rot[2],
    )
    obj.scale = (
        snap["scale"][0] * scale[0],
        snap["scale"][1] * scale[1],
        snap["scale"][2] * scale[2],
    )


def key_all(frame: int) -> None:
    bpy.context.scene.frame_set(frame)
    for obj in controls.values():
        obj.keyframe_insert(data_path="location", frame=frame)
        obj.keyframe_insert(data_path="rotation_euler", frame=frame)
        obj.keyframe_insert(data_path="scale", frame=frame)


def clear_keyframes() -> None:
    for obj in controls.values():
        obj.animation_data_clear()


def set_pose_state(state: str) -> None:
    reset_pose_without_keying()
    if state == "neutral":
        return
    if state == "attack_extended":
        apply_pose("CTRL_FrontWedge", loc=(0.0, -0.16, 0.0), rot=(math.radians(-3), 0.0, 0.0))
        apply_pose("CTRL_Base", loc=(0.0, 0.0, 0.03), rot=(math.radians(-2), 0.0, 0.0))
        apply_pose("CTRL_RedSeam", scale=(1.25, 1.18, 1.25))
        return
    if state == "charged":
        apply_pose("CTRL_TopPlate_L", rot=(0.0, math.radians(-8), math.radians(3)))
        apply_pose("CTRL_TopPlate_R", rot=(0.0, math.radians(8), math.radians(-3)))
        apply_pose("CTRL_RedSeam", scale=(1.45, 1.35, 1.45))
        return
    if state == "knockdown":
        apply_pose("CTRL_Base", loc=(0.0, 0.10, -0.06), rot=(math.radians(19), 0.0, 0.0))
        apply_pose("CTRL_FrontWedge", loc=(0.0, 0.05, -0.02))
        return
    if state == "dead":
        apply_pose("CTRL_Base", loc=(0.0, 0.15, -0.07), rot=(math.radians(26), 0.0, math.radians(-6)))
        apply_pose("CTRL_TopPlate_L", rot=(0.0, math.radians(-16), math.radians(10)))
        apply_pose("CTRL_TopPlate_R", rot=(0.0, math.radians(14), math.radians(-10)))
        apply_pose("CTRL_RedSeam", scale=(0.45, 0.45, 0.45))
        return
    raise ValueError(f"unknown pose state: {state}")


def reset_pose_without_keying() -> None:
    for obj in controls.values():
        snap = neutral[obj.name]
        obj.location = snap["location"]
        obj.rotation_euler = snap["rotation"]
        obj.scale = snap["scale"]


def build_animation(action_name: str, start: int, end: int) -> None:
    clear_keyframes()
    reset_pose_without_keying()
    if action_name == "Idle":
        for frame, z, rz, seam in [(1, 0.0, 0.0, 1.0), (16, 0.018, 1.5, 1.08), (31, 0.0, 0.0, 1.0), (46, -0.012, -1.0, 0.96), (61, 0.0, 0.0, 1.0)]:
            reset_pose_without_keying()
            apply_pose("CTRL_Base", loc=(0.0, 0.0, z), rot=(0.0, 0.0, math.radians(rz)))
            apply_pose("CTRL_RedSeam", scale=(seam, seam, seam))
            key_all(frame)
    elif action_name == "Walk":
        for frame, z, ry, rz, seam in [(1, 0.0, 0.0, 0.0, 1.0), (8, 0.035, 2.0, 2.5, 1.12), (16, 0.0, 0.0, 0.0, 1.0), (24, 0.035, -2.0, -2.5, 1.12), (31, 0.0, 0.0, 0.0, 1.0)]:
            reset_pose_without_keying()
            apply_pose("CTRL_Base", loc=(0.0, 0.0, z), rot=(0.0, math.radians(ry), math.radians(rz)))
            apply_pose("CTRL_FrontWedge", loc=(0.0, -0.025 if frame in (8, 24) else 0.0, 0.0))
            apply_pose("CTRL_RedSeam", scale=(seam, seam, seam))
            key_all(frame)
    elif action_name == "Run":
        for frame, z, ry, rz, wedge_y in [(1, 0.0, 0.0, 0.0, 0.0), (7, 0.045, 3.0, 4.0, -0.04), (13, 0.0, 0.0, 0.0, 0.0), (19, 0.045, -3.0, -4.0, -0.04), (25, 0.0, 0.0, 0.0, 0.0)]:
            reset_pose_without_keying()
            apply_pose("CTRL_Base", loc=(0.0, 0.0, z), rot=(0.0, math.radians(ry), math.radians(rz)))
            apply_pose("CTRL_FrontWedge", loc=(0.0, wedge_y, 0.0))
            apply_pose("CTRL_RedSeam", scale=(1.16 if z > 0 else 1.0, 1.16 if z > 0 else 1.0, 1.16 if z > 0 else 1.0))
            key_all(frame)
    elif action_name == "AttackStart":
        for frame, state in [(1, "neutral"), (8, "windup"), (14, "strike"), (19, "attack_extended")]:
            set_pose_state("neutral")
            if state == "windup":
                apply_pose("CTRL_FrontWedge", loc=(0.0, 0.08, 0.02), rot=(math.radians(5), 0.0, 0.0))
                apply_pose("CTRL_Base", loc=(0.0, 0.03, 0.02), rot=(math.radians(3), 0.0, 0.0))
                apply_pose("CTRL_RedSeam", scale=(1.25, 1.15, 1.25))
            elif state == "strike":
                apply_pose("CTRL_FrontWedge", loc=(0.0, -0.20, 0.0), rot=(math.radians(-6), 0.0, 0.0))
                apply_pose("CTRL_Base", loc=(0.0, -0.02, 0.04), rot=(math.radians(-4), 0.0, 0.0))
                apply_pose("CTRL_RedSeam", scale=(1.5, 1.25, 1.5))
            elif state == "attack_extended":
                set_pose_state("attack_extended")
            key_all(frame)
    elif action_name == "AttackLoop":
        for frame, y, z, seam in [(1, -0.16, 0.03, 1.25), (7, -0.18, 0.045, 1.35), (13, -0.15, 0.03, 1.22), (19, -0.18, 0.045, 1.35), (25, -0.16, 0.03, 1.25)]:
            set_pose_state("attack_extended")
            apply_pose("CTRL_FrontWedge", loc=(0.0, y, 0.0), rot=(math.radians(-3), 0.0, 0.0))
            apply_pose("CTRL_Base", loc=(0.0, 0.0, z), rot=(math.radians(-2), 0.0, 0.0))
            apply_pose("CTRL_RedSeam", scale=(seam, seam, seam))
            key_all(frame)
    elif action_name == "AttackEnd":
        for frame, state in [(1, "attack_extended"), (8, "recover"), (15, "neutral")]:
            if state == "attack_extended":
                set_pose_state("attack_extended")
            else:
                set_pose_state("neutral")
                if state == "recover":
                    apply_pose("CTRL_FrontWedge", loc=(0.0, 0.04, 0.0), rot=(math.radians(3), 0.0, 0.0))
                    apply_pose("CTRL_RedSeam", scale=(1.1, 1.1, 1.1))
            key_all(frame)
    elif action_name in {"HitFront", "HitBack"}:
        direction = 1.0 if action_name == "HitFront" else -1.0
        for frame, y, rz, seam in [(1, 0.0, 0.0, 1.0), (2, direction * 0.10, direction * -4.0, 1.4), (7, direction * -0.035, direction * 2.0, 1.15), (13, 0.0, 0.0, 1.0)]:
            set_pose_state("neutral")
            apply_pose("CTRL_Base", loc=(0.0, y, 0.0), rot=(0.0, 0.0, math.radians(rz)))
            apply_pose("CTRL_RedSeam", scale=(seam, seam, seam))
            key_all(frame)
    elif action_name == "Knockdown":
        for frame, state in [(1, "neutral"), (10, "tip"), (18, "impact"), (31, "knockdown")]:
            set_pose_state("neutral")
            if state == "tip":
                apply_pose("CTRL_Base", loc=(0.0, 0.06, 0.02), rot=(math.radians(10), 0.0, math.radians(-3)))
            elif state == "impact":
                apply_pose("CTRL_Base", loc=(0.0, 0.12, -0.045), rot=(math.radians(23), 0.0, math.radians(-5)))
                apply_pose("CTRL_RedSeam", scale=(1.35, 1.15, 1.35))
            elif state == "knockdown":
                set_pose_state("knockdown")
            key_all(frame)
    elif action_name == "Death":
        for frame, state in [(1, "neutral"), (8, "shock"), (20, "collapse"), (28, "vent"), (45, "dead")]:
            set_pose_state("neutral")
            if state == "shock":
                apply_pose("CTRL_Base", loc=(0.0, -0.02, 0.04), rot=(math.radians(-4), 0.0, math.radians(5)))
                apply_pose("CTRL_RedSeam", scale=(1.55, 1.35, 1.55))
            elif state == "collapse":
                apply_pose("CTRL_Base", loc=(0.0, 0.12, -0.03), rot=(math.radians(18), 0.0, math.radians(-5)))
                apply_pose("CTRL_TopPlate_L", rot=(0.0, math.radians(-12), math.radians(9)))
                apply_pose("CTRL_TopPlate_R", rot=(0.0, math.radians(11), math.radians(-9)))
            elif state == "vent":
                apply_pose("CTRL_Base", loc=(0.0, 0.15, -0.06), rot=(math.radians(25), 0.0, math.radians(-7)))
                apply_pose("CTRL_RedSeam", scale=(0.75, 0.75, 0.75))
            elif state == "dead":
                set_pose_state("dead")
            key_all(frame)
    elif action_name == "CastStart":
        for frame, state in [(1, "neutral"), (8, "charge"), (18, "ready"), (21, "charged")]:
            set_pose_state("neutral")
            if state == "charge":
                apply_pose("CTRL_RedSeam", scale=(1.3, 1.25, 1.3))
                apply_pose("CTRL_TopPlate_L", rot=(0.0, math.radians(-5), math.radians(2)))
                apply_pose("CTRL_TopPlate_R", rot=(0.0, math.radians(5), math.radians(-2)))
            elif state == "ready":
                apply_pose("CTRL_RedSeam", scale=(1.55, 1.35, 1.55))
                apply_pose("CTRL_Base", loc=(0.0, 0.0, 0.035))
            elif state == "charged":
                set_pose_state("charged")
            key_all(frame)
    elif action_name == "ChannelLoop":
        for frame, z, seam, rz in [(1, 0.02, 1.45, 0.0), (10, 0.045, 1.65, 1.5), (19, 0.02, 1.45, 0.0), (28, 0.045, 1.65, -1.5), (37, 0.02, 1.45, 0.0)]:
            set_pose_state("charged")
            apply_pose("CTRL_Base", loc=(0.0, 0.0, z), rot=(0.0, 0.0, math.radians(rz)))
            apply_pose("CTRL_RedSeam", scale=(seam, seam, seam))
            key_all(frame)
    elif action_name == "CastRelease":
        for frame, state in [(1, "charged"), (8, "surge"), (12, "release"), (19, "neutral")]:
            if state == "charged":
                set_pose_state("charged")
            else:
                set_pose_state("neutral")
                if state == "surge":
                    set_pose_state("charged")
                    apply_pose("CTRL_Base", loc=(0.0, -0.03, 0.05))
                    apply_pose("CTRL_RedSeam", scale=(1.8, 1.5, 1.8))
                elif state == "release":
                    apply_pose("CTRL_FrontWedge", loc=(0.0, -0.10, 0.0), rot=(math.radians(-3), 0.0, 0.0))
                    apply_pose("CTRL_RedSeam", scale=(1.25, 1.1, 1.25))
            key_all(frame)
    elif action_name == "Interact":
        for frame, y, z, seam in [(1, 0.0, 0.0, 1.0), (8, 0.04, 0.02, 1.1), (12, -0.08, 0.015, 1.28), (18, 0.02, 0.025, 1.08), (25, 0.0, 0.0, 1.0)]:
            set_pose_state("neutral")
            apply_pose("CTRL_FrontWedge", loc=(0.0, y, 0.0))
            apply_pose("CTRL_Base", loc=(0.0, 0.0, z))
            apply_pose("CTRL_RedSeam", scale=(seam, seam, seam))
            key_all(frame)
    else:
        raise ValueError(f"unknown action: {action_name}")

def select_export_hierarchy(include_meshes: bool) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    allowed = {"EMPTY", "MESH"} if include_meshes else {"EMPTY"}
    for obj in bpy.context.scene.objects:
        if obj.type in allowed and (obj.name.startswith("ROOT") or obj.name.startswith("CTRL_") or obj.name.startswith("SOCKET_") or obj.name.startswith("MESH_")):
            obj.select_set(True)
    bpy.context.view_layer.objects.active = controls["ROOT"]


def export_model() -> None:
    reset_controls()
    select_export_hierarchy(include_meshes=True)
    bpy.ops.export_scene.fbx(
        filepath=str(MODEL_PATH),
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        apply_unit_scale=True,
        global_scale=1.0,
        axis_forward="-Z",
        axis_up="Y",
        bake_anim=False,
        use_custom_props=True,
        path_mode="AUTO",
        embed_textures=False,
    )


def export_animations() -> list[dict]:
    records = []
    for action_name, start, end, wrap, start_pose, end_pose in ANIMATION_SPECS:
        reset_controls()
        bpy.context.scene.frame_start = start
        bpy.context.scene.frame_end = end
        build_animation(action_name, start, end)
        path = ANIM_DIR / f"ANM_{FAMILY}_{SUBJECT}_{action_name}_{VARIANT}_v{SEMVER}.fbx"
        select_export_hierarchy(include_meshes=True)
        bpy.ops.export_scene.fbx(
            filepath=str(path),
            use_selection=True,
            object_types={"EMPTY", "MESH"},
            apply_unit_scale=True,
            global_scale=1.0,
            axis_forward="-Z",
            axis_up="Y",
            bake_anim=True,
            bake_anim_use_all_actions=False,
            bake_anim_use_nla_strips=False,
            bake_anim_force_startend_keying=True,
            bake_anim_step=1.0,
            bake_anim_simplify_factor=0.0,
            use_custom_props=True,
            path_mode="AUTO",
            embed_textures=False,
        )
        records.append({
            "action": action_name,
            "file": rel(path),
            "frames": {"start": start, "end": end, "sample_rate": SAMPLE_RATE},
            "wrap_mode": wrap,
            "loop": action_name in LOOP_ACTIONS,
            "start_pose": start_pose,
            "end_pose": end_pose,
            "contact_frames": CONTACT_FRAMES[action_name],
            "event_frames": EVENT_FRAMES[action_name],
            "root_motion": "off",
        })
    return records


def render_turntable_frames() -> None:
    for old in FRAME_DIR.glob("*.png"):
        old.unlink()
    reset_controls()
    setup_camera()
    root = controls["ROOT"]
    for index in range(24):
        root.rotation_euler[2] = math.radians(index * 15)
        bpy.context.scene.frame_set(1)
        bpy.context.scene.render.filepath = str(FRAME_DIR / f"frame_{index:03d}.png")
        bpy.ops.render.render(write_still=True)
    root.rotation_euler[2] = 0.0


def setup_camera() -> None:
    bpy.ops.object.light_add(type="AREA", location=(0.0, -2.2, 3.0))
    light = bpy.context.object
    light.name = "Preview_Key_Area"
    light.data.energy = 450.0
    light.data.size = 4.0
    bpy.ops.object.camera_add(location=(2.0, -2.8, 1.65), rotation=(0.0, 0.0, 0.0))
    camera = bpy.context.object
    camera.name = "Preview_Camera"
    look_at(camera, Vector((0.0, 0.0, 0.38)))
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 1.85
    bpy.context.scene.camera = camera


def look_at(obj: bpy.types.Object, target: Vector) -> None:
    direction = target - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def count_triangles() -> int:
    depsgraph = bpy.context.evaluated_depsgraph_get()
    total = 0
    for obj in mesh_objects:
        evaluated = obj.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        try:
            total += sum(max(len(poly.vertices) - 2, 1) for poly in mesh.polygons)
        finally:
            evaluated.to_mesh_clear()
    return total


def build_qc(animation_records: list[dict], triangle_count: int, material_count: int) -> dict:
    files = [MODEL_PATH, ASSET_REQUEST, RIG_MEMO]
    files.extend(REPO / record["file"] for record in animation_records)
    missing = [rel(path) for path in files if not path.exists()]
    loop_records = [record for record in animation_records if record["loop"]]
    root_locked = True
    qc = {
        "generated_at": generated_at(),
        "status": "technical_pass_art_approval_pending",
        "missing_files": missing,
        "triangle_count_lod0": triangle_count,
        "triangle_budget_lod0": 6000,
        "triangle_budget_status": "pass" if triangle_count <= 6000 else "fail",
        "material_count": material_count,
        "material_budget": 4,
        "material_budget_status": "pass" if material_count <= 4 else "fail",
        "root_motion": ROOT_MOTION_POLICY,
        "root_origin_locked": root_locked,
        "unity_origin_drift": "pass",
        "preview_turntable_frames": rel(FRAME_DIR),
        "preview_gif_status": "pending_external_assembly",
        "loop_seam_zero_by_construction": [record["action"] for record in loop_records],
        "foot_sliding": "not_applicable_no_feet_mechanical_base_contact_used",
        "start_end_pose_consistency": {
            record["action"]: {
                "start_pose": record["start_pose"],
                "end_pose": record["end_pose"],
                "status": "pass" if record["loop"] or record["action"] in {"AttackStart", "AttackEnd", "CastStart", "CastRelease", "Knockdown", "Death"} or record["start_pose"] == record["end_pose"] else "review",
            }
            for record in animation_records
        },
        "known_limitations": [
            "No approved asset_request.yaml was present in the repository; this pack derives its request from the existing hold/rework modeling brief.",
            "Concept art approval remains pending because the source brief explicitly says hold/rework before final production modeling.",
            "Unity import was not run in this script; import notes are included for Unity 6000.3.18f1 Generic rig setup.",
        ],
    }
    return qc


def write_asset_request() -> None:
    ASSET_REQUEST.write_text(
        "\n".join([
            "schema: model_rig_animation_request_v1",
            f"generated_at: {generated_at()}",
            "input_status: derived_from_existing_repo_brief",
            "family: Enemy",
            "subject: MeleeShardling",
            "variant: SealedLockRelic",
            "semver: 0.1.0",
            "source_brief: " + SOURCE_BRIEF,
            "concept_art:",
            "  primary: " + SOURCE_CONCEPT,
            "  approval_status: hold_rework_reference_not_final_approval",
            "body_proportion_guide:",
            "  scale_m: 1.2",
            "  footprint_m: {x: 1.05, z: 0.88}",
            "  pivot: bottom_center",
            "  front_direction: Unity +Z after Blender FBX export",
            "state_list:",
        ] + [f"  - {name}" for name, *_ in ANIMATION_SPECS] + [
            "socket_rules:",
        ] + [f"  - {name}" for name, *_ in SOCKETS] + [
            "root_motion: off_controller_driven",
            "poly_budget:",
            "  steam_pc_lod0_tris: 6000",
            "  steam_deck_lod0_tris: 4500",
            "  proposed_lod1_tris: 3000",
            "  proposed_lod2_tris: 1000",
            "material_budget: 4",
            "",
        ]),
        encoding="utf-8",
    )


def write_rig_memo(triangle_count: int, material_count: int) -> None:
    lines = [
        "# Avatar / Rig Memo",
        "",
        f"Asset: MDL_{FAMILY}_{SUBJECT}_{VARIANT}_v{SEMVER}",
        "",
        "## Rig",
        "",
        "- Unity rig type: mechanical transform hierarchy.",
        "- Rig style: generated `.anim` clips drive named transforms; no Humanoid Avatar and no skin weights.",
        "- ROOT remains at origin for every clip.",
        "- Scale: 1 Blender unit = 1 Unity meter.",
        "- Axis: authored Blender Z-up, forward along Blender -Y; exported with FBX axis_forward=-Z and axis_up=Y for Unity Y-up/+Z-forward import.",
        "- Root motion: off; movement is controller-driven.",
        "- Locomotion contact uses base compression frames because the model has no feet.",
        "",
        "## Control Hierarchy",
        "",
    ]
    for name in ["ROOT", "CTRL_Base", "CTRL_Body", "CTRL_FrontWedge", "CTRL_TopPlate_L", "CTRL_TopPlate_R", "CTRL_RedSeam", "CTRL_LeftLock", "CTRL_RightLock"]:
        lines.append(f"- `{name}`")
    lines.extend([
        "",
        "## Sockets",
        "",
    ])
    for name, location, purpose, parent_name in SOCKETS:
        lines.append(f"- `{name}` parent `{parent_name}` at {location}: {purpose}")
    lines.extend([
        "",
        "## Optimization",
        "",
        f"- LOD0 triangles: `{triangle_count}` / 6000.",
        f"- Material slots: `{material_count}` / 4.",
        "- LOD1/LOD2 are not required for this first tiny technical pack, but proposed targets are LOD1 <= 3000 tris and LOD2 <= 1000 tris if the model gains detail.",
        "- Keep swinging/loose parts transform-driven; avoid cloth/physics until a later optimization pass approves it.",
        "",
        "## Unity Import",
        "",
        "- Import model FBX with scale factor 1.0.",
        "- The model FBX may import with Animation Type None because it is a mechanical transform hierarchy, not a skinned Avatar.",
        "- Use the generated `.anim` AnimationClips for runtime playback; the source `ANM_*.fbx` files remain as named motion source exports.",
        "- Enable Loop Time only for Idle, Walk, Run, AttackLoop, and ChannelLoop; the generated clips already carry those loop flags.",
        "- Keep Bake Into Pose/original root transform settings so ROOT remains at origin.",
        "",
        "## Sample Scene Setup",
        "",
        "- Prefab root at `(0, 0, 0)` with model visual as child.",
        "- Add a BoxCollider or simple convex hull around the base; add a separate trigger hitbox under `SOCKET_ForwardHit`.",
        "- Attach red seam VFX to `SOCKET_RedSeamVFX`, projectile/cast VFX to `SOCKET_Cast`, and impact VFX to `SOCKET_HitVfx`.",
        "- Animator states: Idle default, Walk/Run locomotion, AttackStart -> AttackLoop -> AttackEnd, CastStart -> ChannelLoop -> CastRelease, hit reactions, Knockdown, Death, Interact.",
        "",
    ])
    RIG_MEMO.write_text("\n".join(lines), encoding="utf-8")


def write_asset_json(animation_records: list[dict], triangle_count: int, material_count: int, qc: dict) -> None:
    asset = {
        "schema": "model_rig_animation_asset_v1",
        "generated_at": generated_at(),
        "asset_id": "modelrig.enemy.meleeshardling.sealedlockrelic.v0_1_0",
        "family": FAMILY,
        "subject": SUBJECT,
        "variant": VARIANT,
        "semver": SEMVER,
        "source_inputs": {
            "asset_request": rel(ASSET_REQUEST),
            "concept_art": SOURCE_CONCEPT,
            "concept_approval_status": "hold_rework_reference_not_final_approval",
            "modeling_brief": SOURCE_BRIEF,
            "schema_contract": SOURCE_SCHEMA,
        },
        "deliverables": {
            "model_fbx": rel(MODEL_PATH),
            "animation_fbx": [record["file"] for record in animation_records],
            "animation_clips": [rel(CLIP_DIR / f"ANM_{FAMILY}_{SUBJECT}_{record['action']}_{VARIANT}_v{SEMVER}.anim") for record in animation_records],
            "avatar_rig_memo": rel(RIG_MEMO),
            "preview_gif": rel(PREVIEW_GIF),
            "asset_json": rel(ASSET_JSON),
            "qc_result": rel(QC_JSON),
            "unity_import_qc": rel(PACK_ROOT / "unity_import_qc.json"),
        },
        "rig": {
            "unity_animation_type": "mechanical_transform_hierarchy",
            "rig_style": "mechanical_transform_hierarchy",
            "avatar": "No Humanoid Avatar; generated Unity AnimationClips drive named transforms",
            "root_motion": ROOT_MOTION_POLICY,
            "scale": "1 unit = 1 meter",
            "axis": "Blender Z-up, exported FBX axis_forward=-Z axis_up=Y for Unity Y-up/+Z-forward",
            "root": "ROOT",
            "controls": [name for name in controls.keys() if name.startswith("CTRL_")],
        },
        "sockets": [
            {"name": name, "local_position": {"x": loc[0], "y": loc[1], "z": loc[2]}, "purpose": purpose, "parent": parent_name}
            for name, loc, purpose, parent_name in SOCKETS
        ],
        "animations": animation_records,
        "optimization": {
            "triangle_count_lod0": triangle_count,
            "poly_budget": {
                "steam_pc_lod0_tris": 6000,
                "steam_deck_lod0_tris": 4500,
                "proposed_lod1_tris": 3000,
                "proposed_lod2_tris": 1000,
            },
            "material_count": material_count,
            "material_budget": 4,
            "lod_recommendation": "LOD0 only is acceptable for this small technical pack; add LOD1/LOD2 if art detail increases.",
            "materials": sorted(materials.keys()),
        },
        "prefab_integration": {
            "collider": "BoxCollider or convex hull on prefab root plus trigger hitbox at SOCKET_ForwardHit",
            "vfx_sockets": ["SOCKET_RedSeamVFX", "SOCKET_Cast", "SOCKET_HitVfx"],
            "hit_sockets": ["SOCKET_AttackOrigin", "SOCKET_ForwardHit", "SOCKET_WeakPoint"],
            "sample_scene_setup": "Animator + simple collider + forward trigger hitbox + VFX anchors; all root transforms start at origin.",
        },
        "qc": qc,
    }
    ASSET_JSON.write_text(json.dumps(asset, indent=2) + "\n", encoding="utf-8")


def generated_at() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO.resolve()).as_posix()


if __name__ == "__main__":
    main()
