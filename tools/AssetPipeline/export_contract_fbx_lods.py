#!/usr/bin/env python3
"""Blender batch: convert contract 3D entries into LOD0/1/2 FBX files."""

from __future__ import annotations

import json
import sys
from pathlib import Path

import bpy


REPO = Path(__file__).resolve().parents[2]
ASSET_MANIFEST = REPO / "asset_manifest.json"


def main() -> None:
    manifest = json.loads(ASSET_MANIFEST.read_text(encoding="utf-8"))
    assets = [asset for asset in manifest.get("assets", []) if asset.get("kind") == "3d"]
    exported = 0
    failed = []
    for index, asset in enumerate(assets, 1):
        source = REPO / asset["source_model"]
        if not source.exists():
            failed.append({"asset": asset["contract_name"], "reason": f"missing source: {asset['source_model']}"})
            continue
        try:
            for lod_key, ratio in (("lod0", 1.0), ("lod1", 0.55), ("lod2", 0.28)):
                output = REPO / asset["final_files"][lod_key]
                output.parent.mkdir(parents=True, exist_ok=True)
                clear_scene()
                import_model(source)
                prepare_scene(asset, ratio)
                export_fbx(output)
                exported += 1
        except Exception as error:  # Blender batch should finish remaining assets.
            failed.append({"asset": asset["contract_name"], "reason": str(error)})
        if index % 25 == 0:
            print(f"contract FBX progress: {index}/{len(assets)} assets, {exported} lod files")
    report_path = REPO / "artifacts" / "Reports" / "contract-fbx-lod-export.json"
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps({
        "asset_count": len(assets),
        "fbx_exported": exported,
        "failed": failed,
    }, indent=2) + "\n", encoding="utf-8")
    print(f"Exported {exported} FBX LOD files. Failed: {len(failed)}")
    if failed:
        print(json.dumps(failed[:20], indent=2))
        sys.exit(1)


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    for datablock in (bpy.data.meshes, bpy.data.materials, bpy.data.images):
        for item in list(datablock):
            if item.users == 0:
                datablock.remove(item)


def import_model(path: Path) -> None:
    suffix = path.suffix.lower()
    if suffix == ".obj":
        if hasattr(bpy.ops.wm, "obj_import"):
            bpy.ops.wm.obj_import(filepath=str(path))
        else:
            bpy.ops.import_scene.obj(filepath=str(path))
    elif suffix == ".fbx":
        bpy.ops.import_scene.fbx(filepath=str(path))
    else:
        raise ValueError(f"unsupported model source format: {path}")


def prepare_scene(asset: dict, ratio: float) -> None:
    mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not mesh_objects:
        raise ValueError("source import produced no mesh objects")
    for obj in mesh_objects:
        obj.name = sanitize_object_name(f"{asset['contract_name']}_{obj.name}")
        obj.data.name = f"{obj.name}_mesh"
        ensure_uv0(obj)
        if ratio < 0.999:
            mod = obj.modifiers.new(name="contract_lod_decimate", type="DECIMATE")
            mod.ratio = ratio
            mod.use_collapse_triangulate = True
            bpy.context.view_layer.objects.active = obj
            obj.select_set(True)
            try:
                bpy.ops.object.modifier_apply(modifier=mod.name)
            finally:
                obj.select_set(False)
        obj.select_set(True)
    bpy.context.view_layer.objects.active = mesh_objects[0]


def ensure_uv0(obj) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    if not obj.data.uv_layers:
        obj.data.uv_layers.new(name="UVMap")
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    try:
        bpy.ops.uv.smart_project(angle_limit=1.15192, island_margin=0.02)
    finally:
        bpy.ops.object.mode_set(mode="OBJECT")


def export_fbx(path: Path) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.context.scene.objects:
        if obj.type == "MESH":
            obj.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        apply_unit_scale=True,
        global_scale=1.0,
        object_types={"MESH"},
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        path_mode="AUTO",
        embed_textures=False,
    )


def sanitize_object_name(value: str) -> str:
    return "".join(char if char.isalnum() or char in "_-@" else "_" for char in value)[:120]


if __name__ == "__main__":
    main()
