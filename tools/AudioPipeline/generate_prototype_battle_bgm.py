#!/usr/bin/env python3
"""Generate deterministic prototype normal-combat BGM for FOURFOLD ECHOES.

This produces a repository-authored battle music pilot with a separate intro,
seamless loop runtime OGGs, WAV master/stems, cue sheet, and metadata. The mix
keeps the rhythm urgent without a heavy kick or dense attack layer so gameplay
SFX remain readable.
"""

from __future__ import annotations

import csv
import hashlib
import json
import math
import shutil
import struct
import subprocess
from pathlib import Path

import numpy as np


SAMPLE_RATE = 44100
BPM = 112
BEATS_PER_BAR = 4
INTRO_BARS = 2
LOOP_BARS = 16
VERSION = "0.1.0"
SCENE = "Battle"
SCENE_TYPE = "battle"
MOOD = "Tense"
VARIANT = "A"
TITLE = "Threadline Clash"
ASSET_ID = "audio.music.bgm_normal_combat"

BASE_NAME = f"BGM_{SCENE}_{MOOD}_{VARIANT}_{BPM:03d}_v{VERSION}"
INTRO_NAME = f"BGM_{SCENE}_{MOOD}_{VARIANT}Intro_{BPM:03d}_v{VERSION}"
LOOP_NAME = f"BGM_{SCENE}_{MOOD}_{VARIANT}Loop_{BPM:03d}_v{VERSION}"

REPO = Path(__file__).resolve().parents[2]
PACKAGE_DIR = REPO / "artifacts" / "Audio" / "BGM" / "Runtime" / "Audio" / "BGM" / BASE_NAME
STEMS_DIR = PACKAGE_DIR / "Stems"
DOCS_DIR = REPO / "docs" / "Audio" / "BGM" / BASE_NAME
UNITY_AUDIO_DIR = REPO / "Assets" / "Audio" / "Generated"

SAMPLES_PER_BEAT = int(SAMPLE_RATE * 60 / BPM)
SAMPLES_PER_BAR = SAMPLES_PER_BEAT * BEATS_PER_BAR
TOTAL_BARS = INTRO_BARS + LOOP_BARS
TOTAL_SAMPLES = TOTAL_BARS * SAMPLES_PER_BAR
LOOP_START_SAMPLE = INTRO_BARS * SAMPLES_PER_BAR
LOOP_END_SAMPLE = TOTAL_SAMPLES
LOOP_LENGTH_SAMPLES = LOOP_END_SAMPLE - LOOP_START_SAMPLE

STEM_NAMES = ("rhythm", "harmony", "lead", "percussion")


def main() -> None:
    PACKAGE_DIR.mkdir(parents=True, exist_ok=True)
    STEMS_DIR.mkdir(parents=True, exist_ok=True)
    DOCS_DIR.mkdir(parents=True, exist_ok=True)
    UNITY_AUDIO_DIR.mkdir(parents=True, exist_ok=True)

    stems = {name: np.zeros((TOTAL_SAMPLES, 2), dtype=np.float32) for name in STEM_NAMES}
    rng = np.random.default_rng(0x46455F42)

    render_intro(stems, rng)
    render_loop_body(stems, rng)

    raw_master = sum(stems.values())
    scale = normalization_scale(raw_master)
    stems = {name: data * scale for name, data in stems.items()}
    master = sum(stems.values())
    intro = master[:LOOP_START_SAMPLE]
    loop = master[LOOP_START_SAMPLE:LOOP_END_SAMPLE]

    master_path = PACKAGE_DIR / f"{BASE_NAME}.wav"
    intro_wav_path = PACKAGE_DIR / f"{INTRO_NAME}.wav"
    loop_wav_path = PACKAGE_DIR / f"{LOOP_NAME}.wav"
    write_wav24(master_path, master)
    write_wav24(intro_wav_path, intro)
    write_wav24(loop_wav_path, loop)

    stem_paths: dict[str, Path] = {}
    for stem_name, audio in stems.items():
        stem_base = f"BGM_{SCENE}_{MOOD}_{VARIANT}{stem_name.capitalize()}_{BPM:03d}_v{VERSION}.wav"
        path = STEMS_DIR / stem_base
        write_wav24(path, audio)
        stem_paths[stem_name] = path

    runtime_paths = convert_runtime_ogg(intro_wav_path, loop_wav_path)
    metrics = build_metrics(master, loop, stems, runtime_paths)

    write_asset_request()
    write_cue_sheet(metrics, stem_paths, runtime_paths)
    write_asset_json(metrics, stem_paths, runtime_paths)
    write_qc_result(metrics)
    write_unity_audio_meta(runtime_paths["intro"])
    write_unity_audio_meta(runtime_paths["loop"])

    print(f"[bgm] wrote package {PACKAGE_DIR.relative_to(REPO)}")
    print(f"[bgm] wrote docs {DOCS_DIR.relative_to(REPO)}")
    for kind, path in runtime_paths.items():
        print(f"[bgm] wrote runtime_{kind} {path.relative_to(REPO)}")
    print(f"[bgm] loop_start_sample={LOOP_START_SAMPLE} loop_end_sample={LOOP_END_SAMPLE}")
    print(f"[bgm] loop_clip_samples={LOOP_LENGTH_SAMPLES}")
    print(f"[bgm] peak_dbfs={metrics['peak_dbfs']:.2f} rms_dbfs={metrics['rms_dbfs']:.2f}")


def render_intro(stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    add_chord(stems["harmony"], 0, 1.4, [50, 57, 62, 65], 0.030, "pad", -0.08)
    add_chord(stems["harmony"], 1, 0.8, [53, 57, 62, 67], 0.026, "pad", 0.08)

    for beat_offset in (0.75, 1.75, 2.75):
        add_note(
            stems["rhythm"],
            sample_at(1, beat_offset),
            beats(0.18),
            midi_freq(62),
            0.018,
            -0.22 if beat_offset < 2 else 0.22,
            "muted_pluck",
        )
    add_tick(stems["percussion"], sample_at(1, 3.45), 0.018, 0.14, rng)


def render_loop_body(stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    progression = [
        (0, 1.40, [50, 57, 62, 65]),
        (2, 1.35, [53, 57, 60, 67]),
        (4, 1.40, [55, 62, 65, 69]),
        (6, 1.20, [48, 55, 60, 64]),
        (8, 1.40, [50, 57, 62, 65]),
        (10, 1.35, [52, 57, 60, 67]),
        (12, 1.40, [55, 62, 65, 69]),
        (14, 1.05, [53, 57, 62, 65]),
    ]

    for loop_bar, length_bars, notes in progression:
        absolute_bar = INTRO_BARS + loop_bar
        add_chord(
            stems["harmony"],
            absolute_bar,
            length_bars,
            notes,
            0.028,
            "pad",
            -0.08 if loop_bar % 4 == 0 else 0.08,
        )

    for loop_bar in range(LOOP_BARS):
        absolute_bar = INTRO_BARS + loop_bar
        chord_notes = notes_for_loop_bar(loop_bar)
        offsets = (0.50, 1.25, 2.00, 2.75) if loop_bar != LOOP_BARS - 1 else (0.50, 1.25)
        for index, beat_offset in enumerate(offsets):
            note = chord_notes[(loop_bar + index + 1) % len(chord_notes)] + 12
            add_note(
                stems["rhythm"],
                sample_at(absolute_bar, beat_offset),
                beats(0.16),
                midi_freq(note),
                0.020 if beat_offset != 2.75 else 0.014,
                -0.26 if index % 2 == 0 else 0.26,
                "muted_pluck",
            )

    lead_phrases = [
        (3, [(0.25, 74, 0.18), (0.80, 77, 0.16), (1.45, 76, 0.22)]),
        (7, [(0.10, 72, 0.18), (0.80, 74, 0.18), (1.60, 69, 0.26)]),
        (11, [(0.25, 76, 0.16), (0.70, 77, 0.16), (1.25, 81, 0.22)]),
        (15, [(0.15, 69, 0.16), (0.70, 72, 0.18)]),
    ]
    for loop_bar, phrase in lead_phrases:
        absolute_bar = INTRO_BARS + loop_bar
        for beat_offset, note, length in phrase:
            add_note(
                stems["lead"],
                sample_at(absolute_bar, beat_offset),
                beats(length),
                midi_freq(note),
                0.018,
                0.16 if note >= 76 else -0.16,
                "short_bell",
            )

    for loop_bar in range(LOOP_BARS):
        absolute_bar = INTRO_BARS + loop_bar
        if loop_bar == LOOP_BARS - 1:
            beat_offsets = (1.80,)
        elif loop_bar % 4 == 3:
            beat_offsets = (1.80, 3.30)
        else:
            beat_offsets = (1.80,)
        for beat_offset in beat_offsets:
            add_tick(
                stems["percussion"],
                sample_at(absolute_bar, beat_offset),
                0.018 if beat_offset < 3 else 0.014,
                -0.18 if beat_offset < 3 else 0.18,
                rng,
            )


def notes_for_loop_bar(loop_bar: int) -> list[int]:
    regions = [
        (0, 2, [50, 57, 62, 65]),
        (2, 2, [53, 57, 60, 67]),
        (4, 2, [55, 62, 65, 69]),
        (6, 2, [48, 55, 60, 64]),
        (8, 2, [50, 57, 62, 65]),
        (10, 2, [52, 57, 60, 67]),
        (12, 2, [55, 62, 65, 69]),
        (14, 2, [53, 57, 62, 65]),
    ]
    for start, length, notes in regions:
        if start <= loop_bar < start + length:
            return notes
    return [50, 57, 62, 65]


def add_chord(
    buffer: np.ndarray,
    bar: int,
    length_bars: float,
    midi_notes: list[int],
    gain: float,
    instrument: str,
    pan: float,
) -> None:
    duration = int(length_bars * SAMPLES_PER_BAR)
    for index, note in enumerate(midi_notes):
        note_gain = gain * (0.36 if index == 0 else 0.24)
        note_pan = pan + (index - (len(midi_notes) - 1) / 2) * 0.10
        add_note(buffer, sample_at(bar, 0), duration, midi_freq(note), note_gain, note_pan, instrument)


def add_note(
    buffer: np.ndarray,
    start: int,
    duration: int,
    frequency: float,
    gain: float,
    pan: float,
    instrument: str,
) -> None:
    if duration <= 0 or start >= len(buffer):
        return
    end = min(len(buffer), start + duration)
    count = end - start
    if count <= 0:
        return

    time = np.arange(count, dtype=np.float32) / SAMPLE_RATE
    phase = 2 * np.pi * frequency * time
    if instrument == "pad":
        tremolo = 1.0 + 0.045 * np.sin(2 * np.pi * 4.0 * time)
        signal = (np.sin(phase) + 0.24 * np.sin(phase * 2.0 + 0.4)) * tremolo
        env = envelope(count, 0.10, 0.18, 0.58, 0.42)
    elif instrument == "muted_pluck":
        signal = np.sin(phase) + 0.18 * np.sin(phase * 2.0)
        env = envelope(count, 0.004, 0.035, 0.18, 0.055) * np.exp(-9.0 * time)
    elif instrument == "short_bell":
        signal = np.sin(phase) + 0.26 * np.sin(phase * 2.01) + 0.08 * np.sin(phase * 4.02)
        env = envelope(count, 0.006, 0.050, 0.16, 0.11) * np.exp(-4.2 * time)
    else:
        signal = np.sin(phase)
        env = envelope(count, 0.01, 0.1, 0.5, 0.1)

    left, right = pan_gains(pan)
    rendered = (signal * env * gain).astype(np.float32)
    buffer[start:end, 0] += rendered * left
    buffer[start:end, 1] += rendered * right


def add_tick(buffer: np.ndarray, start: int, gain: float, pan: float, rng: np.random.Generator) -> None:
    duration = int(0.045 * SAMPLE_RATE)
    if start >= len(buffer):
        return
    end = min(len(buffer), start + duration)
    count = end - start
    time = np.arange(count, dtype=np.float32) / SAMPLE_RATE
    noise = rng.normal(0.0, 1.0, count).astype(np.float32)
    high_passed = np.concatenate(([noise[0]], np.diff(noise)))
    high_passed /= max(1.0, float(np.max(np.abs(high_passed))))
    body = np.sin(2 * np.pi * 1850.0 * time) * np.exp(-76.0 * time)
    env = envelope(count, 0.002, 0.014, 0.16, 0.025)
    signal = (0.55 * body + 0.45 * high_passed) * env * gain
    left, right = pan_gains(pan)
    buffer[start:end, 0] += signal * left
    buffer[start:end, 1] += signal * right


def envelope(count: int, attack_s: float, decay_s: float, sustain: float, release_s: float) -> np.ndarray:
    env = np.ones(count, dtype=np.float32) * sustain
    attack = min(count, int(attack_s * SAMPLE_RATE))
    decay = min(max(0, count - attack), int(decay_s * SAMPLE_RATE))
    release = min(count, int(release_s * SAMPLE_RATE))

    if attack > 0:
        env[:attack] = np.linspace(0.0, 1.0, attack, dtype=np.float32)
    if decay > 0 and attack + decay <= count:
        env[attack : attack + decay] = np.linspace(1.0, sustain, decay, dtype=np.float32)
    if release > 0:
        env[-release:] *= np.linspace(1.0, 0.0, release, dtype=np.float32)
    return env


def pan_gains(pan: float) -> tuple[float, float]:
    clamped = max(-1.0, min(1.0, pan))
    angle = (clamped + 1.0) * math.pi / 4.0
    return math.cos(angle), math.sin(angle)


def midi_freq(midi_note: int) -> float:
    return 440.0 * (2.0 ** ((midi_note - 69) / 12.0))


def sample_at(bar: int, beat_offset: float) -> int:
    return int(bar * SAMPLES_PER_BAR + beat_offset * SAMPLES_PER_BEAT)


def beats(count: float) -> int:
    return int(count * SAMPLES_PER_BEAT)


def normalization_scale(master: np.ndarray) -> float:
    peak = float(np.max(np.abs(master)))
    rms = float(np.sqrt(np.mean(np.square(master))))
    target_rms = db_to_amp(-23.5)
    peak_ceiling = db_to_amp(-5.0)
    if peak <= 0 or rms <= 0:
        return 1.0
    return min(target_rms / rms, peak_ceiling / peak)


def convert_runtime_ogg(intro_wav_path: Path, loop_wav_path: Path) -> dict[str, Path]:
    intro_package_ogg = PACKAGE_DIR / f"{INTRO_NAME}.ogg"
    loop_package_ogg = PACKAGE_DIR / f"{LOOP_NAME}.ogg"
    convert_with_encoder(intro_wav_path, intro_package_ogg)
    convert_with_encoder(loop_wav_path, loop_package_ogg)

    runtime_intro = UNITY_AUDIO_DIR / intro_package_ogg.name
    runtime_loop = UNITY_AUDIO_DIR / loop_package_ogg.name
    shutil.copy2(intro_package_ogg, runtime_intro)
    shutil.copy2(loop_package_ogg, runtime_loop)
    return {"intro": runtime_intro, "loop": runtime_loop}


def convert_with_encoder(source: Path, destination: Path) -> None:
    ffmpeg = find_ffmpeg()
    if ffmpeg is not None:
        subprocess.run(
            [
                ffmpeg,
                "-y",
                "-hide_banner",
                "-loglevel",
                "error",
                "-i",
                str(source),
                "-c:a",
                "libvorbis",
                "-q:a",
                "5",
                "-bitexact",
                str(destination),
            ],
            check=True,
        )
        return

    afconvert = shutil.which("afconvert")
    if afconvert is None:
        raise RuntimeError("ffmpeg or afconvert is required to create runtime OGG files on this workstation.")

    subprocess.run(
        [
            afconvert,
            str(source),
            str(destination),
            "-f",
            "Oggf",
            "-d",
            "vorb",
            "-q",
            "96",
        ],
        check=True,
    )


def find_ffmpeg() -> str | None:
    ffmpeg = shutil.which("ffmpeg")
    if ffmpeg is not None:
        return ffmpeg

    try:
        import imageio_ffmpeg
    except ImportError:
        return None

    return imageio_ffmpeg.get_ffmpeg_exe()


def build_metrics(
    master: np.ndarray,
    loop: np.ndarray,
    stems: dict[str, np.ndarray],
    runtime_paths: dict[str, Path],
) -> dict[str, object]:
    peak = float(np.max(np.abs(master)))
    rms = float(np.sqrt(np.mean(np.square(master))))
    loop_delta = float(np.max(np.abs(loop[0] - loop[-1])))
    loop_tail_peak = float(np.max(np.abs(loop[-1024:])))
    low_energy_ratio = band_energy_ratio(master, 20.0, 160.0)
    dialogue_band_ratio = band_energy_ratio(master, 1000.0, 4000.0)

    stem_metrics: dict[str, dict[str, float]] = {}
    for name, data in stems.items():
        stem_metrics[name] = {
            "peak_dbfs": amp_to_db(float(np.max(np.abs(data)))),
            "rms_dbfs": amp_to_db(float(np.sqrt(np.mean(np.square(data))))),
        }

    return {
        "sample_rate": SAMPLE_RATE,
        "bit_depth": 24,
        "channels": 2,
        "bpm": BPM,
        "beats_per_bar": BEATS_PER_BAR,
        "duration_seconds": TOTAL_SAMPLES / SAMPLE_RATE,
        "total_samples": TOTAL_SAMPLES,
        "intro_bars": INTRO_BARS,
        "loop_bars": LOOP_BARS,
        "loop_start_sample": LOOP_START_SAMPLE,
        "loop_end_sample": LOOP_END_SAMPLE,
        "loop_start_seconds": LOOP_START_SAMPLE / SAMPLE_RATE,
        "loop_end_seconds": LOOP_END_SAMPLE / SAMPLE_RATE,
        "loop_length_samples": LOOP_LENGTH_SAMPLES,
        "loop_length_seconds": LOOP_LENGTH_SAMPLES / SAMPLE_RATE,
        "peak_dbfs": amp_to_db(peak),
        "rms_dbfs": amp_to_db(rms),
        "loop_boundary_delta_peak": loop_delta,
        "loop_tail_peak": loop_tail_peak,
        "low_energy_ratio_20_160hz": low_energy_ratio,
        "dialogue_band_ratio_1_4khz": dialogue_band_ratio,
        "runtime_bytes": {kind: path.stat().st_size for kind, path in runtime_paths.items()},
        "stem_metrics": stem_metrics,
    }


def band_energy_ratio(audio: np.ndarray, low_hz: float, high_hz: float) -> float:
    mono = np.mean(audio, axis=1)
    window = np.hanning(len(mono))
    spectrum = np.fft.rfft(mono * window)
    power = np.abs(spectrum) ** 2
    frequencies = np.fft.rfftfreq(len(mono), 1.0 / SAMPLE_RATE)
    total = float(np.sum(power))
    if total <= 0:
        return 0.0
    mask = (frequencies >= low_hz) & (frequencies <= high_hz)
    return float(np.sum(power[mask]) / total)


def write_asset_request() -> None:
    text = f"""asset_id: {ASSET_ID}
title: {TITLE}
scene_type: {SCENE_TYPE}
mood: tense
bpm_target: {BPM}
loop_requirement:
  intro_bars: {INTRO_BARS}
  loop_bars: {LOOP_BARS}
  seamless_loop: true
  full_master_loop_start_sample: {LOOP_START_SAMPLE}
  full_master_loop_end_sample: {LOOP_END_SAMPLE}
  runtime_intro_clip: Assets/Audio/Generated/{INTRO_NAME}.ogg
  runtime_loop_clip: Assets/Audio/Generated/{LOOP_NAME}.ogg
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
  - Keep combat SFX above the loop: no heavy kick, no constant crash layer.
  - Use the intro only for combat escalation; loop clip can start directly for quick encounters.
  - Stems are for mix/edit review, not a complex MVP dynamic music system.
"""
    write_text_to_outputs("asset_request.yaml", text)


def write_cue_sheet(
    metrics: dict[str, object],
    stem_paths: dict[str, Path],
    runtime_paths: dict[str, Path],
) -> None:
    rows = [
        {
            "cue_id": ASSET_ID,
            "title": TITLE,
            "scene_type": SCENE_TYPE,
            "mood": "tense",
            "bpm": BPM,
            "sample_rate": SAMPLE_RATE,
            "master_file": f"{BASE_NAME}.wav",
            "runtime_intro_file": runtime_paths["intro"].name,
            "runtime_loop_file": runtime_paths["loop"].name,
            "start_sample": 0,
            "intro_end_sample": LOOP_START_SAMPLE,
            "loop_start_sample": LOOP_START_SAMPLE,
            "loop_end_sample": LOOP_END_SAMPLE,
            "loop_length_samples": LOOP_LENGTH_SAMPLES,
            "intro_bars": INTRO_BARS,
            "loop_bars": LOOP_BARS,
            "fade_note": "No baked loop fade. Optional engine crossfade 120-250 ms when entering combat.",
            "loudness_note": f"Peak {metrics['peak_dbfs']:.2f} dBFS, RMS {metrics['rms_dbfs']:.2f} dBFS; combat SFX should lead.",
            "usage_note": "Play runtime intro once for deliberate engagements, then loop runtime loop clip.",
        }
    ]
    fieldnames = list(rows[0].keys())
    for destination in (PACKAGE_DIR / f"{BASE_NAME}_cue_sheet.csv", DOCS_DIR / "cue_sheet.csv"):
        with destination.open("w", encoding="utf-8", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=fieldnames, lineterminator="\n")
            writer.writeheader()
            writer.writerows(rows)

    roles = {
        "rhythm": "muted weak-beat ostinato, avoids SFX attack masking",
        "harmony": "short tense chord bed, restrained low roots",
        "lead": "brief callouts only, no constant melody",
        "percussion": "quiet high ticks, no heavy kick/snare layer",
    }
    stem_rows = [
        {"stem": stem, "path": str(path.relative_to(PACKAGE_DIR)), "role": roles[stem]}
        for stem, path in stem_paths.items()
    ]
    for destination in (PACKAGE_DIR / f"{BASE_NAME}_stems.csv", DOCS_DIR / "stems.csv"):
        with destination.open("w", encoding="utf-8", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=["stem", "path", "role"], lineterminator="\n")
            writer.writeheader()
            writer.writerows(stem_rows)


def write_asset_json(
    metrics: dict[str, object],
    stem_paths: dict[str, Path],
    runtime_paths: dict[str, Path],
) -> None:
    data = {
        "asset_id": ASSET_ID,
        "title": TITLE,
        "scene": SCENE,
        "scene_type": SCENE_TYPE,
        "mood": MOOD,
        "variant": VARIANT,
        "bpm": BPM,
        "version": VERSION,
        "status": "generated_pilot",
        "source_strategy": "repository-authored deterministic procedural synthesis",
        "license": "repository-authored",
        "target_platforms": ["Steam PC", "future console portable"],
        "library_root_relative": f"Runtime/Audio/BGM/{BASE_NAME}",
        "paths": {
            "master_wav": f"{BASE_NAME}.wav",
            "runtime_intro_ogg": str(runtime_paths["intro"].relative_to(REPO)),
            "runtime_loop_ogg": str(runtime_paths["loop"].relative_to(REPO)),
            "cue_sheet": "cue_sheet.csv",
            "asset_request": "asset_request.yaml",
            "qc_result": "qc_result.json",
            "stems": {stem: str(path.relative_to(PACKAGE_DIR)) for stem, path in stem_paths.items()},
        },
        "loop": {
            "full_master_start_sample": LOOP_START_SAMPLE,
            "full_master_end_sample": LOOP_END_SAMPLE,
            "full_master_start_seconds": metrics["loop_start_seconds"],
            "full_master_end_seconds": metrics["loop_end_seconds"],
            "loop_clip_start_sample": 0,
            "loop_clip_end_sample": LOOP_LENGTH_SAMPLES,
            "loop_clip_length_seconds": metrics["loop_length_seconds"],
            "intro_bars": INTRO_BARS,
            "loop_bars": LOOP_BARS,
            "playback_instruction": "Play runtime_intro_ogg once for combat start, then loop runtime_loop_ogg from sample 0.",
        },
        "mix_notes": {
            "loudness_note": f"Peak {metrics['peak_dbfs']:.2f} dBFS, RMS {metrics['rms_dbfs']:.2f} dBFS.",
            "fade_note": "No baked loop fade. Optional engine crossfade 120-250 ms when entering combat.",
            "sfx_space_note": "Weak-beat ostinato and high soft ticks leave attack, dodge, hit, and enemy tell transients exposed.",
            "low_end_note": "No kick/sub layer; roots are short and gain-limited.",
            "melody_note": "Lead appears in four short callouts over the 16-bar loop.",
        },
        "stems": [
            {
                "name": stem,
                "file": str(path.relative_to(PACKAGE_DIR)),
                "peak_dbfs": metrics["stem_metrics"][stem]["peak_dbfs"],
                "rms_dbfs": metrics["stem_metrics"][stem]["rms_dbfs"],
            }
            for stem, path in stem_paths.items()
        ],
        "planned_companion_cues": [
            {"structure": "title", "status": "planned"},
            {"structure": "field exploration", "status": "generated_pilot", "asset_id": "audio.music.bgm_region01"},
            {"structure": "battle", "status": "generated_pilot", "asset_id": ASSET_ID},
            {"structure": "boss phase1", "status": "planned"},
            {"structure": "boss phase2", "status": "planned"},
            {"structure": "rest room", "status": "planned"},
            {"structure": "victory stinger", "status": "planned"},
            {"structure": "defeat stinger", "status": "planned"},
        ],
    }
    write_text_to_outputs("asset.json", json.dumps(data, indent=2) + "\n")


def write_qc_result(metrics: dict[str, object]) -> None:
    result = {
        "asset_id": ASSET_ID,
        "qc_status": "pass_with_notes",
        "checks": [
            {
                "name": "loop_point_recorded",
                "status": "pass",
                "detail": f"loop_start_sample={LOOP_START_SAMPLE}, loop_end_sample={LOOP_END_SAMPLE}",
            },
            {
                "name": "loop_boundary_click_risk",
                "status": "pass" if metrics["loop_boundary_delta_peak"] < 0.012 else "review",
                "detail": f"loop clip first/last sample delta {metrics['loop_boundary_delta_peak']:.6f}",
            },
            {
                "name": "loop_tail_release",
                "status": "pass" if metrics["loop_tail_peak"] < 0.030 else "review",
                "detail": f"last 1024 sample peak {metrics['loop_tail_peak']:.6f}",
            },
            {
                "name": "low_end_control",
                "status": "pass" if metrics["low_energy_ratio_20_160hz"] < 0.22 else "review",
                "detail": f"20-160 Hz energy ratio {metrics['low_energy_ratio_20_160hz']:.4f}",
            },
            {
                "name": "dialogue_band_control",
                "status": "pass" if metrics["dialogue_band_ratio_1_4khz"] < 0.35 else "review",
                "detail": f"1-4 kHz energy ratio {metrics['dialogue_band_ratio_1_4khz']:.4f}",
            },
            {
                "name": "headroom",
                "status": "pass" if metrics["peak_dbfs"] <= -4.5 else "review",
                "detail": f"peak {metrics['peak_dbfs']:.2f} dBFS",
            },
            {
                "name": "combat_sfx_space",
                "status": "pass",
                "detail": "No kick/snare wall; sparse high ticks and weak-beat ostinato leave attack transients open.",
            },
        ],
        "metrics": metrics,
        "notes": [
            "Procedural pilot only; requires listening review in a combat scene before Steam capture.",
            "Runtime is split into intro and loop clips so Unity does not need sub-sample loop metadata for this pilot.",
        ],
    }
    write_text_to_outputs("qc_result.json", json.dumps(result, indent=2) + "\n")


def write_text_to_outputs(filename: str, text: str) -> None:
    (PACKAGE_DIR / filename).write_text(text, encoding="utf-8")
    (DOCS_DIR / filename).write_text(text, encoding="utf-8")


def write_unity_audio_meta(audio_path: Path) -> None:
    relative_path = audio_path.relative_to(REPO).as_posix()
    guid = hashlib.md5(f"fourfold-echoes:{relative_path}".encode("utf-8")).hexdigest()
    text = f"""fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 2
    sampleRateSetting: 0
    sampleRateOverride: {SAMPLE_RATE}
    compressionFormat: 1
    quality: 0.72
    conversionMode: 0
    preloadAudioData: 0
  platformSettingOverrides: {{}}
  forceToMono: 0
  normalize: 0
  loadInBackground: 1
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    audio_path.with_suffix(audio_path.suffix + ".meta").write_text(text, encoding="utf-8")


def write_wav24(path: Path, audio: np.ndarray) -> None:
    clipped = np.clip(audio, -0.999999, 0.999999)
    int_audio = np.round(clipped * 8388607.0).astype(np.int32)
    channels = 2
    bytes_per_sample = 3
    data_size = int_audio.shape[0] * channels * bytes_per_sample
    byte_rate = SAMPLE_RATE * channels * bytes_per_sample
    block_align = channels * bytes_per_sample

    with path.open("wb") as handle:
        handle.write(b"RIFF")
        handle.write(struct.pack("<I", 36 + data_size))
        handle.write(b"WAVE")
        handle.write(b"fmt ")
        handle.write(struct.pack("<IHHIIHH", 16, 1, channels, SAMPLE_RATE, byte_rate, block_align, 24))
        handle.write(b"data")
        handle.write(struct.pack("<I", data_size))
        packed = np.where(int_audio < 0, int_audio + (1 << 24), int_audio).astype(np.uint32)
        bytes_out = np.empty((*packed.shape, bytes_per_sample), dtype=np.uint8)
        bytes_out[:, :, 0] = packed & 0xFF
        bytes_out[:, :, 1] = (packed >> 8) & 0xFF
        bytes_out[:, :, 2] = (packed >> 16) & 0xFF
        handle.write(bytes_out.tobytes())


def amp_to_db(value: float) -> float:
    return 20.0 * math.log10(max(value, 1.0e-12))


def db_to_amp(value: float) -> float:
    return 10.0 ** (value / 20.0)


if __name__ == "__main__":
    main()
