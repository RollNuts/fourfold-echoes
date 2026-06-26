using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldD020SliceSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/D020VerticalSlice.unity";
        private const string MaterialFolder = "Assets/Art/Generated/D020/Materials";
        private const string AttackClipPath = "Assets/Audio/Generated/attack_basic.wav";
        private const string HitClipPath = "Assets/Audio/Generated/hit_enemy.wav";
        private const string DodgeClipPath = "Assets/Audio/Generated/dodge.wav";
        private const string RewardClaimClipPath = "Assets/Audio/Generated/relic_pickup.wav";
        private const string RewardReadyClipPath = "Assets/Audio/Generated/discovery_stinger.wav";
        private const string ToolPulseClipPath = "Assets/Audio/Generated/tool_pulse.wav";
        private const string ToolTargetHitClipPath = "Assets/Audio/Generated/shortcut_open.wav";
        private const string ExplorationMusicPath = "Assets/Audio/Generated/d020_exploration_loop.wav";
        private const string BossMusicPath = "Assets/Audio/Generated/d020_boss_loop.wav";

        public static void BuildAndValidate()
        {
            Build();
            ValidateGeneratedScene();
        }

        public static void Build()
        {
            EnsureFolders();
            FourfoldProductionArtP0Builder.BuildAssetsOnly();
            var assets = CreateGeneratedAssets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "D020VerticalSlice";

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.19f, 0.22f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.13f, 0.14f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.055f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.09f, 0.10f, 0.12f);
            RenderSettings.fogDensity = 0.012f;

            CreateLighting();
            var camera = CreateCamera();
            var root = new GameObject("D020 Slice Field");
            CreateField(root.transform, assets);
            var player = CreatePlayer(root.transform, assets);
            var meleeEnemy = CreateMeleeEnemy(root.transform, assets);
            var rangedEnemy = CreateRangedEnemy(root.transform, assets);
            var firstBoss = CreateFirstBoss(root.transform, assets);
            var chest = CreateChest(root.transform, assets);
            var secondChest = CreateSecondChest(root.transform, assets);
            var node = CreateExplorationToolProof(root.transform, assets);
            var secondNode = CreateSecondExplorationToolProof(root.transform, assets);
            CreateRuntimeHook(player.transform, new[] { meleeEnemy.transform, rangedEnemy.transform, firstBoss.transform }, chest.transform, secondChest.transform, node, secondNode, camera);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            Selection.activeObject = camera.gameObject;
            AssetDatabase.SaveAssets();
            Debug.Log($"FOURFOLD D-020 vertical slice scene generated at {ScenePath}");
        }

        public static void ValidateGeneratedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"D-020 vertical slice scene is missing or invalid: {ScenePath}");
            }

            Require("D020 Player");
            Require("D020 Enemy Read Target");
            Require("D020 Enemy Ranged Read Target");
            Require("D020 First Boss");
            Require("D020 Relic Chest");
            Require("D020 Second Relic Chest");
            Require("D020 Hub Return Gate");
            Require("D020 Exploration Tool Node");
            Require("D020 Second Exploration Tool Node");
            Require("D020 Shortcut Locked Barrier");
            Require("D020 Second Route Locked Barrier");
            Require("D020 Shortcut Route");
            Require("D020 Second Gimmick Route");
            Require("D020 Top Down Camera");
            RequireComponent<ExplorationTool>("D020 Runtime Hook");
            RequireComponent<D020SliceController>("D020 Runtime Hook");
            RequireComponent<ExplorationNode>("D020 Exploration Tool Node");
            ValidateRuntimeHookReferences();

            if (Camera.main == null)
            {
                throw new InvalidOperationException("D-020 vertical slice scene has no MainCamera tagged camera.");
            }

            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            if (renderers.Length < 24)
            {
                throw new InvalidOperationException($"D-020 vertical slice has too few renderers for a readable proof scene: {renderers.Length}");
            }

            Debug.Log("FOURFOLD D-020 vertical slice scene validation passed.");
        }

        private static void ValidateRuntimeHookReferences()
        {
            var hook = FindSceneObject("D020 Runtime Hook");
            if (hook == null)
            {
                throw new InvalidOperationException("D-020 runtime hook is missing.");
            }

            var controller = hook.GetComponent<D020SliceController>();
            if (controller == null || controller.player == null || controller.enemies == null || controller.enemies.Length < 3 || controller.rewardClaimPoint == null || controller.secondRewardClaimPoint == null || controller.returnGatePoint == null)
            {
                throw new InvalidOperationException("D-020 playable controller is missing required player, enemies, reward, or return references.");
            }

            if (hook.GetComponent<AudioSource>() == null)
            {
                throw new InvalidOperationException("D-020 runtime hook is missing its shared AudioSource.");
            }

            var tool = hook.GetComponent<ExplorationTool>();
            if (tool == null || tool.player == null || tool.nodes == null || tool.nodes.Length < 2 || tool.nodes[0] == null || tool.nodes[1] == null)
            {
                throw new InvalidOperationException("D-020 exploration tool is missing required player or two node references.");
            }

            if (controller.explorationTool == null || controller.requiredToolNode == null || controller.secondToolNode == null || controller.shortcutLockedRead == null || controller.secondRouteLockedRead == null)
            {
                throw new InvalidOperationException("D-020 playable controller is missing exploration tool gate or lock-read references.");
            }
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
        }

        private static GeneratedAssets CreateGeneratedAssets()
        {
            return new GeneratedAssets
            {
                floor = CreateMaterial("D020_Floor", new Color(0.38f, 0.35f, 0.30f), 0.05f, 0.45f, null),
                floorDark = CreateMaterial("D020_FloorDark", new Color(0.18f, 0.17f, 0.18f), 0.02f, 0.34f, null),
                route = CreateMaterial("D020_RouteGold", new Color(0.93f, 0.68f, 0.26f), 0.02f, 0.45f, new Color(0.42f, 0.21f, 0.04f)),
                player = CreateMaterial("D020_PlayerIvory", new Color(0.92f, 0.78f, 0.52f), 0.06f, 0.43f, null),
                playerCape = CreateMaterial("D020_PlayerCape", new Color(0.18f, 0.22f, 0.62f), 0f, 0.38f, new Color(0.02f, 0.04f, 0.16f)),
                playerAura = CreateMaterial("D020_PlayerAura", new Color(0.22f, 0.66f, 1.0f), 0f, 0.48f, new Color(0.05f, 0.22f, 0.52f)),
                playerBlade = CreateMaterial("D020_PlayerBlade", new Color(0.82f, 0.86f, 0.88f), 0.28f, 0.62f, null),
                enemy = CreateMaterial("D020_EnemyInk", new Color(0.07f, 0.055f, 0.08f), 0.08f, 0.36f, null),
                enemyArmor = CreateMaterial("D020_EnemyArmor", new Color(0.16f, 0.13f, 0.12f), 0.12f, 0.42f, null),
                enemyTell = CreateMaterial("D020_EnemyTell", new Color(0.94f, 0.16f, 0.08f), 0f, 0.5f, new Color(0.65f, 0.06f, 0.02f)),
                chest = CreateMaterial("D020_ChestWood", new Color(0.42f, 0.24f, 0.12f), 0f, 0.42f, null),
                relic = CreateMaterial("D020_RelicBlue", new Color(0.20f, 0.68f, 0.94f), 0f, 0.5f, new Color(0.05f, 0.26f, 0.52f)),
                tool = CreateMaterial("D020_ToolSignal", new Color(0.86f, 0.92f, 0.52f), 0f, 0.56f, new Color(0.54f, 0.45f, 0.08f))
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

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("D020 Top Down Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(13.0f, 16.2f, -13.6f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(1.2f, 0.1f, 0.4f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 11.4f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 90f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            return camera;
        }

        private static void CreateLighting()
        {
            var keyObject = new GameObject("D020 Warm Key Light");
            keyObject.transform.rotation = Quaternion.Euler(54f, -38f, 0f);
            var key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.1f;
            key.color = new Color(1f, 0.78f, 0.55f);
            key.shadows = LightShadows.Soft;

            CreatePointLight("D020 Tool Fill", new Vector3(-3.2f, 2.2f, -1.8f), new Color(0.95f, 0.78f, 0.22f), 2.6f, 6.2f);
            CreatePointLight("D020 Enemy Fill", new Vector3(0.8f, 2.1f, 1.4f), new Color(1.0f, 0.34f, 0.18f), 1.4f, 5.4f);
            CreatePointLight("D020 Reward Fill", new Vector3(4.8f, 2.0f, 3.4f), new Color(0.22f, 0.58f, 1.0f), 2.4f, 5.6f);
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

        private static void CreateField(Transform root, GeneratedAssets assets)
        {
            var field = new GameObject("D020 Continuous Slice Field");
            field.transform.SetParent(root);

            for (var x = -10; x <= 12; x++)
            {
                for (var z = -7; z <= 8; z++)
                {
                    var isEdge = x <= -10 || x >= 12 || z <= -7 || z >= 8;
                    var isMilestone = (x >= -9 && x <= -6 && z >= -6 && z <= -4)
                        || (x >= -6 && x <= -3 && z >= -3 && z <= -1)
                        || (x >= 0 && x <= 3 && z >= -1 && z <= 2)
                        || (x >= 7 && x <= 11 && z >= 4 && z <= 7);
                    var material = isEdge ? assets.floorDark : isMilestone ? assets.route : assets.floor;
                    var height = isMilestone ? -0.055f : -0.08f;
                    CreateBlock(field.transform, $"D020 Field Floor {x},{z}", material, new Vector3(x, height, z), new Vector3(0.96f, 0.16f, 0.96f));
                }
            }

            for (var x = -10; x <= 12; x += 3)
            {
                CreateBlock(field.transform, $"D020 Low North Wall {x}", assets.floorDark, new Vector3(x, 0.34f, 8.55f), new Vector3(1.55f, 0.64f, 0.30f));
                if (x < -5 || x > -2)
                {
                    CreateBlock(field.transform, $"D020 Low South Wall {x}", assets.floorDark, new Vector3(x, 0.34f, -7.55f), new Vector3(1.55f, 0.64f, 0.30f));
                }
            }

            for (var z = -5; z <= 8; z += 3)
            {
                CreateBlock(field.transform, $"D020 Low East Wall {z}", assets.floorDark, new Vector3(12.55f, 0.34f, z), new Vector3(0.30f, 0.64f, 1.55f));
            }

            CreateBlock(field.transform, "D020 Start Threshold", assets.route, new Vector3(-8.0f, 0.025f, -5.65f), new Vector3(2.4f, 0.06f, 0.20f), Quaternion.Euler(0f, 8f, 0f));
            CreateBlock(field.transform, "D020 Tool Route Line A", assets.route, new Vector3(-6.75f, 0.03f, -4.45f), new Vector3(2.0f, 0.06f, 0.18f), Quaternion.Euler(0f, 34f, 0f));
            CreateBlock(field.transform, "D020 Tool Route Line B", assets.route, new Vector3(-5.25f, 0.035f, -3.10f), new Vector3(2.0f, 0.06f, 0.18f), Quaternion.Euler(0f, 38f, 0f));
            CreateBlock(field.transform, "D020 Combat Route Line", assets.route, new Vector3(-1.15f, 0.035f, -0.20f), new Vector3(3.4f, 0.06f, 0.18f), Quaternion.Euler(0f, 25f, 0f));
            CreateBlock(field.transform, "D020 Reward Route Line", assets.route, new Vector3(5.95f, 0.035f, 3.45f), new Vector3(4.2f, 0.06f, 0.18f), Quaternion.Euler(0f, 18f, 0f));
            CreateBlock(field.transform, "D020 Second Room Route Line", assets.route, new Vector3(7.4f, 0.035f, -2.55f), new Vector3(3.4f, 0.06f, 0.18f), Quaternion.Euler(0f, -30f, 0f));
            CreateBlock(field.transform, "D020 Shortcut Locked Barrier", assets.tool, new Vector3(-2.60f, 0.35f, -0.92f), new Vector3(1.56f, 0.70f, 0.18f), Quaternion.Euler(0f, 24f, 0f));
            CreateBlock(field.transform, "D020 Second Route Locked Barrier", assets.tool, new Vector3(10.06f, 0.32f, -5.08f), new Vector3(1.36f, 0.64f, 0.16f), Quaternion.Euler(0f, -18f, 0f));
            CreateBlock(field.transform, "D020 Reward Low Rail A", assets.floorDark, new Vector3(7.2f, 0.28f, 6.85f), new Vector3(2.2f, 0.52f, 0.22f));
            CreateBlock(field.transform, "D020 Reward Low Rail B", assets.floorDark, new Vector3(10.4f, 0.28f, 6.85f), new Vector3(2.2f, 0.52f, 0.22f));
            CreateBlock(field.transform, "D020 Enemy Arena Marker", assets.enemyTell, new Vector3(1.8f, 0.018f, 0.95f), new Vector3(2.8f, 0.04f, 1.55f), Quaternion.Euler(0f, 24f, 0f));
            CreateBlock(field.transform, "D020 Second Gimmick Room Marker", assets.tool, new Vector3(8.0f, 0.018f, -4.0f), new Vector3(2.6f, 0.04f, 1.5f), Quaternion.Euler(0f, -18f, 0f));
            CreateBlock(field.transform, "D020 Tool Target Backplate", assets.floorDark, new Vector3(-5.05f, 0.44f, -2.65f), new Vector3(1.2f, 0.86f, 0.20f));
            CreateBlock(field.transform, "D020 Second Tool Backplate", assets.floorDark, new Vector3(8.30f, 0.44f, -4.25f), new Vector3(1.2f, 0.86f, 0.20f));
            CreateBlock(field.transform, "D020 Midfield Landmark A", assets.floorDark, new Vector3(-1.0f, 0.54f, 2.7f), new Vector3(0.42f, 1.08f, 1.1f));
            CreateBlock(field.transform, "D020 Midfield Landmark B", assets.floorDark, new Vector3(4.4f, 0.54f, -1.3f), new Vector3(1.2f, 1.08f, 0.36f));
        }

        private static GameObject CreatePlayer(Transform root, GeneratedAssets assets)
        {
            var player = new GameObject("D020 Player");
            player.transform.SetParent(root);
            player.transform.position = new Vector3(-8.2f, 0.12f, -5.65f);
            player.transform.rotation = Quaternion.Euler(0f, 42f, 0f);

            CreatePrimitive(player.transform, PrimitiveType.Cylinder, "D020 Player Read Circle", assets.route, new Vector3(0f, 0.025f, 0f), new Vector3(1.18f, 0.035f, 1.18f));
            CreatePrimitive(player.transform, PrimitiveType.Cylinder, "D020 Player Blue Control Ring", assets.playerAura, new Vector3(0f, 0.04f, 0f), new Vector3(1.42f, 0.026f, 1.42f));
            if (TryInstantiateProductionPrefab(FourfoldProductionArtP0Builder.HeroPrefabPath, player.transform, "D020 Player Production Art", Vector3.zero, Quaternion.identity, Vector3.one))
            {
                return player;
            }

            CreateBlock(player.transform, "D020 Player Left Boot", assets.floorDark, new Vector3(-0.18f, 0.18f, 0.12f), new Vector3(0.26f, 0.30f, 0.44f));
            CreateBlock(player.transform, "D020 Player Right Boot", assets.floorDark, new Vector3(0.18f, 0.18f, -0.12f), new Vector3(0.26f, 0.30f, 0.44f));
            CreatePrimitive(player.transform, PrimitiveType.Capsule, "D020 Player Hero Tunic", assets.player, new Vector3(0f, 0.88f, 0f), new Vector3(0.62f, 0.86f, 0.50f));
            CreateBlock(player.transform, "D020 Player Hero Cloak Read", assets.playerCape, new Vector3(-0.18f, 0.86f, -0.34f), new Vector3(0.82f, 1.12f, 0.16f));
            CreatePrimitive(player.transform, PrimitiveType.Sphere, "D020 Player Head Read", assets.player, new Vector3(0f, 1.56f, 0f), new Vector3(0.46f, 0.42f, 0.46f));
            CreateBlock(player.transform, "D020 Player Crest Read", assets.route, new Vector3(0f, 1.94f, -0.02f), new Vector3(0.42f, 0.18f, 0.16f), Quaternion.Euler(0f, 0f, 8f));
            CreateBlock(player.transform, "D020 Player Sword Read", assets.playerBlade, new Vector3(0.46f, 0.98f, 0.40f), new Vector3(0.13f, 1.25f, 0.08f), Quaternion.Euler(34f, 0f, -28f));
            CreateBlock(player.transform, "D020 Player Sword Grip Read", assets.floorDark, new Vector3(0.30f, 0.58f, 0.30f), new Vector3(0.18f, 0.32f, 0.12f), Quaternion.Euler(34f, 0f, -28f));
            CreateBlock(player.transform, "D020 One Tool Held Read", assets.tool, new Vector3(0.68f, 1.04f, -0.16f), new Vector3(0.16f, 1.06f, 0.13f), Quaternion.Euler(0f, 0f, -25f));
            CreatePrimitive(player.transform, PrimitiveType.Sphere, "D020 Tool Hand Glow", assets.tool, new Vector3(0.74f, 1.58f, -0.18f), new Vector3(0.26f, 0.26f, 0.26f));
            return player;
        }

        private static GameObject CreateMeleeEnemy(Transform root, GeneratedAssets assets)
        {
            var enemy = new GameObject("D020 Enemy Read Target");
            enemy.transform.SetParent(root);
            enemy.transform.position = new Vector3(1.85f, 0.12f, 0.95f);
            enemy.transform.rotation = Quaternion.Euler(0f, 200f, 0f);

            if (TryInstantiateProductionPrefab(FourfoldProductionArtP0Builder.EnemyPrefabPath, enemy.transform, "D020 Enemy Production Art", Vector3.zero, Quaternion.identity, Vector3.one))
            {
                return enemy;
            }

            CreatePrimitive(enemy.transform, PrimitiveType.Capsule, "D020 Enemy Heavy Body", assets.enemy, new Vector3(0f, 0.74f, 0f), new Vector3(1.06f, 0.92f, 0.82f));
            CreateBlock(enemy.transform, "D020 Enemy Shoulder Left", assets.enemyArmor, new Vector3(-0.62f, 1.10f, -0.02f), new Vector3(0.42f, 0.28f, 0.52f), Quaternion.Euler(0f, 0f, 18f));
            CreateBlock(enemy.transform, "D020 Enemy Shoulder Right", assets.enemyArmor, new Vector3(0.62f, 1.10f, -0.02f), new Vector3(0.42f, 0.28f, 0.52f), Quaternion.Euler(0f, 0f, -18f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Enemy Tell Core", assets.enemyTell, new Vector3(0f, 1.24f, -0.36f), new Vector3(0.38f, 0.34f, 0.20f));
            CreateBlock(enemy.transform, "D020 Enemy Left Arm", assets.enemyArmor, new Vector3(-0.78f, 0.64f, 0.10f), new Vector3(0.28f, 0.72f, 0.24f), Quaternion.Euler(0f, 0f, 22f));
            CreateBlock(enemy.transform, "D020 Enemy Heavy Club Read", assets.enemyArmor, new Vector3(0.74f, 0.78f, -0.38f), new Vector3(0.24f, 1.25f, 0.18f), Quaternion.Euler(20f, 0f, -34f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Enemy Club Tip Read", assets.enemyTell, new Vector3(1.05f, 1.35f, -0.52f), new Vector3(0.26f, 0.26f, 0.26f));
            CreatePrimitive(enemy.transform, PrimitiveType.Cylinder, "D020 Enemy Melee Danger Read", assets.enemyTell, new Vector3(0f, 0.035f, -1.05f), new Vector3(1.55f, 0.025f, 1.55f));
            return enemy;
        }

        private static GameObject CreateRangedEnemy(Transform root, GeneratedAssets assets)
        {
            var enemy = new GameObject("D020 Enemy Ranged Read Target");
            enemy.transform.SetParent(root);
            enemy.transform.position = new Vector3(5.45f, 0.12f, 2.75f);
            enemy.transform.rotation = Quaternion.Euler(0f, 226f, 0f);

            CreatePrimitive(enemy.transform, PrimitiveType.Cylinder, "D020 Ranged Enemy Base", assets.enemy, new Vector3(0f, 0.40f, 0f), new Vector3(0.62f, 0.74f, 0.62f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Ranged Enemy Head", assets.enemyArmor, new Vector3(0f, 1.10f, 0f), new Vector3(0.42f, 0.36f, 0.42f));
            CreateBlock(enemy.transform, "D020 Ranged Enemy Back Mantle", assets.enemyArmor, new Vector3(-0.10f, 0.78f, 0.24f), new Vector3(0.72f, 0.82f, 0.14f));
            CreateBlock(enemy.transform, "D020 Ranged Enemy Staff", assets.enemyTell, new Vector3(0.54f, 0.92f, -0.08f), new Vector3(0.12f, 1.32f, 0.10f), Quaternion.Euler(0f, 0f, -12f));
            CreateBlock(enemy.transform, "D020 Ranged Enemy Aim Line", assets.enemyTell, new Vector3(-0.70f, 0.13f, -0.78f), new Vector3(1.55f, 0.035f, 0.10f), Quaternion.Euler(0f, 28f, 0f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Ranged Enemy Tell Orb", assets.enemyTell, new Vector3(0.64f, 1.62f, -0.12f), new Vector3(0.24f, 0.24f, 0.24f));
            return enemy;
        }

        private static GameObject CreateFirstBoss(Transform root, GeneratedAssets assets)
        {
            var boss = new GameObject("D020 First Boss");
            boss.transform.SetParent(root);
            boss.transform.position = new Vector3(3.8f, 0.18f, -1.85f);
            boss.transform.rotation = Quaternion.Euler(0f, 206f, 0f);

            CreatePrimitive(boss.transform, PrimitiveType.Cylinder, "D020 Boss Ground Read", assets.enemyTell, new Vector3(0f, 0.035f, 0f), new Vector3(2.35f, 0.026f, 2.35f));
            CreatePrimitive(boss.transform, PrimitiveType.Capsule, "D020 Boss Heavy Core", assets.enemy, new Vector3(0f, 1.02f, 0f), new Vector3(1.55f, 1.34f, 1.18f));
            CreateBlock(boss.transform, "D020 Boss Crown Left", assets.enemyArmor, new Vector3(-0.56f, 1.88f, -0.08f), new Vector3(0.26f, 0.62f, 0.20f), Quaternion.Euler(0f, 0f, -18f));
            CreateBlock(boss.transform, "D020 Boss Crown Right", assets.enemyArmor, new Vector3(0.56f, 1.88f, -0.08f), new Vector3(0.26f, 0.62f, 0.20f), Quaternion.Euler(0f, 0f, 18f));
            CreatePrimitive(boss.transform, PrimitiveType.Sphere, "D020 Boss Exposed Core", assets.enemyTell, new Vector3(0f, 1.30f, -0.56f), new Vector3(0.56f, 0.46f, 0.24f));
            CreateBlock(boss.transform, "D020 Boss Left Arm", assets.enemyArmor, new Vector3(-1.05f, 0.92f, -0.10f), new Vector3(0.36f, 1.08f, 0.28f), Quaternion.Euler(0f, 0f, 22f));
            CreateBlock(boss.transform, "D020 Boss Right Arm", assets.enemyArmor, new Vector3(1.05f, 0.92f, -0.10f), new Vector3(0.36f, 1.08f, 0.28f), Quaternion.Euler(0f, 0f, -22f));
            CreateBlock(boss.transform, "D020 Boss Sweeping Blade", assets.enemyTell, new Vector3(1.30f, 0.58f, -0.76f), new Vector3(0.20f, 1.92f, 0.16f), Quaternion.Euler(18f, 0f, -48f));
            CreateBlock(boss.transform, "D020 Boss Back Banner", assets.floorDark, new Vector3(0f, 1.16f, 0.62f), new Vector3(1.02f, 1.18f, 0.14f));
            return boss;
        }

        private static GameObject CreateChest(Transform root, GeneratedAssets assets)
        {
            var chest = new GameObject("D020 Relic Chest");
            chest.transform.SetParent(root);
            chest.transform.position = new Vector3(9.8f, 0.1f, 6.45f);
            chest.transform.rotation = Quaternion.Euler(0f, -18f, 0f);

            if (TryInstantiateProductionPrefab(FourfoldProductionArtP0Builder.RewardChestPrefabPath, chest.transform, "D020 Reward Production Art", Vector3.zero, Quaternion.identity, Vector3.one))
            {
                return chest;
            }

            CreateBlock(chest.transform, "D020 Chest Base", assets.chest, Vector3.zero, new Vector3(1.02f, 0.48f, 0.72f));
            CreateBlock(chest.transform, "D020 Chest Lid", assets.route, new Vector3(0f, 0.39f, 0f), new Vector3(1.08f, 0.17f, 0.78f));
            CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Visible Relic", assets.relic, new Vector3(0f, 0.88f, 0f), new Vector3(0.34f, 0.46f, 0.34f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Reward Footprint", assets.relic, new Vector3(0f, 0.03f, 0f), new Vector3(1.24f, 0.026f, 1.24f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Reward Vertical Beam", assets.relic, new Vector3(0f, 1.86f, 0f), new Vector3(0.12f, 1.35f, 0.12f));
            CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Reward Beacon", assets.relic, new Vector3(0f, 3.18f, 0f), new Vector3(0.22f, 0.34f, 0.22f));
            return chest;
        }

        private static GameObject CreateSecondChest(Transform root, GeneratedAssets assets)
        {
            var chest = new GameObject("D020 Second Relic Chest");
            chest.transform.SetParent(root);
            chest.transform.position = new Vector3(10.7f, 0.1f, -5.35f);
            chest.transform.rotation = Quaternion.Euler(0f, 28f, 0f);

            CreateBlock(chest.transform, "D020 Second Chest Base", assets.chest, Vector3.zero, new Vector3(0.92f, 0.42f, 0.66f));
            CreateBlock(chest.transform, "D020 Second Chest Lid", assets.route, new Vector3(0f, 0.35f, 0f), new Vector3(0.98f, 0.15f, 0.72f));
            CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Second Visible Relic", assets.relic, new Vector3(0f, 0.78f, 0f), new Vector3(0.28f, 0.38f, 0.28f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Second Reward Footprint", assets.relic, new Vector3(0f, 0.03f, 0f), new Vector3(1.02f, 0.026f, 1.02f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Second Reward Beam", assets.relic, new Vector3(0f, 1.48f, 0f), new Vector3(0.10f, 1.0f, 0.10f));
            CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Second Reward Beacon", assets.relic, new Vector3(0f, 2.48f, 0f), new Vector3(0.18f, 0.28f, 0.18f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Hub Return Gate", assets.route, new Vector3(0f, 0.06f, 0f), new Vector3(1.45f, 0.03f, 1.45f));
            var returnBeacon = CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Return Gate Beacon", assets.route, new Vector3(0f, 1.10f, 0.42f), new Vector3(0.20f, 0.20f, 0.20f));
            returnBeacon.SetActive(false);
            return chest;
        }

        private static ExplorationNode CreateExplorationToolProof(Transform root, GeneratedAssets assets)
        {
            var proof = new GameObject("D020 One Tool Proof");
            proof.transform.SetParent(root);

            var response = new GameObject("D020 Shortcut Route");
            response.transform.SetParent(proof.transform);
            response.transform.position = Vector3.zero;
            CreateBlock(response.transform, "D020 Shortcut Slab A", assets.route, new Vector3(-8.9f, 0.05f, -1.10f), new Vector3(1.02f, 0.07f, 0.38f), Quaternion.Euler(0f, 17f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab B", assets.route, new Vector3(-7.95f, 0.06f, -0.58f), new Vector3(1.02f, 0.07f, 0.38f), Quaternion.Euler(0f, -14f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab C", assets.route, new Vector3(-7.02f, 0.07f, -0.02f), new Vector3(1.02f, 0.07f, 0.38f), Quaternion.Euler(0f, 16f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab D", assets.route, new Vector3(-6.10f, 0.08f, 0.56f), new Vector3(0.96f, 0.07f, 0.34f), Quaternion.Euler(0f, -8f, 0f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Shortcut Open Spark A", assets.tool, new Vector3(-7.95f, 0.48f, -0.58f), new Vector3(0.24f, 0.24f, 0.24f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Shortcut Open Spark B", assets.tool, new Vector3(-6.10f, 0.54f, 0.56f), new Vector3(0.22f, 0.22f, 0.22f));
            CreateBlock(response.transform, "D020 Shortcut Direction Beam", assets.tool, new Vector3(-6.80f, 0.14f, 0.20f), new Vector3(2.10f, 0.045f, 0.09f), Quaternion.Euler(0f, 32f, 0f));

            var nodeObject = new GameObject("D020 Exploration Tool Node");
            nodeObject.transform.SetParent(proof.transform);
            nodeObject.transform.position = new Vector3(-5.05f, 0.1f, -2.85f);
            var footprint = CreatePrimitive(nodeObject.transform, PrimitiveType.Cylinder, "D020 Tool Node Footprint", assets.tool, Vector3.zero, new Vector3(0.92f, 0.026f, 0.92f));
            if (!TryInstantiateProductionPrefab(FourfoldProductionArtP0Builder.PedestalPrefabPath, nodeObject.transform, "D020 Tool Node Production Art", Vector3.zero, Quaternion.identity, Vector3.one))
            {
                CreateBlock(nodeObject.transform, "D020 Tool Node Pedestal", assets.floorDark, new Vector3(0f, 0.24f, 0f), new Vector3(0.66f, 0.44f, 0.66f));
                CreateBlock(nodeObject.transform, "D020 Tool Node Signal A", assets.tool, new Vector3(-0.10f, 0.68f, -0.04f), new Vector3(0.16f, 0.62f, 0.09f), Quaternion.Euler(0f, 0f, 45f));
                CreateBlock(nodeObject.transform, "D020 Tool Node Signal B", assets.tool, new Vector3(0.16f, 0.68f, -0.04f), new Vector3(0.16f, 0.62f, 0.09f), Quaternion.Euler(0f, 0f, -45f));
            }
            var activeRead = CreatePrimitive(nodeObject.transform, PrimitiveType.Sphere, "D020 Tool Node Active Read", assets.relic, new Vector3(0f, 1.02f, -0.03f), new Vector3(0.24f, 0.24f, 0.24f));
            activeRead.SetActive(false);
            response.SetActive(false);

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.activationRadius = 2.6f;
            node.responseTarget = response;
            node.idleRead = footprint;
            node.activeRead = activeRead;
            node.highlightRenderers = response.GetComponentsInChildren<Renderer>(true);
            node.ResetNode();
            return node;
        }

        private static ExplorationNode CreateSecondExplorationToolProof(Transform root, GeneratedAssets assets)
        {
            var proof = new GameObject("D020 Second Tool Proof");
            proof.transform.SetParent(root);

            var response = new GameObject("D020 Second Gimmick Route");
            response.transform.SetParent(proof.transform);
            response.transform.position = Vector3.zero;
            CreateBlock(response.transform, "D020 Second Bridge Slab A", assets.route, new Vector3(8.85f, 0.08f, -4.75f), new Vector3(0.96f, 0.07f, 0.34f), Quaternion.Euler(0f, -24f, 0f));
            CreateBlock(response.transform, "D020 Second Bridge Slab B", assets.route, new Vector3(9.72f, 0.09f, -5.05f), new Vector3(0.96f, 0.07f, 0.34f), Quaternion.Euler(0f, 12f, 0f));
            CreateBlock(response.transform, "D020 Second Bridge Beam", assets.tool, new Vector3(9.28f, 0.18f, -4.90f), new Vector3(1.55f, 0.045f, 0.09f), Quaternion.Euler(0f, -15f, 0f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Second Open Spark", assets.tool, new Vector3(9.72f, 0.55f, -5.05f), new Vector3(0.22f, 0.22f, 0.22f));

            var nodeObject = new GameObject("D020 Second Exploration Tool Node");
            nodeObject.transform.SetParent(proof.transform);
            nodeObject.transform.position = new Vector3(8.30f, 0.1f, -4.35f);
            var footprint = CreatePrimitive(nodeObject.transform, PrimitiveType.Cylinder, "D020 Second Tool Node Footprint", assets.tool, Vector3.zero, new Vector3(0.82f, 0.026f, 0.82f));
            CreateBlock(nodeObject.transform, "D020 Second Tool Node Pedestal", assets.floorDark, new Vector3(0f, 0.22f, 0f), new Vector3(0.58f, 0.38f, 0.58f));
            CreateBlock(nodeObject.transform, "D020 Second Tool Node Signal", assets.tool, new Vector3(0f, 0.64f, -0.04f), new Vector3(0.14f, 0.56f, 0.09f), Quaternion.Euler(0f, 0f, 32f));
            var activeRead = CreatePrimitive(nodeObject.transform, PrimitiveType.Sphere, "D020 Second Tool Node Active Read", assets.relic, new Vector3(0f, 0.92f, -0.03f), new Vector3(0.22f, 0.22f, 0.22f));
            activeRead.SetActive(false);
            response.SetActive(false);

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.activationRadius = 2.5f;
            node.responseTarget = response;
            node.idleRead = footprint;
            node.activeRead = activeRead;
            node.highlightRenderers = response.GetComponentsInChildren<Renderer>(true);
            node.ResetNode();
            return node;
        }

        private static void CreateRuntimeHook(Transform player, Transform[] enemies, Transform rewardClaimPoint, Transform secondRewardClaimPoint, ExplorationNode node, ExplorationNode secondNode, Camera camera)
        {
            var hookObject = new GameObject("D020 Runtime Hook");
            var audioSource = hookObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = 0.85f;
            var musicSource = hookObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.dopplerLevel = 0f;
            musicSource.volume = 0.24f;

            var tool = hookObject.AddComponent<ExplorationTool>();
            tool.player = player;
            tool.nodes = new[] { node, secondNode };
            tool.range = 2.8f;
            tool.cooldownSeconds = 0.42f;
            tool.useKey = KeyCode.Q;
            tool.alternateUseKey = KeyCode.JoystickButton2;
            tool.pulseRead = node.idleRead;
            tool.pulse = LoadOptionalAudioClip(ToolPulseClipPath);
            tool.targetHit = LoadOptionalAudioClip(ToolTargetHitClipPath);

            var controller = hookObject.AddComponent<D020SliceController>();
            controller.player = player;
            controller.enemies = enemies;
            controller.explorationTool = tool;
            controller.requiredToolNode = node;
            controller.shortcutLockedRead = FindSceneObject("D020 Shortcut Locked Barrier");
            controller.secondToolNode = secondNode;
            controller.secondRouteLockedRead = FindSceneObject("D020 Second Route Locked Barrier");
            controller.rewardReadyRead = FindInChildren(rewardClaimPoint, "D020 Reward Beacon")?.gameObject
                ?? FindInChildren(rewardClaimPoint, "FE_RELIC_SPARK_P0")?.gameObject;
            controller.rewardClaimPoint = rewardClaimPoint;
            controller.secondRewardReadyRead = FindInChildren(secondRewardClaimPoint, "D020 Second Reward Beacon")?.gameObject;
            controller.secondRewardClaimPoint = secondRewardClaimPoint;
            controller.returnReadyRead = FindInChildren(secondRewardClaimPoint, "D020 Return Gate Beacon")?.gameObject;
            controller.returnGatePoint = secondRewardClaimPoint;
            controller.fixedCamera = camera;
            controller.audioSource = audioSource;
            controller.musicSource = musicSource;
            controller.attackClip = LoadOptionalAudioClip(AttackClipPath);
            controller.hitClip = LoadOptionalAudioClip(HitClipPath);
            controller.dodgeClip = LoadOptionalAudioClip(DodgeClipPath);
            controller.rewardClaimClip = LoadOptionalAudioClip(RewardClaimClipPath);
            controller.rewardReadyClip = LoadOptionalAudioClip(RewardReadyClipPath);
            controller.explorationMusicClip = LoadOptionalAudioClip(ExplorationMusicPath);
            controller.bossMusicClip = LoadOptionalAudioClip(BossMusicPath);
        }

        private static AudioClip LoadOptionalAudioClip(string path)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"D-020 optional audio clip is missing and will be skipped: {path}");
            }

            return clip;
        }

        private static GameObject CreateBlock(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            return CreateBlock(parent, name, material, localPosition, localScale, Quaternion.identity);
        }

        private static GameObject CreateBlock(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            return CreatePrimitive(parent, PrimitiveType.Cube, name, material, localPosition, localScale, localRotation);
        }

        private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            return CreatePrimitive(parent, type, name, material, localPosition, localScale, Quaternion.identity);
        }

        private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string name, Material material, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = localRotation;
            gameObject.transform.localScale = localScale;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;
            return gameObject;
        }

        private static bool TryInstantiateProductionPrefab(string prefabPath, Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return false;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            return true;
        }

        private static void Require(string name)
        {
            if (FindSceneObject(name) == null)
            {
                throw new InvalidOperationException($"Required object missing from D-020 vertical slice scene: {name}");
            }
        }

        private static void RequireComponent<T>(string name) where T : Component
        {
            var gameObject = FindSceneObject(name);
            if (gameObject == null || gameObject.GetComponent<T>() == null)
            {
                throw new InvalidOperationException($"Required component {typeof(T).Name} missing on object: {name}");
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

        private sealed class GeneratedAssets
        {
            public Material floor;
            public Material floorDark;
            public Material route;
            public Material player;
            public Material playerCape;
            public Material playerAura;
            public Material playerBlade;
            public Material enemy;
            public Material enemyArmor;
            public Material enemyTell;
            public Material chest;
            public Material relic;
            public Material tool;
        }
    }
}
