#!/usr/bin/env python3
"""Export contract FBX LODs directly from OBJ without Blender.

This exporter is intentionally conservative: it reads repository-authored OBJ
blockouts, triangulates faces, writes simple ASCII FBX meshes with generated
UV0, and creates deterministic LOD0/1/2 face reductions. It exists so the mass
asset batch can keep moving when Blender is unavailable.
"""

from __future__ import annotations

import json
import math
import re
from pathlib import Path


REPO = Path(__file__).resolve().parents[2]
ASSET_MANIFEST = REPO / "asset_manifest.json"
REPORT = REPO / "artifacts" / "Reports" / "contract-ascii-fbx-lod-export.json"

LOD_RATIOS = {
    "lod0": 1.0,
    "lod1": 0.55,
    "lod2": 0.28,
}


def main() -> None:
    manifest = json.loads(ASSET_MANIFEST.read_text(encoding="utf-8"))
    exported = []
    failed = []
    for asset in manifest.get("assets", []):
        if asset.get("kind") != "3d":
            continue
        source = REPO / asset["source_model"]
        if not source.exists():
            failed.append({"asset": asset["contract_name"], "reason": f"missing source OBJ: {asset['source_model']}"})
            continue
        try:
            mesh = parse_obj(source)
            for lod, ratio in LOD_RATIOS.items():
                triangles = reduce_triangles(mesh["triangles"], ratio)
                output = REPO / asset["final_files"][lod]
                output.parent.mkdir(parents=True, exist_ok=True)
                write_ascii_fbx(output, asset["contract_name"], mesh["vertices"], triangles)
                exported.append({
                    "asset": asset["contract_name"],
                    "lod": lod,
                    "source": asset["source_model"],
                    "output": rel(output),
                    "triangles": len(triangles),
                })
        except Exception as error:
            failed.append({"asset": asset.get("contract_name"), "reason": str(error)})

    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps({
        "source": "direct OBJ parser",
        "lod_ratios": LOD_RATIOS,
        "exported_lod_files": len(exported),
        "exported_assets": len({item["asset"] for item in exported}),
        "failed": failed,
    }, indent=2) + "\n", encoding="utf-8")
    print(f"ASCII FBX exported: {len(exported)} LOD files")
    print(f"Assets exported: {len({item['asset'] for item in exported})}")
    print(f"Failed: {len(failed)}")
    if failed:
        print(json.dumps(failed[:20], indent=2))


def parse_obj(path: Path) -> dict:
    vertices: list[tuple[float, float, float]] = []
    triangles: list[tuple[int, int, int]] = []
    with path.open("r", encoding="utf-8", errors="replace") as handle:
        for line in handle:
            if line.startswith("v "):
                parts = line.split()
                if len(parts) < 4:
                    continue
                vertices.append((float(parts[1]), float(parts[2]), float(parts[3])))
            elif line.startswith("f "):
                refs = [parse_face_ref(token, len(vertices)) for token in line.split()[1:]]
                refs = [ref for ref in refs if ref is not None]
                if len(refs) < 3:
                    continue
                for index in range(1, len(refs) - 1):
                    tri = (refs[0], refs[index], refs[index + 1])
                    if len(set(tri)) == 3:
                        triangles.append(tri)
    if not vertices:
        raise ValueError(f"OBJ has no vertices: {rel(path)}")
    if not triangles:
        raise ValueError(f"OBJ has no triangulatable faces: {rel(path)}")
    return {"vertices": vertices, "triangles": triangles}


def parse_face_ref(token: str, vertex_count: int) -> int | None:
    raw = token.split("/")[0]
    if not raw:
        return None
    index = int(raw)
    if index < 0:
        index = vertex_count + index
    else:
        index -= 1
    if index < 0 or index >= vertex_count:
        return None
    return index


def reduce_triangles(triangles: list[tuple[int, int, int]], ratio: float) -> list[tuple[int, int, int]]:
    if ratio >= 0.999:
        return list(triangles)
    target = max(1, round(len(triangles) * ratio))
    if target >= len(triangles):
        return list(triangles)
    step = len(triangles) / target
    selected = []
    seen = set()
    cursor = 0.0
    while len(selected) < target:
        index = min(len(triangles) - 1, int(cursor))
        cursor += step
        if index in seen:
            index = next((candidate for candidate in range(len(triangles)) if candidate not in seen), index)
        selected.append(triangles[index])
        seen.add(index)
    return selected


def write_ascii_fbx(path: Path, name: str, source_vertices: list[tuple[float, float, float]], triangles: list[tuple[int, int, int]]) -> None:
    vertices: list[tuple[float, float, float]] = []
    polygon_indices: list[int] = []
    uv_values: list[tuple[float, float]] = []

    bounds = compute_bounds(source_vertices)
    for tri in triangles:
        base = len(vertices)
        for original_index in tri:
            vertex = source_vertices[original_index]
            vertices.append(vertex)
            uv_values.append(project_uv(vertex, bounds))
        polygon_indices.extend([base, base + 1, -(base + 3)])

    model_name = sanitize_name(name)
    geometry_id = stable_id(model_name, 11)
    model_id = stable_id(model_name, 29)
    material_id = stable_id(model_name, 47)
    vertex_numbers = flatten_vertices(vertices)
    uv_numbers = flatten_uvs(uv_values)
    uv_indices = ",".join(str(index) for index in range(len(uv_values)))
    polygon_numbers = ",".join(str(index) for index in polygon_indices)

    text = f"""; FBX 7.4.0 project file
; FOURFOLD_ASCII_FBX_EXPORTER: direct_obj_lod_v1
; FOURFOLD_TRIANGLES: {len(triangles)}
FBXHeaderExtension:  {{
    FBXHeaderVersion: 1003
    FBXVersion: 7400
    Creator: "FOURFOLD direct OBJ contract exporter"
}}
GlobalSettings:  {{
    Version: 1000
    Properties70:  {{
        P: "UpAxis", "int", "Integer", "",1
        P: "UpAxisSign", "int", "Integer", "",1
        P: "FrontAxis", "int", "Integer", "",2
        P: "FrontAxisSign", "int", "Integer", "",1
        P: "CoordAxis", "int", "Integer", "",0
        P: "CoordAxisSign", "int", "Integer", "",1
        P: "UnitScaleFactor", "double", "Number", "",1
    }}
}}
Definitions:  {{
    Version: 100
    Count: 3
    ObjectType: "Geometry" {{ Count: 1 }}
    ObjectType: "Model" {{ Count: 1 }}
    ObjectType: "Material" {{ Count: 1 }}
}}
Objects:  {{
    Geometry: {geometry_id}, "Geometry::{model_name}_mesh", "Mesh" {{
        GeometryVersion: 124
        Vertices: *{len(vertices) * 3} {{
            a: {vertex_numbers}
        }}
        PolygonVertexIndex: *{len(polygon_indices)} {{
            a: {polygon_numbers}
        }}
        LayerElementUV: 0 {{
            Version: 101
            Name: "UVMap"
            MappingInformationType: "ByPolygonVertex"
            ReferenceInformationType: "IndexToDirect"
            UV: *{len(uv_values) * 2} {{
                a: {uv_numbers}
            }}
            UVIndex: *{len(uv_values)} {{
                a: {uv_indices}
            }}
        }}
        Layer: 0 {{
            Version: 100
            LayerElement:  {{
                Type: "LayerElementUV"
                TypedIndex: 0
            }}
        }}
    }}
    Model: {model_id}, "Model::{model_name}", "Mesh" {{
        Version: 232
        Properties70:  {{
            P: "Lcl Translation", "Lcl Translation", "", "A",0,0,0
            P: "Lcl Rotation", "Lcl Rotation", "", "A",0,0,0
            P: "Lcl Scaling", "Lcl Scaling", "", "A",1,1,1
        }}
        Shading: T
        Culling: "CullingOff"
    }}
    Material: {material_id}, "Material::{model_name}_mat", "" {{
        Version: 102
        ShadingModel: "phong"
        MultiLayer: 0
        Properties70:  {{
            P: "DiffuseColor", "Color", "", "A",0.72,0.62,0.48
            P: "SpecularColor", "Color", "", "A",0.05,0.05,0.05
        }}
    }}
}}
Connections:  {{
    C: "OO",{geometry_id},{model_id}
    C: "OO",{material_id},{model_id}
    C: "OO",{model_id},0
}}
"""
    path.write_text(text, encoding="utf-8")


def compute_bounds(vertices: list[tuple[float, float, float]]) -> tuple[tuple[float, float, float], tuple[float, float, float]]:
    xs = [vertex[0] for vertex in vertices]
    ys = [vertex[1] for vertex in vertices]
    zs = [vertex[2] for vertex in vertices]
    return (min(xs), min(ys), min(zs)), (max(xs), max(ys), max(zs))


def project_uv(vertex: tuple[float, float, float], bounds: tuple[tuple[float, float, float], tuple[float, float, float]]) -> tuple[float, float]:
    mins, maxs = bounds
    span_x = max(maxs[0] - mins[0], 0.0001)
    span_z = max(maxs[2] - mins[2], 0.0001)
    return ((vertex[0] - mins[0]) / span_x, (vertex[2] - mins[2]) / span_z)


def flatten_vertices(vertices: list[tuple[float, float, float]]) -> str:
    values = []
    for vertex in vertices:
        values.extend(format_float(value) for value in vertex)
    return ",".join(values)


def flatten_uvs(uvs: list[tuple[float, float]]) -> str:
    values = []
    for uv in uvs:
        values.extend(format_float(value) for value in uv)
    return ",".join(values)


def format_float(value: float) -> str:
    if not math.isfinite(value):
        return "0"
    return f"{value:.6f}".rstrip("0").rstrip(".") or "0"


def sanitize_name(value: str) -> str:
    return re.sub(r"[^A-Za-z0-9_]+", "_", value)[:80]


def stable_id(value: str, salt: int) -> int:
    number = 100000 + salt
    for char in value:
        number = (number * 131 + ord(char)) % 900000000
    return number + 100000


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO).as_posix()


if __name__ == "__main__":
    main()
