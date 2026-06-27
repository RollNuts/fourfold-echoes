using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class EnemyAttackDriver : MonoBehaviour
    {
        [Header("Attack Origin")]
        public Transform attackOrigin;

        private readonly HashSet<Damageable> damagedTargets = new HashSet<Damageable>();
        private EnemyDefinition lastDefinition;
        private Vector3 lastAttackCenter;
        private float lastAttackRadius;

        private void Awake()
        {
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }
        }

        public void Configure(EnemyDefinition definition)
        {
            lastDefinition = definition;
        }

        public int ResolveHit(EnemyDefinition definition, GameObject owner)
        {
            Configure(definition);
            damagedTargets.Clear();

            var origin = attackOrigin != null ? attackOrigin : transform;
            var forward = origin.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = transform.forward;
                forward.y = 0f;
            }
            forward = forward.sqrMagnitude <= 0.0001f ? Vector3.forward : forward.normalized;

            lastAttackCenter = origin.position + forward * definition.attackRange;
            lastAttackRadius = definition.attackRadius;

            var searchRadius = definition.attackRange + definition.attackRadius;
            var colliders = Physics.OverlapSphere(origin.position, searchRadius, definition.targetLayers, QueryTriggerInteraction.Collide);
            var ownerDamageable = owner != null ? owner.GetComponentInParent<Damageable>() : null;
            var hits = 0;
            var arcThreshold = Mathf.Cos(definition.attackArcDegrees * 0.5f * Mathf.Deg2Rad);

            for (var index = 0; index < colliders.Length; index++)
            {
                var damageable = colliders[index].GetComponentInParent<Damageable>();
                if (damageable == null || damageable == ownerDamageable || !damageable.IsAlive || damagedTargets.Contains(damageable))
                {
                    continue;
                }

                var toTarget = damageable.transform.position - origin.position;
                toTarget.y = 0f;
                var distance = toTarget.magnitude;
                if (distance > searchRadius || distance <= 0.0001f)
                {
                    continue;
                }

                if (definition.attackArcDegrees < 359f && Vector3.Dot(forward, toTarget / distance) < arcThreshold)
                {
                    continue;
                }

                if (damageable.ApplyDamage(definition.attackDamage, owner, colliders[index].ClosestPoint(origin.position)))
                {
                    damagedTargets.Add(damageable);
                    hits++;
                }
            }

            return hits;
        }

        private void OnDrawGizmosSelected()
        {
            if (lastDefinition == null || !lastDefinition.drawDebug)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.1f, 0.05f, 0.28f);
            Gizmos.DrawWireSphere(lastAttackCenter, Mathf.Max(0.05f, lastAttackRadius));
        }
    }
}
