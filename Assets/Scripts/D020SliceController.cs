using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020SliceController : MonoBehaviour
    {
        [Header("Scene")]
        public Transform player;
        public Transform[] enemies;
        public GameObject rewardReadyRead;
        public Transform rewardClaimPoint;
        public Camera fixedCamera;

        [Header("Input")]
        public KeyCode attackKey = KeyCode.Space;
        public KeyCode dodgeKey = KeyCode.LeftShift;
        public KeyCode interactKey = KeyCode.E;
        public KeyCode retryKey = KeyCode.R;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip attackClip;
        public AudioClip hitClip;
        public AudioClip dodgeClip;
        public AudioClip rewardClaimClip;
        public AudioClip rewardReadyClip;

        private const float MoveSpeed = 5.2f;
        private const float DodgeSpeed = 10.2f;
        private const float DodgeDuration = 0.18f;
        private const float DodgeCooldown = 0.5f;
        private const float AttackRange = 1.85f;
        private const float AttackCooldown = 0.28f;
        private const float EnemySenseRange = 7.5f;
        private const float EnemySpeed = 1.65f;
        private const float EnemyAttackRange = 1.18f;
        private const float EnemyAttackWindup = 0.62f;
        private const float EnemyAttackCooldown = 1.15f;
        private const float PlayerMaxHealth = 100f;
        private const float MeleeEnemyDamage = 26f;
        private const float RangedEnemyDamage = 18f;
        private const float InvulnerableAfterHit = 0.65f;
        private const float RewardRange = 1.8f;
        private const string SaveKeyCleared = "fourfold.d020.slice.cleared";
        private const float MinX = -9.2f;
        private const float MaxX = 10.6f;
        private const float MinZ = -6.2f;
        private const float MaxZ = 7.0f;

        private float attackTimer;
        private float attackReadTimer;
        private float dodgeTimer;
        private float dodgeCooldownTimer;
        private Vector3 facing = new Vector3(1f, 0f, 1f).normalized;
        private Vector3 dodgeDirection = Vector3.right;
        private float[] enemyHealth;
        private float[] enemyAttackTimer;
        private float[] enemyWindupTimer;
        private GameObject[] enemyAttackReads;
        private Vector3 initialPlayerPosition;
        private Quaternion initialPlayerRotation;
        private Vector3[] initialEnemyPositions;
        private Quaternion[] initialEnemyRotations;
        private Vector3[] initialEnemyScales;
        private float playerHealth = PlayerMaxHealth;
        private float playerInvulnerableTimer;
        private bool rewardClaimed;
        private bool runFailed;
        private bool runCleared;
        private bool previousClearLoaded;
        private GameObject attackRead;
        private GameObject rewardClaimRead;
        private Material attackMaterial;
        private Material enemyAttackMaterial;
        private Material rewardMaterial;
        private bool rewardReadyCuePlayed;

        private void Awake()
        {
            if (player == null)
            {
                player = transform;
            }

            EnsureAudioSource();
            initialPlayerPosition = player.position;
            initialPlayerRotation = player.rotation;
            enemyHealth = new float[enemies == null ? 0 : enemies.Length];
            enemyAttackTimer = new float[enemyHealth.Length];
            enemyWindupTimer = new float[enemyHealth.Length];
            enemyAttackReads = new GameObject[enemyHealth.Length];
            initialEnemyPositions = new Vector3[enemyHealth.Length];
            initialEnemyRotations = new Quaternion[enemyHealth.Length];
            initialEnemyScales = new Vector3[enemyHealth.Length];
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                enemyHealth[i] = i == 0 ? 90f : 55f;
                enemyAttackTimer[i] = 0.28f + i * 0.25f;
                if (enemies != null && enemies[i] != null)
                {
                    initialEnemyPositions[i] = enemies[i].position;
                    initialEnemyRotations[i] = enemies[i].rotation;
                    initialEnemyScales[i] = enemies[i].localScale;
                }
            }

            attackMaterial = RuntimeMaterial("D020_Runtime_Attack_Read", new Color(1.0f, 0.52f, 0.12f), new Color(0.85f, 0.24f, 0.05f));
            enemyAttackMaterial = RuntimeMaterial("D020_Runtime_Enemy_Attack_Read", new Color(1.0f, 0.16f, 0.08f), new Color(0.80f, 0.03f, 0.01f));
            rewardMaterial = RuntimeMaterial("D020_Runtime_Reward_Read", new Color(0.25f, 0.72f, 1.0f), new Color(0.08f, 0.36f, 0.8f));
            attackRead = CreateDisc("D020 Runtime Attack Read", attackMaterial, 1.0f);
            attackRead.transform.SetParent(player, false);
            attackRead.transform.localPosition = new Vector3(0.82f, 0.06f, 0.34f);
            attackRead.SetActive(false);
            for (var i = 0; i < enemyAttackReads.Length; i++)
            {
                enemyAttackReads[i] = CreateDisc($"D020 Runtime Enemy Attack Read {i}", enemyAttackMaterial, 1.0f);
                enemyAttackReads[i].SetActive(false);
            }

            rewardClaimRead = CreateDisc("D020 Runtime Reward Claim Read", rewardMaterial, 1.35f);
            if (rewardClaimPoint != null)
            {
                rewardClaimRead.transform.position = rewardClaimPoint.position + new Vector3(0f, 0.08f, 0f);
            }
            rewardClaimRead.SetActive(false);
            SetRewardReady(false);
            previousClearLoaded = PlayerPrefs.GetInt(SaveKeyCleared, 0) == 1;
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            attackTimer = Mathf.Max(0f, attackTimer - dt);
            attackReadTimer = Mathf.Max(0f, attackReadTimer - dt);
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldownTimer = Mathf.Max(0f, dodgeCooldownTimer - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);

            if (Input.GetKeyDown(retryKey))
            {
                ResetRun();
                return;
            }

            if (runFailed || runCleared)
            {
                UpdateAttackRead();
                UpdateEnemyAttackReads();
                UpdateRewardState();
                return;
            }

            MovePlayer(dt);
            UpdateEnemies(dt);
            UpdateAttackRead();
            UpdateEnemyAttackReads();
            UpdateRewardState();

            if (Input.GetKeyDown(attackKey) || Input.GetMouseButtonDown(0))
            {
                TryAttack();
            }

            if ((Input.GetKeyDown(dodgeKey) || Input.GetKeyDown(KeyCode.RightShift)) && dodgeCooldownTimer <= 0f)
            {
                BeginDodge();
            }

            if (Input.GetKeyDown(interactKey))
            {
                TryClaimReward();
            }
        }

        private void MovePlayer(float dt)
        {
            var input = ReadMoveInput();
            if (input.sqrMagnitude > 0.001f)
            {
                facing = input.normalized;
            }

            var move = dodgeTimer > 0f ? dodgeDirection : input;
            var speed = dodgeTimer > 0f ? DodgeSpeed : MoveSpeed;
            player.position += move * speed * dt;
            player.position = new Vector3(
                Mathf.Clamp(player.position.x, MinX, MaxX),
                player.position.y,
                Mathf.Clamp(player.position.z, MinZ, MaxZ));

            if (facing.sqrMagnitude > 0.001f)
            {
                player.rotation = Quaternion.LookRotation(facing, Vector3.up);
            }
        }

        private void TryAttack()
        {
            if (attackTimer > 0f)
            {
                return;
            }

            attackTimer = AttackCooldown;
            attackReadTimer = 0.11f;
            PlayCue(attackClip, 0.72f);
            var hitAny = false;
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemyHealth[i] <= 0f)
                {
                    continue;
                }

                var toEnemy = enemy.position - player.position;
                toEnemy.y = 0f;
                if (toEnemy.magnitude > AttackRange)
                {
                    continue;
                }

                var directionScore = Vector3.Dot(facing.normalized, toEnemy.normalized);
                if (directionScore < 0.05f)
                {
                    continue;
                }

                hitAny = true;
                enemyHealth[i] -= i == 0 ? 34f : 42f;
                enemy.position += toEnemy.normalized * 0.34f;
                enemy.localScale = Vector3.one * (enemyHealth[i] <= 0f ? 0.86f : 1.08f);
                if (enemyHealth[i] <= 0f)
                {
                    enemy.gameObject.SetActive(false);
                    if (enemyAttackReads != null && i < enemyAttackReads.Length && enemyAttackReads[i] != null)
                    {
                        enemyAttackReads[i].SetActive(false);
                    }
                }
            }

            if (hitAny)
            {
                PlayCue(hitClip, 0.86f);
            }

            if (!hitAny && attackRead != null)
            {
                attackRead.transform.localScale = new Vector3(1.05f, 0.025f, 1.05f);
            }
        }

        private void BeginDodge()
        {
            dodgeDirection = facing.sqrMagnitude > 0.001f ? facing : Vector3.right;
            dodgeTimer = DodgeDuration;
            dodgeCooldownTimer = DodgeCooldown;
            PlayCue(dodgeClip, 0.68f);
        }

        private void UpdateEnemies(float dt)
        {
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemyHealth[i] <= 0f)
                {
                    continue;
                }

                var toPlayer = player.position - enemy.position;
                toPlayer.y = 0f;
                if (toPlayer.magnitude > EnemySenseRange || toPlayer.sqrMagnitude <= 0.01f)
                {
                    enemyWindupTimer[i] = 0f;
                    enemyAttackTimer[i] = Mathf.Max(0f, enemyAttackTimer[i] - dt);
                    continue;
                }

                var desired = toPlayer.normalized;
                enemy.rotation = Quaternion.LookRotation(-desired, Vector3.up);
                enemyAttackTimer[i] = Mathf.Max(0f, enemyAttackTimer[i] - dt);

                if (toPlayer.magnitude <= EnemyAttackRange && enemyAttackTimer[i] <= 0f)
                {
                    enemyWindupTimer[i] += dt;
                    if (enemyWindupTimer[i] >= EnemyAttackWindup)
                    {
                        ResolveEnemyAttack(i, enemy, toPlayer);
                        enemyWindupTimer[i] = 0f;
                        enemyAttackTimer[i] = EnemyAttackCooldown + i * 0.22f;
                    }
                    continue;
                }

                enemyWindupTimer[i] = 0f;
                var flank = new Vector3(-desired.z, 0f, desired.x) * Mathf.Sin(Time.time * (0.8f + i * 0.35f)) * 0.28f;
                enemy.position += (desired + flank).normalized * EnemySpeed * dt;
            }
        }

        private void ResolveEnemyAttack(int index, Transform enemy, Vector3 toPlayer)
        {
            if (playerInvulnerableTimer > 0f || dodgeTimer > 0f || runFailed || runCleared)
            {
                return;
            }

            var distance = toPlayer.magnitude;
            if (distance > EnemyAttackRange + 0.28f)
            {
                return;
            }

            playerHealth = Mathf.Max(0f, playerHealth - (index == 0 ? MeleeEnemyDamage : RangedEnemyDamage));
            playerInvulnerableTimer = InvulnerableAfterHit;
            var knockback = (player.position - enemy.position);
            knockback.y = 0f;
            if (knockback.sqrMagnitude > 0.01f)
            {
                player.position += knockback.normalized * 0.42f;
            }

            PlayCue(hitClip, 0.62f);
            if (playerHealth <= 0f)
            {
                runFailed = true;
                SetRewardReady(false);
            }
        }

        private void UpdateAttackRead()
        {
            if (attackRead == null)
            {
                return;
            }

            var active = attackReadTimer > 0f;
            attackRead.SetActive(active);
            if (!active)
            {
                return;
            }

            attackRead.transform.localPosition = facing.normalized * 0.86f + new Vector3(0f, 0.06f, 0f);
            var pulse = 1.16f + Mathf.Sin(Time.time * 36f) * 0.08f;
            attackRead.transform.localScale = new Vector3(pulse, 0.025f, pulse);
        }

        private void UpdateEnemyAttackReads()
        {
            if (enemyAttackReads == null)
            {
                return;
            }

            for (var i = 0; i < enemyAttackReads.Length; i++)
            {
                var read = enemyAttackReads[i];
                if (read == null)
                {
                    continue;
                }

                var enemy = enemies != null && i < enemies.Length ? enemies[i] : null;
                var active = enemy != null && enemy.gameObject.activeSelf && enemyHealth[i] > 0f && enemyWindupTimer[i] > 0f;
                read.SetActive(active);
                if (!active)
                {
                    continue;
                }

                var progress = Mathf.Clamp01(enemyWindupTimer[i] / EnemyAttackWindup);
                read.transform.position = enemy.position + new Vector3(0f, 0.07f, 0f);
                var radius = Mathf.Lerp(0.72f, 1.28f, progress);
                read.transform.localScale = new Vector3(radius, 0.025f, radius);
            }
        }

        private void UpdateRewardState()
        {
            var ready = AllEnemiesDefeated();
            if (ready && !rewardClaimed && !rewardReadyCuePlayed)
            {
                rewardReadyCuePlayed = true;
                PlayCue(rewardReadyClip, 0.78f);
            }

            SetRewardReady(ready);
            if (rewardClaimRead != null)
            {
                rewardClaimRead.SetActive(ready && !rewardClaimed);
                if (rewardClaimPoint != null)
                {
                    rewardClaimRead.transform.position = rewardClaimPoint.position + new Vector3(0f, 0.08f, 0f);
                    var pulse = 1.0f + Mathf.Sin(Time.time * 4.5f) * 0.08f;
                    rewardClaimRead.transform.localScale = new Vector3(pulse, 0.025f, pulse);
                }
            }
        }

        private void TryClaimReward()
        {
            if (rewardClaimed || !AllEnemiesDefeated() || rewardClaimPoint == null)
            {
                return;
            }

            if (Vector3.Distance(player.position, rewardClaimPoint.position) > RewardRange)
            {
                return;
            }

            rewardClaimed = true;
            runCleared = true;
            PlayerPrefs.SetInt(SaveKeyCleared, 1);
            PlayerPrefs.Save();
            PlayCue(rewardClaimClip, 0.92f);
            if (rewardReadyRead != null)
            {
                rewardReadyRead.transform.localScale = Vector3.one * 1.25f;
            }
            if (rewardClaimRead != null)
            {
                rewardClaimRead.SetActive(false);
            }
        }

        private void ResetRun()
        {
            playerHealth = PlayerMaxHealth;
            playerInvulnerableTimer = 0f;
            runFailed = false;
            runCleared = false;
            rewardClaimed = false;
            rewardReadyCuePlayed = false;
            attackTimer = 0f;
            attackReadTimer = 0f;
            dodgeTimer = 0f;
            dodgeCooldownTimer = 0f;

            if (player != null)
            {
                player.position = initialPlayerPosition;
                player.rotation = initialPlayerRotation;
                player.localScale = Vector3.one;
            }

            for (var i = 0; i < enemyHealth.Length; i++)
            {
                enemyHealth[i] = i == 0 ? 90f : 55f;
                enemyAttackTimer[i] = 0.28f + i * 0.25f;
                enemyWindupTimer[i] = 0f;
                if (enemies != null && enemies[i] != null)
                {
                    enemies[i].gameObject.SetActive(true);
                    enemies[i].position = initialEnemyPositions[i];
                    enemies[i].rotation = initialEnemyRotations[i];
                    enemies[i].localScale = initialEnemyScales[i] == Vector3.zero ? Vector3.one : initialEnemyScales[i];
                }

                if (enemyAttackReads != null && enemyAttackReads[i] != null)
                {
                    enemyAttackReads[i].SetActive(false);
                }
            }

            if (attackRead != null)
            {
                attackRead.SetActive(false);
            }
            if (rewardClaimRead != null)
            {
                rewardClaimRead.SetActive(false);
            }
            SetRewardReady(false);
        }

        private bool AllEnemiesDefeated()
        {
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                if (enemyHealth[i] > 0f)
                {
                    return false;
                }
            }

            return enemyHealth.Length > 0;
        }

        private void SetRewardReady(bool ready)
        {
            if (rewardReadyRead != null)
            {
                rewardReadyRead.SetActive(ready);
            }
        }

        private static Vector3 ReadMoveInput()
        {
            var x = 0f;
            var z = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                x -= 1f;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                x += 1f;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                z += 1f;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                z -= 1f;
            }

            var input = new Vector3(x, 0f, z);
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = 0.85f;
        }

        private void PlayCue(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            if (audioSource == null)
            {
                EnsureAudioSource();
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
        }

        private static GameObject CreateDisc(string name, Material material, float radius)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.localScale = new Vector3(radius, 0.025f, radius);
            var renderer = disc.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var collider = disc.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            return disc;
        }

        private void OnGUI()
        {
            var width = Mathf.Min(520f, Screen.width - 32f);
            var rect = new Rect(16f, 16f, width, 146f);
            GUI.Box(rect, GUIContent.none);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 42, 16, 24),
                normal = { textColor = Color.white },
                wordWrap = true
            };

            var objective = runCleared
                ? "CLEAR: reward secured. Press R to replay."
                : runFailed
                    ? "FAILED: press R to retry."
                    : AllEnemiesDefeated()
                        ? "Claim the relic chest with E."
                        : "Defeat the enemies, use the tool node, then claim the relic.";

            GUI.Label(new Rect(30f, 26f, width - 28f, 34f), $"HP {Mathf.CeilToInt(playerHealth)} / {Mathf.CeilToInt(PlayerMaxHealth)}", style);
            GUI.Label(new Rect(30f, 58f, width - 28f, 58f), objective, style);
            GUI.Label(new Rect(30f, 112f, width - 28f, 34f), "Move WASD/Arrows  Attack Space/Click  Dodge Shift  Interact E  Retry R", style);

            if (previousClearLoaded && !runCleared)
            {
                GUI.Label(new Rect(16f, Screen.height - 42f, width, 28f), "Local progress: this slice was cleared before.", style);
            }
        }

        private static Material RuntimeMaterial(string name, Color color, Color emission)
        {
            var material = new Material(Shader.Find("Standard"))
            {
                name = name,
                color = color
            };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            return material;
        }
    }
}
