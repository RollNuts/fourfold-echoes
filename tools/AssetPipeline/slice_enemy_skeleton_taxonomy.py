#!/usr/bin/env python3
"""Slice and classify the first enemy skeleton taxonomy concept sheet."""

from __future__ import annotations

import json
from pathlib import Path

from PIL import Image


REPO = Path(__file__).resolve().parents[2]
SHEET = REPO / "artifacts" / "Concepts" / "EnemySkeletonTaxonomy" / "Batch_20260627" / "FE_ENEMY_SKELETON_TAXONOMY_16Thumbs_v001.png"
OUT_DIR = SHEET.parent / "Crops_v001"
REPORT_JSON = REPO / "artifacts" / "Reports" / "enemy-skeleton-taxonomy-v001.json"
REPORT_MD = REPO / "artifacts" / "Reports" / "enemy-skeleton-taxonomy-v001.md"

SLOTS = [
    ("ESK-01", "Small biped", "accept_template_seed", "Good base for fodder. Strip final panel details before mannequin modeling."),
    ("ESK-02", "Heavy biped", "accept_template_seed", "Good blocker/brute mass. Needs neutral joint landmarks and less finished armor."),
    ("ESK-03", "Quadruped beast", "accept_template_seed", "Strong charger base. Remove crystal color identity for neutral template."),
    ("ESK-04", "Slime/blob", "accept_template_seed", "Good non-biped body category. Core socket is readable."),
    ("ESK-05", "Floating caster", "accept_template_seed", "Good ranged/support hover skeleton. Keep side shields as optional parts."),
    ("ESK-06", "Winged flyer", "revise_before_template", "Useful silhouette, but too close to familiar bat/imp defaults. Needs more original head language."),
    ("ESK-07", "Insect/arachnid", "accept_template_seed", "Good crawler template. Simplify shell into neutral body mannequin."),
    ("ESK-08", "Serpent/long body", "accept_template_seed", "Useful segmented line-hazard body. Head should become swappable."),
    ("ESK-09", "Dragon/wyvern", "revise_before_template", "Useful dragon category, but the head/wing read is too familiar. Must be reworked before modeling."),
    ("ESK-10", "Golem/mech", "accept_template_seed", "Good slow telegraph bruiser. Works as a separate construct skeleton."),
    ("ESK-11", "Plant/root", "accept_template_seed", "Good rooted turret/support body. Keep as plant hazard skeleton, not NPC body."),
    ("ESK-12", "Boss multi-anchor", "accept_template_seed", "Useful multi-limb socket boss seed. Needs clearer weak-point hierarchy."),
    ("ESK-13", "Crab shell", "accept_template_seed", "Good alternate low crawler. Good for shielded side-step enemy."),
    ("ESK-14", "Rolling shell", "accept_template_seed", "Good roll/charge hazard. Needs unfolded attack pose concept later."),
    ("ESK-15", "Mimic/tool construct", "accept_template_seed", "Good object-enemy skeleton. Keep because ARPG needs surprise/interact enemies."),
    ("ESK-16", "Tall support caster", "revise_before_template", "Readable support silhouette, but too humanoid/staff-like. Rework to avoid generic caster."),
]


def main() -> None:
    image = Image.open(SHEET).convert("RGB")
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    cell_w = image.width // 4
    cell_h = image.height // 4
    records = []
    for index, (category_id, name, status, note) in enumerate(SLOTS):
        row, col = divmod(index, 4)
        crop = image.crop((col * cell_w, row * cell_h, (col + 1) * cell_w, (row + 1) * cell_h))
        file_name = f"{category_id}_{slug(name)}.png"
        crop_path = OUT_DIR / file_name
        crop.save(crop_path)
        records.append({
            "slot": index + 1,
            "category_id": category_id,
            "name": name,
            "status": status,
            "crop": rel(crop_path),
            "note": note,
        })

    report = {
        "source_sheet": rel(SHEET),
        "crop_dir": rel(OUT_DIR),
        "purpose": "enemy skeleton taxonomy seeds before final monster detailing",
        "template_scope": "enemy_monster_boss_only",
        "excluded_scope": "playable_and_friendly_npc_mannequin",
        "records": records,
        "next_template_order": ["ESK-01", "ESK-03", "ESK-05", "ESK-09", "ESK-10", "ESK-04", "ESK-07", "ESK-12"],
    }
    REPORT_JSON.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    REPORT_MD.write_text(markdown(report), encoding="utf-8")

    print(f"Sliced {len(records)} enemy skeleton concepts")
    print(f"Report: {rel(REPORT_MD)}")
    print(f"Crops: {rel(OUT_DIR)}")


def markdown(report: dict) -> str:
    lines = [
        "# Enemy Skeleton Taxonomy v001",
        "",
        "## Source",
        "",
        f"- Sheet: `{report['source_sheet']}`",
        f"- Crops: `{report['crop_dir']}`",
        "- Scope: enemy, monster, miniboss, and boss skeletons only",
        "- Excluded: playable and friendly NPC mannequin",
        "",
        "## Classification",
        "",
        "| Slot | ID | Template | Status | Crop | Note |",
        "|---:|---|---|---|---|---|",
    ]
    for record in report["records"]:
        lines.append(
            f"| {record['slot']} | {record['category_id']} | {record['name']} | "
            f"{record['status']} | `{record['crop']}` | {record['note']} |"
        )
    lines.extend([
        "",
        "## Next Template Order",
        "",
    ])
    for index, category_id in enumerate(report["next_template_order"], 1):
        lines.append(f"{index}. {category_id}")
    lines.extend([
        "",
        "## Production Note",
        "",
        "These are not final enemies. Accepted slots become neutral 3D mannequins first, then receive heads, wings, horns, tails, shells, armor, weak cores, materials, and biome variants.",
    ])
    return "\n".join(lines) + "\n"


def slug(text: str) -> str:
    return text.lower().replace("/", "_").replace(" ", "_")


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO).as_posix()


if __name__ == "__main__":
    main()
