using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    [DisallowMultipleComponent]
    public sealed class ProductionCombatRewardFocus : MonoBehaviour
    {
        private const string TargetSceneName = "ProductionCombatSlice";

        public ProductionCombatSliceController controller;
        public Transform rewardTarget;

        [Min(0f)]
        public float pulseAmplitude = 0.08f;
        [Min(0.05f)]
        public float pulseFrequency = 2.15f;
        [Min(0.1f)]
        public float settleSpeed = 10f;

        private static bool sceneHookRegistered;
        private Vector3 baseScale = Vector3.one;
        private bool hasBaseScale;
        private float focusTimer;

        public bool IsFocusing { get; private set; }
        public float CurrentPulse01 { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!sceneHookRegistered)
            {
                SceneManager.sceneLoaded += HandleSceneLoaded;
                sceneHookRegistered = true;
            }

            EnsureForScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureForScene(scene);
        }

        private static void EnsureForScene(Scene scene)
        {
            if (!scene.IsValid() || scene.name != TargetSceneName)
            {
                return;
            }

            var controllers = FindObjectsByType<ProductionCombatSliceController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var index = 0; index < controllers.Length; index++)
            {
                var item = controllers[index];
                if (item.GetComponent<ProductionCombatRewardFocus>() != null)
                {
                    continue;
                }

                var focus = item.gameObject.AddComponent<ProductionCombatRewardFocus>();
                focus.controller = item;
            }
        }

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<ProductionCombatSliceController>();
            }
        }

        private void Update()
        {
            if (controller == null)
            {
                return;
            }

            ApplyFocus(Time.deltaTime, controller.GateOpen, controller.RewardClaimed);
        }

        public void ApplyFocus(float dt, bool gateOpen, bool rewardClaimed)
        {
            var target = ResolveTarget();
            if (target == null)
            {
                IsFocusing = false;
                CurrentPulse01 = 0f;
                return;
            }

            if (!hasBaseScale)
            {
                baseScale = target.localScale;
                hasBaseScale = true;
            }

            var shouldFocus = gateOpen && !rewardClaimed && target.gameObject.activeInHierarchy;
            IsFocusing = shouldFocus;
            if (shouldFocus)
            {
                focusTimer += Mathf.Max(0f, dt);
                CurrentPulse01 = (Mathf.Sin(focusTimer * pulseFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
                var pulse = 1f + (CurrentPulse01 * pulseAmplitude);
                target.localScale = Vector3.Lerp(target.localScale, baseScale * pulse, Mathf.Clamp01(dt * settleSpeed));
                return;
            }

            CurrentPulse01 = 0f;
            focusTimer = 0f;
            if (!rewardClaimed)
            {
                target.localScale = Vector3.Lerp(target.localScale, baseScale, Mathf.Clamp01(dt * settleSpeed));
            }
        }

        private Transform ResolveTarget()
        {
            if (rewardTarget != null)
            {
                return rewardTarget;
            }

            if (controller != null && controller.rewardChest != null)
            {
                rewardTarget = controller.rewardChest.transform;
            }

            return rewardTarget;
        }
    }
}
