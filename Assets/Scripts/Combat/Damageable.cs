using System;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, GameObject source, Vector3 point)
        {
            Amount = amount;
            Source = source;
            Point = point;
        }

        public float Amount { get; }
        public GameObject Source { get; }
        public Vector3 Point { get; }
    }

    public sealed class Damageable : MonoBehaviour
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [Header("Health")]
        [Min(1f)]
        public float maxHealth = 100f;
        public bool resetOnEnable = true;

        [Header("Readability")]
        public bool showHitFlash = true;
        public GameObject hitFlashPrefab;
        public Color hitFlashColor = new Color(1f, 0.92f, 0.28f, 0.95f);
        public Color heavyHitFlashColor = new Color(1f, 0.62f, 0.08f, 0.98f);
        public Color lowHealthHitFlashColor = new Color(1f, 0.28f, 0.12f, 0.98f);
        public Color defeatFlashColor = new Color(1f, 0.36f, 0.14f, 1f);
        public bool showHitConfirmFlash = true;
        public Color hitConfirmFlashColor = new Color(0.42f, 0.88f, 1f, 0.9f);
        [Range(0.01f, 1f)]
        public float heavyHitFlashThreshold = 0.35f;
        [Range(0.01f, 1f)]
        public float lowHealthHitFlashThreshold = 0.3f;
        public float hitFlashDuration = 0.16f;
        public float defeatFlashDuration = 0.28f;
        public float hitConfirmFlashDuration = 0.1f;
        public float heavyHitFlashDurationMultiplier = 1.25f;
        public float lowHealthHitFlashDurationMultiplier = 1.4f;
        public float hitFlashScale = 0.36f;
        public float heavyHitFlashScaleMultiplier = 1.18f;
        public float lowHealthHitFlashScaleMultiplier = 1.28f;
        public float defeatFlashScaleMultiplier = 1.55f;
        public float hitConfirmFlashScale = 0.28f;
        public float hitFlashHeight = 0.55f;

        [SerializeField]
        private float currentHealth;

        private GameObject hitFlashInstance;
        private Renderer[] hitFlashRenderers;
        private MaterialPropertyBlock hitFlashBlock;
        private float hitFlashTimer;

        public event Action<Damageable, DamageInfo> Damaged;
        public event Action<Damageable, DamageInfo> Died;

        public float CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0f;
        public float Health01 => maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
        public GameObject HitFlashInstance => hitFlashInstance;

        private void Awake()
        {
            if (currentHealth <= 0f)
            {
                ResetHealth();
            }
        }

        private void OnEnable()
        {
            if (resetOnEnable)
            {
                ResetHealth();
            }
        }

        private void OnDisable()
        {
            SetHitFlashVisible(false);
        }

        private void Update()
        {
            if (hitFlashTimer <= 0f)
            {
                return;
            }

            hitFlashTimer = Mathf.Max(0f, hitFlashTimer - Time.deltaTime);
            if (hitFlashTimer <= 0f)
            {
                SetHitFlashVisible(false);
            }
        }

        public void ConfigureMaxHealth(float health, bool refill)
        {
            maxHealth = Mathf.Max(1f, health);
            if (refill || currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }

        public void ResetHealth()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
        }

        public bool ApplyDamage(float amount, GameObject source = null, Vector3 point = default)
        {
            if (!IsAlive || amount <= 0f)
            {
                return false;
            }

            var applied = Mathf.Min(amount, currentHealth);
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            var defeated = currentHealth <= 0f;
            var info = new DamageInfo(applied, source, point);
            if (showHitFlash)
            {
                var lowHealthHit = !defeated && Health01 <= lowHealthHitFlashThreshold;
                var heavyHit = !defeated
                    && !lowHealthHit
                    && maxHealth > 0f
                    && applied / maxHealth >= heavyHitFlashThreshold;
                var flashColor = defeated
                    ? defeatFlashColor
                    : (lowHealthHit ? lowHealthHitFlashColor : (heavyHit ? heavyHitFlashColor : hitFlashColor));
                var scaleMultiplier = defeated
                    ? Mathf.Max(1f, defeatFlashScaleMultiplier)
                    : (lowHealthHit
                        ? Mathf.Max(1f, lowHealthHitFlashScaleMultiplier)
                        : (heavyHit ? Mathf.Max(1f, heavyHitFlashScaleMultiplier) : 1f));
                var flashScale = hitFlashScale * scaleMultiplier;
                var durationMultiplier = lowHealthHit
                    ? Mathf.Max(1f, lowHealthHitFlashDurationMultiplier)
                    : (heavyHit ? Mathf.Max(1f, heavyHitFlashDurationMultiplier) : 1f);
                var flashDuration = defeated ? defeatFlashDuration : hitFlashDuration * durationMultiplier;
                TriggerFlash(point, flashColor, flashScale, flashDuration);
            }
            Damaged?.Invoke(this, info);

            if (defeated)
            {
                Died?.Invoke(this, info);
            }

            return true;
        }

        public void Kill(GameObject source = null)
        {
            ApplyDamage(Mathf.Max(currentHealth, maxHealth), source, transform.position);
        }

        public void ShowHitConfirm(Vector3 point = default)
        {
            if (!showHitConfirmFlash)
            {
                return;
            }

            TriggerFlash(point, hitConfirmFlashColor, hitConfirmFlashScale, hitConfirmFlashDuration);
        }

        private void TriggerFlash(Vector3 point, Color color, float scale, float duration)
        {
            EnsureHitFlash();
            if (hitFlashInstance == null)
            {
                return;
            }

            var markerPosition = point == default ? transform.position : point;
            markerPosition.y = Mathf.Max(markerPosition.y, transform.position.y + Mathf.Max(0f, hitFlashHeight));
            hitFlashInstance.transform.position = markerPosition;
            hitFlashInstance.transform.rotation = Quaternion.identity;
            hitFlashInstance.transform.localScale = Vector3.one * Mathf.Max(0.05f, scale);
            ApplyHitFlashColor(color);
            hitFlashTimer = Mathf.Max(0.02f, duration);
            SetHitFlashVisible(true);
        }

        private void EnsureHitFlash()
        {
            if (hitFlashInstance != null)
            {
                return;
            }

            hitFlashInstance = hitFlashPrefab != null
                ? Instantiate(hitFlashPrefab, transform)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitFlashInstance.name = "Damageable Hit Flash";
            hitFlashInstance.transform.SetParent(transform, true);
            DisableHitFlashColliders(hitFlashInstance);
            hitFlashRenderers = hitFlashInstance.GetComponentsInChildren<Renderer>();
            ApplyHitFlashColor(hitFlashColor);
            hitFlashInstance.SetActive(false);
        }

        private static void DisableHitFlashColliders(GameObject marker)
        {
            var colliders = marker.GetComponentsInChildren<Collider>();
            for (var index = 0; index < colliders.Length; index++)
            {
                colliders[index].enabled = false;
            }
        }

        private void ApplyHitFlashColor(Color color)
        {
            if (hitFlashRenderers == null || hitFlashRenderers.Length == 0)
            {
                return;
            }

            if (hitFlashBlock == null)
            {
                hitFlashBlock = new MaterialPropertyBlock();
            }

            for (var index = 0; index < hitFlashRenderers.Length; index++)
            {
                var targetRenderer = hitFlashRenderers[index];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(hitFlashBlock);
                hitFlashBlock.SetColor(BaseColorProperty, color);
                hitFlashBlock.SetColor(ColorProperty, color);
                targetRenderer.SetPropertyBlock(hitFlashBlock);
            }
        }

        private void SetHitFlashVisible(bool visible)
        {
            if (hitFlashInstance != null)
            {
                hitFlashInstance.SetActive(visible);
            }
        }
    }
}
