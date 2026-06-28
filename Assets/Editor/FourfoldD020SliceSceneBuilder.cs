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
            var enemy = CreateEnemy(root.transform, assets, player.transform);
            var reward = CreateChest(root.transform, assets, player.transform);
            var nodes = CreateExplorationToolProof(root.transform, assets);
            reward.requiredEnemy = enemy;
            reward.requiredNode = nodes[0];
            reward.requiredNodes = new[] { nodes[1] };
            CreateRuntimeHook(player.transform, nodes, reward, assets);

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
            Require("D020 Reward Lens Node");
            Require("D020 Reward Lens Chamber");
            Require("D020 Reward Lens Chamber Path");
            Require("D020 Reward Lens Chamber Idle Read");
            Require("D020 Shortcut Route");
            Require("D020 Reward Lens Response");
            Require("D020 Top Down Camera");
            RequireComponent<ExplorationTool>("D020 Runtime Hook");
            RequireComponent<ExplorationNode>("D020 Exploration Tool Node");
            RequireComponent<ExplorationNode>("D020 Reward Lens Node");
            RequireComponent<D020PlayerController>("D020 Player");
            RequireComponent<D020EnemyDummy>("D020 Enemy Read Target");
            RequireComponent<D020RelicReward>("D020 Relic Chest");
            RequireComponent<D020ProgressSave>("D020 Runtime Hook");
            RequireComponent<D020HudController>("D020 HUD");

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
                tool = CreateMaterial("D020_ToolSignal", new Color(0.86f, 0.92f, 0.52f), 0f, 0.56f, new Color(0.54f, 0.45f, 0.08f)),
                toolPulse = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/tool_pulse.wav"),
                shortcutOpen = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/shortcut_open.wav"),
                relicPickup = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/relic_pickup.wav")
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
            cameraObject.transform.position = new Vector3(6.2f, 9.4f, -7.2f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.2f, 0.4f, -0.15f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.6f;
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
            CreateRewardLensChamber(room.transform, assets);
        }

        private static void CreateRewardLensChamber(Transform room, GeneratedAssets assets)
        {
            var chamber = new GameObject("D020 Reward Lens Chamber");
            chamber.transform.SetParent(room);

            CreateBlock(chamber.transform, "D020 Reward Lens Chamber Entrance", assets.route, new Vector3(1.25f, 0.028f, -1.02f), new Vector3(1.1f, 0.055f, 0.16f), Quaternion.Euler(0f, -18f, 0f));
            CreateBlock(chamber.transform, "D020 Reward Lens Chamber Left Rail", assets.floorDark, new Vector3(1.85f, 0.30f, -2.15f), new Vector3(1.18f, 0.58f, 0.20f), Quaternion.Euler(0f, 28f, 0f));
            CreateBlock(chamber.transform, "D020 Reward Lens Chamber Right Rail", assets.floorDark, new Vector3(3.75f, 0.30f, -0.48f), new Vector3(1.10f, 0.58f, 0.20f), Quaternion.Euler(0f, 27f, 0f));
            CreateBlock(chamber.transform, "D020 Reward Lens Chamber Back Rail", assets.floorDark, new Vector3(3.52f, 0.32f, -2.32f), new Vector3(1.28f, 0.60f, 0.22f), Quaternion.Euler(0f, -18f, 0f));
            CreatePrimitive(chamber.transform, PrimitiveType.Cylinder, "D020 Reward Lens Chamber Problem Ring", assets.relic, new Vector3(2.35f, 0.032f, -0.72f), new Vector3(0.82f, 0.026f, 0.82f));
            CreateBlock(chamber.transform, "D020 Reward Lens Chamber Locked Thread A", assets.floorDark, new Vector3(2.55f, 0.045f, -1.34f), new Vector3(0.95f, 0.055f, 0.12f), Quaternion.Euler(0f, 42f, 0f));
            CreateBlock(chamber.transform, "D020 Reward Lens Chamber Locked Thread B", assets.floorDark, new Vector3(2.98f, 0.048f, -1.15f), new Vector3(0.82f, 0.055f, 0.12f), Quaternion.Euler(0f, -24f, 0f));
        }

        private static GameObject CreatePlayer(Transform root, GeneratedAssets assets)
        {
            var player = new GameObject("D020 Player");
            player.transform.SetParent(root);
            player.transform.position = new Vector3(0f, 0.12f, -2.25f);
            player.transform.rotation = Quaternion.Euler(0f, 32f, 0f);

            CreatePrimitive(player.transform, PrimitiveType.Cylinder, "D020 Player Read Circle", assets.route, new Vector3(0f, 0.025f, 0f), new Vector3(0.86f, 0.035f, 0.86f));
            CreateBlock(player.transform, "D020 Player Feet", assets.player, new Vector3(0f, 0.17f, 0f), new Vector3(0.34f, 0.26f, 0.32f));
            CreatePrimitive(player.transform, PrimitiveType.Capsule, "D020 Player Body", assets.player, new Vector3(0f, 0.78f, 0f), new Vector3(0.44f, 0.70f, 0.38f));
            CreatePrimitive(player.transform, PrimitiveType.Sphere, "D020 Player Head", assets.player, new Vector3(0f, 1.39f, 0f), new Vector3(0.36f, 0.34f, 0.36f));
            CreateBlock(player.transform, "D020 Player Cape", assets.playerCape, new Vector3(-0.12f, 0.74f, -0.18f), new Vector3(0.55f, 0.88f, 0.11f));
            CreateBlock(player.transform, "D020 One Tool Held Read", assets.tool, new Vector3(0.47f, 0.86f, -0.05f), new Vector3(0.15f, 0.92f, 0.12f), Quaternion.Euler(0f, 0f, -25f));
            var attackRead = CreatePrimitive(player.transform, PrimitiveType.Cylinder, "D020 Player Attack Read", assets.tool, new Vector3(0f, 0.045f, 0.88f), new Vector3(0.86f, 0.022f, 0.46f));
            attackRead.SetActive(false);

            var controller = player.AddComponent<D020PlayerController>();
            controller.attackRead = attackRead;
            controller.attackRange = 1.45f;
            controller.moveSpeed = 3.35f;
            controller.minBounds = new Vector2(-4.1f, -2.8f);
            controller.maxBounds = new Vector2(4.1f, 2.8f);
            return player;
        }

        private static D020EnemyDummy CreateEnemy(Transform root, GeneratedAssets assets, Transform player)
        {
            var enemy = new GameObject("D020 Enemy Read Target");
            enemy.transform.SetParent(root);
            enemy.transform.position = new Vector3(1.65f, 0.12f, 0.35f);
            enemy.transform.rotation = Quaternion.Euler(0f, 200f, 0f);

            CreatePrimitive(enemy.transform, PrimitiveType.Capsule, "D020 Enemy Body", assets.enemy, new Vector3(0f, 0.72f, 0f), new Vector3(0.74f, 0.82f, 0.64f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Enemy Tell Core", assets.enemyTell, new Vector3(0f, 1.06f, -0.22f), new Vector3(0.26f, 0.26f, 0.14f));
            CreateBlock(enemy.transform, "D020 Enemy Left Arm", assets.enemy, new Vector3(-0.55f, 0.72f, 0f), new Vector3(0.22f, 0.58f, 0.20f), Quaternion.Euler(0f, 0f, 18f));
            CreateBlock(enemy.transform, "D020 Enemy Right Arm", assets.enemy, new Vector3(0.55f, 0.72f, 0f), new Vector3(0.22f, 0.58f, 0.20f), Quaternion.Euler(0f, 0f, -18f));
            var tellRead = CreatePrimitive(enemy.transform, PrimitiveType.Cylinder, "D020 Enemy Attack Read", assets.enemyTell, new Vector3(0f, 0.035f, -0.9f), new Vector3(0.95f, 0.025f, 0.95f));
            var defeatedRead = CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Enemy Defeated Shard", assets.route, new Vector3(0f, 0.24f, 0f), new Vector3(0.42f, 0.20f, 0.42f));
            defeatedRead.SetActive(false);

            var dummy = enemy.AddComponent<D020EnemyDummy>();
            dummy.target = player;
            dummy.tellRead = tellRead;
            dummy.defeatedRead = defeatedRead;
            dummy.maxHealth = 3;
            dummy.slowChaseSpeed = 0.45f;
            return dummy;
        }

        private static D020RelicReward CreateChest(Transform root, GeneratedAssets assets, Transform player)
        {
            var chest = new GameObject("D020 Relic Chest");
            chest.transform.SetParent(root);
            chest.transform.position = new Vector3(3.05f, 0.1f, -1.55f);
            chest.transform.rotation = Quaternion.Euler(0f, -18f, 0f);

            CreateBlock(chest.transform, "D020 Chest Base", assets.chest, Vector3.zero, new Vector3(0.78f, 0.42f, 0.58f));
            CreateBlock(chest.transform, "D020 Chest Lid", assets.route, new Vector3(0f, 0.33f, 0f), new Vector3(0.82f, 0.15f, 0.62f));
            var visibleRelic = CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Visible Relic", assets.relic, new Vector3(0f, 0.72f, 0f), new Vector3(0.26f, 0.36f, 0.26f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Reward Footprint", assets.relic, new Vector3(0f, 0.03f, 0f), new Vector3(0.92f, 0.026f, 0.92f));
            var collectedRead = CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Reward Collected Read", assets.tool, new Vector3(0f, 1.02f, 0f), new Vector3(0.18f, 0.18f, 0.18f));
            collectedRead.SetActive(false);

            var reward = chest.AddComponent<D020RelicReward>();
            reward.rewardId = "d020.region01.relic.01";
            reward.pickupRadius = 1.15f;
            reward.player = player;
            reward.idleRead = visibleRelic;
            reward.collectedRead = collectedRead;
            reward.pickupClip = assets.relicPickup;
            reward.ResetReward();
            return reward;
        }

        private static ExplorationNode[] CreateExplorationToolProof(Transform root, GeneratedAssets assets)
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
            CreateBlock(nodeObject.transform, "D020 Tool Node Signal", assets.tool, new Vector3(0f, 0.58f, -0.03f), new Vector3(0.14f, 0.44f, 0.08f), Quaternion.Euler(0f, 0f, 45f));
            var activeRead = CreatePrimitive(nodeObject.transform, PrimitiveType.Sphere, "D020 Tool Node Active Read", assets.relic, new Vector3(0f, 0.86f, -0.03f), new Vector3(0.18f, 0.18f, 0.18f));
            activeRead.SetActive(false);
            response.SetActive(false);

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.activationRadius = 2.6f;
            node.responseTarget = response;
            node.idleRead = footprint;
            node.activeRead = activeRead;
            node.highlightRenderers = response.GetComponentsInChildren<Renderer>(true);
            node.ResetNode();

            var rewardResponse = new GameObject("D020 Reward Lens Response");
            rewardResponse.transform.SetParent(proof.transform);
            rewardResponse.transform.position = Vector3.zero;
            var chamberPath = new GameObject("D020 Reward Lens Chamber Path");
            chamberPath.transform.SetParent(rewardResponse.transform);
            chamberPath.transform.localPosition = Vector3.zero;
            CreateBlock(chamberPath.transform, "D020 Reward Lens Chamber Path A", assets.route, new Vector3(2.10f, 0.074f, -0.92f), new Vector3(0.92f, 0.062f, 0.18f), Quaternion.Euler(0f, -24f, 0f));
            CreateBlock(chamberPath.transform, "D020 Reward Lens Chamber Path B", assets.route, new Vector3(2.62f, 0.078f, -1.20f), new Vector3(0.86f, 0.062f, 0.18f), Quaternion.Euler(0f, 44f, 0f));
            CreateBlock(chamberPath.transform, "D020 Reward Lens Chamber Path C", assets.route, new Vector3(3.15f, 0.082f, -1.42f), new Vector3(0.72f, 0.062f, 0.18f), Quaternion.Euler(0f, -18f, 0f));
            CreatePrimitive(chamberPath.transform, PrimitiveType.Sphere, "D020 Reward Lens Gate Open Read", assets.tool, new Vector3(2.98f, 0.42f, -1.18f), new Vector3(0.20f, 0.20f, 0.20f));
            CreatePrimitive(rewardResponse.transform, PrimitiveType.Cylinder, "D020 Reward Lens Open Ring", assets.relic, new Vector3(3.05f, 0.072f, -1.55f), new Vector3(1.22f, 0.032f, 1.22f));
            CreateBlock(rewardResponse.transform, "D020 Reward Lens Beam A", assets.tool, new Vector3(2.54f, 0.13f, -0.78f), new Vector3(0.08f, 0.05f, 1.18f), Quaternion.Euler(0f, -34f, 0f));
            CreateBlock(rewardResponse.transform, "D020 Reward Lens Beam B", assets.tool, new Vector3(3.08f, 0.14f, -0.98f), new Vector3(0.08f, 0.05f, 1.24f), Quaternion.Euler(0f, 18f, 0f));
            CreateBlock(rewardResponse.transform, "D020 Reward Lens Beam C", assets.tool, new Vector3(3.45f, 0.15f, -1.35f), new Vector3(0.08f, 0.05f, 0.82f), Quaternion.Euler(0f, 52f, 0f));
            CreatePrimitive(rewardResponse.transform, PrimitiveType.Sphere, "D020 Reward Lens Spark A", assets.relic, new Vector3(2.44f, 0.46f, -0.78f), new Vector3(0.18f, 0.18f, 0.18f));
            CreatePrimitive(rewardResponse.transform, PrimitiveType.Sphere, "D020 Reward Lens Spark B", assets.tool, new Vector3(3.58f, 0.44f, -1.34f), new Vector3(0.16f, 0.16f, 0.16f));

            var rewardNodeObject = new GameObject("D020 Reward Lens Node");
            rewardNodeObject.transform.SetParent(proof.transform);
            rewardNodeObject.transform.position = new Vector3(2.35f, 0.1f, -0.72f);
            var rewardIdleRead = new GameObject("D020 Reward Lens Chamber Idle Read");
            rewardIdleRead.transform.SetParent(rewardNodeObject.transform);
            rewardIdleRead.transform.localPosition = Vector3.zero;
            CreatePrimitive(rewardIdleRead.transform, PrimitiveType.Cylinder, "D020 Reward Lens Footprint", assets.tool, Vector3.zero, new Vector3(0.58f, 0.026f, 0.58f));
            CreateBlock(rewardIdleRead.transform, "D020 Reward Lens Closed Gate Read", assets.floorDark, new Vector3(0.72f, 0.16f, -0.78f), new Vector3(0.74f, 0.30f, 0.12f), Quaternion.Euler(0f, 34f, 0f));
            CreateBlock(rewardIdleRead.transform, "D020 Reward Lens Closed Thread Read", assets.tool, new Vector3(0.52f, 0.35f, -0.54f), new Vector3(0.10f, 0.38f, 0.06f), Quaternion.Euler(0f, 0f, -30f));
            CreateBlock(rewardNodeObject.transform, "D020 Reward Lens Pedestal", assets.floorDark, new Vector3(0f, 0.18f, 0f), new Vector3(0.42f, 0.32f, 0.42f));
            CreateBlock(rewardNodeObject.transform, "D020 Reward Lens Signal", assets.relic, new Vector3(0f, 0.52f, 0.02f), new Vector3(0.12f, 0.38f, 0.08f), Quaternion.Euler(0f, 0f, -38f));
            var rewardActiveRead = CreatePrimitive(rewardNodeObject.transform, PrimitiveType.Sphere, "D020 Reward Lens Active Read", assets.relic, new Vector3(0f, 0.78f, 0.02f), new Vector3(0.16f, 0.16f, 0.16f));
            rewardActiveRead.SetActive(false);
            rewardResponse.SetActive(false);

            var rewardNode = rewardNodeObject.AddComponent<ExplorationNode>();
            rewardNode.activationRadius = 2.25f;
            rewardNode.responseTarget = rewardResponse;
            rewardNode.idleRead = rewardIdleRead;
            rewardNode.activeRead = rewardActiveRead;
            rewardNode.highlightRenderers = rewardResponse.GetComponentsInChildren<Renderer>(true);
            rewardNode.ResetNode();
            return new[] { node, rewardNode };
        }

        private static void CreateRuntimeHook(Transform player, ExplorationNode[] nodes, D020RelicReward reward, GeneratedAssets assets)
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
            tool.pulseRead = nodes[0].idleRead;
            tool.pulse = assets.toolPulse;
            tool.targetHit = assets.shortcutOpen;

            var progress = hookObject.AddComponent<D020ProgressSave>();
            progress.nodes = nodes;
            progress.nodeIds = new[] { "d020.region01.shortcut.01", "d020.region01.reward_lens.01" };
            progress.rewards = new[] { reward };
            progress.rewardIds = new[] { "d020.region01.relic.01" };
            progress.saveFileName = "d020-region01-progress.json";
            progress.loadOnAwake = true;
            progress.saveOnProgressChanged = true;

            var hudObject = new GameObject("D020 HUD");
            var hud = hudObject.AddComponent<D020HudController>();
            hud.player = player.GetComponent<D020PlayerController>();
            hud.tool = tool;
            hud.node = nodes[0];
            hud.nodes = nodes;
            hud.reward = reward;
            hud.progressSave = progress;
            hud.RefreshNow();
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
            public AudioClip toolPulse;
            public AudioClip shortcutOpen;
            public AudioClip relicPickup;
        }
    }
}
