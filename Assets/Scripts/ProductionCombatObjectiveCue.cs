using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class ProductionCombatObjectiveCue : MonoBehaviour
    {
        private const string TargetSceneName = "ProductionCombatSlice";
        private const float ControllerRefreshSeconds = 0.25f;
        private const float ChangePulseSeconds = 1.1f;
        private const float DesktopBreakpoint = 920f;

        private static ProductionCombatObjectiveCue instance;

        private ProductionCombatSliceController controller;
        private GUIStyle panelStyle;
        private GUIStyle kickerStyle;
        private GUIStyle objectiveStyle;
        private GUIStyle promptStyle;
        private Texture2D panelTexture;
        private Texture2D accentTexture;
        private string lastObjective;
        private float objectiveChangedAt;
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

            var gameObject = new GameObject(nameof(ProductionCombatObjectiveCue));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<ProductionCombatObjectiveCue>();
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
            lastObjective = null;
            objectiveChangedAt = Time.unscaledTime;
            nextControllerRefreshAt = 0f;
        }

        private void OnGUI()
        {
            if (!inTargetScene)
            {
                return;
            }

            RefreshControllerIfNeeded();
            if (controller == null)
            {
                return;
            }

            var objective = BuildObjectiveText(
                controller.State,
                controller.WardensHealth01,
                controller.ShortcutOpen,
                controller.BossUnlocked,
                controller.BossHealth01,
                controller.GateOpen,
                controller.RewardClaimed,
                controller.ToolReady01);
            var actionPrompt = BuildActionPromptText(
                controller.State,
                controller.WardensHealth01,
                controller.ShortcutOpen,
                controller.BossUnlocked,
                controller.BossHealth01,
                controller.GateOpen,
                controller.RewardClaimed,
                controller.ToolReady01);

            if (string.IsNullOrEmpty(objective))
            {
                lastObjective = null;
                return;
            }

            if (objective != lastObjective)
            {
                lastObjective = objective;
                objectiveChangedAt = Time.unscaledTime;
            }

            EnsureStyles();
            var rect = BuildPanelRect(Screen.width, Screen.height);
            if (rect.width < 300f || rect.height < 56f)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            GUI.Box(rect, GUIContent.none, panelStyle);

            var pulse01 = Mathf.Clamp01(1f - ((Time.unscaledTime - objectiveChangedAt) / ChangePulseSeconds));
            var accentAlpha = Mathf.Lerp(0.72f, 1f, pulse01);
            GUI.color = new Color(1f, 1f, 1f, accentAlpha);
            GUI.DrawTexture(new Rect(rect.x + 12f, rect.y + 12f, 4f, rect.height - 24f), accentTexture);

            GUI.color = previousColor;
            GUI.Label(new Rect(rect.x + 26f, rect.y + 10f, rect.width - 42f, 18f), "Next", kickerStyle);
            GUI.Label(new Rect(rect.x + 26f, rect.y + 29f, rect.width - 42f, 24f), objective, objectiveStyle);
            if (!string.IsNullOrEmpty(actionPrompt))
            {
                GUI.Label(new Rect(rect.x + 26f, rect.y + 56f, rect.width - 42f, 22f), actionPrompt, promptStyle);
            }
        }

        public static string BuildObjectiveText(
            ProductionCombatRunState state,
            float wardensHealth01,
            bool shortcutOpen,
            bool bossUnlocked,
            float bossHealth01,
            bool gateOpen,
            bool rewardClaimed,
            float toolReady01)
        {
            if (state != ProductionCombatRunState.Playing || rewardClaimed)
            {
                return string.Empty;
            }

            if (gateOpen)
            {
                return "Claim the reward at the open gate";
            }

            if (bossUnlocked)
            {
                return bossHealth01 > 0.001f
                    ? "Break the boss gate"
                    : "Move through the opening gate";
            }

            if (wardensHealth01 > 0.001f)
            {
                return "Defeat the wardens";
            }

            if (!shortcutOpen)
            {
                return toolReady01 >= 0.99f
                    ? "Use the Echo Tool to reveal the shortcut"
                    : "Hold near the shortcut while the Echo Tool recovers";
            }

            return "Move through the shortcut route";
        }

        public static string BuildActionPromptText(
            ProductionCombatRunState state,
            float wardensHealth01,
            bool shortcutOpen,
            bool bossUnlocked,
            float bossHealth01,
            bool gateOpen,
            bool rewardClaimed,
            float toolReady01)
        {
            if (state != ProductionCombatRunState.Playing || rewardClaimed)
            {
                return string.Empty;
            }

            if (gateOpen)
            {
                return "North Button / E / RMB: Claim reward";
            }

            if (bossUnlocked)
            {
                return bossHealth01 > 0.001f
                    ? "South Button / J / Mouse: Attack boss"
                    : "Move through the open gate";
            }

            if (wardensHealth01 > 0.001f)
            {
                return "South Button / J / Mouse: Attack";
            }

            if (!shortcutOpen)
            {
                return toolReady01 >= 0.99f
                    ? "North Button / E / RMB: Echo Tool"
                    : "Stay close until the tool is ready";
            }

            return "Left Stick / WASD: Follow the revealed route";
        }

        public static Rect BuildPanelRect(float screenWidth, float screenHeight)
        {
            var width = Mathf.Min(520f, Mathf.Max(0f, screenWidth - 48f));
            var height = 88f;
            var x = screenWidth >= DesktopBreakpoint ? screenWidth - width - 24f : 24f;
            var y = screenWidth >= DesktopBreakpoint ? 20f : Mathf.Min(190f, Mathf.Max(24f, screenHeight - height - 24f));
            return new Rect(x, y, width, height);
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
            panelTexture.SetPixel(0, 0, new Color(0.025f, 0.03f, 0.035f, 0.84f));
            panelTexture.Apply();

            accentTexture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.DontSave
            };
            accentTexture.SetPixel(0, 0, new Color(0.92f, 0.67f, 0.28f, 1f));
            accentTexture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTexture },
                padding = new RectOffset(18, 18, 10, 10)
            };

            kickerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.67f, 0.28f, 1f) }
            };

            objectiveStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.96f, 0.97f, 0.94f, 1f) }
            };

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                normal = { textColor = new Color(0.72f, 0.80f, 0.78f, 1f) }
            };
        }
    }
}
