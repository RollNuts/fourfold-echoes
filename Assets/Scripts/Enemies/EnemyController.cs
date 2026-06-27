using System;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    [RequireComponent(typeof(EnemyMotor))]
    [RequireComponent(typeof(EnemySensor))]
    [RequireComponent(typeof(EnemyAttackDriver))]
    [RequireComponent(typeof(Damageable))]
    public sealed class EnemyController : MonoBehaviour
    {
        [Header("Config")]
        public EnemyDefinition definition;
        public Transform targetOverride;
        public bool autoStart = true;

        [Header("Readability")]
        public bool applyStateReadabilityTint = true;
        public Renderer[] readabilityRenderers;
        public Color telegraphTint = new Color(1f, 0.72f, 0.18f, 1f);
        public Color attackTint = new Color(1f, 0.2f, 0.12f, 1f);
        public Color recoverTint = new Color(0.55f, 0.62f, 0.7f, 1f);

        [Header("Debug")]
        [SerializeField]
        private EnemyState currentState = EnemyState.Search;

        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        private EnemyMotor motor;
        private EnemySensor sensor;
        private EnemyAnimatorBridge animatorBridge;
        private EnemyAttackDriver attackDriver;
        private EnemyTelegraphVfx telegraphVfx;
        private Damageable damageable;
        private MaterialPropertyBlock readabilityBlock;
        private Vector3 homePosition;
        private Vector3 lastKnownTargetPosition;
        private float stateTimer;
        private float attackCooldown;
        private float repathTimer;
        private bool attackResolved;
        private bool initialized;

        public event Action<EnemyState, EnemyState> StateChanged;

        public EnemyState CurrentState => currentState;
        public Vector3 HomePosition => homePosition;
        public Damageable Damageable => damageable;

        private void Awake()
        {
            CacheComponents();
            homePosition = transform.position;
        }

        private void OnEnable()
        {
            CacheComponents();
            if (damageable != null)
            {
                damageable.Damaged += HandleDamaged;
                damageable.Died += HandleDied;
            }
        }

        private void Start()
        {
            if (autoStart && definition != null)
            {
                ResetAi(targetOverride);
            }
        }

        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.Damaged -= HandleDamaged;
                damageable.Died -= HandleDied;
            }
            telegraphVfx?.Hide();
            ClearReadabilityTint();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void ResetAi(Transform forcedTarget = null)
        {
            CacheComponents();
            if (definition == null)
            {
                Debug.LogWarning($"{nameof(EnemyController)} on {name} has no EnemyDefinition.", this);
                return;
            }

            if (forcedTarget != null)
            {
                targetOverride = forcedTarget;
            }

            homePosition = transform.position;
            lastKnownTargetPosition = targetOverride != null ? targetOverride.position : transform.position;
            stateTimer = 0f;
            attackCooldown = 0f;
            repathTimer = 0f;
            attackResolved = false;
            initialized = true;

            sensor.SetTarget(targetOverride);
            motor.Configure(definition);
            attackDriver.Configure(definition);
            damageable.ConfigureMaxHealth(definition.maxHealth, true);
            telegraphVfx?.Hide();
            ChangeState(EnemyState.Search);
            ApplyReadabilityTint(currentState);
        }

        public void Tick(float dt)
        {
            if (!initialized || definition == null)
            {
                return;
            }

            dt = Mathf.Max(0f, dt);
            attackCooldown = Mathf.Max(0f, attackCooldown - dt);
            repathTimer = Mathf.Max(0f, repathTimer - dt);

            if (damageable != null && !damageable.IsAlive)
            {
                ChangeState(EnemyState.Dead);
            }

            if (currentState == EnemyState.Dead)
            {
                motor.Stop();
                animatorBridge.SetSpeed(0f);
                return;
            }

            stateTimer += dt;
            var perception = sensor.Sample(definition, homePosition, transform);
            if (perception.TargetVisible)
            {
                lastKnownTargetPosition = perception.Target.position;
            }

            switch (currentState)
            {
                case EnemyState.Search:
                    TickSearch(perception);
                    break;
                case EnemyState.Chase:
                    TickChase(perception, dt);
                    break;
                case EnemyState.Telegraph:
                    TickTelegraph(perception, dt);
                    break;
                case EnemyState.Attack:
                    TickAttack(dt);
                    break;
                case EnemyState.Recover:
                    TickRecover(perception);
                    break;
                case EnemyState.Retreat:
                    TickRetreat(perception, dt);
                    break;
                case EnemyState.ReturnHome:
                    TickReturnHome(perception, dt);
                    break;
            }

            motor.TickSpeed(definition, dt);
            animatorBridge.SetSpeed(motor.NormalizedSpeed);
        }

        private void TickSearch(EnemyPerception perception)
        {
            motor.Stop();
            if (perception.CanEngage)
            {
                ChangeState(EnemyState.Chase);
            }
        }

        private void TickChase(EnemyPerception perception, float dt)
        {
            if (perception.OutsideLeash || !perception.HasKnownTarget)
            {
                ChangeState(EnemyState.ReturnHome);
                return;
            }

            if (perception.WithinAttackRange && attackCooldown <= 0f)
            {
                ChangeState(EnemyState.Telegraph);
                return;
            }

            var destination = perception.TargetVisible ? perception.Target.position : lastKnownTargetPosition;
            if (!perception.TargetVisible && motor.HasArrived(destination, definition))
            {
                ChangeState(EnemyState.Search);
                return;
            }

            MoveToDestination(destination, dt);
            if (perception.ToTarget.sqrMagnitude > 0.0001f)
            {
                motor.Face(perception.ToTarget, definition, dt);
            }
        }

        private void TickTelegraph(EnemyPerception perception, float dt)
        {
            motor.Stop();
            var facing = perception.HasKnownTarget ? lastKnownTargetPosition - transform.position : transform.forward;
            motor.Face(facing, definition, dt);
            UpdateTelegraphVfx(definition.telegraphTime <= 0f ? 1f : stateTimer / definition.telegraphTime);

            if (perception.OutsideLeash)
            {
                ChangeState(EnemyState.ReturnHome);
                return;
            }

            if (stateTimer >= definition.telegraphTime)
            {
                ChangeState(EnemyState.Attack);
            }
        }

        private void TickAttack(float dt)
        {
            motor.Stop();
            UpdateTelegraphVfx(1f);
            if (!attackResolved)
            {
                attackDriver.ResolveHit(definition, gameObject);
                attackResolved = true;
            }

            if (stateTimer >= definition.activeTime)
            {
                ChangeState(EnemyState.Recover);
            }
        }

        private void TickRecover(EnemyPerception perception)
        {
            motor.Stop();
            if (stateTimer < definition.recoveryTime)
            {
                return;
            }

            if (definition.retreatAfterAttack && perception.HasKnownTarget && definition.retreatDuration > 0f)
            {
                ChangeState(EnemyState.Retreat);
                return;
            }

            ChangeState(perception.CanEngage ? EnemyState.Chase : EnemyState.ReturnHome);
        }

        private void TickRetreat(EnemyPerception perception, float dt)
        {
            if (perception.OutsideLeash)
            {
                ChangeState(EnemyState.ReturnHome);
                return;
            }

            if (!perception.HasKnownTarget)
            {
                ChangeState(EnemyState.Search);
                return;
            }

            var away = transform.position - lastKnownTargetPosition;
            away.y = 0f;
            if (away.sqrMagnitude <= 0.0001f)
            {
                away = -transform.forward;
            }

            motor.MoveDirection(away.normalized, definition, dt);
            if (perception.DistanceToTarget >= definition.retreatDistance || stateTimer >= definition.retreatDuration)
            {
                ChangeState(perception.CanEngage ? EnemyState.Chase : EnemyState.Search);
            }
        }

        private void TickReturnHome(EnemyPerception perception, float dt)
        {
            if (perception.CanEngage)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            MoveToDestination(homePosition, dt);
            if (motor.HasArrived(homePosition, definition))
            {
                ChangeState(EnemyState.Search);
            }
        }

        private void MoveToDestination(Vector3 destination, float dt)
        {
            if (repathTimer > 0f && motor.UsesNavMesh)
            {
                return;
            }

            motor.MoveTo(destination, definition, dt);
            repathTimer = definition.repathInterval;
        }

        private void ChangeState(EnemyState next)
        {
            if (currentState == next)
            {
                return;
            }

            var previous = currentState;
            currentState = next;
            stateTimer = 0f;
            attackResolved = false;

            if (next == EnemyState.Attack)
            {
                attackCooldown = definition != null ? definition.cooldownTime : 0f;
            }

            animatorBridge.SetState(next);
            if (next == EnemyState.Telegraph)
            {
                animatorBridge.TriggerTelegraph();
                UpdateTelegraphVfx(0f);
            }
            else if (next == EnemyState.Attack)
            {
                animatorBridge.TriggerAttack();
            }
            else if (next == EnemyState.Dead)
            {
                animatorBridge.TriggerDeath();
            }

            if (next != EnemyState.Telegraph && next != EnemyState.Attack)
            {
                telegraphVfx?.Hide();
            }

            ApplyReadabilityTint(next);
            StateChanged?.Invoke(previous, next);
        }

        private void UpdateTelegraphVfx(float normalizedProgress)
        {
            if (telegraphVfx == null || definition == null)
            {
                return;
            }

            var origin = attackDriver != null && attackDriver.attackOrigin != null ? attackDriver.attackOrigin : transform;
            var forward = origin.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = transform.forward;
                forward.y = 0f;
            }
            forward = forward.sqrMagnitude <= 0.0001f ? Vector3.forward : forward.normalized;

            telegraphVfx.Show(definition, origin.position + forward * definition.attackRange, forward, normalizedProgress);
        }

        private void CacheComponents()
        {
            motor = motor != null ? motor : GetComponent<EnemyMotor>();
            sensor = sensor != null ? sensor : GetComponent<EnemySensor>();
            animatorBridge = animatorBridge != null ? animatorBridge : GetComponent<EnemyAnimatorBridge>();
            if (animatorBridge == null)
            {
                animatorBridge = gameObject.AddComponent<EnemyAnimatorBridge>();
            }
            attackDriver = attackDriver != null ? attackDriver : GetComponent<EnemyAttackDriver>();
            telegraphVfx = telegraphVfx != null ? telegraphVfx : GetComponent<EnemyTelegraphVfx>();
            damageable = damageable != null ? damageable : GetComponent<Damageable>();
            CacheReadabilityRenderers();
        }

        private void CacheReadabilityRenderers()
        {
            if (readabilityRenderers != null && readabilityRenderers.Length > 0)
            {
                return;
            }

            readabilityRenderers = GetComponentsInChildren<Renderer>();
        }

        private void ApplyReadabilityTint(EnemyState state)
        {
            if (!applyStateReadabilityTint)
            {
                ClearReadabilityTint();
                return;
            }

            switch (state)
            {
                case EnemyState.Telegraph:
                    SetReadabilityTint(telegraphTint);
                    break;
                case EnemyState.Attack:
                    SetReadabilityTint(attackTint);
                    break;
                case EnemyState.Recover:
                    SetReadabilityTint(recoverTint);
                    break;
                default:
                    ClearReadabilityTint();
                    break;
            }
        }

        private void SetReadabilityTint(Color color)
        {
            CacheReadabilityRenderers();
            if (readabilityRenderers == null || readabilityRenderers.Length == 0)
            {
                return;
            }

            if (readabilityBlock == null)
            {
                readabilityBlock = new MaterialPropertyBlock();
            }
            for (var index = 0; index < readabilityRenderers.Length; index++)
            {
                var targetRenderer = readabilityRenderers[index];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(readabilityBlock);
                readabilityBlock.SetColor(BaseColorProperty, color);
                readabilityBlock.SetColor(ColorProperty, color);
                targetRenderer.SetPropertyBlock(readabilityBlock);
            }
        }

        private void ClearReadabilityTint()
        {
            if (readabilityRenderers == null)
            {
                return;
            }

            for (var index = 0; index < readabilityRenderers.Length; index++)
            {
                if (readabilityRenderers[index] != null)
                {
                    readabilityRenderers[index].SetPropertyBlock(null);
                }
            }
        }

        private void HandleDamaged(Damageable target, DamageInfo info)
        {
            if (target.IsAlive)
            {
                animatorBridge.TriggerHit();
            }
        }

        private void HandleDied(Damageable target, DamageInfo info)
        {
            ChangeState(EnemyState.Dead);
        }

        private void OnDrawGizmosSelected()
        {
            if (definition == null || !definition.drawDebug)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.22f);
            Gizmos.DrawWireSphere(Application.isPlaying ? homePosition : transform.position, definition.leashRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.05f, lastKnownTargetPosition + Vector3.up * 0.05f);
        }
    }
}
