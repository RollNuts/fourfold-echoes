using System;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public readonly struct EnemyPerception
    {
        public EnemyPerception(
            Transform target,
            Vector3 toTarget,
            float distanceToTarget,
            bool targetVisible,
            bool hasKnownTarget,
            bool withinAttackRange,
            bool outsideLeash)
        {
            Target = target;
            ToTarget = toTarget;
            DistanceToTarget = distanceToTarget;
            TargetVisible = targetVisible;
            HasKnownTarget = hasKnownTarget;
            WithinAttackRange = withinAttackRange;
            OutsideLeash = outsideLeash;
        }

        public Transform Target { get; }
        public Vector3 ToTarget { get; }
        public float DistanceToTarget { get; }
        public bool TargetVisible { get; }
        public bool HasKnownTarget { get; }
        public bool WithinAttackRange { get; }
        public bool OutsideLeash { get; }
        public bool CanEngage => Target != null && TargetVisible && !OutsideLeash;
    }

    public sealed class EnemySensor : MonoBehaviour
    {
        [Header("Runtime Target")]
        public Transform target;

        private EnemyDefinition lastDefinition;
        private EnemyPerception lastPerception;
        private Vector3 lastHomePosition;

        public EnemyPerception LastPerception => lastPerception;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public EnemyPerception Sample(EnemyDefinition definition, Vector3 homePosition, Transform self)
        {
            lastDefinition = definition;
            lastHomePosition = homePosition;

            var candidate = target != null ? target : AcquireTarget(definition, self);
            if (!IsViableTarget(candidate))
            {
                candidate = null;
            }

            var outsideLeash = DistanceXZ(self.position, homePosition) > definition.leashRadius;
            if (candidate == null)
            {
                lastPerception = new EnemyPerception(null, Vector3.zero, float.PositiveInfinity, false, false, false, outsideLeash);
                return lastPerception;
            }

            var toTarget = candidate.position - self.position;
            toTarget.y = 0f;
            var distance = toTarget.magnitude;
            outsideLeash = outsideLeash || DistanceXZ(candidate.position, homePosition) > definition.leashRadius;
            var inDetection = distance <= definition.detectionRadius;
            var inMemory = distance <= definition.loseSightRadius;
            var visible = inDetection
                && IsInsideFieldOfView(definition, self, toTarget)
                && HasLineOfSight(definition, self, candidate);
            var attackRange = definition.attackRange + definition.attackRadius;
            var withinAttackRange = visible && distance <= attackRange;

            lastPerception = new EnemyPerception(candidate, toTarget, distance, visible, inMemory, withinAttackRange, outsideLeash);
            return lastPerception;
        }

        private Transform AcquireTarget(EnemyDefinition definition, Transform self)
        {
            var colliders = Physics.OverlapSphere(self.position, definition.detectionRadius, definition.targetLayers, QueryTriggerInteraction.Ignore);
            Transform best = null;
            var bestDistance = float.PositiveInfinity;

            for (var index = 0; index < colliders.Length; index++)
            {
                var candidate = colliders[index].transform;
                if (candidate == self || candidate.IsChildOf(self) || !IsViableTarget(candidate))
                {
                    continue;
                }

                var distance = DistanceXZ(candidate.position, self.position);
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static bool IsViableTarget(Transform candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            var damageable = candidate.GetComponentInParent<Damageable>();
            return damageable == null || damageable.IsAlive;
        }

        private static bool IsInsideFieldOfView(EnemyDefinition definition, Transform self, Vector3 toTarget)
        {
            if (definition.fieldOfViewDegrees >= 359f || toTarget.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            var forward = self.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            var threshold = Mathf.Cos(definition.fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad);
            return Vector3.Dot(forward.normalized, toTarget.normalized) >= threshold;
        }

        private static bool HasLineOfSight(EnemyDefinition definition, Transform self, Transform candidate)
        {
            if (!definition.requireLineOfSight || definition.lineOfSightMask.value == 0)
            {
                return true;
            }

            var from = self.position + Vector3.up * definition.eyeHeight;
            var to = candidate.position + Vector3.up * definition.eyeHeight;
            var delta = to - from;
            var distance = delta.magnitude;
            if (distance <= 0.001f)
            {
                return true;
            }

            var hits = Physics.RaycastAll(from, delta / distance, distance, definition.lineOfSightMask, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0)
            {
                return true;
            }

            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            for (var index = 0; index < hits.Length; index++)
            {
                var hitTransform = hits[index].transform;
                if (hitTransform == self || hitTransform.IsChildOf(self))
                {
                    continue;
                }

                return hitTransform == candidate || hitTransform.IsChildOf(candidate);
            }

            return true;
        }

        private static float DistanceXZ(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void OnDrawGizmosSelected()
        {
            if (lastDefinition == null || !lastDefinition.drawDebug)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.75f, 0.12f, 0.22f);
            Gizmos.DrawWireSphere(transform.position, lastDefinition.detectionRadius);
            Gizmos.color = new Color(1f, 0.2f, 0.12f, 0.14f);
            Gizmos.DrawWireSphere(transform.position, lastDefinition.loseSightRadius);
            Gizmos.color = new Color(0.2f, 0.65f, 1f, 0.16f);
            Gizmos.DrawWireSphere(lastHomePosition, lastDefinition.leashRadius);

            if (lastPerception.Target != null)
            {
                Gizmos.color = lastPerception.TargetVisible ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * lastDefinition.eyeHeight, lastPerception.Target.position + Vector3.up * lastDefinition.eyeHeight);
            }
        }
    }
}
