using UnityEngine;

namespace FourfoldEchoes.BuilderPrototype
{
    public sealed class BuilderPrototypeSpineController : MonoBehaviour
    {
        public const string SceneContractText = "PR-01 spine only: traversal, camera, HUD, and reserved subsystem hooks.";
        public const string ControlPromptText = "Move LS/WASD | Build X/B | Combat Y/C | Loot LB/L | Extract RB/E | Reset Start/R";
        public const string BuildHookPromptText = "Build hook reserved for PR-02; no placement or deletion ships in PR-01.";
        public const string CombatHookPromptText = "Combat hook reserved for PR-03; no attacks, enemies, or damage ship in PR-01.";
        public const string LootHookPromptText = "Loot hook reserved for PR-04; no drops or affixes ship in PR-01.";
        public const string ExtractHookPromptText = "Extract hook reserved for PR-05; no extraction reward flow ships in PR-01.";

        [Header("Scene")]
        public Transform player;
        public Camera followCamera;
        public Transform buildHookAnchor;
        public Transform combatHookAnchor;
        public Transform lootHookAnchor;
        public Transform extractHookAnchor;

        [Header("Movement")]
        public float moveSpeed = 5.2f;
        public Vector2 xBounds = new Vector2(-6.4f, 6.4f);
        public Vector2 zBounds = new Vector2(-4.4f, 4.4f);

        [Header("Camera")]
        public Vector3 cameraOffset = new Vector3(0f, 8.4f, -7.2f);
        public float cameraFollowSharpness = 12f;

        [Header("Prototype HUD")]
        public bool showDebugHud = true;

        private readonly BuilderPrototypeRunState runState = new BuilderPrototypeRunState();
        private Vector3 facing = Vector3.forward;
        private Vector3 startPosition;

        public BuilderPrototypeMode CurrentMode => runState.Mode;
        public int CarriedLootValue => runState.CarriedLootValue;
        public int DangerTier => runState.DangerTier;
        public int BankedLootValue => runState.BankedLootValue;
        public bool HasRequiredHookAnchors => buildHookAnchor != null && combatHookAnchor != null && lootHookAnchor != null && extractHookAnchor != null;

        public void Awake()
        {
            if (player != null)
            {
                startPosition = player.position;
            }

            SnapCameraToPlayer();
        }

        public void Update()
        {
            HandleModeInput();
            HandlePrototypeStateInput();
            MovePlayer(Time.deltaTime);
            FollowPlayer(Time.deltaTime);
        }

        public void ResetPrototypeRun()
        {
            runState.ResetRun();
            if (player != null)
            {
                player.position = startPosition;
            }

            SnapCameraToPlayer();
        }

        public static Vector3 ClampToArena(Vector3 position, Vector2 xRange, Vector2 zRange)
        {
            return new Vector3(
                Mathf.Clamp(position.x, xRange.x, xRange.y),
                position.y,
                Mathf.Clamp(position.z, zRange.x, zRange.y));
        }

        public static string PromptFor(BuilderPrototypeMode mode)
        {
            switch (mode)
            {
                case BuilderPrototypeMode.BuildHook:
                    return BuildHookPromptText;
                case BuilderPrototypeMode.CombatHook:
                    return CombatHookPromptText;
                case BuilderPrototypeMode.LootHook:
                    return LootHookPromptText;
                case BuilderPrototypeMode.ExtractHook:
                    return ExtractHookPromptText;
                default:
                    return ControlPromptText;
            }
        }

        private void HandleModeInput()
        {
            if (Pressed(KeyCode.B, KeyCode.JoystickButton2))
            {
                runState.SetMode(BuilderPrototypeMode.BuildHook);
            }
            else if (Pressed(KeyCode.C, KeyCode.JoystickButton3))
            {
                runState.SetMode(BuilderPrototypeMode.CombatHook);
            }
            else if (Pressed(KeyCode.L, KeyCode.JoystickButton4))
            {
                runState.SetMode(BuilderPrototypeMode.LootHook);
            }
            else if (Pressed(KeyCode.E, KeyCode.JoystickButton5))
            {
                runState.SetMode(BuilderPrototypeMode.ExtractHook);
            }
            else if (Pressed(KeyCode.Tab, KeyCode.JoystickButton1))
            {
                runState.SetMode(BuilderPrototypeMode.Traverse);
            }
        }

        private void HandlePrototypeStateInput()
        {
            if (Pressed(KeyCode.R, KeyCode.JoystickButton7))
            {
                ResetPrototypeRun();
            }
        }

        private void MovePlayer(float deltaTime)
        {
            if (player == null)
            {
                return;
            }

            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (input.sqrMagnitude > 0.001f)
            {
                facing = input.normalized;
                player.rotation = Quaternion.LookRotation(facing, Vector3.up);
            }

            player.position = ClampToArena(player.position + input * (moveSpeed * deltaTime), xBounds, zBounds);
        }

        private void FollowPlayer(float deltaTime)
        {
            if (player == null || followCamera == null)
            {
                return;
            }

            var target = player.position + cameraOffset;
            var t = 1f - Mathf.Exp(-cameraFollowSharpness * deltaTime);
            followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, target, t);
            followCamera.transform.rotation = Quaternion.LookRotation(player.position - followCamera.transform.position, Vector3.up);
        }

        private void SnapCameraToPlayer()
        {
            if (player == null || followCamera == null)
            {
                return;
            }

            followCamera.transform.position = player.position + cameraOffset;
            followCamera.transform.rotation = Quaternion.LookRotation(player.position - followCamera.transform.position, Vector3.up);
        }

        private static bool Pressed(KeyCode keyboard, KeyCode gamepad)
        {
            return Input.GetKeyDown(keyboard) || Input.GetKeyDown(gamepad);
        }

        private void OnGUI()
        {
            if (!showDebugHud)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 390f, 136f), GUI.skin.box);
            GUILayout.Label(SceneContractText);
            GUILayout.Label("Mode: " + BuilderPrototypeRunState.LabelFor(runState.Mode));
            GUILayout.Label("Carried Loot Value: " + runState.CarriedLootValue);
            GUILayout.Label("Danger Tier: " + runState.DangerTier + "/" + BuilderPrototypeRunState.MaxDangerTier);
            GUILayout.Label(PromptFor(runState.Mode));
            GUILayout.EndArea();
        }
    }
}
