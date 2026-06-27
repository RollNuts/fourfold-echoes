using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class ProductionCombatGateRewardCue : MonoBehaviour
    {
        private const string TargetSceneName = "ProductionCombatSlice";
        private const float ControllerRefreshSeconds = 0.25f;
        private const float PanelHeight = 82f;
        private const float SideMargin = 24f;
        private const float TopMargin = 88f;

        private static ProductionCombatGateRewardCue instance;

        private ProductionCombatSliceController controller;
        private Texture2D panelTexture;
        private Texture2D accentTexture;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private float nextControllerRefreshAt;
        private bool inTargetScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            instance.RefreshSceneState(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureInstance();
            instance.RefreshSceneState(scene);
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            var gameObject = new GameObject(nameof(ProductionCombatGateRewardCue));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<ProductionCombatGateRewardCue>();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                instance = null;
            }

            if (panelTexture != null)
            {
                Destroy(panelTexture);
                panelTexture = null;
            }

            if (accentTexture != null)
            {
                Destroy(accentTexture);
                accentTexture = null;
            }
        }

        private void RefreshSceneState(Scene scene)
        {
            inTargetScene = scene.name == TargetSceneName;
            controller = null;
            nextControllerRefreshAt = 0f;
        }

        private void OnGUI()
        {
            if (!inTargetScene)
            {
                return;
            }

            RefreshControllerIfNeeded();
            if (controller == null || !ShouldShowCue(controller.State, controller.GateOpen, controller.RewardClaimed))
            {
                return;
            }

            var rect = CalculateCueRect(Screen.width, Screen.height);
            if (rect.width < 240f)
            {
                return;
            }

            EnsureStyles();

            var previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.Box(rect, GUIContent.none, panelStyle);

            var accent = new Rect(rect.x + 14f, rect.y + 18f, 10f, rect.height - 36f);
            GUI.DrawTexture(accent, accentTexture);

            var titleRect = new Rect(rect.x + 34f, rect.y + 13f, rect.width - 48f, 24f);
            var bodyRect = new Rect(rect.x + 34f, rect.y + 38f, rect.width - 48f, 34f);
            GUI.Label(titleRect, "Gate open", titleStyle);
            GUI.Label(bodyRect, BuildBodyText(IsPlayerNearReward(controller)), bodyStyle);
            GUI.color = previousColor;
        }

        public static bool ShouldShowCue(ProductionCombatRunState state, bool gateOpen, bool rewardClaimed)
        {
            return state == ProductionCombatRunState.Playing && gateOpen && !rewardClaimed;
        }

        public static Rect CalculateCueRect(float screenWidth, float screenHeight)
        {
            var usableWidth = Mathf.Max(0f, screenWidth - (SideMargin * 2f));
            var width = Mathf.Min(360f, Mathf.Max(286f, usableWidth * 0.34f));
            width = Mathf.Min(width, usableWidth);
            var x = Mathf.Max(16f, screenWidth - width - SideMargin);
            var maxY = Mathf.Max(16f, screenHeight - PanelHeight - 16f);
            var y = Mathf.Clamp(TopMargin, 16f, maxY);
            return new Rect(x, y, width, PanelHeight);
        }

        public static string BuildBodyText(bool playerNearReward)
        {
            return playerNearReward
                ? "North Button / E / RMB: Claim reward"
                : "Reach the chest, then claim the reward.";
        }

        public static bool IsPlayerNearReward(Vector3 playerPosition, Vector3 rewardPosition)
        {
            return ProductionCombatSliceController.IsWithinRewardClaimRange(playerPosition, rewardPosition);
        }

        private static bool IsPlayerNearReward(ProductionCombatSliceController sliceController)
        {
            return sliceController.player != null
                && sliceController.rewardChest != null
                && IsPlayerNearReward(sliceController.player.position, sliceController.rewardChest.transform.position);
        }

        private void RefreshControllerIfNeeded()
        {
            if (controller != null && controller.isActiveAndEnabled)
            {
                return;
            }

            if (Time.unscaledTime < nextControllerRefreshAt)
            {
                return;
            }

            controller = Object.FindFirstObjectByType<ProductionCombatSliceController>();
            nextControllerRefreshAt = Time.unscaledTime + ControllerRefreshSeconds;
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelTexture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.DontSave
            };
            panelTexture.SetPixel(0, 0, new Color(0.025f, 0.03f, 0.035f, 0.86f));
            panelTexture.Apply();

            accentTexture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.DontSave
            };
            accentTexture.SetPixel(0, 0, new Color(0.95f, 0.72f, 0.32f, 1f));
            accentTexture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTexture },
                padding = new RectOffset(14, 14, 12, 12)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(0.97f, 0.93f, 0.82f, 1f);

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Clip,
                fontSize = 13,
                wordWrap = true
            };
            bodyStyle.normal.textColor = new Color(0.78f, 0.82f, 0.78f, 1f);
        }
    }
}
