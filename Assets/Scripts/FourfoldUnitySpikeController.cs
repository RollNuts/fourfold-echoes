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

        private EchoPhase currentPhase = EchoPhase.Ember;
        private EchoPhase? lastHitPhase;
        private float chainTimer;
        private float attackCooldown;
        private float dodgeTimer;
        private float dodgeCooldown;
        private float altarHeat;
        private float enemyHealth = 70f;
        private bool gateOpen;
        private bool rewardClaimed;
        private string lastEvent = "Entered Ashen Threshold";
        private Vector3 facing = Vector3.right;

        private AudioSource audioSource;
        private AudioClip attackTone;
        private AudioClip comboTone;
        private AudioClip gateTone;
        private AudioClip rewardTone;

        public void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            attackTone = CreateTone("AttackTone", 220f, 0.07f);
            comboTone = CreateTone("ComboTone", 440f, 0.12f);
            gateTone = CreateTone("GateTone", 330f, 0.16f);
            rewardTone = CreateTone("RewardTone", 660f, 0.2f);
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

            HandlePhaseInput();
            MovePlayer(dt);

            if (Input.GetKeyDown(KeyCode.Space) && dodgeCooldown <= 0f)
            {
                dodgeTimer = DodgeDuration;
                dodgeCooldown = DodgeCooldown;
                lastEvent = "Dodge";
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
                    audioSource.PlayOneShot(gateTone, 0.38f);
                }
            }

            if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)) && IsGateClaimReady())
            {
                rewardClaimed = true;
                lastEvent = "Recovered Ember Afterglow";
                audioSource.PlayOneShot(rewardTone, 0.42f);
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
            }
            else if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                currentPhase = (EchoPhase)(((int)currentPhase + 3) % 4);
                ApplyPhaseMaterial();
                lastEvent = $"{currentPhase} phase";
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

        private void Attack()
        {
            attackCooldown = AttackCooldown;
            audioSource.PlayOneShot(attackTone, 0.28f);

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
                audioSource.PlayOneShot(comboTone, 0.38f);
            }
            else
            {
                lastEvent = $"{currentPhase} hit";
            }

            if (enemyHealth <= 0f)
            {
                lastEvent = combo ? "Steam Burst surged altar 35%" : "Hollow down";
            }
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

        private void UpdatePresentation()
        {
            var enemyAlive = enemyHealth > 0f;
            enemy.gameObject.SetActive(enemyAlive);
            if (enemyAlive)
            {
                var scale = Mathf.Lerp(0.45f, 1f, enemyHealth / 70f);
                enemy.localScale = new Vector3(scale, 1f, scale);
                enemy.GetComponentInChildren<Renderer>().sharedMaterial = enemyMaterial;
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

        private static AudioClip CreateTone(string clipName, float frequency, float duration)
        {
            const int sampleRate = 44100;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = Mathf.Clamp01(1f - t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.18f;
            }
            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
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
            GUI.Label(new Rect(24, 48, 520, 32), $"Phase: {currentPhase}   Enemy: {Mathf.RoundToInt(enemyHealth)}   Altar: {Mathf.RoundToInt(altarHeat)}%   Gate: {(gateOpen ? "Open" : "Closed")}", style);
            GUI.Label(new Rect(24, 78, 720, 32), $"Event: {lastEvent}", style);
            GUI.Label(new Rect(24, Screen.height - 42, 900, 32), "Move WASD/Arrows | Attack J/Click | Dodge Space | Phase [ ] | Hold K at altar | Claim E/Right click", style);
        }
    }
}
