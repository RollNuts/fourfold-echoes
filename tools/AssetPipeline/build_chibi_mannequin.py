#!/usr/bin/env python3
"""Build the shared playable/NPC chibi mannequin without touching other models."""

from __future__ import annotations

import importlib.util
import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw


REPO = Path(__file__).resolve().parents[2]
GENERATOR_PATH = REPO / "tools" / "AssetPipeline" / "generate_production_model_pack.py"
TARGET_NAME = "FE_CHAR_TEMPLATE_ChibiMannequin_01"


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
    turnaround_file = generator.PREVIEW_DIR / f"{spec.name}_Turnaround.png"
    model_file.parent.mkdir(parents=True, exist_ok=True)
    preview_file.parent.mkdir(parents=True, exist_ok=True)
    generator.write_obj(mesh, model_file, spec.name)
    generator.render_preview(mesh, preview_file, spec)
    render_turnaround(generator, mesh, turnaround_file)

    record = generator.make_record(spec, mesh, model_file, preview_file)
    record.update({
        "source_file": generator.rel(Path(__file__)),
        "source_reference": generator.rel(Path(__file__)),
        "style_gate_status": "playable_npc_template_candidate",
        "acceptance_status": "template_candidate_requires_human_art_review",
        "turnaround_preview_file": generator.rel(turnaround_file),
        "template_scope": "playable_and_friendly_npc_only",
        "excluded_scope": "enemies_monsters_bosses",
        "quality_revision": 1,
        "quality_notes": [
            "Built as the shared body mannequin before costume, hair, or gear modeling.",
            "Enemy, monster, and boss assets must use separate skeleton families.",
            "Visible joint landmarks are intended for later rig/socket and costume fitting work.",
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
    manifest["playable_npc_mannequin"] = {
        "target": TARGET_NAME,
        "status": "template_candidate_importable",
        "model_file": generator.rel(model_file),
        "preview_file": generator.rel(preview_file),
        "turnaround_preview_file": generator.rel(turnaround_file),
        "triangle_count": record["triangles_lod0"],
        "primitive_counts": record["primitive_counts"],
        "scope": "playable_and_friendly_npc_only",
        "excluded_scope": "enemies_monsters_bosses",
    }
    generator.MANIFEST_FILE.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")

    print(f"Built {TARGET_NAME}")
    print(f"Model: {generator.rel(model_file)}")
    print(f"Preview: {generator.rel(preview_file)}")
    print(f"Turnaround: {generator.rel(turnaround_file)}")
    print(f"Triangles: {record['triangles_lod0']}")
    print(f"Primitive counts: {record['primitive_counts']}")


def render_turnaround(generator, mesh, path: Path) -> None:
    panels = [
        ("Front", lambda vertex: (vertex[0], vertex[2], vertex[1])),
        ("Side", lambda vertex: (-vertex[1], vertex[2], vertex[0])),
        ("Back", lambda vertex: (-vertex[0], vertex[2], -vertex[1])),
        ("Iso", lambda vertex: ((vertex[0] - vertex[1]) * 0.86, (vertex[0] + vertex[1]) * 0.48 - vertex[2] * 0.92, vertex[0] + vertex[1] + vertex[2])),
    ]
    panel_w, panel_h = 360, 520
    image = Image.new("RGB", (panel_w * len(panels), panel_h), (18, 21, 27))
    draw = ImageDraw.Draw(image, "RGBA")

    for panel_index, (label, projector) in enumerate(panels):
        transformed = [projector(vertex) for vertex in mesh.vertices]
        xs = [point[0] for point in transformed]
        ys = [point[1] for point in transformed]
        min_x, max_x = min(xs), max(xs)
        min_y, max_y = min(ys), max(ys)
        scale = min(250 / max(max_x - min_x, 0.1), 410 / max(max_y - min_y, 0.1))
        center_x = (min_x + max_x) * 0.5
        offset_x = panel_index * panel_w + panel_w * 0.5
        bottom_y = panel_h - 34

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
            draw.polygon(points_2d, fill=color, outline=(0, 0, 0, 18))
        draw.text((panel_index * panel_w + 18, 18), label, fill=(230, 234, 226))

    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


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
