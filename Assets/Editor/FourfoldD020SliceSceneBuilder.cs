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

        public static void BuildAndValidate()
        {
            Build();
            ValidateGeneratedScene();
        }

        public static void Build()
        {
            EnsureFolders();
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
            var root = new GameObject("D020 Slice World");
            CreateRoom(root.transform, assets);
            var player = CreatePlayer(root.transform, assets);
            CreateEnemy(root.transform, assets, player.transform);
            CreateChest(root.transform, assets, player.transform);
            var shortcutNode = CreateExplorationToolProof(root.transform, assets);
            var secondNode = CreateSecondGimmickProof(root.transform, assets, player.transform);
            CreateRuntimeHook(player.transform, new[] { shortcutNode, secondNode }, assets);

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
            Require("D020 Relic Chest");
            Require("D020 Exploration Tool Node");
            Require("D020 Shortcut Route");
            Require("D020 Second Gimmick Room");
            Require("D020 Second Tool Node");
            Require("D020 Second Reward Route");
            Require("D020 Second Relic Chest");
            Require("D020 Top Down Camera");
            RequireComponent<ExplorationTool>("D020 Runtime Hook");
            RequireComponent<D020ProgressSave>("D020 Runtime Hook");
            RequireComponent<ExplorationNode>("D020 Exploration Tool Node");
            RequireComponent<ExplorationNode>("D020 Second Tool Node");
            RequireComponent<D020PlayerController>("D020 Player");
            RequireComponent<D020EnemyDummy>("D020 Enemy Read Target");
            RequireComponent<D020RelicReward>("D020 Relic Chest");
            RequireComponent<D020RelicReward>("D020 Second Relic Chest");
            var tool = FindSceneObject("D020 Runtime Hook").GetComponent<ExplorationTool>();
            var progressSave = FindSceneObject("D020 Runtime Hook").GetComponent<D020ProgressSave>();
            var player = FindSceneObject("D020 Player").GetComponent<D020PlayerController>();
            var firstReward = FindSceneObject("D020 Relic Chest").GetComponent<D020RelicReward>();
            var secondReward = FindSceneObject("D020 Second Relic Chest").GetComponent<D020RelicReward>();
            if (tool.NodeCount < 2)
            {
                throw new InvalidOperationException("D-020 runtime hook must reference two exploration nodes for the two-gimmick-room proof.");
            }
            if (progressSave.nodes == null || progressSave.nodes.Length < 2)
            {
                throw new InvalidOperationException("D-020 progress save must reference the two exploration nodes.");
            }

            RequireAudioClip(player.attackClip, "D-020 player attack SFX");
            RequireAudioClip(player.hitClip, "D-020 player hit-confirm SFX");
            RequireAudioClip(player.enemyDefeatClip, "D-020 enemy defeat SFX");
            RequireAudioClip(player.dodgeClip, "D-020 player dodge SFX");
            RequireAudioClip(tool.pulse, "D-020 exploration tool pulse SFX");
            RequireAudioClip(tool.targetHit, "D-020 exploration tool target-hit SFX");
            RequireAudioClip(tool.fail, "D-020 exploration tool fail SFX");
            RequireAudioClip(firstReward.pickupClip, "D-020 first relic pickup SFX");
            RequireAudioClip(secondReward.pickupClip, "D-020 second relic pickup SFX");

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
                playerCape = CreateMaterial("D020_PlayerCape", new Color(0.40f, 0.14f, 0.28f), 0f, 0.38f, null),
                enemy = CreateMaterial("D020_EnemyInk", new Color(0.07f, 0.055f, 0.08f), 0.08f, 0.36f, null),
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
            cameraObject.transform.position = new Vector3(8.8f, 11.2f, -8.6f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(3.3f, 0.35f, -0.05f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.0f;
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

            CreatePointLight("D020 Tool Fill", new Vector3(-1.6f, 2.1f, -1.0f), new Color(0.95f, 0.78f, 0.22f), 2.2f, 5.5f);
            CreatePointLight("D020 Reward Fill", new Vector3(3.2f, 1.8f, -1.9f), new Color(0.22f, 0.58f, 1.0f), 2.0f, 4.8f);
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

        private static void CreateRoom(Transform root, GeneratedAssets assets)
        {
            var room = new GameObject("D020 Readable Room");
            room.transform.SetParent(root);

            for (var x = -4; x <= 4; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    var material = Mathf.Abs(x) == 4 || Mathf.Abs(z) == 3 ? assets.floorDark : assets.floor;
                    CreateBlock(room.transform, $"D020 Floor {x},{z}", material, new Vector3(x, -0.08f, z), new Vector3(0.96f, 0.16f, 0.96f));
                }
            }

            for (var x = -4; x <= 4; x += 2)
            {
                CreateBlock(room.transform, $"D020 Low North Wall {x}", assets.floorDark, new Vector3(x, 0.38f, 3.45f), new Vector3(1.15f, 0.72f, 0.30f));
                CreateBlock(room.transform, $"D020 Low South Wall {x}", assets.floorDark, new Vector3(x, 0.38f, -3.45f), new Vector3(1.15f, 0.72f, 0.30f));
            }

            CreateBlock(room.transform, "D020 Shortcut Gap Left", assets.floorDark, new Vector3(-4.55f, 0.28f, 1.15f), new Vector3(0.28f, 0.56f, 1.1f));
            CreateBlock(room.transform, "D020 Shortcut Gap Right", assets.floorDark, new Vector3(-4.55f, 0.28f, -1.15f), new Vector3(0.28f, 0.56f, 1.1f));
            CreateBlock(room.transform, "D020 Main Path Line A", assets.route, new Vector3(-1.2f, 0.025f, -1.6f), new Vector3(1.2f, 0.05f, 0.14f), Quaternion.Euler(0f, 16f, 0f));
            CreateBlock(room.transform, "D020 Main Path Line B", assets.route, new Vector3(0.2f, 0.025f, -1.25f), new Vector3(1.2f, 0.05f, 0.14f), Quaternion.Euler(0f, -8f, 0f));

            CreateBlock(room.transform, "D020 Room Link", assets.route, new Vector3(4.55f, 0.02f, -0.15f), new Vector3(1.4f, 0.05f, 0.24f), Quaternion.Euler(0f, 8f, 0f));

            var secondRoom = new GameObject("D020 Second Gimmick Room");
            secondRoom.transform.SetParent(root);
            for (var x = 5; x <= 10; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    var edge = x == 10 || Mathf.Abs(z) == 3;
                    var material = edge ? assets.floorDark : assets.floor;
                    CreateBlock(secondRoom.transform, $"D020 Second Floor {x},{z}", material, new Vector3(x, -0.075f, z), new Vector3(0.94f, 0.15f, 0.94f));
                }
            }

            for (var z = -2; z <= 2; z += 2)
            {
                CreateBlock(secondRoom.transform, $"D020 Second Low East Wall {z}", assets.floorDark, new Vector3(10.48f, 0.32f, z), new Vector3(0.28f, 0.64f, 1.15f));
            }

            CreateBlock(secondRoom.transform, "D020 Second Entry Marker", assets.tool, new Vector3(5.25f, 0.045f, -0.1f), new Vector3(0.72f, 0.055f, 0.18f), Quaternion.Euler(0f, -14f, 0f));
            CreateBlock(secondRoom.transform, "D020 Second Hidden Path Hint A", assets.floorDark, new Vector3(7.2f, 0.035f, 1.25f), new Vector3(0.7f, 0.05f, 0.12f), Quaternion.Euler(0f, 35f, 0f));
            CreateBlock(secondRoom.transform, "D020 Second Hidden Path Hint B", assets.floorDark, new Vector3(8.05f, 0.035f, 1.58f), new Vector3(0.7f, 0.05f, 0.12f), Quaternion.Euler(0f, 18f, 0f));
        }

        private static GameObject CreatePlayer(Transform root, GeneratedAssets assets)
        {
            var player = new GameObject("D020 Player");
            player.transform.SetParent(root);
            player.transform.position = new Vector3(0f, 0.12f, -2.25f);
            player.transform.rotation = Quaternion.Euler(0f, 32f, 0f);

            CreatePrimitive(player.transform, PrimitiveType.Cylinder, "D020 Player Read Circle", assets.route, new Vector3(0f, 0.025f, 0f), new Vector3(0.86f, 0.035f, 0.86f));
            CreateBlock(player.transform, "D020 Player Left Boot", assets.floorDark, new Vector3(-0.16f, 0.16f, 0.08f), new Vector3(0.20f, 0.22f, 0.34f));
            CreateBlock(player.transform, "D020 Player Right Boot", assets.floorDark, new Vector3(0.16f, 0.16f, 0.08f), new Vector3(0.20f, 0.22f, 0.34f));
            CreateBlock(player.transform, "D020 Player Tunic Block", assets.player, new Vector3(0f, 0.62f, 0f), new Vector3(0.48f, 0.76f, 0.38f));
            CreateBlock(player.transform, "D020 Player Shoulder Bar", assets.route, new Vector3(0f, 0.98f, 0.02f), new Vector3(0.68f, 0.12f, 0.34f));
            CreatePrimitive(player.transform, PrimitiveType.Sphere, "D020 Player Hood Head", assets.player, new Vector3(0f, 1.32f, 0.04f), new Vector3(0.38f, 0.34f, 0.38f));
            CreateBlock(player.transform, "D020 Player Facing Crest", assets.route, new Vector3(0f, 1.35f, 0.30f), new Vector3(0.14f, 0.16f, 0.16f), Quaternion.Euler(18f, 0f, 0f));
            CreateBlock(player.transform, "D020 Player Cape Panel", assets.playerCape, new Vector3(0f, 0.70f, -0.25f), new Vector3(0.62f, 0.94f, 0.10f), Quaternion.Euler(8f, 0f, 0f));
            CreateBlock(player.transform, "D020 Player Tool Shaft", assets.tool, new Vector3(0.50f, 0.82f, 0.05f), new Vector3(0.13f, 0.98f, 0.10f), Quaternion.Euler(0f, 0f, -28f));
            CreatePrimitive(player.transform, PrimitiveType.Sphere, "D020 Player Tool Core", assets.relic, new Vector3(0.68f, 1.26f, 0.10f), new Vector3(0.22f, 0.22f, 0.22f));
            var attackRead = CreatePrimitive(player.transform, PrimitiveType.Cylinder, "D020 Player Attack Read", assets.tool, new Vector3(0f, 0.045f, 0.92f), new Vector3(0.98f, 0.022f, 0.52f));
            attackRead.SetActive(false);

            var audioSource = player.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.76f;
            var controller = player.AddComponent<D020PlayerController>();
            controller.attackRead = attackRead;
            controller.attackRange = 1.45f;
            controller.moveSpeed = 3.35f;
            controller.minBounds = new Vector2(-4.1f, -2.8f);
            controller.maxBounds = new Vector2(10.3f, 2.8f);
            controller.attackClip = LoadAudioClip("attack_basic.wav");
            controller.hitClip = LoadAudioClip("hit_enemy.wav");
            controller.enemyDefeatClip = LoadAudioClip("enemy_death.wav");
            controller.dodgeClip = LoadAudioClip("dodge.wav");
            return player;
        }

        private static void CreateEnemy(Transform root, GeneratedAssets assets, Transform player)
        {
            var enemy = new GameObject("D020 Enemy Read Target");
            enemy.transform.SetParent(root);
            enemy.transform.position = new Vector3(1.65f, 0.12f, 0.35f);
            enemy.transform.rotation = Quaternion.Euler(0f, 200f, 0f);

            CreatePrimitive(enemy.transform, PrimitiveType.Capsule, "D020 Enemy Heavy Body", assets.enemy, new Vector3(0f, 0.70f, 0f), new Vector3(0.82f, 0.78f, 0.70f));
            CreateBlock(enemy.transform, "D020 Enemy Mask Plate", assets.floorDark, new Vector3(0f, 1.08f, -0.24f), new Vector3(0.58f, 0.28f, 0.12f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Enemy Tell Core", assets.enemyTell, new Vector3(0f, 1.12f, -0.32f), new Vector3(0.24f, 0.24f, 0.14f));
            CreateBlock(enemy.transform, "D020 Enemy Left Horn", assets.enemyTell, new Vector3(-0.34f, 1.34f, -0.04f), new Vector3(0.12f, 0.42f, 0.12f), Quaternion.Euler(0f, 0f, 28f));
            CreateBlock(enemy.transform, "D020 Enemy Right Horn", assets.enemyTell, new Vector3(0.34f, 1.34f, -0.04f), new Vector3(0.12f, 0.42f, 0.12f), Quaternion.Euler(0f, 0f, -28f));
            CreateBlock(enemy.transform, "D020 Enemy Left Claw", assets.enemy, new Vector3(-0.68f, 0.70f, -0.10f), new Vector3(0.24f, 0.68f, 0.20f), Quaternion.Euler(0f, 0f, 22f));
            CreateBlock(enemy.transform, "D020 Enemy Right Claw", assets.enemy, new Vector3(0.68f, 0.70f, -0.10f), new Vector3(0.24f, 0.68f, 0.20f), Quaternion.Euler(0f, 0f, -22f));
            CreateBlock(enemy.transform, "D020 Enemy Forward Fang", assets.enemyTell, new Vector3(0f, 0.68f, -0.48f), new Vector3(0.20f, 0.38f, 0.12f), Quaternion.Euler(-16f, 0f, 0f));
            var tellRead = CreatePrimitive(enemy.transform, PrimitiveType.Cylinder, "D020 Enemy Attack Read", assets.enemyTell, new Vector3(0f, 0.035f, -0.9f), new Vector3(0.95f, 0.025f, 0.95f));
            var defeatedRead = CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Enemy Defeated Shard", assets.route, new Vector3(0f, 0.24f, 0f), new Vector3(0.42f, 0.20f, 0.42f));
            defeatedRead.SetActive(false);

            var dummy = enemy.AddComponent<D020EnemyDummy>();
            dummy.target = player;
            dummy.tellRead = tellRead;
            dummy.defeatedRead = defeatedRead;
            dummy.maxHealth = 3;
            dummy.slowChaseSpeed = 0.45f;
        }

        private static D020RelicReward CreateChest(Transform root, GeneratedAssets assets, Transform player)
        {
            var chest = new GameObject("D020 Relic Chest");
            chest.transform.SetParent(root);
            chest.transform.position = new Vector3(3.05f, 0.1f, -1.55f);
            chest.transform.rotation = Quaternion.Euler(0f, -18f, 0f);

            CreateBlock(chest.transform, "D020 Chest Base", assets.chest, Vector3.zero, new Vector3(0.78f, 0.42f, 0.58f));
            CreateBlock(chest.transform, "D020 Chest Lid", assets.route, new Vector3(0f, 0.33f, 0f), new Vector3(0.82f, 0.15f, 0.62f));
            CreateBlock(chest.transform, "D020 Chest Front Band", assets.route, new Vector3(0f, 0.20f, -0.31f), new Vector3(0.54f, 0.10f, 0.08f));
            var visibleRelic = CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Visible Relic", assets.relic, new Vector3(0f, 0.76f, 0f), new Vector3(0.30f, 0.40f, 0.30f));
            CreateBlock(chest.transform, "D020 Reward Vertical Beacon", assets.relic, new Vector3(0f, 1.10f, 0f), new Vector3(0.10f, 0.62f, 0.10f), Quaternion.Euler(0f, 0f, 45f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Reward Footprint", assets.relic, new Vector3(0f, 0.03f, 0f), new Vector3(0.92f, 0.026f, 0.92f));
            var collectedRead = CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Relic Collected Spark", assets.route, new Vector3(0f, 0.92f, 0f), new Vector3(0.42f, 0.42f, 0.42f));
            collectedRead.SetActive(false);

            var reward = chest.AddComponent<D020RelicReward>();
            reward.rewardId = "reward.d020.relic.first";
            reward.player = player;
            reward.idleRead = visibleRelic;
            reward.collectedRead = collectedRead;
            reward.pickupClip = LoadAudioClip("relic_pickup.wav");
            return reward;
        }

        private static ExplorationNode CreateExplorationToolProof(Transform root, GeneratedAssets assets)
        {
            var proof = new GameObject("D020 One Tool Proof");
            proof.transform.SetParent(root);

            var response = new GameObject("D020 Shortcut Route");
            response.transform.SetParent(proof.transform);
            response.transform.position = Vector3.zero;
            CreateBlock(response.transform, "D020 Shortcut Slab A", assets.route, new Vector3(-3.95f, 0.05f, -0.65f), new Vector3(0.82f, 0.07f, 0.34f), Quaternion.Euler(0f, 17f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab B", assets.route, new Vector3(-3.42f, 0.06f, -0.18f), new Vector3(0.82f, 0.07f, 0.34f), Quaternion.Euler(0f, -14f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab C", assets.route, new Vector3(-2.86f, 0.07f, 0.24f), new Vector3(0.82f, 0.07f, 0.34f), Quaternion.Euler(0f, 16f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab D", assets.route, new Vector3(-2.3f, 0.08f, 0.62f), new Vector3(0.78f, 0.07f, 0.30f), Quaternion.Euler(0f, -8f, 0f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Shortcut Open Spark A", assets.tool, new Vector3(-3.55f, 0.42f, -0.2f), new Vector3(0.20f, 0.20f, 0.20f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Shortcut Open Spark B", assets.tool, new Vector3(-2.45f, 0.48f, 0.58f), new Vector3(0.18f, 0.18f, 0.18f));
            CreateBlock(response.transform, "D020 Shortcut Direction Beam", assets.tool, new Vector3(-2.62f, 0.12f, 0.38f), new Vector3(1.35f, 0.045f, 0.08f), Quaternion.Euler(0f, 32f, 0f));

            var nodeObject = new GameObject("D020 Exploration Tool Node");
            nodeObject.transform.SetParent(proof.transform);
            nodeObject.transform.position = new Vector3(-1.2f, 0.1f, -1.85f);
            var footprint = CreatePrimitive(nodeObject.transform, PrimitiveType.Cylinder, "D020 Tool Node Footprint", assets.tool, Vector3.zero, new Vector3(0.64f, 0.026f, 0.64f));
            CreateBlock(nodeObject.transform, "D020 Tool Node Pedestal", assets.floorDark, new Vector3(0f, 0.2f, 0f), new Vector3(0.48f, 0.36f, 0.48f));
            CreateBlock(nodeObject.transform, "D020 Tool Node Left Fork", assets.tool, new Vector3(-0.18f, 0.62f, -0.03f), new Vector3(0.11f, 0.54f, 0.08f), Quaternion.Euler(0f, 0f, -18f));
            CreateBlock(nodeObject.transform, "D020 Tool Node Right Fork", assets.tool, new Vector3(0.18f, 0.62f, -0.03f), new Vector3(0.11f, 0.54f, 0.08f), Quaternion.Euler(0f, 0f, 18f));
            CreatePrimitive(nodeObject.transform, PrimitiveType.Sphere, "D020 Tool Node Idle Lens", assets.tool, new Vector3(0f, 0.88f, -0.03f), new Vector3(0.20f, 0.20f, 0.20f));
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

        private static ExplorationNode CreateSecondGimmickProof(Transform root, GeneratedAssets assets, Transform player)
        {
            var proof = new GameObject("D020 Second Tool Proof");
            proof.transform.SetParent(root);

            var response = new GameObject("D020 Second Reward Route");
            response.transform.SetParent(proof.transform);
            response.transform.position = Vector3.zero;
            CreateBlock(response.transform, "D020 Second Reward Slab A", assets.route, new Vector3(7.28f, 0.07f, 1.18f), new Vector3(0.78f, 0.07f, 0.32f), Quaternion.Euler(0f, 22f, 0f));
            CreateBlock(response.transform, "D020 Second Reward Slab B", assets.route, new Vector3(8.02f, 0.08f, 1.46f), new Vector3(0.78f, 0.07f, 0.32f), Quaternion.Euler(0f, 14f, 0f));
            CreateBlock(response.transform, "D020 Second Reward Slab C", assets.route, new Vector3(8.76f, 0.09f, 1.65f), new Vector3(0.78f, 0.07f, 0.32f), Quaternion.Euler(0f, 5f, 0f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Second Route Spark A", assets.tool, new Vector3(7.68f, 0.44f, 1.32f), new Vector3(0.18f, 0.18f, 0.18f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Second Route Spark B", assets.tool, new Vector3(8.74f, 0.5f, 1.64f), new Vector3(0.18f, 0.18f, 0.18f));

            var chest = new GameObject("D020 Second Relic Chest");
            chest.transform.SetParent(response.transform);
            chest.transform.position = new Vector3(9.45f, 0.14f, 1.8f);
            chest.transform.rotation = Quaternion.Euler(0f, -36f, 0f);
            CreateBlock(chest.transform, "D020 Second Chest Base", assets.chest, Vector3.zero, new Vector3(0.70f, 0.38f, 0.52f));
            CreateBlock(chest.transform, "D020 Second Chest Lid", assets.route, new Vector3(0f, 0.31f, 0f), new Vector3(0.74f, 0.13f, 0.56f));
            CreateBlock(chest.transform, "D020 Second Chest Front Band", assets.route, new Vector3(0f, 0.18f, -0.28f), new Vector3(0.48f, 0.09f, 0.07f));
            var visibleRelic = CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Second Visible Relic", assets.relic, new Vector3(0f, 0.70f, 0f), new Vector3(0.25f, 0.36f, 0.25f));
            CreateBlock(chest.transform, "D020 Second Reward Beacon", assets.relic, new Vector3(0f, 1.02f, 0f), new Vector3(0.09f, 0.50f, 0.09f), Quaternion.Euler(0f, 0f, 45f));
            var collectedRead = CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Second Relic Collected Spark", assets.route, new Vector3(0f, 0.84f, 0f), new Vector3(0.36f, 0.36f, 0.36f));
            collectedRead.SetActive(false);
            var reward = chest.AddComponent<D020RelicReward>();
            reward.rewardId = "reward.d020.relic.second";
            reward.player = player;
            reward.idleRead = visibleRelic;
            reward.collectedRead = collectedRead;
            reward.pickupClip = LoadAudioClip("relic_pickup.wav");

            var nodeObject = new GameObject("D020 Second Tool Node");
            nodeObject.transform.SetParent(proof.transform);
            nodeObject.transform.position = new Vector3(6.85f, 0.1f, -1.7f);
            var footprint = CreatePrimitive(nodeObject.transform, PrimitiveType.Cylinder, "D020 Second Node Footprint", assets.tool, Vector3.zero, new Vector3(0.58f, 0.026f, 0.58f));
            CreateBlock(nodeObject.transform, "D020 Second Node Pedestal", assets.floorDark, new Vector3(0f, 0.18f, 0f), new Vector3(0.44f, 0.32f, 0.44f));
            CreateBlock(nodeObject.transform, "D020 Second Node Left Fork", assets.tool, new Vector3(-0.16f, 0.56f, 0f), new Vector3(0.10f, 0.46f, 0.08f), Quaternion.Euler(0f, 0f, -18f));
            CreateBlock(nodeObject.transform, "D020 Second Node Right Fork", assets.tool, new Vector3(0.16f, 0.56f, 0f), new Vector3(0.10f, 0.46f, 0.08f), Quaternion.Euler(0f, 0f, 18f));
            CreatePrimitive(nodeObject.transform, PrimitiveType.Sphere, "D020 Second Node Idle Lens", assets.tool, new Vector3(0f, 0.78f, 0f), new Vector3(0.20f, 0.16f, 0.20f));
            var activeRead = CreateBlock(nodeObject.transform, "D020 Second Node Active Beam", assets.relic, new Vector3(0f, 0.98f, 0f), new Vector3(0.13f, 0.58f, 0.13f), Quaternion.Euler(0f, 0f, 45f));
            activeRead.SetActive(false);
            response.SetActive(false);

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.activationRadius = 2.4f;
            node.responseTarget = response;
            node.idleRead = footprint;
            node.activeRead = activeRead;
            node.highlightRenderers = response.GetComponentsInChildren<Renderer>(true);
            node.ResetNode();
            return node;
        }

        private static void CreateRuntimeHook(Transform player, ExplorationNode[] nodes, GeneratedAssets assets)
        {
            var hookObject = new GameObject("D020 Runtime Hook");
            var audioSource = hookObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            var tool = hookObject.AddComponent<ExplorationTool>();
            tool.player = player;
            tool.nodes = nodes;
            tool.range = 2.8f;
            tool.cooldownSeconds = 0.42f;
            tool.pulse = LoadAudioClip("tool_pulse.wav");
            tool.targetHit = LoadAudioClip("tool_target_hit.wav");
            tool.fail = LoadAudioClip("tool_fail.wav");
            var pulseRead = CreatePrimitive(player, PrimitiveType.Cylinder, "D020 Tool Pulse Read", assets.tool, new Vector3(0f, 0.055f, 0f), new Vector3(1.08f, 0.035f, 1.08f));
            pulseRead.SetActive(false);
            tool.pulseRead = pulseRead;

            var progressSave = hookObject.AddComponent<D020ProgressSave>();
            progressSave.nodes = nodes;
            progressSave.nodeIds = new[]
            {
                "node.d020_shortcut_route",
                "node.d020_second_reward_route"
            };
        }

        private static AudioClip LoadAudioClip(string filename)
        {
            var path = $"Assets/Audio/Generated/{filename}";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                throw new FileNotFoundException($"Required D-020 generated audio clip is missing: {path}", path);
            }

            return clip;
        }

        private static void RequireAudioClip(AudioClip clip, string label)
        {
            if (clip == null)
            {
                throw new InvalidOperationException($"{label} is not assigned.");
            }
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
            public Material enemy;
            public Material enemyTell;
            public Material chest;
            public Material relic;
            public Material tool;
        }
    }
}
