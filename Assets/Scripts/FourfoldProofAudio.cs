using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.Spike
{
    public enum FourfoldProofAudioCue
    {
        Attack,
        Hit,
        Dodge,
        PhaseAccent,
        AltarHeat,
        GateOpen,
        RoomClear,
        EnemyTell,
        PlayerHit,
        Reward
    }

    public sealed class FourfoldProofAudio : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private readonly Dictionary<FourfoldProofAudioCue, AudioClip> clips = new Dictionary<FourfoldProofAudioCue, AudioClip>();
        private AudioSource source;
        private bool disabled;

        public bool IsAvailable => !disabled && source != null;

        public void Play(FourfoldProofAudioCue cue, float volume = 1f)
        {
            Play(cue, volume, 1f);
        }

        public void PlayPhase(EchoPhase phase)
        {
            var pitch = phase switch
            {
                EchoPhase.Tide => 1.12f,
                EchoPhase.Bloom => 1.26f,
                EchoPhase.Prism => 1.41f,
                _ => 1f
            };
            Play(FourfoldProofAudioCue.PhaseAccent, 0.24f, pitch);
        }

        public void PlayAltarHeat(float heat01)
        {
            var clampedHeat = Mathf.Clamp01(heat01);
            Play(FourfoldProofAudioCue.AltarHeat, Mathf.Lerp(0.12f, 0.3f, clampedHeat), Mathf.Lerp(0.72f, 1.36f, clampedHeat));
        }

        private void Awake()
        {
            EnsureReady();
        }

        private void Play(FourfoldProofAudioCue cue, float volume, float pitch)
        {
            if (!EnsureReady() || !clips.TryGetValue(cue, out var clip) || clip == null)
            {
                return;
            }

            try
            {
                source.pitch = Mathf.Clamp(pitch, 0.5f, 1.8f);
                source.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
            catch (Exception exception)
            {
                disabled = true;
                Debug.LogWarning($"FOURFOLD procedural audio disabled: {exception.Message}");
            }
        }

        private bool EnsureReady()
        {
            if (disabled)
            {
                return false;
            }

            if (source != null && clips.Count > 0)
            {
                return true;
            }

            try
            {
                source = GetComponent<AudioSource>();
                if (source == null)
                {
                    source = gameObject.AddComponent<AudioSource>();
                }

                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.dopplerLevel = 0f;
                source.volume = 1f;
                BuildClips();
                return true;
            }
            catch (Exception exception)
            {
                disabled = true;
                Debug.LogWarning($"FOURFOLD procedural audio unavailable: {exception.Message}");
                return false;
            }
        }

        private void BuildClips()
        {
            clips.Clear();
            clips[FourfoldProofAudioCue.Attack] = CreateTone(
                "FOURFOLD_Attack_Procedural",
                new ToneSegment(238f, 0.055f, 0.2f),
                new ToneSegment(318f, 0.035f, 0.16f));
            clips[FourfoldProofAudioCue.Hit] = CreateTone(
                "FOURFOLD_Hit_Procedural",
                new ToneSegment(392f, 0.055f, 0.2f),
                new ToneSegment(588f, 0.07f, 0.16f));
            clips[FourfoldProofAudioCue.Dodge] = CreateTone(
                "FOURFOLD_Dodge_Procedural",
                new ToneSegment(560f, 0.04f, 0.11f),
                new ToneSegment(420f, 0.055f, 0.09f));
            clips[FourfoldProofAudioCue.PhaseAccent] = CreateTone(
                "FOURFOLD_Phase_Procedural",
                new ToneSegment(330f, 0.035f, 0.16f),
                new ToneSegment(495f, 0.04f, 0.13f));
            clips[FourfoldProofAudioCue.AltarHeat] = CreateTone(
                "FOURFOLD_AltarHeat_Procedural",
                new ToneSegment(196f, 0.055f, 0.14f),
                new ToneSegment(245f, 0.045f, 0.11f));
            clips[FourfoldProofAudioCue.GateOpen] = CreateTone(
                "FOURFOLD_GateOpen_Procedural",
                new ToneSegment(247f, 0.075f, 0.18f),
                new ToneSegment(330f, 0.075f, 0.18f),
                new ToneSegment(494f, 0.1f, 0.16f));
            clips[FourfoldProofAudioCue.RoomClear] = CreateTone(
                "FOURFOLD_RoomClear_Procedural",
                new ToneSegment(523f, 0.065f, 0.16f),
                new ToneSegment(659f, 0.075f, 0.15f),
                new ToneSegment(784f, 0.13f, 0.14f));
            clips[FourfoldProofAudioCue.EnemyTell] = CreateTone(
                "FOURFOLD_EnemyTell_Procedural",
                new ToneSegment(146f, 0.09f, 0.16f),
                new ToneSegment(110f, 0.08f, 0.12f));
            clips[FourfoldProofAudioCue.PlayerHit] = CreateTone(
                "FOURFOLD_PlayerHit_Procedural",
                new ToneSegment(92f, 0.1f, 0.2f),
                new ToneSegment(68f, 0.075f, 0.14f));
            clips[FourfoldProofAudioCue.Reward] = CreateTone(
                "FOURFOLD_Reward_Procedural",
                new ToneSegment(660f, 0.075f, 0.17f),
                new ToneSegment(880f, 0.11f, 0.15f),
                new ToneSegment(990f, 0.08f, 0.12f));
        }

        private static AudioClip CreateTone(string clipName, params ToneSegment[] segments)
        {
            var totalSamples = 0;
            for (var i = 0; i < segments.Length; i++)
            {
                totalSamples += Mathf.Max(1, Mathf.CeilToInt(SampleRate * segments[i].Duration));
            }

            var data = new float[totalSamples];
            var writeIndex = 0;
            for (var segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
            {
                var segment = segments[segmentIndex];
                var samples = Mathf.Max(1, Mathf.CeilToInt(SampleRate * segment.Duration));
                for (var sampleIndex = 0; sampleIndex < samples && writeIndex < data.Length; sampleIndex++)
                {
                    var t = sampleIndex / (float)SampleRate;
                    var progress = sampleIndex / (float)samples;
                    var attack = Mathf.Clamp01(progress / 0.12f);
                    var release = Mathf.Clamp01((1f - progress) / 0.85f);
                    var envelope = attack * release;
                    data[writeIndex] = Mathf.Sin(2f * Mathf.PI * segment.Frequency * t) * envelope * segment.Amplitude;
                    writeIndex++;
                }
            }

            var clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private readonly struct ToneSegment
        {
            public ToneSegment(float frequency, float duration, float amplitude)
            {
                Frequency = frequency;
                Duration = duration;
                Amplitude = amplitude;
            }

            public float Frequency { get; }
            public float Duration { get; }
            public float Amplitude { get; }
        }
    }
}
