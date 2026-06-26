using System;
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
        public KeyCode gamepadAttackKey = KeyCode.JoystickButton0;
        public KeyCode gamepadDodgeKey = KeyCode.JoystickButton1;
        public KeyCode gamepadInteractKey = KeyCode.JoystickButton3;
        public KeyCode gamepadRetryKey = KeyCode.JoystickButton7;

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
        private const float BossAttackRange = 2.05f;
        private const float BossAttackWindup = 0.92f;
        private const float BossAttackCooldown = 1.65f;
        private const float BossSpeed = 1.12f;
        private const float PlayerMaxHealth = 100f;
        private const float MeleeEnemyDamage = 26f;
        private const float RangedEnemyDamage = 18f;
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
        private const string SaveKeySkillStock = "fourfold.d020.skill.lumen_edge.stock";
        private const string SaveKeyEquippedSkill = "fourfold.d020.skill.equipped";
        private const string SaveKeyLostSkillCount = "fourfold.d020.skill.lost_count";
        private const int SkillNone = 0;
        private const int SkillLumenEdge = 1;
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
        private bool previousShortcutLoaded;
        private bool previousRewardLoaded;
        private bool previousSecondNodeLoaded;
        private bool previousSecondRewardLoaded;
        private bool previousReturnedToHubLoaded;
        private int skillStock;
        private int equippedSkill;
        private int lostSkillCount;
        private int clearCount;
        private bool skillAwardedThisRun;
        private bool failureLossApplied;
        private bool lostSkillThisRun;
        private bool returnedToHubThisRun;
        private bool returnRegisteredThisRun;
        private FourfoldProgressData progressData;
        private GameObject attackRead;
        private GameObject rewardClaimRead;
        private GameObject secondRewardClaimRead;
        private GameObject returnGateClaimRead;
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
            EnsureExplorationReferences();
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
                enemyHealth[i] = InitialEnemyHealth(i);
                enemyAttackTimer[i] = InitialEnemyAttackDelay(i);
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
            var dt = Time.deltaTime;
            attackTimer = Mathf.Max(0f, attackTimer - dt);
            attackReadTimer = Mathf.Max(0f, attackReadTimer - dt);
            dodgeTimer = Mathf.Max(0f, dodgeTimer - dt);
            dodgeCooldownTimer = Mathf.Max(0f, dodgeCooldownTimer - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);

            if (Pressed(retryKey, gamepadRetryKey))
            {
                ResetRun();
                return;
            }

            if (runFailed || runCleared)
            {
                UpdateAttackRead();
                UpdateEnemyAttackReads();
                UpdateRewardState();
                UpdateReturnState();
                UpdateTraversalReads();
                if (runCleared && Pressed(interactKey, gamepadInteractKey))
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
                var attackRange = EnemyAttackRangeFor(i);
                var attackWindup = EnemyAttackWindupFor(i);
                var attackCooldown = EnemyAttackCooldownFor(i);

                if (toPlayer.magnitude <= attackRange && enemyAttackTimer[i] <= 0f)
                {
                    enemyWindupTimer[i] += dt;
                    if (enemyWindupTimer[i] >= attackWindup)
                    {
                        ResolveEnemyAttack(i, enemy, toPlayer);
                        enemyWindupTimer[i] = 0f;
                        enemyAttackTimer[i] = attackCooldown;
                    }
                    continue;
                }

                enemyWindupTimer[i] = 0f;
                var flank = new Vector3(-desired.z, 0f, desired.x) * Mathf.Sin(Time.time * (0.8f + i * 0.35f)) * 0.28f;
                enemy.position += (desired + flank).normalized * EnemySpeedFor(i) * dt;
            }
        }

        private void ResolveEnemyAttack(int index, Transform enemy, Vector3 toPlayer)
        {
            if (playerInvulnerableTimer > 0f || dodgeTimer > 0f || runFailed || runCleared)
            {
                return;
            }

            var distance = toPlayer.magnitude;
            if (distance > EnemyAttackRangeFor(index) + 0.28f)
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
                runFailed = true;
                ApplyFailureSkillLoss();
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
            var skillBoost = EquippedLumenEdge() ? 0.22f : 0f;
            var pulse = 1.16f + skillBoost + Mathf.Sin(Time.time * 36f) * 0.08f;
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
                read.transform.position = enemy.position + new Vector3(0f, 0.07f, 0f);
                var minRadius = IsBossEnemy(i) ? 1.25f : 0.72f;
                var maxRadius = IsBossEnemy(i) ? 2.35f : 1.28f;
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
                rewardClaimRead.SetActive(ready && !previousRewardLoaded);
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
                secondRewardClaimRead.SetActive(secondReady && !previousSecondRewardLoaded);
                if (secondRewardClaimPoint != null)
                {
                    secondRewardClaimRead.transform.position = secondRewardClaimPoint.position + new Vector3(0f, 0.08f, 0f);
                    var pulse = 0.92f + Mathf.Sin(Time.time * 5.2f) * 0.07f;
                    secondRewardClaimRead.transform.localScale = new Vector3(pulse, 0.025f, pulse);
                }
            }

            if (secondRewardReadyRead != null)
            {
                secondRewardReadyRead.SetActive(secondReady && !previousSecondRewardLoaded);
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
            if (previousRewardLoaded || !RewardReady() || rewardClaimPoint == null)
            {
                return false;
            }

            if (Vector3.Distance(player.position, rewardClaimPoint.position) > RewardRange)
            {
                return false;
            }

            rewardClaimed = true;
            AwardSkillReward();
            previousClearLoaded = true;
            previousRewardLoaded = true;
            previousShortcutLoaded = ToolGateSolved();
            runCleared = secondToolNode == null || secondRewardClaimPoint == null || previousSecondRewardLoaded;
            PersistProgress();
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
            if (previousSecondRewardLoaded || !SecondRewardReady() || secondRewardClaimPoint == null)
            {
                return false;
            }

            if (Vector3.Distance(player.position, secondRewardClaimPoint.position) > RewardRange)
            {
                return false;
            }

            skillStock += 1;
            equippedSkill = SkillLumenEdge;
            previousSecondRewardLoaded = true;
            previousClearLoaded = true;
            rewardClaimed = true;
            runCleared = true;
            previousReturnedToHubLoaded = false;
            PersistProgress();
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
                secondRouteLockedRead.SetActive(previousRewardLoaded && !SecondToolGateSolved());
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
            skillAwardedThisRun = false;
            failureLossApplied = false;
            lostSkillThisRun = false;
            returnedToHubThisRun = false;
            returnRegisteredThisRun = false;
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
        }

        private void LoadProgress()
        {
            progressData = FourfoldProgressSave.Load();
            if (!FourfoldProgressSave.HasSaveFile())
            {
                MigrateLegacyPlayerPrefs(progressData);
            }

            previousClearLoaded = progressData.d020Cleared;
            previousShortcutLoaded = progressData.d020ShortcutOpened;
            previousRewardLoaded = progressData.d020RewardClaimed && previousClearLoaded;
            previousSecondNodeLoaded = progressData.d020SecondNodeOpened;
            previousSecondRewardLoaded = progressData.d020SecondRewardClaimed && previousClearLoaded;
            previousReturnedToHubLoaded = progressData.d020ReturnedToHub && previousClearLoaded;
            clearCount = Mathf.Max(0, progressData.d020ClearCount);
            skillStock = Mathf.Max(0, progressData.d020LumenEdgeStock);
            equippedSkill = progressData.d020EquippedSkill;
            lostSkillCount = Mathf.Max(0, progressData.d020LostSkillCount);

            if (equippedSkill != SkillNone && skillStock <= 0)
            {
                equippedSkill = SkillNone;
                PersistProgress();
            }
            else if (equippedSkill == SkillNone && skillStock > 0)
            {
                equippedSkill = SkillLumenEdge;
                PersistProgress();
            }
        }

        private void AwardSkillReward()
        {
            if (skillAwardedThisRun)
            {
                return;
            }

            skillAwardedThisRun = true;
            skillStock += 1;
            equippedSkill = SkillLumenEdge;
        }

        private void ApplyFailureSkillLoss()
        {
            if (failureLossApplied)
            {
                return;
            }

            failureLossApplied = true;
            if (equippedSkill == SkillNone)
            {
                return;
            }

            equippedSkill = SkillNone;
            skillStock = Mathf.Max(0, skillStock - 1);
            lostSkillCount += 1;
            lostSkillThisRun = true;
            PersistProgress();
        }

        private float CurrentAttackDamage(int enemyIndex)
        {
            var baseDamage = IsBossEnemy(enemyIndex) ? 30f : enemyIndex == 0 ? 34f : 42f;
            return EquippedLumenEdge() ? baseDamage + 12f : baseDamage;
        }

        private float InitialEnemyHealth(int index)
        {
            if (IsBossEnemy(index))
            {
                return 220f;
            }

            return index == 0 ? 90f : 55f;
        }

        private float InitialEnemyAttackDelay(int index)
        {
            return IsBossEnemy(index) ? 0.9f : 0.28f + index * 0.25f;
        }

        private float EnemyAttackRangeFor(int index)
        {
            return IsBossEnemy(index) ? BossAttackRange : EnemyAttackRange;
        }

        private float EnemyAttackWindupFor(int index)
        {
            return IsBossEnemy(index) ? BossAttackWindup : EnemyAttackWindup;
        }

        private float EnemyAttackCooldownFor(int index)
        {
            return IsBossEnemy(index) ? BossAttackCooldown : EnemyAttackCooldown + index * 0.22f;
        }

        private float EnemySpeedFor(int index)
        {
            return IsBossEnemy(index) ? BossSpeed : EnemySpeed;
        }

        private float EnemyDamageFor(int index)
        {
            if (IsBossEnemy(index))
            {
                return BossEnemyDamage;
            }

            return index == 0 ? MeleeEnemyDamage : RangedEnemyDamage;
        }

        private bool IsBossEnemy(int index)
        {
            var enemy = enemies != null && index >= 0 && index < enemies.Length ? enemies[index] : null;
            return enemy != null && enemy.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool EquippedLumenEdge()
        {
            return equippedSkill == SkillLumenEdge && skillStock > 0;
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
                    ? $"  Boss {Mathf.CeilToInt(enemyHealth[i])}"
                    : "  Boss down";
            }

            return string.Empty;
        }

        private bool RewardReady()
        {
            return AllEnemiesDefeated() && ToolGateSolved() && !previousRewardLoaded && !rewardClaimed;
        }

        private bool SecondRewardReady()
        {
            return AllEnemiesDefeated() && previousRewardLoaded && SecondToolGateSolved() && !previousSecondRewardLoaded;
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
            data.d020LumenEdgeStock = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeySkillStock, 0));
            data.d020EquippedSkill = PlayerPrefs.GetInt(SaveKeyEquippedSkill, SkillNone);
            data.d020LostSkillCount = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeyLostSkillCount, 0));
        }

        private void PersistProgress()
        {
            if (progressData == null)
            {
                progressData = new FourfoldProgressData();
            }

            progressData.currentScene = "scene.d020_vertical_slice";
            progressData.d020Cleared = previousClearLoaded || runCleared;
            progressData.d020ShortcutOpened = previousShortcutLoaded;
            progressData.d020RewardClaimed = previousRewardLoaded || rewardClaimed;
            progressData.d020SecondNodeOpened = previousSecondNodeLoaded;
            progressData.d020SecondRewardClaimed = previousSecondRewardLoaded;
            progressData.d020ReturnedToHub = previousReturnedToHubLoaded;
            progressData.d020ClearCount = Mathf.Max(0, clearCount);
            progressData.d020LumenEdgeStock = Mathf.Max(0, skillStock);
            progressData.d020EquippedSkill = equippedSkill;
            progressData.d020LostSkillCount = Mathf.Max(0, lostSkillCount);
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
            PlayerPrefs.SetInt(SaveKeySkillStock, progressData.d020LumenEdgeStock);
            PlayerPrefs.SetInt(SaveKeyEquippedSkill, progressData.d020EquippedSkill);
            PlayerPrefs.SetInt(SaveKeyLostSkillCount, progressData.d020LostSkillCount);
            PlayerPrefs.Save();
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
            var rect = new Rect(16f, 16f, width, 182f);
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
                    ? lostSkillThisRun
                        ? "FAILED: equipped skill shattered. Press R to retry."
                        : "FAILED: press R to retry."
                    : !ToolGateSolved()
                        ? "Activate the glowing tool node with Q or gamepad X."
                        : !AllEnemiesDefeated()
                        ? "Defeat the enemies, then claim the relic."
                        : !previousRewardLoaded
                        ? "Claim the first relic chest with E."
                        : !SecondToolGateSolved()
                        ? "Use the same tool on the second node."
                        : !previousSecondRewardLoaded
                        ? "Claim the second relic with E."
                        : "Rewards secured. Press R to replay.";

            var toolState = explorationTool == null
                ? "Tool --"
                : explorationTool.IsReady
                    ? "Tool READY"
                    : $"Tool cooldown {Mathf.CeilToInt(explorationTool.Cooldown01 * 100f)}%";
            var skillState = EquippedLumenEdge()
                ? $"Skill Lumen Edge equipped  Stock {skillStock}"
                : $"Skill empty  Stock {skillStock}";
            var resultState = clearCount > 0
                ? $"Clears returned {clearCount}"
                : "Clear the boss, claim both relics, return to hub";

            GUI.Label(new Rect(30f, 26f, width - 28f, 34f), $"HP {Mathf.CeilToInt(playerHealth)} / {Mathf.CeilToInt(PlayerMaxHealth)}{BossHealthSuffix()}", style);
            GUI.Label(new Rect(30f, 58f, width - 28f, 30f), toolState, style);
            GUI.Label(new Rect(30f, 88f, width - 28f, 30f), skillState, style);
            GUI.Label(new Rect(30f, 118f, width - 28f, 52f), objective, style);
            GUI.Label(new Rect(30f, 164f, width - 28f, 34f), $"{resultState}  Move WASD/Stick  Attack Space/A  Dodge Shift/B  Tool Q/X  Interact E/Y", style);

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
