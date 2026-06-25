#!/usr/bin/env node
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const sampleRate = 44100;
const outputDir = path.join(repo, "Assets/Audio/Generated");

const cues = [
  { file: "attack_basic.wav", seconds: 0.28, render: renderAttack },
  { file: "hit_enemy.wav", seconds: 0.24, render: renderHit },
  { file: "dodge.wav", seconds: 0.22, render: renderDodge },
  { file: "relic_pickup.wav", seconds: 0.42, render: renderRelicPickup },
  { file: "tool_pulse.wav", seconds: 0.48, render: renderToolPulse },
  { file: "shortcut_open.wav", seconds: 0.56, render: renderShortcutOpen },
  { file: "discovery_stinger.wav", seconds: 0.72, render: renderDiscovery }
];

fs.mkdirSync(outputDir, { recursive: true });

for (const cue of cues) {
  const samples = renderCue(cue.seconds, cue.render);
  const absolutePath = path.join(outputDir, cue.file);
  fs.writeFileSync(absolutePath, buildWav(samples));
  writeUnityMeta(absolutePath);
  console.log(`[audio] wrote ${path.relative(repo, absolutePath)}`);
}

function renderCue(seconds, renderer) {
  const count = Math.max(1, Math.floor(seconds * sampleRate));
  const samples = new Int16Array(count);
  let seed = 0x466f7572;

  const random = () => {
    seed = (1664525 * seed + 1013904223) >>> 0;
    return seed / 0xffffffff;
  };

  for (let index = 0; index < count; index++) {
    const time = index / sampleRate;
    const progress = index / (count - 1);
    const value = clamp(renderer(time, progress, random), -0.98, 0.98);
    samples[index] = Math.round(value * 32767);
  }

  return samples;
}

function renderAttack(time, progress, random) {
  const sweep = sine(chirp(time, 920, 180, progress)) * envelope(progress, 0.015, 0.78);
  const edge = noise(random) * envelope(progress, 0.006, 0.6) * (1 - progress) * 0.22;
  const click = sine(1800 * time) * envelope(progress, 0.002, 0.08) * 0.18;
  return (sweep * 0.42) + edge + click;
}

function renderHit(time, progress, random) {
  const body = sine(chirp(time, 190, 62, progress)) * envelope(progress, 0.004, 0.5) * 0.48;
  const crack = noise(random) * envelope(progress, 0.002, 0.24) * 0.2;
  const grit = sine(780 * time + sine(70 * time) * 1.8) * envelope(progress, 0.01, 0.32) * 0.1;
  return body + crack + grit;
}

function renderDodge(time, progress, random) {
  const whoosh = noise(random) * Math.sin(Math.PI * progress) * 0.24;
  const lift = sine(chirp(time, 320, 640, progress)) * envelope(progress, 0.02, 0.72) * 0.12;
  const tail = sine(140 * time) * (1 - progress) * 0.05;
  return whoosh + lift + tail;
}

function renderRelicPickup(time, progress) {
  const bellA = sine(659.25 * time) * envelope(progress, 0.006, 0.86) * 0.24;
  const bellB = sine(987.77 * time) * envelope(progress, 0.012, 0.72) * 0.18;
  const bellC = sine(1318.51 * time) * envelope(progress, 0.018, 0.62) * 0.1;
  return (bellA + bellB + bellC) * (1 - progress * 0.55);
}

function renderToolPulse(time, progress, random) {
  const shimmer = sine(chirp(time, 240, 760, progress) + sine(7 * time) * 1.6) * Math.sin(Math.PI * progress) * 0.2;
  const undertone = sine(55 * time) * envelope(progress, 0.03, 0.8) * 0.16;
  const sparkle = noise(random) * Math.sin(Math.PI * progress) * 0.08;
  return shimmer + undertone + sparkle;
}

function renderShortcutOpen(time, progress, random) {
  const stone = sine(chirp(time, 92, 148, progress)) * envelope(progress, 0.01, 0.72) * 0.28;
  const gear = sine(310 * time + sine(18 * time) * 0.9) * Math.sin(Math.PI * progress) * 0.12;
  const grit = noise(random) * envelope(progress, 0.02, 0.58) * 0.1;
  return stone + gear + grit;
}

function renderDiscovery(time, progress) {
  const root = sine(261.63 * time) * envelope(progress, 0.018, 0.9) * 0.18;
  const fifth = sine(392 * time) * envelope(Math.max(0, progress - 0.12), 0.02, 0.76) * 0.18;
  const octave = sine(523.25 * time) * envelope(Math.max(0, progress - 0.24), 0.025, 0.64) * 0.16;
  const air = sine(1046.5 * time) * envelope(Math.max(0, progress - 0.36), 0.03, 0.52) * 0.08;
  return (root + fifth + octave + air) * (1 - progress * 0.38);
}

function sine(phase) {
  return Math.sin(2 * Math.PI * phase);
}

function chirp(time, startHz, endHz, progress) {
  const hz = startHz + (endHz - startHz) * progress;
  return hz * time;
}

function envelope(progress, attack, releaseStart) {
  const attackGain = attack <= 0 ? 1 : clamp(progress / attack, 0, 1);
  const releaseGain = progress <= releaseStart ? 1 : clamp(1 - ((progress - releaseStart) / (1 - releaseStart)), 0, 1);
  return attackGain * releaseGain;
}

function noise(random) {
  return random() * 2 - 1;
}

function clamp(value, minimum, maximum) {
  return Math.min(maximum, Math.max(minimum, value));
}

function buildWav(samples) {
  const dataLength = samples.length * 2;
  const buffer = Buffer.alloc(44 + dataLength);
  buffer.write("RIFF", 0);
  buffer.writeUInt32LE(36 + dataLength, 4);
  buffer.write("WAVE", 8);
  buffer.write("fmt ", 12);
  buffer.writeUInt32LE(16, 16);
  buffer.writeUInt16LE(1, 20);
  buffer.writeUInt16LE(1, 22);
  buffer.writeUInt32LE(sampleRate, 24);
  buffer.writeUInt32LE(sampleRate * 2, 28);
  buffer.writeUInt16LE(2, 32);
  buffer.writeUInt16LE(16, 34);
  buffer.write("data", 36);
  buffer.writeUInt32LE(dataLength, 40);

  for (let index = 0; index < samples.length; index++) {
    buffer.writeInt16LE(samples[index], 44 + index * 2);
  }

  return buffer;
}

function writeUnityMeta(absolutePath) {
  const relativePath = path.relative(repo, absolutePath).replaceAll(path.sep, "/");
  const metaPath = `${absolutePath}.meta`;
  const guid = crypto.createHash("md5").update(`fourfold-echoes:${relativePath}`).digest("hex");
  const text = `fileFormatVersion: 2
guid: ${guid}
AudioImporter:
  externalObjects: {}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: ${sampleRate}
    compressionFormat: 1
    quality: 1
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {}
  forceToMono: 1
  normalize: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 1
  userData:
  assetBundleName:
  assetBundleVariant:
`;
  fs.writeFileSync(metaPath, text);
}
