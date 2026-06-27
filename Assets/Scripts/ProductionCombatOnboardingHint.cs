using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    internal sealed class ProductionCombatOnboardingHint : MonoBehaviour
    {
        private const string TargetSceneName = "ProductionCombatSlice";
        private const float FullOpacitySeconds = 6.5f;
        private const float FadeSeconds = 1.5f;
        private const int PanelHeight = 112;
        private const int PanelMargin = 18;

        private static ProductionCombatOnboardingHint instance;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private Texture2D panelTexture;
        private float sceneStartedAt;
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

            var gameObject = new GameObject(nameof(ProductionCombatOnboardingHint));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<ProductionCombatOnboardingHint>();
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
        }

        private void RefreshSceneState(Scene scene)
        {
            inTargetScene = scene.name == TargetSceneName;
            sceneStartedAt = Time.unscaledTime;
        }

        private void OnGUI()
        {
            if (!inTargetScene)
            {
                return;
            }

            var elapsed = Time.unscaledTime - sceneStartedAt;
            var alpha = elapsed <= FullOpacitySeconds
                ? 1f
                : Mathf.Clamp01(1f - ((elapsed - FullOpacitySeconds) / FadeSeconds));
            if (alpha <= 0f)
            {
                return;
            }

            EnsureStyles();

            var width = Mathf.Min(560f, Screen.width - (PanelMargin * 2f));
            if (width < 280f)
            {
                return;
            }

            var rect = new Rect(PanelMargin, Screen.height - PanelHeight - PanelMargin, width, PanelHeight);
            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, 22f), "Controls", titleStyle);
            GUI.Label(
                new Rect(rect.x + 16f, rect.y + 38f, rect.width - 32f, 64f),
                "Move: Left Stick / WASD    Attack: South Button / J / Mouse\n"
                + "Echo Tool / Claim: North Button / E / Right Mouse    Pause: Menu / Esc / P\n"
                + "Retry after defeat: R",
                bodyStyle);

            GUI.color = previousColor;
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
            panelTexture.SetPixel(0, 0, new Color(0.04f, 0.045f, 0.055f, 0.88f));
            panelTexture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTexture },
                padding = new RectOffset(16, 16, 12, 12)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.98f, 0.91f, 0.68f, 1f) }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(0.96f, 0.97f, 0.98f, 1f) }
            };
        }
    }
}
