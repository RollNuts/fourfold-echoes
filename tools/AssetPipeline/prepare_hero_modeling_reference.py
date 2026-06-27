#!/usr/bin/env python3
"""Prepare the actual modeling reference for FE_CHAR_PLAYER_Hero_01.

This is intentionally separate from model generation. The concept crop and
brief are the contract the model must match before any costume mesh work starts.
"""

from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


REPO = Path(__file__).resolve().parents[2]
SOURCE_SHEET = REPO / "artifacts" / "Concepts" / "PopDeformedRework" / "Batch_20260627" / "FE_POP_REWORK_BATCH_01_SignatureHeroLead_16Thumbs.png"
MANNEQUIN_TURNAROUND = REPO / "artifacts" / "Previews" / "ProductionModelPack" / "FE_CHAR_TEMPLATE_ChibiMannequin_01_Turnaround.png"
REJECTED_PREVIEW = REPO / "artifacts" / "Previews" / "ProductionModelPack" / "FE_CHAR_PLAYER_Hero_01.png"
OUT_DIR = REPO / "artifacts" / "References" / "Characters" / "FE_CHAR_PLAYER_Hero_01"
CONCEPT_CROP = OUT_DIR / "FE_CHAR_PLAYER_Hero_01_concept_top_left.png"
REFERENCE_BOARD = OUT_DIR / "FE_CHAR_PLAYER_Hero_01_reference_board.png"
BRIEF = OUT_DIR / "FE_CHAR_PLAYER_Hero_01_ModelingBrief.md"


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    sheet = Image.open(SOURCE_SHEET).convert("RGB")
    cell_w, cell_h = sheet.width // 4, sheet.height // 4
    concept = sheet.crop((0, 0, cell_w, cell_h))
    concept.save(CONCEPT_CROP)
    make_reference_board(concept)
    write_brief()

    print(f"Concept crop: {rel(CONCEPT_CROP)}")
    print(f"Reference board: {rel(REFERENCE_BOARD)}")
    print(f"Modeling brief: {rel(BRIEF)}")


def make_reference_board(concept: Image.Image) -> None:
    board = Image.new("RGB", (1600, 900), (18, 21, 27))
    draw = ImageDraw.Draw(board)

    concept_large = concept.resize((768, 512), Image.Resampling.LANCZOS)
    board.paste(concept_large, (24, 70))
    draw.text((24, 28), "TARGET CONCEPT - match this silhouette, not just color tokens", fill=(235, 238, 230))

    if MANNEQUIN_TURNAROUND.exists():
        mannequin = Image.open(MANNEQUIN_TURNAROUND).convert("RGB")
        mannequin.thumbnail((740, 270), Image.Resampling.LANCZOS)
        board.paste(mannequin, (830, 90))
        draw.text((830, 58), "BODY TEMPLATE - playable/NPC only", fill=(235, 238, 230))

    if REJECTED_PREVIEW.exists():
        rejected = Image.open(REJECTED_PREVIEW).convert("RGB")
        rejected.thumbnail((360, 360), Image.Resampling.LANCZOS)
        board.paste(rejected, (830, 430))
        draw.text((830, 398), "REJECTED - do not continue from this shape", fill=(255, 150, 120))

    checklist = [
        "Must read at thumbnail: brown spiky hair + blue/orange jacket + backpack + extended cube tool.",
        "Use the mannequin only as inner proportion, then dress it with concept-specific volumes.",
        "Large head, short torso, chunky hands/boots, low confident stance.",
        "No plinth, no crown hair, no tall cylinder torso, no loose primitive chain tool.",
        "Next 3D pass must ship with same-angle preview against this board.",
    ]
    y = 430
    for line in checklist:
        for wrapped in wrap_text(f"- {line}", 52):
            draw.text((1220, y), wrapped, fill=(214, 222, 218))
            y += 24
        y += 14

    board.save(REFERENCE_BOARD)


def wrap_text(text: str, max_chars: int) -> list[str]:
    words = text.split()
    lines: list[str] = []
    current = ""
    for word in words:
        candidate = f"{current} {word}".strip()
        if current and len(candidate) > max_chars:
            lines.append(current)
            current = f"  {word}" if text.startswith("- ") else word
        else:
            current = candidate
    if current:
        lines.append(current)
    return lines


def write_brief() -> None:
    BRIEF.write_text(
        """# FE_CHAR_PLAYER_Hero_01 Modeling Brief

## Purpose

The concept art exists to be the modeling contract. The prior Hero_01 model is rejected because it matched loose element names, not the drawing.

## Source

- Target concept sheet: `artifacts/Concepts/PopDeformedRework/Batch_20260627/FE_POP_REWORK_BATCH_01_SignatureHeroLead_16Thumbs.png`
- Target candidate: top-left character only
- Dedicated crop: `artifacts/References/Characters/FE_CHAR_PLAYER_Hero_01/FE_CHAR_PLAYER_Hero_01_concept_top_left.png`
- Reference board: `artifacts/References/Characters/FE_CHAR_PLAYER_Hero_01/FE_CHAR_PLAYER_Hero_01_reference_board.png`
- Body template: `FE_CHAR_TEMPLATE_ChibiMannequin_01`

## Hard Requirements

- Use the mannequin only as the inner playable/NPC body proportion.
- Do not reuse the rejected Hero_01 silhouette.
- Match the target concept's large rounded head, layered brown spiky hair, short torso, chunky hands, chunky boots, and low confident stance.
- Outfit must read as blue jacket over dark undersuit with orange shoulder/cuff/hem accents.
- Backpack must be a multi-part gray/white block pack with orange modules.
- Hand tool must read as a black/orange emitter with cyan dotted beam and cyan cube.
- The model must be evaluated with a same-angle preview against the reference board before acceptance.

## Rejection Conditions

- Looks like a different character with only matching colors.
- Hair reads as a crown, cap, helmet, or block slabs.
- Body reads as a tall cylinder or generic primitive stack.
- Backpack is a plain box.
- Tool is a loose chain of primitives.
- Face lacks large readable eyes and brows.
- A display plinth is needed to make the silhouette work.
""",
        encoding="utf-8",
    )


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO).as_posix()


if __name__ == "__main__":
    main()
