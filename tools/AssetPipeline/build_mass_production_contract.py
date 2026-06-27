#!/usr/bin/env python3
"""Build first-pass contract outputs for the mass-production asset batch.

This script does not try to polish individual assets. It creates the shared
rules, contract-named texture stubs, UI/VFX/audio/animation first-pass files,
and an asset_manifest.json that the Blender LOD converter and validator can use.
"""

from __future__ import annotations

import hashlib
import json
import math
import re
import wave
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image, ImageDraw


REPO = Path(__file__).resolve().parents[2]
SOURCE_MANIFEST = REPO / "artifacts" / "Reports" / "fourfold-model-pack.json"
OUTPUT_ROOT = REPO / "Assets" / "Art"
SHARED_DIR = REPO / "Assets" / "Art" / "Production" / "Shared"
REPORT_DIR = REPO / "artifacts" / "Reports"
ASSET_MANIFEST = REPO / "asset_manifest.json"

GENERATED_AT = "2026-06-27T00:00:00+09:00"

CATEGORY_MAP = {
    "Character": "player",
    "NPC": "npc",
    "Enemy": "enemy",
    "Boss": "boss",
    "Equipment": "equipment",
    "Environment": "environment",
    "Prop": "prop",
}

AVATAR_POLICY = {
    "player": "Humanoid",
    "npc": "Humanoid",
    "enemy": "Generic",
    "elite": "Generic",
    "boss": "Generic",
    "equipment": "None",
    "environment": "None",
    "prop": "None",
    "ui": "None",
    "vfx": "None",
    "audio": "None",
    "anim": "HumanoidOrGenericByBinding",
}

DEFAULT_CLIPS = {
    "player": ["idle", "walk", "run", "attack01", "hit", "death"],
    "npc": ["idle", "walk"],
    "enemy": ["idle", "walk", "attack01", "hit", "death"],
    "elite": ["idle", "walk", "attack01", "hit", "death"],
    "boss": ["idle", "walk", "attack01", "hit", "death"],
}

UI_ITEMS = [
    "potion_hp", "potion_mp", "elixir", "revive_leaf", "iron_key", "boss_key",
    "coin", "gem_blue", "gem_red", "gem_green", "ore_iron", "ore_gold",
    "herb_green", "mushroom", "scroll_fire", "scroll_ice", "scroll_heal", "ticket",
    "sword_common", "sword_rare", "axe_common", "bow_common", "staff_common", "shield_common",
    "helmet", "armor", "boots", "gloves", "ring", "amulet",
    "skill_slash", "skill_dash", "skill_firebolt", "skill_iceburst", "skill_heal", "skill_barrier",
    "buff_attack", "buff_defense", "debuff_poison", "debuff_slow", "quest_main", "quest_side",
    "map_marker", "save_marker", "shop", "forge", "inventory", "settings",
]

VFX_ITEMS = [
    "hit_spark", "slash_arc", "fire_cast", "ice_burst", "heal_pulse", "buff_ring",
    "debuff_smoke", "loot_beam", "projectile_orb", "projectile_arrow", "aoe_warning", "boss_roar",
    "dash_trail", "guard_clash", "death_pop", "spawn_flash",
]

AUDIO_ITEMS = [
    ("ui_click", 660), ("ui_confirm", 880), ("ui_cancel", 330), ("pickup_coin", 990),
    ("pickup_item", 740), ("sword_swing", 220), ("sword_hit", 180), ("bow_release", 520),
    ("magic_cast", 440), ("magic_hit", 360), ("enemy_hit", 260), ("enemy_death", 140),
    ("player_step", 120), ("door_open", 200), ("rare_drop", 1040), ("level_up", 1320),
]

ANIM_GROUPS = {
    "player_shared": ("player", DEFAULT_CLIPS["player"]),
    "npc_shared": ("npc", DEFAULT_CLIPS["npc"]),
    "enemy_small_biped": ("enemy", DEFAULT_CLIPS["enemy"]),
    "enemy_quadruped": ("enemy", DEFAULT_CLIPS["enemy"]),
    "enemy_floating_caster": ("enemy", DEFAULT_CLIPS["enemy"]),
    "boss_generic_large": ("boss", DEFAULT_CLIPS["boss"]),
}

MINIMUM_COUNTS = {
    "player": 6,
    "npc": 3,
    "enemy": 12,
    "elite": 4,
    "boss": 4,
    "equipment": 5,
    "environment": 20,
    "prop": 20,
    "ui": 32,
    "vfx": 12,
    "audio": 12,
    "anim": 20,
}


def main() -> None:
    source = json.loads(SOURCE_MANIFEST.read_text(encoding="utf-8"))
    source_assets = source.get("assets", [])
    ensure_dirs()
    write_shared_contracts(source)

    manifest_assets = []
    for asset in source_assets:
        entry = build_3d_entry(asset)
        create_texture_set(entry)
        manifest_assets.append(entry)

    manifest_assets.extend(build_ui_entries())
    manifest_assets.extend(build_vfx_entries())
    manifest_assets.extend(build_audio_entries())
    manifest_assets.extend(build_animation_entries())

    manifest = {
        "schema_version": "mass_asset_contract_v1",
        "generated_at": GENERATED_AT,
        "project": "FOURFOLD ECHOES",
        "target_engine": "Unity",
        "game_type": "stylized fantasy ARPG",
        "pipeline_order": [
            "concept",
            "skeleton_avatar_policy",
            "motion_design",
            "blockout_and_body",
            "ai_mocap_or_generation",
            "cleanup_import_validation",
        ],
        "shared_rules": rel(SHARED_DIR / "shared_rules.json"),
        "avatar_policy": rel(SHARED_DIR / "avatar_policy.json"),
        "motion_design": rel(SHARED_DIR / "motion_design.json"),
        "source_manifest": rel(SOURCE_MANIFEST),
        "asset_root": rel(OUTPUT_ROOT),
        "minimum_counts": MINIMUM_COUNTS,
        "coverage_summary": coverage_summary(manifest_assets),
        "unity_import": {
            "status": "blocked",
            "reason": "Unity batchmode licensing initialization failed during production_art.import_model_pack.",
            "last_attempt": "tools/unity_forge_command.sh Temp/FourfoldForgeInbox/commands/import.production_art_model_pack.esk05_template.2026062704.ready.json",
        },
        "assets": manifest_assets,
    }
    ASSET_MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {rel(ASSET_MANIFEST)} ({len(manifest_assets)} assets)")
    print(f"Coverage: {json.dumps(manifest['coverage_summary'], sort_keys=True)}")


def ensure_dirs() -> None:
    for path in [
        SHARED_DIR,
        REPORT_DIR,
    ]:
        path.mkdir(parents=True, exist_ok=True)


def write_shared_contracts(source: dict) -> None:
    shared_rules = {
        "schema_version": "shared_rules_v1",
        "generated_at": GENERATED_AT,
        "art_direction": "stylized readable fantasy ARPG, pop-deformed, silhouette-first",
        "output_priority": ["coverage", "spec_compliance", "naming_consistency", "unity_import_success"],
        "non_goals": ["single_asset_polish_loop", "photoreal_microdetail", "untracked_manual_assets"],
        "naming_rule": "[category]_[name]_[variant]_[part]_lod[0-2]",
        "texture_suffixes": ["_bc", "_n", "_orm", "_emi", "_msk"],
        "animation_suffixes": ["@idle", "@walk", "@run", "@attack01", "@hit", "@death"],
        "scale": "Unity 1 unit = 1 meter",
        "pivot_defaults": {
            "character": "feet bottom-center",
            "enemy": "ground center or hover ground reference",
            "weapon": "grip",
            "environment": "grid snap origin",
            "ui": "center",
            "audio": "not applicable",
        },
        "mesh_defaults": {
            "lods": ["lod0", "lod1", "lod2"],
            "uv0": "required",
            "uv2": "required only for static lightmapped environment/building assets",
            "material_slots": "minimum necessary; default <= 5",
            "texture_set": ["bc", "n", "orm"],
        },
        "batch_rule": "Generate the full batch first, then regenerate only failed validation assets. Max two micro-fix loops per asset.",
        "source_art_direction_contract": source.get("art_direction_contract", ""),
    }
    avatar_policy = {
        "schema_version": "avatar_policy_v1",
        "generated_at": GENERATED_AT,
        "policy": AVATAR_POLICY,
        "player_npc_template": {
            "skeleton": "FE_CHAR_TEMPLATE_ChibiMannequin_01",
            "avatar": "Humanoid",
            "model": "Assets/Art/Production/P0/Models/FE_CHAR_TEMPLATE_ChibiMannequin_01.obj",
        },
        "enemy_templates": source.get("enemy_skeleton_templates", {}),
        "rule": "Avatar and movement family are selected before body modeling. Do not model a creature whose attack origin, locomotion, or root motion policy is unknown.",
    }
    motion_design = {
        "schema_version": "motion_design_v1",
        "generated_at": GENERATED_AT,
        "default_clip_sets": DEFAULT_CLIPS,
        "root_motion": {
            "player": "off by default; controller-driven ARPG movement",
            "npc": "off by default; nav/controller-driven",
            "enemy": "off for normal locomotion; charge clips may expose event windows",
            "boss": "mixed; large attacks use authored event windows",
        },
        "animation_events": ["footstep", "attack_active_start", "attack_active_end", "vfx_spawn", "projectile_release", "hit_react", "death_hide"],
        "rule": "Motion contract must exist before blockout/fleshing. Generated mocap or procedural clips clean up to this contract.",
    }
    (SHARED_DIR / "shared_rules.json").write_text(json.dumps(shared_rules, indent=2) + "\n", encoding="utf-8")
    (SHARED_DIR / "avatar_policy.json").write_text(json.dumps(avatar_policy, indent=2) + "\n", encoding="utf-8")
    (SHARED_DIR / "motion_design.json").write_text(json.dumps(motion_design, indent=2) + "\n", encoding="utf-8")


def build_3d_entry(asset: dict) -> dict:
    category = category_for_asset(asset)
    name = compact_name(asset.get("name", asset.get("asset_id", "asset")))
    variant = variant_for_asset(asset)
    part = part_for_category(category)
    stem = f"{category}_{name}_{variant}_{part}"
    asset_dir = category_asset_dir(category, stem)
    model_dir = asset_dir / "Models"
    texture_dir = asset_dir / "Textures"
    model_dir.mkdir(parents=True, exist_ok=True)
    texture_dir.mkdir(parents=True, exist_ok=True)
    lods = {
        f"lod{index}": rel(model_dir / f"{stem}_lod{index}.fbx")
        for index in range(3)
    }
    textures = {
        suffix: rel(texture_dir / f"{stem}_{suffix}.png")
        for suffix in ("bc", "n", "orm")
    }
    return {
        "asset_id": asset.get("asset_id"),
        "source_asset_name": asset.get("name"),
        "contract_name": stem,
        "category": category,
        "kind": "3d",
        "pipeline_stage": "blockout_and_body",
        "concept_status": "existing_project_owned_first_pass",
        "skeleton_avatar_policy": AVATAR_POLICY[category],
        "motion_design": DEFAULT_CLIPS.get(category, []),
        "source_model": asset.get("model_file"),
        "final_files": lods,
        "textures": textures,
        "unity_prefab": f"Assets/Prefabs/MassProduction/{category}/{stem}.prefab",
        "preview_file": asset.get("preview_file"),
        "pivot": pivot_for_category(category),
        "scale": "unity_meters",
        "uv": {"uv0": "required", "uv2": "required_if_static_lightmapped"},
        "material_policy": "minimum_materials_first_pass",
        "unity_import_status": "blocked_by_unity_license",
        "validation_status": "pending",
    }


def build_ui_entries() -> list[dict]:
    entries = []
    for item in UI_ITEMS:
        category = "ui"
        stem = f"{category}_{item}_v001_icon"
        path128 = OUTPUT_ROOT / "UI" / "Icons" / "Generated" / stem / f"{stem}_128.png"
        path256 = OUTPUT_ROOT / "UI" / "Icons" / "Generated" / stem / f"{stem}_256.png"
        create_icon(path128, item, 128)
        create_icon(path256, item, 256)
        entries.append({
            "asset_id": f"ui.{item}",
            "contract_name": stem,
            "category": category,
            "kind": "ui",
            "pipeline_stage": "cleanup_import_validation",
            "concept_status": "procedural_first_pass",
            "skeleton_avatar_policy": "None",
            "motion_design": [],
            "final_files": {"png_128": rel(path128), "png_256": rel(path256)},
            "unity_import_status": "blocked_by_unity_license",
            "validation_status": "pending",
        })
    return entries


def build_vfx_entries() -> list[dict]:
    entries = []
    for item in VFX_ITEMS:
        category = "vfx"
        stem = f"{category}_{item}_v001_flipbook"
        path = OUTPUT_ROOT / "VFX" / "Generated" / stem / f"{stem}.png"
        create_vfx_flipbook(path, item)
        entries.append({
            "asset_id": f"vfx.{item}",
            "contract_name": stem,
            "category": category,
            "kind": "vfx",
            "pipeline_stage": "cleanup_import_validation",
            "concept_status": "procedural_first_pass",
            "skeleton_avatar_policy": "None",
            "motion_design": ["vfx_spawn"],
            "final_files": {"png": rel(path)},
            "unity_import_status": "blocked_by_unity_license",
            "validation_status": "pending",
        })
    return entries


def build_audio_entries() -> list[dict]:
    entries = []
    for item, frequency in AUDIO_ITEMS:
        category = "audio"
        stem = f"{category}_{item}_v001_master"
        path = OUTPUT_ROOT / "Audio" / "SFX" / stem / f"{stem}.wav"
        create_wav(path, frequency)
        entries.append({
            "asset_id": f"audio.{item}",
            "contract_name": stem,
            "category": category,
            "kind": "audio",
            "pipeline_stage": "cleanup_import_validation",
            "concept_status": "procedural_first_pass",
            "skeleton_avatar_policy": "None",
            "motion_design": [],
            "final_files": {"wav": rel(path)},
            "unity_import_status": "blocked_by_unity_license",
            "validation_status": "pending",
        })
    return entries


def build_animation_entries() -> list[dict]:
    entries = []
    for group, (category, clips) in ANIM_GROUPS.items():
        for clip in clips:
            stem = f"anim_{group}_v001_body@{clip}"
            path = OUTPUT_ROOT / "Animations" / "Generated" / group / f"{stem}.anim"
            create_anim_stub(path, stem, clip)
            entries.append({
                "asset_id": f"anim.{group}.{clip}",
                "contract_name": stem,
                "category": "anim",
                "kind": "animation",
                "pipeline_stage": "motion_design",
                "concept_status": "not_applicable",
                "skeleton_avatar_policy": AVATAR_POLICY[category],
                "motion_design": [clip],
                "final_files": {"anim": rel(path)},
                "unity_import_status": "blocked_by_unity_license",
                "validation_status": "pending",
            })
    return entries


def create_texture_set(entry: dict) -> None:
    color = color_from_name(entry["contract_name"])
    for suffix, rel_path in entry["textures"].items():
        path = REPO / rel_path
        if suffix == "bc":
            image = Image.new("RGBA", (64, 64), (*color, 255))
            draw = ImageDraw.Draw(image, "RGBA")
            draw.rectangle((0, 0, 63, 63), outline=(255, 255, 255, 48))
            draw.line((0, 48, 64, 24), fill=(255, 255, 255, 32), width=4)
        elif suffix == "n":
            image = Image.new("RGBA", (64, 64), (128, 128, 255, 255))
        else:
            image = Image.new("RGBA", (64, 64), (180, 128, 12, 255))
        image.save(path)


def create_icon(path: Path, name: str, size: int) -> None:
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image, "RGBA")
    color = color_from_name(name)
    scale = size / 128

    def s(value: int) -> int:
        return round(value * scale)

    draw.rounded_rectangle((s(16), s(16), s(112), s(112)), radius=s(22), fill=(*color, 230), outline=(25, 30, 38, 255), width=max(1, s(5)))
    draw.ellipse((s(38), s(28), s(90), s(80)), fill=(255, 255, 255, 66))
    draw.polygon([(s(64), s(26)), (s(82), s(62)), (s(64), s(102)), (s(46), s(62))], fill=(255, 255, 255, 110))
    draw.rectangle((s(34), s(86), s(94), s(98)), fill=(30, 35, 42, 180))
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def create_vfx_flipbook(path: Path, name: str) -> None:
    cell = 128
    image = Image.new("RGBA", (cell * 4, cell * 4), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image, "RGBA")
    color = color_from_name(name)
    for frame in range(16):
        x = (frame % 4) * cell
        y = (frame // 4) * cell
        radius = 14 + frame * 3
        alpha = max(40, 220 - frame * 9)
        center = (x + 64, y + 64)
        draw.ellipse((center[0] - radius, center[1] - radius, center[0] + radius, center[1] + radius), outline=(*color, alpha), width=6)
        draw.line((x + 64, y + 16, x + 64, y + 112), fill=(255, 255, 255, alpha // 2), width=3)
        draw.line((x + 16, y + 64, x + 112, y + 64), fill=(*color, alpha // 2), width=3)
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def create_wav(path: Path, frequency: int) -> None:
    sample_rate = 44100
    duration = 0.35
    frames = int(sample_rate * duration)
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(sample_rate)
        data = bytearray()
        for index in range(frames):
            t = index / sample_rate
            envelope = math.sin(math.pi * min(1.0, index / frames))
            sample = int(12000 * envelope * math.sin(2 * math.pi * frequency * t))
            data.extend(sample.to_bytes(2, "little", signed=True))
        wav.writeframes(bytes(data))


def create_anim_stub(path: Path, name: str, clip: str) -> None:
    # Minimal Unity YAML AnimationClip placeholder. Motion cleanup replaces curves later.
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        "\n".join([
            "%YAML 1.1",
            "%TAG !u! tag:unity3d.com,2011:",
            "--- !u!74 &7400000",
            "AnimationClip:",
            "  m_ObjectHideFlags: 0",
            "  m_CorrespondingSourceObject: {fileID: 0}",
            "  m_PrefabInstance: {fileID: 0}",
            "  m_PrefabAsset: {fileID: 0}",
            f"  m_Name: {name}",
            "  serializedVersion: 7",
            "  m_Legacy: 0",
            "  m_Compressed: 0",
            "  m_UseHighQualityCurve: 1",
            "  m_RotationCurves: []",
            "  m_CompressedRotationCurves: []",
            "  m_EulerCurves: []",
            "  m_PositionCurves: []",
            "  m_ScaleCurves: []",
            "  m_FloatCurves: []",
            "  m_PPtrCurves: []",
            "  m_SampleRate: 30",
            "  m_WrapMode: 0",
            "  m_Bounds:",
            "    m_Center: {x: 0, y: 0.8, z: 0}",
            "    m_Extent: {x: 0.5, y: 0.8, z: 0.5}",
            "  m_ClipBindingConstant:",
            "    genericBindings: []",
            "    pptrCurveMapping: []",
            "  m_AnimationClipSettings:",
            "    serializedVersion: 2",
            "    m_AdditiveReferencePoseClip: {fileID: 0}",
            "    m_AdditiveReferencePoseTime: 0",
            "    m_StartTime: 0",
            f"    m_StopTime: {clip_duration(clip)}",
            "    m_OrientationOffsetY: 0",
            "    m_Level: 0",
            "    m_CycleOffset: 0",
            "    m_HasAdditiveReferencePose: 0",
            f"    m_LoopTime: {1 if clip in {'idle', 'walk', 'run'} else 0}",
            "    m_LoopBlend: 0",
            "    m_LoopBlendOrientation: 0",
            "    m_LoopBlendPositionY: 0",
            "    m_LoopBlendPositionXZ: 0",
            "    m_KeepOriginalOrientation: 1",
            "    m_KeepOriginalPositionY: 1",
            "    m_KeepOriginalPositionXZ: 1",
            "    m_HeightFromFeet: 0",
            "    m_Mirror: 0",
            "  m_EditorCurves: []",
            "  m_EulerEditorCurves: []",
            "  m_HasGenericRootTransform: 0",
            "  m_HasMotionFloatCurves: 0",
            "  m_Events: []",
        ]) + "\n",
        encoding="utf-8",
    )


def clip_duration(clip: str) -> float:
    return {
        "idle": 2.0,
        "walk": 1.0,
        "run": 0.8,
        "attack01": 0.8,
        "hit": 0.4,
        "death": 1.4,
    }.get(clip, 1.0)


def coverage_summary(assets: list[dict]) -> dict:
    counts: dict[str, int] = {}
    for asset in assets:
        counts[asset["category"]] = counts.get(asset["category"], 0) + 1
    missing = {
        category: {"required": minimum, "actual": counts.get(category, 0)}
        for category, minimum in MINIMUM_COUNTS.items()
        if counts.get(category, 0) < minimum
    }
    return {"counts": dict(sorted(counts.items())), "missing_minimums": missing}


def compact_name(name: str) -> str:
    cleaned = re.sub(r"^FE_", "", name, flags=re.IGNORECASE)
    cleaned = re.sub(r"[^A-Za-z0-9]+", "_", cleaned).strip("_").lower()
    cleaned = re.sub(r"_(lod[0-2]|obj|fbx)$", "", cleaned)
    return cleaned[:64] or "asset"


def category_for_asset(asset: dict) -> str:
    category = CATEGORY_MAP.get(asset.get("category"), "prop")
    if category != "enemy":
        return category
    name = " ".join([
        str(asset.get("name") or ""),
        str(asset.get("asset_id") or ""),
        str(asset.get("archetype") or ""),
        str(asset.get("product_line_role") or ""),
    ]).lower()
    elite_tokens = (
        "floatingcaster",
        "miniboss",
        "shieldclamp",
        "blockcharger",
        "elite",
    )
    if any(token in name for token in elite_tokens):
        return "elite"
    return "enemy"


def variant_for_asset(asset: dict) -> str:
    area = str(asset.get("area_code") or asset.get("style") or "v001")
    cleaned = re.sub(r"[^A-Za-z0-9]+", "_", area).strip("_").lower()
    if cleaned in {"", "common", "module"}:
        return "v001"
    return cleaned[:24]


def part_for_category(category: str) -> str:
    return {
        "player": "body",
        "npc": "body",
        "enemy": "body",
        "elite": "body",
        "boss": "body",
        "equipment": "mesh",
        "environment": "module",
        "prop": "mesh",
    }.get(category, "asset")


def category_asset_dir(category: str, stem: str) -> Path:
    roots = {
        "player": OUTPUT_ROOT / "Characters" / "Player",
        "npc": OUTPUT_ROOT / "Characters" / "NPC",
        "enemy": OUTPUT_ROOT / "Enemies" / "Common",
        "elite": OUTPUT_ROOT / "Enemies" / "Elite",
        "boss": OUTPUT_ROOT / "Enemies" / "Bosses",
        "equipment": OUTPUT_ROOT / "Weapons" / "Generated",
        "environment": OUTPUT_ROOT / "Environment" / "Generated",
        "prop": OUTPUT_ROOT / "Props" / "Common",
    }
    return roots.get(category, OUTPUT_ROOT / "Misc" / category) / stem


def pivot_for_category(category: str) -> str:
    return {
        "player": "feet_bottom_center",
        "npc": "feet_bottom_center",
        "enemy": "ground_or_hover_reference_center",
        "elite": "ground_or_hover_reference_center",
        "boss": "ground_center",
        "equipment": "grip_or_base",
        "environment": "grid_snap_origin",
        "prop": "base_center",
    }.get(category, "center")


def color_from_name(name: str) -> tuple[int, int, int]:
    digest = hashlib.sha1(name.encode("utf-8")).digest()
    return (
        96 + digest[0] % 120,
        80 + digest[1] % 130,
        72 + digest[2] % 140,
    )


def rel(path: Path | str) -> str:
    return Path(path).resolve().relative_to(REPO).as_posix()


if __name__ == "__main__":
    main()
