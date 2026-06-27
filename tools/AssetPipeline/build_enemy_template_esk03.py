#!/usr/bin/env python3
"""Build ESK-03 neutral quadruped charger enemy skeleton template."""

from __future__ import annotations

import importlib.util
import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw


REPO = Path(__file__).resolve().parents[2]
GENERATOR_PATH = REPO / "tools" / "AssetPipeline" / "generate_production_model_pack.py"
TARGET_NAME = "FE_ENEMY_TEMPLATE_ESK03_Quadruped_01"
TURNAROUND_SUFFIX = "_Turnaround.png"
SOCKET_REPORT = REPO / "artifacts" / "Reports" / "enemy-template-esk03-quadruped-sockets.json"
REPORT_MD = REPO / "artifacts" / "Reports" / "enemy-template-esk03-quadruped-20260627.md"
CONCEPT_CROP = REPO / "artifacts" / "Concepts" / "EnemySkeletonTaxonomy" / "Batch_20260627" / "Crops_v001" / "ESK-03_quadruped_beast.png"


def socket(name: str, x: float, y: float, z: float, purpose: str) -> dict:
    return {"name": name, "position": {"x": x, "y": y, "z": z}, "purpose": purpose}


SOCKET_PLAN = [
    socket("SOCKET_Ground", 0.000, 0.000, 0.000, "bottom-center pivot and four-foot grounding reference"),
    socket("SOCKET_Head", 0.000, 0.730, 0.705, "head, mask, tusk, or crest swap"),
    socket("SOCKET_Mouth", 0.000, 0.730, 0.925, "bite/spit point; separate from charge hitbox"),
    socket("SOCKET_AttackOrigin", 0.000, 0.735, 1.030, "charge/ram hitbox origin"),
    socket("SOCKET_ChestCore", 0.000, 0.625, 0.420, "front chest core and charger read"),
    socket("SOCKET_WeakPoint", 0.225, 0.680, -0.075, "side/back targetable weak point"),
    socket("SOCKET_Back", 0.000, 0.690, -0.080, "saddle armor, mane, crest, or shell attachment"),
    socket("SOCKET_Tail", 0.000, 0.645, -0.690, "tail, tail club, or rear VFX attachment"),
    socket("SOCKET_LeftHand", -0.210, 0.120, 0.520, "front-left paw/claw attachment"),
    socket("SOCKET_RightHand", 0.210, 0.120, 0.520, "front-right paw/claw attachment"),
    socket("SOCKET_Cast", 0.000, 0.760, 0.875, "telegraph marker during charge windup"),
    socket("SOCKET_HitVfx", 0.000, 0.600, 0.100, "body impact center"),
]

ANIMATION_CLIPS = [
    {"name": "idle_loop", "frames": 54, "notes": "subtle breathing; four paws remain planted"},
    {"name": "move_loop", "frames": 28, "notes": "trot loop with stable foot contacts"},
    {"name": "turn_in_place", "frames": 24, "notes": "short quadruped pivot without foot sliding"},
    {"name": "charge_windup", "frames": 24, "notes": "front mass lowers and braces; attack tell visible by frame 8"},
    {"name": "charge", "frames": 20, "notes": "active hitbox from SOCKET_AttackOrigin"},
    {"name": "skid_recovery", "frames": 26, "notes": "charge end skid; no instant turn"},
    {"name": "attack_b", "frames": 30, "notes": "short bite/body bump alternate attack"},
    {"name": "hit_light", "frames": 12, "notes": "small side recoil, paws keep readable contact"},
    {"name": "hit_heavy", "frames": 22, "notes": "strong recoil, weak point flashes"},
    {"name": "stagger", "frames": 38, "notes": "brief exposed weak-point state"},
    {"name": "death", "frames": 46, "notes": "collapse onto side without gore"},
    {"name": "spawn", "frames": 32, "notes": "enter from crouch/ground socket"},
]


def main() -> None:
    generator = load_generator()
    specs = generator.load_specs()
    spec = next((item for item in specs if item.name == TARGET_NAME), None)
    if spec is None:
        raise RuntimeError(f"Missing AssetSpec for {TARGET_NAME}")

    mesh = generator.build_mesh(spec)
    generator.trim_materials(mesh, spec.material_budget)

    model_file = generator.model_path(spec)
    preview_file = generator.PREVIEW_DIR / f"{spec.name}.png"
    turnaround_file = generator.PREVIEW_DIR / f"{spec.name}{TURNAROUND_SUFFIX}"
    model_file.parent.mkdir(parents=True, exist_ok=True)
    preview_file.parent.mkdir(parents=True, exist_ok=True)
    generator.write_obj(mesh, model_file, spec.name)
    generator.render_preview(mesh, preview_file, spec)
    render_turnaround(mesh, turnaround_file)

    socket_payload = {
        "asset": TARGET_NAME,
        "skeleton_category": "ESK-03",
        "movement_mode": "Run/charge quadruped",
        "collider_profile": "capsule_actor",
        "collider_contract": "root capsule covers main body only; charge and weak-point volumes are separate runtime hitboxes derived from sockets",
        "socket_plan": SOCKET_PLAN,
        "minimum_animation_clips": ANIMATION_CLIPS,
    }
    SOCKET_REPORT.write_text(json.dumps(socket_payload, indent=2) + "\n", encoding="utf-8")

    record = generator.make_record(spec, mesh, model_file, preview_file)
    record.update({
        "source_file": generator.rel(Path(__file__)),
        "source_reference": generator.rel(Path(__file__)),
        "style_gate_status": "enemy_skeleton_template_candidate",
        "acceptance_status": "enemy_skeleton_template_requires_human_art_and_animation_review",
        "skeleton_category_id": "ESK-03",
        "skeleton_template_name": "Quadruped charger",
        "template_scope": "enemy_monster_only",
        "excluded_scope": "playable_and_friendly_npc_mannequin",
        "movement_mode": "Run/charge quadruped",
        "socket_plan": SOCKET_PLAN,
        "socket_report": generator.rel(SOCKET_REPORT),
        "minimum_animation_clips": ANIMATION_CLIPS,
        "turnaround_preview_file": generator.rel(turnaround_file),
        "concept_seed": generator.rel(CONCEPT_CROP),
        "collider_contract": "capsule_actor on root for body; charge and weak-point hitboxes derive from sockets, not root collider",
        "quality_revision": 1,
        "quality_notes": [
            "Neutral quadruped charger skeleton template, not a finished beast.",
            "Purple crystal/crest details from the concept are reserved for optional parts, not base identity.",
            "Designed to scale into charger/beast variants by swapping head, tail, back, paw, armor, and weak-core parts.",
        ],
    })

    manifest = json.loads(generator.MANIFEST_FILE.read_text(encoding="utf-8"))
    assets = manifest.get("assets")
    if not isinstance(assets, list):
        raise RuntimeError(f"Invalid manifest assets array: {generator.MANIFEST_FILE}")

    for index, asset in enumerate(assets):
        if asset.get("name") == TARGET_NAME:
            assets[index] = record
            break
    else:
        assets.insert(0, record)

    manifest["assets"] = assets
    manifest["consistency_summary"] = generator.consistency_summary(assets)
    manifest["enemy_skeleton_templates"] = {
        **manifest.get("enemy_skeleton_templates", {}),
        "ESK-03": {
            "target": TARGET_NAME,
            "status": "template_candidate_importable",
            "model_file": generator.rel(model_file),
            "preview_file": generator.rel(preview_file),
            "turnaround_preview_file": generator.rel(turnaround_file),
            "socket_report": generator.rel(SOCKET_REPORT),
            "triangle_count": record["triangles_lod0"],
            "primitive_counts": record["primitive_counts"],
        },
    }
    generator.MANIFEST_FILE.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    write_report(generator, record, model_file, preview_file, turnaround_file)

    print(f"Built {TARGET_NAME}")
    print(f"Model: {generator.rel(model_file)}")
    print(f"Preview: {generator.rel(preview_file)}")
    print(f"Turnaround: {generator.rel(turnaround_file)}")
    print(f"Socket report: {generator.rel(SOCKET_REPORT)}")
    print(f"Triangles: {record['triangles_lod0']}")
    print(f"Primitive counts: {record['primitive_counts']}")


def render_turnaround(mesh, path: Path) -> None:
    panels = [
        ("Front", lambda vertex: (vertex[0], vertex[2], vertex[1])),
        ("Side", lambda vertex: (-vertex[1], vertex[2], vertex[0])),
        ("Back", lambda vertex: (-vertex[0], vertex[2], -vertex[1])),
        ("Iso", lambda vertex: ((vertex[0] - vertex[1]) * 0.86, (vertex[0] + vertex[1]) * 0.48 - vertex[2] * 0.92, vertex[0] + vertex[1] + vertex[2])),
    ]
    panel_w, panel_h = 360, 500
    image = Image.new("RGB", (panel_w * len(panels), panel_h), (18, 21, 27))
    draw = ImageDraw.Draw(image, "RGBA")

    for panel_index, (label, projector) in enumerate(panels):
        transformed = [projector(vertex) for vertex in mesh.vertices]
        xs = [point[0] for point in transformed]
        ys = [point[1] for point in transformed]
        min_x, max_x = min(xs), max(xs)
        min_y, max_y = min(ys), max(ys)
        scale = min(255 / max(max_x - min_x, 0.1), 390 / max(max_y - min_y, 0.1))
        center_x = (min_x + max_x) * 0.5
        bottom_y = panel_h - 32
        offset_x = panel_index * panel_w + panel_w * 0.5

        face_items = []
        for material, face in mesh.faces:
            points = [transformed[index - 1] for index in face]
            depth = sum(point[2] for point in points) / len(points)
            points_2d = [
                ((point[0] - center_x) * scale + offset_x, bottom_y - (point[1] - min_y) * scale)
                for point in points
            ]
            face_items.append((depth, material, points_2d))
        for _, material, points_2d in sorted(face_items):
            color = (*mesh.materials[material], 240)
            draw.polygon(points_2d, fill=color, outline=(0, 0, 0, 20))
        draw.text((panel_index * panel_w + 18, 18), label, fill=(230, 234, 226))

    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def write_report(generator, record: dict, model_file: Path, preview_file: Path, turnaround_file: Path) -> None:
    lines = [
        "# Enemy Template ESK-03 Quadruped - 2026-06-27",
        "",
        "## Asset",
        "",
        f"- Asset: `{TARGET_NAME}`",
        f"- Model: `{generator.rel(model_file)}`",
        f"- Prefab: `{record['unity_prefab']}`",
        f"- Preview: `{generator.rel(preview_file)}`",
        f"- Turnaround: `{generator.rel(turnaround_file)}`",
        f"- Socket report: `{generator.rel(SOCKET_REPORT)}`",
        f"- Concept seed crop: `{generator.rel(CONCEPT_CROP)}`",
        "",
        "## Scope",
        "",
        "- Neutral quadruped enemy skeleton template only.",
        "- Not a finished beast, wolf, boar, lizard, dragon, or crystal monster.",
        "- Future variants should scale or swap head, tail, back, paw/claw, armor, and weak-core parts.",
        "",
        "## Geometry",
        "",
        f"- Triangles: `{record['triangles_lod0']}`",
        f"- Primitive counts: `{record['primitive_counts']}`",
        f"- Collider: `{record['collision_profile']}`",
        f"- Movement mode: `{record['movement_mode']}`",
        "",
        "## Sockets",
        "",
    ]
    for item in SOCKET_PLAN:
        p = item["position"]
        lines.append(f"- `{item['name']}`: ({p['x']}, {p['y']}, {p['z']}) - {item['purpose']}")
    lines.extend([
        "",
        "## Minimum Animation Clips",
        "",
    ])
    for clip in ANIMATION_CLIPS:
        lines.append(f"- `{clip['name']}`: {clip['frames']} frames - {clip['notes']}")
    REPORT_MD.write_text("\n".join(lines) + "\n", encoding="utf-8")


def load_generator():
    spec = importlib.util.spec_from_file_location("fourfold_model_pack_generator", GENERATOR_PATH)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Could not load generator module: {GENERATOR_PATH}")
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


if __name__ == "__main__":
    main()
