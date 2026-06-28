using System;
using System.IO;
using FourfoldEchoes.Spike;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public enum ProductionCombatRunState
    {
        Title,
        Playing,
        Paused,
        PlayerDown,
        Completed
    }

    public sealed class ProductionCombatSliceController : MonoBehaviour
    {
        public const KeyCode RewardClaimKeyboardKey = KeyCode.E;
        public const KeyCode RewardClaimControllerButton = KeyCode.JoystickButton3;
        public const int RewardClaimMouseButton = 1;

        public static Func<LocalSaveService> SaveServiceFactory { get; set; } = LocalSaveService.CreateDefault;

        [Header("Scene")]
        public Transform player;
        public Transform[] enemies;
        public Transform boss;
        public ExplorationTool explorationTool;
        public ExplorationNode shortcutNode;
        public Transform gateLeft;
        public Transform gateRight;
        public Transform gateClaimBadge;
        public GameObject rewardChest;
        public GameObject rewardPad;
        public GameObject rewardIdleRead;
        public GameObject rewardClaimRead;
        public Camera fixedCamera;

        [Header("Materials")]
        public Material downMaterial;
        public Material gateClosedMaterial;
        public Material gateReadyMaterial;
        public Material gateOpenMaterial;
        public Material rewardMaterial;

        [Header("Tuning")]
        public float moveSpeed = 5.2f;
        public float attackRange = 1.85f;
        public float attackDamage = 34f;
        public float enemyMaxHealth = 78f;
        public float bossMaxHealth = 230f;
        public float enemySenseRange = 6.8f;
        public float bossSenseRange = 8.2f;
        public float enemyMoveSpeed = 1.35f;
        public float bossMoveSpeed = 0.82f;
        public float hostileStrikeRange = 1.05f;
        public float hostileDamage = 8f;
        public float bossDamage = 16f;

        [Header("Debug")]
        public bool showDebugOverlay;

        [Header("Save")]
        public bool useLocalSave = true;

        private const float PlayerMaxHealth = 100f;
        private const float AttackCooldownSeconds = 0.22f;
        private const float HostileAttackCooldownSeconds = 1.05f;
        private const float PlayerInvulnerableSeconds = 0.4f;
        private const float RoomMinX = -6.35f;
        private const float RoomMaxX = 6.35f;
        private const float RoomMinZ = -4.65f;
        private const float RoomMaxZ = 4.45f;
        private const string LocalSaveReadyStatus = "Local save ready";
        private const string AutosaveOffStatus = "Autosave off";
        private const string ProgressRestoredStatus = "Progress restored";
        private const string ProgressSavedStatus = "Progress saved";
        private const string SaveFailedStatus = "Save failed - progress kept";
        private const string SavedProgressRestoredEvent = "Saved progress restored";
        private const string SavedRewardRestoredEvent = "Saved reward restored";

        private float[] health;
        private float[] strikeCooldowns;
        private Vector3[] startPositions;
        private Vector3[] startScales;
        private Quaternion gateLeftClosedRotation;
        private Quaternion gateRightClosedRotation;
        private Vector3 playerStartPosition;
        private Vector3 facing = Vector3.right;
        private float playerHealth = PlayerMaxHealth;
        private float attackCooldown;
        private float playerInvulnerableTimer;
        private bool bossUnlocked;
        private bool gateOpen;
        private bool rewardClaimed;
        private ProductionCombatRunState runState = ProductionCombatRunState.Title;
        private ProductionCombatSliceProgressSnapshot lastSavedProgress;
        private LocalSaveService localSaveService;
        private FourfoldProofAudio proofAudio;
        private string lastEvent = "Production slice ready";
        private string saveStatus = LocalSaveReadyStatus;

        public int HostileCount => (enemies == null ? 0 : enemies.Length) + (boss == null ? 0 : 1);
        public ProductionCombatRunState State => runState;
        public bool BossUnlocked => bossUnlocked;
        public bool GateOpen => gateOpen;
        public bool RewardClaimed => rewardClaimed;
        public bool ShortcutOpen => shortcutNode != null && shortcutNode.IsSolved;
        public string LastEvent => lastEvent;
        public string SaveStatus => saveStatus;
        public float PlayerHealth01 => Mathf.Clamp01(playerHealth / PlayerMaxHealth);
        public float WardensHealth01 => LivingMinor01();
        public float BossHealth01 => Boss01();
        public float ToolReady01 => explorationTool == null ? 1f : 1f - explorationTool.Cooldown01;
        public bool CanPause => runState == ProductionCombatRunState.Playing || runState == ProductionCombatRunState.Paused;

        private void Awake()
        {
            proofAudio = GetComponent<FourfoldProofAudio>();
            CacheStarts();
            ResetSliceCore();
            LoadProgressIfEnabled();
            SetRunState(ProductionCombatRunState.Title);
            EnsureRuntimeUi();
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            if (runState == ProductionCombatRunState.Title || runState == ProductionCombatRunState.Paused)
            {
                ApplyPresentation(dt);
                return;
            }

            if (runState == ProductionCombatRunState.PlayerDown || runState == ProductionCombatRunState.Completed)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RetryRun();
                    return;
                }

                ApplyPresentation(dt);
                return;
            }

            attackCooldown = Mathf.Max(0f, attackCooldown - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);

            if (Input.GetKeyDown(KeyCode.R))
            {
                RetryRun();
                return;
            }

            if (playerHealth > 0f)
            {
                MovePlayer(dt);
                if (AttackPressed() && attackCooldown <= 0f)
                {
                    ResolvePlayerAttack();
                }
            }

            UpdateProgressState();
            UpdateHostiles(dt);
            HandleRewardClaim();
            SaveProgressIfChanged();
            ApplyPresentation(dt);
        }

        public void ResetSlice()
        {
            ResetSliceCore();
        }

        public void BeginRun()
        {
            ResetSliceCore();
            LoadProgressIfEnabled();
            SetRunState(rewardClaimed ? ProductionCombatRunState.Completed : ProductionCombatRunState.Playing);
            PlayAudioCue(FourfoldProofAudioCue.PhaseAccent, 0.18f);
        }

        public void RetryRun()
        {
            ResetSliceCore();
            LoadProgressIfEnabled();
            SetRunState(rewardClaimed ? ProductionCombatRunState.Completed : ProductionCombatRunState.Playing);
            lastEvent = rewardClaimed ? "Saved reward already claimed" : "Retry started";
            PlayAudioCue(FourfoldProofAudioCue.PhaseAccent, 0.16f);
        }

        public void ReturnToTitle()
        {
            ResetSliceCore();
            SetRunState(ProductionCombatRunState.Title);
            LoadProgressIfEnabled();
            PlayAudioCue(FourfoldProofAudioCue.PhaseAccent, 0.12f);
        }

        public void SetPaused(bool paused)
        {
            if (paused && runState == ProductionCombatRunState.Playing)
            {
                SetRunState(ProductionCombatRunState.Paused);
                lastEvent = "Paused";
                PlayAudioCue(FourfoldProofAudioCue.PhaseAccent, 0.12f);
            }
            else if (!paused && runState == ProductionCombatRunState.Paused)
            {
                SetRunState(ProductionCombatRunState.Playing);
                lastEvent = "Resumed";
                PlayAudioCue(FourfoldProofAudioCue.PhaseAccent, 0.14f);
            }
        }

        public void TogglePause()
        {
            if (runState == ProductionCombatRunState.Playing)
            {
                SetPaused(true);
            }
            else if (runState == ProductionCombatRunState.Paused)
            {
                SetPaused(false);
            }
        }

        public void ConfigureSaveService(LocalSaveService service)
        {
            localSaveService = service;
        }

        public void ApplySavedProgress(FourfoldSaveData data)
        {
            var snapshot = ProductionCombatSliceProgress.Read(data);
            ApplyProgressSnapshot(snapshot);
            lastSavedProgress = CaptureProgressSnapshot();
            if (HasAnyProgress(lastSavedProgress))
            {
                saveStatus = ProgressRestoredStatus;
                lastEvent = lastSavedProgress.RewardClaimed ? SavedRewardRestoredEvent : SavedProgressRestoredEvent;
            }
            else
            {
                saveStatus = LocalSaveReadyStatus;
            }
        }

        public void WriteSavedProgress(FourfoldSaveData data)
        {
            ProductionCombatSliceProgress.Write(data, CaptureProgressSnapshot());
        }

        public bool ClearMinorWardens()
        {
            if (health == null)
            {
                return false;
            }

            var enemyCount = enemies == null ? 0 : enemies.Length;
            if (enemyCount == 0)
            {
                return false;
            }

            for (var index = 0; index < enemyCount && index < health.Length; index++)
            {
                health[index] = 0f;
            }

            UpdateProgressState();
            lastEvent = bossUnlocked ? "Boss unlocked" : "Wardens cleared";
            SaveProgressIfChanged();
            ApplyPresentation(999f);
            return true;
        }

        public bool ClearBossGate()
        {
            UpdateProgressState();
            if (!bossUnlocked || boss == null || health == null)
            {
                return false;
            }

            var bossIndex = enemies == null ? 0 : enemies.Length;
            if (bossIndex < 0 || bossIndex >= health.Length)
            {
                return false;
            }

            health[bossIndex] = 0f;
            UpdateProgressState();
            lastEvent = "Boss core broken";
            SaveProgressIfChanged();
            ApplyPresentation(999f);
            return gateOpen;
        }

        public bool ClaimReward()
        {
            if (!gateOpen || rewardClaimed)
            {
                return false;
            }

            rewardClaimed = true;
            lastEvent = "Reward claimed";
            SetRunState(ProductionCombatRunState.Completed);
            PlayAudioCue(FourfoldProofAudioCue.Reward, 0.42f);
            SaveProgressIfChanged();
            ApplyPresentation(999f);
            return true;
        }

        private void ResetSliceCore()
        {
            var count = HostileCount;
            health = new float[count];
            strikeCooldowns = new float[count];

            for (var index = 0; index < count; index++)
            {
                health[index] = IsBossIndex(index) ? bossMaxHealth : enemyMaxHealth;
                var hostile = GetHostile(index);
                if (hostile != null && startPositions != null && index < startPositions.Length)
                {
                    hostile.position = startPositions[index];
                    if (startScales != null && index < startScales.Length)
                    {
                        hostile.localScale = startScales[index];
                    }
                    hostile.gameObject.SetActive(true);
                }
            }

            if (player != null)
            {
                player.position = playerStartPosition;
            }

            if (shortcutNode != null)
            {
                shortcutNode.ResetNode();
            }

            playerHealth = PlayerMaxHealth;
            attackCooldown = 0f;
            playerInvulnerableTimer = 0f;
            bossUnlocked = false;
            gateOpen = false;
            rewardClaimed = false;
            lastSavedProgress = CaptureProgressSnapshot();
            saveStatus = useLocalSave ? LocalSaveReadyStatus : AutosaveOffStatus;
            lastEvent = "Defeat the two wardens, reveal the shortcut, then break the boss gate";
            ApplyPresentation(999f);
        }

        private void CacheStarts()
        {
            playerStartPosition = player != null ? player.position : transform.position;
            var count = HostileCount;
            startPositions = new Vector3[count];
            startScales = new Vector3[count];
            for (var index = 0; index < count; index++)
            {
                var hostile = GetHostile(index);
                startPositions[index] = hostile != null ? hostile.position : Vector3.zero;
                startScales[index] = hostile != null ? hostile.localScale : Vector3.one;
            }

            gateLeftClosedRotation = gateLeft != null ? gateLeft.rotation : Quaternion.identity;
            gateRightClosedRotation = gateRight != null ? gateRight.rotation : Quaternion.identity;
        }

        private void MovePlayer(float dt)
        {
            if (player == null)
            {
                return;
            }

            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);
            if (input.sqrMagnitude > 0.001f)
            {
                facing = input.normalized;
                player.position += input * moveSpeed * dt;
                player.rotation = Quaternion.Slerp(player.rotation, Quaternion.LookRotation(facing, Vector3.up), dt * 14f);
            }

            player.position = new Vector3(
                Mathf.Clamp(player.position.x, RoomMinX, RoomMaxX),
                player.position.y,
                Mathf.Clamp(player.position.z, RoomMinZ, RoomMaxZ));
        }

        private void ResolvePlayerAttack()
        {
            attackCooldown = AttackCooldownSeconds;
            PlayAudioCue(FourfoldProofAudioCue.Attack, 0.24f);
            var targetIndex = FindAttackTarget();
            if (targetIndex < 0)
            {
                lastEvent = "Attack whiffed";
                return;
            }

            health[targetIndex] = Mathf.Max(0f, health[targetIndex] - attackDamage);
            var target = GetHostile(targetIndex);
            if (target != null)
            {
                var push = target.position - player.position;
                push.y = 0f;
                if (push.sqrMagnitude > 0.001f)
                {
                    target.position += push.normalized * 0.22f;
                }
            }

            lastEvent = health[targetIndex] <= 0f
                ? (IsBossIndex(targetIndex) ? "Boss core broken" : "Warden down")
                : (IsBossIndex(targetIndex) ? "Boss staggered" : "Warden hit");
            PlayAudioCue(FourfoldProofAudioCue.Hit, IsBossIndex(targetIndex) ? 0.34f : 0.28f);
        }

        private int FindAttackTarget()
        {
            var bestIndex = -1;
            var bestDistance = float.PositiveInfinity;
            for (var index = 0; index < HostileCount; index++)
            {
                if (!IsAlive(index) || IsLockedBoss(index))
                {
                    continue;
                }

                var hostile = GetHostile(index);
                if (hostile == null)
                {
                    continue;
                }

                var toTarget = hostile.position - player.position;
                toTarget.y = 0f;
                var distance = toTarget.magnitude;
                if (distance > attackRange)
                {
                    continue;
                }

                if (distance > 0.55f && Vector3.Dot(facing, toTarget.normalized) < -0.1f)
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private void UpdateHostiles(float dt)
        {
            if (player == null || playerHealth <= 0f)
            {
                return;
            }

            for (var index = 0; index < HostileCount; index++)
            {
                strikeCooldowns[index] = Mathf.Max(0f, strikeCooldowns[index] - dt);
                if (!IsAlive(index) || IsLockedBoss(index))
                {
                    continue;
                }

                var hostile = GetHostile(index);
                if (hostile == null)
                {
                    continue;
                }

                var toPlayer = player.position - hostile.position;
                toPlayer.y = 0f;
                var distance = toPlayer.magnitude;
                var senseRange = IsBossIndex(index) ? bossSenseRange : enemySenseRange;
                if (distance <= senseRange && distance > hostileStrikeRange)
                {
                    var speed = IsBossIndex(index) ? bossMoveSpeed : enemyMoveSpeed;
                    hostile.position += toPlayer.normalized * speed * dt;
                    hostile.rotation = Quaternion.Slerp(hostile.rotation, Quaternion.LookRotation(toPlayer.normalized, Vector3.up), dt * 8f);
                }

                if (distance <= hostileStrikeRange && strikeCooldowns[index] <= 0f)
                {
                    strikeCooldowns[index] = HostileAttackCooldownSeconds;
                    DamagePlayer(IsBossIndex(index) ? bossDamage : hostileDamage);
                }
            }
        }

        private void DamagePlayer(float damage)
        {
            if (playerInvulnerableTimer > 0f)
            {
                return;
            }

            playerHealth = Mathf.Max(0f, playerHealth - damage);
            playerInvulnerableTimer = PlayerInvulnerableSeconds;
            lastEvent = BuildDamageEventText(PlayerHealth01);
            if (playerHealth <= 0f)
            {
                SetRunState(ProductionCombatRunState.PlayerDown);
            }

            PlayAudioCue(FourfoldProofAudioCue.PlayerHit, 0.32f);
        }

        public static string BuildDamageEventText(float playerHealth01)
        {
            if (playerHealth01 <= 0f)
            {
                return "Hero down - choose Retry";
            }

            if (ProductionCombatLowHealthWarning.IsCriticalHealth(playerHealth01))
            {
                return "Critical hit - dodge now";
            }

            return ProductionCombatLowHealthWarning.IsWarningHealth(playerHealth01)
                ? "Hit taken - create space"
                : "Hit taken - dodge next tell";
        }

        private void UpdateProgressState()
        {
            var wasBossUnlocked = bossUnlocked;
            var wasGateOpen = gateOpen;
            var minorCleared = true;
            var enemyCount = enemies == null ? 0 : enemies.Length;
            for (var index = 0; index < enemyCount; index++)
            {
                if (IsAlive(index))
                {
                    minorCleared = false;
                    break;
                }
            }

            var shortcutSolved = shortcutNode == null || shortcutNode.IsSolved;
            bossUnlocked = minorCleared && shortcutSolved;
            if (bossUnlocked && boss != null && boss.gameObject.activeSelf)
            {
                lastEvent = lastEvent == "Warden down" ? "Boss unlocked" : lastEvent;
            }

            var bossIndex = enemyCount;
            gateOpen = boss == null || (bossUnlocked && !IsAlive(bossIndex));
            if (!wasBossUnlocked && bossUnlocked)
            {
                PlayAudioCue(FourfoldProofAudioCue.RoomClear, 0.28f);
            }

            if (!wasGateOpen && gateOpen)
            {
                PlayAudioCue(FourfoldProofAudioCue.GateOpen, 0.38f);
            }
        }

        private void HandleRewardClaim()
        {
            if (!gateOpen || rewardClaimed || player == null || rewardChest == null)
            {
                return;
            }

            var distance = Vector3.Distance(player.position, rewardChest.transform.position);
            if (distance <= 1.65f && InteractPressed())
            {
                ClaimReward();
            }
        }

        private void ApplyPresentation(float dt)
        {
            for (var index = 0; index < HostileCount; index++)
            {
                var hostile = GetHostile(index);
                if (hostile == null)
                {
                    continue;
                }

                var alive = IsAlive(index);
                hostile.gameObject.SetActive(alive || health[index] <= 0f);
                if (!alive)
                {
                    ApplyFirstRendererMaterial(hostile, downMaterial);
                    hostile.localScale = Vector3.Lerp(hostile.localScale, Vector3.one * 0.82f, Mathf.Clamp01(dt * 8f));
                }
            }

            var gateTarget = gateOpen
                ? gateOpenMaterial
                : bossUnlocked
                    ? gateReadyMaterial
                    : gateClosedMaterial;
            ApplyFirstRendererMaterial(gateLeft, gateTarget);
            ApplyFirstRendererMaterial(gateRight, gateTarget);

            if (gateLeft != null)
            {
                var target = gateOpen ? gateLeftClosedRotation * Quaternion.Euler(0f, -62f, 0f) : gateLeftClosedRotation;
                gateLeft.rotation = Quaternion.Slerp(gateLeft.rotation, target, Mathf.Clamp01(dt * 5f));
            }

            if (gateRight != null)
            {
                var target = gateOpen ? gateRightClosedRotation * Quaternion.Euler(0f, 62f, 0f) : gateRightClosedRotation;
                gateRight.rotation = Quaternion.Slerp(gateRight.rotation, target, Mathf.Clamp01(dt * 5f));
            }

            if (gateClaimBadge != null)
            {
                gateClaimBadge.gameObject.SetActive(bossUnlocked);
                gateClaimBadge.Rotate(Vector3.up, 44f * dt, Space.World);
            }

            if (rewardPad != null)
            {
                rewardPad.SetActive(gateOpen);
                ApplyFirstRendererMaterial(rewardPad.transform, gateOpen ? rewardMaterial : null);
            }

            if (rewardIdleRead != null)
            {
                rewardIdleRead.SetActive(ShouldShowRewardPickupRead(gateOpen, rewardClaimed, false));
            }

            if (rewardClaimRead != null)
            {
                rewardClaimRead.SetActive(ShouldShowRewardPickupRead(gateOpen, rewardClaimed, true));
            }

            if (rewardChest != null && rewardClaimed)
            {
                rewardChest.transform.localScale = Vector3.Lerp(rewardChest.transform.localScale, Vector3.one * 1.16f, Mathf.Clamp01(dt * 8f));
            }
        }

        internal static bool ShouldShowRewardPickupRead(bool gateOpen, bool rewardClaimed, bool claimRead)
        {
            return gateOpen && rewardClaimed == claimRead;
        }

        private bool IsAlive(int index)
        {
            return health != null && index >= 0 && index < health.Length && health[index] > 0f;
        }

        private bool IsLockedBoss(int index)
        {
            return IsBossIndex(index) && !bossUnlocked;
        }

        private bool IsBossIndex(int index)
        {
            var enemyCount = enemies == null ? 0 : enemies.Length;
            return boss != null && index == enemyCount;
        }

        private Transform GetHostile(int index)
        {
            var enemyCount = enemies == null ? 0 : enemies.Length;
            if (index < enemyCount)
            {
                return enemies[index];
            }

            return boss != null && index == enemyCount ? boss : null;
        }

        private static bool AttackPressed()
        {
            return Input.GetKeyDown(KeyCode.J)
                || Input.GetMouseButtonDown(0)
                || Input.GetKeyDown(KeyCode.JoystickButton0);
        }

        public static bool IsRewardClaimKey(KeyCode keyCode)
        {
            return keyCode == RewardClaimKeyboardKey || keyCode == RewardClaimControllerButton;
        }

        private static bool InteractPressed()
        {
            return Input.GetKeyDown(RewardClaimKeyboardKey)
                || Input.GetMouseButtonDown(RewardClaimMouseButton)
                || Input.GetKeyDown(RewardClaimControllerButton);
        }

        private static void ApplyFirstRendererMaterial(Transform root, Material material)
        {
            if (root == null || material == null)
            {
                return;
            }

            var renderer = root.GetComponentInChildren<Renderer>(true);
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private void SetRunState(ProductionCombatRunState state)
        {
            runState = state;
            if (explorationTool != null)
            {
                explorationTool.enabled = state == ProductionCombatRunState.Playing;
            }
        }

        private void LoadProgressIfEnabled()
        {
            if (!useLocalSave)
            {
                lastSavedProgress = CaptureProgressSnapshot();
                saveStatus = AutosaveOffStatus;
                return;
            }

            ApplySavedProgress(GetSaveService().LoadOrCreate());
        }

        private void SaveProgressIfChanged()
        {
            if (!useLocalSave)
            {
                return;
            }

            var snapshot = CaptureProgressSnapshot();
            if (HasSameProgress(snapshot, lastSavedProgress) || !TrySaveProgress(snapshot))
            {
                return;
            }

            lastSavedProgress = snapshot;
            saveStatus = ProgressSavedStatus;
        }

        private bool TrySaveProgress(ProductionCombatSliceProgressSnapshot snapshot)
        {
            try
            {
                var service = GetSaveService();
                var data = service.LoadOrCreate();
                ProductionCombatSliceProgress.Write(data, snapshot);
                service.Save(data);
                return true;
            }
            catch (IOException)
            {
                lastEvent = "Save failed - progress kept in memory";
                saveStatus = SaveFailedStatus;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                lastEvent = "Save failed - progress kept in memory";
                saveStatus = SaveFailedStatus;
                return false;
            }
            catch (ArgumentException)
            {
                lastEvent = "Save failed - progress kept in memory";
                saveStatus = SaveFailedStatus;
                return false;
            }
        }

        private LocalSaveService GetSaveService()
        {
            if (localSaveService == null)
            {
                var factory = SaveServiceFactory ?? LocalSaveService.CreateDefault;
                localSaveService = factory() ?? LocalSaveService.CreateDefault();
            }

            return localSaveService;
        }

        private ProductionCombatSliceProgressSnapshot CaptureProgressSnapshot()
        {
            return new ProductionCombatSliceProgressSnapshot(ShortcutOpen, GateOpen, RewardClaimed);
        }

        private void ApplyProgressSnapshot(ProductionCombatSliceProgressSnapshot snapshot)
        {
            if (snapshot.ShortcutOpen && shortcutNode != null)
            {
                shortcutNode.SetSolved(true);
            }

            if (snapshot.BossDefeated)
            {
                for (var index = 0; health != null && index < health.Length; index++)
                {
                    health[index] = 0f;
                }

                bossUnlocked = true;
                gateOpen = true;
            }

            if (snapshot.RewardClaimed)
            {
                rewardClaimed = true;
                if (runState == ProductionCombatRunState.Playing)
                {
                    SetRunState(ProductionCombatRunState.Completed);
                }
            }

            if (snapshot.ShortcutOpen || snapshot.BossDefeated || snapshot.RewardClaimed)
            {
                lastEvent = snapshot.RewardClaimed
                    ? "Reward already claimed"
                    : snapshot.BossDefeated
                        ? "Boss gate already open"
                        : "Shortcut already open";
            }
        }

        private static bool HasSameProgress(ProductionCombatSliceProgressSnapshot left, ProductionCombatSliceProgressSnapshot right)
        {
            return left.ShortcutOpen == right.ShortcutOpen
                && left.BossDefeated == right.BossDefeated
                && left.RewardClaimed == right.RewardClaimed;
        }

        private static bool HasAnyProgress(ProductionCombatSliceProgressSnapshot snapshot)
        {
            return snapshot.ShortcutOpen || snapshot.BossDefeated || snapshot.RewardClaimed;
        }

        private void EnsureRuntimeUi()
        {
            var ui = GetComponent<ProductionCombatSliceUi>();
            if (ui == null)
            {
                ui = gameObject.AddComponent<ProductionCombatSliceUi>();
            }

            ui.controller = this;
        }

        private void PlayAudioCue(FourfoldProofAudioCue cue, float volume)
        {
            if (proofAudio == null)
            {
                proofAudio = GetComponent<FourfoldProofAudio>();
            }

            proofAudio?.Play(cue, volume);
        }

        private void OnGUI()
        {
            if (!showDebugOverlay)
            {
                return;
            }

            var box = new Rect(20f, 20f, 360f, 158f);
            GUI.Box(box, "FOURFOLD ECHOES - Production Combat Slice");
            GUI.Label(new Rect(36f, 48f, 320f, 22f), lastEvent);
            DrawBar(new Rect(36f, 76f, 290f, 15f), playerHealth / PlayerMaxHealth, new Color(0.3f, 0.95f, 0.55f), "Hero");
            DrawBar(new Rect(36f, 98f, 290f, 15f), LivingMinor01(), new Color(0.95f, 0.45f, 0.25f), "Wardens");
            DrawBar(new Rect(36f, 120f, 290f, 15f), Boss01(), bossUnlocked ? new Color(0.85f, 0.45f, 1f) : new Color(0.3f, 0.3f, 0.35f), bossUnlocked ? "Boss" : "Boss locked");
            GUI.Label(new Rect(36f, 144f, 330f, 20f), $"Shortcut {(shortcutNode != null && shortcutNode.IsSolved ? "open" : "closed")} | Gate {(gateOpen ? "open" : "sealed")} | Reward {(rewardClaimed ? "claimed" : "waiting")}");
        }

        private float LivingMinor01()
        {
            var enemyCount = enemies == null ? 0 : enemies.Length;
            if (enemyCount == 0)
            {
                return 1f;
            }

            var total = 0f;
            for (var index = 0; index < enemyCount; index++)
            {
                total += health != null && index < health.Length ? health[index] / enemyMaxHealth : 0f;
            }

            return Mathf.Clamp01(total / enemyCount);
        }

        private float Boss01()
        {
            if (boss == null)
            {
                return 1f;
            }

            var index = enemies == null ? 0 : enemies.Length;
            return health != null && index < health.Length ? Mathf.Clamp01(health[index] / bossMaxHealth) : 0f;
        }

        private static void DrawBar(Rect rect, float value, Color color, string label)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.68f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value), rect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 6f, rect.y - 2f, rect.width - 12f, rect.height + 4f), label);
        }
    }
}
