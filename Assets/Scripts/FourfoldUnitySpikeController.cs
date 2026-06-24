using UnityEngine;

namespace FourfoldEchoes.Spike
{
    public enum EchoPhase
    {
        Ember,
        Tide,
        Bloom,
        Prism
    }

    public sealed class FourfoldUnitySpikeController : MonoBehaviour
    {
        [Header("Scene")]
        public Transform player;
        public Transform enemy;
        public Transform altarCore;
        public Transform altarGlow;
        public Transform gateLeft;
        public Transform gateRight;
        public Transform gateClaimBadge;
        public Camera fixedCamera;

        [Header("Materials")]
        public Material emberMaterial;
        public Material tideMaterial;
        public Material bloomMaterial;
        public Material prismMaterial;
        public Material playerMaterial;
        public Material enemyMaterial;
        public Material enemyDeadMaterial;
        public Material altarMaterial;
        public Material gateClosedMaterial;
        public Material gateOpenMaterial;
        public Material gateReadyMaterial;

        private const float MoveSpeed = 4.6f;
        private const float DodgeSpeed = 9.5f;
        private const float DodgeDuration = 0.18f;
        private const float DodgeCooldown = 0.55f;
        private const float DodgeInvulnerableDuration = 0.24f;
        private const float DodgeBufferDuration = 0.18f;
        private const float AttackRange = 1.45f;
        private const float AttackCooldown = 0.38f;
        private const float AttackFlashDuration = 0.13f;
        private const float AttackBufferDuration = 0.24f;
        private const float AttackComboWindow = 0.92f;
        private const float AttackMoveCommitment = 0.38f;
        private const float AttackLungeDuration = 0.08f;
        private const float ChainWindow = 1.35f;
        private const float AltarRange = 1.35f;
        private const float AltarHeatPerSecond = 34f;
        private const float EnemySenseRange = 4.2f;
        private const float EnemyStrikeRange = 1.05f;
        private const float EnemyMoveSpeed = 1.45f;
        private const float EnemyWindupDuration = 0.9f;
        private const float EnemyRecoveryDuration = 0.62f;
        private const float EnemyDamage = 18f;
        private const float EnemyMaxHealth = 70f;
        private const float EnemyHitFlashDuration = 0.16f;
        private const float EnemyDeathVisibleDuration = 0.55f;
        private const float EnemyKnockbackDamping = 9f;
        private const float PlayerMaxHealth = 100f;
        private const float PlayerInvulnerableDuration = 0.55f;
        private const float GateOpenPulseDuration = 0.8f;
        private const float RewardPickupDuration = 1.25f;

        private EchoPhase currentPhase = EchoPhase.Ember;
        private EchoPhase? lastHitPhase;
        private float chainTimer;
        private float attackCooldown;
        private float attackBufferTimer;
        private float attackCommitTimer;
        private float attackFlashTimer;
        private float attackImpactTimer;
        private float attackComboTimer;
        private float attackLungeTimer;
        private float dodgeTimer;
        private float dodgeCooldown;
        private float dodgeBufferTimer;
        private float enemyWindupTimer;
        private float enemyRecoveryTimer;
        private float enemyHitFlashTimer;
        private float enemyDeathTimer;
        private float playerHealth = PlayerMaxHealth;
        private float playerInvulnerableTimer;
        private float playerHitFlashTimer;
        private float altarHeat;
        private float enemyHealth = EnemyMaxHealth;
        private float gateOpenPulseTimer;
        private float rewardPickupTimer;
        private int attackComboStep;
        private bool attackImpactPending;
        private bool gateOpen;
        private bool rewardClaimed;
        private string lastEvent = "Entered Ashen Threshold";
        private Vector3 facing = Vector3.right;
        private Vector3 dodgeDirection = Vector3.right;
        private Vector3 attackLungeDirection = Vector3.right;
        private Vector3 enemyKnockbackVelocity;
        private Vector3 enemyTelegraphDirection = Vector3.left;
        private Vector3 playerStartPosition;
        private Vector3 enemyStartPosition;
        private Transform enemyTellRing;
        private Transform enemyStrikeLane;
        private Transform enemyRecoveryRing;
        private Transform dodgeReadyRing;
        private Transform playerInvulnerableHalo;
        private Transform attackArc;
        private Transform enemyHitBurst;
        private Transform enemyDeathBurst;
        private Transform altarPromptRing;
        private Transform gateOpenPulse;
        private Transform rewardPickupBurst;
        private FourfoldProofAudio proofAudio;
        private float nextAltarHeatAudio;

        public void Awake()
        {
            playerStartPosition = player.position;
            enemyStartPosition = enemy.position;
            proofAudio = GetComponent<FourfoldProofAudio>();
            if (proofAudio == null)
            {
                proofAudio = gameObject.AddComponent<FourfoldProofAudio>();
            }
            CreateRuntimeIndicators();
            ApplyPhaseMaterial();
            UpdatePresentation();
        }

        public void Update()
        {
            var dt = Time.deltaTime;
            attackCooldown = Mathf.Max(0f, attackCooldown - dt);
            attackBufferTimer = Mathf.Max(0f, attackBufferTimer - dt);
            attackCommitTimer = Mathf.Max(0f, attackCommitTimer - dt);
            attackFlashTimer = Mathf.Max(0f, attackFlashTimer - dt);
            attackComboTimer = Mathf.Max(0f, attackComboTimer - dt);
            attackLungeTimer = Mathf.Max(0f, attackLungeTimer - dt);
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown - dt);
            dodgeBufferTimer = Mathf.Max(0f, dodgeBufferTimer - dt);
            chainTimer = Mathf.Max(0f, chainTimer - dt);
            enemyHitFlashTimer = Mathf.Max(0f, enemyHitFlashTimer - dt);
            enemyDeathTimer = Mathf.Max(0f, enemyDeathTimer - dt);
            gateOpenPulseTimer = Mathf.Max(0f, gateOpenPulseTimer - dt);
            rewardPickupTimer = Mathf.Max(0f, rewardPickupTimer - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);
            playerHitFlashTimer = Mathf.Max(0f, playerHitFlashTimer - dt);
            UpdateAttackImpact(dt);

            if (ResetPressed())
            {
                ResetRoom();
            }

            if (playerHealth <= 0f)
            {
                UpdatePresentation();
                return;
            }

            HandlePhaseInput();
            MovePlayer(dt);
            UpdateEnemy(dt);

            if (DodgePressed())
            {
                if (CanBeginDodge())
                {
                    BeginDodge();
                }
                else
                {
                    dodgeBufferTimer = DodgeBufferDuration;
                    lastEvent = dodgeCooldown > 0f ? $"Dodge cooling {dodgeCooldown:0.0}s" : "Dodge queued";
                }
            }

            if (AttackPressed())
            {
                if (CanBeginAttack())
                {
                    BeginAttack();
                }
                else
                {
                    attackBufferTimer = AttackBufferDuration;
                    lastEvent = attackComboTimer > 0f ? "Next swing queued" : $"Attack recovering {attackCooldown:0.0}s";
                }
            }

            ProcessBufferedActions();

            if (AltarHeld() && currentPhase == EchoPhase.Ember && IsInRange(player, altarCore, AltarRange) && !gateOpen)
            {
                altarHeat = Mathf.Min(100f, altarHeat + AltarHeatPerSecond * dt);
                if (altarHeat >= 100f)
                {
                    gateOpen = true;
                    gateOpenPulseTimer = GateOpenPulseDuration;
                    lastEvent = IsGateClaimReady() ? "Gate claim ready" : "Gate opened";
                    proofAudio.Play(FourfoldProofAudioCue.GateOpen, 0.38f);
                }
                else if (Time.time >= nextAltarHeatAudio)
                {
                    proofAudio.PlayAltarHeat(altarHeat / 100f);
                    nextAltarHeatAudio = Time.time + 0.42f;
                }
            }

            if (InteractPressed() && IsGateClaimReady())
            {
                rewardClaimed = true;
                rewardPickupTimer = RewardPickupDuration;
                gateOpenPulseTimer = GateOpenPulseDuration;
                lastEvent = "Ember Afterglow claimed";
                proofAudio.Play(FourfoldProofAudioCue.Reward, 0.42f);
            }

            UpdatePresentation();
        }

        private void HandlePhaseInput()
        {
            if (NextPhasePressed())
            {
                currentPhase = (EchoPhase)(((int)currentPhase + 1) % 4);
                ApplyPhaseMaterial();
                lastEvent = $"{currentPhase} phase";
                proofAudio.PlayPhase(currentPhase);
            }
            else if (PreviousPhasePressed())
            {
                currentPhase = (EchoPhase)(((int)currentPhase + 3) % 4);
                ApplyPhaseMaterial();
                lastEvent = $"{currentPhase} phase";
                proofAudio.PlayPhase(currentPhase);
            }
        }

        private void MovePlayer(float dt)
        {
            var input = ReadMoveInput();
            if (input.sqrMagnitude > 0.001f)
            {
                facing = input.normalized;
            }
            var movement = dodgeTimer > 0f ? dodgeDirection : input;
            var speed = dodgeTimer > 0f ? DodgeSpeed : MoveSpeed;
            if (attackCommitTimer > 0f && dodgeTimer <= 0f)
            {
                speed *= AttackMoveCommitment;
            }
            player.position += movement * speed * dt;
            if (attackLungeTimer > 0f && dodgeTimer <= 0f)
            {
                player.position += attackLungeDirection * AttackLungeSpeedForStep(attackComboStep) * dt;
            }
            ClampPlayerToRoom();
        }

        private void BeginDodge()
        {
            var input = ReadMoveInput();
            dodgeDirection = input.sqrMagnitude > 0.001f ? input.normalized : facing;
            facing = dodgeDirection;
            dodgeTimer = DodgeDuration;
            dodgeCooldown = DodgeCooldown;
            dodgeBufferTimer = 0f;
            attackBufferTimer = 0f;
            playerInvulnerableTimer = Mathf.Max(playerInvulnerableTimer, DodgeInvulnerableDuration);
            lastEvent = "Dodged through";
            proofAudio.Play(FourfoldProofAudioCue.Dodge, 0.26f);
        }

        private void UpdateEnemy(float dt)
        {
            ApplyEnemyKnockback(dt);

            if (enemyHealth <= 0f)
            {
                return;
            }

            if (enemyRecoveryTimer > 0f)
            {
                enemyRecoveryTimer = Mathf.Max(0f, enemyRecoveryTimer - dt);
                return;
            }

            var toPlayer = player.position - enemy.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;

            if (enemyWindupTimer > 0f)
            {
                enemyWindupTimer = Mathf.Max(0f, enemyWindupTimer - dt);
                if (enemyWindupTimer <= 0f)
                {
                    ResolveEnemyStrike();
                }
                return;
            }

            if (distance <= EnemyStrikeRange)
            {
                enemyTelegraphDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : enemyTelegraphDirection;
                enemyWindupTimer = EnemyWindupDuration;
                lastEvent = "Hollow strike incoming";
                proofAudio.Play(FourfoldProofAudioCue.EnemyTell, 0.22f);
                return;
            }

            if (distance <= EnemySenseRange && distance > 0.01f)
            {
                enemy.position += toPlayer.normalized * EnemyMoveSpeed * dt;
            }
        }

        private void ResolveEnemyStrike()
        {
            enemyWindupTimer = 0f;
            enemyRecoveryTimer = EnemyRecoveryDuration;

            if (!IsPlayerInEnemyStrikeLane())
            {
                lastEvent = "Hollow whiffed - punish now";
                return;
            }

            if (dodgeTimer > 0f || playerInvulnerableTimer > 0f)
            {
                lastEvent = "Evaded - hollow exposed";
                return;
            }

            playerHealth = Mathf.Max(0f, playerHealth - EnemyDamage);
            playerInvulnerableTimer = PlayerInvulnerableDuration;
            playerHitFlashTimer = 0.28f;
            lastEvent = playerHealth <= 0f ? "Downed by hollow strike" : "Hollow hit - read the tell";
            proofAudio.Play(FourfoldProofAudioCue.PlayerHit, 0.32f);
        }

        private void ApplyEnemyKnockback(float dt)
        {
            if (enemyKnockbackVelocity.sqrMagnitude <= 0.001f)
            {
                enemyKnockbackVelocity = Vector3.zero;
                return;
            }

            enemy.position += enemyKnockbackVelocity * dt;
            enemy.position = new Vector3(
                Mathf.Clamp(enemy.position.x, -4.4f, 5.4f),
                enemy.position.y,
                Mathf.Clamp(enemy.position.z, -2.8f, 2.8f)
            );
            enemyKnockbackVelocity = Vector3.Lerp(enemyKnockbackVelocity, Vector3.zero, EnemyKnockbackDamping * dt);
        }

        private bool IsPlayerInEnemyStrikeLane()
        {
            var toPlayer = player.position - enemy.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;
            if (distance > EnemyStrikeRange + 0.25f)
            {
                return false;
            }

            if (distance <= 0.15f)
            {
                return true;
            }

            var direction = enemyTelegraphDirection.sqrMagnitude > 0.001f ? enemyTelegraphDirection.normalized : toPlayer.normalized;
            return Vector3.Dot(direction, toPlayer.normalized) >= 0.35f;
        }

        private void BeginAttack()
        {
            if (attackComboTimer > 0f)
            {
                attackComboStep = (attackComboStep + 1) % 3;
            }
            else
            {
                attackComboStep = 0;
            }

            attackCooldown = AttackRecoveryForStep(attackComboStep);
            attackBufferTimer = 0f;
            attackCommitTimer = AttackCommitmentForStep(attackComboStep);
            attackFlashTimer = AttackFlashDurationForStep(attackComboStep);
            attackImpactTimer = AttackImpactDelayForStep(attackComboStep);
            attackImpactPending = true;
            attackComboTimer = AttackComboWindow;
            attackLungeTimer = AttackLungeDuration;
            attackLungeDirection = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.right;
            lastEvent = $"{AttackNameForStep(attackComboStep)} swing";
            proofAudio.Play(FourfoldProofAudioCue.Attack, 0.28f);
        }

        private void UpdateAttackImpact(float dt)
        {
            if (!attackImpactPending)
            {
                return;
            }

            if (playerHealth <= 0f)
            {
                attackImpactPending = false;
                return;
            }

            attackImpactTimer -= dt;
            if (attackImpactTimer > 0f)
            {
                return;
            }

            attackImpactPending = false;
            ResolveAttackImpact();
        }

        private void ResolveAttackImpact()
        {
            if (enemyHealth <= 0f || !IsEnemyInAttackArc(AttackRangeForStep(attackComboStep)))
            {
                lastEvent = $"{AttackNameForStep(attackComboStep)} whiff";
                return;
            }

            var combo = lastHitPhase.HasValue && lastHitPhase.Value != currentPhase && chainTimer > 0f;
            var damage = AttackDamageForStep(attackComboStep) + (combo ? 20f : 0f);
            enemyHealth = Mathf.Max(0f, enemyHealth - damage);
            enemyHitFlashTimer = EnemyHitFlashDuration;
            enemyKnockbackVelocity = KnockbackDirectionFromPlayerToEnemy() * (combo ? 5.8f : AttackKnockbackForStep(attackComboStep));
            lastHitPhase = currentPhase;
            chainTimer = ChainWindow;

            if (combo)
            {
                altarHeat = gateOpen ? altarHeat : Mathf.Min(100f, altarHeat + 35f);
                lastEvent = $"{AttackNameForStep(attackComboStep)} chain surged altar {Mathf.RoundToInt(altarHeat)}%";
                proofAudio.Play(FourfoldProofAudioCue.Hit, 0.38f);
            }
            else
            {
                lastEvent = $"{AttackNameForStep(attackComboStep)} hit";
                proofAudio.Play(FourfoldProofAudioCue.Hit, 0.24f);
            }

            if (enemyHealth <= 0f)
            {
                enemyHealth = 0f;
                enemyDeathTimer = EnemyDeathVisibleDuration;
                enemyWindupTimer = 0f;
                enemyRecoveryTimer = 0f;
                lastEvent = gateOpen ? "Hollow down - claim gate" : $"Hollow down - altar {Mathf.RoundToInt(altarHeat)}%";
                proofAudio.Play(FourfoldProofAudioCue.RoomClear, 0.36f);
            }
        }

        private Vector3 KnockbackDirectionFromPlayerToEnemy()
        {
            var direction = enemy.position - player.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : facing;
        }

        private void ResetRoom()
        {
            currentPhase = EchoPhase.Ember;
            lastHitPhase = null;
            chainTimer = 0f;
            attackCooldown = 0f;
            attackBufferTimer = 0f;
            attackCommitTimer = 0f;
            attackFlashTimer = 0f;
            attackImpactTimer = 0f;
            attackComboTimer = 0f;
            attackLungeTimer = 0f;
            attackComboStep = 0;
            attackImpactPending = false;
            dodgeTimer = 0f;
            dodgeCooldown = 0f;
            dodgeBufferTimer = 0f;
            enemyWindupTimer = 0f;
            enemyRecoveryTimer = 0f;
            enemyHitFlashTimer = 0f;
            enemyDeathTimer = 0f;
            gateOpenPulseTimer = 0f;
            rewardPickupTimer = 0f;
            playerInvulnerableTimer = 0f;
            playerHitFlashTimer = 0f;
            altarHeat = 0f;
            enemyHealth = EnemyMaxHealth;
            enemyKnockbackVelocity = Vector3.zero;
            playerHealth = PlayerMaxHealth;
            gateOpen = false;
            rewardClaimed = false;
            lastEvent = "Room reset";
            facing = Vector3.right;
            nextAltarHeatAudio = 0f;
            dodgeDirection = Vector3.right;
            attackLungeDirection = Vector3.right;
            enemyTelegraphDirection = Vector3.left;
            player.position = playerStartPosition;
            enemy.position = enemyStartPosition;
            enemy.gameObject.SetActive(true);
            ApplyPhaseMaterial();
            UpdatePresentation();
        }

        private void ApplyPhaseMaterial()
        {
            player.GetComponentInChildren<Renderer>().sharedMaterial = CurrentPhaseMaterial();
        }

        private Material CurrentPhaseMaterial()
        {
            var material = currentPhase switch
            {
                EchoPhase.Tide => tideMaterial,
                EchoPhase.Bloom => bloomMaterial,
                EchoPhase.Prism => prismMaterial,
                _ => emberMaterial
            };
            return material != null ? material : playerMaterial;
        }

        private void CreateRuntimeIndicators()
        {
            enemyTellRing = CreateIndicator("Enemy Strike Tell", PrimitiveType.Cylinder, gateReadyMaterial, new Vector3(1.2f, 0.025f, 1.2f));
            enemyStrikeLane = CreateIndicator("Enemy Strike Lane", PrimitiveType.Cube, enemyMaterial, new Vector3(0.24f, 0.035f, 1.2f));
            enemyRecoveryRing = CreateIndicator("Enemy Recovery Window", PrimitiveType.Cylinder, tideMaterial, new Vector3(1.15f, 0.02f, 1.15f));
            dodgeReadyRing = CreateIndicator("Dodge Cooldown Ring", PrimitiveType.Cylinder, tideMaterial, new Vector3(0.75f, 0.018f, 0.75f));
            playerInvulnerableHalo = CreateIndicator("Player Invulnerability Halo", PrimitiveType.Cylinder, gateReadyMaterial, new Vector3(0.92f, 0.02f, 0.92f));
            attackArc = CreateIndicator("Player Attack Arc", PrimitiveType.Cube, CurrentPhaseMaterial(), new Vector3(1.05f, 0.055f, 0.32f));
            enemyHitBurst = CreateIndicator("Enemy Hit Burst", PrimitiveType.Sphere, gateReadyMaterial, new Vector3(0.8f, 0.2f, 0.8f));
            enemyDeathBurst = CreateIndicator("Enemy Death Burst", PrimitiveType.Cylinder, enemyDeadMaterial, new Vector3(0.9f, 0.035f, 0.9f));
            altarPromptRing = CreateIndicator("Altar Ember Prompt", PrimitiveType.Cylinder, emberMaterial, new Vector3(1.1f, 0.018f, 1.1f));
            gateOpenPulse = CreateIndicator("Gate Open Pulse", PrimitiveType.Cylinder, gateReadyMaterial, new Vector3(0.8f, 0.025f, 0.8f));
            rewardPickupBurst = CreateIndicator("Reward Pickup Burst", PrimitiveType.Sphere, gateReadyMaterial, new Vector3(0.6f, 0.6f, 0.6f));
        }

        private static Transform CreateIndicator(string name, PrimitiveType type, Material material, Vector3 scale)
        {
            var indicator = GameObject.CreatePrimitive(type);
            indicator.name = name;
            indicator.transform.localScale = scale;
            var collider = indicator.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            var renderer = indicator.GetComponent<Renderer>();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
            indicator.SetActive(false);
            return indicator.transform;
        }

        private void UpdatePresentation()
        {
            var playerRenderer = player.GetComponentInChildren<Renderer>();
            if (playerHitFlashTimer > 0f && playerMaterial != null)
            {
                playerRenderer.sharedMaterial = playerMaterial;
            }
            else if ((dodgeTimer > 0f || playerInvulnerableTimer > 0f) && gateReadyMaterial != null && Mathf.Sin(Time.time * 24f) > 0f)
            {
                playerRenderer.sharedMaterial = gateReadyMaterial;
            }
            else if (rewardPickupTimer > 0f && gateReadyMaterial != null && Mathf.Sin(Time.time * 18f) > -0.25f)
            {
                playerRenderer.sharedMaterial = gateReadyMaterial;
            }
            else
            {
                playerRenderer.sharedMaterial = CurrentPhaseMaterial();
            }

            if (dodgeReadyRing != null)
            {
                var showDodgeRing = dodgeCooldown > 0f || dodgeTimer > 0f || dodgeBufferTimer > 0f;
                dodgeReadyRing.gameObject.SetActive(showDodgeRing);
                if (showDodgeRing)
                {
                    var progress = 1f - Mathf.Clamp01(dodgeCooldown / DodgeCooldown);
                    var queuedPulse = dodgeBufferTimer > 0f ? 0.14f * Mathf.Sin(Time.time * 28f) : 0f;
                    var size = Mathf.Lerp(0.52f, 1.05f, progress) + queuedPulse;
                    dodgeReadyRing.position = new Vector3(player.position.x, 0.028f, player.position.z);
                    dodgeReadyRing.localScale = new Vector3(size, 0.018f, size);
                }
            }

            if (playerInvulnerableHalo != null)
            {
                var showInvulnerable = dodgeTimer > 0f || playerInvulnerableTimer > 0f;
                playerInvulnerableHalo.gameObject.SetActive(showInvulnerable);
                if (showInvulnerable)
                {
                    var pulse = 0.08f * Mathf.Sin(Time.time * 34f);
                    playerInvulnerableHalo.position = new Vector3(player.position.x, 0.052f, player.position.z);
                    playerInvulnerableHalo.localScale = new Vector3(0.82f + pulse, 0.02f, 0.82f + pulse);
                }
            }

            if (attackArc != null)
            {
                attackArc.gameObject.SetActive(attackFlashTimer > 0f);
                if (attackFlashTimer > 0f)
                {
                    var progress = 1f - attackFlashTimer / AttackFlashDurationForStep(attackComboStep);
                    var direction = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.right;
                    var comboScale = 1f + attackComboStep * 0.18f;
                    attackArc.position = player.position + direction * Mathf.Lerp(0.58f, 1.12f + attackComboStep * 0.08f, progress);
                    attackArc.position = new Vector3(attackArc.position.x, 0.18f + attackComboStep * 0.02f, attackArc.position.z);
                    attackArc.rotation = Quaternion.LookRotation(direction, Vector3.up);
                    attackArc.localScale = new Vector3(Mathf.Lerp(0.7f, 1.35f, progress) * comboScale, 0.055f, 0.34f + attackComboStep * 0.08f);
                    attackArc.GetComponent<Renderer>().sharedMaterial = attackImpactPending && gateReadyMaterial != null ? gateReadyMaterial : CurrentPhaseMaterial();
                }
            }

            var enemyAlive = enemyHealth > 0f;
            var enemyVisible = enemyAlive || enemyDeathTimer > 0f;
            enemy.gameObject.SetActive(enemyVisible);
            if (enemyVisible)
            {
                var enemyRenderer = enemy.GetComponentInChildren<Renderer>();
                if (enemyAlive)
                {
                    var scale = Mathf.Lerp(0.45f, 1f, enemyHealth / EnemyMaxHealth);
                    var windupProgress = enemyWindupTimer > 0f ? 1f - enemyWindupTimer / EnemyWindupDuration : 0f;
                    var windupPulse = enemyWindupTimer > 0f ? Mathf.Lerp(0.04f, 0.18f, windupProgress) * Mathf.Sin(Time.time * 28f) : 0f;
                    var recoverySquash = enemyRecoveryTimer > 0f ? -0.08f : 0f;
                    var hitPulse = enemyHitFlashTimer > 0f ? 0.14f : 0f;
                    enemy.localScale = new Vector3(scale + windupPulse + hitPulse, 1f + windupPulse + hitPulse + recoverySquash, scale + windupPulse + hitPulse);
                    if (enemyHitFlashTimer > 0f && gateReadyMaterial != null)
                    {
                        enemyRenderer.sharedMaterial = gateReadyMaterial;
                    }
                    else if (enemyRecoveryTimer > 0f && tideMaterial != null)
                    {
                        enemyRenderer.sharedMaterial = tideMaterial;
                    }
                    else
                    {
                        enemyRenderer.sharedMaterial = enemyMaterial;
                    }
                }
                else
                {
                    var deathProgress = 1f - enemyDeathTimer / EnemyDeathVisibleDuration;
                    var scale = Mathf.Lerp(0.9f, 0.45f, deathProgress);
                    enemy.localScale = new Vector3(scale, Mathf.Lerp(0.75f, 0.18f, deathProgress), scale);
                    enemyRenderer.sharedMaterial = enemyDeadMaterial != null ? enemyDeadMaterial : enemyMaterial;
                }
            }

            if (enemyHitBurst != null)
            {
                var showHitBurst = enemyHitFlashTimer > 0f && enemyAlive;
                enemyHitBurst.gameObject.SetActive(showHitBurst);
                if (showHitBurst)
                {
                    var progress = 1f - enemyHitFlashTimer / EnemyHitFlashDuration;
                    var size = Mathf.Lerp(0.45f, 0.95f, progress);
                    enemyHitBurst.position = new Vector3(enemy.position.x, 0.72f, enemy.position.z);
                    enemyHitBurst.localScale = new Vector3(size, 0.18f, size);
                }
            }

            if (enemyDeathBurst != null)
            {
                enemyDeathBurst.gameObject.SetActive(enemyDeathTimer > 0f);
                if (enemyDeathTimer > 0f)
                {
                    var progress = 1f - enemyDeathTimer / EnemyDeathVisibleDuration;
                    var size = Mathf.Lerp(0.7f, 1.9f, progress);
                    enemyDeathBurst.position = new Vector3(enemy.position.x, 0.05f, enemy.position.z);
                    enemyDeathBurst.localScale = new Vector3(size, 0.035f, size);
                }
            }

            if (enemyTellRing != null)
            {
                var showingTell = enemyAlive && enemyWindupTimer > 0f;
                enemyTellRing.gameObject.SetActive(showingTell);
                if (showingTell)
                {
                    var progress = 1f - enemyWindupTimer / EnemyWindupDuration;
                    enemyTellRing.position = new Vector3(enemy.position.x, 0.035f, enemy.position.z);
                    var size = Mathf.Lerp(0.8f, 1.7f, progress);
                    enemyTellRing.localScale = new Vector3(size, 0.025f, size);
                    enemyTellRing.GetComponent<Renderer>().sharedMaterial = progress > 0.66f && enemyMaterial != null ? enemyMaterial : gateReadyMaterial;
                }
            }

            if (enemyStrikeLane != null)
            {
                var showingLane = enemyAlive && enemyWindupTimer > 0f;
                enemyStrikeLane.gameObject.SetActive(showingLane);
                if (showingLane)
                {
                    var progress = 1f - enemyWindupTimer / EnemyWindupDuration;
                    var direction = enemyTelegraphDirection.sqrMagnitude > 0.001f ? enemyTelegraphDirection.normalized : Vector3.left;
                    enemyStrikeLane.position = enemy.position + direction * ((EnemyStrikeRange + 0.5f) * 0.5f);
                    enemyStrikeLane.position = new Vector3(enemyStrikeLane.position.x, 0.08f, enemyStrikeLane.position.z);
                    enemyStrikeLane.rotation = Quaternion.LookRotation(direction, Vector3.up);
                    enemyStrikeLane.localScale = new Vector3(Mathf.Lerp(0.24f, 0.58f, progress), 0.035f, EnemyStrikeRange + 0.5f);
                }
            }

            if (enemyRecoveryRing != null)
            {
                var showingRecovery = enemyAlive && enemyRecoveryTimer > 0f && enemyWindupTimer <= 0f;
                enemyRecoveryRing.gameObject.SetActive(showingRecovery);
                if (showingRecovery)
                {
                    var progress = 1f - enemyRecoveryTimer / EnemyRecoveryDuration;
                    var pulse = 0.08f * Mathf.Sin(Time.time * 18f);
                    var size = Mathf.Lerp(1.45f, 0.82f, progress) + pulse;
                    enemyRecoveryRing.position = new Vector3(enemy.position.x, 0.04f, enemy.position.z);
                    enemyRecoveryRing.localScale = new Vector3(size, 0.02f, size);
                    enemyRecoveryRing.GetComponent<Renderer>().sharedMaterial = tideMaterial != null ? tideMaterial : gateReadyMaterial;
                }
            }

            var heat = altarHeat / 100f;
            var altarNearby = IsInRange(player, altarCore, AltarRange) && !gateOpen;
            var altarCanCharge = altarNearby && currentPhase == EchoPhase.Ember;
            var chargingAltar = altarCanCharge && AltarHeld();
            var altarPulse = chargingAltar ? 0.08f * Mathf.Sin(Time.time * 20f) : 0f;
            altarCore.localScale = new Vector3(0.85f + altarPulse, 0.35f + heat * 0.55f + altarPulse, 0.85f + altarPulse);
            altarGlow.gameObject.SetActive((heat > 0.01f && !gateOpen) || altarCanCharge || gateOpenPulseTimer > 0f);
            if (altarGlow.gameObject.activeSelf)
            {
                var glowPulse = altarCanCharge ? 0.12f * Mathf.Sin(Time.time * 14f) : 0f;
                altarGlow.localScale = Vector3.one * (Mathf.Lerp(0.9f, 1.85f, heat) + glowPulse);
            }

            if (altarPromptRing != null)
            {
                var showPrompt = altarNearby || (heat > 0.01f && !gateOpen);
                altarPromptRing.gameObject.SetActive(showPrompt);
                if (showPrompt)
                {
                    var promptPulse = altarCanCharge ? 0.12f * Mathf.Sin(Time.time * 10f) : 0f;
                    var size = Mathf.Lerp(1.0f, 1.55f, Mathf.Max(heat, 0.18f)) + promptPulse;
                    altarPromptRing.position = new Vector3(altarCore.position.x, 0.032f, altarCore.position.z);
                    altarPromptRing.localScale = new Vector3(size, 0.018f, size);
                    altarPromptRing.GetComponent<Renderer>().sharedMaterial = altarCanCharge && gateReadyMaterial != null ? gateReadyMaterial : emberMaterial;
                }
            }

            gateLeft.localPosition = Vector3.Lerp(new Vector3(3.7f, 0.9f, -0.48f), new Vector3(3.45f, 0.9f, -0.75f), gateOpen ? 1f : 0f);
            gateRight.localPosition = Vector3.Lerp(new Vector3(3.7f, 0.9f, 0.48f), new Vector3(3.45f, 0.9f, 0.75f), gateOpen ? 1f : 0f);
            var gateMaterial = IsGateClaimReady() && gateReadyMaterial != null ? gateReadyMaterial : gateOpen ? gateOpenMaterial : gateClosedMaterial;
            gateLeft.GetComponentInChildren<Renderer>().sharedMaterial = gateMaterial;
            gateRight.GetComponentInChildren<Renderer>().sharedMaterial = gateMaterial;
            gateClaimBadge.gameObject.SetActive(IsGateClaimReady());
            if (IsGateClaimReady())
            {
                var badgePulse = 1f + 0.16f * Mathf.Sin(Time.time * 12f);
                gateClaimBadge.localScale = new Vector3(0.32f * badgePulse, 0.32f * badgePulse, 0.08f);
                gateClaimBadge.Rotate(Vector3.up, 110f * Time.deltaTime, Space.World);
            }

            if (gateOpenPulse != null)
            {
                var showGatePulse = gateOpenPulseTimer > 0f || IsGateClaimReady() || rewardPickupTimer > 0f;
                gateOpenPulse.gameObject.SetActive(showGatePulse);
                if (showGatePulse)
                {
                    var progress = gateOpenPulseTimer > 0f ? 1f - gateOpenPulseTimer / GateOpenPulseDuration : 1f;
                    var pulse = IsGateClaimReady() || rewardPickupTimer > 0f ? 0.16f * Mathf.Sin(Time.time * 9f) : 0f;
                    var size = Mathf.Lerp(0.65f, 2.15f, progress) + pulse;
                    gateOpenPulse.position = new Vector3(3.45f, 0.04f, 0f);
                    gateOpenPulse.localScale = new Vector3(size, 0.025f, size);
                }
            }

            if (rewardPickupBurst != null)
            {
                rewardPickupBurst.gameObject.SetActive(rewardPickupTimer > 0f);
                if (rewardPickupTimer > 0f)
                {
                    var progress = 1f - rewardPickupTimer / RewardPickupDuration;
                    var size = Mathf.Lerp(0.35f, 1.15f, progress);
                    rewardPickupBurst.position = player.position + Vector3.up * Mathf.Lerp(0.55f, 1.15f, progress);
                    rewardPickupBurst.localScale = Vector3.one * size;
                }
            }
        }

        private bool IsGateClaimReady()
        {
            return gateOpen && !rewardClaimed && enemyHealth <= 0f;
        }

        private static bool IsInRange(Transform a, Transform b, float range)
        {
            var delta = a.position - b.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= range * range;
        }

        private bool CanBeginAttack()
        {
            return playerHealth > 0f && attackCooldown <= 0f && dodgeTimer <= 0f;
        }

        private bool CanBeginDodge()
        {
            return playerHealth > 0f && dodgeCooldown <= 0f && dodgeTimer <= 0f && attackCommitTimer <= 0f;
        }

        private void ProcessBufferedActions()
        {
            if (dodgeBufferTimer > 0f && CanBeginDodge())
            {
                BeginDodge();
            }
            else if (attackBufferTimer > 0f && CanBeginAttack())
            {
                BeginAttack();
            }
        }

        private bool IsEnemyInAttackArc(float range)
        {
            var toEnemy = enemy.position - player.position;
            toEnemy.y = 0f;
            var distance = toEnemy.magnitude;
            if (distance > range)
            {
                return false;
            }

            if (distance <= 0.45f)
            {
                return true;
            }

            var direction = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.right;
            var forgivingArc = attackComboStep >= 2 ? 0.08f : 0.22f;
            return Vector3.Dot(direction, toEnemy.normalized) >= forgivingArc;
        }

        private void ClampPlayerToRoom()
        {
            player.position = new Vector3(
                Mathf.Clamp(player.position.x, -4.4f, 5.4f),
                player.position.y,
                Mathf.Clamp(player.position.z, -2.8f, 2.8f)
            );
        }

        private static bool AttackPressed()
        {
            return Input.GetKeyDown(KeyCode.J)
                || Input.GetMouseButtonDown(0)
                || Input.GetKeyDown(KeyCode.JoystickButton0)
                || Input.GetKeyDown(KeyCode.JoystickButton2);
        }

        private static bool DodgePressed()
        {
            return Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.JoystickButton1);
        }

        private static bool AltarHeld()
        {
            return Input.GetKey(KeyCode.K)
                || Input.GetKey(KeyCode.JoystickButton3);
        }

        private static bool InteractPressed()
        {
            return Input.GetKeyDown(KeyCode.E)
                || Input.GetMouseButtonDown(1)
                || Input.GetKeyDown(KeyCode.JoystickButton3);
        }

        private static bool ResetPressed()
        {
            return Input.GetKeyDown(KeyCode.R)
                || Input.GetKeyDown(KeyCode.JoystickButton7);
        }

        private static bool NextPhasePressed()
        {
            return Input.GetKeyDown(KeyCode.RightBracket)
                || Input.GetKeyDown(KeyCode.JoystickButton5);
        }

        private static bool PreviousPhasePressed()
        {
            return Input.GetKeyDown(KeyCode.LeftBracket)
                || Input.GetKeyDown(KeyCode.JoystickButton4);
        }

        private static string AttackNameForStep(int step)
        {
            return step switch
            {
                1 => "Cross cut",
                2 => "Heavy cleave",
                _ => "Quick cut"
            };
        }

        private static float AttackRecoveryForStep(int step)
        {
            return step switch
            {
                1 => AttackCooldown + 0.04f,
                2 => AttackCooldown + 0.16f,
                _ => AttackCooldown - 0.06f
            };
        }

        private static float AttackCommitmentForStep(int step)
        {
            return step switch
            {
                1 => 0.24f,
                2 => 0.34f,
                _ => 0.20f
            };
        }

        private static float AttackFlashDurationForStep(int step)
        {
            return step switch
            {
                1 => AttackFlashDuration + 0.08f,
                2 => AttackFlashDuration + 0.14f,
                _ => AttackFlashDuration + 0.05f
            };
        }

        private static float AttackImpactDelayForStep(int step)
        {
            return step switch
            {
                1 => 0.075f,
                2 => 0.10f,
                _ => 0.06f
            };
        }

        private static float AttackDamageForStep(int step)
        {
            return step switch
            {
                1 => 22f,
                2 => 32f,
                _ => 18f
            };
        }

        private static float AttackRangeForStep(int step)
        {
            return step switch
            {
                1 => AttackRange + 0.12f,
                2 => AttackRange + 0.28f,
                _ => AttackRange
            };
        }

        private static float AttackKnockbackForStep(int step)
        {
            return step switch
            {
                1 => 4.1f,
                2 => 5.2f,
                _ => 3.4f
            };
        }

        private static float AttackLungeSpeedForStep(int step)
        {
            return step switch
            {
                1 => 3.2f,
                2 => 3.8f,
                _ => 2.8f
            };
        }

        private static Vector3 ReadMoveInput()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude < 0.04f)
            {
                return Vector3.zero;
            }
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }
            return input;
        }

        public void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            var barStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };
            var controlStyle = new GUIStyle(style)
            {
                fontSize = 14
            };

            var dodgeReady = dodgeCooldown <= 0f;
            var attackReady = attackCooldown <= 0f;
            var dodgeLabel = dodgeBufferTimer > 0f ? "Dodge queued" : dodgeReady ? "Dodge READY" : $"Dodge {dodgeCooldown:0.0}s";
            var attackLabel = attackBufferTimer > 0f ? "Swing queued" : attackReady ? "Attack READY" : $"{AttackNameForStep(attackComboStep)} {attackCooldown:0.0}s";
            var comboLabel = attackComboTimer > 0f ? $"Combo {attackComboStep + 1}/3" : "Combo ready";
            var chainLabel = chainTimer > 0f ? $"Phase chain {chainTimer:0.0}s" : comboLabel;
            var chainProgress = chainTimer > 0f ? chainTimer / ChainWindow : attackComboTimer / AttackComboWindow;
            var gateState = IsGateClaimReady() ? "Ready" : gateOpen ? "Open" : "Sealed";

            if (playerHitFlashTimer > 0f || playerHealth <= 0f)
            {
                var previousColor = GUI.color;
                GUI.color = new Color(0.7f, 0.02f, 0f, playerHealth <= 0f ? 0.28f : 0.15f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = previousColor;
            }

            GUI.Label(new Rect(24, 18, 520, 32), "FOURFOLD ECHOES - Gate A", style);
            GUI.Label(new Rect(24, 48, 800, 32), $"{currentPhase} Echo   HP {Mathf.RoundToInt(playerHealth)}   Hollow {Mathf.RoundToInt(enemyHealth)}   Altar {Mathf.RoundToInt(altarHeat)}%   Gate {gateState}", style);
            GUI.Label(new Rect(24, 78, 760, 32), $"Cue: {lastEvent}", style);
            GUI.Label(new Rect(24, 108, 900, 32), ObjectiveText(), style);

            DrawBar(new Rect(24, 142, 180, 20), 1f - dodgeCooldown / DodgeCooldown, new Color(0.25f, 0.75f, 1f, 0.82f), dodgeLabel, barStyle);
            DrawBar(new Rect(216, 142, 180, 20), 1f - attackCooldown / AttackRecoveryForStep(attackComboStep), new Color(1f, 0.55f, 0.22f, 0.82f), attackLabel, barStyle);
            DrawBar(new Rect(408, 142, 180, 20), chainProgress, new Color(0.78f, 0.5f, 1f, 0.82f), chainLabel, barStyle);
            DrawBar(new Rect(600, 142, 220, 20), altarHeat / 100f, new Color(1f, 0.82f, 0.28f, 0.82f), $"Altar {Mathf.RoundToInt(altarHeat)}%", barStyle);
            if (enemyWindupTimer > 0f)
            {
                DrawBar(new Rect(24, 168, 300, 20), 1f - enemyWindupTimer / EnemyWindupDuration, new Color(1f, 0.18f, 0.18f, 0.88f), "Hollow strike incoming", barStyle);
            }
            else if (enemyRecoveryTimer > 0f && enemyHealth > 0f)
            {
                DrawBar(new Rect(24, 168, 300, 20), enemyRecoveryTimer / EnemyRecoveryDuration, new Color(0.24f, 0.84f, 0.82f, 0.86f), "Hollow exposed", barStyle);
            }
            if (rewardPickupTimer > 0f)
            {
                GUI.Label(new Rect(24, 194, 720, 32), "Ember Afterglow acquired", style);
            }
            if (playerHealth <= 0f)
            {
                GUI.Label(new Rect(24, 226, 720, 32), "Downed - press R to reset the room", style);
            }
            GUI.Label(new Rect(24, Screen.height - 42, Screen.width - 48, 32), "Move WASD/Arrows/Stick | Attack J/Click/Pad | Dodge Space/Pad | Phase [ ]/Shoulders | Altar K/Pad | Claim E/Right/Pad | Reset R/Start", controlStyle);
        }

        private string ObjectiveText()
        {
            if (playerHealth <= 0f)
            {
                return "Recover: reset and re-enter the fight";
            }
            if (rewardClaimed)
            {
                return "Reward secured: Ember Afterglow";
            }
            if (IsGateClaimReady())
            {
                return "Gate ready: claim the Ember Afterglow";
            }
            if (gateOpen)
            {
                return enemyHealth > 0f ? "Gate open: finish the hollow" : "Return to the gate";
            }
            if (enemyHealth <= 0f)
            {
                return "Use the Ember altar to open the gate";
            }
            if (IsInRange(player, altarCore, AltarRange))
            {
                return currentPhase == EchoPhase.Ember ? "Hold the altar action to kindle the gate" : "Switch to Ember to kindle the altar";
            }
            return "Chain weapon hits, read the hollow tell, and kindle the gate";
        }

        private static void DrawBar(Rect rect, float normalized, Color fillColor, string label, GUIStyle style)
        {
            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.48f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(normalized), rect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 6f, rect.y, rect.width - 10f, rect.height), label, style);
            GUI.color = previousColor;
        }
    }
}
