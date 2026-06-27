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
        private const float RewardNoticeDuration = 3.4f;
        private const float MeleeEnemyDamage = 26f;
        private const float RangedEnemyDamage = 18f;
        private const float EliteEnemyDamage = 30f;
        private const float BossEnemyDamage = 34f;
        private const float InvulnerableAfterHit = 0.65f;
        private const float RewardRange = 1.8f;
        private const float LumenLinkHitRecovery = 4f;
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
        private const int PauseResume = 0;
        private const int PauseSettings = 1;
        private const int PauseRetry = 2;
        private const int PauseTitle = 3;
        private const int PauseMenuCount = 4;
        private const int FailureRetry = 0;
        private const int FailureHub = 1;
        private const int FailureTitle = 2;
        private const int FailureMenuCount = 3;
        private const int SettingsCount = 6;
        private const float AxisRepeatDelay = 0.24f;

        private enum PendingExitAction
        {
            None,
            RetryRun,
            ReturnToTitle
        }

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
        private float rewardNoticeTimer;
        private string rewardNoticeTitle = string.Empty;
        private string rewardNoticeBody = string.Empty;
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
        private PendingExitAction pendingExitAction;
        private bool pendingExitReturnToPaused;
        private bool settingsOpen;
        private int selectedPauseIndex;
        private int selectedFailureIndex;
        private int selectedSettingIndex;
        private float axisRepeatTimer;

        public static bool LayoutFitsResolution(int screenWidth, int screenHeight, bool pauseOpen, out string reason)
        {
            reason = string.Empty;
            if (screenWidth < 960 || screenHeight < 540)
            {
                reason = $"resolution too small for D-020 HUD: {screenWidth}x{screenHeight}";
                return false;
            }

            var hudRect = PrimaryHudRect(screenWidth);
            var bossHudRect = BossHudRect(screenWidth);
            var bottomHintRect = BottomHintRect(screenWidth, screenHeight);
            var bottomProgressRect = BottomProgressRect(screenWidth, screenHeight);
            if (hudRect.xMax > screenWidth - 16f || hudRect.yMax >= bottomHintRect.y - 12f)
            {
                reason = $"D-020 primary HUD overlaps safe area or bottom hints at {screenWidth}x{screenHeight}: hud={hudRect} hints={bottomHintRect}";
                return false;
            }

            if (bossHudRect.x < hudRect.xMax + 12f || bossHudRect.xMax > screenWidth - 24f)
            {
                reason = $"D-020 boss HUD overlaps primary HUD or safe area at {screenWidth}x{screenHeight}: hud={hudRect} boss={bossHudRect}";
                return false;
            }

            if (bottomHintRect.x < 16f || bottomHintRect.xMax > screenWidth - 16f || bottomProgressRect.yMax > screenHeight - 10f)
            {
                reason = $"D-020 bottom hints exceed safe area at {screenWidth}x{screenHeight}: controls={bottomHintRect} progress={bottomProgressRect}";
                return false;
            }

            if (!pauseOpen)
            {
                return true;
            }

            var pauseWidth = Mathf.Min(560f, screenWidth - 48f);
            var pauseHeight = 326f;
            var pauseRect = new Rect((screenWidth - pauseWidth) * 0.5f, (screenHeight - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
            if (pauseRect.x < 24f || pauseRect.y < 24f || pauseRect.xMax > screenWidth - 24f || pauseRect.yMax > screenHeight - 24f)
            {
                reason = $"D-020 pause/confirm panel exceeds safe area at {screenWidth}x{screenHeight}: {pauseRect}";
                return false;
            }

            var failWidth = Mathf.Min(540f, screenWidth - 48f);
            var failHeight = 326f;
            var failRect = new Rect((screenWidth - failWidth) * 0.5f, (screenHeight - failHeight) * 0.5f, failWidth, failHeight);
            if (failRect.x < 24f || failRect.y < 24f || failRect.xMax > screenWidth - 24f || failRect.yMax > screenHeight - 24f)
            {
                reason = $"D-020 failure result panel exceeds safe area at {screenWidth}x{screenHeight}: {failRect}";
                return false;
            }

            var rewardNoticeWidth = Mathf.Min(440f, screenWidth - 48f);
            var rewardNoticeRect = new Rect(screenWidth - rewardNoticeWidth - 24f, 310f, rewardNoticeWidth, 104f);
            if (rewardNoticeRect.x < 24f || rewardNoticeRect.y < 24f || rewardNoticeRect.xMax > screenWidth - 24f || rewardNoticeRect.yMax > screenHeight - 24f)
            {
                reason = $"D-020 reward notice panel exceeds safe area at {screenWidth}x{screenHeight}: {rewardNoticeRect}";
                return false;
            }

            return true;
        }

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
            FourfoldInputPrompts.ObserveFrameInput();
            axisRepeatTimer = Mathf.Max(0f, axisRepeatTimer - Time.unscaledDeltaTime);
            UpdateToolInputLock();
            if (HandlePendingExitConfirmation())
            {
                return;
            }

            if (!paused && Pressed(retryKey, gamepadRetryKey))
            {
                RequestRetryRun();
                return;
            }

            if (Pressed(pauseKey, gamepadPauseKey))
            {
                if (settingsOpen)
                {
                    CloseSettings();
                    return;
                }

                SetPaused(!paused);
                return;
            }

            if (paused)
            {
                UpdatePauseInput();
                return;
            }

            if (runFailed)
            {
                UpdateFailureInput();
                return;
            }

            var dt = Time.deltaTime;
            attackTimer = Mathf.Max(0f, attackTimer - dt);
            attackReadTimer = Mathf.Max(0f, attackReadTimer - dt);
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldownTimer = Mathf.Max(0f, dodgeCooldownTimer - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);
            rewardNoticeTimer = Mathf.Max(0f, rewardNoticeTimer - dt);
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
                ApplyLumenLinkRecovery();
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
            var relicBoost = AnyRelicActive() ? 0.22f : 0f;
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
            ShowRewardNotice(
                FourfoldLanguage.T(progressData, "LUMEN EDGE EARNED", "LUMEN EDGE 獲得"),
                FourfoldLanguage.T(progressData, "Damage improves now; return to hub to save it.", "攻撃力が今すぐ上がる。ハブへ帰還すると保存される。"));
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
            ShowRewardNotice(
                FourfoldLanguage.T(progressData, "LUMEN WARD EARNED", "LUMEN WARD 獲得"),
                FourfoldLanguage.T(progressData, "Damage taken drops now; return to hub to save both rewards.", "被ダメージが今すぐ下がる。ハブへ帰還すると両方の報酬が保存される。"));
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
            pendingExitAction = PendingExitAction.None;
            SetPaused(false);
            PersistProgress();
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(FourfoldGameIds.UnitySceneTitle);
            }

            return true;
        }

        public bool TryReturnToHubAfterFailure()
        {
            if (!runFailed)
            {
                return false;
            }

            pendingExitAction = PendingExitAction.None;
            SetPaused(false);
            firstRewardClaimedThisRun = false;
            secondRewardClaimedThisRun = false;
            returnedToHubThisRun = false;
            PersistProgress();

            progressData = FourfoldProgressSave.Load();
            progressData.currentScene = FourfoldGameIds.SceneHubCrossroads;
            progressData.hubUnlocked = true;
            progressData.regionD020Unlocked = true;
            progressData.lumenRodUnlocked = true;
            progressData.d020FailureCount = Mathf.Max(0, failureCount);
            progressData.d020RewardClaimed = previousRewardLoaded;
            progressData.d020SecondRewardClaimed = previousSecondRewardLoaded;
            progressData.d020ReturnedToHub = previousReturnedToHubLoaded;
            FourfoldProgressSave.Save(progressData);

            if (Application.isPlaying)
            {
                SceneManager.LoadScene(FourfoldGameIds.UnitySceneHubCrossroads);
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
            pendingExitAction = PendingExitAction.None;
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
            selectedFailureIndex = FailureRetry;
            runTimerSeconds = 0f;
            lastReturnTimeSeconds = 0f;
            lastLostRelicsOnFailure = 0;
            bossDefeatTimer = 0f;
            rewardNoticeTimer = 0f;
            rewardNoticeTitle = string.Empty;
            rewardNoticeBody = string.Empty;
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
            selectedFailureIndex = FailureRetry;
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
            if (!paused)
            {
                settingsOpen = false;
            }
            else
            {
                selectedPauseIndex = 0;
            }

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

        public void OpenSettings()
        {
            progressData = FourfoldProgressSave.Load();
            SetPaused(true);
            settingsOpen = true;
            selectedSettingIndex = 0;
        }

        public void CloseSettings()
        {
            settingsOpen = false;
            SaveSettings();
        }

        public void AdjustSelectedSetting(float delta)
        {
            progressData = FourfoldProgressSave.Load();
            var step = delta >= 0f ? 0.1f : -0.1f;
            switch (selectedSettingIndex)
            {
                case 0:
                    progressData.masterVolume = Mathf.Clamp01(progressData.masterVolume + step);
                    break;
                case 1:
                    progressData.musicVolume = Mathf.Clamp01(progressData.musicVolume + step);
                    break;
                case 2:
                    progressData.sfxVolume = Mathf.Clamp01(progressData.sfxVolume + step);
                    break;
                case 3:
                    progressData.uiScale = Mathf.Clamp(progressData.uiScale + step, 0.85f, 1.25f);
                    break;
                case 4:
                    progressData.language = FourfoldLanguage.Toggle(progressData.language);
                    break;
                default:
                    progressData.showControlHints = !progressData.showControlHints;
                    break;
            }

            SaveSettings();
            if (musicSource != null)
            {
                musicSource.volume = (currentMusicClip == bossMusicClip ? 0.31f : 0.24f) * MusicVolumeScale();
            }
        }

        private void UpdatePauseInput()
        {
            if (settingsOpen)
            {
                UpdateSettingsInput();
                return;
            }

            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedPauseIndex = Wrap(selectedPauseIndex - 1, PauseMenuCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedPauseIndex = Wrap(selectedPauseIndex + 1, PauseMenuCount);
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadAttackKey))
            {
                ActivatePauseSelection();
                return;
            }

            if (Pressed(retryKey, gamepadRetryKey))
            {
                RequestRetryRun();
                return;
            }

            if (Pressed(returnToTitleKey, gamepadReturnToTitleKey))
            {
                RequestReturnToTitle();
            }
        }

        private void UpdateFailureInput()
        {
            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedFailureIndex = Wrap(selectedFailureIndex - 1, FailureMenuCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedFailureIndex = Wrap(selectedFailureIndex + 1, FailureMenuCount);
            }

            if (Pressed(retryKey, gamepadRetryKey))
            {
                RequestRetryRun();
                return;
            }

            if (Pressed(returnToTitleKey, gamepadReturnToTitleKey))
            {
                RequestReturnToTitle();
                return;
            }

            if (!Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadAttackKey))
            {
                return;
            }

            if (selectedFailureIndex == FailureRetry)
            {
                RequestRetryRun();
            }
            else if (selectedFailureIndex == FailureHub)
            {
                TryReturnToHubAfterFailure();
            }
            else
            {
                RequestReturnToTitle();
            }
        }

        private void UpdateSettingsInput()
        {
            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedSettingIndex = Wrap(selectedSettingIndex - 1, SettingsCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedSettingIndex = Wrap(selectedSettingIndex + 1, SettingsCount);
            }

            if (Pressed(KeyCode.LeftArrow, KeyCode.A) || HorizontalAxisPressed(-1f))
            {
                AdjustSelectedSetting(-1f);
            }
            else if (Pressed(KeyCode.RightArrow, KeyCode.D) || HorizontalAxisPressed(1f))
            {
                AdjustSelectedSetting(1f);
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadAttackKey) || Pressed(returnToTitleKey, gamepadReturnToTitleKey, gamepadDodgeKey))
            {
                CloseSettings();
            }
        }

        private void ActivatePauseSelection()
        {
            switch (selectedPauseIndex)
            {
                case PauseResume:
                    SetPaused(false);
                    break;
                case PauseSettings:
                    OpenSettings();
                    break;
                case PauseRetry:
                    RequestRetryRun();
                    break;
                case PauseTitle:
                    RequestReturnToTitle();
                    break;
            }
        }

        private void SaveSettings()
        {
            if (progressData == null)
            {
                progressData = FourfoldProgressSave.Load();
            }

            progressData.settingsInitialized = true;
            FourfoldProgressSave.Save(progressData);
        }

        private void RequestRetryRun()
        {
            if (ShouldConfirmRunAbandon())
            {
                OpenPendingExitConfirmation(PendingExitAction.RetryRun);
                return;
            }

            ResetRun();
        }

        private void RequestReturnToTitle()
        {
            if (ShouldConfirmRunAbandon())
            {
                OpenPendingExitConfirmation(PendingExitAction.ReturnToTitle);
                return;
            }

            TryReturnToTitle();
        }

        private bool ShouldConfirmRunAbandon()
        {
            return !returnedToHubThisRun && ClaimedRelicCountThisRun() > 0;
        }

        private void OpenPendingExitConfirmation(PendingExitAction action)
        {
            pendingExitAction = action;
            pendingExitReturnToPaused = paused;
            SetPaused(true);
        }

        private bool HandlePendingExitConfirmation()
        {
            if (pendingExitAction == PendingExitAction.None)
            {
                return false;
            }

            if (Pressed(pauseKey, gamepadPauseKey, gamepadDodgeKey))
            {
                var returnToPaused = pendingExitReturnToPaused;
                pendingExitAction = PendingExitAction.None;
                SetPaused(returnToPaused);
                return true;
            }

            var confirmPressed = Pressed(interactKey, gamepadInteractKey, gamepadAttackKey);
            if (pendingExitAction == PendingExitAction.RetryRun)
            {
                confirmPressed = confirmPressed || Pressed(retryKey, gamepadRetryKey);
            }
            else if (pendingExitAction == PendingExitAction.ReturnToTitle)
            {
                confirmPressed = confirmPressed || Pressed(returnToTitleKey, gamepadReturnToTitleKey);
            }

            if (!confirmPressed)
            {
                return true;
            }

            var action = pendingExitAction;
            pendingExitAction = PendingExitAction.None;
            if (action == PendingExitAction.RetryRun)
            {
                ResetRun();
            }
            else
            {
                TryReturnToTitle();
            }

            return true;
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
            var damage = EnemyBaseDamageFor(index);
            return LumenWardActive() ? damage * 0.78f : damage;
        }

        private float EnemyBaseDamageFor(int index)
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
            return firstRewardClaimedThisRun || (previousRewardLoaded && progressData != null && progressData.d020EdgeEquipped);
        }

        private bool LumenWardActive()
        {
            return secondRewardClaimedThisRun || (previousSecondRewardLoaded && progressData != null && progressData.d020WardEquipped);
        }

        private bool LumenLinkActive()
        {
            return LumenEdgeActive() && LumenWardActive();
        }

        private float LumenLinkRecoveryAmount()
        {
            return LumenLinkActive() ? LumenLinkHitRecovery : 0f;
        }

        private void ApplyLumenLinkRecovery()
        {
            var recovery = LumenLinkRecoveryAmount();
            if (recovery <= 0f || playerHealth >= PlayerMaxHealth)
            {
                return;
            }

            playerHealth = Mathf.Min(PlayerMaxHealth, playerHealth + recovery);
        }

        private void ShowRewardNotice(string title, string body)
        {
            rewardNoticeTitle = title;
            rewardNoticeBody = body;
            rewardNoticeTimer = RewardNoticeDuration;
        }

        private bool AnyRelicActive()
        {
            return LumenEdgeActive() || LumenWardActive();
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

        private int EquippedRelicCount()
        {
            var count = previousRewardLoaded && progressData != null && progressData.d020EdgeEquipped ? 1 : 0;
            if (previousSecondRewardLoaded && progressData != null && progressData.d020WardEquipped)
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

        private string RelicStateText()
        {
            var returned = ReturnedRelicCount();
            var equipped = EquippedRelicCount();
            var run = ClaimedRelicCountThisRun();
            if (LumenEdgeActive() && LumenWardActive())
            {
                return FourfoldLanguage.T(
                    progressData,
                    $"Lumen Link +DMG -DMG +HP  Saved{returned}/2 Equipped{equipped}/2 Run{run}/2",
                    $"Lumen Link +攻撃 -被弾 +回復  保存{returned}/2 装備{equipped}/2 今回{run}/2");
            }

            if (LumenEdgeActive())
            {
                return FourfoldLanguage.T(
                    progressData,
                    $"Lumen Edge +DMG  Saved{returned}/2 Equipped{equipped}/2 Run{run}/2",
                    $"Lumen Edge +攻撃  保存{returned}/2 装備{equipped}/2 今回{run}/2");
            }

            if (LumenWardActive())
            {
                return FourfoldLanguage.T(
                    progressData,
                    $"Ward -DMG  Saved{returned}/2 Equipped{equipped}/2 Run{run}/2",
                    $"Ward -被弾  保存{returned}/2 装備{equipped}/2 今回{run}/2");
            }

            return FourfoldLanguage.T(
                progressData,
                $"Rewards  Saved{returned}/2 Equipped{equipped}/2 Run{run}/2",
                $"報酬  保存{returned}/2 装備{equipped}/2 今回{run}/2");
        }

        private string DodgeStateText()
        {
            if (dodgeTimer > 0f)
            {
                return FourfoldLanguage.T(progressData, "Dodge ACTIVE", "回避中");
            }

            if (dodgeCooldownTimer <= 0f)
            {
                return FourfoldLanguage.T(progressData, "Dodge READY", "回避 準備OK");
            }

            return FourfoldLanguage.T(
                progressData,
                $"Dodge CD {Mathf.CeilToInt((dodgeCooldownTimer / DodgeCooldown) * 100f)}%",
                $"回避 待機 {Mathf.CeilToInt((dodgeCooldownTimer / DodgeCooldown) * 100f)}%");
        }

        private string RunRiskStateText()
        {
            var runRewards = ClaimedRelicCountThisRun();
            if (returnedToHubThisRun)
            {
                return FourfoldLanguage.T(progressData, "RESULT: clear saved by hub return.", "結果: ハブ帰還でクリア保存済み。");
            }

            if (runFailed)
            {
                return FourfoldLanguage.T(progressData, "FAILED: saved hub progress unchanged.", "失敗: ハブ保存済みの進行は変わらない。");
            }

            if (runRewards > 0)
            {
                return FourfoldLanguage.T(
                    progressData,
                    $"AT RISK: hub return saves {runRewards} earned reward(s).",
                    $"リスク: 報酬 {runRewards} 個はハブ帰還で保存。");
            }

            return FourfoldLanguage.T(progressData, "RISK: failure restarts this attempt; saved progress stays.", "リスク: 失敗で攻略やり直し。保存済み進行は保持。");
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
                label = FourfoldLanguage.T(progressData, "RETURN", "帰還");
                return true;
            }

            if (!ToolGateSolved() && requiredToolNode != null)
            {
                target = requiredToolNode.transform;
                label = FourfoldLanguage.T(progressData, "ROUTE", "道");
                return true;
            }

            if (!AllEnemiesDefeated())
            {
                target = ObjectiveEnemy();
                if (target != null)
                {
                    label = target.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0
                        ? FourfoldLanguage.T(progressData, "BOSS", "ボス")
                        : target.name.IndexOf("Elite", StringComparison.OrdinalIgnoreCase) >= 0
                            ? FourfoldLanguage.T(progressData, "ELITE", "強敵")
                            : FourfoldLanguage.T(progressData, "ENEMY", "敵");
                    return true;
                }
            }

            if (!firstRewardClaimedThisRun && RewardReady() && rewardClaimPoint != null)
            {
                target = rewardClaimPoint;
                label = FourfoldLanguage.T(progressData, "REWARD", "報酬");
                return true;
            }

            if (firstRewardClaimedThisRun && !SecondToolGateSolved() && secondToolNode != null)
            {
                target = secondToolNode.transform;
                label = FourfoldLanguage.T(progressData, "SHORTCUT", "近道");
                return true;
            }

            if (!secondRewardClaimedThisRun && SecondRewardReady() && secondRewardClaimPoint != null)
            {
                target = secondRewardClaimPoint;
                label = FourfoldLanguage.T(progressData, "SECOND REWARD", "2つ目の報酬");
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

        private bool AxisPressed(float sign)
        {
            var value = SafeAxis("Vertical");
            if (sign < 0f)
            {
                value = -value;
            }

            return value > 0.55f && ConsumeAxisRepeat();
        }

        private bool HorizontalAxisPressed(float sign)
        {
            var value = SafeAxis("Horizontal");
            if (sign < 0f)
            {
                value = -value;
            }

            return value > 0.55f && ConsumeAxisRepeat();
        }

        private bool ConsumeAxisRepeat()
        {
            if (axisRepeatTimer > 0f)
            {
                return false;
            }

            axisRepeatTimer = AxisRepeatDelay;
            return true;
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

        private static int Wrap(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            return (value % count + count) % count;
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
            progressData.d020LoadoutInitialized = true;
            if (firstRewardClaimedThisRun && previousRewardLoaded)
            {
                progressData.d020EdgeEquipped = true;
            }
            if (secondRewardClaimedThisRun && previousSecondRewardLoaded)
            {
                progressData.d020WardEquipped = true;
            }
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
            if (progressData == null)
            {
                progressData = FourfoldProgressSave.Load();
            }

            var uiScale = FourfoldRuntimeUi.SafeUiScale(progressData);
            var rect = PrimaryHudRect(Screen.width);
            var width = rect.width;
            FourfoldRuntimeUi.DrawPanel(rect);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 42, 16, 24) * uiScale),
                normal = { textColor = Color.white },
                wordWrap = true
            };
            var mutedStyle = FourfoldRuntimeUi.MutedStyle(Screen.height, uiScale);
            var hpStyle = FourfoldRuntimeUi.BodyStyle(Screen.height, uiScale);

            var objective = runCleared
                ? returnedToHubThisRun
                    ? FourfoldInputPrompts.RegionReplayObjective(progressData)
                    : FourfoldInputPrompts.RegionReturnGateObjective(progressData)
                : runFailed
                    ? FourfoldInputPrompts.RegionRetryObjective(progressData)
                    : !ToolGateSolved()
                        ? FourfoldLanguage.T(progressData, "Step 1/6: use the tool to open the sealed route.", "手順1/6: ツールで封じられた道を開く。")
                        : BossDefeatedThisRun() && !AllEnemiesDefeated()
                        ? FourfoldLanguage.T(progressData, "BOSS DOWN: finish the remaining enemies to unlock rewards.", "ボス撃破: 残りの敵を倒すと報酬が開く。")
                        : !AllEnemiesDefeated()
                        ? FourfoldLanguage.T(progressData, "Step 2/6: defeat enemies, read tells, then claim the reward.", "手順2/6: 予兆を読みながら敵を倒し、報酬を獲得。")
                        : !firstRewardClaimedThisRun
                        ? FourfoldInputPrompts.RegionClaimEdgeObjective(progressData)
                        : !SecondToolGateSolved()
                        ? FourfoldLanguage.T(progressData, "Step 4/6: use the tool to open the shortcut seal.", "手順4/6: ツールで近道の封印を開く。")
                        : !secondRewardClaimedThisRun
                        ? FourfoldInputPrompts.RegionClaimWardObjective(progressData)
                        : FourfoldLanguage.T(progressData, "Step 6/6: return to hub to save this clear.", "手順6/6: ハブへ帰還して今回のクリアを保存。");

            var toolState = explorationTool == null
                ? FourfoldLanguage.T(progressData, "Tool --", "ツール --")
                : explorationTool.IsReady
                    ? FourfoldLanguage.T(progressData, "Tool READY", "ツール 準備OK")
                    : FourfoldLanguage.T(progressData, $"Tool CD {Mathf.CeilToInt(explorationTool.Cooldown01 * 100f)}%", $"ツール 待機 {Mathf.CeilToInt(explorationTool.Cooldown01 * 100f)}%");
            var relicState = RelicStateText();
            var resultState = clearCount > 0
                ? FourfoldLanguage.T(progressData, $"Saved clears {clearCount}", $"保存クリア {clearCount}")
                : ClaimedRelicCountThisRun() > 0
                    ? FourfoldLanguage.T(progressData, $"Return to save {ClaimedRelicCountThisRun()} reward(s)", $"帰還して報酬 {ClaimedRelicCountThisRun()} 個を保存")
                    : FourfoldLanguage.T(progressData, "Loop: tool, boss, rewards, hub return", "ループ: ツール、ボス、報酬、ハブ帰還");
            if (failureCount > 0)
            {
                resultState += FourfoldLanguage.T(progressData, $"  Failed runs {failureCount}", $"  失敗 {failureCount}");
            }
            var timeState = returnedToHubThisRun && lastReturnTimeSeconds > 0f
                ? FourfoldLanguage.T(progressData, $"Returned {FormatRunTime(lastReturnTimeSeconds)}{(bestClearTimeImproved ? "  BEST" : string.Empty)}", $"帰還 {FormatRunTime(lastReturnTimeSeconds)}{(bestClearTimeImproved ? "  BEST" : string.Empty)}")
                : FourfoldLanguage.T(progressData, $"Attempt {FormatRunTime(runTimerSeconds)}", $"攻略 {FormatRunTime(runTimerSeconds)}");
            if (bestClearTimeSeconds > 0f)
            {
                timeState += FourfoldLanguage.T(progressData, $"  Best {FormatRunTime(bestClearTimeSeconds)}", $"  最速 {FormatRunTime(bestClearTimeSeconds)}");
            }

            FourfoldRuntimeUi.DrawBar(new Rect(30f, 30f, width - 56f, 26f), playerHealth / PlayerMaxHealth, new Color(0.35f, 0.92f, 0.52f), $"HP {Mathf.CeilToInt(playerHealth)} / {Mathf.CeilToInt(PlayerMaxHealth)}", hpStyle);
            DrawBossHud(hpStyle, mutedStyle);
            var chipArea = width - 70f;
            var toolChipWidth = chipArea * 0.27f;
            var dodgeChipWidth = chipArea * 0.25f;
            var relicChipWidth = chipArea - toolChipWidth - dodgeChipWidth - 12f;
            FourfoldRuntimeUi.DrawChip(new Rect(30f, 64f, toolChipWidth, 30f), toolState, new Color(0.25f, 0.70f, 1.0f), mutedStyle);
            FourfoldRuntimeUi.DrawChip(new Rect(36f + toolChipWidth, 64f, dodgeChipWidth, 30f), DodgeStateText(), new Color(0.34f, 0.90f, 0.52f), mutedStyle);
            FourfoldRuntimeUi.DrawChip(new Rect(42f + toolChipWidth + dodgeChipWidth, 64f, relicChipWidth, 30f), relicState, new Color(1.0f, 0.72f, 0.24f), mutedStyle);
            GUI.Label(new Rect(30f, 104f, width - 56f, 50f), objective, style);
            DrawRunProgressRail(new Rect(30f, 156f, width - 56f, 34f), mutedStyle);
            FourfoldRuntimeUi.DrawDivider(30f, 198f, width - 56f);
            var riskColor = ClaimedRelicCountThisRun() > 0 && !returnedToHubThisRun
                ? new Color(1.0f, 0.46f, 0.22f)
                : new Color(0.34f, 0.90f, 0.52f);
            FourfoldRuntimeUi.DrawChip(new Rect(30f, 206f, width - 56f, 34f), RunRiskStateText(), riskColor, mutedStyle);
            GUI.Label(new Rect(30f, 246f, width - 56f, 26f), $"{resultState}  {timeState}", mutedStyle);
            DrawObjectiveMarker(style);
            DrawRewardNotice(style, mutedStyle);
            if (progressData == null || progressData.showControlHints)
            {
                DrawControlHint(style);
            }

            if (pendingExitAction != PendingExitAction.None)
            {
                DrawPendingExitConfirmation(style);
            }
            else if (paused)
            {
                var pauseWidth = Mathf.Min(480f, Screen.width - 48f);
                var pauseHeight = 326f;
                var pauseRect = new Rect((Screen.width - pauseWidth) * 0.5f, (Screen.height - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
                FourfoldRuntimeUi.DrawPanel(pauseRect);
                GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 18f, pauseWidth - 48f, 32f), settingsOpen ? FourfoldLanguage.T(progressData, "SETTINGS", "設定") : FourfoldLanguage.T(progressData, "PAUSED", "ポーズ"), style);
                if (settingsOpen)
                {
                    DrawSettings(pauseRect, style, mutedStyle);
                }
                else
                {
                    DrawPauseMenu(pauseRect, style, mutedStyle);
                }
            }
            else if (runFailed)
            {
                DrawFailureResult(style, mutedStyle);
            }
            else if (bossDefeatTimer > 0f)
            {
                var beatWidth = Mathf.Min(520f, Screen.width - 48f);
                var beatHeight = 116f;
                var beatRect = new Rect((Screen.width - beatWidth) * 0.5f, Screen.height * 0.22f, beatWidth, beatHeight);
                FourfoldRuntimeUi.DrawPanel(beatRect);
                GUI.Label(new Rect(beatRect.x + 24f, beatRect.y + 20f, beatWidth - 48f, 32f), FourfoldLanguage.T(progressData, "BOSS DOWN", "ボス撃破"), style);
                GUI.Label(new Rect(beatRect.x + 24f, beatRect.y + 56f, beatWidth - 48f, 44f), FourfoldLanguage.T(progressData, "Claim rewards, then return to hub to save the result.", "報酬を取得し、ハブへ帰還して結果を保存する。"), style);
            }

            if (previousReturnedToHubLoaded && !runCleared)
            {
                GUI.Label(BottomProgressRect(Screen.width, Screen.height), FourfoldLanguage.T(progressData, "Saved progress: hub return after clear is complete.", "保存済み進行: クリア後のハブ帰還は完了。"), style);
            }
            else if (previousClearLoaded && !runCleared)
            {
                var edgeEquipped = previousRewardLoaded && progressData != null && progressData.d020EdgeEquipped;
                var wardEquipped = previousSecondRewardLoaded && progressData != null && progressData.d020WardEquipped;
                var progress = edgeEquipped && wardEquipped
                    ? FourfoldLanguage.T(progressData, "Saved progress: Lumen Edge and Ward equipped from hub loadout.", "保存済み進行: ハブロードアウトのLumen EdgeとWardを装備中。")
                    : edgeEquipped
                        ? FourfoldLanguage.T(progressData, "Saved progress: Lumen Edge equipped from hub loadout.", "保存済み進行: ハブロードアウトのLumen Edgeを装備中。")
                        : wardEquipped
                            ? FourfoldLanguage.T(progressData, "Saved progress: Lumen Ward equipped from hub loadout.", "保存済み進行: ハブロードアウトのLumen Wardを装備中。")
                            : previousRewardLoaded || previousSecondRewardLoaded
                                ? FourfoldLanguage.T(progressData, "Saved progress: reward skills are stored; equip them in the hub loadout.", "保存済み進行: 報酬スキルは保存済み。ハブロードアウトで装備できる。")
                                : FourfoldLanguage.T(progressData, "Saved progress: this region was cleared before.", "保存済み進行: この地域はクリア済み。");
                GUI.Label(BottomProgressRect(Screen.width, Screen.height), progress, style);
            }
            else if (previousShortcutLoaded)
            {
                GUI.Label(BottomProgressRect(Screen.width, Screen.height), FourfoldLanguage.T(progressData, "Saved progress: shortcut opened.", "保存済み進行: ショートカット開通済み。"), style);
            }
        }

        private void DrawPendingExitConfirmation(GUIStyle style)
        {
            var panelWidth = Mathf.Min(560f, Screen.width - 48f);
            var panelHeight = 220f;
            var panelRect = new Rect((Screen.width - panelWidth) * 0.5f, (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);
            FourfoldRuntimeUi.DrawPanel(panelRect);

            var earnedRewardCount = ClaimedRelicCountThisRun();
            var actionLabel = pendingExitAction == PendingExitAction.RetryRun
                ? FourfoldLanguage.T(progressData, "RETRY REGION", "地域再挑戦")
                : FourfoldLanguage.T(progressData, "RETURN TO TITLE", "タイトルへ戻る");
            var confirmLabel = pendingExitAction == PendingExitAction.RetryRun
                ? FourfoldInputPrompts.RegionRetryConfirm(progressData)
                : FourfoldInputPrompts.RegionTitleConfirm(progressData);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 20f, panelWidth - 48f, 32f), actionLabel, style);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 58f, panelWidth - 48f, 96f), FourfoldLanguage.T(progressData, $"This attempt has {earnedRewardCount} earned reward(s) not saved by hub return. Leaving now restarts from the last saved hub state.", $"この攻略ではハブ帰還で未保存の報酬を {earnedRewardCount} 個取得済み。ここで離脱すると、最後にハブで保存した状態から再開する。"), style);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 164f, panelWidth - 48f, 42f), $"{confirmLabel}  {FourfoldInputPrompts.RegionPendingExitCancel(progressData)}", FourfoldRuntimeUi.MutedStyle(Screen.height));
        }

        private void DrawFailureResult(GUIStyle style, GUIStyle mutedStyle)
        {
            var panelWidth = Mathf.Min(540f, Screen.width - 48f);
            var panelHeight = 326f;
            var panelRect = new Rect((Screen.width - panelWidth) * 0.5f, (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);
            FourfoldRuntimeUi.DrawPanel(panelRect);

            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 18f, panelWidth - 48f, 32f), FourfoldLanguage.T(progressData, "ATTEMPT FAILED", "攻略失敗"), style);
            var lossText = lastLostRelicsOnFailure > 0
                ? FourfoldLanguage.T(progressData, $"Failed before hub return: {lastLostRelicsOnFailure} earned reward(s) were not saved. Hub-saved rewards remain active.", $"ハブ帰還前に失敗: 報酬 {lastLostRelicsOnFailure} 個は未保存。ハブ保存済みの報酬は有効。")
                : FourfoldLanguage.T(progressData, "No rewards were earned this attempt. Saved hub progress is unchanged.", "この攻略では報酬未取得。ハブ保存済みの進行は変わらない。");
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 58f, panelWidth - 48f, 48f), lossText, style);
            FourfoldRuntimeUi.DrawChip(new Rect(panelRect.x + 24f, panelRect.y + 112f, panelWidth - 48f, 34f), FourfoldLanguage.T(progressData, $"Failed attempts {failureCount}. Save rewards by returning to the hub after a clear.", $"失敗 {failureCount}。クリア後にハブへ戻ると報酬が保存される。"), new Color(1.0f, 0.46f, 0.22f), mutedStyle);
            FourfoldRuntimeUi.DrawDivider(panelRect.x + 24f, panelRect.y + 162f, panelWidth - 48f);

            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "Retry Region", "地域を再挑戦"),
                FourfoldLanguage.T(progressData, "Return to Hub", "ハブへ戻る"),
                FourfoldLanguage.T(progressData, "Return to Title", "タイトルへ戻る")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(panelRect.x + 32f, panelRect.y + 178f + i * 38f, panelWidth - 64f, 32f), labels[i], selectedFailureIndex == i, style);
            }

            GUI.Label(new Rect(panelRect.x + 32f, panelRect.y + panelHeight - 38f, panelWidth - 64f, 24f), FourfoldInputPrompts.RegionFailure(progressData), mutedStyle);
        }

        private void DrawRewardNotice(GUIStyle style, GUIStyle mutedStyle)
        {
            if (rewardNoticeTimer <= 0f || string.IsNullOrEmpty(rewardNoticeTitle) || runFailed || paused)
            {
                return;
            }

            var panelWidth = Mathf.Min(440f, Screen.width - 48f);
            var panelHeight = 104f;
            var panelRect = new Rect(Screen.width - panelWidth - 24f, 310f, panelWidth, panelHeight);
            FourfoldRuntimeUi.DrawPanel(panelRect);
            GUI.Label(new Rect(panelRect.x + 22f, panelRect.y + 16f, panelWidth - 44f, 28f), rewardNoticeTitle, style);
            GUI.Label(new Rect(panelRect.x + 22f, panelRect.y + 50f, panelWidth - 44f, 42f), rewardNoticeBody, mutedStyle);
        }

        private void DrawRunProgressRail(Rect rect, GUIStyle style)
        {
            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "1 Tool", "1 ツール"),
                FourfoldLanguage.T(progressData, "2 Boss", "2 ボス"),
                FourfoldLanguage.T(progressData, "3 Edge", "3 Edge"),
                FourfoldLanguage.T(progressData, "4 Route", "4 道"),
                FourfoldLanguage.T(progressData, "5 Ward", "5 Ward"),
                FourfoldLanguage.T(progressData, "6 Return", "6 帰還")
            };
            var completed = new[]
            {
                ToolGateSolved(),
                BossDefeatedThisRun(),
                firstRewardClaimedThisRun,
                SecondToolGateSolved(),
                secondRewardClaimedThisRun,
                returnedToHubThisRun
            };

            var activeIndex = 0;
            while (activeIndex < completed.Length && completed[activeIndex])
            {
                activeIndex++;
            }

            if (runFailed)
            {
                activeIndex = -1;
            }

            var gap = 5f;
            var cellWidth = (rect.width - gap * (labels.Length - 1)) / labels.Length;
            for (var i = 0; i < labels.Length; i++)
            {
                var color = completed[i]
                    ? new Color(0.34f, 0.90f, 0.52f)
                    : i == activeIndex
                        ? new Color(1.0f, 0.72f, 0.24f)
                        : new Color(0.45f, 0.50f, 0.58f);
                var label = completed[i]
                    ? labels[i] + FourfoldLanguage.T(progressData, " OK", " OK")
                    : i == activeIndex
                        ? labels[i] + FourfoldLanguage.T(progressData, " NOW", " NOW")
                        : labels[i];
                FourfoldRuntimeUi.DrawChip(new Rect(rect.x + i * (cellWidth + gap), rect.y, cellWidth, rect.height), label, color, style);
            }
        }

        private void DrawPauseMenu(Rect rect, GUIStyle style, GUIStyle mutedStyle)
        {
            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "Resume", "再開"),
                FourfoldLanguage.T(progressData, "Settings", "設定"),
                FourfoldLanguage.T(progressData, "Retry Region", "地域を再挑戦"),
                FourfoldLanguage.T(progressData, "Return to Title", "タイトルへ戻る")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 24f, rect.y + 58f + i * 38f, rect.width - 48f, 32f), labels[i], selectedPauseIndex == i, style);
            }

            var runRisk = RunRiskStateText();
            GUI.Label(new Rect(rect.x + 24f, rect.y + 220f, rect.width - 48f, 48f), runRisk, mutedStyle);
        }

        private void DrawBossHud(GUIStyle body, GUIStyle mutedStyle)
        {
            if (!TryGetBossHud(out var label, out var health01, out var enraged))
            {
                return;
            }

            var rect = BossHudRect(Screen.width);
            FourfoldRuntimeUi.DrawPanel(new Rect(rect.x - 6f, rect.y - 6f, rect.width + 12f, 76f));
            FourfoldRuntimeUi.DrawBar(rect, health01, enraged ? new Color(1.0f, 0.28f, 0.18f) : new Color(0.82f, 0.32f, 1.0f), label, body);
            var hint = enraged
                ? FourfoldLanguage.T(progressData, "Pattern changed. Watch the line attack.", "行動変化。直線攻撃に注意。")
                : FourfoldLanguage.T(progressData, "Boss pressure active. Keep position and read the tell.", "ボス戦中。位置取りと予兆を読む。");
            GUI.Label(new Rect(rect.x + 10f, rect.y + 34f, rect.width - 20f, 26f), hint, mutedStyle);
        }

        private bool TryGetBossHud(out string label, out float health01, out bool enraged)
        {
            label = string.Empty;
            health01 = 0f;
            enraged = false;

            if (enemies == null || enemyHealth == null)
            {
                return false;
            }

            for (var i = 0; i < enemies.Length && i < enemyHealth.Length; i++)
            {
                if (!IsBossEnemy(i) || enemies[i] == null || enemyHealth[i] <= 0f)
                {
                    continue;
                }

                var maxHealth = Mathf.Max(1f, InitialEnemyHealth(i));
                var currentHealth = Mathf.CeilToInt(enemyHealth[i]);
                enraged = BossEnraged(i);
                health01 = enemyHealth[i] / maxHealth;
                label = FourfoldLanguage.T(
                    progressData,
                    $"BOSS HP {currentHealth} / {Mathf.CeilToInt(maxHealth)}{(enraged ? "  ENRAGED" : string.Empty)}",
                    $"ボスHP {currentHealth} / {Mathf.CeilToInt(maxHealth)}{(enraged ? "  激化" : string.Empty)}");
                return true;
            }

            return false;
        }

        private void DrawSettings(Rect rect, GUIStyle style, GUIStyle mutedStyle)
        {
            progressData = FourfoldProgressSave.Load();
            var labels = new[]
            {
                $"{FourfoldLanguage.T(progressData, "Master Volume", "マスター音量")} {Mathf.RoundToInt(progressData.masterVolume * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "Music Volume", "音楽音量")} {Mathf.RoundToInt(progressData.musicVolume * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "SFX Volume", "効果音音量")} {Mathf.RoundToInt(progressData.sfxVolume * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "UI Scale", "UIサイズ")} {Mathf.RoundToInt(progressData.uiScale * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "Language", "言語")} {FourfoldLanguage.Label(progressData)}",
                $"{FourfoldLanguage.T(progressData, "Control Hints", "操作ヒント")} {(progressData.showControlHints ? FourfoldLanguage.T(progressData, "On", "表示") : FourfoldLanguage.T(progressData, "Off", "非表示"))}"
            };

            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 24f, rect.y + 54f + i * 34f, rect.width - 48f, 30f), labels[i], selectedSettingIndex == i, style);
            }

            GUI.Label(new Rect(rect.x + 24f, rect.y + rect.height - 42f, rect.width - 48f, 34f), FourfoldInputPrompts.SharedSettings(progressData), mutedStyle);
        }

        private void DrawControlHint(GUIStyle style)
        {
            if (paused || runFailed)
            {
                return;
            }

            GUI.Label(BottomHintRect(Screen.width, Screen.height), FourfoldInputPrompts.RegionControls(progressData), style);
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
            FourfoldRuntimeUi.DrawPanel(rect);
            var prefix = offscreen
                ? FourfoldLanguage.T(progressData, "NEXT >", "次 >")
                : FourfoldLanguage.T(progressData, "NEXT", "次");
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

        private static Rect BottomHintRect(int screenWidth, int screenHeight)
        {
            return new Rect(16f, screenHeight - 76f, Mathf.Min(900f, screenWidth - 32f), 30f);
        }

        private static Rect BottomProgressRect(int screenWidth, int screenHeight)
        {
            return new Rect(16f, screenHeight - 44f, Mathf.Min(760f, screenWidth - 32f), 28f);
        }

        private static Rect PrimaryHudRect(int screenWidth)
        {
            return new Rect(16f, 16f, Mathf.Min(600f, screenWidth - 32f), 282f);
        }

        private static Rect BossHudRect(int screenWidth)
        {
            var width = Mathf.Min(430f, Mathf.Max(300f, screenWidth - 660f));
            return new Rect(screenWidth - width - 24f, 30f, width, 30f);
        }
    }
}
