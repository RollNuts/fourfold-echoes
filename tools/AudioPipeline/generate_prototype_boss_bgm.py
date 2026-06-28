#!/usr/bin/env python3
"""Generate deterministic prototype boss phase BGM for FOURFOLD ECHOES."""

from __future__ import annotations

import csv
import hashlib
import json
import math
import shutil
import struct
import subprocess
from dataclasses import dataclass
from pathlib import Path

import numpy as np


SAMPLE_RATE = 44100
BEATS_PER_BAR = 4
VERSION = "0.1.0"
VARIANT = "A"
REPO = Path(__file__).resolve().parents[2]
UNITY_AUDIO_DIR = REPO / "Assets" / "Audio" / "Generated"
STEM_NAMES = ("rhythm", "harmony", "lead", "percussion")


@dataclass(frozen=True)
class TrackSpec:
    asset_id: str
    title: str
    scene: str
    scene_type: str
    mood: str
    bpm: int
    intro_bars: int
    loop_bars: int
    seed: int
    target_rms_dbfs: float
    peak_ceiling_dbfs: float

    @property
    def base_name(self) -> str:
        return f"BGM_{self.scene}_{self.mood}_{VARIANT}_{self.bpm:03d}_v{VERSION}"

    @property
    def intro_name(self) -> str:
        return f"BGM_{self.scene}_{self.mood}_{VARIANT}Intro_{self.bpm:03d}_v{VERSION}"

    @property
    def loop_name(self) -> str:
        return f"BGM_{self.scene}_{self.mood}_{VARIANT}Loop_{self.bpm:03d}_v{VERSION}"

    @property
    def samples_per_beat(self) -> int:
        return int(SAMPLE_RATE * 60 / self.bpm)

    @property
    def samples_per_bar(self) -> int:
        return self.samples_per_beat * BEATS_PER_BAR

    @property
    def total_samples(self) -> int:
        return (self.intro_bars + self.loop_bars) * self.samples_per_bar

    @property
    def loop_start_sample(self) -> int:
        return self.intro_bars * self.samples_per_bar

    @property
    def loop_end_sample(self) -> int:
        return self.total_samples

    @property
    def loop_length_samples(self) -> int:
        return self.loop_end_sample - self.loop_start_sample


TRACKS = (
    TrackSpec(
        asset_id="audio.music.bgm_boss",
        title="Root Warden Coil",
        scene="BossPhase1",
        scene_type="boss phase1",
        mood="Ominous",
        bpm=104,
        intro_bars=2,
        loop_bars=16,
        seed=0x46455F5031,
        target_rms_dbfs=-23.2,
        peak_ceiling_dbfs=-5.2,
    ),
    TrackSpec(
        asset_id="audio.music.bgm_boss_phase2",
        title="Root Warden Break",
        scene="BossPhase2",
        scene_type="boss phase2",
        mood="Urgent",
        bpm=128,
        intro_bars=1,
        loop_bars=16,
        seed=0x46455F5032,
        target_rms_dbfs=-22.8,
        peak_ceiling_dbfs=-5.4,
    ),
)


def main() -> None:
    UNITY_AUDIO_DIR.mkdir(parents=True, exist_ok=True)
    for spec in TRACKS:
        render_track(spec)


def render_track(spec: TrackSpec) -> None:
    package_dir = package_dir_for(spec)
    stems_dir = package_dir / "Stems"
    docs_dir = docs_dir_for(spec)
    package_dir.mkdir(parents=True, exist_ok=True)
    stems_dir.mkdir(parents=True, exist_ok=True)
    docs_dir.mkdir(parents=True, exist_ok=True)

    stems = {name: np.zeros((spec.total_samples, 2), dtype=np.float32) for name in STEM_NAMES}
    rng = np.random.default_rng(spec.seed)
    if spec.scene == "BossPhase1":
        render_phase1(spec, stems, rng)
    else:
        render_phase2(spec, stems, rng)

    raw_master = sum(stems.values())
    scale = normalization_scale(spec, raw_master)
    stems = {name: data * scale for name, data in stems.items()}
    master = sum(stems.values())
    intro = master[: spec.loop_start_sample]
    loop = master[spec.loop_start_sample : spec.loop_end_sample]

    master_path = package_dir / f"{spec.base_name}.wav"
    intro_wav_path = package_dir / f"{spec.intro_name}.wav"
    loop_wav_path = package_dir / f"{spec.loop_name}.wav"
    write_wav24(master_path, master)
    write_wav24(intro_wav_path, intro)
    write_wav24(loop_wav_path, loop)

    stem_paths: dict[str, Path] = {}
    for stem_name, audio in stems.items():
        path = stems_dir / f"BGM_{spec.scene}_{spec.mood}_{VARIANT}{stem_name.capitalize()}_{spec.bpm:03d}_v{VERSION}.wav"
        write_wav24(path, audio)
        stem_paths[stem_name] = path

    runtime_paths = convert_runtime_ogg(spec, intro_wav_path, loop_wav_path, package_dir)
    metrics = build_metrics(spec, master, loop, stems, runtime_paths)
    write_asset_request(spec, package_dir, docs_dir)
    write_cue_sheet(spec, metrics, stem_paths, runtime_paths, package_dir, docs_dir)
    write_asset_json(spec, metrics, stem_paths, runtime_paths, package_dir, docs_dir)
    write_qc_result(spec, metrics, package_dir, docs_dir)
    write_unity_audio_meta(runtime_paths["intro"])
    write_unity_audio_meta(runtime_paths["loop"])

    print(f"[bgm] wrote {spec.base_name}")
    print(f"[bgm] loop_start_sample={spec.loop_start_sample} loop_end_sample={spec.loop_end_sample}")
    print(f"[bgm] peak_dbfs={metrics['peak_dbfs']:.2f} rms_dbfs={metrics['rms_dbfs']:.2f}")


def render_phase1(spec: TrackSpec, stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    add_chord(spec, stems["harmony"], 0, 1.2, [45, 52, 57, 62], 0.034, "dark_pad", -0.10)
    add_chord(spec, stems["harmony"], 1, 0.8, [48, 55, 60, 64], 0.030, "dark_pad", 0.10)
    add_note(spec, stems["lead"], sample_at(spec, 1, 2.5), beats(spec, 0.55), midi_freq(67), 0.020, 0.12, "warning_bell")
    add_tick(spec, stems["percussion"], sample_at(spec, 1, 3.45), 0.016, -0.12, rng)

    progression = [
        (0, 1.35, [45, 52, 57, 62]),
        (2, 1.20, [48, 55, 60, 64]),
        (4, 1.35, [43, 50, 55, 59]),
        (6, 1.05, [47, 54, 59, 62]),
        (8, 1.35, [45, 52, 57, 62]),
        (10, 1.20, [41, 48, 53, 57]),
        (12, 1.35, [48, 55, 60, 64]),
        (14, 0.95, [47, 52, 57, 60]),
    ]
    for loop_bar, length, notes in progression:
        add_chord(spec, stems["harmony"], spec.intro_bars + loop_bar, length, notes, 0.030, "dark_pad", -0.08 if loop_bar % 4 == 0 else 0.08)

    for loop_bar in range(spec.loop_bars):
        absolute_bar = spec.intro_bars + loop_bar
        notes = phase1_notes(loop_bar)
        offsets = (0.75, 1.75, 2.75) if loop_bar != spec.loop_bars - 1 else (0.75,)
        for index, beat_offset in enumerate(offsets):
            add_note(spec, stems["rhythm"], sample_at(spec, absolute_bar, beat_offset), beats(spec, 0.18), midi_freq(notes[index % len(notes)] + 12), 0.018, -0.26 if index % 2 == 0 else 0.26, "muted_pluck")
        if loop_bar % 4 in (1, 3) and loop_bar != spec.loop_bars - 1:
            add_tick(spec, stems["percussion"], sample_at(spec, absolute_bar, 3.20), 0.014, 0.18, rng)

    for loop_bar, phrase in (
        (3, [(0.35, 67, 0.24), (1.15, 69, 0.20)]),
        (7, [(0.20, 65, 0.22), (1.10, 62, 0.28)]),
        (11, [(0.40, 69, 0.18), (0.95, 72, 0.22)]),
    ):
        add_lead_phrase(spec, stems["lead"], spec.intro_bars + loop_bar, phrase)


def render_phase2(spec: TrackSpec, stems: dict[str, np.ndarray], rng: np.random.Generator) -> None:
    add_chord(spec, stems["harmony"], 0, 0.72, [43, 50, 55, 62], 0.032, "dark_pad", 0.0)
    add_tick(spec, stems["percussion"], sample_at(spec, 0, 3.30), 0.020, 0.16, rng)

    progression = [
        (0, 1.15, [43, 50, 55, 62]),
        (2, 1.05, [45, 52, 57, 64]),
        (4, 1.15, [48, 55, 60, 67]),
        (6, 0.95, [47, 54, 59, 65]),
        (8, 1.15, [43, 50, 55, 62]),
        (10, 1.05, [41, 48, 53, 60]),
        (12, 1.15, [48, 55, 60, 67]),
        (14, 0.80, [50, 55, 59, 65]),
    ]
    for loop_bar, length, notes in progression:
        add_chord(spec, stems["harmony"], spec.intro_bars + loop_bar, length, notes, 0.026, "dark_pad", -0.07 if loop_bar % 4 == 0 else 0.07)

    for loop_bar in range(spec.loop_bars):
        absolute_bar = spec.intro_bars + loop_bar
        notes = phase2_notes(loop_bar)
        offsets = (0.50, 1.25, 2.00, 2.75) if loop_bar != spec.loop_bars - 1 else (0.50, 1.25)
        for index, beat_offset in enumerate(offsets):
            add_note(spec, stems["rhythm"], sample_at(spec, absolute_bar, beat_offset), beats(spec, 0.12), midi_freq(notes[(index + 1) % len(notes)] + 12), 0.020, -0.24 if index % 2 == 0 else 0.24, "muted_pluck")
        if loop_bar % 2 == 1 and loop_bar != spec.loop_bars - 1:
            add_tick(spec, stems["percussion"], sample_at(spec, absolute_bar, 3.10), 0.016, 0.18, rng)

    for loop_bar, phrase in (
        (2, [(0.20, 74, 0.14), (0.70, 76, 0.13), (1.20, 79, 0.18)]),
        (6, [(0.15, 72, 0.14), (0.65, 74, 0.14), (1.30, 67, 0.22)]),
        (10, [(0.30, 76, 0.12), (0.80, 79, 0.13), (1.40, 81, 0.16)]),
        (14, [(0.25, 74, 0.13), (0.90, 72, 0.16)]),
    ):
        add_lead_phrase(spec, stems["lead"], spec.intro_bars + loop_bar, phrase)


def phase1_notes(loop_bar: int) -> list[int]:
    groups = ([45, 52, 57, 62], [48, 55, 60, 64], [43, 50, 55, 59], [47, 54, 59, 62])
    return groups[(loop_bar // 2) % len(groups)]


def phase2_notes(loop_bar: int) -> list[int]:
    groups = ([43, 50, 55, 62], [45, 52, 57, 64], [48, 55, 60, 67], [47, 54, 59, 65])
    return groups[(loop_bar // 2) % len(groups)]


def add_lead_phrase(spec: TrackSpec, buffer: np.ndarray, bar: int, phrase: list[tuple[float, int, float]]) -> None:
    for beat_offset, note, length in phrase:
        add_note(spec, buffer, sample_at(spec, bar, beat_offset), beats(spec, length), midi_freq(note), 0.018, 0.14 if note >= 70 else -0.14, "warning_bell")


def add_chord(spec: TrackSpec, buffer: np.ndarray, bar: int, length_bars: float, midi_notes: list[int], gain: float, instrument: str, pan: float) -> None:
    duration = int(length_bars * spec.samples_per_bar)
    for index, note in enumerate(midi_notes):
        note_gain = gain * (0.10 if index == 0 else 0.25)
        note_pan = pan + (index - (len(midi_notes) - 1) / 2) * 0.09
        add_note(spec, buffer, sample_at(spec, bar, 0), duration, midi_freq(note), note_gain, note_pan, instrument)


def add_note(spec: TrackSpec, buffer: np.ndarray, start: int, duration: int, frequency: float, gain: float, pan: float, instrument: str) -> None:
    if duration <= 0 or start >= len(buffer):
        return
    end = min(len(buffer), start + duration)
    count = end - start
    if count <= 0:
        return

    time = np.arange(count, dtype=np.float32) / SAMPLE_RATE
    phase = 2 * np.pi * frequency * time
    if instrument == "dark_pad":
        tremolo = 1.0 + 0.040 * np.sin(2 * np.pi * 3.0 * time)
        signal = (np.sin(phase) + 0.24 * np.sin(phase * 2.0 + 0.5) + 0.08 * np.sin(phase * 3.0)) * tremolo
        env = envelope(count, 0.12, 0.18, 0.54, 0.38)
    elif instrument == "muted_pluck":
        signal = np.sin(phase) + 0.20 * np.sin(phase * 2.0)
        env = envelope(count, 0.004, 0.028, 0.16, 0.050) * np.exp(-9.4 * time)
    elif instrument == "warning_bell":
        signal = np.sin(phase) + 0.30 * np.sin(phase * 2.02) + 0.09 * np.sin(phase * 3.96)
        env = envelope(count, 0.006, 0.055, 0.15, 0.12) * np.exp(-4.5 * time)
    else:
        signal = np.sin(phase)
        env = envelope(count, 0.01, 0.1, 0.5, 0.1)

    left, right = pan_gains(pan)
    rendered = (signal * env * gain).astype(np.float32)
    buffer[start:end, 0] += rendered * left
    buffer[start:end, 1] += rendered * right


def add_tick(spec: TrackSpec, buffer: np.ndarray, start: int, gain: float, pan: float, rng: np.random.Generator) -> None:
    duration = int(0.050 * SAMPLE_RATE)
    if start >= len(buffer):
        return
    end = min(len(buffer), start + duration)
    count = end - start
    time = np.arange(count, dtype=np.float32) / SAMPLE_RATE
    noise = rng.normal(0.0, 1.0, count).astype(np.float32)
    high_passed = np.concatenate(([noise[0]], np.diff(noise)))
    high_passed /= max(1.0, float(np.max(np.abs(high_passed))))
    body = np.sin(2 * np.pi * 1650.0 * time) * np.exp(-72.0 * time)
    env = envelope(count, 0.002, 0.014, 0.16, 0.028)
    signal = (0.58 * body + 0.42 * high_passed) * env * gain
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


def sample_at(spec: TrackSpec, bar: int, beat_offset: float) -> int:
    return int(bar * spec.samples_per_bar + beat_offset * spec.samples_per_beat)


def beats(spec: TrackSpec, count: float) -> int:
    return int(count * spec.samples_per_beat)


def normalization_scale(spec: TrackSpec, master: np.ndarray) -> float:
    peak = float(np.max(np.abs(master)))
    rms = float(np.sqrt(np.mean(np.square(master))))
    if peak <= 0 or rms <= 0:
        return 1.0
    return min(db_to_amp(spec.target_rms_dbfs) / rms, db_to_amp(spec.peak_ceiling_dbfs) / peak)


def convert_runtime_ogg(spec: TrackSpec, intro_wav_path: Path, loop_wav_path: Path, package_dir: Path) -> dict[str, Path]:
    intro_package_ogg = package_dir / f"{spec.intro_name}.ogg"
    loop_package_ogg = package_dir / f"{spec.loop_name}.ogg"
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
        subprocess.run([ffmpeg, "-y", "-hide_banner", "-loglevel", "error", "-i", str(source), "-c:a", "libvorbis", "-q:a", "5", "-bitexact", str(destination)], check=True)
        return
    afconvert = shutil.which("afconvert")
    if afconvert is None:
        raise RuntimeError("ffmpeg or afconvert is required to create runtime OGG files on this workstation.")
    subprocess.run([afconvert, str(source), str(destination), "-f", "Oggf", "-d", "vorb", "-q", "96"], check=True)


def find_ffmpeg() -> str | None:
    ffmpeg = shutil.which("ffmpeg")
    if ffmpeg is not None:
        return ffmpeg
    try:
        import imageio_ffmpeg
    except ImportError:
        return None
    return imageio_ffmpeg.get_ffmpeg_exe()


def build_metrics(spec: TrackSpec, master: np.ndarray, loop: np.ndarray, stems: dict[str, np.ndarray], runtime_paths: dict[str, Path]) -> dict[str, object]:
    peak = float(np.max(np.abs(master)))
    rms = float(np.sqrt(np.mean(np.square(master))))
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
        "bpm": spec.bpm,
        "beats_per_bar": BEATS_PER_BAR,
        "duration_seconds": spec.total_samples / SAMPLE_RATE,
        "total_samples": spec.total_samples,
        "intro_bars": spec.intro_bars,
        "loop_bars": spec.loop_bars,
        "loop_start_sample": spec.loop_start_sample,
        "loop_end_sample": spec.loop_end_sample,
        "loop_start_seconds": spec.loop_start_sample / SAMPLE_RATE,
        "loop_end_seconds": spec.loop_end_sample / SAMPLE_RATE,
        "loop_length_samples": spec.loop_length_samples,
        "loop_length_seconds": spec.loop_length_samples / SAMPLE_RATE,
        "peak_dbfs": amp_to_db(peak),
        "rms_dbfs": amp_to_db(rms),
        "loop_boundary_delta_peak": float(np.max(np.abs(loop[0] - loop[-1]))),
        "loop_tail_peak": float(np.max(np.abs(loop[-1024:]))),
        "low_energy_ratio_20_160hz": band_energy_ratio(master, 20.0, 160.0),
        "dialogue_band_ratio_1_4khz": band_energy_ratio(master, 1000.0, 4000.0),
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


def write_asset_request(spec: TrackSpec, package_dir: Path, docs_dir: Path) -> None:
    text = f"""asset_id: {spec.asset_id}
title: {spec.title}
scene_type: {spec.scene_type}
mood: {spec.mood.lower()}
bpm_target: {spec.bpm}
loop_requirement:
  intro_bars: {spec.intro_bars}
  loop_bars: {spec.loop_bars}
  seamless_loop: true
  full_master_loop_start_sample: {spec.loop_start_sample}
  full_master_loop_end_sample: {spec.loop_end_sample}
  runtime_intro_clip: Assets/Audio/Generated/{spec.intro_name}.ogg
  runtime_loop_clip: Assets/Audio/Generated/{spec.loop_name}.ogg
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
  - Boss tell and impact SFX outrank this music.
  - Use the intro for deliberate boss phase entry, then loop the runtime loop clip.
  - Keep phase transitions in code simple: stop or crossfade phase1, play phase2 intro once, then phase2 loop.
"""
    write_text_to_outputs(package_dir, docs_dir, "asset_request.yaml", text)


def write_cue_sheet(spec: TrackSpec, metrics: dict[str, object], stem_paths: dict[str, Path], runtime_paths: dict[str, Path], package_dir: Path, docs_dir: Path) -> None:
    rows = [{
        "cue_id": spec.asset_id,
        "title": spec.title,
        "scene_type": spec.scene_type,
        "mood": spec.mood.lower(),
        "bpm": spec.bpm,
        "sample_rate": SAMPLE_RATE,
        "master_file": f"{spec.base_name}.wav",
        "runtime_intro_file": runtime_paths["intro"].name,
        "runtime_loop_file": runtime_paths["loop"].name,
        "start_sample": 0,
        "intro_end_sample": spec.loop_start_sample,
        "loop_start_sample": spec.loop_start_sample,
        "loop_end_sample": spec.loop_end_sample,
        "loop_length_samples": spec.loop_length_samples,
        "intro_bars": spec.intro_bars,
        "loop_bars": spec.loop_bars,
        "fade_note": "No baked loop fade. Optional engine crossfade 150-300 ms on phase transition.",
        "loudness_note": f"Peak {metrics['peak_dbfs']:.2f} dBFS, RMS {metrics['rms_dbfs']:.2f} dBFS; boss SFX should lead.",
        "usage_note": "Play runtime intro once for phase entry, then loop runtime loop clip.",
    }]
    for destination in (package_dir / f"{spec.base_name}_cue_sheet.csv", docs_dir / "cue_sheet.csv"):
        with destination.open("w", encoding="utf-8", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=list(rows[0].keys()), lineterminator="\n")
            writer.writeheader()
            writer.writerows(rows)

    roles = {
        "rhythm": "muted boss pulse, avoids boss tell attack masking",
        "harmony": "dark phase bed, restrained sub energy",
        "lead": "short warning callouts only",
        "percussion": "quiet high ticks, no impact replacement",
    }
    stem_rows = [{"stem": stem, "path": str(path.relative_to(package_dir)), "role": roles[stem]} for stem, path in stem_paths.items()]
    for destination in (package_dir / f"{spec.base_name}_stems.csv", docs_dir / "stems.csv"):
        with destination.open("w", encoding="utf-8", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=["stem", "path", "role"], lineterminator="\n")
            writer.writeheader()
            writer.writerows(stem_rows)


def write_asset_json(spec: TrackSpec, metrics: dict[str, object], stem_paths: dict[str, Path], runtime_paths: dict[str, Path], package_dir: Path, docs_dir: Path) -> None:
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
        "paths": {
            "master_wav": f"{spec.base_name}.wav",
            "runtime_intro_ogg": str(runtime_paths["intro"].relative_to(REPO)),
            "runtime_loop_ogg": str(runtime_paths["loop"].relative_to(REPO)),
            "cue_sheet": "cue_sheet.csv",
            "asset_request": "asset_request.yaml",
            "qc_result": "qc_result.json",
            "stems": {stem: str(path.relative_to(package_dir)) for stem, path in stem_paths.items()},
        },
        "loop": {
            "full_master_start_sample": spec.loop_start_sample,
            "full_master_end_sample": spec.loop_end_sample,
            "full_master_start_seconds": metrics["loop_start_seconds"],
            "full_master_end_seconds": metrics["loop_end_seconds"],
            "loop_clip_start_sample": 0,
            "loop_clip_end_sample": spec.loop_length_samples,
            "loop_clip_length_seconds": metrics["loop_length_seconds"],
            "intro_bars": spec.intro_bars,
            "loop_bars": spec.loop_bars,
            "playback_instruction": "Play runtime_intro_ogg once for phase entry, then loop runtime_loop_ogg from sample 0.",
        },
        "mix_notes": {
            "loudness_note": f"Peak {metrics['peak_dbfs']:.2f} dBFS, RMS {metrics['rms_dbfs']:.2f} dBFS.",
            "fade_note": "No baked loop fade. Optional engine crossfade 150-300 ms on phase transition.",
            "sfx_space_note": "Music avoids heavy impacts so boss tells, impacts, shield breaks, and transition SFX stay in front.",
            "low_end_note": "No sub/kick layer; roots are short and gain-limited.",
            "melody_note": "Lead is limited to short warning callouts.",
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
        "planned_companion_cues": companion_cues(spec.asset_id, spec.scene_type),
    }
    write_text_to_outputs(package_dir, docs_dir, "asset.json", json.dumps(data, indent=2) + "\n")


def companion_cues(asset_id: str, scene_type: str) -> list[dict[str, str]]:
    cues = [
        ("title", "planned", ""),
        ("field exploration", "generated_pilot", "audio.music.bgm_region01"),
        ("battle", "generated_pilot", "audio.music.bgm_normal_combat"),
        ("boss phase1", "generated_pilot" if scene_type == "boss phase1" else "planned", asset_id if scene_type == "boss phase1" else ""),
        ("boss phase2", "generated_pilot" if scene_type == "boss phase2" else "planned", asset_id if scene_type == "boss phase2" else ""),
        ("rest room", "planned", ""),
        ("victory stinger", "planned", ""),
        ("defeat stinger", "planned", ""),
    ]
    result = []
    for structure, status, cue_asset_id in cues:
        entry = {"structure": structure, "status": status}
        if cue_asset_id:
            entry["asset_id"] = cue_asset_id
        result.append(entry)
    return result


def write_qc_result(spec: TrackSpec, metrics: dict[str, object], package_dir: Path, docs_dir: Path) -> None:
    result = {
        "asset_id": spec.asset_id,
        "qc_status": "pass_with_notes",
        "checks": [
            {"name": "loop_point_recorded", "status": "pass", "detail": f"loop_start_sample={spec.loop_start_sample}, loop_end_sample={spec.loop_end_sample}"},
            {"name": "loop_boundary_click_risk", "status": "pass" if metrics["loop_boundary_delta_peak"] < 0.012 else "review", "detail": f"loop clip first/last sample delta {metrics['loop_boundary_delta_peak']:.6f}"},
            {"name": "loop_tail_release", "status": "pass" if metrics["loop_tail_peak"] < 0.030 else "review", "detail": f"last 1024 sample peak {metrics['loop_tail_peak']:.6f}"},
            {"name": "low_end_control", "status": "pass" if metrics["low_energy_ratio_20_160hz"] < 0.24 else "review", "detail": f"20-160 Hz energy ratio {metrics['low_energy_ratio_20_160hz']:.4f}"},
            {"name": "dialogue_band_control", "status": "pass" if metrics["dialogue_band_ratio_1_4khz"] < 0.35 else "review", "detail": f"1-4 kHz energy ratio {metrics['dialogue_band_ratio_1_4khz']:.4f}"},
            {"name": "headroom", "status": "pass" if metrics["peak_dbfs"] <= -4.5 else "review", "detail": f"peak {metrics['peak_dbfs']:.2f} dBFS"},
            {"name": "boss_sfx_space", "status": "pass", "detail": "No music impact layer; boss tells and hits remain priority transients."},
        ],
        "metrics": metrics,
        "notes": [
            "Procedural pilot only; requires listening review in a boss scene before Steam capture.",
            "Runtime is split into intro and loop clips so Unity does not need sub-sample loop metadata for this pilot.",
        ],
    }
    write_text_to_outputs(package_dir, docs_dir, "qc_result.json", json.dumps(result, indent=2) + "\n")


def write_text_to_outputs(package_dir: Path, docs_dir: Path, filename: str, text: str) -> None:
    (package_dir / filename).write_text(text, encoding="utf-8")
    (docs_dir / filename).write_text(text, encoding="utf-8")


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


def package_dir_for(spec: TrackSpec) -> Path:
    return REPO / "artifacts" / "Audio" / "BGM" / "Runtime" / "Audio" / "BGM" / spec.base_name


def docs_dir_for(spec: TrackSpec) -> Path:
    return REPO / "docs" / "Audio" / "BGM" / spec.base_name


def amp_to_db(value: float) -> float:
    return 20.0 * math.log10(max(value, 1.0e-12))


def db_to_amp(value: float) -> float:
    return 10.0 ** (value / 20.0)


if __name__ == "__main__":
    main()
