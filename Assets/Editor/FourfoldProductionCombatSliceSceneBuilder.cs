using System;
using System.Collections.Generic;
using System.IO;
using FourfoldEchoes.Product;
using FourfoldEchoes.Spike;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldProductionCombatSliceSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/ProductionCombatSlice.unity";
        private const string MaterialFolder = "Assets/Art/Generated/ProductionSlice/Materials";
        private const string RootName = "Production Combat Slice World";

        public static void BuildAndValidate()
        {
            Build();
            ValidateGeneratedScene();
        }

        public static void Build()
        {
            EnsureFolders();
            var materials = CreateMaterials();
            var library = new PrefabLibrary();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ProductionCombatSlice";
            ConfigureRenderSettings();
            CreateLighting();
            var camera = CreateCamera();

            var root = new GameObject(RootName);
            var field = new GameObject("PCS Block Field Arena");
            field.transform.SetParent(root.transform);
            var combatants = new GameObject("PCS Combatants");
            combatants.transform.SetParent(root.transform);
            var interactables = new GameObject("PCS Interactables");
            interactables.transform.SetParent(root.transform);
            var libraryYard = new GameObject("PCS Production Asset Yard");
            libraryYard.transform.SetParent(root.transform);

            CreateBlockField(field.transform, library);
            var player = Place(library.Hero, combatants.transform, "PCS Player Hero - FE_CHAR_PLAYER_Hero_01", new Vector3(-4.15f, 0.12f, -3.1f), Quaternion.Euler(0f, 42f, 0f), Vector3.one);
            var enemy = Place(library.MeleeEnemy, combatants.transform, "PCS Melee Enemy - FE_ENEMY_MELEE_Shardling", new Vector3(0.55f, 0.12f, 0.65f), Quaternion.Euler(0f, 218f, 0f), Vector3.one);
            var rangedEnemy = Place(library.RangedEnemy, combatants.transform, "PCS Ranged Enemy - FE_ENEMY_RANGED_BloomSpitter", new Vector3(2.85f, 0.12f, 1.95f), Quaternion.Euler(0f, 226f, 0f), Vector3.one);
            var boss = Place(library.Boss, combatants.transform, "PCS Boss Read Target - FE_BOSS_01_RootWarden", new Vector3(4.25f, 0.12f, -1.85f), Quaternion.Euler(0f, 245f, 0f), Vector3.one * 0.82f);

            var altarCore = Place(library.Pedestal, interactables.transform, "PCS Altar Core - FE_PROP_R01_GimmickPedestal_01", new Vector3(1.55f, 0.1f, -1.65f), Quaternion.identity, Vector3.one);
            var altarGlow = Place(library.RootSigil, interactables.transform, "PCS Altar Glow - FE_RELIC_RootSigil_01", new Vector3(1.55f, 0.72f, -1.65f), Quaternion.identity, Vector3.one * 1.15f);
            var gateLeft = Place(library.RootGate, interactables.transform, "PCS Gate Left - FE_PROP_R01_RootGate_01", new Vector3(3.7f, 0.9f, -0.48f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            var gateRight = Place(library.RootGate, interactables.transform, "PCS Gate Right - FE_PROP_R01_RootGate_01", new Vector3(3.7f, 0.9f, 0.48f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            var gateClaimBadge = Place(library.EmberSeed, interactables.transform, "PCS Gate Claim Badge - FE_RELIC_EmberSeed_01", new Vector3(3.45f, 1.72f, 0f), Quaternion.identity, Vector3.one);
            var rewardChest = Place(library.RelicChest, interactables.transform, "PCS Reward Chest - FE_PROP_COMMON_RelicChest_01", new Vector3(5.05f, 0.1f, 2.95f), Quaternion.Euler(0f, -18f, 0f), Vector3.one);
            var rewardPad = Place(library.RewardPad, interactables.transform, "PCS Reward Receiver Pad - FE_PROP_COMMON_RewardReceiverPad_01", new Vector3(4.25f, 0.08f, 2.35f), Quaternion.identity, Vector3.one);

            var node = CreateExplorationProof(interactables.transform, library, player.transform);
            CreateRuntimeHook(root.transform, player.transform, enemy.transform, rangedEnemy.transform, boss.transform, altarCore.transform, altarGlow.transform, gateLeft.transform, gateRight.transform, gateClaimBadge.transform, rewardChest, rewardPad, camera, materials, node);
            CreateAssetYard(libraryYard.transform, library);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(FourfoldD020SliceSceneBuilder.ScenePath, true)
            };
            Selection.activeObject = camera.gameObject;
            AssetDatabase.SaveAssets();
            Debug.Log($"FOURFOLD production combat slice scene generated at {ScenePath}");
        }

        public static void ValidateGeneratedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"Production combat slice scene is missing or invalid: {ScenePath}");
            }

            var root = Require(RootName);
            var player = RequirePrefab("PCS Player Hero - FE_CHAR_PLAYER_Hero_01", "FE_CHAR_PLAYER_Hero_01");
            var melee = RequirePrefab("PCS Melee Enemy - FE_ENEMY_MELEE_Shardling", "FE_ENEMY_MELEE_Shardling");
            RequirePrefab("PCS Ranged Enemy - FE_ENEMY_RANGED_BloomSpitter", "FE_ENEMY_RANGED_BloomSpitter");
            var boss = RequirePrefab("PCS Boss Read Target - FE_BOSS_01_RootWarden", "FE_BOSS_01_RootWarden");
            RequirePrefab("PCS Altar Core - FE_PROP_R01_GimmickPedestal_01", "FE_PROP_R01_GimmickPedestal_01");
            RequirePrefab("PCS Gate Left - FE_PROP_R01_RootGate_01", "FE_PROP_R01_RootGate_01");
            RequirePrefab("PCS Reward Chest - FE_PROP_COMMON_RelicChest_01", "FE_PROP_COMMON_RelicChest_01");
            RequirePrefab("PCS Exploration Tool Read - FE_PROP_COMMON_ExplorationTool_01", "FE_PROP_COMMON_ExplorationTool_01");
            RequirePrefab("PCS Revealed Shortcut Bridge - FE_ENV_R01_ShortcutBridge_01", "FE_ENV_R01_ShortcutBridge_01");

            var controller = RequireComponent<ProductionCombatSliceController>("PCS Runtime Hook");
            var tool = RequireComponent<ExplorationTool>("PCS Exploration Tool Runtime");
            var node = RequireComponent<ExplorationNode>("PCS Exploration Tool Node");

            if (controller.enemies == null || controller.enemies.Length < 2 || controller.boss == null || controller.rewardChest == null || controller.rewardPad == null)
            {
                throw new InvalidOperationException("Production combat slice controller is not wired to two enemies, boss, and reward objects.");
            }

            RequireCollider<CapsuleCollider>(player, "player hero");
            RequireCollider<CapsuleCollider>(melee, "melee enemy");
            RequireCollider<CapsuleCollider>(boss, "boss");
            RequireCollider<BoxCollider>(Require("PCS Revealed Shortcut Bridge - FE_ENV_R01_ShortcutBridge_01"), "shortcut bridge");

            var prefabPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var p3PrefabPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var prefabInstanceCount = 0;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(transform.gameObject);
                if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/Prefabs/Production/", StringComparison.Ordinal))
                {
                    continue;
                }

                prefabInstanceCount++;
                prefabPaths.Add(path);
                if (path.StartsWith("Assets/Prefabs/Production/P3/", StringComparison.Ordinal))
                {
                    p3PrefabPaths.Add(path);
                }
            }

            if (prefabInstanceCount < 70)
            {
                throw new InvalidOperationException($"Production combat slice uses too few Production prefab instances: {prefabInstanceCount}");
            }
            if (prefabPaths.Count < 18)
            {
                throw new InvalidOperationException($"Production combat slice uses too few distinct Production prefabs: {prefabPaths.Count}");
            }
            if (p3PrefabPaths.Count < 28)
            {
                throw new InvalidOperationException($"Production combat slice asset yard does not include the full P3 batch: {p3PrefabPaths.Count}/28 distinct P3 prefabs.");
            }

            var solved = node.TryActivate(tool.player, tool.range);
            if (!solved || !node.IsSolved || node.responseTarget == null || !node.responseTarget.activeSelf)
            {
                throw new InvalidOperationException("Production exploration tool does not activate its shortcut response.");
            }
            node.ResetNode();

            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var missingMaterialSlots = 0;
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null)
                    {
                        missingMaterialSlots++;
                    }
                }
            }
            if (renderers.Length < 120)
            {
                throw new InvalidOperationException($"Production combat slice has too few renderers for the prefab field: {renderers.Length}");
            }
            if (missingMaterialSlots > 0)
            {
                throw new InvalidOperationException($"Production combat slice has missing material slots: {missingMaterialSlots}");
            }
            if (Camera.main == null)
            {
                throw new InvalidOperationException("Production combat slice has no MainCamera tagged camera.");
            }

            Debug.Log($"FOURFOLD production combat slice validation passed; prefabInstances={prefabInstanceCount}, distinctPrefabs={prefabPaths.Count}, renderers={renderers.Length}");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.2f, 0.23f);
            RenderSettings.ambientEquatorColor = new Color(0.14f, 0.14f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.055f, 0.06f, 0.065f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.08f, 0.10f, 0.13f);
            RenderSettings.fogDensity = 0.01f;
        }

        private static void CreateLighting()
        {
            var keyObject = new GameObject("PCS Warm Key Light");
            keyObject.transform.rotation = Quaternion.Euler(56f, -42f, 0f);
            var key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.18f;
            key.color = new Color(1f, 0.78f, 0.56f);
            key.shadows = LightShadows.Soft;
            CreatePointLight("PCS Combat Fill", new Vector3(0.2f, 2.6f, 1.8f), new Color(1.0f, 0.34f, 0.18f), 2.1f, 7.4f);
            CreatePointLight("PCS Tool Fill", new Vector3(-3.5f, 2.4f, -1.6f), new Color(0.82f, 0.92f, 0.34f), 2.2f, 6.8f);
            CreatePointLight("PCS Reward Fill", new Vector3(4.9f, 2.2f, 2.9f), new Color(0.20f, 0.56f, 1.0f), 2.6f, 6.4f);
        }

        private static void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("PCS Top Down Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(8.8f, 13.4f, -10.6f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.2f, 0.35f, 0.25f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.6f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 95f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.075f, 0.09f, 0.12f);
            return camera;
        }

        private static void CreateBlockField(Transform parent, PrefabLibrary library)
        {
            var floorPrefabs = new[] { library.GrassFloor, library.GrassFloorLong, library.HubFloor, library.HubCrackedFloor };
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    var prefab = floorPrefabs[Mathf.Abs(x + z) % floorPrefabs.Length];
                    Place(prefab, parent, $"PCS Floor {x},{z}", new Vector3(x * 1.96f, 0f, z * 1.96f), Quaternion.identity, Vector3.one);
                }
            }

            for (var x = -3; x <= 3; x++)
            {
                Place(library.StoneWall, parent, $"PCS North Wall {x}", new Vector3(x * 1.96f, 0.02f, 4f * 1.96f), Quaternion.identity, Vector3.one);
                Place(library.StoneWall, parent, $"PCS South Wall {x}", new Vector3(x * 1.96f, 0.02f, -4f * 1.96f), Quaternion.Euler(0f, 180f, 0f), Vector3.one);
            }
            for (var z = -3; z <= 3; z++)
            {
                Place(library.Fence, parent, $"PCS East Rail {z}", new Vector3(4f * 1.96f, 0.02f, z * 1.96f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            }

            Place(library.ShortcutBridge, parent, "PCS Entry Shortcut Bridge", new Vector3(-5.85f, 0.05f, -1.0f), Quaternion.Euler(0f, 25f, 0f), Vector3.one);
            Place(library.WoodBridge, parent, "PCS Wood Bridge Reward Route", new Vector3(3.15f, 0.06f, 2.9f), Quaternion.Euler(0f, 72f, 0f), Vector3.one);
            Place(library.WaterEdge, parent, "PCS Water Edge Read", new Vector3(-1.85f, 0.04f, 4.65f), Quaternion.identity, Vector3.one);
            Place(library.HazardFloor, parent, "PCS Hazard Floor Read", new Vector3(0.7f, 0.035f, 2.65f), Quaternion.identity, Vector3.one);
        }

        private static ExplorationNode CreateExplorationProof(Transform parent, PrefabLibrary library, Transform player)
        {
            var proof = new GameObject("PCS Exploration Proof");
            proof.transform.SetParent(parent);

            var response = new GameObject("PCS Revealed Shortcut Route");
            response.transform.SetParent(proof.transform);
            Place(library.ShortcutBridge, response.transform, "PCS Revealed Shortcut Bridge - FE_ENV_R01_ShortcutBridge_01", new Vector3(-4.6f, 0.06f, 1.35f), Quaternion.Euler(0f, 31f, 0f), Vector3.one);
            Place(library.BridgeWood, response.transform, "PCS Revealed Wood Bridge - FE_ENV_R01_BF_BridgeWood_1x2", new Vector3(-3.05f, 0.08f, 2.15f), Quaternion.Euler(0f, 38f, 0f), Vector3.one * 0.74f);

            var nodeObject = Place(library.CheckpointPad, proof.transform, "PCS Exploration Tool Node", new Vector3(-3.45f, 0.08f, -1.85f), Quaternion.identity, Vector3.one);
            var idleRead = Place(library.ExplorationTool, nodeObject.transform, "PCS Exploration Tool Read - FE_PROP_COMMON_ExplorationTool_01", new Vector3(0f, 0.42f, 0f), Quaternion.Euler(0f, 16f, 0f), Vector3.one * 0.78f);
            var activeRead = Place(library.RootSigil, nodeObject.transform, "PCS Exploration Tool Active Sigil - FE_RELIC_RootSigil_01", new Vector3(0f, 0.86f, 0f), Quaternion.identity, Vector3.one);
            activeRead.SetActive(false);
            response.SetActive(false);

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.activationRadius = 2.8f;
            node.responseTarget = response;
            node.idleRead = idleRead;
            node.activeRead = activeRead;
            node.highlightRenderers = response.GetComponentsInChildren<Renderer>(true);
            node.ResetNode();

            if (player != null)
            {
                player.position = new Vector3(-4.15f, player.position.y, -3.1f);
            }
            return node;
        }

        private static void CreateRuntimeHook(Transform root, Transform player, Transform enemy, Transform rangedEnemy, Transform boss, Transform altarCore, Transform altarGlow, Transform gateLeft, Transform gateRight, Transform gateClaimBadge, GameObject rewardChest, GameObject rewardPad, Camera camera, SliceMaterials materials, ExplorationNode node)
        {
            var hook = new GameObject("PCS Runtime Hook");
            hook.transform.SetParent(root);
            hook.AddComponent<FourfoldProofAudio>();
            var controller = hook.AddComponent<ProductionCombatSliceController>();
            controller.player = player;
            controller.enemies = new[] { enemy, rangedEnemy };
            controller.boss = boss;
            controller.gateLeft = gateLeft;
            controller.gateRight = gateRight;
            controller.gateClaimBadge = gateClaimBadge;
            controller.rewardChest = rewardChest;
            controller.rewardPad = rewardPad;
            controller.fixedCamera = camera;
            controller.downMaterial = materials.enemyDead;
            controller.gateClosedMaterial = materials.gateClosed;
            controller.gateOpenMaterial = materials.gateOpen;
            controller.gateReadyMaterial = materials.gateReady;
            controller.rewardMaterial = materials.prism;

            var explorationHook = new GameObject("PCS Exploration Tool Runtime");
            explorationHook.transform.SetParent(root);
            var audioSource = explorationHook.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            var tool = explorationHook.AddComponent<ExplorationTool>();
            tool.player = player;
            tool.nodes = new[] { node };
            tool.range = 2.9f;
            tool.cooldownSeconds = 0.42f;
            tool.pulseRead = node.idleRead;

            controller.explorationTool = tool;
            controller.shortcutNode = node;
        }

        private static void CreateAssetYard(Transform parent, PrefabLibrary library)
        {
            var yardPrefabs = new List<GameObject>
            {
                library.SignatureLead, library.HeavyGuard, library.StampCaster, library.RangerScout, library.FieldMedic,
                library.Merchant, library.Smith, library.Cartographer, library.BlockCharger, library.LineSpitter, library.GlyphSwarm,
                library.ToolBlade, library.BlockHammer, library.GuardClamp, library.SealStamp, library.RouteSpool,
                library.RootGate, library.RelicChest, library.RewardPad, library.CheckpointPad,
                library.HubSaveStone, library.HubThreadLamp, library.HubRegionGate, library.RootArch, library.MossPillar,
                library.FlowerClump, library.ShortGrass, library.ToolMirror, library.BossSpawnAnchor, library.BossRewardExit,
                library.FurnaceWarden, library.GlassWarden, library.AmberRelay, library.CharcoalTile, library.CrystalBridge,
                library.ColdWhiteLamp, library.CrystalCluster, library.LowWallCrystal, library.HeatVent, library.ToolLens
            };
            yardPrefabs.AddRange(LoadProductionPhasePrefabs("P3"));

            for (var index = 0; index < yardPrefabs.Count; index++)
            {
                var x = -6.3f + (index % 10) * 1.35f;
                var z = 6.65f + (index / 10) * 1.35f;
                Place(yardPrefabs[index], parent, $"PCS Asset Yard {index:00} - {yardPrefabs[index].name}", new Vector3(x, 0.12f, z), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 0.55f);
            }
        }

        private static List<GameObject> LoadProductionPhasePrefabs(string phase)
        {
            var folder = $"Assets/Prefabs/Production/{phase}";
            var prefabs = new List<GameObject>();
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return prefabs;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            var paths = new List<string>(guids.Length);
            foreach (var guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            paths.Sort(StringComparer.Ordinal);

            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }
            return prefabs;
        }

        private static GameObject Place(GameObject prefab, Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not instantiate prefab: {AssetDatabase.GetAssetPath(prefab)}");
            }
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;
            return instance;
        }

        private static SliceMaterials CreateMaterials()
        {
            return new SliceMaterials
            {
                ember = CreateMaterial("PCS_Ember", new Color(1.0f, 0.42f, 0.16f), 0.02f, 0.48f, new Color(0.6f, 0.12f, 0.02f)),
                tide = CreateMaterial("PCS_Tide", new Color(0.18f, 0.72f, 0.92f), 0.02f, 0.54f, new Color(0.03f, 0.25f, 0.42f)),
                bloom = CreateMaterial("PCS_Bloom", new Color(0.48f, 0.86f, 0.32f), 0.01f, 0.5f, new Color(0.12f, 0.34f, 0.08f)),
                prism = CreateMaterial("PCS_Prism", new Color(0.72f, 0.48f, 1.0f), 0.02f, 0.58f, new Color(0.18f, 0.08f, 0.42f)),
                player = CreateMaterial("PCS_PlayerFallback", new Color(0.92f, 0.78f, 0.52f), 0.04f, 0.42f, null),
                enemy = CreateMaterial("PCS_EnemyTell", new Color(0.94f, 0.16f, 0.08f), 0.02f, 0.48f, new Color(0.58f, 0.05f, 0.03f)),
                enemyDead = CreateMaterial("PCS_EnemyDown", new Color(0.18f, 0.18f, 0.20f), 0.02f, 0.36f, null),
                altar = CreateMaterial("PCS_Altar", new Color(0.93f, 0.68f, 0.26f), 0.04f, 0.46f, new Color(0.36f, 0.16f, 0.02f)),
                gateClosed = CreateMaterial("PCS_GateClosed", new Color(0.24f, 0.20f, 0.18f), 0.08f, 0.38f, null),
                gateOpen = CreateMaterial("PCS_GateOpen", new Color(0.20f, 0.58f, 1.0f), 0.02f, 0.55f, new Color(0.03f, 0.2f, 0.54f)),
                gateReady = CreateMaterial("PCS_GateReady", new Color(1.0f, 0.82f, 0.28f), 0.02f, 0.56f, new Color(0.54f, 0.32f, 0.03f))
            };
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness, Color? emission)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.name = name;
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject Require(string name)
        {
            var gameObject = FindSceneObject(name);
            if (gameObject == null)
            {
                throw new InvalidOperationException($"Required object missing from production combat slice: {name}");
            }
            return gameObject;
        }

        private static GameObject RequirePrefab(string name, string prefabName)
        {
            var gameObject = Require(name);
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            if (string.IsNullOrEmpty(path) || !path.EndsWith($"/{prefabName}.prefab", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Object {name} is not an instance of {prefabName}; actual prefab path: {path}");
            }
            if (FindInChildren(gameObject.transform, "Visual") == null)
            {
                throw new InvalidOperationException($"Production prefab instance has no Visual child: {name}");
            }
            if (gameObject.GetComponentsInChildren<Renderer>(true).Length == 0)
            {
                throw new InvalidOperationException($"Production prefab instance has no renderer: {name}");
            }
            return gameObject;
        }

        private static T RequireComponent<T>(string name) where T : Component
        {
            var gameObject = Require(name);
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"Required component {typeof(T).Name} missing on object: {name}");
            }
            return component;
        }

        private static void RequireCollider<T>(GameObject gameObject, string label) where T : Collider
        {
            var collider = gameObject.GetComponent<T>();
            if (collider == null || !collider.enabled)
            {
                throw new InvalidOperationException($"Production {label} prefab is missing enabled {typeof(T).Name}: {gameObject.name}");
            }
        }

        private static GameObject FindSceneObject(string name)
        {
            var scene = EditorSceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var found = FindInChildren(roots[i].transform, name);
                if (found != null)
                {
                    return found.gameObject;
                }
            }
            return null;
        }

        private static Transform FindInChildren(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }
            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindInChildren(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private sealed class PrefabLibrary
        {
            public readonly GameObject Hero = Load("P0", "FE_CHAR_PLAYER_Hero_01");
            public readonly GameObject SignatureLead = Load("P0", "FE_CHAR_PLAYER_SignatureLead_01");
            public readonly GameObject HeavyGuard = Load("P0", "FE_CHAR_PLAYER_HeavyGuard_01");
            public readonly GameObject StampCaster = Load("P0", "FE_CHAR_PLAYER_StampCaster_01");
            public readonly GameObject RangerScout = Load("P0", "FE_CHAR_PLAYER_RangerScout_01");
            public readonly GameObject FieldMedic = Load("P0", "FE_CHAR_PLAYER_FieldMedic_01");
            public readonly GameObject Merchant = Load("P0", "FE_CHAR_NPC_MerchantTray_01");
            public readonly GameObject Smith = Load("P0", "FE_CHAR_NPC_UpgradeSmith_01");
            public readonly GameObject Cartographer = Load("P0", "FE_CHAR_NPC_CartographerGuide_01");
            public readonly GameObject MeleeEnemy = Load("P0", "FE_ENEMY_MELEE_Shardling");
            public readonly GameObject RangedEnemy = Load("P0", "FE_ENEMY_RANGED_BloomSpitter");
            public readonly GameObject BlockCharger = Load("P0", "FE_ENEMY_R01_BlockCharger_01");
            public readonly GameObject LineSpitter = Load("P0", "FE_ENEMY_R01_LineSpitter_01");
            public readonly GameObject GlyphSwarm = Load("P0", "FE_ENEMY_R01_GlyphSwarm_01");
            public readonly GameObject Boss = Load("P0", "FE_BOSS_01_RootWarden");
            public readonly GameObject ToolBlade = Load("P0", "FE_PROP_COMMON_ToolBlade_01");
            public readonly GameObject BlockHammer = Load("P0", "FE_PROP_COMMON_BlockHammer_01");
            public readonly GameObject GuardClamp = Load("P0", "FE_PROP_COMMON_GuardClamp_01");
            public readonly GameObject SealStamp = Load("P0", "FE_PROP_COMMON_SealStamp_01");
            public readonly GameObject RouteSpool = Load("P0", "FE_PROP_COMMON_RouteSpool_01");
            public readonly GameObject Pedestal = Load("P0", "FE_PROP_R01_GimmickPedestal_01");
            public readonly GameObject RelicChest = Load("P0", "FE_PROP_COMMON_RelicChest_01");
            public readonly GameObject ExplorationTool = Load("P0", "FE_PROP_COMMON_ExplorationTool_01");
            public readonly GameObject RootGate = Load("P0", "FE_PROP_R01_RootGate_01");
            public readonly GameObject EmberSeed = Load("P0", "FE_RELIC_EmberSeed_01");
            public readonly GameObject RootSigil = Load("P0", "FE_RELIC_RootSigil_01");
            public readonly GameObject CheckpointPad = Load("P0", "FE_PROP_COMMON_CheckpointSocketPad_01");
            public readonly GameObject RewardPad = Load("P0", "FE_PROP_COMMON_RewardReceiverPad_01");
            public readonly GameObject GrassFloor = Load("P0", "FE_ENV_R01_BF_GrassFloor_1x1_Flat");
            public readonly GameObject GrassFloorLong = Load("P0", "FE_ENV_R01_BF_GrassFloor_1x2_Straight");
            public readonly GameObject HubFloor = Load("P0", "FE_ENV_HUB_BF_StoneFloor_1x1_Clean");
            public readonly GameObject HubCrackedFloor = Load("P0", "FE_ENV_HUB_BF_StoneFloor_1x1_Cracked");
            public readonly GameObject StoneWall = Load("P0", "FE_ENV_HUB_BF_StoneWall_Straight_1x1");
            public readonly GameObject Fence = Load("P0", "FE_ENV_R01_BF_FenceRailing_Straight");
            public readonly GameObject ShortcutBridge = Load("P0", "FE_ENV_R01_ShortcutBridge_01");
            public readonly GameObject BridgeWood = Load("P0", "FE_ENV_R01_BF_BridgeWood_1x2");
            public readonly GameObject WoodBridge = Load("P0", "FE_ENV_R01_BF_BridgeWood_1x2");
            public readonly GameObject WaterEdge = Load("P0", "FE_ENV_R01_BF_WaterChannelEdge_Straight");
            public readonly GameObject HazardFloor = Load("P0", "FE_ENV_HUB_BF_HazardFloor_Spike");
            public readonly GameObject HubSaveStone = Load("P1", "FE_PROP_HUB_SaveStone_01");
            public readonly GameObject HubThreadLamp = Load("P1", "FE_PROP_HUB_ThreadLamp_01");
            public readonly GameObject HubRegionGate = Load("P1", "FE_PROP_HUB_RegionGate_01");
            public readonly GameObject RootArch = Load("P1", "FE_PROP_R01_RootArch_01");
            public readonly GameObject MossPillar = Load("P1", "FE_PROP_R01_MossPillar_01");
            public readonly GameObject FlowerClump = Load("P1", "FE_PROP_R01_FlowerClump_01");
            public readonly GameObject ShortGrass = Load("P1", "FE_PROP_R01_ShortGrass_01");
            public readonly GameObject ToolMirror = Load("P1", "FE_PROP_R01_ToolMirror_01");
            public readonly GameObject BossSpawnAnchor = Load("P1", "FE_PROP_BOSS_SpawnAnchor_01");
            public readonly GameObject BossRewardExit = Load("P1", "FE_PROP_BOSS_RewardExit_01");
            public readonly GameObject FurnaceWarden = Load("P2", "FE_BOSS_02_FurnaceWarden");
            public readonly GameObject GlassWarden = Load("P2", "FE_BOSS_03_GlassWarden");
            public readonly GameObject AmberRelay = Load("P2", "FE_PROP_R02_AmberRelay_01");
            public readonly GameObject CharcoalTile = Load("P2", "FE_ENV_R02_FloorCharcoalTile_01");
            public readonly GameObject CrystalBridge = Load("P2", "FE_ENV_R03_CrystalBridge_01");
            public readonly GameObject ColdWhiteLamp = Load("P2", "FE_PROP_R03_ColdWhiteLamp_01");
            public readonly GameObject CrystalCluster = Load("P2", "FE_PROP_R03_CrystalCluster_01");
            public readonly GameObject LowWallCrystal = Load("P2", "FE_PROP_R03_LowWallCrystal_01");
            public readonly GameObject HeatVent = Load("P2", "FE_PROP_R02_HeatVent_01");
            public readonly GameObject ToolLens = Load("P2", "FE_PROP_R03_ToolLens_01");

            private static GameObject Load(string phase, string name)
            {
                var path = $"Assets/Prefabs/Production/{phase}/{name}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    throw new FileNotFoundException($"Production prefab not found: {path}");
                }
                return prefab;
            }
        }

        private sealed class SliceMaterials
        {
            public Material ember;
            public Material tide;
            public Material bloom;
            public Material prism;
            public Material player;
            public Material enemy;
            public Material enemyDead;
            public Material altar;
            public Material gateClosed;
            public Material gateOpen;
            public Material gateReady;
        }
    }
}
