using System;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020HudController : MonoBehaviour
    {
        public const string RoomTitleText = "D020 Vertical Slice";
        private const string RelicUnlockedFeedback = "Relic Unlocked";

        public bool showHud = true;
        public D020PlayerController player;
        public ExplorationTool tool;
        public ExplorationNode node;
        public ExplorationNode[] nodes;
        public D020RelicReward reward;
        public D020RelicReward[] rewards;
        public D020ProgressSave progressSave;

        private GUIStyle boxStyle;
        private GUIStyle titleStyle;
        private GUIStyle lineStyle;
        private Texture2D backgroundTexture;
        private int observedSaveCount = -1;
        private int observedLoadCount = -1;
        private float feedbackTimer;
        private string feedbackRead = string.Empty;
        private bool hasObservedRewardState;
        private int observedRewardUnlockedCount;
        private int observedRewardCollectedCount;

        public string ToolRead { get; private set; }
        public string RewardRead { get; private set; }
        public string ProgressRead { get; private set; }
        public string PromptRead { get; private set; }

        private void Awake()
        {
            RefreshNow();
            CaptureSaveCounters();
        }

        private void Update()
        {
            RefreshNow();
            UpdateSaveFeedback(Time.deltaTime);
        }

        public void RefreshNow()
        {
            UpdateRewardUnlockFeedback();
            ToolRead = BuildToolRead();
            RewardRead = BuildRewardRead();
            ProgressRead = BuildProgressRead();
            PromptRead = BuildPromptRead();
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            EnsureStyles();
            RefreshNow();

            var rect = new Rect(18f, 18f, 270f, 128f);
            GUI.Box(rect, GUIContent.none, boxStyle);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, 240f, 24f), RoomTitleText, titleStyle);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 36f, 240f, 22f), ToolRead, lineStyle);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 58f, 240f, 22f), RewardRead, lineStyle);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 80f, 240f, 22f), ProgressRead, lineStyle);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 102f, 240f, 22f), PromptRead, lineStyle);
        }

        private string BuildToolRead()
        {
            if (tool == null)
            {
                return "Tool --";
            }

            if (tool.IsReady)
            {
                return "Tool Ready";
            }

            var readyPercent = Mathf.RoundToInt((1f - tool.Cooldown01) * 100f);
            return $"Tool {Mathf.Clamp(readyPercent, 0, 99)}%";
        }

        private string BuildRewardRead()
        {
            var rewardCount = CountRewards();
            if (rewardCount == 0)
            {
                return "Relic --";
            }

            if (rewardCount == 1)
            {
                var singleReward = GetFirstReward();
                if (singleReward != null && singleReward.IsCollected)
                {
                    return "Relic Claimed";
                }

                return singleReward != null && singleReward.IsUnlocked ? "Relic Ready" : "Relic Locked";
            }

            var collectedCount = CountCollectedRewards();
            if (collectedCount >= rewardCount)
            {
                return $"Relics Claimed {collectedCount}/{rewardCount}";
            }

            return CountClaimableRewards() > 0
                ? $"Relic Ready {collectedCount}/{rewardCount}"
                : $"Relics Locked {collectedCount}/{rewardCount}";
        }

        private string BuildProgressRead()
        {
            if (progressSave == null)
            {
                return "Progress --";
            }

            if (feedbackTimer > 0f && !string.IsNullOrEmpty(feedbackRead))
            {
                return feedbackRead;
            }

            return $"Progress S{progressSave.SolvedNodeCount} R{progressSave.CollectedRewardCount}";
        }

        private string BuildPromptRead()
        {
            if (HasUnsolvedNode())
            {
                return "Use tool: E / North";
            }

            var rewardCount = CountRewards();
            if (rewardCount == 0)
            {
                return "Press forward";
            }

            if (rewardCount == 1)
            {
                var singleReward = GetFirstReward();
                if (singleReward == null)
                {
                    return "Press forward";
                }

                if (singleReward.IsCollected)
                {
                    return "Relic secured";
                }

                return singleReward.IsUnlocked ? "Claim relic: E / North" : "Defeat the enemy";
            }

            if (CountClaimableRewards() > 0)
            {
                return "Claim relic: E / North";
            }

            return CountCollectedRewards() >= rewardCount ? "Relics secured" : "Defeat the enemy";
        }

        private bool HasUnsolvedNode()
        {
            if (node != null && !node.IsSolved)
            {
                return true;
            }

            if (nodes == null)
            {
                return false;
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                var target = nodes[i];
                if (target != null && !target.IsSolved)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateSaveFeedback(float deltaTime)
        {
            if (progressSave == null)
            {
                feedbackTimer = 0f;
                feedbackRead = string.Empty;
                return;
            }

            if (observedSaveCount < 0 || observedLoadCount < 0)
            {
                CaptureSaveCounters();
                return;
            }

            if (progressSave.LoadCount != observedLoadCount)
            {
                observedLoadCount = progressSave.LoadCount;
                feedbackTimer = 1.4f;
                feedbackRead = "Progress Loaded";
            }
            else if (progressSave.SaveCount != observedSaveCount)
            {
                observedSaveCount = progressSave.SaveCount;
                if (feedbackTimer <= 0f || feedbackRead != RelicUnlockedFeedback)
                {
                    feedbackTimer = 1.4f;
                    feedbackRead = "Progress Saved";
                }
            }
            else if (feedbackTimer > 0f)
            {
                feedbackTimer = Mathf.Max(0f, feedbackTimer - Mathf.Max(0f, deltaTime));
            }
        }

        private void UpdateRewardUnlockFeedback()
        {
            var rewardCount = CountRewards();
            if (rewardCount == 0)
            {
                hasObservedRewardState = false;
                observedRewardUnlockedCount = 0;
                observedRewardCollectedCount = 0;
                return;
            }

            var unlockedCount = CountUnlockedRewards();
            var collectedCount = CountCollectedRewards();
            if (!hasObservedRewardState)
            {
                CaptureRewardState(unlockedCount, collectedCount);
                return;
            }

            if (collectedCount > observedRewardCollectedCount)
            {
                feedbackTimer = 0f;
                feedbackRead = string.Empty;
            }
            else if (unlockedCount > observedRewardUnlockedCount && collectedCount < rewardCount)
            {
                feedbackTimer = 1.4f;
                feedbackRead = RelicUnlockedFeedback;
            }

            CaptureRewardState(unlockedCount, collectedCount);
        }

        private void CaptureRewardState(int unlockedCount, int collectedCount)
        {
            hasObservedRewardState = true;
            observedRewardUnlockedCount = unlockedCount;
            observedRewardCollectedCount = collectedCount;
        }

        private bool HasRewardList()
        {
            if (rewards == null)
            {
                return false;
            }

            for (var i = 0; i < rewards.Length; i++)
            {
                if (rewards[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private D020RelicReward GetFirstReward()
        {
            if (HasRewardList())
            {
                for (var i = 0; i < rewards.Length; i++)
                {
                    if (rewards[i] != null)
                    {
                        return rewards[i];
                    }
                }
            }

            return reward;
        }

        private int CountRewards()
        {
            if (!HasRewardList())
            {
                return reward != null ? 1 : 0;
            }

            var count = 0;
            for (var i = 0; i < rewards.Length; i++)
            {
                if (rewards[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountUnlockedRewards()
        {
            var count = 0;
            ForEachReward(target =>
            {
                if (target.IsUnlocked)
                {
                    count++;
                }
            });
            return count;
        }

        private int CountCollectedRewards()
        {
            var count = 0;
            ForEachReward(target =>
            {
                if (target.IsCollected)
                {
                    count++;
                }
            });
            return count;
        }

        private int CountClaimableRewards()
        {
            var count = 0;
            ForEachReward(target =>
            {
                if (!target.IsCollected && target.IsUnlocked)
                {
                    count++;
                }
            });
            return count;
        }

        private void ForEachReward(Action<D020RelicReward> visit)
        {
            if (visit == null)
            {
                return;
            }

            if (HasRewardList())
            {
                for (var i = 0; i < rewards.Length; i++)
                {
                    var target = rewards[i];
                    if (target != null)
                    {
                        visit(target);
                    }
                }

                return;
            }

            if (reward != null)
            {
                visit(reward);
            }
        }

        private void CaptureSaveCounters()
        {
            if (progressSave == null)
            {
                observedSaveCount = 0;
                observedLoadCount = 0;
                return;
            }

            observedSaveCount = progressSave.SaveCount;
            observedLoadCount = progressSave.LoadCount;
        }

        private void EnsureStyles()
        {
            if (boxStyle != null)
            {
                return;
            }

            backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
            backgroundTexture.SetPixel(0, 0, new Color(0.04f, 0.05f, 0.07f, 0.84f));
            backgroundTexture.Apply();

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 10, 10)
            };
            boxStyle.normal.background = backgroundTexture;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.88f, 0.62f) }
            };

            lineStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.92f, 0.94f, 0.97f) }
            };
        }
    }
}
