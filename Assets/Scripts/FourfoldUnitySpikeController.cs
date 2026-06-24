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
        private const float AttackRange = 1.45f;
        private const float AttackCooldown = 0.38f;
        private const float ChainWindow = 1.35f;
        private const float AltarRange = 1.35f;
        private const float AltarHeatPerSecond = 34f;
        private const float EnemySenseRange = 4.2f;
        private const float EnemyStrikeRange = 1.05f;
        private const float EnemyMoveSpeed = 1.45f;
        private const float EnemyWindupDuration = 0.72f;
        private const float EnemyRecoveryDuration = 0.62f;
        private const float EnemyDamage = 18f;
        private const float PlayerMaxHealth = 100f;
        private const float PlayerInvulnerableDuration = 0.55f;

        private EchoPhase currentPhase = EchoPhase.Ember;
        private EchoPhase? lastHitPhase;
        private float chainTimer;
        private float attackCooldown;
        private float dodgeTimer;
        private float dodgeCooldown;
        private float enemyWindupTimer;
        private float enemyRecoveryTimer;
        private float playerHealth = PlayerMaxHealth;
        private float playerInvulnerableTimer;
        private float playerHitFlashTimer;
        private float altarHeat;
        private float enemyHealth = 70f;
        private bool gateOpen;
        private bool rewardClaimed;
        private string lastEvent = "Entered Ashen Threshold";
        private Vector3 facing = Vector3.right;
        private Vector3 playerStartPosition;
        private Vector3 enemyStartPosition;
        private Transform enemyTellRing;
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
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown - dt);
            chainTimer = Mathf.Max(0f, chainTimer - dt);
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

            if (Input.GetKeyDown(KeyCode.Space) && dodgeCooldown <= 0f)
            {
                dodgeTimer = DodgeDuration;
                dodgeCooldown = DodgeCooldown;
                lastEvent = "Dodge";
                proofAudio.Play(FourfoldProofAudioCue.Dodge, 0.26f);
            }

            if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && attackCooldown <= 0f)
            {
                Attack();
            }

            if (Input.GetKey(KeyCode.K) && currentPhase == EchoPhase.Ember && IsInRange(player, altarCore, AltarRange) && !gateOpen)
            {
                altarHeat = Mathf.Min(100f, altarHeat + AltarHeatPerSecond * dt);
                if (altarHeat >= 100f)
                {
                    gateOpen = true;
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
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }
            if (input.sqrMagnitude > 0.001f)
            {
                facing = input.normalized;
            }
            var speed = dodgeTimer > 0f ? DodgeSpeed : MoveSpeed;
            player.position += input * speed * dt;
            player.position = new Vector3(
                Mathf.Clamp(player.position.x, -4.4f, 5.4f),
                player.position.y,
                Mathf.Clamp(player.position.z, -2.8f, 2.8f)
            );
        }

        private void UpdateEnemy(float dt)
        {
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

            if (!IsInRange(player, enemy, EnemyStrikeRange + 0.18f))
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

        private void Attack()
        {
            attackCooldown = AttackCooldown;
            proofAudio.Play(FourfoldProofAudioCue.Attack, 0.28f);

            if (enemyHealth <= 0f || !IsInRange(player, enemy, AttackRange))
            {
                lastEvent = "Attack whiff";
                return;
            }

            var combo = lastHitPhase.HasValue && lastHitPhase.Value != currentPhase && chainTimer > 0f;
            var damage = 25f + (combo ? 20f : 0f);
            enemyHealth = Mathf.Max(0f, enemyHealth - damage);
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
                lastEvent = combo ? "Steam Burst surged altar 35%" : "Hollow down";
                proofAudio.Play(FourfoldProofAudioCue.RoomClear, 0.36f);
            }
        }

        private void ResetRoom()
        {
            currentPhase = EchoPhase.Ember;
            lastHitPhase = null;
            chainTimer = 0f;
            attackCooldown = 0f;
            dodgeTimer = 0f;
            dodgeCooldown = 0f;
            enemyWindupTimer = 0f;
            enemyRecoveryTimer = 0f;
            playerInvulnerableTimer = 0f;
            playerHitFlashTimer = 0f;
            altarHeat = 0f;
            enemyHealth = 70f;
            playerHealth = PlayerMaxHealth;
            gateOpen = false;
            rewardClaimed = false;
            lastEvent = "Room reset";
            facing = Vector3.right;
            nextAltarHeatAudio = 0f;
            player.position = playerStartPosition;
            enemy.position = enemyStartPosition;
            ApplyPhaseMaterial();
            UpdatePresentation();
        }

        private void ApplyPhaseMaterial()
        {
            var material = currentPhase switch
            {
                EchoPhase.Tide => tideMaterial,
                EchoPhase.Bloom => bloomMaterial,
                EchoPhase.Prism => prismMaterial,
                _ => emberMaterial
            };
            player.GetComponentInChildren<Renderer>().sharedMaterial = material != null ? material : playerMaterial;
        }

        private void CreateRuntimeIndicators()
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Enemy Strike Tell";
            ring.transform.localScale = new Vector3(1.2f, 0.025f, 1.2f);
            var collider = ring.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            ring.GetComponent<Renderer>().sharedMaterial = gateReadyMaterial;
            ring.SetActive(false);
            enemyTellRing = ring.transform;
        }

        private void UpdatePresentation()
        {
            var playerRenderer = player.GetComponentInChildren<Renderer>();
            if (playerHitFlashTimer > 0f && playerMaterial != null)
            {
                playerRenderer.sharedMaterial = playerMaterial;
            }
            else
            {
                ApplyPhaseMaterial();
            }

            var enemyAlive = enemyHealth > 0f;
            enemy.gameObject.SetActive(enemyAlive);
            if (enemyAlive)
            {
                var scale = Mathf.Lerp(0.45f, 1f, enemyHealth / 70f);
                var windupPulse = enemyWindupTimer > 0f ? 0.16f * Mathf.Sin(Time.time * 32f) : 0f;
                enemy.localScale = new Vector3(scale + windupPulse, 1f + windupPulse, scale + windupPulse);
                enemy.GetComponentInChildren<Renderer>().sharedMaterial = enemyMaterial;
            }

            if (enemyTellRing != null)
            {
                var showingTell = enemyAlive && enemyWindupTimer > 0f;
                enemyTellRing.gameObject.SetActive(showingTell);
                if (showingTell)
                {
                    var progress = 1f - enemyWindupTimer / EnemyWindupDuration;
                    enemyTellRing.position = new Vector3(enemy.position.x, 0.035f, enemy.position.z);
                    enemyTellRing.localScale = new Vector3(Mathf.Lerp(0.8f, 1.55f, progress), 0.025f, Mathf.Lerp(0.8f, 1.55f, progress));
                }
            }

            var heat = altarHeat / 100f;
            altarCore.localScale = new Vector3(0.85f, 0.35f + heat * 0.55f, 0.85f);
            altarGlow.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.75f, heat);
            altarGlow.gameObject.SetActive(heat > 0.01f && !gateOpen);

            gateLeft.localPosition = Vector3.Lerp(new Vector3(3.7f, 0.9f, -0.48f), new Vector3(3.45f, 0.9f, -0.75f), gateOpen ? 1f : 0f);
            gateRight.localPosition = Vector3.Lerp(new Vector3(3.7f, 0.9f, 0.48f), new Vector3(3.45f, 0.9f, 0.75f), gateOpen ? 1f : 0f);
            gateLeft.GetComponentInChildren<Renderer>().sharedMaterial = gateOpen ? gateOpenMaterial : gateClosedMaterial;
            gateRight.GetComponentInChildren<Renderer>().sharedMaterial = gateOpen ? gateOpenMaterial : gateClosedMaterial;
            gateClaimBadge.gameObject.SetActive(IsGateClaimReady());
            if (IsGateClaimReady())
            {
                gateClaimBadge.Rotate(Vector3.up, 70f * Time.deltaTime, Space.World);
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

        public void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(24, 18, 520, 32), "FOURFOLD ECHOES - Unity room spike", style);
            GUI.Label(new Rect(24, 48, 760, 32), $"Phase: {currentPhase}   HP: {Mathf.RoundToInt(playerHealth)}   Enemy: {Mathf.RoundToInt(enemyHealth)}   Altar: {Mathf.RoundToInt(altarHeat)}%   Gate: {(gateOpen ? "Open" : "Closed")}", style);
            GUI.Label(new Rect(24, 78, 720, 32), $"Event: {lastEvent}", style);
            if (playerHealth <= 0f)
            {
                GUI.Label(new Rect(24, 108, 720, 32), "Downed - press R to reset the room", style);
            }
            GUI.Label(new Rect(24, Screen.height - 42, 980, 32), "Move WASD/Arrows | Attack J/Click | Dodge Space | Phase [ ] | Hold K at altar | Claim E/Right click | Reset R", style);
            GUI.Label(new Rect(24, Screen.height - 70, 980, 32), "Audio proof: procedural runtime tones only; no external clips loaded", style);
        }
    }
}
