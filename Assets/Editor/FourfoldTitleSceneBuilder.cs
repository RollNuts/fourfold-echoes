using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldTitleSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Title.unity";
        private const string MaterialFolder = "Assets/Art/Generated/Title/Materials";

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
            scene.name = FourfoldGameIds.UnitySceneTitle;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.12f, 0.13f, 0.17f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.08f, 0.10f);
            RenderSettings.ambientGroundColor = new Color(0.035f, 0.032f, 0.04f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.07f, 0.08f, 0.11f);
            RenderSettings.fogDensity = 0.018f;

            CreateLighting();
            var camera = CreateCamera();
            CreateTitleSet(materials);
            CreateRuntimeHook(camera);

            EditorSceneManager.SaveScene(scene, ScenePath);
            ApplyBuildSettings();
            Selection.activeObject = camera.gameObject;
            AssetDatabase.SaveAssets();
            Debug.Log($"FOURFOLD Title scene generated at {ScenePath}");
        }

        public static void ValidateGeneratedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"Title scene is missing or invalid: {ScenePath}");
            }

            Require("Title Runtime Hook");
            Require("Title Main Camera");
            Require("Title Golden Gate");
            Require("Title Fourfold Mark");
            RequireComponent<TitleSceneController>("Title Runtime Hook");
            ValidateRuntimeHookReferences();
            ValidateBuildSettings();

            if (Camera.main == null)
            {
                throw new InvalidOperationException("Title scene has no MainCamera tagged camera.");
            }

            Debug.Log("FOURFOLD Title scene validation passed.");
        }

        public static void ApplyBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(FourfoldHubSceneBuilder.ScenePath, true),
                new EditorBuildSettingsScene(FourfoldD020SliceSceneBuilder.ScenePath, true)
            };
        }

        public static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length < 3)
            {
                throw new InvalidOperationException("Build Settings must include Title, HubCrossroads, and D020VerticalSlice.");
            }

            if (scenes[0].path != ScenePath
                || scenes[1].path != FourfoldHubSceneBuilder.ScenePath
                || scenes[2].path != FourfoldD020SliceSceneBuilder.ScenePath
                || !scenes[0].enabled
                || !scenes[1].enabled
                || !scenes[2].enabled)
            {
                throw new InvalidOperationException("Build Settings order must be Title, HubCrossroads, then D020VerticalSlice.");
            }
        }

        private static void ValidateRuntimeHookReferences()
        {
            var hook = GameObject.Find("Title Runtime Hook");
            if (hook == null)
            {
                throw new InvalidOperationException("Title runtime hook is missing.");
            }

            var controller = hook.GetComponent<TitleSceneController>();
            if (controller == null || controller.titleCamera == null)
            {
                throw new InvalidOperationException("Title runtime hook is missing a controller or camera reference.");
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
                floor = CreateMaterial("Title_Floor", new Color(0.12f, 0.11f, 0.12f)),
                stone = CreateMaterial("Title_Stone", new Color(0.20f, 0.19f, 0.18f)),
                gold = CreateMaterial("Title_Gold", new Color(1.0f, 0.70f, 0.24f), new Color(0.45f, 0.22f, 0.04f)),
                blue = CreateMaterial("Title_Blue", new Color(0.20f, 0.44f, 0.95f), new Color(0.02f, 0.12f, 0.42f)),
                red = CreateMaterial("Title_Red", new Color(0.95f, 0.30f, 0.18f), new Color(0.45f, 0.06f, 0.03f)),
                green = CreateMaterial("Title_Green", new Color(0.30f, 0.76f, 0.34f), new Color(0.05f, 0.28f, 0.06f)),
                violet = CreateMaterial("Title_Violet", new Color(0.72f, 0.36f, 1.0f), new Color(0.24f, 0.08f, 0.46f))
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

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateLighting()
        {
            var key = new GameObject("Title Warm Key Light");
            var light = key.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.color = new Color(1.0f, 0.78f, 0.54f);
            key.transform.rotation = Quaternion.Euler(48f, -42f, 0f);

            CreatePointLight("Title Gate Glow", new Vector3(0f, 2.4f, 2.6f), new Color(1.0f, 0.58f, 0.18f), 6.0f, 3.4f);
            CreatePointLight("Title Fourfold Glow", new Vector3(0f, 2.0f, -1.6f), new Color(0.35f, 0.52f, 1.0f), 4.4f, 2.2f);
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
            var cameraObject = new GameObject("Title Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 7.8f, -7.2f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.8f, 0.9f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.060f, 0.075f);
            return camera;
        }

        private static void CreateTitleSet(GeneratedMaterials materials)
        {
            var root = new GameObject("Title Set");
            CreateBlock(root.transform, "Title Floor", materials.floor, new Vector3(0f, -0.10f, 0f), new Vector3(8.8f, 0.20f, 6.2f));
            CreateBlock(root.transform, "Title Back Wall", materials.stone, new Vector3(0f, 1.05f, 3.35f), new Vector3(9.2f, 2.1f, 0.35f));
            CreateBlock(root.transform, "Title Left Low Wall", materials.stone, new Vector3(-4.55f, 0.45f, 0f), new Vector3(0.35f, 0.9f, 6.4f));
            CreateBlock(root.transform, "Title Right Low Wall", materials.stone, new Vector3(4.55f, 0.45f, 0f), new Vector3(0.35f, 0.9f, 6.4f));
            CreateBlock(root.transform, "Title Golden Gate Left", materials.gold, new Vector3(-0.82f, 1.0f, 2.82f), new Vector3(0.32f, 2.0f, 0.22f));
            CreateBlock(root.transform, "Title Golden Gate Right", materials.gold, new Vector3(0.82f, 1.0f, 2.82f), new Vector3(0.32f, 2.0f, 0.22f));
            CreateBlock(root.transform, "Title Golden Gate Top", materials.gold, new Vector3(0f, 2.08f, 2.82f), new Vector3(1.96f, 0.28f, 0.24f));
            CreatePrimitive(root.transform, PrimitiveType.Sphere, "Title Golden Gate", materials.gold, new Vector3(0f, 1.08f, 2.70f), new Vector3(0.64f, 0.92f, 0.18f));

            var mark = new GameObject("Title Fourfold Mark");
            mark.transform.SetParent(root.transform);
            mark.transform.position = new Vector3(0f, 0.28f, -1.20f);
            CreatePrimitive(mark.transform, PrimitiveType.Cylinder, "Title Mark Ember", materials.red, new Vector3(-0.55f, 0f, 0f), new Vector3(0.34f, 0.06f, 0.34f));
            CreatePrimitive(mark.transform, PrimitiveType.Cylinder, "Title Mark Tide", materials.blue, new Vector3(0.55f, 0f, 0f), new Vector3(0.34f, 0.06f, 0.34f));
            CreatePrimitive(mark.transform, PrimitiveType.Cylinder, "Title Mark Bloom", materials.green, new Vector3(0f, 0f, -0.55f), new Vector3(0.34f, 0.06f, 0.34f));
            CreatePrimitive(mark.transform, PrimitiveType.Cylinder, "Title Mark Prism", materials.violet, new Vector3(0f, 0f, 0.55f), new Vector3(0.34f, 0.06f, 0.34f));
        }

        private static void CreateRuntimeHook(Camera camera)
        {
            var hook = new GameObject("Title Runtime Hook");
            var controller = hook.AddComponent<TitleSceneController>();
            controller.titleCamera = camera;
        }

        private static GameObject CreateBlock(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            return CreateBlock(parent, name, material, localPosition, localScale, Quaternion.identity);
        }

        private static GameObject CreateBlock(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            var block = CreatePrimitive(parent, PrimitiveType.Cube, name, material, localPosition, localScale);
            block.transform.localRotation = localRotation;
            return block;
        }

        private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localScale = localScale;
            var renderer = gameObject.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            return gameObject;
        }

        private static GameObject Require(string name)
        {
            var found = GameObject.Find(name);
            if (found == null)
            {
                throw new InvalidOperationException($"Title scene is missing required object: {name}");
            }

            return found;
        }

        private static T RequireComponent<T>(string name) where T : Component
        {
            var gameObject = Require(name);
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"Title scene object {name} is missing required component {typeof(T).Name}.");
            }

            return component;
        }

        private sealed class GeneratedMaterials
        {
            public Material floor;
            public Material stone;
            public Material gold;
            public Material blue;
            public Material red;
            public Material green;
            public Material violet;
        }
    }
}
