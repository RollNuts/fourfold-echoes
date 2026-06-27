using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class ProductionCombatShortcutToolPrompt : MonoBehaviour
    {
        private const string TargetSceneName = "ProductionCombatSlice";
        private const float ControllerRefreshSeconds = 0.25f;
        public const float DefaultPromptRange = 2.8f;

        private static ProductionCombatShortcutToolPrompt instance;

        private ProductionCombatSliceController controller;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle detailStyle;
        private Texture2D panelTexture;
        private Texture2D accentTexture;
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

            var gameObject = new GameObject(nameof(ProductionCombatShortcutToolPrompt));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<ProductionCombatShortcutToolPrompt>();
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
            if (controller == null || controller.player == null || controller.shortcutNode == null)
            {
                return;
            }

            if (!ShouldShowPrompt(
                    controller.State,
                    controller.WardensHealth01,
                    controller.ShortcutOpen,
                    controller.GateOpen,
                    controller.RewardClaimed,
                    controller.player.position,
                    controller.shortcutNode.transform.position,
                    DefaultPromptRange))
            {
                return;
            }

            EnsureStyles();

            var rect = BuildPromptRect(Screen.width, Screen.height);
            if (rect.width < 260f || rect.height < 52f)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.95f);
            GUI.Box(rect, GUIContent.none, panelStyle);

            GUI.color = new Color(1f, 1f, 1f, 0.9f);
            GUI.DrawTexture(new Rect(rect.x + 12f, rect.y + 12f, 4f, rect.height - 24f), accentTexture);

            GUI.color = previousColor;
            GUI.Label(new Rect(rect.x + 28f, rect.y + 8f, rect.width - 44f, 22f), "Echo Tool target", titleStyle);
            GUI.Label(new Rect(rect.x + 28f, rect.y + 30f, rect.width - 44f, 18f), BuildDetailText(controller.ToolReady01), detailStyle);
        }

        public static bool ShouldShowPrompt(
            ProductionCombatRunState state,
            float wardensHealth01,
            bool shortcutOpen,
            bool gateOpen,
            bool rewardClaimed,
            Vector3 playerPosition,
            Vector3 shortcutPosition,
            float promptRange)
        {
            if (state != ProductionCombatRunState.Playing
                || wardensHealth01 > 0.001f
                || shortcutOpen
                || gateOpen
                || rewardClaimed)
            {
                return false;
            }

            playerPosition.y = 0f;
            shortcutPosition.y = 0f;
            return Vector3.Distance(playerPosition, shortcutPosition) <= Mathf.Max(0f, promptRange);
        }

        public static string BuildDetailText(float toolReady01)
        {
            return toolReady01 >= 0.99f
                ? "Use the Echo Tool at this signal"
                : "Stay close while the tool recharges";
        }

        public static Rect BuildPromptRect(float screenWidth, float screenHeight)
        {
            var width = Mathf.Min(380f, Mathf.Max(0f, screenWidth - 48f));
            var height = 58f;
            var x = (screenWidth - width) * 0.5f;
            var y = Mathf.Max(24f, screenHeight - height - 104f);
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
            if (panelTexture == null)
            {
                panelTexture = new Texture2D(1, 1)
                {
                    hideFlags = HideFlags.DontSave
                };
                panelTexture.SetPixel(0, 0, new Color(0.025f, 0.03f, 0.035f, 0.86f));
                panelTexture.Apply();
            }

            if (accentTexture == null)
            {
                accentTexture = new Texture2D(1, 1)
                {
                    hideFlags = HideFlags.DontSave
                };
                accentTexture.SetPixel(0, 0, new Color(0.36f, 0.78f, 0.74f, 1f));
                accentTexture.Apply();
            }

            if (panelStyle != null)
            {
                return;
            }

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTexture;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = new Color(0.88f, 0.98f, 0.94f, 1f);

            detailStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft
            };
            detailStyle.normal.textColor = new Color(0.72f, 0.80f, 0.78f, 1f);
        }
    }
}
