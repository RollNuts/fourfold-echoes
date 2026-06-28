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
        public const string ControlPromptText = "Move LS/WASD | Attack A/X/J | Dodge B/Space | Hold Altar Y/K | Claim Y/E | Phase LB/RB/[/] | Reset Start/R";
        public const string DownedPromptText = "Downed - press Start/R to reset the room";
        public const string CriticalHealthPromptText = "Critical HP - press B/Space through the tell, then create space";
        public const string EnemyWindupPromptText = "Dodge B/Space now";
        public const string EnemyRecoveryPromptText = "Attack A/X/J now";

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

        private const float MoveSpeed = 5.15f;
        private const float DodgeSpeed = 11.25f;
        private const float DodgeDuration = 0.22f;
        private const float DodgeCooldown = 0.42f;
        private const float DodgeInvulnerableDuration = 0.36f;
        private const float DodgeBufferDuration = 0.24f;
        private const float AttackRange = 1.82f;
        private const float AttackCooldown = 0.25f;
        private const float AttackFlashDuration = 0.16f;
        private const float AttackBufferDuration = 0.28f;
        private const float AttackMoveCommitment = 0.72f;
        private const float AttackLungeDuration = 0.1f;
        private const float AttackLungeSpeed = 4.4f;
        private const float AttackImpactDelay = 0.01f;
        private const float AttackDamage = 30f;
        private const float AttackKnockback = 4.6f;
        private const float AltarRange = 1.65f;
        private const float AltarHeatPerSecond = 62f;
        private const float EnemySenseRange = 5.7f;
        private const float EnemyStrikeRange = 1.0f;
        private const float EnemyMoveSpeed = 1.18f;
        private const float EnemyWindupDuration = 1.08f;
        private const float EnemyRecoveryDuration = 0.85f;
        private const float EnemyDamage = 14f;
        private const float EnemyMaxHealth = 70f;
        private const float EnemyHitFlashDuration = 0.2f;
        private const float EnemyDeathVisibleDuration = 0.85f;
        private const float EnemyKnockbackDamping = 9f;
        private const float PlayerMaxHealth = 100f;
        private const float CriticalHealthThreshold = 0.3f;
        private const float CriticalHealthOverlayAlpha = 0.08f;
        private const float PlayerInvulnerableDuration = 0.55f;
        private const float GateOpenPulseDuration = 1.1f;
        private const float RewardPickupDuration = 1.65f;
        private const float RoomCompleteDuration = 1.8f;
        private const float RoomMinX = -5.05f;
        private const float RoomMaxX = 5.2f;
        private const float RoomMinZ = -3.15f;
        private const float RoomMaxZ = 3.15f;
        private const float GateCenterX = 3.45f;
        private const float GateCenterZ = 0f;
        private const float GateRange = 1.45f;

        private EchoPhase currentPhase = EchoPhase.Ember;
        private float attackCooldown;
        private float attackBufferTimer;
        private float attackCommitTimer;
        private float attackFlashTimer;
        private float attackImpactTimer;
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
        private float altarBlockedTimer;
        private float gateBlockedTimer;
        private float roomCompleteTimer;
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
        private Transform altarLockRing;
        private Transform altarGateLink;
        private Transform hollowGateLock;
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
            attackLungeTimer = Mathf.Max(0f, attackLungeTimer - dt);
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown - dt);
            dodgeBufferTimer = Mathf.Max(0f, dodgeBufferTimer - dt);
            enemyHitFlashTimer = Mathf.Max(0f, enemyHitFlashTimer - dt);
            enemyDeathTimer = Mathf.Max(0f, enemyDeathTimer - dt);
            gateOpenPulseTimer = Mathf.Max(0f, gateOpenPulseTimer - dt);
            rewardPickupTimer = Mathf.Max(0f, rewardPickupTimer - dt);
            altarBlockedTimer = Mathf.Max(0f, altarBlockedTimer - dt);
            gateBlockedTimer = Mathf.Max(0f, gateBlockedTimer - dt);
            roomCompleteTimer = Mathf.Max(0f, roomCompleteTimer - dt);
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
                    lastEvent = "Attack queued";
                }
            }

            ProcessBufferedActions();

            if (AltarHeld() && IsInRange(player, altarCore, AltarRange) && !gateOpen)
            {
                if (IsAltarBlocked())
                {
                    altarBlockedTimer = 0.32f;
                    gateBlockedTimer = 0.32f;
                    lastEvent = "Hollow blocks the altar";
                }
                else
                {
                    altarHeat = Mathf.Min(100f, altarHeat + AltarHeatPerSecond * dt);
                    if (altarHeat >= 100f)
                    {
                        gateOpen = true;
                        gateOpenPulseTimer = GateOpenPulseDuration;
                        lastEvent = "Gate opened - claim reward";
                        proofAudio.Play(FourfoldProofAudioCue.GateOpen, 0.38f);
                    }
                    else if (Time.time >= nextAltarHeatAudio)
                    {
                        lastEvent = $"Gate opening {Mathf.RoundToInt(altarHeat)}%";
                        proofAudio.PlayAltarHeat(altarHeat / 100f);
                        nextAltarHeatAudio = Time.time + 0.32f;
                    }
                }
            }

            if (InteractPressed() && IsGateBlocked())
            {
                gateBlockedTimer = 0.45f;
                lastEvent = "Gate sealed by hollow";
            }

            if (InteractPressed() && IsGateClaimReady())
            {
                rewardClaimed = true;
                rewardPickupTimer = RewardPickupDuration;
                roomCompleteTimer = RoomCompleteDuration;
                gateOpenPulseTimer = GateOpenPulseDuration;
                lastEvent = "Room complete";
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
                player.position += attackLungeDirection * AttackLungeSpeed * dt;
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
            lastEvent = playerHealth <= 0f
                ? "Downed by hollow strike"
                : IsCriticalHealth(playerHealth, PlayerMaxHealth) ? CriticalHealthPromptText : "Hollow hit - read the tell";
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
                Mathf.Clamp(enemy.position.x, RoomMinX, RoomMaxX),
                enemy.position.y,
                Mathf.Clamp(enemy.position.z, RoomMinZ, RoomMaxZ)
            );
            enemyKnockbackVelocity = Vector3.Lerp(enemyKnockbackVelocity, Vector3.zero, EnemyKnockbackDamping * dt);
        }

        private bool IsPlayerInEnemyStrikeLane()
        {
            var toPlayer = player.position - enemy.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;
            if (distance > EnemyStrikeRange + 0.18f)
            {
                return false;
            }

            if (distance <= 0.15f)
            {
                return true;
            }

            var direction = enemyTelegraphDirection.sqrMagnitude > 0.001f ? enemyTelegraphDirection.normalized : toPlayer.normalized;
            return Vector3.Dot(direction, toPlayer.normalized) >= 0.45f;
        }

        private void BeginAttack()
        {
            attackCooldown = AttackCooldown;
            attackBufferTimer = 0f;
            attackCommitTimer = 0.11f;
            attackFlashTimer = AttackFlashDuration;
            attackImpactTimer = AttackImpactDelay;
            attackImpactPending = true;
            attackLungeTimer = AttackLungeDuration;
            attackLungeDirection = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.right;
            lastEvent = "Attack";
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
            if (enemyHealth <= 0f || !IsEnemyInAttackArc(AttackRange))
            {
                lastEvent = "Attack whiff";
                return;
            }

            enemyHealth = Mathf.Max(0f, enemyHealth - AttackDamage);
            enemyHitFlashTimer = EnemyHitFlashDuration;
            enemyKnockbackVelocity = KnockbackDirectionFromPlayerToEnemy() * AttackKnockback;
            lastEvent = "Hollow hit";
            proofAudio.Play(FourfoldProofAudioCue.Hit, 0.3f);

            if (enemyHealth <= 0f)
            {
                enemyHealth = 0f;
                enemyDeathTimer = EnemyDeathVisibleDuration;
                enemyWindupTimer = 0f;
                enemyRecoveryTimer = 0f;
                altarBlockedTimer = 0f;
                gateBlockedTimer = 0f;
                lastEvent = "Hollow down - altar unlocked";
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
            attackCooldown = 0f;
            attackBufferTimer = 0f;
            attackCommitTimer = 0f;
            attackFlashTimer = 0f;
            attackImpactTimer = 0f;
            attackLungeTimer = 0f;
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
            altarBlockedTimer = 0f;
            gateBlockedTimer = 0f;
            roomCompleteTimer = 0f;
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
            altarLockRing = CreateIndicator("Hollow Locks Altar", PrimitiveType.Cylinder, enemyMaterial, new Vector3(1.45f, 0.02f, 1.45f));
            altarGateLink = CreateIndicator("Altar Opens Gate Link", PrimitiveType.Cube, gateReadyMaterial, new Vector3(2.15f, 0.035f, 0.16f));
            hollowGateLock = CreateIndicator("Hollow Gate Lock", PrimitiveType.Cube, enemyMaterial, new Vector3(4.6f, 0.035f, 0.12f));
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
                    var progress = 1f - attackFlashTimer / AttackFlashDuration;
                    var direction = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.right;
                    attackArc.position = player.position + direction * Mathf.Lerp(0.58f, 1.24f, progress);
                    attackArc.position = new Vector3(attackArc.position.x, 0.2f, attackArc.position.z);
                    attackArc.rotation = Quaternion.LookRotation(direction, Vector3.up);
                    attackArc.localScale = new Vector3(Mathf.Lerp(0.85f, 1.55f, progress), 0.055f, 0.48f);
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
            var enemyAliveBlocking = IsAltarBlocked();
            var altarNearby = IsInRange(player, altarCore, AltarRange) && !gateOpen;
            var altarCanCharge = altarNearby && !enemyAliveBlocking;
            var chargingAltar = altarCanCharge && AltarHeld();
            var altarLockedPulse = enemyAliveBlocking ? 0.07f * Mathf.Sin(Time.time * 10f) : 0f;
            var altarPulse = chargingAltar ? 0.1f * Mathf.Sin(Time.time * 20f) : altarLockedPulse;
            altarCore.localScale = new Vector3(0.85f + altarPulse, 0.35f + heat * 0.55f + Mathf.Abs(altarPulse), 0.85f + altarPulse);
            if (altarMaterial != null && enemyAliveBlocking && altarBlockedTimer > 0f)
            {
                altarCore.GetComponent<Renderer>().sharedMaterial = enemyMaterial != null ? enemyMaterial : altarMaterial;
            }
            else
            {
                altarCore.GetComponent<Renderer>().sharedMaterial = altarMaterial;
            }
            altarGlow.gameObject.SetActive((heat > 0.01f && !gateOpen) || altarCanCharge || gateOpenPulseTimer > 0f || roomCompleteTimer > 0f);
            if (altarGlow.gameObject.activeSelf)
            {
                var glowPulse = altarCanCharge ? 0.14f * Mathf.Sin(Time.time * 14f) : 0f;
                var completePulse = roomCompleteTimer > 0f ? 0.35f * Mathf.Sin(Time.time * 12f) : 0f;
                altarGlow.localScale = Vector3.one * (Mathf.Lerp(0.9f, 1.95f, heat) + glowPulse + completePulse);
            }

            if (altarPromptRing != null)
            {
                var showPrompt = altarNearby || (heat > 0.01f && !gateOpen) || enemyAliveBlocking;
                altarPromptRing.gameObject.SetActive(showPrompt);
                if (showPrompt)
                {
                    var promptPulse = altarCanCharge ? 0.12f * Mathf.Sin(Time.time * 10f) : enemyAliveBlocking ? 0.08f * Mathf.Sin(Time.time * 16f) : 0f;
                    var size = Mathf.Lerp(1.0f, 1.62f, Mathf.Max(heat, 0.18f)) + promptPulse;
                    altarPromptRing.position = new Vector3(altarCore.position.x, 0.032f, altarCore.position.z);
                    altarPromptRing.localScale = new Vector3(size, 0.018f, size);
                    altarPromptRing.GetComponent<Renderer>().sharedMaterial = enemyAliveBlocking && enemyMaterial != null ? enemyMaterial : altarCanCharge && gateReadyMaterial != null ? gateReadyMaterial : emberMaterial;
                }
            }

            if (altarLockRing != null)
            {
                var showLock = enemyAliveBlocking || altarBlockedTimer > 0f;
                altarLockRing.gameObject.SetActive(showLock);
                if (showLock)
                {
                    var pulse = 0.12f * Mathf.Sin(Time.time * 12f);
                    var size = 1.5f + pulse;
                    altarLockRing.position = new Vector3(altarCore.position.x, 0.055f, altarCore.position.z);
                    altarLockRing.localScale = new Vector3(size, 0.02f, size);
                }
            }

            if (altarGateLink != null)
            {
                var showLink = !enemyAliveBlocking && (heat > 0.01f || gateOpen || rewardClaimed);
                altarGateLink.gameObject.SetActive(showLink);
                if (showLink)
                {
                    var linkPower = rewardClaimed ? 1f : gateOpen ? 0.9f : Mathf.Max(0.18f, heat);
                    var pulse = (chargingAltar || gateOpen || rewardClaimed) ? 0.1f * Mathf.Sin(Time.time * 18f) : 0f;
                    altarGateLink.position = new Vector3(2.38f, 0.075f, GateCenterZ);
                    altarGateLink.localScale = new Vector3(Mathf.Lerp(0.55f, 2.15f, linkPower), 0.035f, 0.16f + pulse);
                    altarGateLink.GetComponent<Renderer>().sharedMaterial = gateReadyMaterial != null ? gateReadyMaterial : emberMaterial;
                }
            }

            if (hollowGateLock != null)
            {
                var showGateLock = enemyAliveBlocking && !gateOpen;
                hollowGateLock.gameObject.SetActive(showGateLock);
                if (showGateLock)
                {
                    var pulse = gateBlockedTimer > 0f ? 0.08f * Mathf.Sin(Time.time * 24f) : 0f;
                    hollowGateLock.position = new Vector3(1.1f, 0.065f, GateCenterZ);
                    hollowGateLock.localScale = new Vector3(4.6f, 0.035f, 0.12f + pulse);
                }
            }

            gateLeft.localPosition = Vector3.Lerp(new Vector3(3.7f, 0.9f, -0.48f), new Vector3(3.45f, 0.9f, -0.75f), gateOpen ? 1f : 0f);
            gateRight.localPosition = Vector3.Lerp(new Vector3(3.7f, 0.9f, 0.48f), new Vector3(3.45f, 0.9f, 0.75f), gateOpen ? 1f : 0f);
            var gateMaterial = gateBlockedTimer > 0f && enemyMaterial != null ? enemyMaterial : IsGateClaimReady() && gateReadyMaterial != null ? gateReadyMaterial : gateOpen ? gateOpenMaterial : enemyAliveBlocking && enemyMaterial != null ? enemyMaterial : gateClosedMaterial;
            gateLeft.GetComponentInChildren<Renderer>().sharedMaterial = gateMaterial;
            gateRight.GetComponentInChildren<Renderer>().sharedMaterial = gateMaterial;
            gateClaimBadge.gameObject.SetActive(IsGateClaimReady() || rewardPickupTimer > 0f);
            if (IsGateClaimReady() || rewardPickupTimer > 0f)
            {
                var badgePulse = 1f + 0.22f * Mathf.Sin(Time.time * 12f);
                var badgeSize = rewardPickupTimer > 0f ? 0.48f : 0.32f;
                gateClaimBadge.localScale = new Vector3(badgeSize * badgePulse, badgeSize * badgePulse, 0.08f);
                gateClaimBadge.Rotate(Vector3.up, 110f * Time.deltaTime, Space.World);
            }

            if (gateOpenPulse != null)
            {
                var showGatePulse = gateOpenPulseTimer > 0f || IsGateClaimReady() || rewardPickupTimer > 0f || gateBlockedTimer > 0f;
                gateOpenPulse.gameObject.SetActive(showGatePulse);
                if (showGatePulse)
                {
                    var progress = gateOpenPulseTimer > 0f ? 1f - gateOpenPulseTimer / GateOpenPulseDuration : 1f;
                    var pulse = IsGateClaimReady() || rewardPickupTimer > 0f ? 0.2f * Mathf.Sin(Time.time * 9f) : gateBlockedTimer > 0f ? 0.1f * Mathf.Sin(Time.time * 18f) : 0f;
                    var size = Mathf.Lerp(0.65f, rewardPickupTimer > 0f ? 2.7f : 2.15f, progress) + pulse;
                    gateOpenPulse.position = new Vector3(GateCenterX, 0.04f, GateCenterZ);
                    gateOpenPulse.localScale = new Vector3(size, 0.025f, size);
                    gateOpenPulse.GetComponent<Renderer>().sharedMaterial = gateBlockedTimer > 0f && enemyMaterial != null ? enemyMaterial : gateReadyMaterial;
                }
            }

            if (rewardPickupBurst != null)
            {
                rewardPickupBurst.gameObject.SetActive(rewardPickupTimer > 0f);
                if (rewardPickupTimer > 0f)
                {
                    var progress = 1f - rewardPickupTimer / RewardPickupDuration;
                    var size = Mathf.Lerp(0.55f, 1.8f, progress);
                    rewardPickupBurst.position = player.position + Vector3.up * Mathf.Lerp(0.6f, 1.35f, progress);
                    rewardPickupBurst.localScale = Vector3.one * size;
                }
            }
        }

        private bool IsGateClaimReady()
        {
            return gateOpen && !rewardClaimed && enemyHealth <= 0f;
        }

        private bool IsAltarBlocked()
        {
            return enemyHealth > 0f;
        }

        private bool IsGateBlocked()
        {
            return enemyHealth > 0f && IsInRange(player.position, new Vector3(GateCenterX, player.position.y, GateCenterZ), GateRange);
        }

        private static bool IsInRange(Transform a, Transform b, float range)
        {
            var delta = a.position - b.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= range * range;
        }

        private static bool IsInRange(Vector3 a, Vector3 b, float range)
        {
            var delta = a - b;
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

            if (distance <= 0.65f)
            {
                return true;
            }

            var direction = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.right;
            return Vector3.Dot(direction, toEnemy.normalized) >= -0.05f;
        }

        private void ClampPlayerToRoom()
        {
            player.position = new Vector3(
                Mathf.Clamp(player.position.x, RoomMinX, RoomMaxX),
                player.position.y,
                Mathf.Clamp(player.position.z, RoomMinZ, RoomMaxZ)
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

            var hollowState = enemyHealth > 0f ? "Blocking" : "Down";
            var altarState = IsAltarBlocked() ? "Locked" : gateOpen ? "Opened" : "Ready";
            var gateState = IsGateClaimReady() ? "Ready" : gateOpen ? "Open" : "Sealed";

            var shouldShowDangerOverlay = playerHitFlashTimer > 0f
                || IsCriticalHealth(playerHealth, PlayerMaxHealth)
                || playerHealth <= 0f
                || roomCompleteTimer > 0f;
            if (shouldShowDangerOverlay)
            {
                var previousColor = GUI.color;
                GUI.color = roomCompleteTimer > 0f
                    ? new Color(1f, 0.68f, 0.08f, 0.18f)
                    : new Color(
                        0.7f,
                        0.02f,
                        0f,
                        CriticalHealthOverlayAlphaFor(playerHealth, PlayerMaxHealth, playerHitFlashTimer));
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = previousColor;
            }

            GUI.Label(new Rect(24, 18, 520, 32), "FOURFOLD ECHOES - Gate A", style);
            GUI.Label(new Rect(24, 48, 840, 32), $"HP {Mathf.RoundToInt(playerHealth)}   Hollow {hollowState}   Altar {altarState}   Gate {gateState}", style);
            GUI.Label(new Rect(24, 78, 900, 32), ObjectiveText(), style);
            GUI.Label(new Rect(24, 108, 760, 28), $"Cue: {lastEvent}", controlStyle);

            DrawBar(new Rect(24, 140, 260, 20), enemyHealth / EnemyMaxHealth, new Color(0.95f, 0.12f, 0.16f, 0.86f), enemyHealth > 0f ? "Hollow blocks progress" : "Hollow down", barStyle);
            DrawBar(new Rect(300, 140, 260, 20), altarHeat / 100f, new Color(1f, 0.82f, 0.28f, 0.86f), gateOpen ? "Gate open" : IsAltarBlocked() ? "Altar locked" : $"Open gate {Mathf.RoundToInt(altarHeat)}%", barStyle);
            if (enemyWindupTimer > 0f)
            {
                DrawBar(new Rect(24, 168, 300, 20), 1f - enemyWindupTimer / EnemyWindupDuration, new Color(1f, 0.18f, 0.18f, 0.88f), EnemyWindupPromptText, barStyle);
            }
            else if (enemyRecoveryTimer > 0f && enemyHealth > 0f)
            {
                DrawBar(new Rect(24, 168, 300, 20), enemyRecoveryTimer / EnemyRecoveryDuration, new Color(0.24f, 0.84f, 0.82f, 0.86f), EnemyRecoveryPromptText, barStyle);
            }
            if (roomCompleteTimer > 0f)
            {
                GUI.Label(new Rect(24, 196, 720, 32), "Room complete", style);
            }
            else if (rewardPickupTimer > 0f)
            {
                GUI.Label(new Rect(24, 194, 720, 32), "Ember Afterglow acquired", style);
            }
            if (playerHealth <= 0f)
            {
                GUI.Label(new Rect(24, 226, 720, 32), DownedPromptText, style);
            }
            else if (IsCriticalHealth(playerHealth, PlayerMaxHealth))
            {
                GUI.Label(new Rect(24, 226, 720, 32), CriticalHealthPromptText, style);
            }
            GUI.Label(new Rect(24, Screen.height - 42, Screen.width - 48, 32), ControlPromptText, controlStyle);
        }

        internal static bool IsCriticalHealth(float currentHealth, float maxHealth)
        {
            return currentHealth > 0f
                && maxHealth > 0f
                && Mathf.Clamp01(currentHealth / maxHealth) <= CriticalHealthThreshold;
        }

        internal static float CriticalHealthOverlayAlphaFor(float currentHealth, float maxHealth, float hitFlashTimer)
        {
            if (currentHealth <= 0f)
            {
                return 0.28f;
            }
            if (hitFlashTimer > 0f)
            {
                return 0.15f;
            }
            return IsCriticalHealth(currentHealth, maxHealth) ? CriticalHealthOverlayAlpha : 0f;
        }

        private string ObjectiveText()
        {
            if (playerHealth <= 0f)
            {
                return "Recover: reset and re-enter the fight";
            }
            if (rewardClaimed)
            {
                return "Room complete";
            }
            if (IsGateClaimReady())
            {
                return "Claim the reward at the open gate";
            }
            if (gateOpen)
            {
                return "Gate open - claim the reward";
            }
            if (enemyHealth <= 0f)
            {
                return "Hold the altar to open the gate";
            }
            if (IsInRange(player, altarCore, AltarRange))
            {
                return "Defeat the hollow to unlock the altar";
            }
            return "Defeat the hollow, open the gate, claim the reward";
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
