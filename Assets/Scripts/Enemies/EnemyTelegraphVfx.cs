using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Product
{
    public sealed class EnemyTelegraphVfx : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject telegraphPrefab;

        [Header("Sizing")]
        [Min(0.05f)]
        public float sourceDiameter = 3.55f;
        [Min(0.05f)]
        public float minimumReadableDiameter = 1.15f;
        [Min(1f)]
        public float attackRadiusDiameterMultiplier = 2.4f;
        [Min(0f)]
        public float groundOffset = 0.035f;

        [Header("Pulse")]
        [Min(0.1f)]
        public float startScaleMultiplier = 0.92f;
        [Min(0.1f)]
        public float endScaleMultiplier = 1.05f;

        private GameObject instance;

        public bool IsVisible => instance != null && instance.activeSelf;
        public GameObject ActiveInstance => instance;

        public void Show(EnemyDefinition definition, Vector3 center, Vector3 forward, float normalizedProgress)
        {
            if (definition == null || telegraphPrefab == null)
            {
                Hide();
                return;
            }

            EnsureInstance();
            if (instance == null)
            {
                return;
            }

            var flatForward = forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude <= 0.0001f)
            {
                flatForward = transform.forward;
                flatForward.y = 0f;
            }
            flatForward = flatForward.sqrMagnitude <= 0.0001f ? Vector3.forward : flatForward.normalized;

            var worldCenter = center;
            worldCenter.y += groundOffset;
            instance.transform.SetPositionAndRotation(worldCenter, Quaternion.LookRotation(flatForward, Vector3.up));

            var desiredDiameter = Mathf.Max(minimumReadableDiameter, definition.attackRadius * attackRadiusDiameterMultiplier);
            var pulse = Mathf.Lerp(startScaleMultiplier, endScaleMultiplier, Mathf.Clamp01(normalizedProgress));
            var scale = desiredDiameter / Mathf.Max(0.05f, sourceDiameter) * pulse;
            instance.transform.localScale = Vector3.one * scale;

            if (!instance.activeSelf)
            {
                instance.SetActive(true);
            }
        }

        public void Hide()
        {
            if (instance != null)
            {
                instance.SetActive(false);
            }
        }

        private void OnDisable()
        {
            Hide();
        }

        private void EnsureInstance()
        {
            if (instance != null || telegraphPrefab == null)
            {
                return;
            }

            instance = Instantiate(telegraphPrefab, transform);
            instance.name = $"{telegraphPrefab.name}_Runtime";
            instance.SetActive(false);
            DisableShadows(instance);
        }

        private static void DisableShadows(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var index = 0; index < renderers.Length; index++)
            {
                renderers[index].shadowCastingMode = ShadowCastingMode.Off;
                renderers[index].receiveShadows = false;
            }
        }
    }
}
