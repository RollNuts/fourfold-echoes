using System;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    [Serializable]
    public sealed class FourfoldSaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string currentScene = "Hub_Crossroads";
        public string hubSpawnId = "default";
        public bool toolOwned = true;
        public bool finalBossDefeated;
        public string[] regionsUnlocked = new[] { "Region_01_GreenRuins" };
        public string[] bossDefeated = Array.Empty<string>();
        public string[] shortcutsOpened = Array.Empty<string>();
        public string[] relicsClaimed = Array.Empty<string>();
        public FourfoldSaveSettings settings = new FourfoldSaveSettings();

        public static FourfoldSaveData CreateNewGame()
        {
            var data = new FourfoldSaveData();
            data.Normalize();
            return data;
        }

        public void Normalize()
        {
            version = CurrentVersion;
            currentScene = string.IsNullOrWhiteSpace(currentScene) ? "Hub_Crossroads" : currentScene;
            hubSpawnId = string.IsNullOrWhiteSpace(hubSpawnId) ? "default" : hubSpawnId;
            regionsUnlocked = NormalizeFlags(regionsUnlocked);
            bossDefeated = NormalizeFlags(bossDefeated);
            shortcutsOpened = NormalizeFlags(shortcutsOpened);
            relicsClaimed = NormalizeFlags(relicsClaimed);
            if (settings == null)
            {
                settings = new FourfoldSaveSettings();
            }
            settings.Normalize();
        }

        public bool IsRegionUnlocked(string id)
        {
            return HasFlag(regionsUnlocked, id);
        }

        public void SetRegionUnlocked(string id, bool unlocked)
        {
            regionsUnlocked = SetFlag(regionsUnlocked, id, unlocked);
        }

        public bool IsBossDefeated(string id)
        {
            return HasFlag(bossDefeated, id);
        }

        public void SetBossDefeated(string id, bool defeated)
        {
            bossDefeated = SetFlag(bossDefeated, id, defeated);
        }

        public bool IsShortcutOpened(string id)
        {
            return HasFlag(shortcutsOpened, id);
        }

        public void SetShortcutOpened(string id, bool opened)
        {
            shortcutsOpened = SetFlag(shortcutsOpened, id, opened);
        }

        public bool IsRelicClaimed(string id)
        {
            return HasFlag(relicsClaimed, id);
        }

        public void SetRelicClaimed(string id, bool claimed)
        {
            relicsClaimed = SetFlag(relicsClaimed, id, claimed);
        }

        private static bool HasFlag(string[] flags, string id)
        {
            if (flags == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            for (var i = 0; i < flags.Length; i++)
            {
                if (string.Equals(flags[i], id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] SetFlag(string[] flags, string id, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NormalizeFlags(flags);
            }

            flags = NormalizeFlags(flags);
            if (enabled)
            {
                if (HasFlag(flags, id))
                {
                    return flags;
                }

                var expanded = new string[flags.Length + 1];
                Array.Copy(flags, expanded, flags.Length);
                expanded[expanded.Length - 1] = id;
                return expanded;
            }

            var removeIndex = -1;
            for (var i = 0; i < flags.Length; i++)
            {
                if (string.Equals(flags[i], id, StringComparison.Ordinal))
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex < 0)
            {
                return flags;
            }

            var reduced = new string[flags.Length - 1];
            for (int source = 0, target = 0; source < flags.Length; source++)
            {
                if (source == removeIndex)
                {
                    continue;
                }

                reduced[target] = flags[source];
                target++;
            }

            return reduced;
        }

        private static string[] NormalizeFlags(string[] flags)
        {
            if (flags == null || flags.Length == 0)
            {
                return Array.Empty<string>();
            }

            var normalized = new string[flags.Length];
            var count = 0;
            for (var i = 0; i < flags.Length; i++)
            {
                var flag = flags[i];
                if (string.IsNullOrWhiteSpace(flag) || Contains(normalized, count, flag))
                {
                    continue;
                }

                normalized[count] = flag;
                count++;
            }

            if (count == normalized.Length)
            {
                return normalized;
            }

            var compact = new string[count];
            Array.Copy(normalized, compact, count);
            return compact;
        }

        private static bool Contains(string[] values, int count, string value)
        {
            for (var i = 0; i < count; i++)
            {
                if (string.Equals(values[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class FourfoldSaveSettings
    {
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public bool fullscreen = true;

        public void Normalize()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
        }
    }
}
