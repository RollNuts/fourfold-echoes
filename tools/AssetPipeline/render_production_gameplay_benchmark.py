#!/usr/bin/env python3
"""Render a dependency-light gameplay benchmark from generated OBJ mesh data."""

from __future__ import annotations

import importlib.util
import math
import sys
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter


REPO = Path(__file__).resolve().parents[2]
GENERATOR_PATH = REPO / "tools" / "AssetPipeline" / "generate_production_model_pack.py"
OUTPUT = REPO / "artifacts" / "Previews" / "ProductionModelPack" / "FE_BENCHMARK_R01_GameplayScene.png"


def main() -> None:
    generator = load_generator()
    specs = {spec.name: spec for spec in generator.load_specs()}
    faces = []

    add_floor(generator, faces)
    placements = [
        ("FE_PROP_COMMON_CheckpointSocketPad_01", (-4.25, -2.55, 0.10), 1.00, 8),
        ("FE_CHAR_PLAYER_SignatureLead_01", (-3.45, -2.36, 0.12), 1.08, 32),
        ("FE_CHAR_PLAYER_HeavyGuard_01", (-2.42, -1.92, 0.12), 0.96, 26),
        ("FE_CHAR_PLAYER_StampCaster_01", (-1.42, -1.18, 0.12), 0.90, 22),
        ("FE_PROP_COMMON_ToolBlade_01", (-3.92, -1.78, 0.20), 0.72, -32),
        ("FE_PROP_COMMON_SealStamp_01", (-1.04, -0.60, 0.20), 0.66, -18),
        ("FE_PROP_R01_GimmickPedestal_01", (-1.75, -0.60, 0.10), 1.06, -10),
        ("FE_ENV_R01_BF_CombatArena_2x2_01", (0.55, -0.14, 0.06), 1.30, 0),
        ("FE_ENEMY_R01_FoldBiter_01", (0.16, 0.38, 0.16), 1.05, 210),
        ("FE_ENEMY_R01_ShieldClamp_01", (1.02, -0.32, 0.16), 1.03, 210),
        ("FE_ENEMY_R01_LineSpitter_01", (2.35, 1.08, 0.16), 0.96, 225),
        ("FE_ENEMY_R01_BlockCharger_01", (2.42, -1.18, 0.16), 1.08, 205),
        ("FE_BOSS_R01_StampMiniboss_01", (3.48, -0.18, 0.16), 0.72, 210),
        ("FE_PROP_COMMON_RewardReceiverPad_01", (4.04, 2.12, 0.10), 1.08, -20),
        ("FE_PROP_COMMON_RelicChest_01", (4.58, 2.62, 0.12), 0.92, -20),
        ("FE_RELIC_EmberSeed_01", (4.38, 2.22, 0.78), 1.05, 0),
        ("FE_ENV_R01_ShortcutBridge_01", (-4.52, 0.54, 0.08), 1.14, 23),
        ("FE_PROP_R01_RootGate_01", (-4.86, -0.38, 0.08), 1.12, 90),
    ]
    for name, location, scale, angle in placements:
        add_asset(generator, specs, faces, name, location, scale, math.radians(angle))

    image = draw_scene(generator, faces)
    image = presentation_grade(image)
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    image.save(OUTPUT)
    print(f"Rendered dependency-light gameplay benchmark: {OUTPUT.relative_to(REPO)}")


def load_generator():
    spec = importlib.util.spec_from_file_location("production_model_pack_generator", GENERATOR_PATH)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Could not load generator at {GENERATOR_PATH}")
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


def add_floor(generator, faces: list[dict]) -> None:
    palette = generator.PALETTE
    floor = generator.Mesh()
    for x in range(-6, 7):
        for y in range(-4, 5):
            edge = abs(x) == 6 or abs(y) == 4
            diagonal = -1 <= (x - y) <= 1
            hero_zone = x <= -2 and y <= -1
            threat_zone = -1 <= x <= 3 and -2 <= y <= 2
            reward_zone = x >= 3 and y >= 1
            material = "hub_ivory" if diagonal else ("stone_warm" if (x + y) % 2 else "moss")
            if edge:
                material = "stone_dark"
            elif hero_zone:
                material = "tool_signal" if (x + y) % 2 else "hub_ivory"
            elif threat_zone:
                material = "route_teal" if (x + y) % 2 else "stone_warm"
            elif reward_zone:
                material = "gold" if (x + y) % 2 else "tool_blue"
            floor.add_box(material, (x, y, -0.035), (0.46, 0.46, 0.04), math.radians(((x * 7 + y * 11) % 9) - 4))
            if not edge and (x + y) % 3 == 0:
                line_material = "danger_red" if threat_zone and (x + y) % 2 == 0 else "stone_dark"
                floor.add_box(line_material, (x + 0.14, y - 0.15, 0.030), (0.34, 0.034, 0.020), math.radians((x * 13 + y * 7) % 36 - 18))
            if not edge and (x * 2 + y) % 7 == 0:
                floor.add_box("flower_yellow", (x - 0.20, y + 0.18, 0.060), (0.09, 0.09, 0.024), 0)
    for material, x, y, sx, sy, angle in [
        ("tool_blue", -4.50, -2.72, 0.96, 0.68, -8),
        ("hub_ivory", -3.05, -2.28, 1.78, 0.98, 12),
        ("tool_signal", -1.70, -0.86, 1.22, 0.74, -10),
        ("enemy_ink", 0.96, 0.02, 1.72, 1.02, -8),
        ("route_teal", 1.08, 0.72, 1.30, 0.62, 13),
        ("stone_dark", 2.62, -1.12, 1.54, 0.94, 15),
        ("gold", 4.10, 2.24, 1.34, 0.86, -18),
    ]:
        floor.add_box(material, (x, y, 0.018), (sx, sy, 0.032), math.radians(angle))
    add_wide_bands(floor)
    push_faces(faces, floor, palette, label="floor", transform=(0, 0, 0, 1, 0))


def add_wide_bands(mesh) -> None:
    for material, x, y, angle, length in [
        ("tool_signal", -3.35, -2.10, 18, 0.86),
        ("tool_signal", -2.55, -1.74, 18, 0.78),
        ("tool_signal", -1.68, -1.28, 18, 0.70),
        ("danger_red", 0.12, -0.20, -18, 0.86),
        ("danger_red", 0.72, 0.16, -18, 0.78),
        ("danger_red", 1.36, 0.48, -18, 0.74),
        ("danger_red", 2.40, -0.64, 20, 0.86),
        ("danger_red", 3.24, -0.98, 20, 0.76),
        ("tool_blue", 3.78, 1.74, -22, 0.76),
        ("tool_blue", 4.36, 2.18, -22, 0.72),
    ]:
        mesh.add_box(material, (x, y, 0.115), (length, 0.105, 0.035), math.radians(angle))
        mesh.add_box("enemy_ink", (x + 0.05, y - 0.06, 0.086), (length * 0.72, 0.040, 0.025), math.radians(angle - 7))


def add_asset(generator, specs, faces: list[dict], name: str, location, scale: float, rotation: float) -> None:
    spec = specs[name]
    mesh = generator.build_mesh(spec)
    generator.trim_materials(mesh, spec.material_budget)
    push_faces(faces, mesh, generator.PALETTE, label=name, transform=(*location, scale, rotation))


def push_faces(faces: list[dict], mesh, palette: dict[str, tuple[int, int, int]], label: str, transform) -> None:
    tx, ty, tz, scale, rotation = transform
    cos_z = math.cos(rotation)
    sin_z = math.sin(rotation)
    transformed = []
    for x, y, z in mesh.vertices:
        sx = x * scale
        sy = y * scale
        rx = sx * cos_z - sy * sin_z + tx
        ry = sx * sin_z + sy * cos_z + ty
        rz = z * scale + tz
        transformed.append((rx, ry, rz))
    for material, face in mesh.faces:
        vertices = [transformed[index - 1] for index in face]
        depth = sum((x + y + z * 0.7) for x, y, z in vertices) / len(vertices)
        faces.append({"label": label, "material": material, "color": palette[material], "vertices": vertices, "depth": depth})


def draw_scene(generator, faces: list[dict]) -> Image.Image:
    projected = []
    for item in faces:
        points = [project(vertex) for vertex in item["vertices"]]
        projected.append({**item, "points": points})
    bounds = projected_bounds(projected)
    width, height = 1280, 720
    bound_w = max(bounds[2] - bounds[0], 0.1)
    bound_h = max(bounds[3] - bounds[1], 0.1)
    scale = min((width * 0.90) / bound_w, (height * 0.82) / bound_h)
    cx = (bounds[0] + bounds[2]) * 0.5
    cy = (bounds[1] + bounds[3]) * 0.5
    image = Image.new("RGB", (width, height), (76, 86, 104))
    draw = ImageDraw.Draw(image, "RGBA")
    for item in sorted(projected, key=lambda face: face["depth"]):
        points = [((x - cx) * scale + width * 0.50, (y - cy) * scale + height * 0.54) for x, y in item["points"]]
        color = item["color"]
        draw.polygon(points, fill=(*lit_color(color, item["vertices"]), 238), outline=(7, 9, 14, 102))
    draw_frame_hud(draw)
    return image


def project(vertex) -> tuple[float, float]:
    x, y, z = vertex
    return (x - y) * 0.86, (x + y) * 0.48 - z * 0.92


def projected_bounds(projected: list[dict]) -> tuple[float, float, float, float]:
    xs = [point[0] for item in projected for point in item["points"]]
    ys = [point[1] for item in projected for point in item["points"]]
    return min(xs), min(ys), max(xs), max(ys)


def lit_color(color: tuple[int, int, int], vertices) -> tuple[int, int, int]:
    avg_z = sum(vertex[2] for vertex in vertices) / len(vertices)
    factor = min(1.32, max(0.76, 0.90 + avg_z * 0.12))
    return tuple(min(255, int(component * factor)) for component in color)


def draw_frame_hud(draw: ImageDraw.ImageDraw) -> None:
    draw.rounded_rectangle((44, 38, 326, 76), radius=8, fill=(9, 12, 18, 160), outline=(209, 242, 117, 210), width=2)
    draw.rectangle((70, 52, 228, 62), fill=(242, 31, 15, 230))
    draw.rectangle((70, 64, 270, 70), fill=(61, 184, 255, 220))
    for x in range(1030, 1190, 38):
        draw.rounded_rectangle((x, 46, x + 28, 74), radius=6, fill=(15, 18, 26, 180), outline=(242, 171, 61, 180), width=2)


def presentation_grade(image: Image.Image) -> Image.Image:
    graded = ImageEnhance.Color(image).enhance(1.32)
    graded = ImageEnhance.Contrast(graded).enhance(1.24)
    graded = ImageEnhance.Brightness(graded).enhance(1.18)
    sharpened = graded.filter(ImageFilter.UnsharpMask(radius=1.6, percent=115, threshold=3))
    edge = sharpened.filter(ImageFilter.FIND_EDGES).convert("L")
    edge = ImageEnhance.Contrast(edge).enhance(0.42)
    dark_edges = Image.new("RGB", sharpened.size, (8, 10, 14))
    return Image.composite(dark_edges, sharpened, edge.point(lambda value: min(64, value)))


if __name__ == "__main__":
    main()
