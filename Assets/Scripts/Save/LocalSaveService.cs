using System;
using System.IO;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class LocalSaveService
    {
        public const string DefaultFileName = "fourfold-save.json";

        private readonly string savePath;

        public LocalSaveService(string savePath)
        {
            if (string.IsNullOrWhiteSpace(savePath))
            {
                throw new ArgumentException("Save path must be a concrete local file path.", nameof(savePath));
            }

            this.savePath = savePath;
        }

        public static LocalSaveService CreateDefault()
        {
            return new LocalSaveService(Path.Combine(Application.persistentDataPath, DefaultFileName));
        }

        public string SavePath => savePath;

        public FourfoldSaveData LoadOrCreate()
        {
            return TryLoad(out var data) ? data : FourfoldSaveData.CreateNewGame();
        }

        public bool TryLoad(out FourfoldSaveData data)
        {
            data = null;
            if (!File.Exists(savePath))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(savePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                data = JsonUtility.FromJson<FourfoldSaveData>(json);
                if (data == null || data.version <= 0)
                {
                    data = null;
                    return false;
                }

                data.Normalize();
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
            {
                data = null;
                return false;
            }
        }

        public void Save(FourfoldSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.Normalize();
            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonUtility.ToJson(data, true);
            var tempPath = savePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, savePath, true);
            File.Delete(tempPath);
        }

        public void Delete()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
        }
    }
}
