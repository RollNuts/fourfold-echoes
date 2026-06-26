using System;
using System.IO;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    [Serializable]
    public sealed class FourfoldProgressData
    {
        public int version = FourfoldProgressSave.CurrentVersion;
        public string currentScene = string.Empty;
        public string lastCompletedRegion = string.Empty;
        public string hubSpawnId = string.Empty;
        public bool hubUnlocked;
        public bool regionD020Unlocked;
        public bool regionD020Cleared;
        public bool lumenRodUnlocked;
        public bool d020Cleared;
        public bool d020BossDefeated;
        public bool d020ShortcutOpened;
        public bool d020RewardClaimed;
        public bool d020SecondNodeOpened;
        public bool d020SecondRewardClaimed;
        public bool d020ReturnedToHub;
        public bool d020LoadoutInitialized;
        public bool d020EdgeEquipped;
        public bool d020WardEquipped;
        public int d020ClearCount;
        public int d020AcknowledgedClearCount;
        public int d020FailureCount;
        public float d020BestClearTimeSeconds;
        public bool settingsInitialized;
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public float uiScale = 1f;
        public bool showControlHints = true;
        public string language = FourfoldLanguage.English;
    }

    public static class FourfoldProgressSave
    {
        public const int CurrentVersion = 1;
        private const string FileName = "fourfold_progress_v1.json";

        public static bool HasSaveFile()
        {
            return File.Exists(SavePath());
        }

        public static FourfoldProgressData Load()
        {
            var path = SavePath();
            if (!File.Exists(path))
            {
                return NewData();
            }

            var backupPath = BackupPath();
            try
            {
                return LoadFromPath(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"FOURFOLD save load failed; trying backup. {exception.Message}");
                if (File.Exists(backupPath))
                {
                    try
                    {
                        var restored = LoadFromPath(backupPath);
                        File.Copy(backupPath, path, true);
                        return restored;
                    }
                    catch (Exception backupException)
                    {
                        Debug.LogWarning($"FOURFOLD backup save load failed; starting with empty progress. {backupException.Message}");
                    }
                }

                return NewData();
            }
        }

        public static void Save(FourfoldProgressData data)
        {
            var clean = Sanitize(data);
            var path = SavePath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = path + ".tmp";
            var backupPath = BackupPath();
            File.WriteAllText(tempPath, JsonUtility.ToJson(clean, true));

            if (File.Exists(path))
            {
                File.Copy(path, backupPath, true);
                File.Delete(path);
            }

            File.Move(tempPath, path);
        }

        public static void DeleteAll()
        {
            DeleteIfExists(SavePath());
            DeleteIfExists(BackupPath());
        }

        public static void CopySettings(FourfoldProgressData source, FourfoldProgressData target)
        {
            if (source == null || target == null)
            {
                return;
            }

            var clean = Sanitize(source);
            target.settingsInitialized = true;
            target.masterVolume = clean.masterVolume;
            target.musicVolume = clean.musicVolume;
            target.sfxVolume = clean.sfxVolume;
            target.uiScale = clean.uiScale;
            target.showControlHints = clean.showControlHints;
            target.language = clean.language;
        }

        public static string SavePath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }

        private static string BackupPath()
        {
            return SavePath() + ".bak";
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static FourfoldProgressData LoadFromPath(string path)
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<FourfoldProgressData>(json);
            return Sanitize(data);
        }

        private static FourfoldProgressData NewData()
        {
            return new FourfoldProgressData
            {
                version = CurrentVersion,
                settingsInitialized = true,
                masterVolume = 1f,
                musicVolume = 1f,
                sfxVolume = 1f,
                uiScale = 1f,
                showControlHints = true,
                language = FourfoldLanguage.English
            };
        }

        private static FourfoldProgressData Sanitize(FourfoldProgressData data)
        {
            if (data == null)
            {
                data = NewData();
            }

            data.version = CurrentVersion;
            data.currentScene = data.currentScene ?? string.Empty;
            data.lastCompletedRegion = data.lastCompletedRegion ?? string.Empty;
            data.hubSpawnId = data.hubSpawnId ?? string.Empty;
            data.d020ClearCount = Mathf.Max(0, data.d020ClearCount);
            data.d020AcknowledgedClearCount = Mathf.Clamp(data.d020AcknowledgedClearCount, 0, data.d020ClearCount);
            data.d020FailureCount = Mathf.Max(0, data.d020FailureCount);
            if (float.IsNaN(data.d020BestClearTimeSeconds) || float.IsInfinity(data.d020BestClearTimeSeconds))
            {
                data.d020BestClearTimeSeconds = 0f;
            }

            data.d020BestClearTimeSeconds = Mathf.Max(0f, data.d020BestClearTimeSeconds);
            if (!data.d020LoadoutInitialized)
            {
                data.d020EdgeEquipped = data.d020RewardClaimed;
                data.d020WardEquipped = data.d020SecondRewardClaimed;
                data.d020LoadoutInitialized = true;
            }

            if (!data.d020RewardClaimed)
            {
                data.d020EdgeEquipped = false;
            }

            if (!data.d020SecondRewardClaimed)
            {
                data.d020WardEquipped = false;
            }

            if (!data.settingsInitialized)
            {
                data.masterVolume = 1f;
                data.musicVolume = 1f;
                data.sfxVolume = 1f;
                data.uiScale = 1f;
                data.showControlHints = true;
                data.language = FourfoldLanguage.English;
                data.settingsInitialized = true;
            }

            data.masterVolume = SanitizeVolume(data.masterVolume, 1f);
            data.musicVolume = SanitizeVolume(data.musicVolume, 1f);
            data.sfxVolume = SanitizeVolume(data.sfxVolume, 1f);
            data.uiScale = SanitizeScale(data.uiScale, 1f);
            data.language = FourfoldLanguage.Sanitize(data.language);
            return data;
        }

        private static float SanitizeVolume(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return fallback;
            }

            return Mathf.Clamp01(value);
        }

        private static float SanitizeScale(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                return fallback;
            }

            return Mathf.Clamp(value, 0.85f, 1.25f);
        }
    }
}
