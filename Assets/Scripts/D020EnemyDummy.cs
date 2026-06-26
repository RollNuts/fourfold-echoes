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
        public float attackRange = 1.65f;
        public float attackWindupSeconds = 0.72f;
        public float attackCooldownSeconds = 1.45f;
        public int attackDamage = 1;

        private int currentHealth;
        private float pulseTime;
        private float attackWindupTimer;
        private float attackCooldownTimer;

        public int CurrentHealth => currentHealth;
        public int HitCount { get; private set; }
        public int AttackCount { get; private set; }
        public bool IsDefeated => currentHealth <= 0;
        public bool IsTelegraphing => attackWindupTimer > 0f;

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
            attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Mathf.Max(0f, deltaTime));
            if (tellRead != null)
            {
                var telegraphScale = IsTelegraphing ? 1.35f : 1f;
                var pulseRate = IsTelegraphing ? 10.5f : 5.4f;
                var pulse = telegraphScale + Mathf.Sin(pulseTime * pulseRate) * 0.08f;
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

            TickAttack(flatDistance, Mathf.Max(0f, deltaTime));

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
            AttackCount = 0;
            pulseTime = 0f;
            attackWindupTimer = 0f;
            attackCooldownTimer = 0f;
            ApplyState();
        }

        private void TickAttack(float flatDistance, float deltaTime)
        {
            if (attackWindupTimer > 0f)
            {
                attackWindupTimer = Mathf.Max(0f, attackWindupTimer - deltaTime);
                if (attackWindupTimer <= 0f)
                {
                    ResolveAttack(flatDistance);
                }

                return;
            }

            if (attackCooldownTimer <= 0f && flatDistance <= attackRange)
            {
                attackWindupTimer = Mathf.Max(0.01f, attackWindupSeconds);
            }
        }

        private void ResolveAttack(float flatDistance)
        {
            attackCooldownTimer = Mathf.Max(0f, attackCooldownSeconds);
            if (flatDistance > attackRange || target == null)
            {
                return;
            }

            var player = target.GetComponent<D020PlayerController>();
            if (player != null && player.TakeDamage(attackDamage))
            {
                AttackCount++;
            }
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
