using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020ProgressSave : MonoBehaviour
    {
        private const int CurrentVersion = 1;

        [Header("Storage")]
        public string saveFileName = "d020-progress.json";
        public string overrideFilePath;
        public bool loadOnAwake = true;
        public bool saveOnSolved = true;

        [Header("Progress Flags")]
        public ExplorationNode[] nodes;
        public string[] nodeIds;

        private readonly HashSet<string> solvedIds = new HashSet<string>();
        private bool[] knownSolved;

        public int SaveCount { get; private set; }
        public int LoadCount { get; private set; }
        public int SolvedCount => solvedIds.Count;
        public string SavePath => GetSavePath();

        private void Awake()
        {
            EnsureKnownState();

            if (loadOnAwake)
            {
                LoadNow();
            }
        }

        private void Update()
        {
            if (saveOnSolved && SyncSolvedFromNodes())
            {
                SaveNow();
            }
        }

        public bool SaveNow()
        {
            try
            {
                SyncSolvedFromNodes();
                var path = GetSavePath();
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var data = new SaveData
                {
                    version = CurrentVersion,
                    solvedNodeIds = ToSortedArray(solvedIds)
                };
                File.WriteAllText(path, JsonUtility.ToJson(data, true));
                SaveCount++;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"D-020 progress save failed: {ex.Message}");
                return false;
            }
        }

        public bool LoadNow()
        {
            try
            {
                var path = GetSavePath();
                if (!File.Exists(path))
                {
                    return false;
                }

                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data == null || data.version != CurrentVersion)
                {
                    Debug.LogWarning($"D-020 progress save ignored unsupported data at {path}");
                    return false;
                }

                solvedIds.Clear();
                if (data.solvedNodeIds != null)
                {
                    for (var i = 0; i < data.solvedNodeIds.Length; i++)
                    {
                        var id = data.solvedNodeIds[i];
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            solvedIds.Add(id);
                        }
                    }
                }

                ApplySolvedToNodes();
                RefreshKnownState();
                LoadCount++;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"D-020 progress load failed: {ex.Message}");
                return false;
            }
        }

        public void ClearSave()
        {
            solvedIds.Clear();
            ApplySolvedToNodes();
            RefreshKnownState();

            var path = GetSavePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public bool SyncSolvedFromNodes()
        {
            EnsureKnownState();

            if (nodes == null)
            {
                return false;
            }

            var changed = false;
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var solved = node != null && node.IsSolved;
                if (knownSolved[i] == solved)
                {
                    continue;
                }

                knownSolved[i] = solved;
                changed = true;
                var id = GetNodeId(i);
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (solved)
                {
                    solvedIds.Add(id);
                }
                else
                {
                    solvedIds.Remove(id);
                }
            }

            return changed;
        }

        private void ApplySolvedToNodes()
        {
            if (nodes == null)
            {
                return;
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node != null)
                {
                    node.SetSolved(solvedIds.Contains(GetNodeId(i)));
                }
            }
        }

        private void EnsureKnownState()
        {
            var length = nodes == null ? 0 : nodes.Length;
            if (knownSolved != null && knownSolved.Length == length)
            {
                return;
            }

            RefreshKnownState();
        }

        private void RefreshKnownState()
        {
            var length = nodes == null ? 0 : nodes.Length;
            knownSolved = new bool[length];
            for (var i = 0; i < length; i++)
            {
                knownSolved[i] = nodes[i] != null && nodes[i].IsSolved;
            }
        }

        private string GetNodeId(int index)
        {
            if (nodeIds != null && index < nodeIds.Length && !string.IsNullOrWhiteSpace(nodeIds[index]))
            {
                return nodeIds[index];
            }

            if (nodes != null && index < nodes.Length && nodes[index] != null)
            {
                return nodes[index].name;
            }

            return $"node-{index}";
        }

        private string GetSavePath()
        {
            if (!string.IsNullOrWhiteSpace(overrideFilePath))
            {
                return overrideFilePath;
            }

            return Path.Combine(Application.persistentDataPath, saveFileName);
        }

        private static string[] ToSortedArray(HashSet<string> values)
        {
            var output = new string[values.Count];
            values.CopyTo(output);
            Array.Sort(output, StringComparer.Ordinal);
            return output;
        }

        [Serializable]
        private sealed class SaveData
        {
            public int version;
            public string[] solvedNodeIds;
        }
    }
}
