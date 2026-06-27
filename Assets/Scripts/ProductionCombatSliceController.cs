using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class ProductionCombatSliceController : MonoBehaviour
    {
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

        private const float PlayerMaxHealth = 100f;
        private const float AttackCooldownSeconds = 0.22f;
        private const float HostileAttackCooldownSeconds = 1.05f;
        private const float PlayerInvulnerableSeconds = 0.4f;
        private const float RoomMinX = -6.35f;
        private const float RoomMaxX = 6.35f;
        private const float RoomMinZ = -4.65f;
        private const float RoomMaxZ = 4.45f;

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
        private string lastEvent = "Production slice ready";

        public int HostileCount => (enemies == null ? 0 : enemies.Length) + (boss == null ? 0 : 1);
        public bool BossUnlocked => bossUnlocked;
        public bool GateOpen => gateOpen;
        public bool RewardClaimed => rewardClaimed;

        private void Awake()
        {
            CacheStarts();
            ResetSlice();
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            attackCooldown = Mathf.Max(0f, attackCooldown - dt);
            playerInvulnerableTimer = Mathf.Max(0f, playerInvulnerableTimer - dt);

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetSlice();
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
            ApplyPresentation(dt);
        }

        public void ResetSlice()
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
            lastEvent = playerHealth <= 0f ? "Hero down - reset with R" : "Hit taken";
        }

        private void UpdateProgressState()
        {
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
        }

        private void HandleRewardClaim()
        {
            if (!gateOpen || rewardClaimed || player == null || rewardChest == null)
            {
                return;
            }

            var distance = Vector3.Distance(player.position, rewardChest.transform.position);
            if (distance <= 1.65f && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)))
            {
                rewardClaimed = true;
                lastEvent = "Reward claimed";
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
                gateClaimBadge.Rotate(Vector3.up, 44f * Time.deltaTime, Space.World);
            }

            if (rewardPad != null)
            {
                rewardPad.SetActive(gateOpen);
                ApplyFirstRendererMaterial(rewardPad.transform, gateOpen ? rewardMaterial : null);
            }

            if (rewardChest != null && rewardClaimed)
            {
                rewardChest.transform.localScale = Vector3.Lerp(rewardChest.transform.localScale, Vector3.one * 1.16f, Mathf.Clamp01(dt * 8f));
            }
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

        private void OnGUI()
        {
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
