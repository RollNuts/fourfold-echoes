using UnityEngine;
using UnityEngine.AI;

namespace FourfoldEchoes.Product
{
    public sealed class EnemyMotor : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Vector3 previousPosition;
        private float normalizedSpeed;

        public bool UsesNavMesh => agent != null && agent.enabled && agent.isOnNavMesh;
        public float NormalizedSpeed => normalizedSpeed;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            previousPosition = transform.position;
        }

        public void Configure(EnemyDefinition definition)
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }

            if (agent == null)
            {
                return;
            }

            agent.speed = definition.moveSpeed;
            agent.angularSpeed = definition.angularSpeed;
            agent.acceleration = definition.acceleration;
            agent.stoppingDistance = definition.stoppingDistance;
            agent.updateRotation = true;
        }

        public void TickSpeed(EnemyDefinition definition, float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            var distance = Vector3.Distance(transform.position, previousPosition);
            normalizedSpeed = Mathf.Clamp01(distance / Mathf.Max(0.01f, definition.moveSpeed * dt));
            previousPosition = transform.position;
        }

        public void MoveTo(Vector3 destination, EnemyDefinition definition, float dt)
        {
            destination.y = transform.position.y;
            if (UsesNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(destination);
                return;
            }

            var toDestination = destination - transform.position;
            toDestination.y = 0f;
            if (toDestination.magnitude <= definition.stoppingDistance)
            {
                Stop();
                return;
            }

            MoveDirection(toDestination.normalized, definition, dt);
        }

        public void MoveDirection(Vector3 direction, EnemyDefinition definition, float dt)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                Stop();
                return;
            }

            if (UsesNavMesh)
            {
                var destination = transform.position + direction.normalized * Mathf.Max(definition.stoppingDistance, definition.moveSpeed * 0.35f);
                agent.isStopped = false;
                agent.SetDestination(destination);
                return;
            }

            var delta = direction.normalized * definition.moveSpeed * dt;
            transform.position += ResolveObstacleSlide(delta, definition);
            Face(direction, definition, dt);
        }

        public void Face(Vector3 direction, EnemyDefinition definition, float dt)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            var maxDegrees = definition.angularSpeed * dt;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegrees);
        }

        public void Stop()
        {
            if (UsesNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            normalizedSpeed = 0f;
        }

        public bool HasArrived(Vector3 destination, EnemyDefinition definition)
        {
            var distance = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(destination.x, 0f, destination.z));
            return distance <= Mathf.Max(0.08f, definition.stoppingDistance);
        }

        private Vector3 ResolveObstacleSlide(Vector3 delta, EnemyDefinition definition)
        {
            if (definition.obstacleMask.value == 0 || delta.sqrMagnitude <= 0.000001f)
            {
                return delta;
            }

            var origin = transform.position + Vector3.up * Mathf.Max(0.1f, definition.characterRadius);
            var distance = delta.magnitude;
            var direction = delta / distance;
            if (!TrySphereCastExcludingSelf(origin, definition.characterRadius, direction, distance, definition.obstacleMask, out var hit))
            {
                return delta;
            }

            var safeDistance = Mathf.Max(0f, hit.distance - 0.025f);
            var safeDelta = direction * safeDistance;
            var slide = Vector3.ProjectOnPlane(delta - safeDelta, hit.normal);
            if (slide.sqrMagnitude <= 0.000001f)
            {
                return safeDelta;
            }

            var slideDistance = slide.magnitude;
            if (TrySphereCastExcludingSelf(origin + safeDelta, definition.characterRadius, slide / slideDistance, slideDistance, definition.obstacleMask, out _))
            {
                return safeDelta;
            }

            return safeDelta + slide;
        }

        private bool TrySphereCastExcludingSelf(Vector3 origin, float radius, Vector3 direction, float distance, LayerMask mask, out RaycastHit nearest)
        {
            nearest = default;
            var found = false;
            var bestDistance = float.PositiveInfinity;
            var hits = Physics.SphereCastAll(origin, radius, direction, distance, mask, QueryTriggerInteraction.Ignore);
            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (hit.transform == transform || hit.transform.IsChildOf(transform) || transform.IsChildOf(hit.transform))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    nearest = hit;
                    bestDistance = hit.distance;
                    found = true;
                }
            }

            return found;
        }
    }
}
