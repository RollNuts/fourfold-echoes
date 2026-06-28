#!/usr/bin/env python3
"""Generate deterministic prototype title, rest room, and result stinger BGM."""

from __future__ import annotations

import csv
import json
import shutil
from dataclasses import dataclass
from pathlib import Path

import numpy as np

from generate_prototype_boss_bgm import (
    REPO,
    SAMPLE_RATE,
    STEM_NAMES,
    UNITY_AUDIO_DIR,
    VARIANT,
    VERSION,
    TrackSpec,
    add_chord,
    add_note,
    add_tick,
    amp_to_db,
    band_energy_ratio,
    beats,
    build_metrics,
    convert_runtime_ogg,
    convert_with_encoder,
    db_to_amp,
    docs_dir_for,
    midi_freq,
    normalization_scale,
    package_dir_for,
    sample_at,
    write_unity_audio_meta,
    write_wav24,
)


@dataclass(frozen=True)
class CueSpec:
    track: TrackSpec
    looping: bool
    role: str
    fade_note: str
    sfx_space_note: str
    melody_note: str


CUES = (
    CueSpec(
        track=TrackSpec("audio.music.bgm_title", "Fourfold Echo", "Title", "title", "Inviting", 78, 2, 12, 0x46455F5431, -24.6, -6.5),
        looping=True,
        role="title menu identity loop",
        fade_note="No baked loop fade. Optional engine fade-in 250-500 ms on title entry.",
        sfx_space_note="Menu select and confirm SFX should remain clearly above the music.",
        melody_note="Main motif appears briefly, then leaves space for title UI movement.",
    ),
    CueSpec(
        track=TrackSpec("audio.music.bgm_rest_room", "Hearth Between Folds", "RestRoom", "rest room", "Warm", 72, 1, 12, 0x46455F5231, -25.4, -7.0),
        looping=True,
        role="safe room and recovery loop",
        fade_note="No baked loop fade. Optional engine crossfade 250-500 ms from exploration music.",
        sfx_space_note="Keep save, heal, menu, and pickup SFX in front; no dense rhythmic layer.",
        melody_note="Lead is a sparse reassurance response, not a foreground tune.",
    ),
    CueSpec(
        track=TrackSpec("audio.music.bgm_victory_stinger", "Thread Rejoined", "VictoryStinger", "victory stinger", "Resolute", 120, 0, 2, 0x46455F5631, -22.8, -5.8),
        looping=False,
        role="victory result one-shot",
        fade_note="One-shot tail is baked. Do not loop; allow tail to finish or duck under result UI.",
        sfx_space_note="No impact-heavy transient; victory SFX and UI confirms can sit above it.",
        melody_note="Lead resolves upward once, then releases.",
    ),
    CueSpec(
        track=TrackSpec("audio.music.bgm_defeat_stinger", "Fold Unmade", "DefeatStinger", "defeat stinger", "Somber", 84, 0, 2, 0x46455F4431, -24.2, -7.0),
        looping=False,
        role="defeat result one-shot",
        fade_note="One-shot tail is baked. Do not loop; allow retry/menu SFX to remain readable.",
        sfx_space_note="No low boom or hit replacement; player damage and retry UI SFX outrank it.",
        melody_note="Lead falls in two short phrases, then gets out of the way.",
    ),
)


def main() -> None:
    UNITY_AUDIO_DIR.mkdir(parents=True, exist_ok=True)
    for cue in CUES:
        render_cue(cue)


def render_cue(cue: CueSpec) -> None:
    spec = cue.track
    package_dir = package_dir_for(spec)
    stems_dir = package_dir / "Stems"
    docs_dir = docs_dir_for(spec)
    package_dir.mkdir(parents=True, exist_ok=True)
    stems_dir.mkdir(parents=True, exist_ok=True)
    docs_dir.mkdir(parents=True, exist_ok=True)

    stems = {name: np.zeros((spec.total_samples, 2), dtype=np.float32) for name in STEM_NAMES}
    rng = np.random.default_rng(spec.seed)
    {
        "Title": render_title,
        "RestRoom": render_rest_room,
        "VictoryStinger": render_victory_stinger,
        "DefeatStinger": render_defeat_stinger,
    }[spec.scene](spec, stems, rng)

    raw_master = sum(stems.values())
    scale = normalization_scale(spec, raw_master)
    if not cue.looping:
        scale = min(scale, db_to_amp(spec.peak_ceiling_dbfs) / max(float(np.max(np.abs(raw_master))), 1.0e-12))
    stems = {name: data * scale for name, data in stems.items()}
    master = sum(stems.values())

    master_path = package_dir / f"{spec.base_name}.wav"
    write_wav24(master_path, master)
    stem_paths = write_stems(spec, stems, stems_dir)

    if cue.looping:
        intro = master[: spec.loop_start_sample]
        loop = master[spec.loop_start_sample : spec.loop_end_sample]
        intro_wav_path = package_dir / f"{spec.intro_name}.wav"
        loop_wav_path = package_dir / f"{spec.loop_name}.wav"
        write_wav24(intro_wav_path, intro)
        write_wav24(loop_wav_path, loop)
        runtime_paths = convert_runtime_ogg(spec, intro_wav_path, loop_wav_path, package_dir)
        metrics = build_metrics(spec, master, loop, stems, runtime_paths)
        write_unity_audio_meta(runtime_paths["intro"])
        write_unity_audio_meta(runtime_paths["loop"])
    else:
        runtime_path = convert_one_shot_ogg(spec, master_path, package_dir)
        runtime_paths = {"one_shot": runtime_path}
        metrics = build_one_shot_metrics(spec, master, stems, runtime_path)
        write_unity_audio_meta(runtime_path)

    write_asset_request(cue, package_dir, docs_dir, runtime_paths)
    write_cue_sheet(cue, metrics, stem_paths, runtime_paths, package_dir, docs_dir)
    write_asset_json(cue, metrics, stem_paths, runtime_paths, package_dir, docs_dir)
    write_qc_result(cue, metrics, package_dir, docs_dir)

    print(f"[bgm] wrote {spec.base_name}")
    print(f"[bgm] peak_dbfs={metrics['peak_dbfs']:.2f} rms_dbfs={metrics['rms_dbfs']:.2f}")


def render_title(spec: TrackSpec, stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    add_chord(spec, stems["harmony"], 0, 1.20, [50, 57, 62, 66], 0.030, "dark_pad", -0.08)
    add_chord(spec, stems["harmony"], 1, 0.80, [52, 59, 64, 69], 0.027, "dark_pad", 0.08)
    add_lead_cells(spec, stems["lead"], 1, [(0.35, 69, 0.22), (1.20, 73, 0.20), (2.15, 76, 0.26)])
    progression = [(0, 1.35, [50, 57, 62, 66]), (2, 1.15, [47, 54, 59, 64]), (4, 1.35, [45, 52, 57, 61]), (6, 1.15, [48, 55, 60, 64]), (8, 1.20, [50, 57, 62, 66]), (10, 0.72, [52, 57, 61, 66])]
    for loop_bar, length, notes in progression:
        add_chord(spec, stems["harmony"], spec.intro_bars + loop_bar, length, notes, 0.027, "dark_pad", -0.06 if loop_bar % 4 == 0 else 0.06)
    for loop_bar in range(spec.loop_bars - 1):
        absolute_bar = spec.intro_bars + loop_bar
        notes = ([50, 57], [52, 59], [45, 52], [48, 55])[(loop_bar // 2) % 4]
        for index, beat_offset in enumerate((0.65, 2.15)):
            add_note(spec, stems["rhythm"], sample_at(spec, absolute_bar, beat_offset), beats(spec, 0.16), midi_freq(notes[index] + 12), 0.014, -0.22 if index == 0 else 0.22, "muted_pluck")
        if loop_bar in (3, 7):
            add_lead_cells(spec, stems["lead"], absolute_bar, [(0.35, 69, 0.18), (1.15, 73, 0.18), (2.05, 76, 0.20)])
        if loop_bar in (5, 9):
            add_tick(spec, stems["percussion"], sample_at(spec, absolute_bar, 3.25), 0.010, 0.16, rng)


def render_rest_room(spec: TrackSpec, stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    add_chord(spec, stems["harmony"], 0, 0.75, [48, 55, 60, 64], 0.024, "dark_pad", 0.0)
    progression = [(0, 1.45, [48, 55, 60, 64]), (2, 1.20, [50, 57, 62, 65]), (4, 1.45, [45, 52, 57, 60]), (6, 1.20, [47, 54, 59, 62]), (8, 1.35, [48, 55, 60, 64]), (10, 0.70, [43, 50, 55, 60])]
    for loop_bar, length, notes in progression:
        add_chord(spec, stems["harmony"], spec.intro_bars + loop_bar, length, notes, 0.023, "dark_pad", -0.05 if loop_bar % 4 == 0 else 0.05)
    for loop_bar in range(spec.loop_bars - 1):
        absolute_bar = spec.intro_bars + loop_bar
        notes = ([48, 55], [50, 57], [45, 52], [47, 54])[(loop_bar // 2) % 4]
        if loop_bar % 2 == 0:
            add_note(spec, stems["rhythm"], sample_at(spec, absolute_bar, 1.50), beats(spec, 0.20), midi_freq(notes[0] + 12), 0.010, -0.18, "muted_pluck")
        if loop_bar in (2, 6, 9):
            add_lead_cells(spec, stems["lead"], absolute_bar, [(0.40, 64, 0.18), (1.25, 67, 0.20)])
        if loop_bar in (4, 8):
            add_tick(spec, stems["percussion"], sample_at(spec, absolute_bar, 3.40), 0.006, 0.20, rng)


def render_victory_stinger(spec: TrackSpec, stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    add_chord(spec, stems["harmony"], 0, 0.75, [50, 57, 62, 66], 0.030, "dark_pad", -0.04)
    add_chord(spec, stems["harmony"], 1, 0.65, [55, 62, 66, 71], 0.034, "dark_pad", 0.05)
    for beat_offset, note in ((0.25, 62), (0.75, 66), (1.25, 69), (2.00, 74), (4.30, 78)):
        add_note(spec, stems["lead"], sample_at(spec, 0, beat_offset), beats(spec, 0.18), midi_freq(note), 0.020, 0.12, "warning_bell")
    add_tick(spec, stems["percussion"], sample_at(spec, 0, 3.60), 0.009, -0.18, rng)


def render_defeat_stinger(spec: TrackSpec, stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    add_chord(spec, stems["harmony"], 0, 0.90, [45, 52, 57, 60], 0.024, "dark_pad", -0.05)
    add_chord(spec, stems["harmony"], 1, 0.75, [43, 50, 55, 59], 0.022, "dark_pad", 0.05)
    for beat_offset, note in ((0.35, 67), (1.15, 64), (2.10, 60), (4.35, 55)):
        add_note(spec, stems["lead"], sample_at(spec, 0, beat_offset), beats(spec, 0.22), midi_freq(note), 0.016, -0.12, "warning_bell")
    add_tick(spec, stems["percussion"], sample_at(spec, 1, 2.90), 0.005, 0.15, rng)


def add_lead_cells(spec: TrackSpec, buffer: np.ndarray, bar: int, cells: list[tuple[float, int, float]]) -> None:
    for beat_offset, note, length in cells:
        add_note(spec, buffer, sample_at(spec, bar, beat_offset), beats(spec, length), midi_freq(note), 0.014, 0.12 if note >= 69 else -0.12, "warning_bell")


def write_stems(spec: TrackSpec, stems: dict[str, np.ndarray], stems_dir: Path) -> dict[str, Path]:
    stem_paths = {}
    for stem_name, audio in stems.items():
        path = stems_dir / f"BGM_{spec.scene}_{spec.mood}_{VARIANT}{stem_name.capitalize()}_{spec.bpm:03d}_v{VERSION}.wav"
        write_wav24(path, audio)
        stem_paths[stem_name] = path
    return stem_paths


def convert_one_shot_ogg(spec: TrackSpec, source_wav: Path, package_dir: Path) -> Path:
    package_ogg = package_dir / f"{spec.base_name}.ogg"
    convert_with_encoder(source_wav, package_ogg)
    runtime_path = UNITY_AUDIO_DIR / package_ogg.name
    shutil.copy2(package_ogg, runtime_path)
    return runtime_path


def build_one_shot_metrics(spec: TrackSpec, master: np.ndarray, stems: dict[str, np.ndarray], runtime_path: Path) -> dict[str, object]:
    stem_metrics = {
        name: {
            "peak_dbfs": amp_to_db(float(np.max(np.abs(data)))),
            "rms_dbfs": amp_to_db(float(np.sqrt(np.mean(np.square(data))))),
        }
        for name, data in stems.items()
    }
    peak = float(np.max(np.abs(master)))
    rms = float(np.sqrt(np.mean(np.square(master))))
    return {
        "sample_rate": SAMPLE_RATE,
        "bit_depth": 24,
        "channels": 2,
        "bpm": spec.bpm,
        "duration_seconds": spec.total_samples / SAMPLE_RATE,
        "total_samples": spec.total_samples,
        "looping": False,
        "one_shot_start_sample": 0,
        "one_shot_end_sample": spec.total_samples,
        "one_shot_length_seconds": spec.total_samples / SAMPLE_RATE,
        "peak_dbfs": amp_to_db(peak),
        "rms_dbfs": amp_to_db(rms),
        "tail_peak": float(np.max(np.abs(master[-1024:]))),
        "low_energy_ratio_20_160hz": band_energy_ratio(master, 20.0, 160.0),
        "dialogue_band_ratio_1_4khz": band_energy_ratio(master, 1000.0, 4000.0),
        "runtime_bytes": {"one_shot": runtime_path.stat().st_size},
        "stem_metrics": stem_metrics,
    }


def write_asset_request(cue: CueSpec, package_dir: Path, docs_dir: Path, runtime_paths: dict[str, Path]) -> None:
    spec = cue.track
    loop_requirement = looping_request(spec) if cue.looping else one_shot_request(spec, runtime_paths["one_shot"])
    text = f"""asset_id: {spec.asset_id}
title: {spec.title}
scene_type: {spec.scene_type}
mood: {spec.mood.lower()}
bpm_target: {spec.bpm}
loop_requirement:
{loop_requirement}
stems_requirement:
  - rhythm
  - harmony
  - lead
  - percussion
target_platforms:
  - Steam PC
  - future console portable
source_strategy: repository-authored deterministic procedural synthesis
license: repository-authored
status: generated_pilot
notes:
  - {cue.sfx_space_note}
  - {cue.melody_note}
  - {cue.fade_note}
"""
    write_text_to_outputs(package_dir, docs_dir, "asset_request.yaml", text)


def looping_request(spec: TrackSpec) -> str:
    return f"""  intro_bars: {spec.intro_bars}
  loop_bars: {spec.loop_bars}
  seamless_loop: true
  full_master_loop_start_sample: {spec.loop_start_sample}
  full_master_loop_end_sample: {spec.loop_end_sample}
  runtime_intro_clip: Assets/Audio/Generated/{spec.intro_name}.ogg
  runtime_loop_clip: Assets/Audio/Generated/{spec.loop_name}.ogg"""


def one_shot_request(spec: TrackSpec, runtime_path: Path) -> str:
    return f"""  intro_bars: 0
  loop_bars: 0
  seamless_loop: false
  one_shot_start_sample: 0
  one_shot_end_sample: {spec.total_samples}
  runtime_clip: Assets/Audio/Generated/{runtime_path.name}"""


def write_cue_sheet(cue: CueSpec, metrics: dict[str, object], stem_paths: dict[str, Path], runtime_paths: dict[str, Path], package_dir: Path, docs_dir: Path) -> None:
    spec = cue.track
    row = {
        "cue_id": spec.asset_id,
        "title": spec.title,
        "scene_type": spec.scene_type,
        "mood": spec.mood.lower(),
        "bpm": spec.bpm,
        "sample_rate": SAMPLE_RATE,
        "master_file": f"{spec.base_name}.wav",
        "runtime_file": runtime_paths.get("one_shot", Path("")).name,
        "runtime_intro_file": runtime_paths.get("intro", Path("")).name,
        "runtime_loop_file": runtime_paths.get("loop", Path("")).name,
        "intro_end_sample": spec.loop_start_sample if cue.looping else 0,
        "loop_start_sample": spec.loop_start_sample if cue.looping else "",
        "loop_end_sample": spec.loop_end_sample if cue.looping else "",
        "loop_length_samples": spec.loop_length_samples if cue.looping else "",
        "one_shot_end_sample": "" if cue.looping else spec.total_samples,
        "looping": str(cue.looping).lower(),
        "fade_note": cue.fade_note,
        "loudness_note": f"Peak {metrics['peak_dbfs']:.2f} dBFS, RMS {metrics['rms_dbfs']:.2f} dBFS.",
        "usage_note": usage_note(cue),
    }
    for destination in (package_dir / f"{spec.base_name}_cue_sheet.csv", docs_dir / "cue_sheet.csv"):
        with destination.open("w", encoding="utf-8", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=list(row.keys()), lineterminator="\n")
            writer.writeheader()
            writer.writerow(row)
    write_stem_manifest(spec, stem_paths, package_dir, docs_dir)


def write_stem_manifest(spec: TrackSpec, stem_paths: dict[str, Path], package_dir: Path, docs_dir: Path) -> None:
    roles = {
        "rhythm": "soft pulse or spacing layer; never masks UI/combat attacks",
        "harmony": "main bed with restrained low root energy",
        "lead": "short motif only when the cue needs identity",
        "percussion": "small ticks only; no impact replacement",
    }
    rows = [{"stem": stem, "path": str(path.relative_to(package_dir)), "role": roles[stem]} for stem, path in stem_paths.items()]
    for destination in (package_dir / f"{spec.base_name}_stems.csv", docs_dir / "stems.csv"):
        with destination.open("w", encoding="utf-8", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=["stem", "path", "role"], lineterminator="\n")
            writer.writeheader()
            writer.writerows(rows)


def write_asset_json(cue: CueSpec, metrics: dict[str, object], stem_paths: dict[str, Path], runtime_paths: dict[str, Path], package_dir: Path, docs_dir: Path) -> None:
    spec = cue.track
    paths = {
        "master_wav": f"{spec.base_name}.wav",
        "cue_sheet": "cue_sheet.csv",
        "asset_request": "asset_request.yaml",
        "qc_result": "qc_result.json",
        "stems": {stem: str(path.relative_to(package_dir)) for stem, path in stem_paths.items()},
    }
    if cue.looping:
        paths["runtime_intro_ogg"] = str(runtime_paths["intro"].relative_to(REPO))
        paths["runtime_loop_ogg"] = str(runtime_paths["loop"].relative_to(REPO))
    else:
        paths["runtime_ogg"] = str(runtime_paths["one_shot"].relative_to(REPO))
    data = {
        "asset_id": spec.asset_id,
        "title": spec.title,
        "scene": spec.scene,
        "scene_type": spec.scene_type,
        "mood": spec.mood,
        "variant": VARIANT,
        "bpm": spec.bpm,
        "version": VERSION,
        "status": "generated_pilot",
        "source_strategy": "repository-authored deterministic procedural synthesis",
        "license": "repository-authored",
        "target_platforms": ["Steam PC", "future console portable"],
        "library_root_relative": f"Runtime/Audio/BGM/{spec.base_name}",
        "paths": paths,
        "loop": loop_metadata(cue, metrics),
        "mix_notes": {
            "loudness_note": f"Peak {metrics['peak_dbfs']:.2f} dBFS, RMS {metrics['rms_dbfs']:.2f} dBFS.",
            "fade_note": cue.fade_note,
            "sfx_space_note": cue.sfx_space_note,
            "low_end_note": "No sub/kick layer; root notes are gain-limited and leave room for SFX.",
            "melody_note": cue.melody_note,
        },
        "stems": [
            {
                "name": stem,
                "file": str(path.relative_to(package_dir)),
                "peak_dbfs": metrics["stem_metrics"][stem]["peak_dbfs"],
                "rms_dbfs": metrics["stem_metrics"][stem]["rms_dbfs"],
            }
            for stem, path in stem_paths.items()
        ],
        "planned_companion_cues": companion_cues(),
    }
    write_text_to_outputs(package_dir, docs_dir, "asset.json", json.dumps(data, indent=2) + "\n")


def loop_metadata(cue: CueSpec, metrics: dict[str, object]) -> dict[str, object]:
    spec = cue.track
    if cue.looping:
        return {
            "looping": True,
            "full_master_start_sample": spec.loop_start_sample,
            "full_master_end_sample": spec.loop_end_sample,
            "full_master_start_seconds": metrics["loop_start_seconds"],
            "full_master_end_seconds": metrics["loop_end_seconds"],
            "loop_clip_start_sample": 0,
            "loop_clip_end_sample": spec.loop_length_samples,
            "loop_clip_length_seconds": metrics["loop_length_seconds"],
            "intro_bars": spec.intro_bars,
            "loop_bars": spec.loop_bars,
            "playback_instruction": usage_note(cue),
        }
    return {
        "looping": False,
        "one_shot_start_sample": 0,
        "one_shot_end_sample": spec.total_samples,
        "one_shot_length_seconds": metrics["one_shot_length_seconds"],
        "playback_instruction": usage_note(cue),
    }


def write_qc_result(cue: CueSpec, metrics: dict[str, object], package_dir: Path, docs_dir: Path) -> None:
    spec = cue.track
    checks = [
        {"name": "low_end_control", "status": "pass" if metrics["low_energy_ratio_20_160hz"] < 0.24 else "review", "detail": f"20-160 Hz energy ratio {metrics['low_energy_ratio_20_160hz']:.4f}"},
        {"name": "dialogue_band_control", "status": "pass" if metrics["dialogue_band_ratio_1_4khz"] < 0.35 else "review", "detail": f"1-4 kHz energy ratio {metrics['dialogue_band_ratio_1_4khz']:.4f}"},
        {"name": "headroom", "status": "pass" if metrics["peak_dbfs"] <= -4.5 else "review", "detail": f"peak {metrics['peak_dbfs']:.2f} dBFS"},
        {"name": "sfx_space", "status": "pass", "detail": cue.sfx_space_note},
    ]
    if cue.looping:
        checks = [
            {"name": "loop_point_recorded", "status": "pass", "detail": f"loop_start_sample={spec.loop_start_sample}, loop_end_sample={spec.loop_end_sample}"},
            {"name": "loop_boundary_click_risk", "status": "pass" if metrics["loop_boundary_delta_peak"] < 0.012 else "review", "detail": f"loop clip first/last sample delta {metrics['loop_boundary_delta_peak']:.6f}"},
            {"name": "loop_tail_release", "status": "pass" if metrics["loop_tail_peak"] < 0.030 else "review", "detail": f"last 1024 sample peak {metrics['loop_tail_peak']:.6f}"},
        ] + checks
    else:
        checks = [
            {"name": "one_shot_duration_recorded", "status": "pass", "detail": f"one_shot_end_sample={spec.total_samples}"},
            {"name": "one_shot_tail_release", "status": "pass" if metrics["tail_peak"] < 0.030 else "review", "detail": f"last 1024 sample peak {metrics['tail_peak']:.6f}"},
        ] + checks
    result = {
        "asset_id": spec.asset_id,
        "qc_status": "pass_with_notes" if all(check["status"] == "pass" for check in checks) else "review",
        "checks": checks,
        "metrics": metrics,
        "notes": [
            "Procedural pilot only; requires listening review in scene before Steam capture.",
            "Runtime loops are split into intro and loop clips; stingers are one-shot runtime clips.",
        ],
    }
    write_text_to_outputs(package_dir, docs_dir, "qc_result.json", json.dumps(result, indent=2) + "\n")


def companion_cues() -> list[dict[str, str]]:
    return [
        {"structure": "title", "status": "generated_pilot", "asset_id": "audio.music.bgm_title"},
        {"structure": "field exploration", "status": "generated_pilot", "asset_id": "audio.music.bgm_region01"},
        {"structure": "battle", "status": "generated_pilot", "asset_id": "audio.music.bgm_normal_combat"},
        {"structure": "boss phase1", "status": "generated_pilot", "asset_id": "audio.music.bgm_boss"},
        {"structure": "boss phase2", "status": "generated_pilot", "asset_id": "audio.music.bgm_boss_phase2"},
        {"structure": "rest room", "status": "generated_pilot", "asset_id": "audio.music.bgm_rest_room"},
        {"structure": "victory stinger", "status": "generated_pilot", "asset_id": "audio.music.bgm_victory_stinger"},
        {"structure": "defeat stinger", "status": "generated_pilot", "asset_id": "audio.music.bgm_defeat_stinger"},
    ]


def usage_note(cue: CueSpec) -> str:
    spec = cue.track
    if cue.looping:
        return f"Play {spec.intro_name}.ogg once, then loop {spec.loop_name}.ogg from sample 0."
    return f"Play {spec.base_name}.ogg once. Do not loop."


def write_text_to_outputs(package_dir: Path, docs_dir: Path, filename: str, text: str) -> None:
    (package_dir / filename).write_text(text, encoding="utf-8")
    (docs_dir / filename).write_text(text, encoding="utf-8")


if __name__ == "__main__":
    main()
