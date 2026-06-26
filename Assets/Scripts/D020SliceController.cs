using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class D020SliceController : MonoBehaviour
    {
        [Header("Scene")]
        public Transform player;
        public Transform[] enemies;
        public GameObject rewardReadyRead;
        public Transform rewardClaimPoint;
        public ExplorationTool explorationTool;
        public ExplorationNode requiredToolNode;
        public GameObject shortcutLockedRead;
        public GameObject secondRewardReadyRead;
        public Transform secondRewardClaimPoint;
        public ExplorationNode secondToolNode;
        public GameObject secondRouteLockedRead;
        public GameObject returnReadyRead;
        public Transform returnGatePoint;
        public Camera fixedCamera;

        [Header("Input")]
        public KeyCode attackKey = KeyCode.Space;
        public KeyCode dodgeKey = KeyCode.LeftShift;
        public KeyCode interactKey = KeyCode.E;
        public KeyCode retryKey = KeyCode.R;
        public KeyCode pauseKey = KeyCode.Escape;
        public KeyCode returnToTitleKey = KeyCode.Backspace;
        public KeyCode gamepadAttackKey = KeyCode.JoystickButton0;
        public KeyCode gamepadDodgeKey = KeyCode.JoystickButton1;
        public KeyCode gamepadInteractKey = KeyCode.JoystickButton3;
        public KeyCode gamepadRetryKey = KeyCode.JoystickButton7;
        public KeyCode gamepadPauseKey = KeyCode.JoystickButton9;
        public KeyCode gamepadReturnToTitleKey = KeyCode.JoystickButton6;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip attackClip;
        public AudioClip hitClip;
        public AudioClip dodgeClip;
        public AudioClip rewardClaimClip;
        public AudioClip rewardReadyClip;
        public AudioSource musicSource;
        public AudioClip explorationMusicClip;
        public AudioClip bossMusicClip;

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
        private const float RangedEnemySpeed = 1.30f;
        private const float RangedPreferredDistance = 4.25f;
        private const float RangedRetreatDistance = 2.55f;
        private const float RangedAttackRange = 5.75f;
        private const float RangedAttackWindup = 0.82f;
        private const float RangedAttackCooldown = 1.55f;
        private const float EliteEnemyHealth = 135f;
        private const float EliteEnemySpeed = 1.26f;
        private const float EliteAttackRange = 1.58f;
        private const float EliteAttackWindup = 0.76f;
        private const float EliteAttackCooldown = 1.32f;
        private const float BossAttackRange = 2.05f;
        private const float BossAttackWindup = 0.92f;
        private const float BossAttackCooldown = 1.65f;
        private const float BossSweepRange = 4.65f;
        private const float BossSweepWindup = 1.08f;
        private const float BossSweepCooldown = 1.95f;
        private const float BossSpeed = 1.12f;
        private const float BossEnragedSpeed = 1.34f;
        private const float BossEnrageHealth = 110f;
        private const int AttackModeCircle = 0;
        private const int AttackModeLine = 1;
        private const float PlayerMaxHealth = 100f;
        private const float MeleeEnemyDamage = 26f;
        private const float RangedEnemyDamage = 18f;
        private const float EliteEnemyDamage = 30f;
        private const float BossEnemyDamage = 34f;
        private const float InvulnerableAfterHit = 0.65f;
        private const float RewardRange = 1.8f;
        private const string SaveKeyCleared = "fourfold.d020.slice.cleared";
        private const string SaveKeyShortcutOpened = "fourfold.d020.slice.shortcut_opened";
        private const string SaveKeyRewardClaimed = "fourfold.d020.slice.reward_claimed";
        private const string SaveKeySecondNodeOpened = "fourfold.d020.slice.second_node_opened";
        private const string SaveKeySecondRewardClaimed = "fourfold.d020.slice.second_reward_claimed";
        private const string SaveKeyReturnedToHub = "fourfold.d020.slice.returned_to_hub";
        private const string SaveKeyClearCount = "fourfold.d020.slice.clear_count";
        private const string SaveKeyFailureCount = "fourfold.d020.slice.failure_count";
        private const string SaveKeyBestClearTime = "fourfold.d020.slice.best_clear_time";
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
        private Vector3[] enemyAttackAimDirections;
        private int[] enemyAttackModes;
        private bool[] bossEnraged;
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
        private bool previousShortcutLoaded;
        private bool previousRewardLoaded;
        private bool previousSecondNodeLoaded;
        private bool previousSecondRewardLoaded;
        private bool previousReturnedToHubLoaded;
        private int clearCount;
        private int failureCount;
        private float runTimerSeconds;
        private float bestClearTimeSeconds;
        private float lastReturnTimeSeconds;
        private int lastLostRelicsOnFailure;
        private float bossDefeatTimer;
        private bool firstRewardClaimedThisRun;
        private bool secondRewardClaimedThisRun;
        private bool returnedToHubThisRun;
        private bool returnRegisteredThisRun;
        private bool failureRegisteredThisRun;
        private bool bestClearTimeImproved;
        private bool bossDefeatedThisRun;
        private FourfoldProgressData progressData;
        private GameObject attackRead;
        private GameObject rewardClaimRead;
        private GameObject secondRewardClaimRead;
        private GameObject returnGateClaimRead;
        private Material attackMaterial;
        private Material enemyAttackMaterial;
        private Material rewardMaterial;
        private bool rewardReadyCuePlayed;
        private AudioClip currentMusicClip;
        private bool paused;

        private void Awake()
        {
            if (player == null)
            {
                player = transform;
            }

            EnsureAudioSource();
            EnsureExplorationReferences();
            initialPlayerPosition = player.position;
            initialPlayerRotation = player.rotation;
            enemyHealth = new float[enemies == null ? 0 : enemies.Length];
            enemyAttackTimer = new float[enemyHealth.Length];
            enemyWindupTimer = new float[enemyHealth.Length];
            enemyAttackAimDirections = new Vector3[enemyHealth.Length];
            enemyAttackModes = new int[enemyHealth.Length];
            bossEnraged = new bool[enemyHealth.Length];
            enemyAttackReads = new GameObject[enemyHealth.Length];
            initialEnemyPositions = new Vector3[enemyHealth.Length];
            initialEnemyRotations = new Quaternion[enemyHealth.Length];
            initialEnemyScales = new Vector3[enemyHealth.Length];
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                enemyHealth[i] = InitialEnemyHealth(i);
                enemyAttackTimer[i] = InitialEnemyAttackDelay(i);
                enemyAttackAimDirections[i] = Vector3.forward;
                enemyAttackModes[i] = IsBossEnemy(i) ? AttackModeLine : AttackModeCircle;
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
                enemyAttackReads[i] = IsRangedEnemy(i)
                    ? CreateBeamRead($"D020 Runtime Enemy Aim Read {i}", enemyAttackMaterial)
                    : CreateDisc($"D020 Runtime Enemy Attack Read {i}", enemyAttackMaterial, 1.0f);
                enemyAttackReads[i].SetActive(false);
            }

            rewardClaimRead = CreateDisc("D020 Runtime Reward Claim Read", rewardMaterial, 1.35f);
            if (rewardClaimPoint != null)
            {
                rewardClaimRead.transform.position = rewardClaimPoint.position + new Vector3(0f, 0.08f, 0f);
            }
            rewardClaimRead.SetActive(false);
            secondRewardClaimRead = CreateDisc("D020 Runtime Second Reward Claim Read", rewardMaterial, 1.2f);
            if (secondRewardClaimPoint != null)
            {
                secondRewardClaimRead.transform.position = secondRewardClaimPoint.position + new Vector3(0f, 0.08f, 0f);
            }
            secondRewardClaimRead.SetActive(false);
            returnGateClaimRead = CreateDisc("D020 Runtime Return Claim Read", rewardMaterial, 1.55f);
            if (returnGatePoint != null)
            {
                returnGateClaimRead.transform.position = returnGatePoint.position + new Vector3(0f, 0.08f, 0f);
            }
            returnGateClaimRead.SetActive(false);
            SetRewardReady(false);
        }

        private void Start()
        {
            LoadProgress();
            EnsureExplorationReferences();
            if (previousShortcutLoaded && requiredToolNode != null)
            {
                requiredToolNode.SetSolved(true);
            }
            if (previousSecondNodeLoaded && secondToolNode != null)
            {
                secondToolNode.SetSolved(true);
            }
        }

        private void Update()
        {
            UpdateToolInputLock();
            if (Pressed(retryKey, gamepadRetryKey))
            {
                ResetRun();
                return;
            }

            if (Pressed(pauseKey, gamepadPauseKey))
            {
                SetPaused(!paused);
                return;
            }

            if (paused)
            {
                if (Pressed(returnToTitleKey, gamepadReturnToTitleKey))
                {
                    TryReturnToTitle();
                }

                return;
            }

            var dt = Time.deltaTime;
            attackTimer = Mathf.Max(0f, attackTimer - dt);
            attackReadTimer = Mathf.Max(0f, attackReadTimer - dt);
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldownTimer = Mathf.Max(0f, dodgeCooldownTimer - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);
            bossDefeatTimer = Mathf.Max(0f, bossDefeatTimer - dt);
            if (!runFailed && !returnedToHubThisRun)
            {
                runTimerSeconds += dt;
            }

            UpdateMusicState();

            if (runFailed)
            {
                UpdateAttackRead();
                UpdateEnemyAttackReads();
                UpdateRewardState();
                UpdateReturnState();
                UpdateTraversalReads();
                return;
            }

            if (runCleared)
            {
                MovePlayer(dt);
                UpdateAttackRead();
                UpdateEnemyAttackReads();
                UpdateRewardState();
                UpdateReturnState();
                UpdateTraversalReads();
                if (Pressed(interactKey, gamepadInteractKey))
                {
                    TryReturnToHub();
                }

                return;
            }

            MovePlayer(dt);
            UpdateEnemies(dt);
            UpdateProgressFlags();
            UpdateAttackRead();
            UpdateEnemyAttackReads();
            UpdateRewardState();
            UpdateReturnState();
            UpdateTraversalReads();

            if (Pressed(attackKey, gamepadAttackKey) || Input.GetMouseButtonDown(0))
            {
                TryAttack();
            }

            if (Pressed(dodgeKey, KeyCode.RightShift, gamepadDodgeKey) && dodgeCooldownTimer <= 0f)
            {
                BeginDodge();
            }

            if (Pressed(interactKey, gamepadInteractKey))
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
            var proposed = player.position + move * speed * dt;
            proposed = new Vector3(
                Mathf.Clamp(proposed.x, MinX, MaxX),
                proposed.y,
                Mathf.Clamp(proposed.z, MinZ, MaxZ));
            player.position = ResolveBlockedMove(player.position, proposed);

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
                enemyHealth[i] -= CurrentAttackDamage(i);
                enemy.position = ResolveBlockedMove(enemy.position, enemy.position + toEnemy.normalized * (IsBossEnemy(i) ? 0.16f : 0.34f));
                enemy.localScale = EnemyHitScale(i, enemyHealth[i] <= 0f);
                TryTriggerBossEnrage(i, enemy);
                if (enemyHealth[i] <= 0f)
                {
                    if (IsBossEnemy(i))
                    {
                        RegisterBossDefeat();
                    }

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
                var distance = toPlayer.magnitude;
                if (distance > EnemySenseRange || toPlayer.sqrMagnitude <= 0.01f)
                {
                    enemyWindupTimer[i] = 0f;
                    enemyAttackTimer[i] = Mathf.Max(0f, enemyAttackTimer[i] - dt);
                    continue;
                }

                var desired = toPlayer.normalized;
                enemy.rotation = Quaternion.LookRotation(-desired, Vector3.up);
                enemyAttackTimer[i] = Mathf.Max(0f, enemyAttackTimer[i] - dt);
                var attackRange = EnemyAttackRangeFor(i);
                var attackWindup = EnemyAttackWindupFor(i);
                var attackCooldown = EnemyAttackCooldownFor(i);

                if (distance <= attackRange && enemyAttackTimer[i] <= 0f)
                {
                    if (enemyWindupTimer[i] <= 0f)
                    {
                        enemyAttackAimDirections[i] = desired;
                    }
                    enemyWindupTimer[i] += dt;
                    if (enemyWindupTimer[i] >= attackWindup)
                    {
                        ResolveEnemyAttack(i, enemy, toPlayer);
                        AdvanceEnemyAttackMode(i);
                        enemyWindupTimer[i] = 0f;
                        enemyAttackTimer[i] = attackCooldown;
                    }
                    continue;
                }

                enemyWindupTimer[i] = 0f;
                if (IsRangedEnemy(i))
                {
                    var strafe = new Vector3(-desired.z, 0f, desired.x) * Mathf.Sin(Time.time * 1.35f) * 0.62f;
                    var moveDirection = distance < RangedRetreatDistance
                        ? -desired + strafe * 0.45f
                        : distance > RangedPreferredDistance
                            ? desired + strafe * 0.25f
                            : strafe;
                    if (moveDirection.sqrMagnitude > 0.001f)
                    {
                        MoveEnemy(enemy, moveDirection, EnemySpeedFor(i), dt);
                    }
                    continue;
                }

                var flank = new Vector3(-desired.z, 0f, desired.x) * Mathf.Sin(Time.time * (0.8f + i * 0.35f)) * 0.28f;
                MoveEnemy(enemy, desired + flank, EnemySpeedFor(i), dt);
            }
        }

        private void ResolveEnemyAttack(int index, Transform enemy, Vector3 toPlayer)
        {
            if (playerInvulnerableTimer > 0f || dodgeTimer > 0f || runFailed || runCleared)
            {
                return;
            }

            if (!EnemyAttackHitsPlayer(index, toPlayer))
            {
                return;
            }

            playerHealth = Mathf.Max(0f, playerHealth - EnemyDamageFor(index));
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
                RegisterRunFailure();
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
            var relicBoost = LumenEdgeActive() ? 0.22f : 0f;
            var pulse = 1.16f + relicBoost + Mathf.Sin(Time.time * 36f) * 0.08f;
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

                var progress = Mathf.Clamp01(enemyWindupTimer[i] / EnemyAttackWindupFor(i));
                if (IsRangedEnemy(i))
                {
                    var aim = EnemyAimDirection(i, enemy);
                    var distance = EnemyAttackRangeFor(i);
                    read.transform.position = enemy.position + aim * (distance * 0.5f) + new Vector3(0f, 0.10f, 0f);
                    read.transform.rotation = Quaternion.LookRotation(aim, Vector3.up);
                    var width = Mathf.Lerp(0.12f, 0.26f, progress);
                    read.transform.localScale = new Vector3(width, 0.035f, Mathf.Max(0.72f, distance));
                    continue;
                }

                if (IsBossEnemy(i) && BossUsesLineAttack(i))
                {
                    var aim = EnemyAimDirection(i, enemy);
                    var distance = EnemyAttackRangeFor(i);
                    read.transform.position = enemy.position + aim * (distance * 0.48f) + new Vector3(0f, 0.09f, 0f);
                    read.transform.rotation = Quaternion.LookRotation(aim, Vector3.up);
                    var width = Mathf.Lerp(0.78f, 1.14f, progress);
                    read.transform.localScale = new Vector3(width, 0.028f, distance);
                    continue;
                }

                read.transform.position = enemy.position + new Vector3(0f, 0.07f, 0f);
                var minRadius = IsBossEnemy(i) ? 1.25f : IsEliteEnemy(i) ? 0.94f : 0.72f;
                var maxRadius = IsBossEnemy(i) ? 2.35f : IsEliteEnemy(i) ? 1.72f : 1.28f;
                var radius = Mathf.Lerp(minRadius, maxRadius, progress);
                read.transform.localScale = new Vector3(radius, 0.025f, radius);
            }
        }

        private void UpdateRewardState()
        {
            var ready = RewardReady();
            if (ready && !rewardClaimed && !rewardReadyCuePlayed)
            {
                rewardReadyCuePlayed = true;
                PlayCue(rewardReadyClip, 0.78f);
            }

            SetRewardReady(ready);
            if (rewardClaimRead != null)
            {
                rewardClaimRead.SetActive(ready);
                if (rewardClaimPoint != null)
                {
                    rewardClaimRead.transform.position = rewardClaimPoint.position + new Vector3(0f, 0.08f, 0f);
                    var pulse = 1.0f + Mathf.Sin(Time.time * 4.5f) * 0.08f;
                    rewardClaimRead.transform.localScale = new Vector3(pulse, 0.025f, pulse);
                }
            }

            var secondReady = SecondRewardReady();
            if (secondRewardClaimRead != null)
            {
                secondRewardClaimRead.SetActive(secondReady);
                if (secondRewardClaimPoint != null)
                {
                    secondRewardClaimRead.transform.position = secondRewardClaimPoint.position + new Vector3(0f, 0.08f, 0f);
                    var pulse = 0.92f + Mathf.Sin(Time.time * 5.2f) * 0.07f;
                    secondRewardClaimRead.transform.localScale = new Vector3(pulse, 0.025f, pulse);
                }
            }

            if (secondRewardReadyRead != null)
            {
                secondRewardReadyRead.SetActive(secondReady);
            }
        }

        private void TryClaimReward()
        {
            if (TryClaimFirstReward())
            {
                return;
            }

            TryClaimSecondReward();
        }

        private bool TryClaimFirstReward()
        {
            if (!RewardReady() || rewardClaimPoint == null)
            {
                return false;
            }

            if (Vector3.Distance(player.position, rewardClaimPoint.position) > RewardRange)
            {
                return false;
            }

            rewardClaimed = true;
            firstRewardClaimedThisRun = true;
            previousShortcutLoaded = ToolGateSolved();
            runCleared = secondToolNode == null || secondRewardClaimPoint == null;
            UpdateToolInputLock();
            PlayCue(rewardClaimClip, 0.92f);
            if (rewardReadyRead != null)
            {
                rewardReadyRead.transform.localScale = Vector3.one * 1.25f;
            }
            if (rewardClaimRead != null)
            {
                rewardClaimRead.SetActive(false);
            }
            return true;
        }

        private bool TryClaimSecondReward()
        {
            if (!SecondRewardReady() || secondRewardClaimPoint == null)
            {
                return false;
            }

            if (Vector3.Distance(player.position, secondRewardClaimPoint.position) > RewardRange)
            {
                return false;
            }

            secondRewardClaimedThisRun = true;
            rewardClaimed = true;
            runCleared = true;
            previousReturnedToHubLoaded = false;
            UpdateToolInputLock();
            PlayCue(rewardClaimClip, 0.86f);
            if (secondRewardReadyRead != null)
            {
                secondRewardReadyRead.SetActive(false);
            }
            if (secondRewardClaimRead != null)
            {
                secondRewardClaimRead.SetActive(false);
            }
            return true;
        }

        private void UpdateReturnState()
        {
            var ready = runCleared && !returnedToHubThisRun && returnGatePoint != null;
            if (returnReadyRead != null)
            {
                returnReadyRead.SetActive(ready);
            }

            if (returnGateClaimRead == null)
            {
                return;
            }

            returnGateClaimRead.SetActive(ready);
            if (!ready)
            {
                return;
            }

            returnGateClaimRead.transform.position = returnGatePoint.position + new Vector3(0f, 0.08f, 0f);
            var pulse = 1.08f + Mathf.Sin(Time.time * 4.0f) * 0.09f;
            returnGateClaimRead.transform.localScale = new Vector3(pulse, 0.025f, pulse);
        }

        public bool TryReturnToTitle()
        {
            SetPaused(false);
            PersistProgress();
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(FourfoldGameIds.UnitySceneTitle);
            }

            return true;
        }

        private bool TryReturnToHub()
        {
            if (!runCleared || returnedToHubThisRun || returnGatePoint == null)
            {
                return false;
            }

            if (Vector3.Distance(player.position, returnGatePoint.position) > RewardRange)
            {
                return false;
            }

            returnedToHubThisRun = true;
            previousReturnedToHubLoaded = true;
            previousClearLoaded = true;
            previousRewardLoaded = previousRewardLoaded || firstRewardClaimedThisRun;
            previousSecondRewardLoaded = previousSecondRewardLoaded || secondRewardClaimedThisRun;
            RegisterReturnedClearTime();

            if (!returnRegisteredThisRun)
            {
                clearCount += 1;
                returnRegisteredThisRun = true;
            }

            if (player != null)
            {
                player.position = initialPlayerPosition;
                player.rotation = initialPlayerRotation;
            }

            PersistProgress();
            if (returnReadyRead != null)
            {
                returnReadyRead.SetActive(false);
            }
            if (returnGateClaimRead != null)
            {
                returnGateClaimRead.SetActive(false);
            }
            PlayCue(rewardReadyClip, 0.58f);
            UpdateToolInputLock();
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(FourfoldGameIds.UnitySceneHubCrossroads);
            }

            return true;
        }

        private void UpdateTraversalReads()
        {
            if (shortcutLockedRead != null)
            {
                shortcutLockedRead.SetActive(!ToolGateSolved());
            }

            if (secondRouteLockedRead != null)
            {
                secondRouteLockedRead.SetActive((previousRewardLoaded || firstRewardClaimedThisRun) && !SecondToolGateSolved());
            }
        }

        private void ResetRun()
        {
            SetPaused(false);
            playerHealth = PlayerMaxHealth;
            playerInvulnerableTimer = 0f;
            runFailed = false;
            runCleared = false;
            rewardClaimed = false;
            rewardReadyCuePlayed = false;
            firstRewardClaimedThisRun = false;
            secondRewardClaimedThisRun = false;
            returnedToHubThisRun = false;
            returnRegisteredThisRun = false;
            failureRegisteredThisRun = false;
            runTimerSeconds = 0f;
            lastReturnTimeSeconds = 0f;
            lastLostRelicsOnFailure = 0;
            bossDefeatTimer = 0f;
            bestClearTimeImproved = false;
            bossDefeatedThisRun = false;
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
                enemyHealth[i] = InitialEnemyHealth(i);
                enemyAttackTimer[i] = InitialEnemyAttackDelay(i);
                enemyWindupTimer[i] = 0f;
                enemyAttackAimDirections[i] = Vector3.forward;
                enemyAttackModes[i] = IsBossEnemy(i) ? AttackModeLine : AttackModeCircle;
                bossEnraged[i] = false;
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
            if (secondRewardClaimRead != null)
            {
                secondRewardClaimRead.SetActive(false);
            }
            if (returnGateClaimRead != null)
            {
                returnGateClaimRead.SetActive(false);
            }
            if (requiredToolNode != null)
            {
                requiredToolNode.SetSolved(previousShortcutLoaded);
            }
            if (secondToolNode != null)
            {
                secondToolNode.SetSolved(previousSecondNodeLoaded);
            }
            SetRewardReady(false);
            UpdateToolInputLock();
        }

        private void RegisterReturnedClearTime()
        {
            lastReturnTimeSeconds = Mathf.Max(0.01f, runTimerSeconds);
            if (bestClearTimeSeconds <= 0f || lastReturnTimeSeconds < bestClearTimeSeconds)
            {
                bestClearTimeSeconds = lastReturnTimeSeconds;
                bestClearTimeImproved = true;
            }
            else
            {
                bestClearTimeImproved = false;
            }
        }

        private void RegisterBossDefeat()
        {
            if (bossDefeatedThisRun)
            {
                return;
            }

            bossDefeatedThisRun = true;
            bossDefeatTimer = 2.8f;
            PlayCue(rewardReadyClip, 0.82f);
        }

        private void RegisterRunFailure()
        {
            if (failureRegisteredThisRun)
            {
                return;
            }

            failureRegisteredThisRun = true;
            runFailed = true;
            rewardClaimed = false;
            failureCount += 1;
            lastLostRelicsOnFailure = ClaimedRelicCountThisRun();
            firstRewardClaimedThisRun = false;
            secondRewardClaimedThisRun = false;
            returnedToHubThisRun = false;
            returnRegisteredThisRun = false;
            SetRewardReady(false);
            if (rewardClaimRead != null)
            {
                rewardClaimRead.SetActive(false);
            }
            if (secondRewardClaimRead != null)
            {
                secondRewardClaimRead.SetActive(false);
            }
            if (returnGateClaimRead != null)
            {
                returnGateClaimRead.SetActive(false);
            }

            PersistProgress();
            UpdateToolInputLock();
        }

        private void SetPaused(bool value)
        {
            if (paused == value)
            {
                return;
            }

            paused = value;
            UpdateToolInputLock();
            if (musicSource == null)
            {
                return;
            }

            if (paused)
            {
                musicSource.Pause();
            }
            else if (currentMusicClip != null && musicSource.clip == currentMusicClip)
            {
                musicSource.UnPause();
            }
        }

        private void UpdateToolInputLock()
        {
            if (explorationTool != null)
            {
                explorationTool.inputEnabled = !paused && !runFailed && !runCleared;
            }
        }

        private void LoadProgress()
        {
            progressData = FourfoldProgressSave.Load();
            if (!FourfoldProgressSave.HasSaveFile())
            {
                MigrateLegacyPlayerPrefs(progressData);
            }

            progressData.hubUnlocked = true;
            progressData.regionD020Unlocked = true;
            progressData.lumenRodUnlocked = true;
            previousClearLoaded = progressData.d020Cleared;
            previousShortcutLoaded = progressData.d020ShortcutOpened;
            previousRewardLoaded = progressData.d020RewardClaimed && previousClearLoaded;
            previousSecondNodeLoaded = progressData.d020SecondNodeOpened;
            previousSecondRewardLoaded = progressData.d020SecondRewardClaimed && previousClearLoaded;
            previousReturnedToHubLoaded = progressData.d020ReturnedToHub && previousClearLoaded;
            clearCount = Mathf.Max(0, progressData.d020ClearCount);
            failureCount = Mathf.Max(0, progressData.d020FailureCount);
            bestClearTimeSeconds = Mathf.Max(0f, progressData.d020BestClearTimeSeconds);
        }

        private float CurrentAttackDamage(int enemyIndex)
        {
            var baseDamage = IsBossEnemy(enemyIndex) ? 30f : IsEliteEnemy(enemyIndex) ? 36f : enemyIndex == 0 ? 34f : 42f;
            return LumenEdgeActive() ? baseDamage + 12f : baseDamage;
        }

        private float InitialEnemyHealth(int index)
        {
            if (IsBossEnemy(index))
            {
                return 220f;
            }

            if (IsEliteEnemy(index))
            {
                return EliteEnemyHealth;
            }

            return IsRangedEnemy(index) ? 62f : 90f;
        }

        private float InitialEnemyAttackDelay(int index)
        {
            if (IsBossEnemy(index))
            {
                return 0.9f;
            }

            if (IsEliteEnemy(index))
            {
                return 0.75f;
            }

            return IsRangedEnemy(index) ? 0.65f : 0.28f + index * 0.25f;
        }

        private float EnemyAttackRangeFor(int index)
        {
            if (IsBossEnemy(index))
            {
                return BossUsesLineAttack(index) ? BossSweepRange : BossAttackRange;
            }

            if (IsEliteEnemy(index))
            {
                return EliteAttackRange;
            }

            return IsRangedEnemy(index) ? RangedAttackRange : EnemyAttackRange;
        }

        private float EnemyAttackWindupFor(int index)
        {
            if (IsBossEnemy(index))
            {
                var windup = BossUsesLineAttack(index) ? BossSweepWindup : BossAttackWindup;
                return BossEnraged(index) ? windup * 0.82f : windup;
            }

            if (IsEliteEnemy(index))
            {
                return EliteAttackWindup;
            }

            return IsRangedEnemy(index) ? RangedAttackWindup : EnemyAttackWindup;
        }

        private float EnemyAttackCooldownFor(int index)
        {
            if (IsBossEnemy(index))
            {
                var cooldown = BossUsesLineAttack(index) ? BossSweepCooldown : BossAttackCooldown;
                return BossEnraged(index) ? cooldown * 0.72f : cooldown;
            }

            if (IsEliteEnemy(index))
            {
                return EliteAttackCooldown;
            }

            return IsRangedEnemy(index) ? RangedAttackCooldown : EnemyAttackCooldown + index * 0.22f;
        }

        private float EnemySpeedFor(int index)
        {
            if (IsBossEnemy(index))
            {
                return BossEnraged(index) ? BossEnragedSpeed : BossSpeed;
            }

            if (IsEliteEnemy(index))
            {
                return EliteEnemySpeed;
            }

            return IsRangedEnemy(index) ? RangedEnemySpeed : EnemySpeed;
        }

        private float EnemyDamageFor(int index)
        {
            if (IsBossEnemy(index))
            {
                return BossEnraged(index) ? BossEnemyDamage + 6f : BossEnemyDamage;
            }

            if (IsEliteEnemy(index))
            {
                return EliteEnemyDamage;
            }

            return IsRangedEnemy(index) ? RangedEnemyDamage : MeleeEnemyDamage;
        }

        private bool IsBossEnemy(int index)
        {
            var enemy = enemies != null && index >= 0 && index < enemies.Length ? enemies[index] : null;
            return enemy != null && enemy.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsRangedEnemy(int index)
        {
            var enemy = enemies != null && index >= 0 && index < enemies.Length ? enemies[index] : null;
            return enemy != null && enemy.name.IndexOf("Ranged", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsEliteEnemy(int index)
        {
            var enemy = enemies != null && index >= 0 && index < enemies.Length ? enemies[index] : null;
            return enemy != null && enemy.name.IndexOf("Elite", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool BossUsesLineAttack(int index)
        {
            return enemyAttackModes != null && index >= 0 && index < enemyAttackModes.Length && enemyAttackModes[index] == AttackModeLine;
        }

        private bool BossEnraged(int index)
        {
            return bossEnraged != null && index >= 0 && index < bossEnraged.Length && bossEnraged[index];
        }

        private void AdvanceEnemyAttackMode(int index)
        {
            if (!IsBossEnemy(index) || enemyAttackModes == null || index < 0 || index >= enemyAttackModes.Length)
            {
                return;
            }

            enemyAttackModes[index] = BossUsesLineAttack(index) ? AttackModeCircle : AttackModeLine;
        }

        private Vector3 EnemyAimDirection(int index, Transform enemy)
        {
            if (enemyAttackAimDirections != null && index >= 0 && index < enemyAttackAimDirections.Length && enemyAttackAimDirections[index].sqrMagnitude > 0.01f)
            {
                return enemyAttackAimDirections[index].normalized;
            }

            var fallback = player != null && enemy != null ? player.position - enemy.position : Vector3.forward;
            fallback.y = 0f;
            return fallback.sqrMagnitude > 0.01f ? fallback.normalized : Vector3.forward;
        }

        private bool EnemyAttackHitsPlayer(int index, Vector3 toPlayer)
        {
            if (IsRangedEnemy(index) || (IsBossEnemy(index) && BossUsesLineAttack(index)))
            {
                var aim = EnemyAimDirection(index, enemies[index]);
                var forwardDistance = Vector3.Dot(toPlayer, aim);
                if (forwardDistance < -0.2f || forwardDistance > EnemyAttackRangeFor(index) + 0.35f)
                {
                    return false;
                }

                var lateral = toPlayer - aim * forwardDistance;
                var width = IsBossEnemy(index) ? 0.88f : 0.42f;
                return lateral.magnitude <= width;
            }

            return toPlayer.magnitude <= EnemyAttackRangeFor(index) + 0.28f;
        }

        private void TryTriggerBossEnrage(int index, Transform enemy)
        {
            if (!IsBossEnemy(index) || BossEnraged(index) || enemyHealth[index] <= 0f || enemyHealth[index] > BossEnrageHealth)
            {
                return;
            }

            bossEnraged[index] = true;
            enemyAttackModes[index] = AttackModeLine;
            enemyAttackTimer[index] = 0.18f;
            enemyWindupTimer[index] = 0f;
            enemy.localScale = EnemyHitScale(index, false);
            var away = player.position - enemy.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.01f)
            {
                var pushed = player.position + away.normalized * 0.72f;
                player.position = ResolveBlockedMove(player.position, pushed);
            }

            PlayCue(rewardReadyClip, 0.70f);
        }

        private Vector3 EnemyHitScale(int index, bool defeated)
        {
            var baseScale = initialEnemyScales != null && index >= 0 && index < initialEnemyScales.Length && initialEnemyScales[index] != Vector3.zero
                ? initialEnemyScales[index]
                : Vector3.one;
            if (defeated)
            {
                return baseScale * 0.86f;
            }

            return baseScale * (BossEnraged(index) ? 1.18f : 1.08f);
        }

        private bool LumenEdgeActive()
        {
            return previousRewardLoaded || previousSecondRewardLoaded || firstRewardClaimedThisRun || secondRewardClaimedThisRun;
        }

        private int ReturnedRelicCount()
        {
            var count = previousRewardLoaded ? 1 : 0;
            if (previousSecondRewardLoaded)
            {
                count += 1;
            }

            return count;
        }

        private int ClaimedRelicCountThisRun()
        {
            var count = firstRewardClaimedThisRun ? 1 : 0;
            if (secondRewardClaimedThisRun)
            {
                count += 1;
            }

            return count;
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

        private string BossHealthSuffix()
        {
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                if (!IsBossEnemy(i))
                {
                    continue;
                }

                return enemyHealth[i] > 0f
                    ? $"  Boss {Mathf.CeilToInt(enemyHealth[i])}{(BossEnraged(i) ? " ENRAGED" : string.Empty)}"
                    : "  Boss down";
            }

            return string.Empty;
        }

        private bool BossDefeatedThisRun()
        {
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                if (IsBossEnemy(i))
                {
                    return enemyHealth[i] <= 0f;
                }
            }

            return bossDefeatedThisRun;
        }

        private bool TryGetObjectiveTarget(out Transform target, out string label)
        {
            target = null;
            label = string.Empty;

            if (runFailed)
            {
                return false;
            }

            if (runCleared && !returnedToHubThisRun && returnGatePoint != null)
            {
                target = returnGatePoint;
                label = "RETURN";
                return true;
            }

            if (!ToolGateSolved() && requiredToolNode != null)
            {
                target = requiredToolNode.transform;
                label = "TOOL NODE";
                return true;
            }

            if (!AllEnemiesDefeated())
            {
                target = ObjectiveEnemy();
                if (target != null)
                {
                    label = target.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0
                        ? "BOSS"
                        : target.name.IndexOf("Elite", StringComparison.OrdinalIgnoreCase) >= 0
                            ? "ELITE"
                            : "ENEMY";
                    return true;
                }
            }

            if (!firstRewardClaimedThisRun && RewardReady() && rewardClaimPoint != null)
            {
                target = rewardClaimPoint;
                label = "RELIC";
                return true;
            }

            if (firstRewardClaimedThisRun && !SecondToolGateSolved() && secondToolNode != null)
            {
                target = secondToolNode.transform;
                label = "SECOND NODE";
                return true;
            }

            if (!secondRewardClaimedThisRun && SecondRewardReady() && secondRewardClaimPoint != null)
            {
                target = secondRewardClaimPoint;
                label = "SECOND RELIC";
                return true;
            }

            return false;
        }

        private Transform ObjectiveEnemy()
        {
            Transform nearest = null;
            Transform elite = null;
            var nearestDistance = float.PositiveInfinity;
            for (var i = 0; i < enemyHealth.Length; i++)
            {
                var enemy = enemies != null && i < enemies.Length ? enemies[i] : null;
                if (enemy == null || enemyHealth[i] <= 0f || !enemy.gameObject.activeSelf)
                {
                    continue;
                }

                if (IsBossEnemy(i))
                {
                    return enemy;
                }

                if (elite == null && IsEliteEnemy(i))
                {
                    elite = enemy;
                }

                var distance = player != null ? Vector3.Distance(player.position, enemy.position) : 0f;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }

            return elite != null ? elite : nearest;
        }

        private static string FormatRunTime(float seconds)
        {
            var safeSeconds = Mathf.Max(0f, seconds);
            var minutes = Mathf.FloorToInt(safeSeconds / 60f);
            var wholeSeconds = Mathf.FloorToInt(safeSeconds % 60f);
            return $"{minutes:0}:{wholeSeconds:00}";
        }

        private bool RewardReady()
        {
            return AllEnemiesDefeated() && ToolGateSolved() && !firstRewardClaimedThisRun;
        }

        private bool SecondRewardReady()
        {
            return AllEnemiesDefeated() && firstRewardClaimedThisRun && SecondToolGateSolved() && !secondRewardClaimedThisRun;
        }

        private bool ToolGateSolved()
        {
            return requiredToolNode == null || requiredToolNode.IsSolved;
        }

        private bool SecondToolGateSolved()
        {
            return secondToolNode == null || secondToolNode.IsSolved;
        }

        private Vector3 ResolveBlockedMove(Vector3 current, Vector3 proposed)
        {
            proposed = ClampToPlayableBounds(proposed);
            if (!IsPositionBlocked(proposed))
            {
                return proposed;
            }

            var xOnly = new Vector3(proposed.x, proposed.y, current.z);
            if (!IsPositionBlocked(xOnly))
            {
                return xOnly;
            }

            var zOnly = new Vector3(current.x, proposed.y, proposed.z);
            if (!IsPositionBlocked(zOnly))
            {
                return zOnly;
            }

            return current;
        }

        private void MoveEnemy(Transform enemy, Vector3 direction, float speed, float deltaTime)
        {
            if (enemy == null || direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var proposed = enemy.position + direction.normalized * speed * deltaTime;
            enemy.position = ResolveBlockedMove(enemy.position, proposed);
        }

        private static Vector3 ClampToPlayableBounds(Vector3 position)
        {
            return new Vector3(
                Mathf.Clamp(position.x, MinX, MaxX),
                position.y,
                Mathf.Clamp(position.z, MinZ, MaxZ));
        }

        private bool IsPositionBlocked(Vector3 position)
        {
            if (!ToolGateSolved() && InsideRect(position, -3.65f, -1.55f, -3.05f, 1.35f))
            {
                return true;
            }

            if (!SecondToolGateSolved() && InsideRect(position, 9.15f, 11.25f, -6.15f, -4.40f))
            {
                return true;
            }

            return false;
        }

        private static bool InsideRect(Vector3 position, float minX, float maxX, float minZ, float maxZ)
        {
            return position.x >= minX && position.x <= maxX && position.z >= minZ && position.z <= maxZ;
        }

        private void UpdateProgressFlags()
        {
            if (requiredToolNode != null && requiredToolNode.IsSolved && !previousShortcutLoaded)
            {
                previousShortcutLoaded = true;
                PersistProgress();
            }

            if (secondToolNode != null && secondToolNode.IsSolved && !previousSecondNodeLoaded)
            {
                previousSecondNodeLoaded = true;
                PersistProgress();
            }
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

            var axisX = SafeAxis("Horizontal");
            var axisZ = SafeAxis("Vertical");
            if (Mathf.Abs(axisX) > Mathf.Abs(x))
            {
                x = axisX;
            }
            if (Mathf.Abs(axisZ) > Mathf.Abs(z))
            {
                z = axisZ;
            }

            var input = new Vector3(x, 0f, z);
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private static float SafeAxis(string axisName)
        {
            try
            {
                return Input.GetAxisRaw(axisName);
            }
            catch (ArgumentException)
            {
                return 0f;
            }
        }

        private static bool Pressed(params KeyCode[] keys)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i] != KeyCode.None && Input.GetKeyDown(keys[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void MigrateLegacyPlayerPrefs(FourfoldProgressData data)
        {
            data.d020Cleared = PlayerPrefs.GetInt(SaveKeyCleared, 0) == 1;
            data.d020ShortcutOpened = PlayerPrefs.GetInt(SaveKeyShortcutOpened, 0) == 1;
            data.d020RewardClaimed = PlayerPrefs.GetInt(SaveKeyRewardClaimed, 0) == 1 && data.d020Cleared;
            data.d020SecondNodeOpened = PlayerPrefs.GetInt(SaveKeySecondNodeOpened, 0) == 1;
            data.d020SecondRewardClaimed = PlayerPrefs.GetInt(SaveKeySecondRewardClaimed, 0) == 1 && data.d020Cleared;
            data.d020ReturnedToHub = PlayerPrefs.GetInt(SaveKeyReturnedToHub, 0) == 1 && data.d020Cleared;
            data.d020ClearCount = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeyClearCount, 0));
            data.d020FailureCount = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeyFailureCount, 0));
            data.d020BestClearTimeSeconds = Mathf.Max(0f, PlayerPrefs.GetFloat(SaveKeyBestClearTime, 0f));
        }

        private void PersistProgress()
        {
            if (progressData == null)
            {
                progressData = new FourfoldProgressData();
            }

            progressData.currentScene = returnedToHubThisRun ? FourfoldGameIds.SceneHubCrossroads : FourfoldGameIds.SceneD020VerticalSlice;
            progressData.hubUnlocked = true;
            progressData.regionD020Unlocked = true;
            progressData.regionD020Cleared = previousClearLoaded;
            progressData.lastCompletedRegion = progressData.regionD020Cleared ? FourfoldGameIds.RegionD020 : progressData.lastCompletedRegion;
            progressData.hubSpawnId = (previousReturnedToHubLoaded || returnedToHubThisRun) ? FourfoldGameIds.HubSpawnReturnGate : progressData.hubSpawnId;
            progressData.lumenRodUnlocked = true;
            progressData.d020Cleared = previousClearLoaded;
            progressData.d020BossDefeated = previousClearLoaded;
            progressData.d020ShortcutOpened = previousShortcutLoaded;
            progressData.d020RewardClaimed = previousRewardLoaded;
            progressData.d020SecondNodeOpened = previousSecondNodeLoaded;
            progressData.d020SecondRewardClaimed = previousSecondRewardLoaded;
            progressData.d020ReturnedToHub = previousReturnedToHubLoaded;
            progressData.d020ClearCount = Mathf.Max(0, clearCount);
            progressData.d020FailureCount = Mathf.Max(0, failureCount);
            progressData.d020BestClearTimeSeconds = Mathf.Max(0f, bestClearTimeSeconds);
            FourfoldProgressSave.Save(progressData);
            MirrorLegacyPlayerPrefs();
        }

        private void MirrorLegacyPlayerPrefs()
        {
            PlayerPrefs.SetInt(SaveKeyCleared, progressData.d020Cleared ? 1 : 0);
            PlayerPrefs.SetInt(SaveKeyShortcutOpened, progressData.d020ShortcutOpened ? 1 : 0);
            PlayerPrefs.SetInt(SaveKeyRewardClaimed, progressData.d020RewardClaimed ? 1 : 0);
            PlayerPrefs.SetInt(SaveKeySecondNodeOpened, progressData.d020SecondNodeOpened ? 1 : 0);
            PlayerPrefs.SetInt(SaveKeySecondRewardClaimed, progressData.d020SecondRewardClaimed ? 1 : 0);
            PlayerPrefs.SetInt(SaveKeyReturnedToHub, progressData.d020ReturnedToHub ? 1 : 0);
            PlayerPrefs.SetInt(SaveKeyClearCount, progressData.d020ClearCount);
            PlayerPrefs.SetInt(SaveKeyFailureCount, progressData.d020FailureCount);
            PlayerPrefs.SetFloat(SaveKeyBestClearTime, progressData.d020BestClearTimeSeconds);
            PlayerPrefs.Save();
        }

        private void EnsureAudioSource()
        {
            var sources = GetComponents<AudioSource>();
            if (audioSource == null)
            {
                audioSource = sources.Length > 0 ? sources[0] : null;
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (musicSource == null)
            {
                musicSource = sources.Length > 1 ? sources[1] : null;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = 0.85f;

            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.dopplerLevel = 0f;
            musicSource.volume = 0.26f;
        }

        private void UpdateMusicState()
        {
            if (musicSource == null)
            {
                EnsureAudioSource();
            }

            var targetClip = BossMusicActive() && bossMusicClip != null ? bossMusicClip : explorationMusicClip;
            if (musicSource == null || targetClip == null)
            {
                return;
            }

            if (currentMusicClip == targetClip && musicSource.isPlaying)
            {
                return;
            }

            currentMusicClip = targetClip;
            musicSource.clip = targetClip;
            musicSource.volume = (targetClip == bossMusicClip ? 0.31f : 0.24f) * MusicVolumeScale();
            musicSource.loop = true;
            musicSource.Play();
        }

        private bool BossMusicActive()
        {
            if (enemies == null || player == null || enemyHealth == null)
            {
                return false;
            }

            for (var i = 0; i < enemies.Length && i < enemyHealth.Length; i++)
            {
                if (!IsBossEnemy(i) || enemies[i] == null || enemyHealth[i] <= 0f)
                {
                    continue;
                }

                var distance = Vector3.Distance(player.position, enemies[i].position);
                return distance <= EnemySenseRange + 1.5f || BossEnraged(i);
            }

            return false;
        }

        private void EnsureExplorationReferences()
        {
            if (explorationTool == null)
            {
                explorationTool = GetComponent<ExplorationTool>();
            }

            if (explorationTool != null)
            {
                explorationTool.useKey = KeyCode.Q;
                explorationTool.alternateUseKey = KeyCode.JoystickButton2;
            }

            if (requiredToolNode == null && explorationTool != null && explorationTool.nodes != null && explorationTool.nodes.Length > 0)
            {
                requiredToolNode = explorationTool.nodes[0];
            }
            if (secondToolNode == null && explorationTool != null && explorationTool.nodes != null && explorationTool.nodes.Length > 1)
            {
                secondToolNode = explorationTool.nodes[1];
            }
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
                audioSource.PlayOneShot(clip, Mathf.Clamp01(volume * SfxVolumeScale()));
            }
        }

        private float MusicVolumeScale()
        {
            if (progressData == null)
            {
                return 1f;
            }

            return Mathf.Clamp01(progressData.masterVolume) * Mathf.Clamp01(progressData.musicVolume);
        }

        private float SfxVolumeScale()
        {
            if (progressData == null)
            {
                return 1f;
            }

            return Mathf.Clamp01(progressData.masterVolume) * Mathf.Clamp01(progressData.sfxVolume);
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

        private static GameObject CreateBeamRead(string name, Material material)
        {
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = name;
            beam.transform.localScale = new Vector3(0.16f, 0.035f, 1.0f);
            var renderer = beam.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var collider = beam.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            return beam;
        }

        private void OnGUI()
        {
            var width = Mathf.Min(600f, Screen.width - 32f);
            var rect = new Rect(16f, 16f, width, 214f);
            GUI.Box(rect, GUIContent.none);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 42, 16, 24),
                normal = { textColor = Color.white },
                wordWrap = true
            };

            var objective = runCleared
                ? returnedToHubThisRun
                    ? "RESULT: returned to hub. Press R to replay the run."
                    : "CLEAR: rewards secured. Press E/Y to return to hub."
                : runFailed
                    ? lastLostRelicsOnFailure > 0
                        ? $"FAILED: lost {lastLostRelicsOnFailure} unreturned relic reward(s). Press R to retry."
                        : "FAILED: press R to retry."
                    : !ToolGateSolved()
                        ? "Activate the glowing tool node with Q or gamepad X."
                        : BossDefeatedThisRun() && !AllEnemiesDefeated()
                        ? "BOSS DOWN: finish the remaining enemies."
                        : !AllEnemiesDefeated()
                        ? "Defeat the enemies, then claim the relic."
                        : !firstRewardClaimedThisRun
                        ? "Claim the first relic chest with E."
                        : !SecondToolGateSolved()
                        ? "Use the same tool on the second node."
                        : !secondRewardClaimedThisRun
                        ? "Claim the second relic with E."
                        : "Rewards secured. Press R to replay.";

            var toolState = explorationTool == null
                ? "Tool --"
                : explorationTool.IsReady
                    ? "Tool READY"
                    : $"Tool cooldown {Mathf.CeilToInt(explorationTool.Cooldown01 * 100f)}%";
            var relicState = LumenEdgeActive()
                ? $"Relic Lumen Edge active  Returned {ReturnedRelicCount()}/2  Run {ClaimedRelicCountThisRun()}/2"
                : $"Relic locked  Returned {ReturnedRelicCount()}/2  Run {ClaimedRelicCountThisRun()}/2";
            var resultState = clearCount > 0
                ? $"Clears returned {clearCount}"
                : ClaimedRelicCountThisRun() > 0
                    ? $"Return to save {ClaimedRelicCountThisRun()} relic reward(s)"
                    : "Clear the boss, claim both relics, return to hub";
            if (failureCount > 0)
            {
                resultState += $"  Failed runs {failureCount}";
            }
            var timeState = returnedToHubThisRun && lastReturnTimeSeconds > 0f
                ? $"Returned {FormatRunTime(lastReturnTimeSeconds)}{(bestClearTimeImproved ? "  BEST" : string.Empty)}"
                : $"Run {FormatRunTime(runTimerSeconds)}";
            if (bestClearTimeSeconds > 0f)
            {
                timeState += $"  Best {FormatRunTime(bestClearTimeSeconds)}";
            }

            GUI.Label(new Rect(30f, 26f, width - 28f, 34f), $"HP {Mathf.CeilToInt(playerHealth)} / {Mathf.CeilToInt(PlayerMaxHealth)}{BossHealthSuffix()}", style);
            GUI.Label(new Rect(30f, 58f, width - 28f, 30f), toolState, style);
            GUI.Label(new Rect(30f, 88f, width - 28f, 30f), relicState, style);
            GUI.Label(new Rect(30f, 118f, width - 28f, 52f), objective, style);
            GUI.Label(new Rect(30f, 164f, width - 28f, 52f), $"{resultState}  {timeState}  Move WASD/Stick  Attack Space/A  Dodge Shift/B  Tool Q/X  Interact E/Y  Pause Esc/Menu", style);
            DrawObjectiveMarker(style);

            if (paused)
            {
                var pauseWidth = Mathf.Min(480f, Screen.width - 48f);
                var pauseHeight = 168f;
                var pauseRect = new Rect((Screen.width - pauseWidth) * 0.5f, (Screen.height - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
                GUI.Box(pauseRect, GUIContent.none);
                GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 22f, pauseWidth - 48f, 32f), "PAUSED", style);
                GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 58f, pauseWidth - 48f, 92f), "Solo run is stopped. Esc/Menu resumes. R/Start retries. Backspace/Select returns to title without banking unreturned relics.", style);
            }
            else if (bossDefeatTimer > 0f)
            {
                var beatWidth = Mathf.Min(520f, Screen.width - 48f);
                var beatHeight = 116f;
                var beatRect = new Rect((Screen.width - beatWidth) * 0.5f, Screen.height * 0.22f, beatWidth, beatHeight);
                GUI.Box(beatRect, GUIContent.none);
                GUI.Label(new Rect(beatRect.x + 24f, beatRect.y + 20f, beatWidth - 48f, 32f), "BOSS DOWN", style);
                GUI.Label(new Rect(beatRect.x + 24f, beatRect.y + 56f, beatWidth - 48f, 44f), "Secure the relics, then return to save progress.", style);
            }

            if (previousReturnedToHubLoaded && !runCleared)
            {
                GUI.Label(new Rect(16f, Screen.height - 42f, width, 28f), "Local progress: returned to hub after a clear.", style);
            }
            else if (previousClearLoaded && !runCleared)
            {
                var progress = previousSecondRewardLoaded
                    ? "Local progress: both relics secured before."
                    : previousRewardLoaded
                        ? "Local progress: first relic secured before."
                        : "Local progress: this slice was cleared before.";
                GUI.Label(new Rect(16f, Screen.height - 42f, width, 28f), progress, style);
            }
            else if (previousShortcutLoaded)
            {
                GUI.Label(new Rect(16f, Screen.height - 42f, width, 28f), "Local progress: shortcut opened.", style);
            }
        }

        private void DrawObjectiveMarker(GUIStyle style)
        {
            if (!TryGetObjectiveTarget(out var target, out var label) || target == null)
            {
                return;
            }

            var camera = fixedCamera != null ? fixedCamera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var markerWorldPosition = target.position + new Vector3(0f, 0.9f, 0f);
            var viewport = camera.WorldToViewportPoint(markerWorldPosition);
            var behindCamera = viewport.z < 0f;
            if (behindCamera)
            {
                viewport.x = 1f - viewport.x;
                viewport.y = 1f - viewport.y;
            }

            var offscreen = behindCamera || viewport.x < 0.08f || viewport.x > 0.92f || viewport.y < 0.10f || viewport.y > 0.90f;
            var screenX = Mathf.Clamp(viewport.x * Screen.width, 64f, Screen.width - 190f);
            var screenY = Mathf.Clamp((1f - viewport.y) * Screen.height, 64f, Screen.height - 72f);
            var distance = player != null ? Vector3.Distance(player.position, target.position) : 0f;
            var rect = new Rect(screenX - 58f, screenY - 18f, 174f, 38f);
            GUI.Box(rect, GUIContent.none);
            var prefix = offscreen ? "NEXT >" : "NEXT";
            GUI.Label(new Rect(rect.x + 12f, rect.y + 7f, rect.width - 24f, rect.height - 10f), $"{prefix} {label} {distance:0}m", style);
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
