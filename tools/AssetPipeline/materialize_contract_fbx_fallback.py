#!/usr/bin/env python3
"""Materialize contract FBX files from existing same-basename FBX sources.

This is a fallback when Blender conversion is unavailable. It preserves all
generated files and lets validation mark duplicate/non-reduced LODs as failed.
"""

from __future__ import annotations

import json
import shutil
from pathlib import Path


REPO = Path(__file__).resolve().parents[2]
ASSET_MANIFEST = REPO / "asset_manifest.json"
REPORT = REPO / "artifacts" / "Reports" / "contract-fbx-fallback-materialize.json"


def main() -> None:
    manifest = json.loads(ASSET_MANIFEST.read_text(encoding="utf-8"))
    copied = []
    missing = []
    for asset in manifest.get("assets", []):
        if asset.get("kind") != "3d":
            continue
        source = REPO / asset["source_model"]
        candidate = source.with_suffix(".fbx")
        if not candidate.exists():
            missing.append({
                "asset": asset["contract_name"],
                "source_model": asset["source_model"],
                "expected_existing_fbx": candidate.resolve().relative_to(REPO).as_posix(),
            })
            continue
        for lod_key, output_rel in asset["final_files"].items():
            output = REPO / output_rel
            output.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(candidate, output)
            copied.append({
                "asset": asset["contract_name"],
                "lod": lod_key,
                "source_fbx": candidate.resolve().relative_to(REPO).as_posix(),
                "output": output_rel,
            })
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps({
        "copied_lod_files": len(copied),
        "source_fbx_assets": len({item["asset"] for item in copied}),
        "missing_source_fbx_assets": len(missing),
        "missing_source_fbx": missing,
    }, indent=2) + "\n", encoding="utf-8")
    print(f"Fallback FBX copied: {len(copied)} files")
    print(f"Assets missing existing FBX: {len(missing)}")
    print(f"Report: {REPORT.resolve().relative_to(REPO).as_posix()}")


if __name__ == "__main__":
    main()
