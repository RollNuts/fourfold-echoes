using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class ProductionCombatLowHealthWarning : MonoBehaviour
    {
        private const string TargetSceneName = "ProductionCombatSlice";
        private const float WarningThreshold = 0.35f;
        private const float CriticalThreshold = 0.18f;
        private const float ControllerRefreshSeconds = 0.25f;

        private static ProductionCombatLowHealthWarning instance;

        private ProductionCombatSliceController controller;
        private Texture2D warningTexture;
        private GUIStyle labelStyle;
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

            var gameObject = new GameObject(nameof(ProductionCombatLowHealthWarning));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<ProductionCombatLowHealthWarning>();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                instance = null;
            }

            if (warningTexture != null)
            {
                Destroy(warningTexture);
                warningTexture = null;
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
            if (controller == null || controller.State != ProductionCombatRunState.Playing)
            {
                return;
            }

            var health = controller.PlayerHealth01;
            if (!IsWarningHealth(health))
            {
                return;
            }

            EnsureStyles();

            var previousColor = GUI.color;
            var danger = Mathf.InverseLerp(WarningThreshold, 0f, health);
            var pulse = 0.74f + (Mathf.Sin(Time.unscaledTime * 7.5f) * 0.16f);
            var alpha = Mathf.Clamp01(Mathf.Lerp(0.18f, 0.46f, danger) * pulse);
            GUI.color = new Color(1f, 1f, 1f, alpha);

            var edge = Mathf.Lerp(14f, 32f, danger);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, edge), warningTexture);
            GUI.DrawTexture(new Rect(0f, Screen.height - edge, Screen.width, edge), warningTexture);
            GUI.DrawTexture(new Rect(0f, 0f, edge, Screen.height), warningTexture);
            GUI.DrawTexture(new Rect(Screen.width - edge, 0f, edge, Screen.height), warningTexture);

            var warningLabel = HealthWarningLabel(health);
            if (!string.IsNullOrEmpty(warningLabel))
            {
                var labelAlpha = IsCriticalHealth(health) ? Mathf.Clamp01(alpha + 0.22f) : Mathf.Clamp01(alpha + 0.08f);
                GUI.color = new Color(1f, 1f, 1f, labelAlpha);
                var rect = new Rect(0f, Screen.height - 126f, Screen.width, 30f);
                GUI.Label(rect, warningLabel, labelStyle);
            }

            GUI.color = previousColor;
        }

        public static bool IsWarningHealth(float health01)
        {
            return health01 > 0f && health01 <= WarningThreshold;
        }

        public static bool IsCriticalHealth(float health01)
        {
            return health01 > 0f && health01 <= CriticalThreshold;
        }

        public static string HealthWarningLabel(float health01)
        {
            if (IsCriticalHealth(health01))
            {
                return "Critical health - dodge now";
            }

            return IsWarningHealth(health01) ? "Low health - create space" : string.Empty;
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
            if (warningTexture == null)
            {
                warningTexture = new Texture2D(1, 1)
                {
                    hideFlags = HideFlags.DontSave
                };
                warningTexture.SetPixel(0, 0, new Color(0.86f, 0.12f, 0.08f, 1f));
                warningTexture.Apply();
            }

            if (labelStyle != null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = new Color(1f, 0.82f, 0.76f, 1f);
        }
    }
}
