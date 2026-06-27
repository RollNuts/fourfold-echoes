#!/usr/bin/env python3
"""Validate the mass-production asset contract and write QA outputs."""

from __future__ import annotations

import hashlib
import json
import re
import wave
from pathlib import Path

from PIL import Image


REPO = Path(__file__).resolve().parents[2]
ASSET_MANIFEST = REPO / "asset_manifest.json"
VALIDATION_REPORT = REPO / "validation_report.json"
FAILED_ASSETS = REPO / "failed_assets.json"
RETRY_QUEUE = REPO / "retry_queue.json"
REQUIRED_TEXTURES = ("bc", "n", "orm")
REQUIRED_LODS = ("lod0", "lod1", "lod2")
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
    manifest = json.loads(ASSET_MANIFEST.read_text(encoding="utf-8"))
    failed = []
    passed = []
    warnings = []

    coverage = validate_coverage(manifest.get("assets", []), failed)
    all_meta_paths = []
    for asset in manifest.get("assets", []):
        errors = validate_asset(asset)
        all_meta_paths.extend(meta_paths_for_asset(asset))
        asset["local_validation_errors"] = errors
        asset["validation_status"] = "failed" if errors else "local_pass"
        if errors:
            failed.append({
                "asset_id": asset.get("asset_id"),
                "contract_name": asset.get("contract_name"),
                "category": asset.get("category"),
                "kind": asset.get("kind"),
                "errors": errors,
            })
        else:
            passed.append(asset.get("contract_name"))
    meta_errors = validate_meta_guid_uniqueness(all_meta_paths)
    for error in meta_errors:
        failed.append({
            "asset_id": "unity.meta",
            "contract_name": "unity_meta_guid_integrity",
            "category": "unity",
            "kind": "meta",
            "errors": [error],
        })

    environment_blockers = environment_blockers_report()
    retry_queue = build_retry_queue(failed)
    report = {
        "schema_version": "mass_asset_validation_v1",
        "generated_at": "2026-06-27T00:00:00+09:00",
        "summary": {
            "asset_count": len(manifest.get("assets", [])),
            "local_passed": len(passed),
            "local_failed": len(failed),
            "coverage_pass": not coverage["missing_minimums"],
            "unity_import_pass": False,
            "unity_import_status": "blocked",
        },
        "coverage": coverage,
        "environment_blockers": environment_blockers,
        "warnings": warnings,
        "failed_assets_path": rel(FAILED_ASSETS),
        "retry_queue_path": rel(RETRY_QUEUE),
    }

    manifest["coverage_summary"] = coverage
    manifest["validation_summary"] = report["summary"]
    ASSET_MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    VALIDATION_REPORT.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    FAILED_ASSETS.write_text(json.dumps({
        "schema_version": "failed_assets_v1",
        "generated_at": report["generated_at"],
        "environment_blockers": environment_blockers,
        "failed_assets": failed,
    }, indent=2) + "\n", encoding="utf-8")
    RETRY_QUEUE.write_text(json.dumps({
        "schema_version": "retry_queue_v1",
        "generated_at": report["generated_at"],
        "retry_policy": "Only failed assets are eligible. Do not regenerate local_pass assets. Max retry_count is 2.",
        "items": retry_queue,
    }, indent=2) + "\n", encoding="utf-8")

    print(json.dumps(report["summary"], indent=2))
    print(f"Wrote {rel(VALIDATION_REPORT)}, {rel(FAILED_ASSETS)}, {rel(RETRY_QUEUE)}")


def validate_coverage(assets: list[dict], failed: list[dict]) -> dict:
    counts: dict[str, int] = {}
    for asset in assets:
        counts[asset.get("category", "unknown")] = counts.get(asset.get("category", "unknown"), 0) + 1
    missing = {
        category: {"required": minimum, "actual": counts.get(category, 0)}
        for category, minimum in MINIMUM_COUNTS.items()
        if counts.get(category, 0) < minimum
    }
    for category, detail in missing.items():
        failed.append({
            "asset_id": f"coverage.{category}",
            "contract_name": f"coverage_{category}",
            "category": category,
            "kind": "coverage",
            "errors": [f"coverage below minimum: {detail['actual']} < {detail['required']}"],
        })
    return {"counts": dict(sorted(counts.items())), "missing_minimums": missing}


def validate_asset(asset: dict) -> list[str]:
    kind = asset.get("kind")
    if kind == "3d":
        return validate_3d(asset)
    if kind == "ui":
        return validate_ui(asset)
    if kind == "vfx":
        return validate_vfx(asset)
    if kind == "audio":
        return validate_audio(asset)
    if kind == "animation":
        return validate_animation(asset)
    return [f"unsupported kind: {kind}"]


def validate_3d(asset: dict) -> list[str]:
    errors = []
    category = asset.get("category", "")
    contract_name = asset.get("contract_name", "")
    if not contract_name.startswith(f"{category}_"):
        errors.append("contract_name does not start with category prefix")
    lod_hashes = {}
    lod_triangles = {}
    for lod in REQUIRED_LODS:
        path_rel = asset.get("final_files", {}).get(lod)
        if not path_rel:
            errors.append(f"missing final_files.{lod}")
            continue
        path = REPO / path_rel
        if path.suffix.lower() != ".fbx":
            errors.append(f"{lod} is not FBX: {path_rel}")
        if not re.search(rf"_lod{lod[-1]}\.fbx$", path.name):
            errors.append(f"{lod} filename does not end with _{lod}.fbx: {path.name}")
        if not path.exists():
            errors.append(f"missing {lod} FBX: {path_rel}")
        else:
            errors.extend(validate_meta(path_rel, "ModelImporter"))
            lod_hashes[lod] = sha256(path)
            triangles = read_fbx_triangle_count(path)
            if triangles is None:
                errors.append(f"{lod} FBX missing FOURFOLD_TRIANGLES metadata: {path_rel}")
            else:
                lod_triangles[lod] = triangles
            if not fbx_has_uv0(path):
                errors.append(f"{lod} FBX missing UV0 LayerElementUV: {path_rel}")
    if len(set(lod_hashes.values())) == 1 and len(lod_hashes) == 3:
        errors.append("LOD0/1/2 FBX files are byte-identical; reduced LODs must be generated")
    if all(lod in lod_triangles for lod in REQUIRED_LODS):
        if not (lod_triangles["lod0"] > lod_triangles["lod1"] > lod_triangles["lod2"]):
            errors.append(f"LOD triangle counts must decrease lod0>lod1>lod2, got {lod_triangles}")
    for suffix in REQUIRED_TEXTURES:
        path_rel = asset.get("textures", {}).get(suffix)
        if not path_rel:
            errors.append(f"missing texture map {suffix}")
            continue
        path = REPO / path_rel
        if not path.name.endswith(f"_{suffix}.png"):
            errors.append(f"texture suffix mismatch for {suffix}: {path.name}")
        if not path.exists():
            errors.append(f"missing texture file: {path_rel}")
        else:
            errors.extend(validate_meta(path_rel, "TextureImporter"))
    if asset.get("skeleton_avatar_policy") not in {"Humanoid", "Generic", "None"}:
        errors.append(f"invalid skeleton/avatar policy: {asset.get('skeleton_avatar_policy')}")
    return errors


def validate_ui(asset: dict) -> list[str]:
    errors = []
    files = asset.get("final_files", {})
    for key, expected_size in (("png_128", 128), ("png_256", 256)):
        path_rel = files.get(key)
        if not path_rel:
            errors.append(f"missing {key}")
            continue
        path = REPO / path_rel
        if not path.exists():
            errors.append(f"missing UI PNG: {path_rel}")
            continue
        errors.extend(validate_meta(path_rel, "TextureImporter"))
        try:
            with Image.open(path) as image:
                if image.size != (expected_size, expected_size):
                    errors.append(f"{key} size mismatch: {image.size}")
                if image.mode != "RGBA":
                    errors.append(f"{key} is not transparent RGBA: {image.mode}")
        except Exception as error:
            errors.append(f"cannot read UI PNG {path_rel}: {error}")
    return errors


def validate_vfx(asset: dict) -> list[str]:
    errors = []
    path_rel = asset.get("final_files", {}).get("png")
    if not path_rel:
        return ["missing vfx png"]
    path = REPO / path_rel
    if not path.exists():
        return [f"missing VFX PNG: {path_rel}"]
    errors.extend(validate_meta(path_rel, "TextureImporter"))
    try:
        with Image.open(path) as image:
            if image.mode != "RGBA":
                errors.append(f"VFX flipbook is not RGBA: {image.mode}")
            if image.size != (512, 512):
                errors.append(f"VFX flipbook expected 512x512, got {image.size}")
    except Exception as error:
        errors.append(f"cannot read VFX PNG {path_rel}: {error}")
    return errors


def validate_audio(asset: dict) -> list[str]:
    path_rel = asset.get("final_files", {}).get("wav")
    if not path_rel:
        return ["missing wav"]
    path = REPO / path_rel
    if not path.exists():
        return [f"missing WAV: {path_rel}"]
    errors = []
    errors.extend(validate_meta(path_rel, "AudioImporter"))
    try:
        with wave.open(str(path), "rb") as wav:
            if wav.getnchannels() != 1:
                errors.append("WAV must be mono for SFX first pass")
            if wav.getframerate() != 44100:
                errors.append(f"WAV sample rate expected 44100, got {wav.getframerate()}")
    except Exception as error:
        errors.append(f"cannot read WAV {path_rel}: {error}")
    return errors


def validate_animation(asset: dict) -> list[str]:
    path_rel = asset.get("final_files", {}).get("anim")
    if not path_rel:
        return ["missing anim file"]
    path = REPO / path_rel
    errors = []
    if not path.exists():
        errors.append(f"missing AnimationClip stub: {path_rel}")
    else:
        errors.extend(validate_meta(path_rel, "NativeFormatImporter"))
    if "@" not in path.name:
        errors.append(f"animation file lacks @clip suffix: {path.name}")
    clip = path.name.split("@")[-1].removesuffix(".anim") if "@" in path.name else ""
    if clip and clip not in {"idle", "walk", "run", "attack01", "hit", "death"}:
        errors.append(f"unsupported animation clip suffix: @{clip}")
    return errors


def environment_blockers_report() -> list[dict]:
    blockers = [
        {
            "code": "UNITY_LICENSE_INIT_FAILED",
            "severity": "blocker",
            "message": "Unity batchmode import could not complete because licensing initialization timed out.",
            "command": "tools/unity_forge_command.sh commands/samples/import-mass-asset-contract-batch.json",
        },
        {
            "code": "BLENDER_METAL_STARTUP_CRASH",
            "severity": "blocker",
            "message": "Blender 5.1.2 crashes during empty background startup, so OBJ->FBX conversion and true LOD decimation could not run.",
            "command": "Blender --background --factory-startup --python-expr \"print('blender-start-ok')\"",
        },
    ]
    return blockers


def meta_paths_for_asset(asset: dict) -> list[str]:
    paths = []
    for value in (asset.get("final_files") or {}).values():
        paths.append(f"{value}.meta")
    for value in (asset.get("textures") or {}).values():
        paths.append(f"{value}.meta")
    return paths


def validate_meta(path_rel: str, expected_importer: str) -> list[str]:
    meta_rel = f"{path_rel}.meta"
    meta_path = REPO / meta_rel
    if not meta_path.exists():
        return [f"missing Unity meta: {meta_rel}"]
    try:
        text = meta_path.read_text(encoding="utf-8", errors="replace")
    except Exception as error:
        return [f"cannot read Unity meta {meta_rel}: {error}"]
    errors = []
    if not re.search(r"^guid:\s*[0-9a-f]{32}\s*$", text, re.MULTILINE):
        errors.append(f"Unity meta missing 32-char guid: {meta_rel}")
    if f"{expected_importer}:" not in text:
        errors.append(f"Unity meta importer mismatch for {meta_rel}; expected {expected_importer}")
    return errors


def validate_meta_guid_uniqueness(meta_paths: list[str]) -> list[str]:
    errors = []
    seen = {}
    for meta_rel in sorted(set(meta_paths)):
        meta_path = REPO / meta_rel
        if not meta_path.exists():
            continue
        text = meta_path.read_text(encoding="utf-8", errors="replace")
        match = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", text, re.MULTILINE)
        if not match:
            continue
        guid = match.group(1)
        previous = seen.get(guid)
        if previous and previous != meta_rel:
            errors.append(f"Unity meta GUID collision: {guid} used by {previous} and {meta_rel}")
        seen[guid] = meta_rel
    return errors


def build_retry_queue(failed: list[dict]) -> list[dict]:
    items = []
    for failure in failed:
        if failure.get("kind") == "coverage":
            items.append({
                "asset_id": failure["asset_id"],
                "contract_name": failure["contract_name"],
                "category": failure["category"],
                "retry_count": 0,
                "reason": failure["errors"],
                "action": "generate_missing_category_assets",
            })
            continue
        errors = failure.get("errors", [])
        if any("missing lod" in error.lower() or "missing" in error.lower() and "fbx" in error.lower() for error in errors):
            action = "export_fbx_lod0_lod1_lod2_from_source_model"
        elif any("byte-identical" in error for error in errors):
            action = "generate_reduced_lod1_lod2_without_touching_passed_assets"
        else:
            action = "repair_contract_spec_failure"
        items.append({
            "asset_id": failure.get("asset_id"),
            "contract_name": failure.get("contract_name"),
            "category": failure.get("category"),
            "kind": failure.get("kind"),
            "retry_count": 0,
            "reason": errors,
            "action": action,
        })
    return items


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def read_fbx_triangle_count(path: Path) -> int | None:
    try:
        with path.open("r", encoding="utf-8", errors="replace") as handle:
            for _ in range(12):
                line = handle.readline()
                if not line:
                    break
                if "FOURFOLD_TRIANGLES:" in line:
                    return int(line.split("FOURFOLD_TRIANGLES:", 1)[1].strip())
    except Exception:
        return None
    return None


def fbx_has_uv0(path: Path) -> bool:
    try:
        with path.open("r", encoding="utf-8", errors="replace") as handle:
            for line in handle:
                if "LayerElementUV" in line:
                    return True
    except Exception:
        return False
    return False


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO).as_posix()


if __name__ == "__main__":
    main()
