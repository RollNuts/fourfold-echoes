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
        [Header("Health")]
        [Min(1f)]
        public float maxHealth = 100f;
        public bool resetOnEnable = true;

        [SerializeField]
        private float currentHealth;

        public event Action<Damageable, DamageInfo> Damaged;
        public event Action<Damageable, DamageInfo> Died;

        public float CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0f;
        public float Health01 => maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);

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
            var info = new DamageInfo(applied, source, point);
            Damaged?.Invoke(this, info);

            if (currentHealth <= 0f)
            {
                Died?.Invoke(this, info);
            }

            return true;
        }

        public void Kill(GameObject source = null)
        {
            ApplyDamage(Mathf.Max(currentHealth, maxHealth), source, transform.position);
        }
    }
}
