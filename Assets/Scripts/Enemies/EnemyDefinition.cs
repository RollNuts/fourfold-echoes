using UnityEngine;

namespace FourfoldEchoes.Product
{
    public enum EnemyArchetype
    {
        Melee,
        Ranged
    }

    public enum EnemyState
    {
        Search,
        Chase,
        Telegraph,
        Attack,
        Recover,
        Retreat,
        ReturnHome,
        Dead
    }

    [CreateAssetMenu(menuName = "FOURFOLD/Enemies/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string enemyId = "FE_ENEMY_MELEE_Shardling";
        public EnemyArchetype archetype = EnemyArchetype.Melee;

        [Header("Health")]
        [Min(1f)]
        public float maxHealth = 70f;

        [Header("Sensing")]
        [Min(0.1f)]
        public float detectionRadius = 6.8f;
        [Min(0.1f)]
        public float loseSightRadius = 8.4f;
        [Min(0.1f)]
        public float leashRadius = 10f;
        [Range(1f, 360f)]
        public float fieldOfViewDegrees = 220f;
        public bool requireLineOfSight = true;
        [Min(0f)]
        public float eyeHeight = 0.65f;
        public LayerMask targetLayers = ~0;
        public LayerMask lineOfSightMask = 0;

        [Header("Movement")]
        [Min(0.05f)]
        public float moveSpeed = 1.35f;
        [Min(1f)]
        public float angularSpeed = 720f;
        [Min(0.1f)]
        public float acceleration = 18f;
        [Min(0f)]
        public float stoppingDistance = 0.75f;
        [Min(0.02f)]
        public float repathInterval = 0.18f;
        [Min(0.05f)]
        public float characterRadius = 0.32f;
        public LayerMask obstacleMask = 0;

        [Header("Attack")]
        [Min(0.05f)]
        public float attackRange = 1.05f;
        [Min(0.05f)]
        public float attackRadius = 0.45f;
        [Range(1f, 360f)]
        public float attackArcDegrees = 105f;
        [Min(0f)]
        public float attackDamage = 8f;
        [Min(0f)]
        public float telegraphTime = 0.42f;
        [Min(0.01f)]
        public float activeTime = 0.08f;
        [Min(0f)]
        public float recoveryTime = 0.46f;
        [Min(0f)]
        public float cooldownTime = 0.85f;

        [Header("Retreat")]
        public bool retreatAfterAttack = true;
        [Min(0f)]
        public float retreatDistance = 2.25f;
        [Min(0f)]
        public float retreatDuration = 0.35f;

        [Header("Debug")]
        public bool drawDebug = true;

        private void OnValidate()
        {
            loseSightRadius = Mathf.Max(loseSightRadius, detectionRadius);
            leashRadius = Mathf.Max(leashRadius, loseSightRadius);
            stoppingDistance = Mathf.Min(stoppingDistance, Mathf.Max(0.01f, attackRange * 0.8f));
        }
    }
}
