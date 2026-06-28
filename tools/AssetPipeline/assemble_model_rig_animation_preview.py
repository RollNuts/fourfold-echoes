#!/usr/bin/env python3
"""Assemble preview.gif for the model-rig animation delivery pack."""

from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image


REPO = Path(__file__).resolve().parents[2]
PACK_ROOT = REPO / "Assets" / "Art" / "ModelRigAnimation" / "Enemy" / "MeleeShardling" / "SealedLockRelic_v0.1.0"
PREVIEW_DIR = PACK_ROOT / "Previews"
FRAME_DIR = PREVIEW_DIR / "preview_frames"
PREVIEW_GIF = PREVIEW_DIR / "preview.gif"
ASSET_JSON = PACK_ROOT / "asset.json"
QC_JSON = PACK_ROOT / "qc_result.json"


def main() -> None:
    frame_paths = sorted(FRAME_DIR.glob("frame_*.png"))
    if not frame_paths:
        raise RuntimeError(f"No turntable preview frames found: {relative(FRAME_DIR)}")

    frames = [Image.open(path).convert("P", palette=adaptive_palette()) for path in frame_paths]
    PREVIEW_GIF.parent.mkdir(parents=True, exist_ok=True)
    frames[0].save(
        PREVIEW_GIF,
        save_all=True,
        append_images=frames[1:],
        duration=70,
        loop=0,
        disposal=2,
    )

    asset = load_json(ASSET_JSON)
    qc = load_json(QC_JSON)
    asset.setdefault("deliverables", {})["preview_gif"] = relative(PREVIEW_GIF)
    asset.setdefault("deliverables", {})["turntable_preview_frames"] = relative(FRAME_DIR)
    asset.setdefault("qc", {})["preview_gif_status"] = "pass"
    asset["qc"]["preview_gif"] = relative(PREVIEW_GIF)
    asset["qc"]["updated_at"] = generated_at()
    qc["preview_gif_status"] = "pass"
    qc["preview_gif"] = relative(PREVIEW_GIF)
    qc["updated_at"] = generated_at()
    qc["missing_files"] = [
        path for path in qc.get("missing_files", [])
        if path != relative(PREVIEW_GIF)
    ]
    ASSET_JSON.write_text(json.dumps(asset, indent=2) + "\n", encoding="utf-8")
    QC_JSON.write_text(json.dumps(qc, indent=2) + "\n", encoding="utf-8")

    print(f"Preview GIF: {relative(PREVIEW_GIF)}")
    print(f"Frames: {len(frame_paths)}")


def adaptive_palette():
    palette = getattr(getattr(Image, "Palette", None), "ADAPTIVE", None)
    return palette if palette is not None else Image.ADAPTIVE


def load_json(path: Path) -> dict:
    if not path.exists():
        raise FileNotFoundError(relative(path))
    return json.loads(path.read_text(encoding="utf-8"))


def generated_at() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def relative(path: Path) -> str:
    return path.resolve().relative_to(REPO.resolve()).as_posix()


if __name__ == "__main__":
    main()
