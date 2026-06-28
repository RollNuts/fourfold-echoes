using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020EnemyDummy : MonoBehaviour
    {
        public int maxHealth = 3;
        public Transform target;
        public GameObject defeatedRead;
        public GameObject tellRead;
        public float slowChaseSpeed = 0.55f;
        public float keepDistance = 1.4f;
        public int criticalHealthThreshold = 1;

        private int currentHealth;
        private float pulseTime;

        public int CurrentHealth => currentHealth;
        public int HitCount { get; private set; }
        public bool IsDefeated => currentHealth <= 0;
        public bool IsCriticalHealth => !IsDefeated && currentHealth <= Mathf.Max(1, criticalHealthThreshold);

        private void Awake()
        {
            ResetEnemy();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (IsDefeated)
            {
                return;
            }

            pulseTime += Mathf.Max(0f, deltaTime);
            if (tellRead != null)
            {
                var pulseSpeed = IsCriticalHealth ? 8.2f : 5.4f;
                var pulseCenter = IsCriticalHealth ? 1.16f : 1f;
                var pulseAmplitude = IsCriticalHealth ? 0.16f : 0.08f;
                var pulse = pulseCenter + Mathf.Sin(pulseTime * pulseSpeed) * pulseAmplitude;
                tellRead.transform.localScale = new Vector3(0.95f * pulse, 0.025f, 0.95f * pulse);
            }

            if (target == null)
            {
                return;
            }

            var delta = target.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.001f)
            {
                return;
            }

            var flatDistance = delta.magnitude;
            if (flatDistance > keepDistance)
            {
                transform.position += delta.normalized * slowChaseSpeed * Mathf.Max(0f, deltaTime);
            }

            var yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.Euler(0f, yaw, 0f),
                280f * Mathf.Max(0f, deltaTime));
        }

        public bool TakeHit(int damage)
        {
            if (IsDefeated)
            {
                return false;
            }

            HitCount++;
            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, damage));
            ApplyState();
            return true;
        }

        public void ResetEnemy()
        {
            currentHealth = Mathf.Max(1, maxHealth);
            HitCount = 0;
            pulseTime = 0f;
            ApplyState();
        }

        private void ApplyState()
        {
            if (defeatedRead != null)
            {
                defeatedRead.SetActive(IsDefeated);
            }

            if (tellRead != null)
            {
                tellRead.SetActive(!IsDefeated);
            }
        }
    }
}
