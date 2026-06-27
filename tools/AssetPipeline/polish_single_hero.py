#!/usr/bin/env python3
"""Rebuild only FE_CHAR_PLAYER_Hero_01 as a polished single-asset pass."""

from __future__ import annotations

import importlib.util
import json
import sys
from pathlib import Path


REPO = Path(__file__).resolve().parents[2]
GENERATOR_PATH = REPO / "tools" / "AssetPipeline" / "generate_production_model_pack.py"
TARGET_NAME = "FE_CHAR_PLAYER_Hero_01"


def main() -> None:
    generator = load_generator()
    specs = generator.load_specs()
    spec = next((item for item in specs if item.name == TARGET_NAME), None)
    if spec is None:
        raise RuntimeError(f"Missing AssetSpec for {TARGET_NAME}")

    mesh = generator.Mesh()
    generator.hero(mesh)
    generator.normalize(mesh)
    generator.trim_materials(mesh, spec.material_budget)

    model_file = generator.model_path(spec)
    preview_file = generator.PREVIEW_DIR / f"{spec.name}.png"
    model_file.parent.mkdir(parents=True, exist_ok=True)
    preview_file.parent.mkdir(parents=True, exist_ok=True)
    generator.write_obj(mesh, model_file, spec.name)
    generator.render_preview(mesh, preview_file, spec)

    record = generator.make_record(spec, mesh, model_file, preview_file)
    record.update({
        "source_file": generator.rel(Path(__file__)),
        "source_reference": generator.rel(Path(__file__)),
        "style_gate_status": "single_asset_polished_candidate",
        "acceptance_status": "single_asset_polished_candidate_requires_human_art_review",
        "quality_revision": 1,
        "quality_notes": [
            "Rebuilt as a single production candidate, not batch filler.",
            "Primary forms use ellipsoid/capsule/rounded-plate geometry instead of raw stacked boxes.",
            "Still repository-authored and commercial-safe; requires human art review before approval."
        ],
    })

    manifest = json.loads(generator.MANIFEST_FILE.read_text(encoding="utf-8"))
    assets = manifest.get("assets")
    if not isinstance(assets, list):
        raise RuntimeError(f"Invalid manifest assets array: {generator.MANIFEST_FILE}")
    replaced = False
    for index, asset in enumerate(assets):
        if asset.get("name") == TARGET_NAME:
            assets[index] = record
            replaced = True
            break
    if not replaced:
        assets.insert(0, record)
    manifest["assets"] = assets
    manifest["consistency_summary"] = generator.consistency_summary(assets)
    manifest["single_asset_polish"] = {
        "target": TARGET_NAME,
        "status": "polished_candidate_importable",
        "model_file": generator.rel(model_file),
        "preview_file": generator.rel(preview_file),
        "triangle_count": record["triangles_lod0"],
        "primitive_counts": record["primitive_counts"],
    }
    generator.MANIFEST_FILE.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")

    print(f"Polished {TARGET_NAME}")
    print(f"Model: {generator.rel(model_file)}")
    print(f"Preview: {generator.rel(preview_file)}")
    print(f"Triangles: {record['triangles_lod0']}")
    print(f"Primitive counts: {record['primitive_counts']}")


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
