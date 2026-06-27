using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020ProgressSave : MonoBehaviour
    {
        private const int SaveVersion = 1;
        private const string DefaultSaveFileName = "d020-progress.json";

        public string saveFileName = DefaultSaveFileName;
        public string overrideFilePath;
        public bool loadOnAwake = true;
        public bool saveOnProgressChanged = true;
        public ExplorationNode[] nodes;
        public string[] nodeIds;
        public D020RelicReward[] rewards;
        public string[] rewardIds;

        private int lastSolvedNodeCount = -1;
        private int lastCollectedRewardCount = -1;

        public int SaveCount { get; private set; }
        public int LoadCount { get; private set; }
        public string LastError { get; private set; }
        public int SolvedNodeCount => CountSolvedNodes();
        public int CollectedRewardCount => CountCollectedRewards();

        private void Awake()
        {
            if (loadOnAwake)
            {
                LoadNow();
            }

            CaptureSnapshot();
        }

        private void Update()
        {
            if (!saveOnProgressChanged || !HasProgressChanged())
            {
                return;
            }

            if (SaveNow())
            {
                CaptureSnapshot();
            }
        }

        public bool SaveNow()
        {
            LastError = string.Empty;

            try
            {
                var path = ResolveSavePath();
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, JsonUtility.ToJson(BuildData(), true));
                SaveCount++;
                CaptureSnapshot();
                return true;
            }
            catch (Exception exception) when (exception is IOException ||
                                             exception is UnauthorizedAccessException ||
                                             exception is ArgumentException ||
                                             exception is NotSupportedException)
            {
                RecordError("D-020 progress save failed to write.", exception);
                return false;
            }
        }

        public bool LoadNow()
        {
            LastError = string.Empty;
            var path = ResolveSavePath();
            if (!File.Exists(path))
            {
                CaptureSnapshot();
                return false;
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data == null || data.version != SaveVersion)
                {
                    LastError = "D-020 progress save version is unsupported.";
                    return false;
                }

                ApplyData(data);
                LoadCount++;
                CaptureSnapshot();
                return true;
            }
            catch (Exception exception) when (exception is IOException ||
                                             exception is UnauthorizedAccessException ||
                                             exception is ArgumentException ||
                                             exception is NotSupportedException)
            {
                RecordError("D-020 progress save failed to read.", exception);
                return false;
            }
        }

        public bool ClearSave(bool resetSceneState = true)
        {
            LastError = string.Empty;

            if (resetSceneState)
            {
                ApplyData(new SaveData());
            }

            var path = ResolveSavePath();
            if (!File.Exists(path))
            {
                CaptureSnapshot();
                return true;
            }

            try
            {
                File.Delete(path);
                CaptureSnapshot();
                return true;
            }
            catch (Exception exception) when (exception is IOException ||
                                             exception is UnauthorizedAccessException ||
                                             exception is ArgumentException ||
                                             exception is NotSupportedException)
            {
                RecordError("D-020 progress save failed to clear.", exception);
                return false;
            }
        }

        private SaveData BuildData()
        {
            return new SaveData
            {
                version = SaveVersion,
                solvedNodeIds = CollectSolvedNodeIds(),
                collectedRewardIds = CollectRewardIds()
            };
        }

        private void ApplyData(SaveData data)
        {
            var solved = ToSet(data.solvedNodeIds);
            var collectedRewards = ToSet(data.collectedRewardIds);

            if (nodes != null)
            {
                for (var i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    if (node != null)
                    {
                        node.SetSolved(solved.Contains(GetNodeId(i, node)));
                    }
                }
            }

            if (rewards == null)
            {
                return;
            }

            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward != null)
                {
                    reward.SetCollected(collectedRewards.Contains(GetRewardId(i, reward)));
                }
            }
        }

        private string[] CollectSolvedNodeIds()
        {
            var ids = new List<string>();
            if (nodes == null)
            {
                return ids.ToArray();
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node != null && node.IsSolved)
                {
                    ids.Add(GetNodeId(i, node));
                }
            }

            return ids.ToArray();
        }

        private string[] CollectRewardIds()
        {
            var ids = new List<string>();
            if (rewards == null)
            {
                return ids.ToArray();
            }

            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward != null && reward.IsCollected)
                {
                    ids.Add(GetRewardId(i, reward));
                }
            }

            return ids.ToArray();
        }

        private int CountSolvedNodes()
        {
            if (nodes == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null && nodes[i].IsSolved)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountCollectedRewards()
        {
            if (rewards == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < rewards.Length; i++)
            {
                if (rewards[i] != null && rewards[i].IsCollected)
                {
                    count++;
                }
            }

            return count;
        }

        private bool HasProgressChanged()
        {
            return CountSolvedNodes() != lastSolvedNodeCount || CountCollectedRewards() != lastCollectedRewardCount;
        }

        private void CaptureSnapshot()
        {
            lastSolvedNodeCount = CountSolvedNodes();
            lastCollectedRewardCount = CountCollectedRewards();
        }

        private string ResolveSavePath()
        {
            if (!string.IsNullOrWhiteSpace(overrideFilePath))
            {
                return overrideFilePath;
            }

            var fileName = string.IsNullOrWhiteSpace(saveFileName) ? DefaultSaveFileName : saveFileName;
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        private string GetNodeId(int index, ExplorationNode node)
        {
            if (nodeIds != null && index < nodeIds.Length && !string.IsNullOrWhiteSpace(nodeIds[index]))
            {
                return nodeIds[index];
            }

            return node != null ? node.name : $"node.{index}";
        }

        private string GetRewardId(int index, D020RelicReward reward)
        {
            if (rewardIds != null && index < rewardIds.Length && !string.IsNullOrWhiteSpace(rewardIds[index]))
            {
                return rewardIds[index];
            }

            if (reward != null && !string.IsNullOrWhiteSpace(reward.rewardId))
            {
                return reward.rewardId;
            }

            return $"reward.{index}";
        }

        private HashSet<string> ToSet(string[] values)
        {
            var set = new HashSet<string>();
            if (values == null)
            {
                return set;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    set.Add(values[i]);
                }
            }

            return set;
        }

        private void RecordError(string message, Exception exception)
        {
            LastError = $"{message} ({exception.GetType().Name})";
            Debug.LogWarning(LastError, this);
        }

        [Serializable]
        private sealed class SaveData
        {
            public int version = SaveVersion;
            public string[] solvedNodeIds = Array.Empty<string>();
            public string[] collectedRewardIds = Array.Empty<string>();
        }
    }
}
