using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace FourfoldEchoes.BuilderPrototype
{
    public sealed class BuilderPrototypeSpineController : MonoBehaviour
    {
        public const string SceneContractText = "PR-05B integrated preview: traversal, build edit loop, tactical telegraphs, deterministic loot, and extract HUD.";
        public const string ControlPromptText = "Move LS/WASD | Build X/B | Combat Y/C | Loot LB/L | Extract RB/E | Reset Start/R";
        public const string BuildHookPromptText = "Build: move cursor LS/arrows | Place A/J | Remove X/K | Exit B/Tab";
        public const string CombatHookPromptText = "Combat preview: read telegraphs, safe lanes, and flank/rear bonus | Exit B/Tab";
        public const string LootHookPromptText = "Loot: collect prototype cache A/J; deterministic pickup raises value and pressure.";
        public const string ExtractHookPromptText = "Extract: bank A/J | lose X/K; deterministic rolls update the run HUD.";
        public const string PrototypeBuildIdentityText = "Echo Forgemason";
        public const string PrototypeBuildSelectedAffixesText = "+BuilderPower +BuildSpeed +StrikerDamage";

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

        [Header("Combat Preview")]
        public Material combatTelegraphMaterial;
        public Material combatSafeMarkerMaterial;
        public Material combatThreatenedMarkerMaterial;
        public Material combatUnsafeMarkerMaterial;
        public float combatPreviewLoopDuration = 3.2f;
        public float combatTelegraphRadius = 2.15f;
        public Vector2 combatLineTelegraphSize = new Vector2(1.35f, 4.4f);
        public Vector3 combatMarkerScale = new Vector3(0.82f, 0.03f, 0.82f);

        [Header("Movement")]
        public float moveSpeed = 5.2f;
        public Vector2 xBounds = new Vector2(-6.4f, 6.4f);
        public Vector2 zBounds = new Vector2(-4.4f, 4.4f);

        [Header("Camera")]
        public Vector3 cameraOffset = new Vector3(0f, 8.4f, -7.2f);
        public float cameraFollowSharpness = 12f;

        [Header("Prototype HUD")]
        public bool showDebugHud = true;

        [Header("Loot Preview")]
        public Transform prototypeLootPickup;
        public Transform prototypeExtractGate;
        public Material lootPickupAvailableMaterial;
        public Material lootPickupCollectedMaterial;
        public Material extractGateReadyMaterial;
        public Material extractGateBankedMaterial;
        public Material extractGateLostMaterial;

        private const float CombatTelegraphResolveAt = 2.15f;
        private const float CombatTelegraphResolveDuration = 0.48f;
        private const float CombatLineTelegraphForwardOffset = 2.05f;

        private readonly BuilderPrototypeRunState runState = new BuilderPrototypeRunState();
        private readonly BuilderPrototypeLootPressureModel lootPressure = new BuilderPrototypeLootPressureModel();
        private readonly BuilderPrototypeCharacterBuildSnapshot characterBuildSnapshot = CreatePrototypeCharacterBuildSheet().Evaluate();
        private readonly BuilderPrototypeCharacterBuildValidation characterBuildValidation = CreatePrototypeCharacterBuildSheet().Validate();
        private readonly Dictionary<Vector2Int, Stack<GameObject>> placedBlocks = new Dictionary<Vector2Int, Stack<GameObject>>();
        private readonly BuilderPrototypeTacticalModel tacticalModel = new BuilderPrototypeTacticalModel();
        private BuilderPrototypeBuildGrid buildGrid;
        private Transform buildCursor;
        private Transform combatPreviewRoot;
        private Transform combatCircleTelegraph;
        private Transform combatLineTelegraph;
        private Transform combatSafetyMarker;
        private float cursorMoveRepeatTimer;
        private float combatPreviewTime;
        private Vector3 facing = Vector3.forward;
        private Vector3 startPosition;
        private string lastBuildEvent = "Build loop ready";
        private string lastLootRunEvent = "Loot run preview ready";
        private BuilderPrototypeExtractionResult lastExtractionResult;
        private BuilderPrototypeTacticalEvaluation combatEvaluation = new BuilderPrototypeTacticalEvaluation(BuilderPrototypePositionSafety.Safe, 0, 0, -1f);
        private BuilderPrototypePositionalBonus combatPositionalBonus = BuilderPrototypePositionalBonus.None;

        public BuilderPrototypeMode CurrentMode => runState.Mode;
        public int CarriedLootValue => lootPressure.CarriedLootValue;
        public int CarriedLootItemCount => lootPressure.CarriedItemCount;
        public int BankedLootValue => lootPressure.BankedLootValue;
        public int BankedLootItemCount => lootPressure.BankedItemCount;
        public int PressureScore => lootPressure.PressureScore;
        public BuilderPrototypePressureBand PressureBand => lootPressure.PressureBand;
        public int ExtractionRiskPercent => lootPressure.ExtractionRiskPercent;
        public int DangerTier => PressureTierForHud(lootPressure.PressureScore);
        public string LastLootRunEvent => lastLootRunEvent;
        public bool HasRequiredHookAnchors => buildHookAnchor != null && combatHookAnchor != null && lootHookAnchor != null && extractHookAnchor != null;
        public bool HasRequiredBuildReferences => editableBlocksRoot != null && placedBlockMaterial != null && buildCursorMaterial != null;
        public bool HasRequiredCombatPreviewReferences => combatTelegraphMaterial != null && combatSafeMarkerMaterial != null && combatThreatenedMarkerMaterial != null && combatUnsafeMarkerMaterial != null;
        public bool HasRequiredLootPreviewReferences =>
            prototypeLootPickup != null &&
            prototypeExtractGate != null &&
            lootPickupAvailableMaterial != null &&
            lootPickupCollectedMaterial != null &&
            extractGateReadyMaterial != null &&
            extractGateBankedMaterial != null &&
            extractGateLostMaterial != null;
        public int BuildBlocksAvailable => buildGrid?.BlocksAvailable ?? startingBuildBlocks;
        public int PlacedBlockCount => buildGrid?.PlacedBlockCount ?? 0;
        public Vector2Int SelectedBuildCell => buildGrid?.SelectedCell ?? Vector2Int.zero;
        public int CombatPreviewTelegraphCount => tacticalModel.TelegraphZoneCount;
        public BuilderPrototypePositionSafety CombatPreviewSafety => combatEvaluation.Safety;
        public BuilderPrototypePositionalBonus CombatPreviewPositionalBonus => combatPositionalBonus;
        public string CombatPreviewHudText => FormatCombatPreviewHud();
        public bool IsPrototypeCharacterBuildValid => characterBuildValidation.IsValid;
        public BuilderPrototypeCharacterBuildSnapshot CharacterBuildSnapshot => characterBuildSnapshot;
        public string CharacterBuildHudText => FormatCharacterBuildHud();
        public string CharacterBuildSourceHudText => FormatCharacterBuildSourceHud();

        public void Awake()
        {
            if (player != null)
            {
                startPosition = player.position;
            }

            ResetBuildGrid();
            ResetCombatPreview();
            UpdateLootPreviewVisuals();
            SnapCameraToPlayer();
        }

        public void Update()
        {
            cursorMoveRepeatTimer = Mathf.Max(0f, cursorMoveRepeatTimer - Time.deltaTime);
            HandleModeInput();
            HandlePrototypeStateInput();
            HandleLootPreviewInput();
            HandleBuildInput();
            MovePlayer(Time.deltaTime);
            UpdateBuildCursor();
            UpdateCombatPreview(Time.deltaTime);
            FollowPlayer(Time.deltaTime);
        }

        public void ResetPrototypeRun()
        {
            runState.ResetRun();
            lootPressure.LoseCarriedLootAndResetPressure();
            lastExtractionResult = null;
            lastLootRunEvent = "Loot run preview reset";
            if (player != null)
            {
                player.position = startPosition;
            }

            UpdateLootPreviewVisuals();
            SnapCameraToPlayer();
            ResetCombatPreview();
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

        public void SetModeForPrototypePreview(BuilderPrototypeMode mode)
        {
            SetPrototypeMode(mode);
        }

        public static Vector3 ClampToArena(Vector3 position, Vector2 xRange, Vector2 zRange)
        {
            return new Vector3(
                Mathf.Clamp(position.x, xRange.x, xRange.y),
                position.y,
                Mathf.Clamp(position.z, zRange.x, zRange.y));
        }

        public static BuilderPrototypeLootItem CreatePrototypeLootForPreview()
        {
            return new BuilderPrototypeLootItem("prototype-echo-cache", BuilderPrototypeLootRarity.Rare, 12, 2);
        }

        public static BuilderPrototypeCharacterBuildSheet CreatePrototypeCharacterBuildSheet()
        {
            return new BuilderPrototypeCharacterBuildSheet()
                .AddSource(new BuilderPrototypeBuildSource("Echo Chisel", BuilderPrototypeBuildSourceKind.Gear, affixBudget: 4))
                .AddSource(new BuilderPrototypeBuildSource("Anchor Greaves", BuilderPrototypeBuildSourceKind.Gear, affixBudget: 2))
                .AddSource(new BuilderPrototypeBuildSource("Anvil Vow", BuilderPrototypeBuildSourceKind.Passive))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuilderPower,
                    BuilderPrototypeStatModifierKind.Flat,
                    8d,
                    "Echo Chisel",
                    affixCost: 2))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuildSpeed,
                    BuilderPrototypeStatModifierKind.AdditivePercent,
                    0.25d,
                    "Echo Chisel",
                    affixCost: 1))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.StrikerDamage,
                    BuilderPrototypeStatModifierKind.Flat,
                    8d,
                    "Echo Chisel",
                    affixCost: 1))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.SentinelGuard,
                    BuilderPrototypeStatModifierKind.Flat,
                    5d,
                    "Anchor Greaves",
                    affixCost: 1))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.Vitality,
                    BuilderPrototypeStatModifierKind.Flat,
                    20d,
                    "Anchor Greaves",
                    affixCost: 1))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BreakerPower,
                    BuilderPrototypeStatModifierKind.Flat,
                    6d,
                    "Anvil Vow"));
        }

        public void CollectPrototypeLootForPreview()
        {
            var item = CreatePrototypeLootForPreview();
            lootPressure.CollectLoot(item);
            lastExtractionResult = null;
            lastLootRunEvent = $"Picked up {item.ItemId}: +{item.ExtractionValue} value, pressure {lootPressure.PressureScore}/{BuilderPrototypeLootPressureModel.MaxPressureScore}";
            UpdateLootPreviewVisuals();
        }

        public BuilderPrototypeExtractionResult BankPrototypeLootForPreview()
        {
            return AttemptPrototypeExtractionForPreview(100);
        }

        public BuilderPrototypeExtractionResult LosePrototypeLootForPreview()
        {
            return AttemptPrototypeExtractionForPreview(0);
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
                SetPrototypeMode(BuilderPrototypeMode.BuildHook);
            }
            else if (Pressed(KeyCode.C, KeyCode.JoystickButton3))
            {
                SetPrototypeMode(BuilderPrototypeMode.CombatHook);
            }
            else if (Pressed(KeyCode.L, KeyCode.JoystickButton4))
            {
                SetPrototypeMode(BuilderPrototypeMode.LootHook);
            }
            else if (Pressed(KeyCode.E, KeyCode.JoystickButton5))
            {
                SetPrototypeMode(BuilderPrototypeMode.ExtractHook);
            }
            else if (Pressed(KeyCode.Tab, KeyCode.JoystickButton1))
            {
                SetPrototypeMode(BuilderPrototypeMode.Traverse);
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

        private void HandleLootPreviewInput()
        {
            if (runState.Mode == BuilderPrototypeMode.LootHook && Pressed(KeyCode.J, KeyCode.JoystickButton0))
            {
                CollectPrototypeLootForPreview();
            }
            else if (runState.Mode == BuilderPrototypeMode.ExtractHook && Pressed(KeyCode.J, KeyCode.JoystickButton0))
            {
                BankPrototypeLootForPreview();
            }
            else if (runState.Mode == BuilderPrototypeMode.ExtractHook && Pressed(KeyCode.K, KeyCode.JoystickButton2))
            {
                LosePrototypeLootForPreview();
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

        private void SetPrototypeMode(BuilderPrototypeMode mode)
        {
            if (runState.Mode == mode)
            {
                return;
            }

            runState.SetMode(mode);
            if (mode == BuilderPrototypeMode.CombatHook)
            {
                combatPreviewTime = 0f;
            }

            UpdateBuildCursor();
            UpdateCombatPreview(0f);
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

        private void ResetCombatPreview()
        {
            combatPreviewTime = 0f;
            RefreshCombatTelegraphs();
            EnsureCombatPreviewObjects();
            UpdateCombatPreview(0f);
        }

        private void UpdateCombatPreview(float deltaTime)
        {
            RefreshCombatTelegraphs();
            EnsureCombatPreviewObjects();

            var active = runState.Mode == BuilderPrototypeMode.CombatHook;
            if (combatPreviewRoot != null)
            {
                combatPreviewRoot.gameObject.SetActive(active);
            }

            if (!active)
            {
                combatEvaluation = new BuilderPrototypeTacticalEvaluation(BuilderPrototypePositionSafety.Safe, 0, 0, -1f);
                combatPositionalBonus = BuilderPrototypePositionalBonus.None;
                return;
            }

            combatPreviewTime += Mathf.Max(0f, deltaTime);
            var loopDuration = Mathf.Max(combatPreviewLoopDuration, CombatTelegraphResolveAt + CombatTelegraphResolveDuration + 0.1f);
            if (combatPreviewTime > loopDuration)
            {
                combatPreviewTime %= loopDuration;
            }

            var playerPosition = TopDown(player != null ? player.position : startPosition);
            combatEvaluation = tacticalModel.EvaluatePosition(playerPosition, combatPreviewTime);
            combatPositionalBonus = BuilderPrototypeTacticalModel.EvaluatePositionalBonus(
                playerPosition,
                CombatTargetPosition,
                CombatTargetFacing);

            PositionCombatPreviewObjects();
            UpdateCombatSafetyMarker();
        }

        private void RefreshCombatTelegraphs()
        {
            tacticalModel.ClearTelegraphZones();

            var targetPosition = CombatTargetPosition;
            var targetFacing = CombatTargetFacing;
            var castWindow = new BuilderPrototypeCastWindow(0f, CombatTelegraphResolveAt, CombatTelegraphResolveDuration);
            tacticalModel.AddTelegraphZone(BuilderPrototypeTelegraphZone.Circle(targetPosition, combatTelegraphRadius, castWindow));
            tacticalModel.AddTelegraphZone(BuilderPrototypeTelegraphZone.Rectangle(
                targetPosition + targetFacing * CombatLineTelegraphForwardOffset,
                combatLineTelegraphSize,
                targetFacing,
                castWindow));
        }

        private void EnsureCombatPreviewObjects()
        {
            if (combatPreviewRoot == null)
            {
                combatPreviewRoot = new GameObject("Combat Hook Tactical Preview").transform;
                combatPreviewRoot.SetParent(transform, false);
            }

            if (combatCircleTelegraph == null)
            {
                combatCircleTelegraph = CreatePreviewPrimitive(
                    PrimitiveType.Cylinder,
                    "Combat Telegraph Circle",
                    combatPreviewRoot,
                    combatTelegraphMaterial);
            }

            if (combatLineTelegraph == null)
            {
                combatLineTelegraph = CreatePreviewPrimitive(
                    PrimitiveType.Cube,
                    "Combat Telegraph Line",
                    combatPreviewRoot,
                    combatTelegraphMaterial);
            }

            if (combatSafetyMarker == null)
            {
                combatSafetyMarker = CreatePreviewPrimitive(
                    PrimitiveType.Cylinder,
                    "Combat Player Safety Marker",
                    combatPreviewRoot,
                    combatSafeMarkerMaterial);
            }

            PositionCombatPreviewObjects();
        }

        private Transform CreatePreviewPrimitive(PrimitiveType primitiveType, string objectName, Transform parent, Material material)
        {
            var previewObject = GameObject.CreatePrimitive(primitiveType);
            previewObject.name = objectName;
            previewObject.transform.SetParent(parent, false);

            var collider = previewObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = previewObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return previewObject.transform;
        }

        private void PositionCombatPreviewObjects()
        {
            var targetPosition = CombatTargetPosition;
            var targetFacing = CombatTargetFacing;
            var targetWorld = new Vector3(targetPosition.x, 0.035f, targetPosition.y);
            var lineWorld = new Vector3(
                targetPosition.x + targetFacing.x * CombatLineTelegraphForwardOffset,
                0.055f,
                targetPosition.y + targetFacing.y * CombatLineTelegraphForwardOffset);

            if (combatCircleTelegraph != null)
            {
                combatCircleTelegraph.position = targetWorld;
                combatCircleTelegraph.localScale = new Vector3(combatTelegraphRadius * 2f, 0.02f, combatTelegraphRadius * 2f);
                combatCircleTelegraph.rotation = Quaternion.identity;
            }

            if (combatLineTelegraph != null)
            {
                combatLineTelegraph.position = lineWorld;
                combatLineTelegraph.localScale = new Vector3(combatLineTelegraphSize.x, 0.04f, combatLineTelegraphSize.y);
                combatLineTelegraph.rotation = RotationForTopDownFacing(targetFacing);
            }

            if (combatSafetyMarker != null && player != null)
            {
                combatSafetyMarker.position = new Vector3(player.position.x, 0.075f, player.position.z);
                combatSafetyMarker.localScale = combatMarkerScale;
                combatSafetyMarker.rotation = Quaternion.identity;
            }
        }

        private void UpdateCombatSafetyMarker()
        {
            if (combatSafetyMarker == null)
            {
                return;
            }

            var renderer = combatSafetyMarker.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            switch (combatEvaluation.Safety)
            {
                case BuilderPrototypePositionSafety.Unsafe:
                    renderer.sharedMaterial = combatUnsafeMarkerMaterial;
                    break;
                case BuilderPrototypePositionSafety.Threatened:
                    renderer.sharedMaterial = combatThreatenedMarkerMaterial;
                    break;
                default:
                    renderer.sharedMaterial = combatSafeMarkerMaterial;
                    break;
            }
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

        private BuilderPrototypeExtractionResult AttemptPrototypeExtractionForPreview(int safetyRollPercent)
        {
            lastExtractionResult = lootPressure.AttemptExtraction(safetyRollPercent);
            switch (lastExtractionResult.Outcome)
            {
                case BuilderPrototypeExtractionOutcome.Extracted:
                    lastLootRunEvent = $"Extracted {lastExtractionResult.BankedValue} value ({lastExtractionResult.BankedItemCount} item), risk {lastExtractionResult.RiskPercent}%, roll {lastExtractionResult.SafetyRollPercent}";
                    break;
                case BuilderPrototypeExtractionOutcome.Lost:
                    lastLootRunEvent = $"Lost {lastExtractionResult.LostValue} value ({lastExtractionResult.LostItemCount} item), risk {lastExtractionResult.RiskPercent}%, roll {lastExtractionResult.SafetyRollPercent}";
                    break;
                default:
                    lastLootRunEvent = "No carried loot to extract";
                    break;
            }

            UpdateLootPreviewVisuals();
            return lastExtractionResult;
        }

        private void UpdateLootPreviewVisuals()
        {
            SetRendererMaterial(
                prototypeLootPickup,
                lootPressure.HasCarriedLoot ? lootPickupCollectedMaterial : lootPickupAvailableMaterial);

            var gateMaterial = extractGateReadyMaterial;
            if (lastExtractionResult != null)
            {
                if (lastExtractionResult.Outcome == BuilderPrototypeExtractionOutcome.Extracted)
                {
                    gateMaterial = extractGateBankedMaterial;
                }
                else if (lastExtractionResult.Outcome == BuilderPrototypeExtractionOutcome.Lost)
                {
                    gateMaterial = extractGateLostMaterial;
                }
            }

            SetRendererMaterial(prototypeExtractGate, gateMaterial);
        }

        private static void SetRendererMaterial(Transform target, Material material)
        {
            if (target == null || material == null)
            {
                return;
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private Vector3 WorldForBuildCell(Vector2Int cell, int stackHeight)
        {
            var x = cell.x - (BuilderPrototypeBuildGrid.DefaultWidth - 1) * 0.5f;
            var z = cell.y - (BuilderPrototypeBuildGrid.DefaultDepth - 1) * 0.5f;
            var y = buildBlockScale.y * 0.5f + Mathf.Max(0, stackHeight - 1) * buildBlockScale.y;
            return new Vector3(x, y, z);
        }

        private Vector2 CombatTargetPosition => TopDown(combatHookAnchor != null ? combatHookAnchor.position : new Vector3(2.25f, 0f, 1.4f));

        private Vector2 CombatTargetFacing
        {
            get
            {
                var towardArenaCenter = -CombatTargetPosition;
                return towardArenaCenter.sqrMagnitude > 0.0001f ? towardArenaCenter.normalized : Vector2.down;
            }
        }

        private static Vector2 TopDown(Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }

        private static Quaternion RotationForTopDownFacing(Vector2 facing)
        {
            var direction = new Vector3(facing.x, 0f, facing.y);
            return direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction.normalized, Vector3.up) : Quaternion.identity;
        }

        private string FormatCombatPreviewHud()
        {
            return "Combat Preview: "
                + combatEvaluation.Safety
                + " | Bonus: "
                + FormatPositionalBonus(combatPositionalBonus)
                + " | Telegraphs: "
                + tacticalModel.TelegraphZoneCount
                + " | Unsafe in: "
                + FormatUnsafeCountdown(combatEvaluation.SecondsUntilUnsafe);
        }

        private static string FormatPositionalBonus(BuilderPrototypePositionalBonus bonus)
        {
            switch (bonus)
            {
                case BuilderPrototypePositionalBonus.Rear:
                    return "Rear";
                case BuilderPrototypePositionalBonus.Flank:
                    return "Flank";
                default:
                    return "None";
            }
        }

        private static string FormatUnsafeCountdown(float secondsUntilUnsafe)
        {
            return secondsUntilUnsafe >= 0f ? secondsUntilUnsafe.ToString("0.0") + "s" : "--";
        }

        private string FormatCharacterBuildHud()
        {
            return "Build: "
                + PrototypeBuildIdentityText
                + " | Role: "
                + FormatRoleTags(characterBuildSnapshot.RoleTags)
                + " | Build "
                + FormatStat(characterBuildSnapshot.GetStat(BuilderPrototypeBuildStatId.BuilderPower))
                + " Speed "
                + FormatStat(characterBuildSnapshot.GetStat(BuilderPrototypeBuildStatId.BuildSpeed))
                + " | Off "
                + FormatStat(characterBuildSnapshot.GetStat(BuilderPrototypeBuildStatId.StrikerDamage))
                + " Break "
                + FormatStat(characterBuildSnapshot.GetStat(BuilderPrototypeBuildStatId.BreakerPower))
                + " Guard "
                + FormatStat(characterBuildSnapshot.GetStat(BuilderPrototypeBuildStatId.SentinelGuard))
                + " | Press "
                + lootPressure.PressureScore
                + "/"
                + BuilderPrototypeLootPressureModel.MaxPressureScore
                + " "
                + lootPressure.PressureBand
                + " Risk "
                + lootPressure.ExtractionRiskPercent
                + "%";
        }

        private string FormatCharacterBuildSourceHud()
        {
            if (characterBuildSnapshot.AffixBudgets.Count == 0)
            {
                return "Source: none";
            }

            var source = characterBuildSnapshot.AffixBudgets[0];
            return "Source: "
                + source.SourceLabel
                + " ("
                + source.SourceKind
                + ") affix "
                + source.Used
                + "/"
                + source.Budget
                + " | "
                + PrototypeBuildSelectedAffixesText;
        }

        private static string FormatRoleTags(IReadOnlyList<BuilderPrototypeBuildRoleTag> roleTags)
        {
            if (roleTags.Count == 0)
            {
                return "None";
            }

            var value = roleTags[0].ToString();
            for (var index = 1; index < roleTags.Count; index++)
            {
                value += "/" + roleTags[index];
            }

            return value;
        }

        private static string FormatStat(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static bool Pressed(KeyCode keyboard, KeyCode gamepad)
        {
            return Input.GetKeyDown(keyboard) || Input.GetKeyDown(gamepad);
        }

        private static int PressureTierForHud(int pressureScore)
        {
            if (pressureScore >= 75)
            {
                return 5;
            }

            if (pressureScore >= 50)
            {
                return 4;
            }

            if (pressureScore >= 25)
            {
                return 2;
            }

            return pressureScore > 0 ? 1 : 0;
        }

        private void OnGUI()
        {
            if (!showDebugHud)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 720f, 300f), GUI.skin.box);
            GUILayout.Label(SceneContractText);
            GUILayout.Label("Mode: " + BuilderPrototypeRunState.LabelFor(runState.Mode));
            GUILayout.Label(CharacterBuildHudText);
            GUILayout.Label(CharacterBuildSourceHudText);
            GUILayout.Label("Build Blocks: " + BuildBlocksAvailable + " | Placed: " + PlacedBlockCount + " | Cursor: " + FormatCell(SelectedBuildCell));
            GUILayout.Label("Build Event: " + lastBuildEvent);
            if (runState.Mode == BuilderPrototypeMode.CombatHook)
            {
                GUILayout.Label(CombatPreviewHudText);
            }
            else if (runState.Mode == BuilderPrototypeMode.LootHook || runState.Mode == BuilderPrototypeMode.ExtractHook)
            {
                GUILayout.Label("Carried Loot: " + lootPressure.CarriedLootValue + " value | " + lootPressure.CarriedItemCount + " item");
                GUILayout.Label("Pressure: " + lootPressure.PressureScore + "/" + BuilderPrototypeLootPressureModel.MaxPressureScore + " " + lootPressure.PressureBand + " | Extract Risk: " + lootPressure.ExtractionRiskPercent + "%");
                GUILayout.Label("Banked Loot: " + lootPressure.BankedLootValue + " value | " + lootPressure.BankedItemCount + " item");
                GUILayout.Label("Loot Event: " + lastLootRunEvent);
            }

            GUILayout.Label(PromptFor(runState.Mode));
            GUILayout.EndArea();
        }

        private static string FormatCell(Vector2Int cell)
        {
            return cell.x + "," + cell.y;
        }
    }
}
