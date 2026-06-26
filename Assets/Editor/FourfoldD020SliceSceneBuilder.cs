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
            CreateMeleeEnemy(root.transform, assets);
            CreateRangedEnemy(root.transform, assets);
            CreateChest(root.transform, assets);
            var node = CreateExplorationToolProof(root.transform, assets);
            CreateRuntimeHook(player.transform, node, assets);

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
            Require("D020 Relic Chest");
            Require("D020 Exploration Tool Node");
            Require("D020 Shortcut Route");
            Require("D020 Top Down Camera");
            RequireComponent<ExplorationTool>("D020 Runtime Hook");
            RequireComponent<ExplorationNode>("D020 Exploration Tool Node");

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
            cameraObject.transform.position = new Vector3(8.6f, 12.4f, -10.4f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.15f, 0.1f, 0.15f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.2f;
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

        private static void CreateRoom(Transform root, GeneratedAssets assets)
        {
            var room = new GameObject("D020 Readable Room");
            room.transform.SetParent(root);

            for (var x = -6; x <= 6; x++)
            {
                for (var z = -5; z <= 5; z++)
                {
                    var material = Mathf.Abs(x) == 6 || Mathf.Abs(z) == 5 ? assets.floorDark : assets.floor;
                    CreateBlock(room.transform, $"D020 Floor {x},{z}", material, new Vector3(x, -0.08f, z), new Vector3(0.96f, 0.16f, 0.96f));
                }
            }

            for (var x = -6; x <= 6; x += 2)
            {
                CreateBlock(room.transform, $"D020 Low North Wall {x}", assets.floorDark, new Vector3(x, 0.34f, 5.55f), new Vector3(1.25f, 0.64f, 0.30f));
                CreateBlock(room.transform, $"D020 Low South Wall {x}", assets.floorDark, new Vector3(x, 0.34f, -5.55f), new Vector3(1.25f, 0.64f, 0.30f));
            }

            for (var z = -3; z <= 5; z += 2)
            {
                CreateBlock(room.transform, $"D020 Low East Wall {z}", assets.floorDark, new Vector3(6.55f, 0.34f, z), new Vector3(0.30f, 0.64f, 1.25f));
            }

            CreateBlock(room.transform, "D020 Shortcut Gate Left Stone", assets.floorDark, new Vector3(-6.55f, 0.34f, 1.7f), new Vector3(0.30f, 0.64f, 1.2f));
            CreateBlock(room.transform, "D020 Shortcut Gate Right Stone", assets.floorDark, new Vector3(-6.55f, 0.34f, -1.25f), new Vector3(0.30f, 0.64f, 1.2f));
            CreateBlock(room.transform, "D020 Start Threshold", assets.route, new Vector3(-4.2f, 0.02f, -4.2f), new Vector3(2.0f, 0.05f, 0.18f), Quaternion.Euler(0f, 12f, 0f));
            CreateBlock(room.transform, "D020 Tool Route Line A", assets.route, new Vector3(-3.45f, 0.025f, -2.75f), new Vector3(1.65f, 0.05f, 0.16f), Quaternion.Euler(0f, 32f, 0f));
            CreateBlock(room.transform, "D020 Tool Route Line B", assets.route, new Vector3(-1.95f, 0.025f, -1.55f), new Vector3(1.6f, 0.05f, 0.16f), Quaternion.Euler(0f, 36f, 0f));
            CreateBlock(room.transform, "D020 Combat Route Line", assets.route, new Vector3(0.05f, 0.025f, 0.35f), new Vector3(2.4f, 0.05f, 0.16f), Quaternion.Euler(0f, 24f, 0f));
            CreateBlock(room.transform, "D020 Reward Route Line", assets.route, new Vector3(3.05f, 0.025f, 2.45f), new Vector3(2.7f, 0.05f, 0.16f), Quaternion.Euler(0f, 18f, 0f));
            CreateBlock(room.transform, "D020 Reward Low Rail A", assets.floorDark, new Vector3(2.2f, 0.26f, 3.85f), new Vector3(1.8f, 0.52f, 0.22f));
            CreateBlock(room.transform, "D020 Reward Low Rail B", assets.floorDark, new Vector3(4.8f, 0.26f, 3.85f), new Vector3(1.8f, 0.52f, 0.22f));
            CreateBlock(room.transform, "D020 Enemy Arena Marker", assets.enemyTell, new Vector3(0.85f, 0.015f, 1.1f), new Vector3(2.2f, 0.035f, 1.35f), Quaternion.Euler(0f, 24f, 0f));
            CreateBlock(room.transform, "D020 Tool Target Backplate", assets.floorDark, new Vector3(-3.35f, 0.44f, -1.35f), new Vector3(1.2f, 0.86f, 0.20f));
        }

        private static GameObject CreatePlayer(Transform root, GeneratedAssets assets)
        {
            var player = new GameObject("D020 Player");
            player.transform.SetParent(root);
            player.transform.position = new Vector3(-4.15f, 0.12f, -3.85f);
            player.transform.rotation = Quaternion.Euler(0f, 42f, 0f);

            CreatePrimitive(player.transform, PrimitiveType.Cylinder, "D020 Player Read Circle", assets.route, new Vector3(0f, 0.025f, 0f), new Vector3(1.18f, 0.035f, 1.18f));
            CreateBlock(player.transform, "D020 Player Feet", assets.player, new Vector3(0f, 0.18f, 0f), new Vector3(0.44f, 0.28f, 0.40f));
            CreatePrimitive(player.transform, PrimitiveType.Capsule, "D020 Player Body", assets.player, new Vector3(0f, 0.86f, 0f), new Vector3(0.56f, 0.86f, 0.46f));
            CreatePrimitive(player.transform, PrimitiveType.Sphere, "D020 Player Head", assets.player, new Vector3(0f, 1.56f, 0f), new Vector3(0.42f, 0.40f, 0.42f));
            CreateBlock(player.transform, "D020 Player Cape", assets.playerCape, new Vector3(-0.14f, 0.82f, -0.24f), new Vector3(0.72f, 1.05f, 0.13f));
            CreateBlock(player.transform, "D020 Player Sword Read", assets.floorDark, new Vector3(0.28f, 0.88f, 0.42f), new Vector3(0.12f, 0.82f, 0.09f), Quaternion.Euler(34f, 0f, -28f));
            CreateBlock(player.transform, "D020 One Tool Held Read", assets.tool, new Vector3(0.58f, 0.98f, -0.08f), new Vector3(0.18f, 1.18f, 0.13f), Quaternion.Euler(0f, 0f, -25f));
            CreatePrimitive(player.transform, PrimitiveType.Sphere, "D020 Tool Hand Glow", assets.tool, new Vector3(0.66f, 1.52f, -0.12f), new Vector3(0.22f, 0.22f, 0.22f));
            return player;
        }

        private static void CreateMeleeEnemy(Transform root, GeneratedAssets assets)
        {
            var enemy = new GameObject("D020 Enemy Read Target");
            enemy.transform.SetParent(root);
            enemy.transform.position = new Vector3(0.72f, 0.12f, 1.05f);
            enemy.transform.rotation = Quaternion.Euler(0f, 200f, 0f);

            CreatePrimitive(enemy.transform, PrimitiveType.Capsule, "D020 Enemy Body", assets.enemy, new Vector3(0f, 0.76f, 0f), new Vector3(0.92f, 0.92f, 0.74f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Enemy Tell Core", assets.enemyTell, new Vector3(0f, 1.18f, -0.28f), new Vector3(0.32f, 0.32f, 0.18f));
            CreateBlock(enemy.transform, "D020 Enemy Left Arm", assets.enemy, new Vector3(-0.68f, 0.74f, 0f), new Vector3(0.26f, 0.66f, 0.22f), Quaternion.Euler(0f, 0f, 18f));
            CreateBlock(enemy.transform, "D020 Enemy Right Arm", assets.enemy, new Vector3(0.68f, 0.74f, 0f), new Vector3(0.26f, 0.66f, 0.22f), Quaternion.Euler(0f, 0f, -18f));
            CreatePrimitive(enemy.transform, PrimitiveType.Cylinder, "D020 Enemy Melee Danger Read", assets.enemyTell, new Vector3(0f, 0.035f, -1.05f), new Vector3(1.25f, 0.025f, 1.25f));
        }

        private static void CreateRangedEnemy(Transform root, GeneratedAssets assets)
        {
            var enemy = new GameObject("D020 Enemy Ranged Read Target");
            enemy.transform.SetParent(root);
            enemy.transform.position = new Vector3(3.25f, 0.12f, 1.95f);
            enemy.transform.rotation = Quaternion.Euler(0f, 226f, 0f);

            CreatePrimitive(enemy.transform, PrimitiveType.Cylinder, "D020 Ranged Enemy Base", assets.enemy, new Vector3(0f, 0.40f, 0f), new Vector3(0.54f, 0.70f, 0.54f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Ranged Enemy Head", assets.enemy, new Vector3(0f, 1.06f, 0f), new Vector3(0.38f, 0.34f, 0.38f));
            CreateBlock(enemy.transform, "D020 Ranged Enemy Staff", assets.enemyTell, new Vector3(0.46f, 0.86f, -0.08f), new Vector3(0.12f, 1.18f, 0.10f), Quaternion.Euler(0f, 0f, -12f));
            CreateBlock(enemy.transform, "D020 Ranged Enemy Aim Line", assets.enemyTell, new Vector3(-0.58f, 0.13f, -0.72f), new Vector3(1.25f, 0.035f, 0.10f), Quaternion.Euler(0f, 28f, 0f));
            CreatePrimitive(enemy.transform, PrimitiveType.Sphere, "D020 Ranged Enemy Tell Orb", assets.enemyTell, new Vector3(0.56f, 1.48f, -0.12f), new Vector3(0.20f, 0.20f, 0.20f));
        }

        private static void CreateChest(Transform root, GeneratedAssets assets)
        {
            var chest = new GameObject("D020 Relic Chest");
            chest.transform.SetParent(root);
            chest.transform.position = new Vector3(5.0f, 0.1f, 3.65f);
            chest.transform.rotation = Quaternion.Euler(0f, -18f, 0f);

            CreateBlock(chest.transform, "D020 Chest Base", assets.chest, Vector3.zero, new Vector3(1.02f, 0.48f, 0.72f));
            CreateBlock(chest.transform, "D020 Chest Lid", assets.route, new Vector3(0f, 0.39f, 0f), new Vector3(1.08f, 0.17f, 0.78f));
            CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Visible Relic", assets.relic, new Vector3(0f, 0.88f, 0f), new Vector3(0.34f, 0.46f, 0.34f));
            CreatePrimitive(chest.transform, PrimitiveType.Cylinder, "D020 Reward Footprint", assets.relic, new Vector3(0f, 0.03f, 0f), new Vector3(1.24f, 0.026f, 1.24f));
            CreatePrimitive(chest.transform, PrimitiveType.Sphere, "D020 Reward Beacon", assets.relic, new Vector3(0f, 1.42f, 0f), new Vector3(0.16f, 0.28f, 0.16f));
        }

        private static ExplorationNode CreateExplorationToolProof(Transform root, GeneratedAssets assets)
        {
            var proof = new GameObject("D020 One Tool Proof");
            proof.transform.SetParent(root);

            var response = new GameObject("D020 Shortcut Route");
            response.transform.SetParent(proof.transform);
            response.transform.position = Vector3.zero;
            CreateBlock(response.transform, "D020 Shortcut Slab A", assets.route, new Vector3(-5.95f, 0.05f, 0.02f), new Vector3(0.96f, 0.07f, 0.38f), Quaternion.Euler(0f, 17f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab B", assets.route, new Vector3(-5.28f, 0.06f, 0.52f), new Vector3(0.96f, 0.07f, 0.38f), Quaternion.Euler(0f, -14f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab C", assets.route, new Vector3(-4.58f, 0.07f, 0.98f), new Vector3(0.96f, 0.07f, 0.38f), Quaternion.Euler(0f, 16f, 0f));
            CreateBlock(response.transform, "D020 Shortcut Slab D", assets.route, new Vector3(-3.9f, 0.08f, 1.45f), new Vector3(0.90f, 0.07f, 0.34f), Quaternion.Euler(0f, -8f, 0f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Shortcut Open Spark A", assets.tool, new Vector3(-5.20f, 0.48f, 0.52f), new Vector3(0.24f, 0.24f, 0.24f));
            CreatePrimitive(response.transform, PrimitiveType.Sphere, "D020 Shortcut Open Spark B", assets.tool, new Vector3(-3.95f, 0.54f, 1.43f), new Vector3(0.22f, 0.22f, 0.22f));
            CreateBlock(response.transform, "D020 Shortcut Direction Beam", assets.tool, new Vector3(-4.42f, 0.14f, 1.12f), new Vector3(1.70f, 0.045f, 0.09f), Quaternion.Euler(0f, 32f, 0f));

            var nodeObject = new GameObject("D020 Exploration Tool Node");
            nodeObject.transform.SetParent(proof.transform);
            nodeObject.transform.position = new Vector3(-3.25f, 0.1f, -1.85f);
            var footprint = CreatePrimitive(nodeObject.transform, PrimitiveType.Cylinder, "D020 Tool Node Footprint", assets.tool, Vector3.zero, new Vector3(0.92f, 0.026f, 0.92f));
            CreateBlock(nodeObject.transform, "D020 Tool Node Pedestal", assets.floorDark, new Vector3(0f, 0.24f, 0f), new Vector3(0.66f, 0.44f, 0.66f));
            CreateBlock(nodeObject.transform, "D020 Tool Node Signal A", assets.tool, new Vector3(-0.10f, 0.68f, -0.04f), new Vector3(0.16f, 0.62f, 0.09f), Quaternion.Euler(0f, 0f, 45f));
            CreateBlock(nodeObject.transform, "D020 Tool Node Signal B", assets.tool, new Vector3(0.16f, 0.68f, -0.04f), new Vector3(0.16f, 0.62f, 0.09f), Quaternion.Euler(0f, 0f, -45f));
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

        private static void CreateRuntimeHook(Transform player, ExplorationNode node, GeneratedAssets assets)
        {
            var hookObject = new GameObject("D020 Runtime Hook");
            var audioSource = hookObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            var tool = hookObject.AddComponent<ExplorationTool>();
            tool.player = player;
            tool.nodes = new[] { node };
            tool.range = 2.8f;
            tool.cooldownSeconds = 0.42f;
            tool.pulseRead = node.idleRead;
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
