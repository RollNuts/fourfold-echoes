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
        public bool d020Cleared;
        public bool d020ShortcutOpened;
        public bool d020RewardClaimed;
        public bool d020SecondNodeOpened;
        public bool d020SecondRewardClaimed;
        public bool d020ReturnedToHub;
        public int d020ClearCount;
        public int d020LumenEdgeStock;
        public int d020EquippedSkill;
        public int d020LostSkillCount;
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

        public static string SavePath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }

        private static string BackupPath()
        {
            return SavePath() + ".bak";
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
                version = CurrentVersion
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
            data.d020ClearCount = Mathf.Max(0, data.d020ClearCount);
            data.d020LumenEdgeStock = Mathf.Max(0, data.d020LumenEdgeStock);
            data.d020EquippedSkill = data.d020LumenEdgeStock > 0 ? data.d020EquippedSkill : 0;
            data.d020LostSkillCount = Mathf.Max(0, data.d020LostSkillCount);
            return data;
        }
    }
}
