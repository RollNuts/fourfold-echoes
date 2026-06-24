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
        private const float AttackRange = 1.45f;
        private const float AttackCooldown = 0.38f;
        private const float AttackFlashDuration = 0.13f;
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

        private EchoPhase currentPhase = EchoPhase.Ember;
        private EchoPhase? lastHitPhase;
        private float chainTimer;
        private float attackCooldown;
        private float attackFlashTimer;
        private float dodgeTimer;
        private float dodgeCooldown;
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
        private bool gateOpen;
        private bool rewardClaimed;
        private string lastEvent = "Entered Ashen Threshold";
        private Vector3 facing = Vector3.right;
        private Vector3 dodgeDirection = Vector3.right;
        private Vector3 enemyKnockbackVelocity;
        private Vector3 enemyTelegraphDirection = Vector3.left;
        private Vector3 playerStartPosition;
        private Vector3 enemyStartPosition;
        private Transform enemyTellRing;
        private Transform enemyStrikeLane;
        private Transform dodgeReadyRing;
        private Transform playerInvulnerableHalo;
        private Transform attackArc;
        private Transform enemyHitBurst;
        private Transform enemyDeathBurst;
        private Transform altarPromptRing;
        private Transform gateOpenPulse;
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
            attackFlashTimer = Mathf.Max(0f, attackFlashTimer - dt);
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown - dt);
            chainTimer = Mathf.Max(0f, chainTimer - dt);
            enemyHitFlashTimer = Mathf.Max(0f, enemyHitFlashTimer - dt);
            enemyDeathTimer = Mathf.Max(0f, enemyDeathTimer - dt);
            gateOpenPulseTimer = Mathf.Max(0f, gateOpenPulseTimer - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);
            playerHitFlashTimer = Mathf.Max(0f, playerHitFlashTimer - dt);

            if (Input.GetKeyDown(KeyCode.R))
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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (dodgeCooldown <= 0f)
                {
                    BeginDodge();
                }
                else
                {
                    lastEvent = $"Dodge cooling {dodgeCooldown:0.0}s";
                }
            }

            if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
            {
                if (attackCooldown <= 0f)
                {
                    Attack();
                }
                else
                {
                    lastEvent = $"Attack recovering {attackCooldown:0.0}s";
                }
            }

            if (Input.GetKey(KeyCode.K) && currentPhase == EchoPhase.Ember && IsInRange(player, altarCore, AltarRange) && !gateOpen)
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

            if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)) && IsGateClaimReady())
            {
                rewardClaimed = true;
                lastEvent = "Recovered Ember Afterglow";
                proofAudio.Play(FourfoldProofAudioCue.Reward, 0.42f);
            }

            UpdatePresentation();
        }

        private void HandlePhaseInput()
        {
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                currentPhase = (EchoPhase)(((int)currentPhase + 1) % 4);
                ApplyPhaseMaterial();
                lastEvent = $"{currentPhase} phase";
                proofAudio.PlayPhase(currentPhase);
            }
            else if (Input.GetKeyDown(KeyCode.LeftBracket))
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
            player.position += movement * speed * dt;
            player.position = new Vector3(
                Mathf.Clamp(player.position.x, -4.4f, 5.4f),
                player.position.y,
                Mathf.Clamp(player.position.z, -2.8f, 2.8f)
            );
        }

        private void BeginDodge()
        {
            var input = ReadMoveInput();
            dodgeDirection = input.sqrMagnitude > 0.001f ? input.normalized : facing;
            facing = dodgeDirection;
            dodgeTimer = DodgeDuration;
            dodgeCooldown = DodgeCooldown;
            playerInvulnerableTimer = Mathf.Max(playerInvulnerableTimer, DodgeInvulnerableDuration);
            lastEvent = "Dodge i-frames";
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
                lastEvent = "Hollow winding up";
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
                lastEvent = "Hollow strike missed";
                return;
            }

            if (dodgeTimer > 0f || playerInvulnerableTimer > 0f)
            {
                lastEvent = "Dodge evaded hollow strike";
                return;
            }

            playerHealth = Mathf.Max(0f, playerHealth - EnemyDamage);
            playerInvulnerableTimer = PlayerInvulnerableDuration;
            playerHitFlashTimer = 0.16f;
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

        private void Attack()
        {
            attackCooldown = AttackCooldown;
            attackFlashTimer = AttackFlashDuration;
            proofAudio.Play(FourfoldProofAudioCue.Attack, 0.28f);

            if (enemyHealth <= 0f || !IsInRange(player, enemy, AttackRange))
            {
                lastEvent = "Attack whiff";
                return;
            }

            var combo = lastHitPhase.HasValue && lastHitPhase.Value != currentPhase && chainTimer > 0f;
            var damage = 25f + (combo ? 20f : 0f);
            enemyHealth = Mathf.Max(0f, enemyHealth - damage);
            enemyHitFlashTimer = EnemyHitFlashDuration;
            enemyKnockbackVelocity = KnockbackDirectionFromPlayerToEnemy() * (combo ? 5.4f : 3.6f);
            lastHitPhase = currentPhase;
            chainTimer = ChainWindow;

            if (combo)
            {
                altarHeat = gateOpen ? altarHeat : Mathf.Min(100f, altarHeat + 35f);
                lastEvent = $"{lastHitPhase.Value} chain surged altar {Mathf.RoundToInt(altarHeat)}%";
                proofAudio.Play(FourfoldProofAudioCue.Hit, 0.38f);
            }
            else
            {
                lastEvent = $"{currentPhase} hit";
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
            attackFlashTimer = 0f;
            dodgeTimer = 0f;
            dodgeCooldown = 0f;
            enemyWindupTimer = 0f;
            enemyRecoveryTimer = 0f;
            enemyHitFlashTimer = 0f;
            enemyDeathTimer = 0f;
            gateOpenPulseTimer = 0f;
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
            dodgeReadyRing = CreateIndicator("Dodge Cooldown Ring", PrimitiveType.Cylinder, tideMaterial, new Vector3(0.75f, 0.018f, 0.75f));
            playerInvulnerableHalo = CreateIndicator("Player Invulnerability Halo", PrimitiveType.Cylinder, gateReadyMaterial, new Vector3(0.92f, 0.02f, 0.92f));
            attackArc = CreateIndicator("Player Attack Arc", PrimitiveType.Cube, CurrentPhaseMaterial(), new Vector3(1.05f, 0.055f, 0.32f));
            enemyHitBurst = CreateIndicator("Enemy Hit Burst", PrimitiveType.Sphere, gateReadyMaterial, new Vector3(0.8f, 0.2f, 0.8f));
            enemyDeathBurst = CreateIndicator("Enemy Death Burst", PrimitiveType.Cylinder, enemyDeadMaterial, new Vector3(0.9f, 0.035f, 0.9f));
            altarPromptRing = CreateIndicator("Altar Ember Prompt", PrimitiveType.Cylinder, emberMaterial, new Vector3(1.1f, 0.018f, 1.1f));
            gateOpenPulse = CreateIndicator("Gate Open Pulse", PrimitiveType.Cylinder, gateReadyMaterial, new Vector3(0.8f, 0.025f, 0.8f));
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
            else
            {
                playerRenderer.sharedMaterial = CurrentPhaseMaterial();
            }

            if (dodgeReadyRing != null)
            {
                var showDodgeRing = dodgeCooldown > 0f || dodgeTimer > 0f;
                dodgeReadyRing.gameObject.SetActive(showDodgeRing);
                if (showDodgeRing)
                {
                    var progress = 1f - Mathf.Clamp01(dodgeCooldown / DodgeCooldown);
                    var size = Mathf.Lerp(0.52f, 1.05f, progress);
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
                    attackArc.position = player.position + direction * Mathf.Lerp(0.62f, 1.05f, progress);
                    attackArc.position = new Vector3(attackArc.position.x, 0.18f, attackArc.position.z);
                    attackArc.rotation = Quaternion.LookRotation(direction, Vector3.up);
                    attackArc.localScale = new Vector3(Mathf.Lerp(0.65f, 1.25f, progress), 0.055f, 0.32f);
                    attackArc.GetComponent<Renderer>().sharedMaterial = CurrentPhaseMaterial();
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
                    var hitPulse = enemyHitFlashTimer > 0f ? 0.14f : 0f;
                    enemy.localScale = new Vector3(scale + windupPulse + hitPulse, 1f + windupPulse + hitPulse, scale + windupPulse + hitPulse);
                    enemyRenderer.sharedMaterial = enemyHitFlashTimer > 0f && gateReadyMaterial != null ? gateReadyMaterial : enemyMaterial;
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
                    enemyStrikeLane.position = enemy.position + direction * ((EnemyStrikeRange + 0.35f) * 0.5f);
                    enemyStrikeLane.position = new Vector3(enemyStrikeLane.position.x, 0.08f, enemyStrikeLane.position.z);
                    enemyStrikeLane.rotation = Quaternion.LookRotation(direction, Vector3.up);
                    enemyStrikeLane.localScale = new Vector3(Mathf.Lerp(0.18f, 0.42f, progress), 0.035f, EnemyStrikeRange + 0.35f);
                }
            }

            var heat = altarHeat / 100f;
            var altarNearby = IsInRange(player, altarCore, AltarRange) && !gateOpen;
            var altarCanCharge = altarNearby && currentPhase == EchoPhase.Ember;
            var chargingAltar = altarCanCharge && Input.GetKey(KeyCode.K);
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
                var showGatePulse = gateOpenPulseTimer > 0f || IsGateClaimReady();
                gateOpenPulse.gameObject.SetActive(showGatePulse);
                if (showGatePulse)
                {
                    var progress = gateOpenPulseTimer > 0f ? 1f - gateOpenPulseTimer / GateOpenPulseDuration : 1f;
                    var pulse = IsGateClaimReady() ? 0.16f * Mathf.Sin(Time.time * 9f) : 0f;
                    var size = Mathf.Lerp(0.65f, 2.15f, progress) + pulse;
                    gateOpenPulse.position = new Vector3(3.45f, 0.04f, 0f);
                    gateOpenPulse.localScale = new Vector3(size, 0.025f, size);
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

        private static Vector3 ReadMoveInput()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
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

            var dodgeReady = dodgeCooldown <= 0f;
            var attackReady = attackCooldown <= 0f;
            var dodgeLabel = dodgeReady ? "Dodge READY" : $"Dodge {dodgeCooldown:0.0}s";
            var attackLabel = attackReady ? "Attack READY" : $"Attack {attackCooldown:0.0}s";
            var chainLabel = chainTimer > 0f ? $"Chain {chainTimer:0.0}s" : "Chain empty";

            GUI.Label(new Rect(24, 18, 520, 32), "FOURFOLD ECHOES - Unity room spike", style);
            GUI.Label(new Rect(24, 48, 760, 32), $"Phase: {currentPhase}   HP: {Mathf.RoundToInt(playerHealth)}   Enemy: {Mathf.RoundToInt(enemyHealth)}   Altar: {Mathf.RoundToInt(altarHeat)}%   Gate: {(gateOpen ? "Open" : "Closed")}", style);
            GUI.Label(new Rect(24, 78, 720, 32), $"Event: {lastEvent}", style);
            GUI.Label(new Rect(24, 108, 900, 32), ObjectiveText(), style);

            DrawBar(new Rect(24, 142, 180, 20), 1f - dodgeCooldown / DodgeCooldown, new Color(0.25f, 0.75f, 1f, 0.82f), dodgeLabel, barStyle);
            DrawBar(new Rect(216, 142, 180, 20), 1f - attackCooldown / AttackCooldown, new Color(1f, 0.55f, 0.22f, 0.82f), attackLabel, barStyle);
            DrawBar(new Rect(408, 142, 180, 20), chainTimer / ChainWindow, new Color(0.78f, 0.5f, 1f, 0.82f), chainLabel, barStyle);
            DrawBar(new Rect(600, 142, 220, 20), altarHeat / 100f, new Color(1f, 0.82f, 0.28f, 0.82f), $"Altar {Mathf.RoundToInt(altarHeat)}%", barStyle);
            if (enemyWindupTimer > 0f)
            {
                DrawBar(new Rect(24, 168, 260, 20), 1f - enemyWindupTimer / EnemyWindupDuration, new Color(1f, 0.18f, 0.18f, 0.88f), "Hollow strike tell", barStyle);
            }
            if (playerHealth <= 0f)
            {
                GUI.Label(new Rect(24, 194, 720, 32), "Downed - press R to reset the room", style);
            }
            GUI.Label(new Rect(24, Screen.height - 42, 980, 32), "Move WASD/Arrows | Attack J/Click | Dodge Space | Phase [ ] | Hold K at altar | Claim E/Right click | Reset R", style);
        }

        private string ObjectiveText()
        {
            if (playerHealth <= 0f)
            {
                return "Objective: reset and read the hollow tell";
            }
            if (rewardClaimed)
            {
                return "Objective: Ember Afterglow secured";
            }
            if (IsGateClaimReady())
            {
                return "Objective: claim the ready gate with E / right click";
            }
            if (gateOpen)
            {
                return enemyHealth > 0f ? "Objective: gate open - defeat the hollow" : "Objective: return to the gate";
            }
            if (enemyHealth <= 0f)
            {
                return "Objective: use the Ember altar to open the gate";
            }
            if (IsInRange(player, altarCore, AltarRange))
            {
                return currentPhase == EchoPhase.Ember ? "Objective: hold K to heat the altar" : "Objective: switch to Ember before using the altar";
            }
            return "Objective: build altar heat and break the hollow guard";
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
