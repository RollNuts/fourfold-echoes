using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldHubSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/HubCrossroads.unity";
        private const string MaterialFolder = "Assets/Art/Generated/Hub/Materials";

        public static void BuildAndValidate()
        {
            Build();
            ValidateGeneratedScene();
        }

        public static void Build()
        {
            EnsureFolders();
            var materials = CreateMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = FourfoldGameIds.UnitySceneHubCrossroads;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.24f, 0.25f, 0.28f);
            RenderSettings.ambientEquatorColor = new Color(0.14f, 0.14f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.055f, 0.05f, 0.055f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.12f, 0.13f, 0.15f);
            RenderSettings.fogDensity = 0.016f;

            CreateLighting();
            var camera = CreateCamera();
            var root = new GameObject("Hub Crossroads");
            CreateGround(root.transform, materials);
            CreateLandmarks(root.transform, materials);
            var returnSpawn = CreateReturnSpawn(root.transform, materials);
            var player = CreatePlayer(root.transform, materials);
            var gate = CreateD020Gate(root.transform, materials);
            var r02Gate = CreateR02Gate(root.transform, materials);
            CreateRuntimeHook(player.transform, returnSpawn.transform, gate.transform, r02Gate.transform, camera);

            EditorSceneManager.SaveScene(scene, ScenePath);
            ApplyBuildSettings();
            Selection.activeObject = camera.gameObject;
            AssetDatabase.SaveAssets();
            Debug.Log($"FOURFOLD Hub Crossroads scene generated at {ScenePath}");
        }

        public static void ValidateGeneratedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"Hub Crossroads scene is missing or invalid: {ScenePath}");
            }

            Require("Hub Crossroads");
            Require("Hub Player");
            Require("Hub Player Spawn");
            Require("Hub Region Gate D020");
            Require("Hub Region Gate R02");
            Require("Hub Top Down Camera");
            RequireComponent<HubSceneController>("Hub Runtime Hook");
            ValidateRuntimeHookReferences();
            ValidateBuildSettings();

            if (Camera.main == null)
            {
                throw new InvalidOperationException("Hub Crossroads scene has no MainCamera tagged camera.");
            }

            Debug.Log("FOURFOLD Hub Crossroads scene validation passed.");
        }

        public static void ApplyBuildSettings()
        {
            var scenes = File.Exists(FourfoldTitleSceneBuilder.ScenePath) && File.Exists(FourfoldD020SliceSceneBuilder.ScenePath)
                ? new[]
                {
                    new EditorBuildSettingsScene(FourfoldTitleSceneBuilder.ScenePath, true),
                    new EditorBuildSettingsScene(ScenePath, true),
                    new EditorBuildSettingsScene(FourfoldD020SliceSceneBuilder.ScenePath, true)
                }
                : File.Exists(FourfoldD020SliceSceneBuilder.ScenePath)
                ? new[]
                {
                    new EditorBuildSettingsScene(ScenePath, true),
                    new EditorBuildSettingsScene(FourfoldD020SliceSceneBuilder.ScenePath, true)
                }
                : new[]
                {
                    new EditorBuildSettingsScene(ScenePath, true)
                };

            EditorBuildSettings.scenes = scenes;
        }

        private static void ValidateRuntimeHookReferences()
        {
            var hook = FindSceneObject("Hub Runtime Hook");
            if (hook == null)
            {
                throw new InvalidOperationException("Hub runtime hook is missing.");
            }

            var controller = hook.GetComponent<HubSceneController>();
            if (controller == null || controller.player == null || controller.returnSpawn == null || controller.d020RegionGate == null || controller.r02RegionGate == null || controller.fixedCamera == null)
            {
                throw new InvalidOperationException("Hub runtime hook is missing required player, spawn, region gates, or camera references.");
            }

            if (controller.regionSceneName != FourfoldGameIds.UnitySceneD020VerticalSlice)
            {
                throw new InvalidOperationException("Hub runtime hook must target the D-020 Unity scene name.");
            }

            if (controller.regionR02SceneName != FourfoldGameIds.UnitySceneR02CinderCanal)
            {
                throw new InvalidOperationException("Hub runtime hook must stage the R02 Unity scene name.");
            }

            if (controller.regionR02Playable)
            {
                throw new InvalidOperationException("Hub runtime hook must keep R02 non-playable until the R02 scene lands.");
            }
        }

        private static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length < 2)
            {
                throw new InvalidOperationException("Build Settings must include HubCrossroads and D020VerticalSlice.");
            }

            if (scenes.Length >= 3 && scenes[0].path == FourfoldTitleSceneBuilder.ScenePath)
            {
                if (scenes[1].path != ScenePath || scenes[2].path != FourfoldD020SliceSceneBuilder.ScenePath || !scenes[0].enabled || !scenes[1].enabled || !scenes[2].enabled)
                {
                    throw new InvalidOperationException("Build Settings order must be Title, HubCrossroads, then D020VerticalSlice.");
                }

                return;
            }

            if (scenes[0].path != ScenePath || scenes[1].path != FourfoldD020SliceSceneBuilder.ScenePath || !scenes[0].enabled || !scenes[1].enabled)
            {
                throw new InvalidOperationException("Build Settings order must be HubCrossroads, then D020VerticalSlice, or Title, HubCrossroads, then D020VerticalSlice.");
            }
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
        }

        private static GeneratedMaterials CreateMaterials()
        {
            return new GeneratedMaterials
            {
                floor = CreateMaterial("Hub_Floor", new Color(0.25f, 0.24f, 0.21f)),
                floorEdge = CreateMaterial("Hub_Floor_Edge", new Color(0.12f, 0.12f, 0.13f)),
                player = CreateMaterial("Hub_Player", new Color(0.22f, 0.36f, 0.88f)),
                playerAccent = CreateMaterial("Hub_Player_Accent", new Color(1.0f, 0.78f, 0.28f)),
                gate = CreateMaterial("Hub_D020_Gate", new Color(0.95f, 0.58f, 0.16f), new Color(0.75f, 0.30f, 0.05f)),
                gateR02 = CreateMaterial("Hub_R02_Gate", new Color(0.30f, 0.62f, 1.0f), new Color(0.05f, 0.25f, 0.72f)),
                gateStone = CreateMaterial("Hub_Gate_Stone", new Color(0.18f, 0.17f, 0.18f)),
                marker = CreateMaterial("Hub_Objective_Marker", new Color(0.25f, 0.76f, 1.0f), new Color(0.0f, 0.35f, 0.72f)),
                shadow = CreateMaterial("Hub_Soft_Boundary", new Color(0.06f, 0.06f, 0.065f))
            };
        }

        private static Material CreateMaterial(string name, Color color)
        {
            return CreateMaterial(name, color, Color.black);
        }

        private static Material CreateMaterial(string name, Color color, Color emission)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = name;
            material.SetColor("_Color", color);
            if (emission.maxColorComponent > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }

            return material;
        }

        private static void CreateLighting()
        {
            var key = new GameObject("Hub Warm Key Light");
            var light = key.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1.0f, 0.82f, 0.62f);
            key.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            CreatePointLight("Hub Gate Light", new Vector3(0f, 3.0f, 4.0f), new Color(1.0f, 0.55f, 0.2f), 4.6f, 2.8f);
            CreatePointLight("Hub Spawn Light", new Vector3(-3.6f, 2.2f, -3.1f), new Color(0.32f, 0.72f, 1.0f), 3.2f, 1.8f);
        }

        private static void CreatePointLight(string name, Vector3 position, Color color, float range, float intensity)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.position = position;
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = color;
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Hub Top Down Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 9.4f, -8.7f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.1f, 1.2f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            return camera;
        }

        private static void CreateGround(Transform parent, GeneratedMaterials materials)
        {
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    var material = Mathf.Abs(x) == 3 || Mathf.Abs(z) == 3 ? materials.floorEdge : materials.floor;
                    CreateBlock(parent, $"Hub Floor {x}_{z}", material, new Vector3(x * 1.05f, -0.08f, z * 1.05f), new Vector3(1.0f, 0.16f, 1.0f));
                }
            }

            CreateBlock(parent, "Hub North Boundary", materials.shadow, new Vector3(0f, 0.28f, 6.0f), new Vector3(8.0f, 0.55f, 0.34f));
            CreateBlock(parent, "Hub West Boundary", materials.shadow, new Vector3(-4.2f, 0.28f, 0.2f), new Vector3(0.34f, 0.55f, 8.0f));
            CreateBlock(parent, "Hub East Boundary", materials.shadow, new Vector3(4.2f, 0.28f, 0.2f), new Vector3(0.34f, 0.55f, 8.0f));
        }

        private static void CreateLandmarks(Transform parent, GeneratedMaterials materials)
        {
            CreateBlock(parent, "Hub Route Plinth West", materials.gateStone, new Vector3(-2.8f, 0.35f, 2.4f), new Vector3(0.65f, 0.85f, 0.65f));
            CreateBlock(parent, "Hub Route Plinth East", materials.gateStone, new Vector3(2.8f, 0.35f, 2.4f), new Vector3(0.65f, 0.85f, 0.65f));
            CreatePrimitive(parent, PrimitiveType.Sphere, "Hub Objective Beacon", materials.marker, new Vector3(0f, 1.35f, 3.85f), new Vector3(0.42f, 0.42f, 0.42f));
        }

        private static GameObject CreateReturnSpawn(Transform parent, GeneratedMaterials materials)
        {
            var spawn = CreatePrimitive(parent, PrimitiveType.Cylinder, "Hub Player Spawn", materials.marker, new Vector3(-2.8f, 0.03f, -2.65f), new Vector3(1.1f, 0.055f, 1.1f));
            spawn.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            return spawn;
        }

        private static GameObject CreatePlayer(Transform parent, GeneratedMaterials materials)
        {
            var player = new GameObject("Hub Player");
            player.transform.SetParent(parent);
            player.transform.localPosition = new Vector3(-2.8f, 0.82f, -2.65f);
            var body = CreatePrimitive(player.transform, PrimitiveType.Capsule, "Hub Player Body", materials.player, Vector3.zero, new Vector3(0.62f, 0.82f, 0.62f));
            body.transform.localRotation = Quaternion.identity;
            CreateBlock(player.transform, "Hub Player Direction Cloak", materials.playerAccent, new Vector3(0f, 0.03f, -0.34f), new Vector3(0.58f, 0.46f, 0.14f));
            CreateBlock(player.transform, "Hub Player Tool Read", materials.playerAccent, new Vector3(0.33f, 0.18f, 0.35f), new Vector3(0.14f, 0.18f, 0.64f), Quaternion.Euler(0f, -18f, 0f));
            return player;
        }

        private static GameObject CreateD020Gate(Transform parent, GeneratedMaterials materials)
        {
            var gate = new GameObject("Hub Region Gate D020");
            gate.transform.SetParent(parent);
            gate.transform.localPosition = new Vector3(0f, 0f, 4.2f);
            CreateBlock(gate.transform, "Hub D020 Gate Left Pillar", materials.gateStone, new Vector3(-0.78f, 0.95f, 0f), new Vector3(0.38f, 1.9f, 0.38f));
            CreateBlock(gate.transform, "Hub D020 Gate Right Pillar", materials.gateStone, new Vector3(0.78f, 0.95f, 0f), new Vector3(0.38f, 1.9f, 0.38f));
            CreateBlock(gate.transform, "Hub D020 Gate Header", materials.gateStone, new Vector3(0f, 1.82f, 0f), new Vector3(1.95f, 0.28f, 0.34f));
            CreatePrimitive(gate.transform, PrimitiveType.Cylinder, "Hub D020 Gate Ring", materials.gate, new Vector3(0f, 0.55f, -0.02f), new Vector3(1.28f, 0.035f, 1.28f), Quaternion.Euler(90f, 0f, 0f));
            CreatePrimitive(gate.transform, PrimitiveType.Sphere, "Hub D020 Gate Core", materials.gate, new Vector3(0f, 0.95f, -0.04f), new Vector3(0.46f, 0.46f, 0.46f));
            return gate;
        }

        private static GameObject CreateR02Gate(Transform parent, GeneratedMaterials materials)
        {
            var gate = new GameObject("Hub Region Gate R02");
            gate.transform.SetParent(parent);
            gate.transform.localPosition = new Vector3(3.05f, 0f, 1.9f);
            gate.transform.localRotation = Quaternion.Euler(0f, -42f, 0f);
            CreateBlock(gate.transform, "Hub R02 Gate Left Pillar", materials.gateStone, new Vector3(-0.62f, 0.82f, 0f), new Vector3(0.30f, 1.64f, 0.34f));
            CreateBlock(gate.transform, "Hub R02 Gate Right Pillar", materials.gateStone, new Vector3(0.62f, 0.82f, 0f), new Vector3(0.30f, 1.64f, 0.34f));
            CreateBlock(gate.transform, "Hub R02 Gate Header", materials.gateStone, new Vector3(0f, 1.56f, 0f), new Vector3(1.58f, 0.24f, 0.30f));
            CreatePrimitive(gate.transform, PrimitiveType.Cylinder, "Hub R02 Gate Ring", materials.gateR02, new Vector3(0f, 0.50f, -0.02f), new Vector3(1.02f, 0.032f, 1.02f), Quaternion.Euler(90f, 0f, 0f));
            CreatePrimitive(gate.transform, PrimitiveType.Sphere, "Hub R02 Gate Core", materials.gateR02, new Vector3(0f, 0.82f, -0.04f), new Vector3(0.34f, 0.34f, 0.34f));
            return gate;
        }

        private static void CreateRuntimeHook(Transform player, Transform returnSpawn, Transform gate, Transform r02Gate, Camera camera)
        {
            var hook = new GameObject("Hub Runtime Hook");
            var controller = hook.AddComponent<HubSceneController>();
            controller.player = player;
            controller.returnSpawn = returnSpawn;
            controller.d020RegionGate = gate;
            controller.r02RegionGate = r02Gate;
            controller.fixedCamera = camera;
            controller.regionSceneName = FourfoldGameIds.UnitySceneD020VerticalSlice;
            controller.regionR02SceneName = FourfoldGameIds.UnitySceneR02CinderCanal;
            controller.regionR02Playable = false;
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
                throw new InvalidOperationException($"Required object missing from Hub Crossroads scene: {name}");
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

        private sealed class GeneratedMaterials
        {
            public Material floor;
            public Material floorEdge;
            public Material player;
            public Material playerAccent;
            public Material gate;
            public Material gateR02;
            public Material gateStone;
            public Material marker;
            public Material shadow;
        }
    }
}
