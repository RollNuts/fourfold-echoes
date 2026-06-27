using UnityEngine;

namespace FourfoldEchoes.Product
{
    public static class FourfoldUiAudio
    {
        private const int SampleRate = 24000;

        private static AudioClip selectClip;
        private static AudioClip confirmClip;
        private static AudioClip backClip;
        private static AudioClip errorClip;
        private static AudioClip pauseClip;

        public static void PlaySelect(Component owner, FourfoldProgressData data)
        {
            Play(owner, data, ref selectClip, "fourfold_ui_select", 620f, 760f, 0.045f, 0.16f);
        }

        public static void PlayConfirm(Component owner, FourfoldProgressData data)
        {
            Play(owner, data, ref confirmClip, "fourfold_ui_confirm", 700f, 1080f, 0.070f, 0.20f);
        }

        public static void PlayBack(Component owner, FourfoldProgressData data)
        {
            Play(owner, data, ref backClip, "fourfold_ui_back", 520f, 340f, 0.060f, 0.16f);
        }

        public static void PlayError(Component owner, FourfoldProgressData data)
        {
            Play(owner, data, ref errorClip, "fourfold_ui_error", 220f, 130f, 0.085f, 0.18f);
        }

        public static void PlayPause(Component owner, FourfoldProgressData data)
        {
            Play(owner, data, ref pauseClip, "fourfold_ui_pause", 360f, 620f, 0.075f, 0.18f);
        }

        private static void Play(Component owner, FourfoldProgressData data, ref AudioClip clip, string clipName, float startHz, float endHz, float seconds, float volume)
        {
            if (!Application.isPlaying || owner == null || owner.gameObject == null)
            {
                return;
            }

            if (clip == null)
            {
                clip = BuildTone(clipName, startHz, endHz, seconds);
            }

            var source = owner.GetComponent<AudioSource>();
            if (source == null)
            {
                source = owner.gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.ignoreListenerPause = true;
            source.PlayOneShot(clip, Mathf.Clamp01(volume * MasterSfxVolume(data)));
        }

        private static float MasterSfxVolume(FourfoldProgressData data)
        {
            if (data == null)
            {
                return 1f;
            }

            return Mathf.Clamp01(data.masterVolume) * Mathf.Clamp01(data.sfxVolume);
        }

        private static AudioClip BuildTone(string clipName, float startHz, float endHz, float seconds)
        {
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(SampleRate * seconds));
            var samples = new float[sampleCount];
            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = sampleCount <= 1 ? 1f : i / (sampleCount - 1f);
                var frequency = Mathf.Lerp(startHz, endHz, t);
                phase += frequency / SampleRate;
                var envelope = Mathf.Sin(Mathf.PI * t);
                samples[i] = Mathf.Sin(phase * Mathf.PI * 2f) * envelope;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
