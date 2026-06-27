using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldSaveVerifier
    {
        public static void VerifySaveRoundtripAndRecovery()
        {
            var savePath = FourfoldProgressSave.SavePath();
            var backupPath = savePath + ".bak";
            byte[] previousSaveBytes = null;
            byte[] previousBackupBytes = null;
            var backupExisted = File.Exists(backupPath);

            if (File.Exists(savePath))
            {
                previousSaveBytes = File.ReadAllBytes(savePath);
            }

            if (backupExisted)
            {
                previousBackupBytes = File.ReadAllBytes(backupPath);
            }

            try
            {
                DeleteIfExists(savePath);
                DeleteIfExists(backupPath);

                var empty = FourfoldProgressSave.Load();
                if (empty.version != FourfoldProgressSave.CurrentVersion || !empty.settingsInitialized)
                {
                    throw new InvalidOperationException("Save verifier failed: empty save did not initialize version or settings.");
                }

                if (!ApproximatelyOne(empty.masterVolume)
                    || !ApproximatelyOne(empty.musicVolume)
                    || !ApproximatelyOne(empty.sfxVolume)
                    || !ApproximatelyOne(empty.uiScale)
                    || !empty.showControlHints
                    || empty.language != FourfoldLanguage.English)
                {
                    throw new InvalidOperationException("Save verifier failed: empty save did not initialize default settings.");
                }

                var data = new FourfoldProgressData
                {
                    currentScene = FourfoldGameIds.SceneD020VerticalSlice,
                    hubUnlocked = true,
                    regionD020Unlocked = true,
                    lumenRodUnlocked = true,
                    d020ShortcutOpened = true,
                    d020RewardClaimed = true,
                    d020SecondRewardClaimed = true,
                    d020LoadoutInitialized = true,
                    d020EdgeEquipped = true,
                    d020WardEquipped = false,
                    d020FailureCount = 3,
                    d020AcknowledgedFailureCount = 2,
                    settingsInitialized = true,
                    masterVolume = 0.7f,
                    musicVolume = 0.5f,
                    sfxVolume = 0.9f,
                    uiScale = 1.15f,
                    showControlHints = false,
                    language = FourfoldLanguage.Japanese
                };
                FourfoldProgressSave.Save(data);

                var roundtrip = FourfoldProgressSave.Load();
                if (roundtrip.currentScene != FourfoldGameIds.SceneD020VerticalSlice
                    || !roundtrip.hubUnlocked
                    || !roundtrip.regionD020Unlocked
                    || !roundtrip.lumenRodUnlocked
                    || !roundtrip.d020ShortcutOpened
                    || !roundtrip.d020RewardClaimed
                    || !roundtrip.d020SecondRewardClaimed
                    || !roundtrip.d020LoadoutInitialized
                    || !roundtrip.d020EdgeEquipped
                    || roundtrip.d020WardEquipped
                    || roundtrip.d020FailureCount != 3
                    || roundtrip.d020AcknowledgedFailureCount != 2
                    || !Approximately(roundtrip.masterVolume, 0.7f)
                    || !Approximately(roundtrip.musicVolume, 0.5f)
                    || !Approximately(roundtrip.sfxVolume, 0.9f)
                    || !Approximately(roundtrip.uiScale, 1.15f)
                    || roundtrip.language != FourfoldLanguage.Japanese
                    || roundtrip.showControlHints)
                {
                    throw new InvalidOperationException("Save verifier failed: save/load roundtrip did not preserve progress and settings.");
                }

                var resetProgress = new FourfoldProgressData();
                FourfoldProgressSave.CopySettings(roundtrip, resetProgress);
                if (!Approximately(resetProgress.masterVolume, 0.7f)
                    || !Approximately(resetProgress.musicVolume, 0.5f)
                    || !Approximately(resetProgress.sfxVolume, 0.9f)
                    || !Approximately(resetProgress.uiScale, 1.15f)
                    || resetProgress.language != FourfoldLanguage.Japanese
                    || resetProgress.showControlHints)
                {
                    throw new InvalidOperationException("Save verifier failed: settings copy did not preserve user UX preferences.");
                }

                FourfoldProgressSave.Save(roundtrip);
                if (!File.Exists(backupPath))
                {
                    throw new InvalidOperationException("Save verifier failed: second save did not create a backup file.");
                }

                File.WriteAllText(savePath, "{ not valid json");
                var recovered = FourfoldProgressSave.Load();
                if (recovered.currentScene != FourfoldGameIds.SceneD020VerticalSlice || recovered.d020FailureCount != 3 || recovered.d020AcknowledgedFailureCount != 2)
                {
                    throw new InvalidOperationException("Save verifier failed: corrupt primary save did not recover from backup.");
                }

                DeleteIfExists(savePath);
                DeleteIfExists(backupPath);
                File.WriteAllText(savePath, "{ still not valid json");
                var fallback = FourfoldProgressSave.Load();
                if (fallback.version != FourfoldProgressSave.CurrentVersion
                    || fallback.d020FailureCount != 0
                    || !fallback.settingsInitialized
                    || fallback.language != FourfoldLanguage.English)
                {
                    throw new InvalidOperationException("Save verifier failed: corrupt save without backup did not fall back to clean progress.");
                }

                Debug.Log("FOURFOLD save verifier passed: defaults, language setting, roundtrip, backup recovery, and corrupt fallback work.");
            }
            finally
            {
                RestoreFile(savePath, previousSaveBytes);
                if (backupExisted)
                {
                    RestoreFile(backupPath, previousBackupBytes);
                }
                else
                {
                    DeleteIfExists(backupPath);
                }
            }
        }

        private static bool ApproximatelyOne(float value)
        {
            return Approximately(value, 1f);
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.0001f;
        }

        private static void RestoreFile(string path, byte[] bytes)
        {
            if (bytes == null)
            {
                DeleteIfExists(path);
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, bytes);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
