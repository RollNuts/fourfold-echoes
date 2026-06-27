using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020HudController : MonoBehaviour
    {
        public bool showHud = true;
        public D020PlayerController player;
        public ExplorationTool tool;
        public ExplorationNode node;
        public D020RelicReward reward;
        public D020ProgressSave progressSave;

        private GUIStyle boxStyle;
        private GUIStyle titleStyle;
        private GUIStyle lineStyle;
        private Texture2D backgroundTexture;
        private int observedSaveCount = -1;
        private int observedLoadCount = -1;
        private float feedbackTimer;
        private string feedbackRead = string.Empty;

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
            GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, 240f, 24f), "D-020 Room", titleStyle);
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
            if (reward == null)
            {
                return "Relic --";
            }

            if (reward.IsCollected)
            {
                return "Relic Claimed";
            }

            return reward.IsUnlocked ? "Relic Ready" : "Relic Locked";
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
            if (node != null && !node.IsSolved)
            {
                return "Use tool: E / North";
            }

            if (reward == null)
            {
                return "Press forward";
            }

            if (reward.IsCollected)
            {
                return "Relic secured";
            }

            return reward.IsUnlocked ? "Claim relic: E / North" : "Defeat the enemy";
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
                feedbackTimer = 1.4f;
                feedbackRead = "Progress Saved";
            }
            else if (feedbackTimer > 0f)
            {
                feedbackTimer = Mathf.Max(0f, feedbackTimer - Mathf.Max(0f, deltaTime));
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
