using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.BuilderPrototype
{
    public sealed class BuilderPrototypeSpineController : MonoBehaviour
    {
        public const string SceneContractText = "PR-02 build edit loop: traversal, camera, HUD, and block placement/deletion.";
        public const string ControlPromptText = "Move LS/WASD | Build X/B | Combat Y/C | Loot LB/L | Extract RB/E | Reset Start/R";
        public const string BuildHookPromptText = "Build: move cursor LS/arrows | Place A/J | Remove X/K | Exit B/Tab";
        public const string CombatHookPromptText = "Combat hook reserved for PR-03; no attacks, enemies, or damage ship in PR-02.";
        public const string LootHookPromptText = "Loot hook reserved for PR-04; no drops or affixes ship in PR-02.";
        public const string ExtractHookPromptText = "Extract hook reserved for PR-05; no extraction reward flow ships in PR-02.";

        [Header("Scene")]
        public Transform player;
        public Camera followCamera;
        public Transform buildHookAnchor;
        public Transform combatHookAnchor;
        public Transform lootHookAnchor;
        public Transform extractHookAnchor;

        [Header("Build Edit")]
        public Transform editableBlocksRoot;
        public Material placedBlockMaterial;
        public Material buildCursorMaterial;
        public int startingBuildBlocks = 18;
        public int buildMaxStackHeight = BuilderPrototypeBuildGrid.DefaultMaxStackHeight;
        public Vector3 buildBlockScale = new Vector3(0.9f, 0.56f, 0.9f);

        private const float CursorMoveRepeatDuration = 0.18f;

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
        private readonly Dictionary<Vector2Int, Stack<GameObject>> placedBlocks = new Dictionary<Vector2Int, Stack<GameObject>>();
        private BuilderPrototypeBuildGrid buildGrid;
        private Transform buildCursor;
        private float cursorMoveRepeatTimer;
        private Vector3 facing = Vector3.forward;
        private Vector3 startPosition;
        private string lastBuildEvent = "Build loop ready";

        public BuilderPrototypeMode CurrentMode => runState.Mode;
        public int CarriedLootValue => runState.CarriedLootValue;
        public int DangerTier => runState.DangerTier;
        public int BankedLootValue => runState.BankedLootValue;
        public bool HasRequiredHookAnchors => buildHookAnchor != null && combatHookAnchor != null && lootHookAnchor != null && extractHookAnchor != null;
        public bool HasRequiredBuildReferences => editableBlocksRoot != null && placedBlockMaterial != null && buildCursorMaterial != null;
        public int BuildBlocksAvailable => buildGrid?.BlocksAvailable ?? startingBuildBlocks;
        public int PlacedBlockCount => buildGrid?.PlacedBlockCount ?? 0;
        public Vector2Int SelectedBuildCell => buildGrid?.SelectedCell ?? Vector2Int.zero;

        public void Awake()
        {
            if (player != null)
            {
                startPosition = player.position;
            }

            ResetBuildGrid();
            SnapCameraToPlayer();
        }

        public void Update()
        {
            cursorMoveRepeatTimer = Mathf.Max(0f, cursorMoveRepeatTimer - Time.deltaTime);
            HandleModeInput();
            HandlePrototypeStateInput();
            HandleBuildInput();
            MovePlayer(Time.deltaTime);
            UpdateBuildCursor();
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

        public void ResetBuildGrid()
        {
            ClearPlacedBlocks();
            buildGrid = new BuilderPrototypeBuildGrid(
                BuilderPrototypeBuildGrid.DefaultWidth,
                BuilderPrototypeBuildGrid.DefaultDepth,
                startingBuildBlocks,
                buildMaxStackHeight);
            EnsureBuildCursor();
            UpdateBuildCursor();
            lastBuildEvent = "Build loop ready";
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
                ResetBuildGrid();
            }
        }

        private void HandleBuildInput()
        {
            if (runState.Mode != BuilderPrototypeMode.BuildHook || buildGrid == null)
            {
                return;
            }

            var cursorDelta = ReadBuildCursorDelta();
            if (cursorDelta != Vector2Int.zero)
            {
                buildGrid.MoveSelection(cursorDelta.x, cursorDelta.y);
                cursorMoveRepeatTimer = CursorMoveRepeatDuration;
            }

            if (Pressed(KeyCode.J, KeyCode.JoystickButton0))
            {
                ApplyBuildResult(buildGrid.TryPlaceSelected());
            }
            else if (Pressed(KeyCode.K, KeyCode.JoystickButton2))
            {
                ApplyBuildResult(buildGrid.TryRemoveSelected());
            }
        }

        private void MovePlayer(float deltaTime)
        {
            if (player == null)
            {
                return;
            }

            if (runState.Mode == BuilderPrototypeMode.BuildHook)
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

        private Vector2Int ReadBuildCursorDelta()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                return new Vector2Int(-1, 0);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                return new Vector2Int(1, 0);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                return new Vector2Int(0, 1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                return new Vector2Int(0, -1);
            }

            if (cursorMoveRepeatTimer > 0f)
            {
                return Vector2Int.zero;
            }

            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical) && Mathf.Abs(horizontal) >= 0.55f)
            {
                return new Vector2Int(horizontal > 0f ? 1 : -1, 0);
            }

            if (Mathf.Abs(vertical) >= 0.55f)
            {
                return new Vector2Int(0, vertical > 0f ? 1 : -1);
            }

            return Vector2Int.zero;
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

        private void ApplyBuildResult(BuilderPrototypeBuildResult result)
        {
            switch (result)
            {
                case BuilderPrototypeBuildResult.Placed:
                    CreatePlacedBlock(buildGrid.SelectedCell, buildGrid.HeightAt(buildGrid.SelectedCell));
                    lastBuildEvent = $"Placed block at {FormatCell(buildGrid.SelectedCell)}";
                    break;
                case BuilderPrototypeBuildResult.Removed:
                    RemovePlacedBlock(buildGrid.SelectedCell);
                    lastBuildEvent = $"Removed block at {FormatCell(buildGrid.SelectedCell)}";
                    break;
                case BuilderPrototypeBuildResult.NoBlocksAvailable:
                    lastBuildEvent = "No blocks available";
                    break;
                case BuilderPrototypeBuildResult.NoPlacedBlock:
                    lastBuildEvent = "No placed block at cursor";
                    break;
                case BuilderPrototypeBuildResult.AtHeightLimit:
                    lastBuildEvent = "Stack height limit reached";
                    break;
                default:
                    lastBuildEvent = "Cursor outside build grid";
                    break;
            }

            UpdateBuildCursor();
        }

        private void CreatePlacedBlock(Vector2Int cell, int stackHeight)
        {
            if (editableBlocksRoot == null)
            {
                return;
            }

            if (!placedBlocks.TryGetValue(cell, out var stack))
            {
                stack = new Stack<GameObject>();
                placedBlocks.Add(cell, stack);
            }

            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = $"Placed Build Block {cell.x},{cell.y},{stackHeight}";
            block.transform.SetParent(editableBlocksRoot, false);
            block.transform.position = WorldForBuildCell(cell, stackHeight);
            block.transform.localScale = buildBlockScale;
            block.GetComponent<Renderer>().sharedMaterial = placedBlockMaterial;
            stack.Push(block);
        }

        private void RemovePlacedBlock(Vector2Int cell)
        {
            if (!placedBlocks.TryGetValue(cell, out var stack) || stack.Count == 0)
            {
                return;
            }

            var block = stack.Pop();
            if (block != null)
            {
                Destroy(block);
            }
        }

        private void ClearPlacedBlocks()
        {
            foreach (var stack in placedBlocks.Values)
            {
                foreach (var block in stack)
                {
                    if (block != null)
                    {
                        Destroy(block);
                    }
                }
            }

            placedBlocks.Clear();
        }

        private void EnsureBuildCursor()
        {
            if (buildCursor != null || editableBlocksRoot == null)
            {
                return;
            }

            var cursorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cursorObject.name = "Build Edit Cursor";
            cursorObject.transform.SetParent(editableBlocksRoot, false);
            cursorObject.transform.localScale = new Vector3(0.96f, 0.05f, 0.96f);
            cursorObject.GetComponent<Renderer>().sharedMaterial = buildCursorMaterial;
            buildCursor = cursorObject.transform;
        }

        private void UpdateBuildCursor()
        {
            EnsureBuildCursor();
            if (buildCursor == null || buildGrid == null)
            {
                return;
            }

            buildCursor.gameObject.SetActive(runState.Mode == BuilderPrototypeMode.BuildHook);
            var cell = buildGrid.SelectedCell;
            var topHeight = buildGrid.HeightAt(cell);
            buildCursor.position = WorldForBuildCell(cell, topHeight + 1) + new Vector3(0f, 0.04f, 0f);
        }

        private Vector3 WorldForBuildCell(Vector2Int cell, int stackHeight)
        {
            var x = cell.x - (BuilderPrototypeBuildGrid.DefaultWidth - 1) * 0.5f;
            var z = cell.y - (BuilderPrototypeBuildGrid.DefaultDepth - 1) * 0.5f;
            var y = buildBlockScale.y * 0.5f + Mathf.Max(0, stackHeight - 1) * buildBlockScale.y;
            return new Vector3(x, y, z);
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

            GUILayout.BeginArea(new Rect(16f, 16f, 460f, 184f), GUI.skin.box);
            GUILayout.Label(SceneContractText);
            GUILayout.Label("Mode: " + BuilderPrototypeRunState.LabelFor(runState.Mode));
            GUILayout.Label("Carried Loot Value: " + runState.CarriedLootValue);
            GUILayout.Label("Danger Tier: " + runState.DangerTier + "/" + BuilderPrototypeRunState.MaxDangerTier);
            GUILayout.Label("Build Blocks: " + BuildBlocksAvailable + " | Placed: " + PlacedBlockCount + " | Cursor: " + FormatCell(SelectedBuildCell));
            GUILayout.Label("Build Event: " + lastBuildEvent);
            GUILayout.Label(PromptFor(runState.Mode));
            GUILayout.EndArea();
        }

        private static string FormatCell(Vector2Int cell)
        {
            return cell.x + "," + cell.y;
        }
    }
}
