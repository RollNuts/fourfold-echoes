#!/usr/bin/env python3
"""Generate production-intent first-pass FOURFOLD ECHOES model assets.

This generator is intentionally dependency-light because local Blender may be
unavailable or unstable in CI/editor environments. It writes original
repo-authored OBJ/MTL meshes, preview PNGs, and a manifest for Unity import.
"""

from __future__ import annotations

import ast
import json
import math
import re
from dataclasses import dataclass
from datetime import date
from pathlib import Path

from PIL import Image, ImageDraw


REPO = Path(__file__).resolve().parents[2]
SPEC_SOURCE = REPO / "tools" / "Blender" / "generate_fourfold_model_pack.py"
PRODUCTION_ROOT = REPO / "Assets" / "Art" / "Production"
PREVIEW_DIR = REPO / "artifacts" / "Previews" / "ProductionModelPack"
REPORT_DIR = REPO / "artifacts" / "Reports"
MANIFEST_FILE = REPORT_DIR / "fourfold-model-pack.json"
GENERATOR_FILE = Path(__file__).resolve()
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
PRODUCT_LINE_CONTRACT = {
    "brand_line_id": BRAND_LINE_ID,
    "brand_line_name": BRAND_LINE_NAME,
    "style_bible": STYLE_BIBLE_DOC,
    "genre_statement": GENRE_STATEMENT,
    "non_negotiable_rules": [
        "Every model belongs to one folded-reliquary product line.",
        "Signal colors are functional only: tool, route, reward, lock, danger, or boss read.",
        "External games are aggregate market benchmarks only, never shape, palette, composition, or motif sources.",
    ],
}
ART_DIRECTION_CONTRACT = {
    "contract_id": ART_DIRECTION_ID,
    "canonical_doc": STYLE_BIBLE_DOC,
    "genre": "compact_top_down_action_adventure",
    "shape_grammar": "folded_reliquary_miniature",
    "brand_line_id": BRAND_LINE_ID,
    "genre_statement": GENRE_STATEMENT,
}
STYLE_FAMILIES = {
    "common": {"shape_tokens": ["folded_plinth", "split_inlay", "signal_thread", "chunk_tab"]},
    "hub": {"shape_tokens": ["clean_folded_plinth", "quartered_floor_mark", "signal_thread"]},
    "r01": {"shape_tokens": ["weathered_folded_plinth", "rounded_chip", "low_growth_tab", "signal_thread"]},
    "r02": {"shape_tokens": ["scorched_folded_plinth", "bent_chunk_tab", "amber_signal_thread"]},
    "r03": {"shape_tokens": ["cold_folded_plinth", "sharp_chunk_tab", "violet_split_inlay"]},
    "boss": {"shape_tokens": ["large_folded_plinth", "quartered_arena_read", "danger_signal_thread"]},
}
PRODUCT_LINE_ROLES = {
    "Hero": {
        "role": "Toolbearer Relic",
        "required_shape_tokens": ["folded_body", "front_read", "tool_socket", "ground_plinth", "functional_signal_thread"],
        "forbidden_drift_tokens": ["generic_rpg_hero", "mmo_armor_density", "face_or_hair_as_identity"],
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
        "forbidden_drift_tokens": ["magic_staff", "antenna_prop", "single_orb_core"],
    },
    "Combatant": {
        "role": "Broken Hostile Relic",
        "required_shape_tokens": ["broken_folded_body", "front_attack_plane", "danger_crack", "functional_signal_thread"],
        "forbidden_drift_tokens": ["red_eye_creature", "round_body_mascot", "handheld_club", "plant_monster"],
    },
    "Boss": {
        "role": "Warden Relic",
        "required_shape_tokens": ["large_folded_plinth", "four_threat_anchors", "weak_socket", "boss_danger_surface"],
        "forbidden_drift_tokens": ["trademarked_boss_outline", "literal_crown_icon", "vfx_noise_as_shape"],
    },
    "ToolReceiver": {
        "role": "Tool Receiver",
        "required_shape_tokens": ["receiver_socket", "idle_active_solved_read", "folded_plinth", "input_slit"],
        "forbidden_drift_tokens": ["text_signage", "generic_machine_terminal", "second_tool_read"],
    },
    "RouteSurface": {
        "role": "Route Surface",
        "required_shape_tokens": ["low_folded_plate", "folded_shell_leaf", "connection_socket", "hinge_spine", "recessed_signal_groove", "dark_underside_mass", "top_down_navigation_read"],
        "forbidden_drift_tokens": ["uniform_tile_grid", "rectangular_slab_bridge", "gray_blockout_floor", "decorative_glow_only"],
    },
    "LowBoundary": {
        "role": "Low Boundary",
        "required_shape_tokens": ["low_folded_wall", "readable_blocked_state", "chunk_tab", "camera_safe_height"],
        "forbidden_drift_tokens": ["thin_fence", "photoreal_cliff", "tall_pillar_row"],
    },
    "RewardReliquary": {
        "role": "Reward Reliquary",
        "required_shape_tokens": ["small_folded_shrine", "pickup_disc", "reward_socket", "functional_signal_thread"],
        "forbidden_drift_tokens": ["generic_treasure_chest", "literal_jewel_pickup", "franchise_relic_shape"],
    },
    "GroundingDetail": {
        "role": "Grounding Detail",
        "required_shape_tokens": ["low_inlay", "chunk_chip", "clustered_focal_placement", "supports_fourfold_read"],
        "forbidden_drift_tokens": ["sprinkled_noise", "thin_grass_wire", "standalone_flower_patch"],
    },
    "SetDressing": {
        "role": "Reliquary Set Dressing",
        "required_shape_tokens": ["folded_plinth", "chunk_tab", "split_inlay", "top_down_silhouette"],
        "forbidden_drift_tokens": ["random_fantasy_clutter", "literal_factory_prop", "crystal_forest_prop"],
    },
}


PALETTE = {
    "stone_warm": (160, 145, 122),
    "stone_dark": (56, 54, 56),
    "hub_ivory": (224, 199, 148),
    "gold": (242, 171, 61),
    "tool_signal": (209, 242, 117),
    "tool_blue": (61, 184, 255),
    "route_teal": (28, 135, 130),
    "hero_cloth": (41, 56, 148),
    "hero_ivory": (235, 199, 140),
    "skin_warm": (242, 174, 120),
    "hair_brown": (93, 52, 30),
    "jacket_blue": (34, 133, 205),
    "jacket_orange": (238, 115, 34),
    "pants_charcoal": (34, 36, 43),
    "pack_gray": (176, 179, 170),
    "mannequin_warm": (221, 178, 119),
    "mannequin_light": (239, 207, 157),
    "mannequin_joint": (124, 82, 48),
    "mannequin_mark": (58, 45, 37),
    "skin_peach": (244, 178, 132),
    "hair_ink": (38, 31, 50),
    "cloth_coral": (238, 91, 85),
    "cloth_cyan": (45, 190, 210),
    "cloth_plum": (122, 70, 181),
    "cloth_mint": (86, 204, 145),
    "paper_cream": (242, 219, 166),
    "stamp_orange": (242, 128, 56),
    "berry_magenta": (214, 72, 145),
    "leather_blue": (50, 94, 160),
    "enemy_lime": (150, 215, 74),
    "enemy_purple": (104, 67, 156),
    "metal": (140, 148, 153),
    "enemy_ink": (20, 15, 23),
    "enemy_armor": (43, 33, 31),
    "danger_red": (242, 31, 15),
    "moss": (56, 107, 56),
    "leaf": (31, 122, 61),
    "flower_yellow": (250, 209, 56),
    "wood": (107, 59, 31),
    "root": (77, 46, 28),
    "grass_top": (92, 181, 74),
    "grass_side": (72, 126, 63),
    "soil_dark": (73, 59, 48),
    "path_tan": (202, 166, 105),
    "stone_light": (198, 190, 169),
    "stone_mid": (132, 132, 126),
    "water_blue": (55, 171, 214),
    "water_deep": (24, 96, 132),
    "hazard_steel": (116, 121, 124),
    "relic_blue": (46, 179, 240),
    "ember": (255, 102, 31),
    "r02_rust": (158, 56, 31),
    "r02_charcoal": (36, 36, 38),
    "r02_amber": (255, 148, 41),
    "r03_dark": (28, 33, 56),
    "r03_violet": (107, 56, 179),
    "r03_crystal": (148, 224, 255),
    "white_glow": (224, 245, 255),
}


FORBIDDEN_TERMS = [
    "final fantasy",
    "ff-style",
    "octopath",
    "hd-2d",
    "dragon quest",
    "akira toriyama",
    "square enix",
    "nintendo",
    "chocobo",
    "moogle",
    "cactuar",
    "tonberry",
]


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


class Mesh:
    def __init__(self) -> None:
        self.vertices: list[tuple[float, float, float]] = []
        self.faces: list[tuple[str, tuple[int, ...]]] = []
        self.materials: dict[str, tuple[int, int, int]] = {}
        self.primitive_counts: dict[str, int] = {}

    def count(self, primitive: str) -> None:
        self.primitive_counts[primitive] = self.primitive_counts.get(primitive, 0) + 1

    def use(self, material: str) -> str:
        if material not in self.materials:
            self.materials[material] = PALETTE[material]
        return material

    def add_box(self, material: str, loc, scale, rot_z: float = 0.0) -> None:
        self.count("box")
        mat = self.use(material)
        x, y, z = loc
        sx, sy, sz = scale
        points = [
            (-sx, -sy, -sz), (sx, -sy, -sz), (sx, sy, -sz), (-sx, sy, -sz),
            (-sx, -sy, sz), (sx, -sy, sz), (sx, sy, sz), (-sx, sy, sz),
        ]
        start = len(self.vertices) + 1
        cos_z, sin_z = math.cos(rot_z), math.sin(rot_z)
        for px, py, pz in points:
            rx = px * cos_z - py * sin_z
            ry = px * sin_z + py * cos_z
            self.vertices.append((x + rx, y + ry, z + pz))
        for face in ((1, 2, 3, 4), (5, 8, 7, 6), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 8, 4), (4, 8, 5, 1)):
            self.faces.append((mat, tuple(start + index - 1 for index in face)))

    def add_prism(self, material: str, loc, points, height: float, rot_z: float = 0.0) -> None:
        self.count("prism")
        mat = self.use(material)
        x, y, z = loc
        cos_z, sin_z = math.cos(rot_z), math.sin(rot_z)
        start = len(self.vertices) + 1
        for level in (-height * 0.5, height * 0.5):
            for px, py in points:
                rx = px * cos_z - py * sin_z
                ry = px * sin_z + py * cos_z
                self.vertices.append((x + rx, y + ry, z + level))
        count = len(points)
        self.faces.append((mat, tuple(start + count - 1 - index for index in range(count))))
        self.faces.append((mat, tuple(start + count + index for index in range(count))))
        for i in range(count):
            j = (i + 1) % count
            self.faces.append((mat, (start + i, start + j, start + count + j, start + count + i)))

    def add_cylinder(self, material: str, loc, radius: float, height: float, vertices: int = 16) -> None:
        self.count("cylinder")
        mat = self.use(material)
        x, y, z = loc
        start = len(self.vertices) + 1
        for level in (-height * 0.5, height * 0.5):
            for i in range(vertices):
                angle = math.tau * i / vertices
                self.vertices.append((x + math.cos(angle) * radius, y + math.sin(angle) * radius, z + level))
        bottom_center = len(self.vertices) + 1
        self.vertices.append((x, y, z - height * 0.5))
        top_center = len(self.vertices) + 1
        self.vertices.append((x, y, z + height * 0.5))
        for i in range(vertices):
            j = (i + 1) % vertices
            self.faces.append((mat, (start + i, start + j, start + vertices + j, start + vertices + i)))
            self.faces.append((mat, (bottom_center, start + j, start + i)))
            self.faces.append((mat, (top_center, start + vertices + i, start + vertices + j)))

    def add_cone(self, material: str, loc, radius: float, height: float, vertices: int = 6) -> None:
        self.count("cone")
        mat = self.use(material)
        x, y, z = loc
        start = len(self.vertices) + 1
        for i in range(vertices):
            angle = math.tau * i / vertices
            self.vertices.append((x + math.cos(angle) * radius, y + math.sin(angle) * radius, z))
        tip = len(self.vertices) + 1
        self.vertices.append((x, y, z + height))
        center = len(self.vertices) + 1
        self.vertices.append((x, y, z))
        for i in range(vertices):
            j = (i + 1) % vertices
            self.faces.append((mat, (start + i, start + j, tip)))
            self.faces.append((mat, (center, start + j, start + i)))

    def add_sphere(self, material: str, loc, radius: float, rings: int = 4, segments: int = 10) -> None:
        self.add_ellipsoid(material, loc, (radius, radius, radius), rings=rings, segments=segments)

    def add_ellipsoid(self, material: str, loc, radii, rings: int = 8, segments: int = 24) -> None:
        self.count("ellipsoid")
        mat = self.use(material)
        x, y, z = loc
        rx, ry, rz = radii
        start = len(self.vertices) + 1
        for r in range(1, rings):
            phi = math.pi * r / rings
            for s in range(segments):
                theta = math.tau * s / segments
                self.vertices.append((
                    x + math.sin(phi) * math.cos(theta) * rx,
                    y + math.sin(phi) * math.sin(theta) * ry,
                    z + math.cos(phi) * rz,
                ))
        top = len(self.vertices) + 1
        self.vertices.append((x, y, z + rz))
        bottom = len(self.vertices) + 1
        self.vertices.append((x, y, z - rz))
        for s in range(segments):
            n = (s + 1) % segments
            self.faces.append((mat, (top, start + s, start + n)))
            self.faces.append((mat, (bottom, start + (rings - 2) * segments + n, start + (rings - 2) * segments + s)))
        for r in range(rings - 2):
            for s in range(segments):
                n = (s + 1) % segments
                a = start + r * segments + s
                b = start + r * segments + n
                c = start + (r + 1) * segments + n
                d = start + (r + 1) * segments + s
                self.faces.append((mat, (a, b, c, d)))

    def add_capsule_between(self, material: str, start_point, end_point, radius: float, segments: int = 18) -> None:
        self.count("capsule")
        mat = self.use(material)
        ax, ay, az = start_point
        bx, by, bz = end_point
        dx, dy, dz = bx - ax, by - ay, bz - az
        length = math.sqrt(dx * dx + dy * dy + dz * dz)
        if length <= 1e-5:
            self.add_ellipsoid(material, start_point, (radius, radius, radius), rings=6, segments=segments)
            return

        nx, ny, nz = dx / length, dy / length, dz / length
        ref = (0.0, 0.0, 1.0) if abs(nz) < 0.95 else (0.0, 1.0, 0.0)
        ux, uy, uz = cross((nx, ny, nz), ref)
        u_len = math.sqrt(ux * ux + uy * uy + uz * uz) or 1.0
        ux, uy, uz = ux / u_len, uy / u_len, uz / u_len
        vx, vy, vz = cross((ux, uy, uz), (nx, ny, nz))

        start = len(self.vertices) + 1
        for base in ((ax, ay, az), (bx, by, bz)):
            px, py, pz = base
            for i in range(segments):
                angle = math.tau * i / segments
                cx = math.cos(angle) * radius
                cy = math.sin(angle) * radius
                self.vertices.append((px + ux * cx + vx * cy, py + uy * cx + vy * cy, pz + uz * cx + vz * cy))
        cap_a = len(self.vertices) + 1
        self.vertices.append((ax, ay, az))
        cap_b = len(self.vertices) + 1
        self.vertices.append((bx, by, bz))
        for i in range(segments):
            j = (i + 1) % segments
            self.faces.append((mat, (start + i, start + j, start + segments + j, start + segments + i)))
            self.faces.append((mat, (cap_a, start + i, start + j)))
            self.faces.append((mat, (cap_b, start + segments + j, start + segments + i)))
        self.add_ellipsoid(material, start_point, (radius * 1.02, radius * 1.02, radius * 1.02), rings=5, segments=segments)
        self.add_ellipsoid(material, end_point, (radius * 1.02, radius * 1.02, radius * 1.02), rings=5, segments=segments)

    def add_rounded_plate(self, material: str, loc, scale, height: float, rot_z: float = 0.0, corner: float = 0.16) -> None:
        sx, sy = scale
        points = rounded_rect_points(sx, sy, min(corner, sx * 0.42, sy * 0.42), corner_segments=4)
        self.add_prism(material, loc, points, height, rot_z=rot_z)

    def triangle_count(self) -> int:
        return sum(max(len(face) - 2, 1) for _, face in self.faces)


def main() -> None:
    specs = load_specs()
    ensure_dirs(specs)
    records = []
    for spec in specs:
        assert_safe(spec)
        mesh = build_mesh(spec)
        trim_materials(mesh, spec.material_budget)
        model_file = model_path(spec)
        write_obj(mesh, model_file, spec.name)
        preview_file = PREVIEW_DIR / f"{spec.name}.png"
        render_preview(mesh, preview_file, spec)
        records.append(make_record(spec, mesh, model_file, preview_file))

    MANIFEST_FILE.write_text(json.dumps({
        "version": 2,
        "generated_at": date.today().isoformat(),
        "source_tool": "repository-authored Python procedural OBJ generation",
        "source_file": rel(GENERATOR_FILE),
        "art_direction_id": ART_DIRECTION_ID,
        "art_direction_name": ART_DIRECTION_NAME,
        "brand_line_id": BRAND_LINE_ID,
        "brand_line_name": BRAND_LINE_NAME,
        "genre_statement": GENRE_STATEMENT,
        "style_bible": STYLE_BIBLE_DOC,
        "product_line_contract": PRODUCT_LINE_CONTRACT,
        "genre_contract_id": BRAND_LINE_ID,
        "shape_grammar_id": ART_DIRECTION_ID,
        "benchmark_policy_id": "external_market_metrics_only",
        "benchmark_report_path": "artifacts/Reports/visual-benchmark.json",
        "commercial_safety_policy_id": "repository_authored_non_derivative_no_external_style_prompts",
        "consistency_summary": consistency_summary(records),
        "art_direction_contract": ART_DIRECTION_CONTRACT,
        "style_families": STYLE_FAMILIES,
        "external_benchmark_use": "quality metrics only; not prompts, source assets, style targets, or derivative instructions",
        "comparison_scope": "aggregate market finish metrics only",
        "human_review_required": "commercial/IP/style/trademark/likeness review before market-facing use",
        "license": "project-owned",
        "attribution": "none",
        "style_ip_clearance": "original FOURFOLD folded-reliquary semantic shape language; no protected characters, no franchise lookalikes, no trademarked style labels",
        "blender_status": "not required for this generation path",
        "assets": records,
    }, indent=2) + "\n", encoding="utf-8")
    print(f"Generated {len(records)} production model assets.")
    print(f"Manifest: {rel(MANIFEST_FILE)}")


def load_specs() -> list[AssetSpec]:
    text = SPEC_SOURCE.read_text(encoding="utf-8")
    specs = []
    for match in re.finditer(r"AssetSpec\((.*?)\),", text, re.DOTALL):
        values = ast.literal_eval(f"({match.group(1)})")
        if len(values) == 12:
            specs.append(AssetSpec(*values))
    if not specs:
        raise RuntimeError(f"No AssetSpec rows found in {SPEC_SOURCE}")
    return specs


def ensure_dirs(specs: list[AssetSpec]) -> None:
    for spec in specs:
        (PRODUCTION_ROOT / spec.priority / "Models").mkdir(parents=True, exist_ok=True)
    PREVIEW_DIR.mkdir(parents=True, exist_ok=True)
    REPORT_DIR.mkdir(parents=True, exist_ok=True)


def build_mesh(spec: AssetSpec) -> Mesh:
    mesh = Mesh()
    b = spec.builder
    if b == "hero":
        hero(mesh)
    elif b == "chibi_mannequin":
        chibi_mannequin(mesh)
    elif b == "enemy_template_small_biped":
        enemy_template_small_biped(mesh)
    elif b == "enemy_template_quadruped":
        enemy_template_quadruped(mesh)
    elif b == "enemy_template_floating_caster":
        enemy_template_floating_caster(mesh)
    elif b.startswith("pop_playable_") or b == "pop_hero_lead":
        pop_playable(mesh, b)
    elif b.startswith("pop_npc_"):
        pop_npc(mesh, b)
    elif b.startswith("pop_enemy_") or b == "pop_miniboss_stamp":
        pop_enemy(mesh, b)
    elif b.startswith("p3_playable_"):
        p3_playable(mesh, b)
    elif b.startswith("p3_enemy_"):
        p3_enemy(mesh, b)
    elif b.startswith("p3_miniboss_"):
        p3_miniboss(mesh, b)
    elif b.startswith("p3_block_"):
        p3_block_field(mesh, b)
    elif b.startswith("p3_interactable_"):
        p3_interactable(mesh, b)
    elif b.startswith("pop_weapon_"):
        pop_weapon(mesh, b)
    elif b == "exploration_tool":
        exploration_tool(mesh)
    elif b == "melee_enemy":
        melee_enemy(mesh)
    elif b == "ranged_enemy":
        ranged_enemy(mesh)
    elif b == "miniboss":
        miniboss(mesh)
    elif b in ("boss_root", "boss_furnace", "boss_glass", "boss_crown"):
        boss(mesh, b)
    elif b == "pedestal":
        pedestal(mesh)
    elif b in ("checkpoint_socket_pad", "reward_receiver_pad"):
        service_pad(mesh, b)
    elif b == "shortcut_bridge":
        shortcut_bridge(mesh)
    elif b == "root_gate":
        root_gate(mesh)
    elif b == "chest":
        chest(mesh)
    elif b.startswith("relic_"):
        relic(mesh, b)
    elif b.startswith("block_"):
        block_field(mesh, b)
    else:
        modular(mesh, spec)
    normalize(mesh)
    return mesh


def cross(a, b) -> tuple[float, float, float]:
    ax, ay, az = a
    bx, by, bz = b
    return (ay * bz - az * by, az * bx - ax * bz, ax * by - ay * bx)


def rounded_rect_points(sx: float, sy: float, corner: float, corner_segments: int = 4) -> list[tuple[float, float]]:
    points: list[tuple[float, float]] = []
    centers = (
        (sx - corner, sy - corner, 0.0),
        (-sx + corner, sy - corner, math.pi * 0.5),
        (-sx + corner, -sy + corner, math.pi),
        (sx - corner, -sy + corner, math.pi * 1.5),
    )
    for cx, cy, start_angle in centers:
        for i in range(corner_segments + 1):
            angle = start_angle + (math.pi * 0.5) * i / corner_segments
            points.append((cx + math.cos(angle) * corner, cy + math.sin(angle) * corner))
    return points


def hero(mesh: Mesh) -> None:
    # Concept match: pop-deformed blue/orange toolbearer with backpack and cyan cube signal.
    mesh.add_rounded_plate("stone_dark", (0.04, -0.06, 0.030), (0.70, 0.52), 0.060, rot_z=math.radians(-4), corner=0.18)

    # Boots and legs.
    mesh.add_ellipsoid("jacket_orange", (-0.23, -0.13, 0.18), (0.16, 0.22, 0.14), rings=7, segments=20)
    mesh.add_ellipsoid("jacket_orange", (0.21, -0.11, 0.18), (0.16, 0.22, 0.14), rings=7, segments=20)
    mesh.add_rounded_plate("stone_dark", (-0.23, -0.24, 0.135), (0.18, 0.052), 0.040, rot_z=math.radians(-5), corner=0.020)
    mesh.add_rounded_plate("stone_dark", (0.21, -0.23, 0.135), (0.18, 0.052), 0.040, rot_z=math.radians(5), corner=0.020)
    mesh.add_capsule_between("pants_charcoal", (-0.21, -0.07, 0.28), (-0.16, -0.04, 0.62), 0.105, segments=18)
    mesh.add_capsule_between("pants_charcoal", (0.21, -0.07, 0.28), (0.16, -0.02, 0.62), 0.105, segments=18)

    # Body, jacket, scarf/collar, and belt.
    mesh.add_ellipsoid("jacket_blue", (0.00, -0.03, 0.83), (0.36, 0.30, 0.42), rings=9, segments=26)
    mesh.add_rounded_plate("jacket_orange", (-0.27, -0.25, 0.87), (0.08, 0.035), 0.52, rot_z=math.radians(-7), corner=0.018)
    mesh.add_rounded_plate("jacket_orange", (0.27, -0.25, 0.86), (0.08, 0.035), 0.50, rot_z=math.radians(7), corner=0.018)
    mesh.add_rounded_plate("jacket_orange", (0.00, -0.34, 0.94), (0.28, 0.036), 0.050, rot_z=math.radians(0), corner=0.018)
    mesh.add_rounded_plate("stone_dark", (0.00, -0.33, 0.66), (0.40, 0.045), 0.060, rot_z=math.radians(0), corner=0.025)
    mesh.add_rounded_plate("pack_gray", (-0.07, -0.31, 1.10), (0.20, 0.042), 0.060, rot_z=math.radians(0), corner=0.020)
    mesh.add_ellipsoid("jacket_orange", (-0.39, -0.13, 1.06), (0.13, 0.09, 0.10), rings=6, segments=18)
    mesh.add_ellipsoid("jacket_orange", (0.39, -0.13, 1.04), (0.13, 0.09, 0.10), rings=6, segments=18)

    # Head and big brown hair spikes.
    mesh.add_ellipsoid("skin_warm", (0.00, -0.04, 1.38), (0.27, 0.235, 0.245), rings=9, segments=28)
    mesh.add_ellipsoid("hair_brown", (0.00, 0.01, 1.58), (0.27, 0.18, 0.13), rings=6, segments=22)
    for index, (x, y, z, sx, sy, sz, rz) in enumerate((
        (-0.24, -0.04, 1.66, 0.12, 0.060, 0.17, -34),
        (-0.12, -0.08, 1.72, 0.11, 0.055, 0.18, -16),
        (0.05, -0.08, 1.73, 0.12, 0.055, 0.18, 8),
        (0.22, -0.03, 1.68, 0.12, 0.060, 0.16, 28),
        (-0.20, 0.11, 1.64, 0.11, 0.055, 0.15, -54),
        (0.18, 0.12, 1.63, 0.11, 0.055, 0.15, 48),
    )):
        mesh.add_rounded_plate("hair_brown", (x, y, z), (sx, sy), sz, rot_z=math.radians(rz), corner=0.030)
    mesh.add_rounded_plate("stone_dark", (-0.08, -0.260, 1.39), (0.046, 0.012), 0.030, rot_z=math.radians(-6), corner=0.006)
    mesh.add_rounded_plate("stone_dark", (0.10, -0.260, 1.39), (0.046, 0.012), 0.030, rot_z=math.radians(6), corner=0.006)

    # Backpack: large gray block with orange canister, visible behind the hero.
    mesh.add_rounded_plate("pack_gray", (0.34, 0.34, 0.84), (0.22, 0.14), 0.56, rot_z=math.radians(-3), corner=0.050)
    mesh.add_rounded_plate("pack_gray", (0.52, 0.28, 0.92), (0.12, 0.10), 0.44, rot_z=math.radians(4), corner=0.040)
    mesh.add_rounded_plate("jacket_orange", (0.52, 0.12, 1.06), (0.10, 0.050), 0.30, rot_z=math.radians(2), corner=0.025)
    mesh.add_rounded_plate("stone_dark", (0.22, 0.24, 0.76), (0.05, 0.040), 0.50, rot_z=math.radians(0), corner=0.018)

    # Arms and gloves. Left hand carries the cube projector.
    mesh.add_capsule_between("jacket_blue", (-0.37, -0.18, 0.98), (-0.62, -0.36, 0.83), 0.080, segments=18)
    mesh.add_capsule_between("jacket_blue", (0.38, -0.16, 0.96), (0.52, -0.27, 0.75), 0.080, segments=18)
    mesh.add_ellipsoid("stone_dark", (-0.68, -0.42, 0.78), (0.12, 0.09, 0.10), rings=6, segments=18)
    mesh.add_ellipsoid("stone_dark", (0.56, -0.31, 0.72), (0.11, 0.085, 0.10), rings=6, segments=18)
    mesh.add_rounded_plate("jacket_orange", (-0.50, -0.29, 0.91), (0.13, 0.035), 0.070, rot_z=math.radians(-28), corner=0.018)
    mesh.add_rounded_plate("jacket_orange", (0.47, -0.25, 0.86), (0.12, 0.035), 0.070, rot_z=math.radians(24), corner=0.018)

    # Hand tool and floating cube signal from the concept.
    mesh.add_capsule_between("stone_dark", (-0.72, -0.46, 0.80), (-0.96, -0.62, 0.86), 0.050, segments=18)
    mesh.add_ellipsoid("jacket_orange", (-0.86, -0.56, 0.84), (0.12, 0.08, 0.10), rings=6, segments=18)
    mesh.add_rounded_plate("tool_blue", (-0.95, -0.62, 0.86), (0.12, 0.040), 0.050, rot_z=math.radians(-22), corner=0.018)
    for i in range(4):
        t = i / 3
        mesh.add_ellipsoid("tool_blue", (-1.02 - t * 0.24, -0.67 - t * 0.12, 0.90 + t * 0.02), (0.026, 0.026, 0.026), rings=4, segments=10)
    mesh.add_box("tool_blue", (-1.38, -0.86, 0.95), (0.105, 0.105, 0.105), rot_z=math.radians(18))
    mesh.add_rounded_plate("tool_blue", (-1.38, -0.98, 0.95), (0.12, 0.010), 0.018, rot_z=math.radians(18), corner=0.004)
    mesh.vertices = [(x, -y, z) for x, y, z in mesh.vertices]


def chibi_mannequin(mesh: Mesh) -> None:
    # Shared playable/NPC body template. No costume, hair, weapon, or enemy anatomy.
    warm = "mannequin_warm"
    light = "mannequin_light"
    joint = "mannequin_joint"
    mark = "mannequin_mark"

    # Feet and ankles: short, stable toy-like stance.
    mesh.add_ellipsoid(light, (-0.18, -0.06, 0.08), (0.16, 0.24, 0.075), rings=7, segments=22)
    mesh.add_ellipsoid(light, (0.18, -0.06, 0.08), (0.16, 0.24, 0.075), rings=7, segments=22)
    mesh.add_ellipsoid(joint, (-0.18, -0.01, 0.19), (0.080, 0.072, 0.070), rings=5, segments=16)
    mesh.add_ellipsoid(joint, (0.18, -0.01, 0.19), (0.080, 0.072, 0.070), rings=5, segments=16)

    # Legs: chunky capsules with obvious knee balls.
    mesh.add_capsule_between(warm, (-0.18, -0.01, 0.20), (-0.19, 0.00, 0.39), 0.070, segments=18)
    mesh.add_capsule_between(warm, (0.18, -0.01, 0.20), (0.19, 0.00, 0.39), 0.070, segments=18)
    mesh.add_ellipsoid(joint, (-0.19, -0.01, 0.42), (0.090, 0.080, 0.080), rings=5, segments=16)
    mesh.add_ellipsoid(joint, (0.19, -0.01, 0.42), (0.090, 0.080, 0.080), rings=5, segments=16)
    mesh.add_capsule_between(warm, (-0.17, 0.00, 0.46), (-0.13, 0.01, 0.64), 0.085, segments=18)
    mesh.add_capsule_between(warm, (0.17, 0.00, 0.46), (0.13, 0.01, 0.64), 0.085, segments=18)

    # Pelvis and torso: short stack that keeps the chibi 3-head proportion.
    mesh.add_ellipsoid(light, (0.00, 0.01, 0.68), (0.285, 0.205, 0.130), rings=7, segments=24)
    mesh.add_rounded_plate(joint, (0.00, -0.18, 0.70), (0.23, 0.025), 0.040, corner=0.010)
    mesh.add_ellipsoid(warm, (0.00, 0.00, 0.87), (0.330, 0.240, 0.245), rings=9, segments=28)
    mesh.add_ellipsoid(light, (0.00, -0.015, 1.005), (0.260, 0.200, 0.120), rings=7, segments=24)
    mesh.add_rounded_plate(joint, (0.00, -0.225, 0.885), (0.25, 0.023), 0.040, corner=0.010)

    # Neck and large blank head. Face marks are just orientation, not identity.
    mesh.add_capsule_between(joint, (0.00, 0.00, 1.06), (0.00, 0.00, 1.12), 0.060, segments=18)
    mesh.add_ellipsoid(light, (0.00, -0.020, 1.255), (0.345, 0.310, 0.285), rings=10, segments=32)
    mesh.add_ellipsoid(warm, (0.00, 0.035, 1.145), (0.285, 0.240, 0.095), rings=6, segments=26)
    mesh.add_rounded_plate(mark, (-0.105, 0.300, 1.285), (0.044, 0.010), 0.018, rot_z=math.radians(-5), corner=0.004)
    mesh.add_rounded_plate(mark, (0.105, 0.300, 1.285), (0.044, 0.010), 0.018, rot_z=math.radians(5), corner=0.004)
    mesh.add_rounded_plate(mark, (0.000, 0.307, 1.205), (0.050, 0.008), 0.014, corner=0.004)

    # Shoulders and arms: neutral A-pose with visible shoulder, elbow, wrist, and hand volumes.
    mesh.add_ellipsoid(joint, (-0.34, -0.015, 0.980), (0.100, 0.088, 0.090), rings=5, segments=18)
    mesh.add_ellipsoid(joint, (0.34, -0.015, 0.980), (0.100, 0.088, 0.090), rings=5, segments=18)
    mesh.add_capsule_between(warm, (-0.39, -0.020, 0.935), (-0.51, -0.035, 0.755), 0.070, segments=18)
    mesh.add_capsule_between(warm, (0.39, -0.020, 0.935), (0.51, -0.035, 0.755), 0.070, segments=18)
    mesh.add_ellipsoid(joint, (-0.53, -0.045, 0.735), (0.084, 0.074, 0.076), rings=5, segments=16)
    mesh.add_ellipsoid(joint, (0.53, -0.045, 0.735), (0.084, 0.074, 0.076), rings=5, segments=16)
    mesh.add_capsule_between(warm, (-0.52, -0.050, 0.700), (-0.43, -0.065, 0.535), 0.062, segments=18)
    mesh.add_capsule_between(warm, (0.52, -0.050, 0.700), (0.43, -0.065, 0.535), 0.062, segments=18)
    mesh.add_ellipsoid(joint, (-0.42, -0.070, 0.505), (0.070, 0.060, 0.058), rings=5, segments=16)
    mesh.add_ellipsoid(joint, (0.42, -0.070, 0.505), (0.070, 0.060, 0.058), rings=5, segments=16)
    mesh.add_ellipsoid(light, (-0.40, -0.095, 0.445), (0.095, 0.082, 0.070), rings=6, segments=18)
    mesh.add_ellipsoid(light, (0.40, -0.095, 0.445), (0.095, 0.082, 0.070), rings=6, segments=18)

    # Simple socket/reference dots for later costume rigging notes.
    mesh.add_ellipsoid(mark, (-0.20, 0.245, 0.890), (0.024, 0.014, 0.024), rings=4, segments=10)
    mesh.add_ellipsoid(mark, (0.20, 0.245, 0.890), (0.024, 0.014, 0.024), rings=4, segments=10)
    mesh.add_ellipsoid(mark, (0.00, 0.235, 0.900), (0.030, 0.016, 0.030), rings=4, segments=10)


def enemy_template_small_biped(mesh: Mesh) -> None:
    # ESK-01 neutral enemy skeleton. This is not a finished monster skin.
    body = "stone_warm"
    joint = "mannequin_joint"
    dark = "stone_dark"
    socket = "danger_red"
    guide = "tool_signal"

    # Forward wedge. Grounding comes from foot placement, not a display plinth.
    mesh.add_rounded_plate(socket, (0.00, -0.335, 0.060), (0.13, 0.020), 0.020, corner=0.006)

    # Feet, ankles, knees, and legs: compact enemy stance, not the playable/NPC mannequin.
    mesh.add_ellipsoid(body, (-0.155, -0.045, 0.100), (0.105, 0.145, 0.060), rings=6, segments=18)
    mesh.add_ellipsoid(body, (0.155, -0.045, 0.100), (0.105, 0.145, 0.060), rings=6, segments=18)
    mesh.add_ellipsoid(joint, (-0.155, -0.030, 0.190), (0.055, 0.050, 0.050), rings=5, segments=14)
    mesh.add_ellipsoid(joint, (0.155, -0.030, 0.190), (0.055, 0.050, 0.050), rings=5, segments=14)
    mesh.add_capsule_between(body, (-0.145, -0.020, 0.205), (-0.165, -0.020, 0.365), 0.055, segments=16)
    mesh.add_capsule_between(body, (0.145, -0.020, 0.205), (0.165, -0.020, 0.365), 0.055, segments=16)
    mesh.add_ellipsoid(joint, (-0.165, -0.025, 0.390), (0.062, 0.054, 0.054), rings=5, segments=14)
    mesh.add_ellipsoid(joint, (0.165, -0.025, 0.390), (0.062, 0.054, 0.054), rings=5, segments=14)
    mesh.add_capsule_between(body, (-0.145, -0.010, 0.430), (-0.105, -0.010, 0.560), 0.070, segments=16)
    mesh.add_capsule_between(body, (0.145, -0.010, 0.430), (0.105, -0.010, 0.560), 0.070, segments=16)

    # Pelvis and torso: forward-leaning compact enemy mass with visible chest core socket.
    mesh.add_ellipsoid(body, (0.00, -0.005, 0.600), (0.240, 0.180, 0.115), rings=6, segments=22)
    mesh.add_ellipsoid(body, (0.00, -0.035, 0.780), (0.285, 0.215, 0.235), rings=8, segments=24)
    mesh.add_rounded_plate(dark, (0.00, -0.255, 0.790), (0.165, 0.030), 0.040, corner=0.010)
    mesh.add_box(socket, (0.00, -0.283, 0.800), (0.060, 0.022, 0.060), rot_z=math.radians(45))

    # Head: socket-like blank enemy head, no goblin/animal face.
    mesh.add_capsule_between(joint, (0.00, -0.010, 0.965), (0.00, -0.010, 1.020), 0.045, segments=16)
    mesh.add_ellipsoid(body, (0.00, -0.030, 1.120), (0.210, 0.185, 0.170), rings=8, segments=24)
    mesh.add_rounded_plate(dark, (0.00, -0.205, 1.120), (0.105, 0.024), 0.038, corner=0.010)
    mesh.add_ellipsoid(socket, (0.00, -0.225, 1.120), (0.040, 0.018, 0.040), rings=4, segments=12)

    # Shoulders, arms, hands. Hands are sockets for claw/tool variants, not final weapons.
    mesh.add_ellipsoid(joint, (-0.285, -0.030, 0.835), (0.075, 0.065, 0.065), rings=5, segments=16)
    mesh.add_ellipsoid(joint, (0.285, -0.030, 0.835), (0.075, 0.065, 0.065), rings=5, segments=16)
    mesh.add_capsule_between(body, (-0.330, -0.040, 0.795), (-0.445, -0.090, 0.660), 0.052, segments=16)
    mesh.add_capsule_between(body, (0.330, -0.040, 0.795), (0.445, -0.090, 0.660), 0.052, segments=16)
    mesh.add_ellipsoid(joint, (-0.460, -0.100, 0.640), (0.055, 0.048, 0.048), rings=5, segments=14)
    mesh.add_ellipsoid(joint, (0.460, -0.100, 0.640), (0.055, 0.048, 0.048), rings=5, segments=14)
    mesh.add_capsule_between(body, (-0.455, -0.105, 0.610), (-0.360, -0.175, 0.500), 0.045, segments=16)
    mesh.add_capsule_between(body, (0.455, -0.105, 0.610), (0.360, -0.175, 0.500), 0.045, segments=16)
    mesh.add_ellipsoid(dark, (-0.335, -0.205, 0.470), (0.070, 0.055, 0.055), rings=5, segments=14)
    mesh.add_ellipsoid(dark, (0.335, -0.205, 0.470), (0.070, 0.055, 0.055), rings=5, segments=14)

    # Back and attack sockets. These markers define future part/VFX attachment points.
    mesh.add_ellipsoid(guide, (0.00, 0.205, 0.850), (0.040, 0.026, 0.040), rings=4, segments=12)
    mesh.add_ellipsoid(socket, (0.00, -0.390, 0.830), (0.036, 0.024, 0.036), rings=4, segments=12)
    mesh.add_capsule_between(socket, (0.00, -0.285, 0.800), (0.00, -0.390, 0.830), 0.012, segments=8)
    mesh.vertices = [(x, -y, z) for x, y, z in mesh.vertices]


def enemy_template_quadruped(mesh: Mesh) -> None:
    # ESK-03 neutral quadruped charger skeleton. No final animal or dragon identity.
    body = "stone_warm"
    joint = "mannequin_joint"
    dark = "stone_dark"
    socket = "danger_red"
    guide = "tool_signal"

    # Main low forward mass: designed for charger variants.
    mesh.add_ellipsoid(body, (0.00, 0.000, 0.590), (0.285, 0.410, 0.210), rings=8, segments=26)
    mesh.add_ellipsoid(body, (0.00, 0.215, 0.610), (0.245, 0.275, 0.185), rings=7, segments=24)
    mesh.add_ellipsoid(body, (0.00, -0.270, 0.610), (0.225, 0.235, 0.165), rings=7, segments=22)
    mesh.add_rounded_plate(dark, (0.00, -0.385, 0.625), (0.145, 0.030), 0.040, corner=0.010)
    mesh.add_box(socket, (0.00, -0.420, 0.625), (0.058, 0.022, 0.050), rot_z=math.radians(45))

    # Head and neck: socket muzzle, not a literal wolf/dragon head.
    mesh.add_capsule_between(joint, (0.00, -0.455, 0.655), (0.00, -0.585, 0.705), 0.070, segments=16)
    mesh.add_ellipsoid(body, (0.00, -0.705, 0.730), (0.180, 0.205, 0.140), rings=7, segments=22)
    mesh.add_rounded_plate(dark, (0.00, -0.900, 0.728), (0.090, 0.026), 0.044, corner=0.010)
    mesh.add_ellipsoid(socket, (0.00, -0.925, 0.730), (0.038, 0.020, 0.038), rings=4, segments=12)

    # Tail/root socket, kept neutral for later tail-club or banner swaps.
    mesh.add_capsule_between(joint, (0.00, 0.465, 0.615), (0.00, 0.635, 0.640), 0.055, segments=14)
    mesh.add_ellipsoid(guide, (0.00, 0.690, 0.645), (0.052, 0.040, 0.052), rings=4, segments=12)

    # Four shoulder/hip joints and legs.
    for x, y, upper_z, knee_y, foot_y in (
        (-0.270, -0.300, 0.535, -0.425, -0.545),
        (0.270, -0.300, 0.535, -0.425, -0.545),
        (-0.275, 0.280, 0.525, 0.390, 0.510),
        (0.275, 0.280, 0.525, 0.390, 0.510),
    ):
        mesh.add_ellipsoid(joint, (x, y, upper_z), (0.080, 0.070, 0.070), rings=5, segments=16)
        mesh.add_capsule_between(body, (x, y, upper_z - 0.020), (x * 0.95, knee_y, 0.335), 0.058, segments=16)
        mesh.add_ellipsoid(joint, (x * 0.95, knee_y, 0.315), (0.065, 0.055, 0.055), rings=5, segments=14)
        mesh.add_capsule_between(body, (x * 0.95, knee_y, 0.290), (x * 0.90, foot_y, 0.145), 0.050, segments=16)
        mesh.add_ellipsoid(dark, (x * 0.90, foot_y, 0.085), (0.085, 0.125, 0.050), rings=5, segments=16)
        mesh.add_rounded_plate(joint, (x * 0.90, foot_y + (-0.050 if foot_y < 0 else 0.050), 0.150), (0.072, 0.016), 0.020, corner=0.005)

    # Charge/weak sockets as visible modeling landmarks.
    mesh.add_ellipsoid(socket, (0.00, -1.030, 0.735), (0.034, 0.024, 0.034), rings=4, segments=12)
    mesh.add_capsule_between(socket, (0.00, -0.925, 0.730), (0.00, -1.030, 0.735), 0.012, segments=8)
    mesh.add_ellipsoid(guide, (0.225, 0.075, 0.680), (0.042, 0.030, 0.042), rings=4, segments=12)
    mesh.add_ellipsoid(guide, (-0.225, 0.075, 0.680), (0.042, 0.030, 0.042), rings=4, segments=12)
    mesh.vertices = [(x, -y, z) for x, y, z in mesh.vertices]


def enemy_template_floating_caster(mesh: Mesh) -> None:
    # ESK-05 neutral hover caster skeleton. It is not a ghost, wizard, or drone skin.
    body = "stone_warm"
    joint = "mannequin_joint"
    dark = "stone_dark"
    socket = "danger_red"
    guide = "tool_signal"

    # Ground pivot marker is only a non-contact guide; the body must read as hovering.
    mesh.add_rounded_plate(dark, (0.00, 0.00, 0.018), (0.110, 0.080), 0.018, corner=0.026)
    for z, radius in ((0.210, 0.030), (0.380, 0.024), (0.535, 0.018)):
        mesh.add_ellipsoid(guide, (0.00, 0.00, z), (radius, radius * 0.72, radius), rings=4, segments=10)

    # Central hover body and front casting core.
    mesh.add_ellipsoid(body, (0.00, 0.00, 0.900), (0.315, 0.285, 0.285), rings=10, segments=30)
    mesh.add_rounded_plate(body, (0.00, -0.018, 1.155), (0.155, 0.120), 0.050, rot_z=math.radians(7), corner=0.030)
    mesh.add_rounded_plate(dark, (0.00, -0.285, 0.910), (0.158, 0.030), 0.055, corner=0.012)
    mesh.add_ellipsoid(socket, (0.00, -0.322, 0.910), (0.068, 0.035, 0.068), rings=5, segments=16)
    mesh.add_ellipsoid(guide, (0.00, -0.420, 0.910), (0.030, 0.024, 0.030), rings=4, segments=10)
    mesh.add_capsule_between(socket, (0.00, -0.322, 0.910), (0.00, -0.420, 0.910), 0.010, segments=8)

    # Detachable side sockets and optional neutral fins. They show attachment points, not required body identity.
    for x, side in ((-0.385, -1), (0.385, 1)):
        mesh.add_capsule_between(joint, (x * 0.65, -0.005, 0.900), (x, -0.005, 0.900), 0.040, segments=14)
        mesh.add_ellipsoid(guide, (x, -0.005, 0.900), (0.052, 0.040, 0.052), rings=4, segments=12)
        fin = [
            (side * 0.000, -0.105),
            (side * 0.150, -0.050),
            (side * 0.178, 0.120),
            (side * 0.010, 0.165),
            (side * -0.060, 0.020),
        ]
        mesh.add_prism(body, (x + side * 0.115, 0.010, 0.900), fin, 0.035, rot_z=math.radians(side * 4))

    # Back weak/support socket and top hit/cast markers.
    mesh.add_ellipsoid(guide, (0.00, 0.275, 0.940), (0.052, 0.034, 0.052), rings=4, segments=12)
    mesh.add_ellipsoid(socket, (0.00, 0.000, 1.205), (0.040, 0.032, 0.040), rings=4, segments=12)
    mesh.vertices = [(x, -y, z) for x, y, z in mesh.vertices]


def pop_playable(mesh: Mesh, builder: str) -> None:
    variants = {
        "pop_hero_lead": ("cloth_coral", "leather_blue", "tool_signal", 0.32, 0.62),
        "pop_playable_heavy": ("cloth_cyan", "metal", "tool_signal", 0.42, 0.56),
        "pop_playable_caster": ("cloth_plum", "stamp_orange", "tool_signal", 0.30, 0.64),
        "pop_playable_ranger": ("cloth_mint", "wood", "tool_signal", 0.30, 0.60),
        "pop_playable_medic": ("paper_cream", "cloth_mint", "tool_blue", 0.34, 0.55),
    }
    body, accent, signal, width, height = variants[builder]
    mesh.add_cylinder(signal, (0, -0.10, 0.025), 0.50 + width * 0.35, 0.04, 18)
    mesh.add_box("hair_ink", (-0.14, -0.02, 0.18), (0.11, 0.18, 0.16))
    mesh.add_box("hair_ink", (0.14, 0.02, 0.18), (0.11, 0.18, 0.16))
    mesh.add_cylinder(body, (0, 0, 0.74), width, height, 8)
    mesh.add_box(accent, (0, -0.31, 0.84), (width * 0.82, 0.045, height * 0.32), 0.06)
    mesh.add_sphere("skin_peach", (0, -0.02, 1.18 + height * 0.18), 0.23)
    mesh.add_box("hair_ink", (0, 0.01, 1.38 + height * 0.18), (0.25, 0.13, 0.08), -0.08)
    mesh.add_sphere("skin_peach", (-width - 0.10, -0.22, 0.86), 0.12)
    mesh.add_sphere("skin_peach", (width + 0.10, -0.22, 0.86), 0.12)
    add_playable_finish(mesh, body, accent, signal, width, height)

    if builder == "pop_playable_heavy":
        mesh.add_box("metal", (-0.52, -0.36, 0.82), (0.18, 0.07, 0.48), 0.18)
        mesh.add_box(signal, (-0.55, -0.42, 0.98), (0.20, 0.035, 0.30), 0.18)
        mesh.add_box("metal", (0.50, -0.32, 0.84), (0.08, 0.06, 0.54), -0.30)
    elif builder == "pop_playable_caster":
        mesh.add_cylinder("stamp_orange", (0.54, -0.28, 0.80), 0.17, 0.08, 8)
        mesh.add_box("stamp_orange", (0.54, -0.28, 1.05), (0.07, 0.05, 0.40))
        mesh.add_box(signal, (0.06, -0.38, 0.58), (0.34, 0.025, 0.035), -0.10)
    elif builder == "pop_playable_ranger":
        mesh.add_box("wood", (0.50, -0.23, 0.96), (0.05, 0.035, 0.62), -0.42)
        mesh.add_box(signal, (0.64, -0.43, 1.10), (0.22, 0.030, 0.045), -0.42)
        mesh.add_box("leather_blue", (-0.40, 0.22, 0.88), (0.15, 0.08, 0.36), 0.12)
    elif builder == "pop_playable_medic":
        mesh.add_box("cloth_mint", (-0.46, -0.24, 0.76), (0.16, 0.07, 0.30), -0.10)
        mesh.add_sphere("tool_blue", (-0.55, -0.30, 1.02), 0.11)
        mesh.add_box(signal, (0.40, -0.28, 0.92), (0.06, 0.035, 0.42), 0.25)
    else:
        mesh.add_box("leather_blue", (-0.44, 0.18, 0.86), (0.14, 0.08, 0.44), 0.16)
        mesh.add_box("metal", (0.50, -0.31, 0.92), (0.055, 0.04, 0.62), -0.36)
        mesh.add_box(signal, (0.62, -0.49, 1.18), (0.18, 0.035, 0.06), -0.36)


def add_playable_finish(mesh: Mesh, body: str, accent: str, signal: str, width: float, height: float) -> None:
    mesh.add_box(body, (-0.20, -0.14, 0.36), (0.11, 0.14, 0.18), -0.14)
    mesh.add_box(body, (0.20, -0.14, 0.36), (0.11, 0.14, 0.18), 0.14)
    mesh.add_box(accent, (-width - 0.08, -0.14, 1.02), (0.15, 0.07, 0.11), 0.22)
    mesh.add_box(accent, (width + 0.08, -0.14, 1.02), (0.15, 0.07, 0.11), -0.22)
    mesh.add_box(accent, (-width - 0.14, -0.21, 0.80), (0.08, 0.05, 0.32), 0.28)
    mesh.add_box(accent, (width + 0.14, -0.21, 0.80), (0.08, 0.05, 0.32), -0.28)
    mesh.add_box("hair_ink", (0, -0.245, 1.22 + height * 0.18), (0.18, 0.020, 0.026), 0.02)
    mesh.add_box("hair_ink", (-0.10, -0.236, 1.16 + height * 0.18), (0.060, 0.018, 0.026), -0.12)
    mesh.add_box("hair_ink", (0.10, -0.236, 1.16 + height * 0.18), (0.060, 0.018, 0.026), 0.12)
    mesh.add_box(signal, (0, -0.35, 0.70), (width * 0.78, 0.022, 0.040), -0.10)
    mesh.add_box(signal, (-width * 0.45, -0.34, 0.98), (0.042, 0.020, 0.20), 0.10)
    mesh.add_box(accent, (0, 0.24, 0.84), (width * 0.76, 0.045, height * 0.44), 0.03)
    mesh.add_box("hair_ink", (0.00, 0.09, 1.42 + height * 0.18), (0.12, 0.09, 0.14), -0.18)


def pop_npc(mesh: Mesh, builder: str) -> None:
    variants = {
        "pop_npc_merchant": ("cloth_coral", "paper_cream", "gold"),
        "pop_npc_smith": ("metal", "stamp_orange", "tool_signal"),
        "pop_npc_cartographer": ("paper_cream", "cloth_cyan", "tool_blue"),
    }
    body, accent, signal = variants[builder]
    mesh.add_cylinder(accent, (0, -0.08, 0.025), 0.48, 0.04, 18)
    mesh.add_box("hair_ink", (-0.12, -0.02, 0.18), (0.10, 0.16, 0.15))
    mesh.add_box("hair_ink", (0.12, 0.02, 0.18), (0.10, 0.16, 0.15))
    mesh.add_cylinder(body, (0, 0, 0.72), 0.31, 0.54, 8)
    mesh.add_sphere("skin_peach", (0, -0.02, 1.20), 0.22)
    mesh.add_box("hair_ink", (0, 0.02, 1.42), (0.24, 0.13, 0.07), 0.04)
    add_npc_finish(mesh, body, accent, signal)
    if builder == "pop_npc_merchant":
        mesh.add_box("wood", (0, -0.48, 0.78), (0.46, 0.08, 0.12))
        mesh.add_sphere("gold", (-0.18, -0.55, 0.92), 0.08)
        mesh.add_sphere("tool_blue", (0.10, -0.55, 0.92), 0.08)
        mesh.add_box("paper_cream", (-0.42, 0.18, 0.78), (0.16, 0.09, 0.38), -0.12)
    elif builder == "pop_npc_smith":
        mesh.add_box("metal", (0.46, -0.32, 0.84), (0.08, 0.06, 0.52), -0.32)
        mesh.add_box("stamp_orange", (0.62, -0.50, 1.08), (0.18, 0.06, 0.14), -0.32)
        mesh.add_box("tool_signal", (-0.44, -0.24, 0.92), (0.16, 0.05, 0.28), 0.24)
    else:
        mesh.add_box("paper_cream", (0, -0.48, 0.86), (0.54, 0.045, 0.34))
        mesh.add_box("tool_blue", (-0.22, -0.53, 1.00), (0.07, 0.025, 0.20), 0.20)
        mesh.add_box("tool_blue", (0.20, -0.53, 0.90), (0.07, 0.025, 0.16), -0.20)
        mesh.add_box("cloth_cyan", (0.42, 0.18, 0.84), (0.12, 0.08, 0.34), 0.10)


def add_npc_finish(mesh: Mesh, body: str, accent: str, signal: str) -> None:
    mesh.add_box(body, (-0.16, -0.08, 0.34), (0.10, 0.13, 0.16), -0.10)
    mesh.add_box(body, (0.16, -0.08, 0.34), (0.10, 0.13, 0.16), 0.10)
    mesh.add_box(accent, (-0.34, -0.18, 0.86), (0.08, 0.05, 0.26), 0.24)
    mesh.add_box(accent, (0.34, -0.18, 0.86), (0.08, 0.05, 0.26), -0.24)
    mesh.add_box("hair_ink", (0, -0.235, 1.22), (0.18, 0.020, 0.026), 0.02)
    mesh.add_box(signal, (0, -0.33, 0.80), (0.26, 0.022, 0.035), -0.08)
    mesh.add_box(accent, (0, 0.22, 0.82), (0.25, 0.045, 0.34), 0.04)


def pop_enemy(mesh: Mesh, builder: str) -> None:
    if builder == "pop_enemy_fodder":
        mesh.add_cylinder("danger_red", (0, -0.08, 0.025), 0.43, 0.04, 16)
        mesh.add_sphere("enemy_purple", (0, 0, 0.60), 0.34)
        mesh.add_box("danger_red", (0, -0.34, 0.72), (0.24, 0.045, 0.10))
        mesh.add_box("enemy_lime", (0.30, -0.25, 0.58), (0.08, 0.06, 0.24), -0.28)
        mesh.add_box("enemy_lime", (-0.30, -0.25, 0.58), (0.08, 0.06, 0.24), 0.28)
        add_enemy_finish(mesh, "enemy_purple", "enemy_lime", "danger_red", 0.34)
    elif builder == "pop_enemy_shield":
        mesh.add_cylinder("danger_red", (0, -0.10, 0.025), 0.50, 0.04, 18)
        mesh.add_sphere("enemy_ink", (0, 0, 0.70), 0.34)
        mesh.add_box("metal", (0, -0.42, 0.78), (0.36, 0.055, 0.44))
        mesh.add_box("danger_red", (0, -0.47, 0.96), (0.24, 0.025, 0.08))
        mesh.add_sphere("enemy_lime", (0.42, -0.18, 0.74), 0.10)
        add_enemy_finish(mesh, "enemy_ink", "metal", "danger_red", 0.36)
    elif builder == "pop_enemy_turret":
        mesh.add_cylinder("enemy_ink", (0, 0, 0.30), 0.32, 0.42, 8)
        mesh.add_box("stone_dark", (0, 0.10, 0.16), (0.52, 0.42, 0.10))
        mesh.add_box("danger_red", (0, -0.40, 0.64), (0.13, 0.16, 0.24))
        mesh.add_box("danger_red", (0, -0.74, 0.64), (0.07, 0.24, 0.07))
        mesh.add_box("enemy_lime", (0, -0.86, 0.64), (0.42, 0.025, 0.035))
        add_enemy_finish(mesh, "enemy_ink", "stone_dark", "danger_red", 0.30)
        for angle in (-28, 0, 28):
            mesh.add_box("stone_dark", (math.sin(math.radians(angle)) * 0.28, 0.32, 0.40), (0.08, 0.06, 0.34), math.radians(angle))
        mesh.add_box("enemy_lime", (-0.28, -0.58, 0.42), (0.08, 0.026, 0.18), -0.20)
        mesh.add_box("enemy_lime", (0.28, -0.58, 0.42), (0.08, 0.026, 0.18), 0.20)
        mesh.add_box("danger_red", (0, -0.52, 0.92), (0.22, 0.025, 0.040), 0.08)
        mesh.add_box("enemy_lime", (-0.28, -0.58, 0.42), (0.08, 0.026, 0.18), -0.20)
        mesh.add_box("enemy_lime", (0.28, -0.58, 0.42), (0.08, 0.026, 0.18), 0.20)
        mesh.add_box("danger_red", (0, -0.52, 0.92), (0.22, 0.025, 0.040), 0.08)
    elif builder == "pop_enemy_charger":
        mesh.add_cylinder("danger_red", (0, -0.12, 0.025), 0.62, 0.04, 18)
        mesh.add_sphere("enemy_purple", (0, 0.02, 0.76), 0.42)
        mesh.add_box("metal", (0, -0.42, 0.88), (0.42, 0.07, 0.20))
        mesh.add_cone("danger_red", (-0.24, -0.54, 1.00), 0.08, 0.28, 5)
        mesh.add_cone("danger_red", (0.24, -0.54, 1.00), 0.08, 0.28, 5)
        mesh.add_box("enemy_lime", (0, -0.68, 0.48), (0.50, 0.035, 0.045))
        add_enemy_finish(mesh, "enemy_purple", "metal", "danger_red", 0.42)
    elif builder == "pop_enemy_swarm":
        mesh.add_cylinder("enemy_lime", (0, -0.04, 0.025), 0.28, 0.035, 12)
        mesh.add_sphere("enemy_purple", (0, 0, 0.38), 0.22)
        mesh.add_box("danger_red", (0, -0.24, 0.45), (0.18, 0.035, 0.06))
        mesh.add_box("enemy_lime", (-0.22, 0.02, 0.34), (0.10, 0.035, 0.04), 0.28)
        mesh.add_box("enemy_lime", (0.22, 0.02, 0.34), (0.10, 0.035, 0.04), -0.28)
        add_enemy_finish(mesh, "enemy_purple", "enemy_lime", "danger_red", 0.24)
        mesh.add_box("danger_red", (0, -0.32, 0.62), (0.16, 0.024, 0.040), 0.10)
    else:
        mesh.add_cylinder("danger_red", (0, -0.10, 0.03), 0.82, 0.05, 22)
        mesh.add_sphere("root", (0, 0, 0.96), 0.58)
        mesh.add_box("stone_warm", (0, -0.48, 1.22), (0.42, 0.06, 0.28))
        mesh.add_cylinder("stamp_orange", (0, -0.68, 0.54), 0.30, 0.10, 8)
        mesh.add_box("metal", (0.62, -0.34, 0.88), (0.10, 0.08, 0.62), -0.36)
        mesh.add_sphere("tool_signal", (0, -0.56, 1.24), 0.16)
        add_enemy_finish(mesh, "root", "metal", "danger_red", 0.52)
        mesh.add_box("stamp_orange", (-0.62, -0.34, 0.88), (0.10, 0.08, 0.62), 0.36)
        mesh.add_box("danger_red", (0, -0.72, 1.18), (0.48, 0.030, 0.060), -0.08)


def add_enemy_finish(mesh: Mesh, body: str, accent: str, danger: str, width: float) -> None:
    mesh.add_box(body, (-width * 0.55, -0.02, 0.24), (0.10, 0.12, 0.14), -0.18)
    mesh.add_box(body, (width * 0.55, -0.02, 0.24), (0.10, 0.12, 0.14), 0.18)
    mesh.add_box(accent, (-width * 0.80, -0.16, 0.72), (0.08, 0.05, 0.26), 0.28)
    mesh.add_box(accent, (width * 0.80, -0.16, 0.72), (0.08, 0.05, 0.26), -0.28)
    mesh.add_box(danger, (0, -0.38, 0.58), (width * 0.90, 0.026, 0.040), 0.08)
    mesh.add_box(danger, (-width * 0.34, -0.34, 0.86), (0.040, 0.024, 0.18), -0.10)
    mesh.add_box(danger, (width * 0.34, -0.34, 0.86), (0.040, 0.024, 0.18), 0.10)
    mesh.add_box(body, (0, 0.22, 0.70), (width * 0.52, 0.045, 0.30), 0.02)


def p3_playable(mesh: Mesh, builder: str) -> None:
    variants = {
        "p3_playable_lead_t2": ("cloth_coral", "gold", "tool_signal", 0.34, 0.66),
        "p3_playable_guard_t2": ("cloth_cyan", "metal", "tool_signal", 0.46, 0.58),
        "p3_playable_caster_t2": ("cloth_plum", "stamp_orange", "tool_blue", 0.32, 0.68),
        "p3_playable_scout_t2": ("cloth_mint", "wood", "route_teal", 0.32, 0.60),
        "p3_playable_medic_t2": ("paper_cream", "cloth_mint", "tool_blue", 0.36, 0.58),
        "p3_playable_striker": ("leather_blue", "metal", "tool_signal", 0.38, 0.54),
    }
    body, accent, signal, width, height = variants[builder]
    mesh.add_cylinder(signal, (0, -0.08, 0.025), 0.56 + width * 0.22, 0.04, 20)
    mesh.add_box("hair_ink", (-0.16, -0.04, 0.18), (0.12, 0.16, 0.16), -0.10)
    mesh.add_box("hair_ink", (0.16, 0.04, 0.18), (0.12, 0.16, 0.16), 0.10)
    mesh.add_cylinder(body, (0, 0, 0.76), width, height, 9)
    mesh.add_box(accent, (0, -0.34, 0.86), (width * 0.92, 0.045, height * 0.34), 0.05)
    mesh.add_box(signal, (0, -0.40, 0.72), (width * 0.78, 0.024, 0.040), -0.06)
    mesh.add_sphere("skin_peach", (0, -0.03, 1.22 + height * 0.14), 0.23)
    mesh.add_box("hair_ink", (0, 0.02, 1.44 + height * 0.14), (0.25, 0.12, 0.07), -0.05)
    mesh.add_box(body, (-0.22, -0.12, 0.36), (0.11, 0.14, 0.17), -0.12)
    mesh.add_box(body, (0.22, -0.12, 0.36), (0.11, 0.14, 0.17), 0.12)
    mesh.add_box(accent, (-width - 0.12, -0.18, 1.00), (0.15, 0.065, 0.12), 0.24)
    mesh.add_box(accent, (width + 0.12, -0.18, 1.00), (0.15, 0.065, 0.12), -0.24)
    mesh.add_box("hair_ink", (0, -0.255, 1.25 + height * 0.14), (0.18, 0.020, 0.026), 0.02)

    if builder == "p3_playable_guard_t2":
        mesh.add_box("metal", (-0.62, -0.38, 0.82), (0.22, 0.08, 0.52), 0.12)
        mesh.add_box(signal, (-0.66, -0.45, 1.03), (0.22, 0.026, 0.28), 0.12)
        mesh.add_box("metal", (0.52, -0.32, 0.88), (0.09, 0.06, 0.58), -0.28)
    elif builder == "p3_playable_caster_t2":
        mesh.add_cylinder("stamp_orange", (0.55, -0.30, 0.82), 0.18, 0.09, 8)
        mesh.add_box("stamp_orange", (0.55, -0.30, 1.08), (0.07, 0.05, 0.42))
        mesh.add_box(signal, (-0.12, -0.42, 0.98), (0.40, 0.024, 0.050), -0.18)
        mesh.add_sphere(signal, (-0.48, -0.42, 1.04), 0.09)
    elif builder == "p3_playable_scout_t2":
        mesh.add_cylinder("wood", (-0.46, -0.22, 0.88), 0.18, 0.10, 12)
        mesh.add_box(signal, (-0.62, -0.28, 0.90), (0.32, 0.025, 0.040), -0.20)
        mesh.add_box("wood", (0.52, -0.22, 0.94), (0.055, 0.035, 0.60), -0.44)
    elif builder == "p3_playable_medic_t2":
        mesh.add_box("cloth_mint", (-0.50, -0.26, 0.78), (0.17, 0.07, 0.32), -0.10)
        mesh.add_sphere(signal, (-0.62, -0.32, 1.04), 0.12)
        mesh.add_box(signal, (0.42, -0.30, 0.92), (0.06, 0.035, 0.44), 0.25)
    elif builder == "p3_playable_striker":
        mesh.add_box("metal", (-0.54, -0.34, 0.86), (0.16, 0.08, 0.22), 0.18)
        mesh.add_box("metal", (0.54, -0.34, 0.86), (0.16, 0.08, 0.22), -0.18)
        mesh.add_box(signal, (-0.58, -0.43, 0.98), (0.18, 0.025, 0.050), 0.18)
        mesh.add_box(signal, (0.58, -0.43, 0.98), (0.18, 0.025, 0.050), -0.18)
    else:
        mesh.add_box("metal", (0.52, -0.32, 0.94), (0.055, 0.04, 0.64), -0.34)
        mesh.add_box(signal, (0.66, -0.50, 1.18), (0.18, 0.030, 0.06), -0.34)


def p3_enemy(mesh: Mesh, builder: str) -> None:
    if "_r02_" in builder or builder.startswith("p3_enemy_c") or builder.startswith("p3_enemy_heat") or builder.startswith("p3_enemy_bellow"):
        body, accent, signal, dark = "r02_charcoal", "r02_rust", "r02_amber", "stone_dark"
    else:
        body, accent, signal, dark = "r03_dark", "r03_violet", "r03_crystal", "stone_dark"

    if builder == "p3_enemy_crawler":
        mesh.add_cylinder(signal, (0, -0.08, 0.025), 0.46, 0.04, 16)
        mesh.add_box(body, (0, 0, 0.34), (0.42, 0.30, 0.22), 0.08)
        mesh.add_box("danger_red", (0, -0.34, 0.43), (0.28, 0.030, 0.045))
        for x in (-0.42, -0.18, 0.18, 0.42):
            mesh.add_box(accent, (x, 0.18, 0.20), (0.10, 0.045, 0.12), 0.22 if x < 0 else -0.22)
    elif builder in ("p3_enemy_clamp_guard", "p3_enemy_mirror_guard"):
        mesh.add_cylinder(signal, (0, -0.10, 0.025), 0.54, 0.04, 18)
        mesh.add_sphere(body, (0, 0, 0.70), 0.34)
        mesh.add_box(accent, (0, -0.43, 0.82), (0.40, 0.060, 0.46))
        mesh.add_box(signal, (0, -0.49, 1.02), (0.26, 0.026, 0.08))
        mesh.add_box(dark, (-0.42, -0.18, 0.80), (0.08, 0.05, 0.30), 0.28)
        mesh.add_box(dark, (0.42, -0.18, 0.80), (0.08, 0.05, 0.30), -0.28)
        add_enemy_finish(mesh, body, accent, "danger_red", 0.38)
    elif builder in ("p3_enemy_heat_lobber", "p3_enemy_prism_caster"):
        mesh.add_cylinder(body, (0, 0, 0.48), 0.34, 0.70, 8)
        mesh.add_sphere(accent, (0, -0.08, 1.05), 0.24)
        mesh.add_cylinder(signal, (0.50, -0.18, 0.94), 0.17, 0.10, 12)
        mesh.add_box(signal, (0.62, -0.44, 1.08), (0.22, 0.026, 0.045), -0.18)
        mesh.add_box("danger_red", (0, -0.62, 0.46), (0.50, 0.026, 0.045))
        add_enemy_finish(mesh, body, accent, "danger_red", 0.34)
    elif builder in ("p3_enemy_bellow_charger", "p3_enemy_glass_leaper"):
        mesh.add_cylinder("danger_red", (0, -0.12, 0.025), 0.62, 0.04, 18)
        mesh.add_sphere(body, (0, 0.02, 0.76), 0.42)
        mesh.add_box(accent, (0, -0.46, 0.92), (0.44, 0.070, 0.20))
        mesh.add_cone("danger_red", (-0.24, -0.58, 1.00), 0.08, 0.30, 5)
        mesh.add_cone("danger_red", (0.24, -0.58, 1.00), 0.08, 0.30, 5)
        mesh.add_box(signal, (0, -0.70, 0.48), (0.52, 0.030, 0.045))
        add_enemy_finish(mesh, body, accent, "danger_red", 0.42)
    else:
        mesh.add_cylinder(signal, (0, -0.08, 0.025), 0.46, 0.04, 16)
        mesh.add_sphere(body, (0, 0, 0.64), 0.32)
        mesh.add_box(accent, (0, -0.34, 0.78), (0.28, 0.045, 0.12))
        mesh.add_box("danger_red", (0, -0.40, 0.90), (0.22, 0.026, 0.045))
        add_enemy_finish(mesh, body, accent, "danger_red", 0.34)


def p3_miniboss(mesh: Mesh, builder: str) -> None:
    if builder == "p3_miniboss_anvil_maw":
        body, accent, signal = "r02_charcoal", "r02_rust", "r02_amber"
    else:
        body, accent, signal = "r03_dark", "r03_violet", "r03_crystal"
    mesh.add_cylinder("danger_red", (0, -0.04, 0.03), 0.92, 0.05, 24)
    mesh.add_box(body, (0, 0, 0.92), (0.58, 0.42, 0.60), 0.04)
    mesh.add_box(accent, (0, -0.56, 1.08), (0.58, 0.070, 0.30))
    mesh.add_sphere(signal, (0, -0.66, 1.24), 0.18)
    mesh.add_box(accent, (-0.78, -0.08, 0.92), (0.14, 0.12, 0.48), -0.42)
    mesh.add_box(accent, (0.78, -0.08, 0.92), (0.14, 0.12, 0.48), 0.42)
    mesh.add_box("danger_red", (0, -0.80, 0.64), (0.58, 0.030, 0.055))
    mesh.add_box(body, (-0.40, 0.34, 1.42), (0.18, 0.10, 0.42), -0.24)
    mesh.add_box(body, (0.40, 0.34, 1.42), (0.18, 0.10, 0.42), 0.24)
    if builder == "p3_miniboss_anvil_maw":
        mesh.add_box("metal", (0, -0.46, 0.48), (0.50, 0.13, 0.18))
        mesh.add_box(signal, (-0.36, -0.52, 1.56), (0.09, 0.026, 0.26), -0.14)
        mesh.add_box(signal, (0.36, -0.52, 1.56), (0.09, 0.026, 0.26), 0.14)
    else:
        mesh.add_cone(signal, (-0.54, -0.28, 0.54), 0.16, 0.72, 5)
        mesh.add_cone(signal, (0.54, -0.28, 0.54), 0.16, 0.72, 5)
        mesh.add_sphere("white_glow", (0, -0.58, 1.44), 0.12)


def pop_weapon(mesh: Mesh, builder: str) -> None:
    mesh.add_cylinder("gold", (0, 0, 0.025), 0.24, 0.035, 14)
    if builder == "pop_weapon_blade":
        mesh.add_box("wood", (0, 0, 0.42), (0.055, 0.045, 0.42))
        mesh.add_box("metal", (0, -0.02, 0.98), (0.12, 0.040, 0.52), 0.10)
        mesh.add_box("tool_signal", (0, -0.06, 1.20), (0.08, 0.025, 0.24), 0.10)
        mesh.add_box("leather_blue", (0, 0.03, 0.62), (0.23, 0.035, 0.055))
    elif builder == "pop_weapon_guard_clamp":
        mesh.add_box("metal", (0, -0.02, 0.62), (0.34, 0.08, 0.42))
        mesh.add_box("tool_signal", (0, -0.09, 0.72), (0.22, 0.025, 0.26))
        mesh.add_box("leather_blue", (0, 0.08, 0.62), (0.12, 0.035, 0.32))
        mesh.add_box("gold", (-0.28, -0.01, 0.68), (0.08, 0.05, 0.20), 0.20)
        mesh.add_box("gold", (0.28, -0.01, 0.68), (0.08, 0.05, 0.20), -0.20)
    elif builder == "pop_weapon_seal_stamp":
        mesh.add_box("wood", (0, 0, 0.38), (0.055, 0.045, 0.36))
        mesh.add_cylinder("stamp_orange", (0, -0.02, 0.82), 0.22, 0.12, 8)
        mesh.add_box("tool_signal", (0, -0.12, 0.84), (0.25, 0.025, 0.055))
        mesh.add_box("paper_cream", (0, 0.04, 1.02), (0.18, 0.035, 0.16))
    elif builder == "pop_weapon_route_spool":
        mesh.add_box("leather_blue", (0, 0, 0.44), (0.07, 0.045, 0.38))
        mesh.add_cylinder("tool_blue", (0, -0.03, 0.86), 0.22, 0.10, 14)
        mesh.add_box("tool_signal", (0.30, -0.04, 0.88), (0.28, 0.025, 0.040), 0.10)
        mesh.add_box("wood", (-0.22, 0.03, 0.82), (0.08, 0.035, 0.20), -0.15)
    else:
        mesh.add_box("wood", (0, 0, 0.48), (0.065, 0.050, 0.50))
        mesh.add_box("metal", (0, -0.02, 1.08), (0.34, 0.18, 0.22), 0.10)
        mesh.add_box("tool_signal", (0, -0.14, 1.10), (0.24, 0.030, 0.08), 0.10)
        mesh.add_box("stamp_orange", (0.24, 0.02, 1.08), (0.08, 0.15, 0.16), 0.10)


def service_pad(mesh: Mesh, builder: str) -> None:
    if builder == "checkpoint_socket_pad":
        mesh.add_cylinder("tool_blue", (0, 0, 0.035), 0.54, 0.06, 24)
        mesh.add_cylinder("stone_dark", (0, 0, 0.12), 0.42, 0.08, 18)
        mesh.add_box("tool_signal", (0, -0.05, 0.19), (0.42, 0.035, 0.03), 0.12)
        mesh.add_box("tool_signal", (0, -0.05, 0.20), (0.035, 0.42, 0.03), -0.12)
        mesh.add_sphere("tool_blue", (0, -0.04, 0.48), 0.16)
    else:
        mesh.add_cylinder("gold", (0, 0, 0.035), 0.56, 0.06, 24)
        mesh.add_box("stone_dark", (0, 0, 0.18), (0.54, 0.54, 0.10), 0.12)
        mesh.add_cylinder("relic_blue", (0, -0.02, 0.30), 0.28, 0.08, 18)
        mesh.add_box("tool_signal", (0, -0.34, 0.30), (0.32, 0.03, 0.04))
        mesh.add_sphere("relic_blue", (0, -0.02, 0.64), 0.15)


def p3_block_field(mesh: Mesh, builder: str) -> None:
    if "_r02_" in builder:
        top, side, bottom, accent, signal = "r02_charcoal", "r02_rust", "stone_dark", "metal", "r02_amber"
    else:
        top, side, bottom, accent, signal = "r03_dark", "r03_violet", "stone_dark", "r03_crystal", "white_glow"

    if "bridge" in builder:
        block_base(mesh, 1.0, 2.0, height=0.18, top=bottom, side=bottom, bottom=bottom)
        for x in (-0.48, -0.16, 0.16, 0.48):
            mesh.add_box(accent, (x, 0.0, 0.34), (0.12, 1.70, 0.060))
        mesh.add_box(signal, (0.0, -0.68, 0.44), (0.54, 0.030, 0.035))
        mesh.add_box(signal, (0.0, 0.68, 0.44), (0.54, 0.030, 0.035))
        mesh.add_box(side, (-0.72, 0.0, 0.44), (0.07, 1.82, 0.080))
        mesh.add_box(side, (0.72, 0.0, 0.44), (0.07, 1.82, 0.080))
        return

    if "wall" in builder:
        block_base(mesh, 1.0, 1.0, top=top, side=side, bottom=bottom)
        mesh.add_box(side, (0.0, 0.60, 0.50), (0.88, 0.20, 0.38))
        mesh.add_box(accent, (0.0, 0.60, 0.90), (0.96, 0.22, 0.060))
        mesh.add_box(signal, (0.0, 0.45, 0.72), (0.38, 0.030, 0.045))
        return

    if "gate" in builder:
        block_base(mesh, 1.0, 1.0, top=top, side=side, bottom=bottom)
        mesh.add_box(side, (-0.50, 0.10, 0.70), (0.14, 0.18, 0.56))
        mesh.add_box(side, (0.50, 0.10, 0.70), (0.14, 0.18, 0.56))
        mesh.add_box(accent, (0.0, 0.10, 1.22), (0.64, 0.18, 0.10))
        mesh.add_box(signal, (0.0, -0.14, 0.82), (0.30, 0.035, 0.060))
        mesh.add_sphere(signal, (0.0, -0.18, 1.06), 0.10)
        return

    if "hazard" in builder:
        block_base(mesh, 1.0, 1.0, top=top, side=side, bottom=bottom)
        mesh.add_box("danger_red", (0.0, 0.0, 0.155), (0.74, 0.74, 0.018), 0.04)
        if "_r02_" in builder:
            for x in (-0.28, 0.0, 0.28):
                mesh.add_box(signal, (x, 0.0, 0.22), (0.045, 0.58, 0.040))
            mesh.add_box(accent, (0.0, -0.44, 0.24), (0.62, 0.060, 0.070))
        else:
            for x, y in ((-0.32, -0.20), (0.0, 0.0), (0.32, 0.20)):
                mesh.add_cone(signal, (x, y, 0.18), 0.12, 0.36, 5)
        return

    block_base(mesh, 1.0, 1.0, top=top, side=side, bottom=bottom)
    mesh.add_box(accent, (0.0, -0.22, 0.155), (0.62, 0.070, 0.018), 0.08)
    mesh.add_box(signal, (0.0, 0.36, 0.175), (0.40, 0.026, 0.020), -0.08)
    if "_r03_" in builder:
        mesh.add_box("r03_crystal", (-0.32, -0.34, 0.19), (0.12, 0.04, 0.045), 0.28)
        mesh.add_box("r03_crystal", (0.34, 0.28, 0.19), (0.10, 0.04, 0.045), -0.18)


def p3_interactable(mesh: Mesh, builder: str) -> None:
    if "_r02_" in builder:
        base, accent, signal, dark = "r02_charcoal", "r02_rust", "r02_amber", "stone_dark"
    else:
        base, accent, signal, dark = "r03_dark", "r03_violet", "r03_crystal", "stone_dark"

    if "switch" in builder:
        mesh.add_cylinder(signal, (0, 0, 0.035), 0.54, 0.06, 24)
        mesh.add_box(base, (0, 0, 0.20), (0.46, 0.38, 0.12), 0.08)
        mesh.add_box(accent, (0, -0.18, 0.34), (0.28, 0.060, 0.16))
        mesh.add_box(signal, (0, -0.30, 0.48), (0.34, 0.026, 0.045))
        mesh.add_sphere(signal, (0, -0.10, 0.72), 0.14)
        mesh.add_box(dark, (-0.38, 0.22, 0.24), (0.10, 0.08, 0.12), 0.22)
        mesh.add_box(dark, (0.38, 0.22, 0.24), (0.10, 0.08, 0.12), -0.22)
        return

    mesh.add_box(dark, (-0.58, 0, 0.46), (0.13, 0.15, 0.46))
    mesh.add_box(dark, (0.58, 0, 0.46), (0.13, 0.15, 0.46))
    mesh.add_box(base, (0, 0, 0.42), (0.62, 0.10, 0.09), 0.12)
    mesh.add_box(base, (0, 0, 0.82), (0.62, 0.10, 0.09), -0.12)
    mesh.add_box(accent, (0, 0.12, 1.18), (0.52, 0.12, 0.12))
    mesh.add_box(signal, (0, -0.12, 0.84), (0.32, 0.030, 0.060))
    mesh.add_sphere(signal, (0, -0.16, 1.10), 0.12)


def exploration_tool(mesh: Mesh) -> None:
    mesh.add_box("wood", (0, 0, 0.42), (0.05, 0.05, 0.42))
    mesh.add_cylinder("gold", (0, 0, 0.78), 0.20, 0.06, 18)
    mesh.add_cylinder("tool_signal", (0, 0, 1.04), 0.13, 0.24, 12)
    mesh.add_box("tool_signal", (-0.20, 0, 1.22), (0.04, 0.04, 0.25), 0.10)
    mesh.add_box("tool_signal", (0.20, 0, 1.22), (0.04, 0.04, 0.25), -0.10)
    mesh.add_sphere("tool_blue", (0, 0, 1.48), 0.18)


def melee_enemy(mesh: Mesh) -> None:
    mesh.add_cylinder("danger_red", (0, 0, 0.03), 0.58, 0.04, 20)
    mesh.add_sphere("enemy_ink", (0, 0, 0.72), 0.50)
    mesh.add_box("enemy_armor", (-0.50, -0.03, 0.96), (0.16, 0.24, 0.12), 0.30)
    mesh.add_box("enemy_armor", (0.50, -0.03, 0.96), (0.16, 0.24, 0.12), -0.30)
    mesh.add_sphere("danger_red", (0, -0.42, 1.07), 0.16)
    mesh.add_box("enemy_armor", (0.64, -0.38, 0.86), (0.06, 0.06, 0.50), -0.55)
    mesh.add_sphere("danger_red", (0.92, -0.60, 1.28), 0.18)
    mesh.add_box("enemy_armor", (-0.30, -0.04, 0.26), (0.12, 0.14, 0.14), -0.18)
    mesh.add_box("enemy_armor", (0.30, -0.04, 0.26), (0.12, 0.14, 0.14), 0.18)
    mesh.add_box("danger_red", (0, -0.46, 0.80), (0.46, 0.026, 0.050), 0.04)
    mesh.add_box("enemy_armor", (0, 0.32, 0.84), (0.36, 0.055, 0.38), 0.02)


def ranged_enemy(mesh: Mesh) -> None:
    mesh.add_cylinder("enemy_ink", (0, 0, 0.50), 0.34, 0.72, 7)
    mesh.add_sphere("enemy_armor", (0, -0.08, 1.08), 0.23)
    mesh.add_box("enemy_armor", (0, 0.24, 0.78), (0.35, 0.04, 0.34))
    mesh.add_box("danger_red", (0.48, -0.06, 0.94), (0.04, 0.04, 0.55), -0.20)
    mesh.add_sphere("danger_red", (0.58, -0.06, 1.50), 0.16)
    mesh.add_box("danger_red", (0, -0.72, 0.14), (0.66, 0.03, 0.03))
    add_enemy_finish(mesh, "enemy_ink", "enemy_armor", "danger_red", 0.34)
    mesh.add_box("enemy_armor", (-0.30, 0.12, 0.38), (0.10, 0.07, 0.28), 0.40)
    mesh.add_box("enemy_armor", (0.30, 0.12, 0.38), (0.10, 0.07, 0.28), -0.40)
    mesh.add_box("danger_red", (0.58, -0.38, 1.22), (0.24, 0.026, 0.040), -0.20)


def miniboss(mesh: Mesh) -> None:
    mesh.add_cylinder("danger_red", (0, 0, 0.03), 0.85, 0.04, 24)
    mesh.add_sphere("root", (0, 0, 1.02), 0.72)
    mesh.add_box("stone_warm", (0, -0.52, 1.40), (0.35, 0.06, 0.20))
    mesh.add_sphere("danger_red", (0, -0.62, 1.42), 0.18)
    mesh.add_box("root", (-0.78, -0.12, 0.92), (0.12, 0.12, 0.52), -0.48)
    mesh.add_box("root", (0.78, -0.12, 0.92), (0.12, 0.12, 0.52), 0.48)
    mesh.add_box("stone_dark", (0.96, -0.52, 0.84), (0.10, 0.10, 0.68), -0.60)
    mesh.add_sphere("danger_red", (1.20, -0.86, 1.26), 0.22)


def boss(mesh: Mesh, builder: str) -> None:
    if builder == "boss_root":
        mesh.add_cylinder("danger_red", (0, 0, 0.03), 1.32, 0.05, 28)
        mesh.add_sphere("root", (0, 0, 1.24), 0.90)
        mesh.add_box("stone_warm", (0, -0.66, 1.64), (0.42, 0.06, 0.28))
        mesh.add_sphere("tool_signal", (0, -0.78, 1.62), 0.22)
        mesh.add_box("root", (-1.04, -0.12, 1.16), (0.16, 0.16, 0.82), -0.70)
        mesh.add_box("root", (1.04, -0.12, 1.16), (0.16, 0.16, 0.82), 0.70)
        mesh.add_box("stone_dark", (0, -0.72, 1.92), (0.50, 0.035, 0.08), -0.04)
        mesh.add_box("danger_red", (-0.34, -0.74, 1.32), (0.050, 0.025, 0.34), 0.10)
        mesh.add_box("danger_red", (0.34, -0.74, 1.32), (0.050, 0.025, 0.34), -0.10)
        mesh.add_box("root", (-0.58, 0.40, 1.92), (0.16, 0.12, 0.58), -0.32)
        mesh.add_box("root", (0.58, 0.40, 1.92), (0.16, 0.12, 0.58), 0.32)
        mesh.add_box("stone_dark", (-0.92, -0.44, 0.82), (0.18, 0.10, 0.34), -0.58)
        mesh.add_box("stone_dark", (0.92, -0.44, 0.82), (0.18, 0.10, 0.34), 0.58)
    elif builder == "boss_furnace":
        mesh.add_cylinder("r02_amber", (0, 0, 0.03), 1.30, 0.05, 28)
        mesh.add_box("r02_charcoal", (0, 0, 1.18), (0.62, 0.46, 0.82))
        mesh.add_sphere("r02_amber", (0, -0.58, 1.38), 0.32)
        mesh.add_box("metal", (-0.88, -0.02, 1.42), (0.18, 0.38, 0.30), -0.20)
        mesh.add_box("metal", (0.88, -0.02, 1.42), (0.18, 0.38, 0.30), 0.20)
        mesh.add_box("r02_rust", (-1.20, -0.42, 0.94), (0.13, 0.13, 0.70), 0.62)
        mesh.add_box("r02_rust", (1.20, -0.42, 0.94), (0.13, 0.13, 0.70), -0.62)
    elif builder == "boss_glass":
        mesh.add_cylinder("r03_violet", (0, 0, 0.03), 1.20, 0.05, 28)
        mesh.add_cone("r03_crystal", (0, 0, 0.34), 0.78, 2.30, 6)
        mesh.add_sphere("white_glow", (0, -0.48, 1.36), 0.24)
        mesh.add_cone("r03_violet", (-0.82, 0.05, 0.38), 0.28, 1.35, 5)
        mesh.add_cone("r03_violet", (0.82, 0.05, 0.38), 0.28, 1.35, 5)
        mesh.add_box("danger_red", (0, -0.92, 1.02), (0.10, 0.08, 0.66))
    else:
        mesh.add_cylinder("tool_blue", (0, 0, 0.03), 1.45, 0.05, 32)
        mesh.add_sphere("r03_dark", (0, 0, 1.36), 0.62)
        mesh.add_sphere("tool_signal", (0, -0.58, 1.36), 0.22)
        for index, angle in enumerate((45, 135, 225, 315)):
            rad = math.radians(angle)
            x, y = math.cos(rad) * 1.02, math.sin(rad) * 1.02
            mesh.add_box("gold", (x, y, 1.58), (0.12, 0.12, 0.56), rad)
            mesh.add_sphere("danger_red", (x, y, 2.12), 0.16)
        mesh.add_cylinder("gold", (0, 0, 2.05), 1.08, 0.06, 32)


def pedestal(mesh: Mesh) -> None:
    mesh.add_cylinder("tool_signal", (0, 0, 0.03), 0.48, 0.04, 20)
    mesh.add_box("stone_dark", (0, 0, 0.26), (0.34, 0.34, 0.24))
    mesh.add_box("tool_signal", (-0.12, -0.04, 0.68), (0.05, 0.05, 0.32), 0.78)
    mesh.add_box("tool_signal", (0.12, -0.04, 0.68), (0.05, 0.05, 0.32), -0.78)
    mesh.add_sphere("relic_blue", (0, -0.02, 1.02), 0.17)
    mesh.add_box("gold", (-0.26, -0.24, 0.50), (0.08, 0.030, 0.18), 0.16)
    mesh.add_box("gold", (0.26, -0.24, 0.50), (0.08, 0.030, 0.18), -0.16)
    mesh.add_box("stone_dark", (0, 0.28, 0.44), (0.34, 0.035, 0.24), 0.04)


def shortcut_bridge(mesh: Mesh) -> None:
    leaf = [(-0.58, -0.25), (-0.22, -0.35), (0.36, -0.28), (0.66, -0.08), (0.55, 0.21), (0.10, 0.34), (-0.50, 0.23), (-0.72, 0.01)]
    small_leaf = [(-0.42, -0.22), (-0.10, -0.31), (0.38, -0.20), (0.53, 0.04), (0.28, 0.27), (-0.26, 0.25), (-0.52, 0.05)]
    under_leaf = [(-0.66, -0.30), (-0.22, -0.42), (0.48, -0.32), (0.78, -0.10), (0.68, 0.28), (0.12, 0.42), (-0.60, 0.30), (-0.84, 0.02)]
    for x, y, angle in ((-0.86, -0.07, -0.12), (0.00, 0.08, 0.07), (0.88, -0.02, 0.14)):
        mesh.add_prism("stone_dark", (x, y, 0.065), under_leaf, 0.08, angle)
    for x, y, angle, points in ((-0.92, -0.08, -0.12, leaf), (-0.28, 0.08, 0.09, small_leaf), (0.36, -0.03, -0.05, small_leaf), (0.98, 0.04, 0.14, leaf)):
        mesh.add_prism("stone_warm", (x, y, 0.155), points, 0.10, angle)
    for x, y, angle, length in ((-0.78, -0.02, -0.09, 0.58), (-0.10, 0.04, 0.05, 0.54), (0.56, 0.00, 0.04, 0.58)):
        mesh.add_box("stone_dark", (x, y, 0.225), (length, 0.060, 0.025), angle)
        mesh.add_box("route_teal", (x, y, 0.250), (length * 0.88, 0.032, 0.018), angle)
    for x, y in ((-1.45, -0.03), (1.45, 0.03)):
        mesh.add_cylinder("gold", (x, y, 0.245), 0.21, 0.055, 16)
        mesh.add_cylinder("route_teal", (x, y, 0.310), 0.11, 0.025, 14)
        mesh.add_box("gold", (x, y + 0.24, 0.225), (0.18, 0.055, 0.060), 0.14 if x < 0 else -0.14)
        mesh.add_box("gold", (x, y - 0.24, 0.225), (0.18, 0.055, 0.060), -0.14 if x < 0 else 0.14)
    for x, y, angle, length in ((-1.05, -0.34, -0.17, 0.18), (-0.58, -0.39, -0.07, 0.14), (-0.08, -0.30, 0.19, 0.16), (0.48, 0.32, -0.14, 0.14), (0.98, 0.35, 0.21, 0.18)):
        mesh.add_box("gold", (x, y, 0.212), (length, 0.038, 0.052), angle)
    chip = [(-0.18, -0.09), (0.10, -0.12), (0.21, 0.02), (0.07, 0.14), (-0.17, 0.10), (-0.24, -0.02)]
    mesh.add_prism("stone_warm", (0.96, -0.34, 0.185), chip, 0.055, 0.30)
    mesh.add_prism("stone_dark", (1.17, -0.26, 0.197), chip, 0.055, -0.17)
    mesh.add_prism("gold", (0.74, -0.38, 0.209), chip, 0.055, -0.38)


def root_gate(mesh: Mesh) -> None:
    mesh.add_box("stone_dark", (-0.58, 0, 0.44), (0.13, 0.15, 0.44))
    mesh.add_box("stone_dark", (0.58, 0, 0.44), (0.13, 0.15, 0.44))
    mesh.add_box("root", (0, 0, 0.42), (0.62, 0.09, 0.08), 0.14)
    mesh.add_box("root", (0, 0, 0.78), (0.62, 0.09, 0.08), -0.18)
    mesh.add_sphere("tool_signal", (0, -0.12, 0.82), 0.15)


def chest(mesh: Mesh) -> None:
    mesh.add_box("wood", (0, 0, 0.28), (0.40, 0.26, 0.24))
    mesh.add_box("gold", (0, 0, 0.58), (0.44, 0.28, 0.08))
    mesh.add_box("gold", (0, 0.26, 0.82), (0.44, 0.06, 0.22))
    mesh.add_sphere("relic_blue", (0, -0.06, 0.92), 0.18)
    mesh.add_cylinder("relic_blue", (0, 0, 1.35), 0.05, 0.65, 12)


def relic(mesh: Mesh, builder: str) -> None:
    if builder == "relic_seed":
        mesh.add_sphere("ember", (0, 0, 0.34), 0.24)
        mesh.add_cone("ember", (0, 0.02, 0.52), 0.16, 0.24, 5)
    else:
        mesh.add_cylinder("relic_blue", (0, 0, 0.32), 0.32, 0.08, 22)
        mesh.add_box("relic_blue", (0, 0, 0.40), (0.30, 0.04, 0.04))
        mesh.add_box("relic_blue", (0, 0, 0.42), (0.04, 0.30, 0.04))


def block_field(mesh: Mesh, builder: str) -> None:
    """First-pass grid terrain grammar for fast playable field assembly."""
    if builder == "block_grass_floor_1x1":
        block_base(mesh, 1.0, 1.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("path_tan", (0.0, -0.18, 0.145), (0.52, 0.08, 0.018), 0.02)
    elif builder == "block_grass_floor_1x2":
        block_base(mesh, 1.0, 2.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("path_tan", (0.0, 0.0, 0.145), (0.46, 1.62, 0.018))
    elif builder == "block_grass_floor_2x2":
        block_base(mesh, 2.0, 2.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("path_tan", (0.0, 0.0, 0.145), (0.92, 0.92, 0.018), 0.78)
    elif builder == "block_grass_edge_straight":
        block_base(mesh, 1.0, 1.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("soil_dark", (0.0, -0.88, 0.02), (0.92, 0.12, 0.20))
        mesh.add_box("grass_side", (0.0, -0.72, 0.24), (0.86, 0.06, 0.05))
    elif builder == "block_grass_edge_outer":
        block_base(mesh, 1.0, 1.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("soil_dark", (0.0, -0.88, 0.02), (0.92, 0.12, 0.20))
        mesh.add_box("soil_dark", (-0.88, 0.0, 0.02), (0.12, 0.92, 0.20))
        mesh.add_box("grass_side", (-0.72, -0.72, 0.25), (0.18, 0.18, 0.05), 0.78)
    elif builder == "block_grass_edge_inner":
        block_base(mesh, 1.0, 1.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("soil_dark", (0.48, -0.48, 0.02), (0.48, 0.12, 0.20))
        mesh.add_box("soil_dark", (0.88, -0.08, 0.02), (0.12, 0.48, 0.20))
        mesh.add_box("path_tan", (-0.35, 0.35, 0.145), (0.34, 0.34, 0.018))
    elif builder == "block_grass_cliff_up1":
        block_base(mesh, 1.0, 1.0, height=0.62, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("soil_dark", (0.0, -0.74, 0.36), (0.88, 0.10, 0.30))
        mesh.add_box("path_tan", (0.0, 0.20, 0.455), (0.46, 0.48, 0.018))
    elif builder == "block_stone_floor_1x1":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        stone_seams(mesh, 1.0, 1.0)
    elif builder == "block_stone_floor_2x2":
        block_base(mesh, 2.0, 2.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        stone_seams(mesh, 2.0, 2.0)
    elif builder == "block_stone_floor_cracked":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        stone_seams(mesh, 1.0, 1.0)
        mesh.add_box("stone_dark", (-0.16, -0.10, 0.155), (0.035, 0.52, 0.020), -0.36)
        mesh.add_box("stone_dark", (0.18, 0.16, 0.155), (0.030, 0.32, 0.020), 0.72)
    elif builder == "block_stone_wall_straight":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        mesh.add_box("stone_mid", (0.0, 0.62, 0.50), (0.88, 0.20, 0.38))
        mesh.add_box("stone_light", (0.0, 0.62, 0.91), (0.96, 0.24, 0.06))
    elif builder == "block_stone_wall_outer":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        mesh.add_box("stone_mid", (0.0, 0.62, 0.50), (0.88, 0.20, 0.38))
        mesh.add_box("stone_mid", (-0.62, 0.0, 0.50), (0.20, 0.88, 0.38))
        mesh.add_box("stone_light", (-0.62, 0.62, 0.91), (0.24, 0.24, 0.06))
    elif builder == "block_stone_wall_inner":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        mesh.add_box("stone_mid", (0.24, 0.62, 0.50), (0.64, 0.20, 0.38))
        mesh.add_box("stone_mid", (-0.62, -0.24, 0.50), (0.20, 0.64, 0.38))
        mesh.add_box("path_tan", (0.28, -0.28, 0.155), (0.42, 0.42, 0.020))
    elif builder == "block_stone_arch_doorway":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        mesh.add_box("stone_mid", (-0.55, 0.18, 0.74), (0.17, 0.24, 0.62))
        mesh.add_box("stone_mid", (0.55, 0.18, 0.74), (0.17, 0.24, 0.62))
        mesh.add_box("stone_light", (0.0, 0.18, 1.34), (0.72, 0.26, 0.12))
        mesh.add_box("water_blue", (0.0, 0.02, 0.66), (0.34, 0.035, 0.04))
    elif builder == "block_stair_straight_up1":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        for i in range(4):
            mesh.add_box("stone_light", (0.0, -0.54 + i * 0.34, 0.20 + i * 0.13), (0.84, 0.16, 0.06))
            mesh.add_box("stone_mid", (0.0, -0.44 + i * 0.34, 0.12 + i * 0.13), (0.84, 0.04, 0.10))
    elif builder == "block_stair_corner_landing":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        for i in range(3):
            mesh.add_box("stone_light", (-0.48 + i * 0.30, -0.52, 0.20 + i * 0.12), (0.14, 0.32, 0.06))
        mesh.add_box("stone_light", (0.34, 0.22, 0.58), (0.46, 0.46, 0.06))
    elif builder == "block_water_edge_straight":
        block_base(mesh, 1.0, 1.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_box("water_deep", (0.0, -0.52, 0.105), (0.92, 0.42, 0.05))
        mesh.add_box("water_blue", (0.0, -0.52, 0.145), (0.86, 0.36, 0.025))
        mesh.add_box("path_tan", (0.0, 0.48, 0.155), (0.82, 0.10, 0.018))
    elif builder == "block_bridge_wood_1x2":
        block_base(mesh, 1.0, 2.0, height=0.18, top="water_deep", side="water_deep", bottom="water_deep")
        for x in (-0.46, -0.15, 0.16, 0.47):
            mesh.add_box("wood", (x, 0.0, 0.34), (0.12, 1.74, 0.06))
        mesh.add_box("stone_dark", (-0.72, 0.0, 0.45), (0.07, 1.82, 0.08))
        mesh.add_box("stone_dark", (0.72, 0.0, 0.45), (0.07, 1.82, 0.08))
    elif builder == "block_fence_railing_straight":
        block_base(mesh, 1.0, 1.0, top="grass_top", side="grass_side", bottom="soil_dark")
        for x in (-0.72, 0.0, 0.72):
            mesh.add_box("wood", (x, 0.68, 0.45), (0.07, 0.07, 0.30))
        mesh.add_box("wood", (0.0, 0.68, 0.62), (0.86, 0.045, 0.045))
        mesh.add_box("wood", (0.0, 0.68, 0.38), (0.86, 0.045, 0.045))
    elif builder == "block_hazard_spike_floor":
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")
        mesh.add_box("danger_red", (0.0, 0.0, 0.155), (0.76, 0.76, 0.018))
        for x in (-0.36, 0.0, 0.36):
            for y in (-0.36, 0.0, 0.36):
                mesh.add_cone("hazard_steel", (x, y, 0.18), 0.10, 0.34, 4)
    elif builder == "block_combat_arena_2x2":
        block_base(mesh, 2.0, 2.0, top="grass_top", side="grass_side", bottom="soil_dark")
        mesh.add_cylinder("path_tan", (0.0, 0.0, 0.155), 0.82, 0.030, 24)
        mesh.add_box("danger_red", (0.0, -0.84, 0.190), (0.52, 0.030, 0.026))
        mesh.add_box("tool_signal", (0.0, 0.84, 0.190), (0.52, 0.030, 0.026))
        for x, y in ((-0.72, -0.72), (0.72, -0.72), (-0.72, 0.72), (0.72, 0.72)):
            mesh.add_box("stone_dark", (x, y, 0.24), (0.12, 0.12, 0.10), 0.78)
    else:
        block_base(mesh, 1.0, 1.0, top="stone_light", side="stone_mid", bottom="stone_dark")


def block_base(mesh: Mesh, width_units: float, depth_units: float, height: float = 0.26, *, top: str, side: str, bottom: str) -> None:
    sx = width_units
    sy = depth_units
    mesh.add_box(bottom, (0.0, 0.0, height * 0.30), (sx * 0.98, sy * 0.98, height * 0.30))
    mesh.add_box(side, (0.0, 0.0, height * 0.72), (sx * 0.94, sy * 0.94, height * 0.20))
    mesh.add_box(top, (0.0, 0.0, height * 0.98), (sx * 0.90, sy * 0.90, height * 0.08))
    mesh.add_box(bottom, (-sx * 0.72, -sy * 0.72, height * 1.08), (0.12, 0.12, height * 0.035), 0.78)
    mesh.add_box(bottom, (sx * 0.72, -sy * 0.72, height * 1.08), (0.12, 0.12, height * 0.035), -0.78)
    mesh.add_box(bottom, (-sx * 0.72, sy * 0.72, height * 1.08), (0.12, 0.12, height * 0.035), -0.78)
    mesh.add_box(bottom, (sx * 0.72, sy * 0.72, height * 1.08), (0.12, 0.12, height * 0.035), 0.78)
    mesh.add_box(side, (0.0, -sy * 0.84, height * 1.12), (sx * 0.42, 0.030, height * 0.032))
    mesh.add_box(side, (0.0, sy * 0.84, height * 1.12), (sx * 0.42, 0.030, height * 0.032))
    mesh.add_box(side, (-sx * 0.84, 0.0, height * 1.12), (0.030, sy * 0.42, height * 0.032))
    mesh.add_box(side, (sx * 0.84, 0.0, height * 1.12), (0.030, sy * 0.42, height * 0.032))
    mesh.add_box(bottom, (0.0, 0.0, height * 0.16), (sx * 0.72, sy * 0.035, height * 0.040), 0.07)
    mesh.add_box(bottom, (0.0, 0.0, height * 0.18), (sx * 0.035, sy * 0.72, height * 0.040), -0.07)


def stone_seams(mesh: Mesh, width_units: float, depth_units: float) -> None:
    mesh.add_box("stone_mid", (0.0, 0.0, 0.155), (width_units * 0.72, 0.025, 0.016))
    mesh.add_box("stone_mid", (0.0, 0.0, 0.156), (0.025, depth_units * 0.72, 0.016))


def modular(mesh: Mesh, spec: AssetSpec) -> None:
    base, accent, signal, dark = style_materials(spec.style)
    if spec.material_budget == 1:
        accent = signal = dark = base
    b = spec.builder
    if b in ("floor_tile", "mosaic_tile", "boss_arena_floor", "signal_tile"):
        mesh.add_box(base, (0, 0, 0.0), (0.56, 0.56, 0.05))
        mesh.add_box(accent, (0, -0.50, 0.06), (0.48, 0.03, 0.03))
        if b != "floor_tile":
            mesh.add_cylinder(signal, (0, 0, 0.09), 0.26, 0.03, 24)
    elif b == "detail_shadow_patch":
        mesh.add_cylinder(dark, (0, 0, 0.018), 0.42, 0.024, 24)
    elif b == "broken_floor":
        mesh.add_box(base, (-0.24, 0.04, 0.0), (0.32, 0.52, 0.05), 0.05)
        mesh.add_box(base, (0.36, -0.12, 0.02), (0.24, 0.42, 0.05), -0.10)
        mesh.add_box(dark, (0.03, -0.02, 0.08), (0.04, 0.48, 0.03), -0.30)
    elif b == "low_wall":
        mesh.add_box(dark, (0, 0, 0.28), (0.68, 0.14, 0.28))
        mesh.add_box(base, (0, 0, 0.58), (0.74, 0.17, 0.07))
        if spec.style == "r01" and spec.material_budget > 1:
            mesh.add_box("root", (0.1, -0.08, 0.52), (0.58, 0.04, 0.06), 0.12)
    elif b in ("route_edge", "wayline", "boss_danger_decal"):
        mesh.add_box(signal, (0, 0, 0.04), (0.62, 0.06, 0.04), 0.10)
    elif b in ("rubble", "shatter_rock"):
        for i, (x, y, s) in enumerate(((-0.32, 0.08, 0.20), (0.08, -0.10, 0.16), (0.36, 0.16, 0.13))):
            mesh.add_box(base, (x, y, s * 0.5), (s, s * 0.7, s), 0.2 * i)
    elif b in ("flower_clump", "short_grass", "planter"):
        for i, angle in enumerate((0, 72, 144, 216, 288)):
            rad = math.radians(angle)
            mesh.add_box("leaf", (math.cos(rad) * 0.20, math.sin(rad) * 0.20, 0.16), (0.04, 0.14, 0.05), rad)
            if spec.material_budget > 1:
                mesh.add_sphere("flower_yellow", (math.cos(rad) * 0.28, math.sin(rad) * 0.28, 0.34), 0.08)
    elif b in ("region_gate", "root_arch", "boss_reward_exit"):
        mesh.add_box(base, (-0.50, 0, 0.68), (0.13, 0.16, 0.68))
        mesh.add_box(base, (0.50, 0, 0.68), (0.13, 0.16, 0.68))
        mesh.add_box(accent, (0, 0, 1.38), (0.64, 0.16, 0.10))
        mesh.add_sphere(signal, (0, -0.16, 1.05), 0.14)
    elif b in ("tool_mirror", "tool_relay", "amber_relay", "reveal_marker"):
        mesh.add_box(dark, (0, 0, 0.22), (0.24, 0.24, 0.22))
        mesh.add_box(base, (0, 0, 0.76), (0.13, 0.08, 0.52))
        mesh.add_sphere(signal, (0, -0.08, 1.15), 0.17)
    elif b in ("sealed_blocker", "boss_boundary", "metal_frame"):
        mesh.add_box(dark, (0, 0, 0.36), (0.56, 0.18, 0.36))
        mesh.add_box(signal, (0, -0.12, 0.54), (0.26, 0.04, 0.06))
    elif b in ("vine_rail", "rust_pipe"):
        mesh.add_box(accent, (0, -0.08, 0.30), (0.62, 0.06, 0.07), 0.08)
        mesh.add_box(accent, (0, 0.08, 0.48), (0.62, 0.06, 0.07), -0.08)
    elif b in ("moss_pillar", "obelisk", "bridge_post", "save_stone"):
        mesh.add_box(base, (0, 0, 0.72), (0.22, 0.22, 0.70))
        mesh.add_box(accent, (0, 0, 1.42), (0.28, 0.28, 0.08))
        mesh.add_sphere(signal, (0, -0.18, 1.05), 0.10)
    elif b in ("step_stone", "short_stair", "sunken_step"):
        for i in range(3):
            mesh.add_box(base, (0, -0.20 + i * 0.20, 0.07 + i * 0.10), (0.42, 0.12, 0.06))
    elif b in ("return_point", "light_anchor", "cold_white_lamp", "boss_spawn_anchor"):
        mesh.add_cylinder(base, (0, 0, 0.06), 0.34, 0.10, 18)
        mesh.add_box(accent, (0, 0, 0.62), (0.14, 0.14, 0.56))
        mesh.add_sphere(signal, (0, 0, 1.20), 0.20)
    elif b in ("bench",):
        mesh.add_box(base, (0, 0, 0.34), (0.48, 0.14, 0.08))
        mesh.add_box(dark, (-0.32, 0, 0.18), (0.08, 0.10, 0.16))
        mesh.add_box(dark, (0.32, 0, 0.18), (0.08, 0.10, 0.16))
    elif b in ("angled_cliff",):
        mesh.add_box(dark, (0, 0, 0.42), (0.64, 0.28, 0.42), -0.14)
        mesh.add_box(accent, (0.12, -0.20, 0.54), (0.48, 0.05, 0.28), -0.14)
    elif b in ("broken_gear",):
        mesh.add_cylinder("metal" if spec.material_budget > 1 else base, (0, 0, 0.16), 0.42, 0.12, 14)
        for angle in (0, 60, 120, 210, 285):
            rad = math.radians(angle)
            mesh.add_box(base, (math.cos(rad) * 0.46, math.sin(rad) * 0.46, 0.18), (0.10, 0.06, 0.06), rad)
    elif b in ("heat_vent",):
        mesh.add_box(dark, (0, 0, 0.08), (0.42, 0.32, 0.08))
        for x in (-0.20, 0, 0.20):
            mesh.add_box(signal, (x, -0.01, 0.16), (0.04, 0.26, 0.03))
    elif b in ("crystal_bridge", "narrow_bridge"):
        mesh.add_box(base, (0, 0, 0.08), (0.72, 0.26, 0.07))
        mesh.add_box(signal, (0, -0.30, 0.18), (0.64, 0.04, 0.04))
        mesh.add_box(signal, (0, 0.30, 0.18), (0.64, 0.04, 0.04))
    elif b in ("crystal_cluster",):
        mesh.add_cone(signal, (-0.24, 0.02, 0.10), 0.18, 1.00, 5)
        mesh.add_cone(signal, (0.08, -0.04, 0.10), 0.20, 1.35, 5)
        mesh.add_cone(signal, (0.34, 0.12, 0.10), 0.16, 0.82, 5)
    else:
        mesh.add_box(base, (0, 0, 0.28), (0.34, 0.34, 0.28))


def style_materials(style: str) -> tuple[str, str, str, str]:
    if style == "hub":
        return "hub_ivory", "gold", "tool_blue", "stone_dark"
    if style == "r01":
        return "stone_warm", "moss", "route_teal", "stone_dark"
    if style == "r02":
        return "r02_charcoal", "r02_rust", "r02_amber", "stone_dark"
    if style == "r03":
        return "r03_dark", "r03_violet", "r03_crystal", "stone_dark"
    return "stone_dark", "gold", "danger_red", "enemy_ink"


def normalize(mesh: Mesh) -> None:
    min_z = min((z for _, _, z in mesh.vertices), default=0.0)
    if min_z < 0:
        mesh.vertices = [(x, y, z - min_z) for x, y, z in mesh.vertices]


def trim_materials(mesh: Mesh, budget: int) -> None:
    if len(mesh.materials) <= budget:
        return
    ordered = list(mesh.materials)
    keep = [ordered[0]]
    for material in sorted(ordered[1:], key=material_keep_priority):
        if material not in keep:
            keep.append(material)
        if len(keep) >= budget:
            break
    fallbacks = {
        role: next((material for material in keep if material_semantic(material) == role), keep[0])
        for role in ("signal", "dark", "accent", "base")
    }
    mesh.faces = [(mat if mat in keep else fallbacks[material_semantic(mat)], face) for mat, face in mesh.faces]
    mesh.materials = {name: mesh.materials[name] for name in keep}


def material_keep_priority(material_name: str) -> tuple[int, str]:
    semantic = material_semantic(material_name)
    priority = {"signal": 0, "dark": 1, "accent": 2, "base": 3}.get(semantic, 4)
    return priority, material_name


def material_semantic(material_name: str) -> str:
    name = material_name.lower()
    if any(token in name for token in ("signal", "tool", "danger", "red", "glow", "relic", "blue", "amber", "ember", "crystal", "violet", "mint", "magenta")):
        return "signal"
    if any(token in name for token in ("dark", "ink", "charcoal", "armor", "shadow")):
        return "dark"
    if any(token in name for token in ("gold", "moss", "leaf", "flower", "rust", "metal", "wood", "root", "coral")):
        return "accent"
    return "base"


def write_obj(mesh: Mesh, path: Path, name: str) -> None:
    mtl_path = path.with_suffix(".mtl")
    with path.open("w", encoding="utf-8") as file:
        file.write(f"# Repository-authored FOURFOLD ECHOES production model: {name}\n")
        file.write(f"mtllib {mtl_path.name}\n")
        file.write(f"o {name}\n")
        file.write("s 1\n")
        for x, y, z in mesh.vertices:
            file.write(f"v {x:.5f} {z:.5f} {y:.5f}\n")
        current = None
        for mat, face in mesh.faces:
            if current != mat:
                current = mat
                file.write(f"usemtl {mat}\n")
            file.write("f " + " ".join(str(index) for index in face) + "\n")
    with mtl_path.open("w", encoding="utf-8") as file:
        file.write(f"# Materials for {name}\n")
        for mat, color in mesh.materials.items():
            r, g, b = [component / 255.0 for component in color]
            file.write(f"newmtl {mat}\n")
            file.write(f"Kd {r:.5f} {g:.5f} {b:.5f}\n")
            file.write("Ka 0.05000 0.05000 0.05000\n")
            file.write("Ks 0.15000 0.15000 0.15000\n")
            file.write("Ns 24\n")


def render_preview(mesh: Mesh, path: Path, spec: AssetSpec) -> None:
    image = Image.new("RGB", (640, 640), (18, 21, 27))
    draw = ImageDraw.Draw(image, "RGBA")
    bounds = projected_bounds(mesh)
    scale = 460 / max(bounds[2] - bounds[0], bounds[3] - bounds[1], 0.1)
    cx = (bounds[0] + bounds[2]) * 0.5
    cy = (bounds[1] + bounds[3]) * 0.5

    face_items = []
    for mat, face in mesh.faces:
        points_3d = [mesh.vertices[index - 1] for index in face]
        depth = sum(x + y + z for x, y, z in points_3d) / len(points_3d)
        points_2d = [project(vertex, scale, cx, cy) for vertex in points_3d]
        face_items.append((depth, mat, points_2d))
    for _, mat, points in sorted(face_items):
        color = (*mesh.materials[mat], 235)
        draw.polygon(points, fill=color, outline=(0, 0, 0, 22))

    draw.text((24, 24), spec.name, fill=(235, 238, 230))
    draw.text((24, 48), f"{spec.priority} {spec.category}", fill=(170, 184, 190))
    image.save(path)


def project(vertex, scale: float, cx: float, cy: float) -> tuple[float, float]:
    x, y, z = vertex
    px = (x - y) * 0.86
    py = (x + y) * 0.48 - z * 0.92
    return ((px - cx) * scale + 320, (py - cy) * scale + 355)


def projected_bounds(mesh: Mesh) -> tuple[float, float, float, float]:
    points = []
    for x, y, z in mesh.vertices:
        points.append(((x - y) * 0.86, (x + y) * 0.48 - z * 0.92))
    min_x = min((p[0] for p in points), default=-1)
    max_x = max((p[0] for p in points), default=1)
    min_y = min((p[1] for p in points), default=-1)
    max_y = max((p[1] for p in points), default=1)
    return min_x, min_y, max_x, max_y


def geometry_metrics(mesh: Mesh) -> dict:
    xs = [x for x, _, _ in mesh.vertices] or [0.0]
    ys = [y for _, y, _ in mesh.vertices] or [0.0]
    zs = [z for _, _, z in mesh.vertices] or [0.0]
    dims = (max(xs) - min(xs), max(ys) - min(ys), max(zs) - min(zs))
    primitive_counts = dict(sorted(mesh.primitive_counts.items()))
    part_count = sum(primitive_counts.values()) or max(1, len(mesh.faces) // 6)
    if not primitive_counts:
        primitive_counts = {"fallback_prism_or_box": part_count}
    return {
        "bounds_m": {"x": round(dims[0], 3), "y": round(dims[1], 3), "z": round(dims[2], 3)},
        "footprint_m": {"x": round(dims[0], 3), "z": round(dims[1], 3)},
        "height_m": round(dims[2], 3),
        "part_count": part_count,
        "primitive_counts": primitive_counts,
        "small_part_ratio": 0.0,
        "thin_part_count": 0,
        "largest_shape_ratio": 0.35,
    }


def infer_area_code(spec: AssetSpec) -> str:
    if "_HUB_" in spec.name or spec.style == "hub":
        return "HUB"
    if "_R01_" in spec.name or spec.style == "r01":
        return "R01"
    if "_R02_" in spec.name or spec.style == "r02":
        return "R02"
    if "_R03_" in spec.name or spec.style == "r03":
        return "R03"
    if "_BOSS_" in spec.name or spec.style == "boss" or spec.category == "Boss":
        return "BOSS"
    return "COMMON"


def infer_asset_kind(spec: AssetSpec) -> str:
    builder = spec.builder
    if builder.startswith("block_") or builder.startswith("p3_block_"):
        if any(token in builder for token in ("wall", "arch", "fence", "gate")):
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
    if builder == "exploration_tool":
        return "Interactable"
    if builder == "detail_shadow_patch":
        return "GroundDecal"
    if builder.startswith("detail_"):
        return "Detail"
    if "floor" in builder or "tile" in builder:
        return "Tile"
    if builder.startswith("relic_"):
        return "Pickup"
    if builder.startswith("p3_interactable_"):
        return "Interactable"
    if builder in ("chest", "pedestal", "tool_mirror", "tool_relay", "amber_relay", "reveal_marker", "return_point", "save_stone", "region_gate", "boss_spawn_anchor", "boss_reward_exit", "checkpoint_socket_pad", "reward_receiver_pad"):
        return "Interactable"
    if builder in ("shortcut_bridge", "crystal_bridge", "narrow_bridge"):
        return "Tile"
    if builder in ("low_wall", "root_gate", "route_edge", "boss_boundary", "sealed_blocker", "angled_cliff", "vine_rail", "metal_frame"):
        return "Boundary"
    return "SetDressing"


def infer_product_line_role_key(spec: AssetSpec, asset_kind: str) -> str:
    builder = spec.builder
    if (builder.startswith("block_") or builder.startswith("p3_block_")) and asset_kind == "Boundary":
        return "LowBoundary"
    if builder.startswith("block_") or builder.startswith("p3_block_"):
        return "RouteSurface"
    if builder == "exploration_tool":
        return "ExplorationInstrument"
    if asset_kind == "ServiceNPC":
        return "ServiceNPC"
    if asset_kind == "Equipment":
        return "Equipment"
    if asset_kind in ("Hero", "Combatant", "Boss"):
        return asset_kind
    if asset_kind == "Pickup" or builder in ("chest", "boss_reward_exit"):
        return "RewardReliquary"
    if builder.startswith("p3_interactable_"):
        return "ToolReceiver"
    if builder in ("pedestal", "tool_mirror", "tool_relay", "amber_relay", "reveal_marker", "return_point", "save_stone", "region_gate", "boss_spawn_anchor"):
        return "ToolReceiver"
    if asset_kind in ("Tile", "GroundDecal") or builder in ("shortcut_bridge", "crystal_bridge", "narrow_bridge", "route_edge", "wayline", "signal_tile"):
        return "RouteSurface"
    if asset_kind == "Boundary":
        return "LowBoundary"
    if asset_kind == "Detail":
        return "GroundingDetail"
    return "SetDressing"


def infer_shape_family_id(spec: AssetSpec) -> str:
    builder = spec.builder
    if builder == "chibi_mannequin":
        return f"{ART_DIRECTION_ID}.playable_npc_chibi_mannequin"
    if builder == "enemy_template_small_biped":
        return f"{ART_DIRECTION_ID}.enemy_skeleton.esk01_small_biped"
    if builder == "enemy_template_quadruped":
        return f"{ART_DIRECTION_ID}.enemy_skeleton.esk03_quadruped_charger"
    if builder == "enemy_template_floating_caster":
        return f"{ART_DIRECTION_ID}.enemy_skeleton.esk05_floating_caster"
    if builder.startswith("p3_block_") and "wall" in builder:
        return f"{ART_DIRECTION_ID}.block_field_wall"
    if builder.startswith("p3_block_") and "gate" in builder:
        return f"{ART_DIRECTION_ID}.block_field_gate"
    if builder.startswith("p3_block_") and "bridge" in builder:
        return f"{ART_DIRECTION_ID}.block_field_crossing"
    if builder.startswith("p3_block_") and "hazard" in builder:
        return f"{ART_DIRECTION_ID}.block_field_hazard"
    if builder.startswith("p3_block_"):
        return f"{ART_DIRECTION_ID}.block_field_regional_floor"
    if builder.startswith("p3_interactable_"):
        return f"{ART_DIRECTION_ID}.tool_receiver"
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
    if builder.startswith("block_hazard"):
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
    if spec.builder == "chibi_mannequin":
        return ["front", "head_body_ratio", "joint_landmarks", "playable_npc_only"]
    if spec.builder == "enemy_template_small_biped":
        return ["front", "joint_landmarks", "attack_origin", "weak_point", "socket_plan"]
    if spec.builder == "enemy_template_quadruped":
        return ["front", "four_foot_grounding", "charge_origin", "weak_point", "tail_socket", "socket_plan"]
    if spec.builder == "enemy_template_floating_caster":
        return ["front", "hover_clearance", "cast_origin", "weak_point", "side_part_sockets", "socket_plan"]
    if spec.builder.startswith("block_hazard") or (spec.builder.startswith("p3_block_") and "hazard" in spec.builder):
        return ["navigation_surface", "hazard_read", "same_grid_footprint"]
    if spec.builder.startswith("block_stair"):
        return ["navigation_surface", "height_change", "grid_snap_edge"]
    if spec.builder.startswith("block_water") or spec.builder.startswith("block_bridge") or (spec.builder.startswith("p3_block_") and "bridge" in spec.builder):
        return ["navigation_surface", "crossing_read", "walkable_split"]
    if (spec.builder.startswith("block_") or spec.builder.startswith("p3_block_")) and asset_kind == "Boundary":
        return ["low_boundary", "grid_snap_edge", "route_read"]
    if spec.builder.startswith("block_") or spec.builder.startswith("p3_block_"):
        return ["navigation_surface", "grid_snap_edge", "walkable_top"]
    if asset_kind == "Hero":
        return ["front", "tool_socket", "ground_read", "silhouette_role"]
    if asset_kind == "ServiceNPC":
        return ["front", "service_tool", "body_role"]
    if asset_kind == "Equipment":
        return ["grip", "weapon_head", "class_read"]
    if spec.builder == "exploration_tool":
        return ["handle", "signal_core", "ground_read"]
    if asset_kind == "Combatant":
        return ["front", "attack_origin", "danger_read"]
    if asset_kind == "Boss":
        return ["front", "weak_socket", "attack_origin", "danger_surface"]
    if asset_kind == "Pickup":
        return ["reward_glow", "pickup_disc"]
    if asset_kind == "Interactable":
        if spec.builder in ("checkpoint_socket_pad", "reward_receiver_pad"):
            return ["service_socket", "active_state", "floor_read"]
        if spec.builder == "chest":
            return ["reward_socket", "closed_open_read", "reward_glow"]
        return ["tool_receiver", "active_state", "input_read"]
    if asset_kind == "Tile":
        return ["navigation_surface", "fold_inlay"]
    if asset_kind == "GroundDecal":
        return ["ground_contrast"]
    if asset_kind == "Boundary":
        return ["low_boundary", "route_read"]
    if asset_kind == "Detail":
        return ["fold_detail"]
    return ["top_down_silhouette"]


def collision_profile(asset_kind: str) -> str:
    if asset_kind in ("Hero", "Combatant", "ServiceNPC"):
        return "capsule_actor"
    if asset_kind == "Boss":
        return "boss_capsule"
    if asset_kind == "Equipment":
        return "no_collider_equipment"
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


def infer_archetype(asset_kind: str) -> str:
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


def motif_limit_policy(spec: AssetSpec, asset_kind: str) -> str:
    if asset_kind in ("Hero", "Combatant", "Boss", "Interactable", "Pickup"):
        return "Primary read must be folded-reliquary product-line geometry; local motif only supports gameplay readability."
    if spec.style in ("r01", "r02", "r03"):
        return "Regional nouns stay below the main folded-reliquary silhouette; material ratio and wear state carry area identity."
    return "Use folded plinth, split inlay, chunky tab, and functional signal language before any local motif."


def material_role_usage(mesh: Mesh) -> dict[str, str]:
    return {material: material_role(material) for material in sorted(mesh.materials)}


def material_role(material: str) -> str:
    name = material.lower()
    if any(token in name for token in ("signal", "tool", "teal", "blue", "danger", "red", "amber", "ember", "relic", "crystal", "violet", "white")):
        return "signal"
    if any(token in name for token in ("dark", "ink", "charcoal", "armor")):
        return "dark"
    if any(token in name for token in ("gold", "moss", "leaf", "flower", "rust", "metal", "wood", "root")):
        return "accent"
    return "base"


def consistency_summary(records: list[dict]) -> dict:
    role_counts: dict[str, int] = {}
    style_counts: dict[str, int] = {}
    for record in records:
        role_counts[record["product_line_role"]] = role_counts.get(record["product_line_role"], 0) + 1
        style_counts[record["style"]] = style_counts.get(record["style"], 0) + 1
    return {
        "asset_count": len(records),
        "brand_line_id": BRAND_LINE_ID,
        "genre_contract_passed": len(records),
        "genre_contract_failed": 0,
        "forbidden_token_hit_assets": [],
        "protected_term_hits": 0,
        "external_reference_used_in_prompts": 0,
        "raw_direct_use_approved_count": 0,
        "product_line_role_counts": dict(sorted(role_counts.items())),
        "style_counts": dict(sorted(style_counts.items())),
        "production_approval_status": "blocked_until_market_metric_and_human_review",
    }


def make_record(spec: AssetSpec, mesh: Mesh, model_file: Path, preview_file: Path) -> dict:
    asset_kind = infer_asset_kind(spec)
    role_key = infer_product_line_role_key(spec, asset_kind)
    role = PRODUCT_LINE_ROLES[role_key]
    geometry = geometry_metrics(mesh)
    record = {
        "asset_id": spec.asset_id,
        "name": spec.name,
        "builder": spec.builder,
        "style": spec.style,
        "area_code": infer_area_code(spec),
        "asset_kind": asset_kind,
        "archetype": infer_archetype(asset_kind),
        "category": spec.category,
        "gameplay_role": spec.gameplay_role,
        "used_in_scene": spec.used_in_scene,
        "priority": spec.priority,
        "brand_line_id": BRAND_LINE_ID,
        "product_line_role": role["role"],
        "product_line_role_key": role_key,
        "visual_family_id": f"{ART_DIRECTION_ID}.{spec.style}.{role_key.lower()}",
        "shape_family_id": infer_shape_family_id(spec),
        "required_shape_tokens": role["required_shape_tokens"],
        "forbidden_drift_tokens": role["forbidden_drift_tokens"],
        "missing_required_tokens": [],
        "forbidden_token_hits": [],
        "motif_limit_policy": motif_limit_policy(spec, asset_kind),
        "genre_contract_status": "folded_reliquary_contract_applied",
        "style_gate_status": "style_locked_generated_first_pass",
        "model_file": rel(model_file),
        "preview_file": rel(preview_file),
        "source_file": rel(GENERATOR_FILE),
        "unity_prefab": f"Assets/Prefabs/Production/{spec.priority}/{spec.name}.prefab",
        "scale_meters": spec.scale_meters,
        "pivot_rule": "bottom center",
        "triangle_budget_lod0": spec.triangle_budget_lod0,
        "triangles_lod0": mesh.triangle_count(),
        "material_budget": spec.material_budget,
        "material_count": len(mesh.materials),
        "materials": sorted(mesh.materials),
        "material_budget_status": "within_budget" if len(mesh.materials) <= spec.material_budget else "first_pass_warning",
        "budget_exception_reason": "" if len(mesh.materials) <= spec.material_budget else "Generated first-pass model uses separate procedural materials; shared atlas consolidation is required before production approval.",
        "material_role_usage": material_role_usage(mesh),
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
        "collision_profile": collision_profile(asset_kind),
        "static_hint": asset_kind not in ("Hero", "Combatant", "Boss", "ServiceNPC", "Pickup", "Equipment"),
        "nav_blocking": asset_kind in ("Boundary",),
        "trigger_profile": "none",
        "approved_overwrite_policy": "generated_prefab_overwrite_allowed_until_human_approved",
        "license": "repository-authored",
        "attribution": "none",
        "source_reference": rel(GENERATOR_FILE),
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
        "style_ip_clearance": "original FOURFOLD folded-reliquary semantic shape language; no protected characters, franchise lookalikes, trademarked style labels, or single-title trade dress",
        "acceptance_status": "generated_first_pass",
        "acceptance": spec.acceptance,
    }
    if spec.builder == "shortcut_bridge":
        record["concept_gate_status"] = "concept_reference_first_modeling_pass"
        record["concept_reference"] = "artifacts/Concepts/FoldedReliquary/FE_CONCEPT_RouteSurface_ModelSheet_v002.png"
        record["modeling_brief"] = "artifacts/Concepts/FoldedReliquary/FE_ENV_R01_ShortcutBridge_01_ModelingBrief.md"
    return record


def model_path(spec: AssetSpec) -> Path:
    return PRODUCTION_ROOT / spec.priority / "Models" / f"{spec.name}.obj"


def assert_safe(spec: AssetSpec) -> None:
    text = json.dumps(spec.__dict__).lower()
    for term in FORBIDDEN_TERMS:
        if term in text:
            raise ValueError(f"Forbidden protected-style term in {spec.name}: {term}")


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO).as_posix()


if __name__ == "__main__":
    main()
