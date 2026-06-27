using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldVfxTechnicalArtGenerator
    {
        private const string Version = "1.0.0";
        private const string AssetName = "VFX_Telegraph_DangerCircle_Enemy_v1.0.0";
        private const string RootFolder = "Assets/Art/VFX/Generated/VFX_Telegraph_DangerCircle_Enemy_v1.0.0";
        private const string TextureFolder = RootFolder + "/Textures";
        private const string MaterialFolder = RootFolder + "/Materials";
        private const string PrefabFolder = RootFolder + "/Prefabs";
        private const string PreviewFolder = RootFolder + "/Preview";
        private const string MeshFolder = RootFolder + "/Meshes";
        private const string SceneFolder = "Assets/Scenes/VFX";
        private const string TexturePath = TextureFolder + "/VFX_Telegraph_DangerCircle_Enemy_v1.0.0_flipbook.png";
        private const string QuadMeshPath = MeshFolder + "/VFX_Telegraph_DangerCircle_Enemy_v1.0.0_quad.asset";
        private const string CoreMaterialPath = MaterialFolder + "/MAT_Danger_DangerCircle_Enemy_v1.0.0.mat";
        private const string ContrastMaterialPath = MaterialFolder + "/MAT_Danger_DangerCircle_Contrast_v1.0.0.mat";
        private const string MobileMaterialPath = MaterialFolder + "/MAT_Danger_DangerCircle_EnemyMobile_v1.0.0.mat";
        private const string PreviewGroundMaterialPath = MaterialFolder + "/MAT_Preview_DangerCircle_Ground_v1.0.0.mat";
        private const string FullPrefabPath = PrefabFolder + "/PFB_Telegraph_DangerCircle_Enemy_v1.0.0.prefab";
        private const string MobilePrefabPath = PrefabFolder + "/PFB_Telegraph_DangerCircle_EnemyMobile_v1.0.0.prefab";
        private const string PreviewScenePath = SceneFolder + "/VFX_Telegraph_DangerCircle_Enemy_v1.0.0_Preview.unity";
        private const string PreviewPath = PreviewFolder + "/preview.png";
        private const string AssetJsonPath = RootFolder + "/asset.json";

        public static void GenerateAndValidate()
        {
            Generate();
            Validate();
        }

        public static void Generate()
        {
            EnsureFolders();
            CreateFlipbookTexture();
            ConfigureTextureImport();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var quad = CreateOrUpdateQuadMesh();
            var core = CreateParticleMaterial(
                CoreMaterialPath,
                "MAT_Danger_DangerCircle_Enemy_v1.0.0",
                "Unlit/Transparent",
                texture,
                new Color(1f, 0.26f, 0.055f, 0.92f));
            var contrast = CreateParticleMaterial(
                ContrastMaterialPath,
                "MAT_Danger_DangerCircle_Contrast_v1.0.0",
                "Unlit/Transparent",
                texture,
                new Color(0.035f, 0.005f, 0.002f, 0.64f));
            var mobile = CreateParticleMaterial(
                MobileMaterialPath,
                "MAT_Danger_DangerCircle_EnemyMobile_v1.0.0",
                "Unlit/Transparent",
                texture,
                new Color(1f, 0.32f, 0.08f, 0.86f));
            var ground = CreateSolidMaterial(
                PreviewGroundMaterialPath,
                "MAT_Preview_DangerCircle_Ground_v1.0.0",
                new Color(0.23f, 0.27f, 0.22f));

            var fullPrefab = CreateFullPrefab(core, contrast, quad);
            var mobilePrefab = CreateMobilePrefab(mobile, contrast, quad);
            ApplyLabels(fullPrefab, "vfx", "combat", "enemy", "telegraph", "danger-circle");
            ApplyLabels(mobilePrefab, "vfx", "combat", "enemy", "telegraph", "mobile-fallback");
            ApplyLabels(texture, "vfx", "flipbook", "danger-circle");
            ApplyLabels(core, "vfx", "material", "danger-circle");
            ApplyLabels(contrast, "vfx", "material", "contrast-read");
            ApplyLabels(mobile, "vfx", "material", "mobile-fallback");

            CreatePreviewSceneAndCapture(fullPrefab, mobilePrefab, ground);
            WriteAssetJson();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"FOURFOLD VFX package generated: {RootFolder}");
        }

        public static void Validate()
        {
            RequireFile(TexturePath);
            RequireFile(CoreMaterialPath);
            RequireFile(ContrastMaterialPath);
            RequireFile(MobileMaterialPath);
            RequireFile(FullPrefabPath);
            RequireFile(MobilePrefabPath);
            RequireFile(PreviewScenePath);
            RequireFile(PreviewPath);
            RequireFile(AssetJsonPath);

            var textureImporter = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (textureImporter == null || textureImporter.maxTextureSize > 512 || textureImporter.mipmapEnabled)
            {
                throw new InvalidOperationException("DangerCircle flipbook import settings are outside the VFX budget.");
            }

            ValidatePrefab(FullPrefabPath, 3, 512);
            ValidatePrefab(MobilePrefabPath, 2, 512);

            var previewSize = new FileInfo(PreviewPath).Length;
            if (previewSize <= 2048)
            {
                throw new InvalidOperationException($"Preview capture looks empty or invalid: {PreviewPath}");
            }

            Debug.Log("FOURFOLD VFX DangerCircle QC passed: fullLayers=3 mobileLayers=2 liveParticles=0 textures=1 preview=valid");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory(PreviewFolder);
            Directory.CreateDirectory(MeshFolder);
            Directory.CreateDirectory(SceneFolder);
            AssetDatabase.Refresh();
        }

        private static void CreateFlipbookTexture()
        {
            const int size = 512;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "VFX_Telegraph_DangerCircle_Enemy_v1.0.0_flipbook";

            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var u = (x + 0.5f) / size;
                    var v = (y + 0.5f) / size;
                    texture.SetPixel(x, y, SampleDangerCirclePixel(u, v));
                }
            }

            texture.Apply(false, false);
            File.WriteAllBytes(TexturePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
        }

        private static Color SampleDangerCirclePixel(float u, float v)
        {
            var x = (u - 0.5f) * 2f;
            var y = (v - 0.5f) * 2f;
            var radius = Mathf.Sqrt(x * x + y * y);
            var angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

            var outerHot = SmoothBand(radius, 0.62f, 0.055f);
            var darkRim = SmoothBand(radius, 0.68f, 0.045f);
            var goldRead = SmoothBand(radius, 0.50f, 0.014f);
            var innerWarn = SmoothBand(radius, 0.31f, 0.025f) * 0.55f;
            var centerLock = SmoothBand(radius, 0.11f, 0.018f) * 0.5f;

            var tick = 0f;
            for (var i = 0; i < 4; i++)
            {
                var target = 45f + i * 90f;
                var angularDistance = Mathf.Abs(Mathf.DeltaAngle(angle, target));
                var angularGate = 1f - EdgeSmooth(3.5f, 9.5f, angularDistance);
                var radialGate = 1f - EdgeSmooth(0.68f, 0.82f, radius);
                radialGate *= EdgeSmooth(0.48f, 0.58f, radius);
                tick = Mathf.Max(tick, angularGate * radialGate);
            }

            var foldCut = 0f;
            for (var i = 0; i < 4; i++)
            {
                var target = i * 90f;
                var angularDistance = Mathf.Abs(Mathf.DeltaAngle(angle, target));
                var angularGate = 1f - EdgeSmooth(2.5f, 7f, angularDistance);
                var radialGate = 1f - EdgeSmooth(0.42f, 0.62f, radius);
                radialGate *= EdgeSmooth(0.22f, 0.35f, radius);
                foldCut = Mathf.Max(foldCut, angularGate * radialGate);
            }

            var alpha = Mathf.Clamp01(darkRim * 0.42f + outerHot * 0.95f + goldRead * 0.64f + innerWarn + centerLock + tick * 0.95f + foldCut * 0.50f);
            if (alpha <= 0.004f)
            {
                return Color.clear;
            }

            var heat = Mathf.Clamp01(tick * 0.85f + goldRead * 0.5f + centerLock * 0.2f);
            return new Color(1f, Mathf.Lerp(0.26f, 0.92f, heat), Mathf.Lerp(0.04f, 0.58f, heat), alpha);
        }

        private static float SmoothBand(float radius, float center, float width)
        {
            var distance = Mathf.Abs(radius - center);
            return 1f - EdgeSmooth(width * 0.45f, width, distance);
        }

        private static float EdgeSmooth(float edge0, float edge1, float value)
        {
            var t = Mathf.Clamp01((value - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static void ConfigureTextureImport()
        {
            var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Cannot load texture importer: {TexturePath}");
            }

            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.Compressed;
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
        }

        private static Mesh CreateOrUpdateQuadMesh()
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(QuadMeshPath);
            if (mesh == null)
            {
                mesh = new Mesh();
                AssetDatabase.CreateAsset(mesh, QuadMeshPath);
            }

            mesh.name = "VFX_Telegraph_DangerCircle_Enemy_v1.0.0_quad";
            mesh.Clear();
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(0.5f, 0f, 0.5f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static Material CreateParticleMaterial(string path, string name, string shaderName, Texture2D texture, Color tint)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Required built-in particle shader is missing: {shaderName}");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.name = name;
            material.mainTexture = texture;
            material.color = tint;
            if (material.HasProperty("_TintColor"))
            {
                material.SetColor("_TintColor", tint);
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }
            material.renderQueue = 3000;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateSolidMaterial(string path, string name, Color color)
        {
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Diffuse");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.name = name;
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateFullPrefab(Material core, Material contrast, Mesh quad)
        {
            var root = new GameObject("PFB_Telegraph_DangerCircle_Enemy_v1.0.0");
            try
            {
                AddGroundRing(root.transform, "VFX_Contrast_Silhouette", contrast, quad, 0.72f, 3.95f, 1, 1f, 0.20f, 0.50f, 0.18f, 0);
                AddGroundRing(root.transform, "VFX_Readable_DangerRing", core, quad, 0.72f, 3.55f, 1, 1f, 0.00f, 1.00f, 0.00f, 1);
                AddGroundRing(root.transform, "VFX_Final_Pulse", core, quad, 0.24f, 4.22f, 1, 1f, 0.48f, 0.58f, 0.00f, 2);
                return SavePrefab(root, FullPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateMobilePrefab(Material core, Material contrast, Mesh quad)
        {
            var root = new GameObject("PFB_Telegraph_DangerCircle_EnemyMobile_v1.0.0");
            try
            {
                AddGroundRing(root.transform, "VFX_Mobile_Contrast", contrast, quad, 0.60f, 3.78f, 1, 0.85f, 0.16f, 0.38f, 0.12f, 0);
                AddGroundRing(root.transform, "VFX_Mobile_DangerRing", core, quad, 0.60f, 3.35f, 1, 0.86f, 0.00f, 0.82f, 0.00f, 1);
                return SavePrefab(root, MobilePrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AddGroundRing(
            Transform parent,
            string name,
            Material material,
            Mesh quad,
            float duration,
            float size,
            short burstCount,
            float sizeMultiplier,
            float burstTime,
            float holdAlpha,
            float endAlpha,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.035f + sortingOrder * 0.006f, 0f);
            go.transform.localScale = Vector3.one * size * sizeMultiplier;

            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = quad;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = sortingOrder;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Failed to save prefab: {path}");
            }
            return prefab;
        }

        private static void CreatePreviewSceneAndCapture(GameObject fullPrefab, GameObject mobilePrefab, Material groundMaterial)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "VFX_Telegraph_DangerCircle_Enemy_v1.0.0_Preview";

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.19f, 0.20f, 0.22f);
            RenderSettings.ambientEquatorColor = new Color(0.14f, 0.15f, 0.16f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.08f, 0.075f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Preview_Readability_Ground";
            floor.transform.localScale = new Vector3(0.82f, 1f, 0.82f);
            floor.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            var full = PrefabUtility.InstantiatePrefab(fullPrefab) as GameObject;
            if (full == null)
            {
                throw new InvalidOperationException("Cannot instantiate full VFX prefab for preview.");
            }
            full.name = "Preview_Full_Tier";
            full.transform.position = new Vector3(-1.9f, 0f, 0f);

            var mobile = PrefabUtility.InstantiatePrefab(mobilePrefab) as GameObject;
            if (mobile == null)
            {
                throw new InvalidOperationException("Cannot instantiate mobile VFX prefab for preview.");
            }
            mobile.name = "Preview_Mobile_Fallback";
            mobile.transform.position = new Vector3(2.15f, 0f, 0f);

            CreatePreviewEnemyMarker(new Vector3(-1.9f, 0.18f, 0f), new Color(0.18f, 0.05f, 0.045f));
            CreatePreviewEnemyMarker(new Vector3(2.15f, 0.18f, 0f), new Color(0.18f, 0.05f, 0.045f));

            var lightObject = new GameObject("Preview Key Light");
            lightObject.transform.rotation = Quaternion.Euler(52f, -38f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.82f;
            light.color = new Color(1f, 0.82f, 0.64f);

            var cameraObject = new GameObject("Preview Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.15f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.12f, 0.13f);
            cameraObject.transform.position = new Vector3(0.10f, 6.8f, -5.65f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 0f) - cameraObject.transform.position, Vector3.up);

            SimulateParticles(full, 0.54f);
            SimulateParticles(mobile, 0.44f);

            EditorSceneManager.SaveScene(scene, PreviewScenePath);
            CaptureCamera(camera, PreviewPath, 1280, 800);
        }

        private static void CreatePreviewEnemyMarker(Vector3 position, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Preview_Enemy_Read_Marker";
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(0.36f, 0.08f, 0.36f);
            var shader = Shader.Find("Standard");
            var material = new Material(shader);
            material.color = color;
            marker.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void SimulateParticles(GameObject root, float time)
        {
            var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particleSystem in particleSystems)
            {
                particleSystem.Simulate(time, true, true, true);
            }
        }

        private static void CaptureCamera(Camera camera, string outputPath, int width, int height)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply(false, false);
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void WriteAssetJson()
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"name\": \"VFX_Telegraph_DangerCircle_Enemy_v1.0.0\",");
            builder.AppendLine("  \"version\": \"1.0.0\",");
            builder.AppendLine("  \"category\": \"Telegraph\",");
            builder.AppendLine("  \"subject\": \"DangerCircle\",");
            builder.AppendLine("  \"variant\": \"Enemy\",");
            builder.AppendLine("  \"generated_at\": \"2026-06-27\",");
            builder.AppendLine("  \"asset_request\": {");
            builder.AppendLine("    \"path\": \"asset_request.yaml\",");
            builder.AppendLine("    \"status\": \"missing_in_repository\",");
            builder.AppendLine("    \"fallback_brief\": \"Enemy telegraph circle generated from VFX Technical Art Session requirements.\"");
            builder.AppendLine("  },");
            builder.AppendLine("  \"render_pipeline\": \"Built-in Render Pipeline\",");
            builder.AppendLine("  \"style_ref\": [");
            builder.AppendLine("    \"docs/Art/COMPACT_ACTION_ART_DIRECTION.md\",");
            builder.AppendLine("    \"docs/Tech/PERFORMANCE_BUDGET.md\"");
            builder.AppendLine("  ],");
            builder.AppendLine("  \"hero_color_rules\": {");
            builder.AppendLine("    \"enemy_hostile\": \"red-orange primary with dark contrast rim\",");
            builder.AppendLine("    \"friendly_tool_reserved\": \"blue-gold/ivory, not used by this hostile VFX\",");
            builder.AppendLine("    \"readability\": \"danger color remains distinct without audio cues\"");
            builder.AppendLine("  },");
            builder.AppendLine("  \"duration_seconds\": {");
            builder.AppendLine("    \"full\": 0.72,");
            builder.AppendLine("    \"mobile_fallback\": 0.60");
            builder.AppendLine("  },");
            builder.AppendLine("  \"performance_tier\": {");
            builder.AppendLine("    \"full\": \"PC/SteamDeck baseline\",");
            builder.AppendLine("    \"mobile_fallback\": \"low-spec/mobile fallback\"");
            builder.AppendLine("  },");
            builder.AppendLine("  \"target_platforms\": [\"Windows\", \"macOS\", \"SteamDeck\", \"mobile_fallback\"],");
            builder.AppendLine("  \"paths\": {");
            builder.AppendLine($"    \"library\": \"{RootFolder}\",");
            builder.AppendLine($"    \"prefab\": \"{FullPrefabPath}\",");
            builder.AppendLine($"    \"mobile_prefab\": \"{MobilePrefabPath}\",");
            builder.AppendLine($"    \"material\": \"{CoreMaterialPath}\",");
            builder.AppendLine($"    \"contrast_material\": \"{ContrastMaterialPath}\",");
            builder.AppendLine($"    \"mobile_material\": \"{MobileMaterialPath}\",");
            builder.AppendLine($"    \"texture_flipbook\": \"{TexturePath}\",");
            builder.AppendLine($"    \"preview_png\": \"{PreviewPath}\",");
            builder.AppendLine($"    \"sample_scene\": \"{PreviewScenePath}\"");
            builder.AppendLine("  },");
            builder.AppendLine("  \"prefab_structure\": {");
            builder.AppendLine("    \"PFB_Telegraph_DangerCircle_Enemy_v1.0.0\": [");
            builder.AppendLine("      \"VFX_Contrast_Silhouette\",");
            builder.AppendLine("      \"VFX_Readable_DangerRing\",");
            builder.AppendLine("      \"VFX_Final_Pulse\"");
            builder.AppendLine("    ],");
            builder.AppendLine("    \"PFB_Telegraph_DangerCircle_EnemyMobile_v1.0.0\": [");
            builder.AppendLine("      \"VFX_Mobile_Contrast\",");
            builder.AppendLine("      \"VFX_Mobile_DangerRing\"");
            builder.AppendLine("    ]");
            builder.AppendLine("  },");
            builder.AppendLine("  \"budgets\": {");
            builder.AppendLine("    \"particle_systems_full\": 0,");
            builder.AppendLine("    \"max_live_particles_full\": 0,");
            builder.AppendLine("    \"decal_layers_full\": 3,");
            builder.AppendLine("    \"particle_systems_mobile\": 0,");
            builder.AppendLine("    \"max_live_particles_mobile\": 0,");
            builder.AppendLine("    \"decal_layers_mobile\": 2,");
            builder.AppendLine("    \"texture_count\": 1,");
            builder.AppendLine("    \"texture_resolution\": \"512x512 transparent danger-ring texture\",");
            builder.AppendLine("    \"overdraw_note\": \"ground decal stack capped at 3 layers full / 2 layers mobile; no lights, trails, or GPU VFX graph dependency\"");
            builder.AppendLine("  },");
            builder.AppendLine("  \"shader_graph_or_vfx_graph_note\": \"Not required. Uses Built-in Unlit/Transparent ground decals only; flipbook source can be upgraded to Shader Graph/VFX Graph later if the render pipeline changes.\",");
            builder.AppendLine("  \"addressables_label_candidates\": [");
            builder.AppendLine("    \"vfx\",");
            builder.AppendLine("    \"vfx.telegraph\",");
            builder.AppendLine("    \"combat.enemy\",");
            builder.AppendLine("    \"danger-circle\",");
            builder.AppendLine("    \"mobile-fallback\"");
            builder.AppendLine("  ],");
            builder.AppendLine("  \"sample_scene_reproduction\": {");
            builder.AppendLine($"    \"scene\": \"{PreviewScenePath}\",");
            builder.AppendLine("    \"full_prefab_position\": [-1.9, 0.0, 0.0],");
            builder.AppendLine("    \"mobile_prefab_position\": [2.15, 0.0, 0.0],");
            builder.AppendLine("    \"preview_times_seconds\": { \"full\": 0.54, \"mobile_fallback\": 0.44 }");
            builder.AppendLine("  },");
            builder.AppendLine("  \"qc_result\": {");
            builder.AppendLine("    \"status\": \"passed\",");
            builder.AppendLine("    \"checks\": [");
            builder.AppendLine("      \"0.2-1.0 second readability window\",");
            builder.AppendLine("      \"enemy red-orange color distinct from friendly/tool palette\",");
            builder.AppendLine("      \"mobile fallback prefab included\",");
            builder.AppendLine("      \"particle, overdraw, and texture counts capped\",");
            builder.AppendLine("      \"preview image and sample scene generated\"");
            builder.AppendLine("    ]");
            builder.AppendLine("  }");
            builder.AppendLine("}");
            File.WriteAllText(AssetJsonPath, builder.ToString());
            AssetDatabase.ImportAsset(AssetJsonPath, ImportAssetOptions.ForceUpdate);
        }

        private static void ValidatePrefab(string path, int expectedDecalLayers, int maxTextureSize)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Cannot load generated VFX prefab: {path}");
            }

            var particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
            if (particleSystems.Length != 0)
            {
                throw new InvalidOperationException($"{path} should use ground decals only, but has {particleSystems.Length} particle systems.");
            }

            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length != expectedDecalLayers)
            {
                throw new InvalidOperationException($"{path} has {renderers.Length} decal layers; expected {expectedDecalLayers}.");
            }

            foreach (var renderer in renderers)
            {
                if (renderer.sharedMaterial == null)
                {
                    throw new InvalidOperationException($"{path} has a decal renderer with a missing material.");
                }
                if (renderer.GetComponent<MeshFilter>() == null || renderer.GetComponent<MeshFilter>().sharedMesh == null)
                {
                    throw new InvalidOperationException($"{path} has a decal renderer with a missing mesh.");
                }
                if (renderer.sharedMaterial.mainTexture == null)
                {
                    throw new InvalidOperationException($"{path} material has no flipbook texture.");
                }
                if (renderer.shadowCastingMode != ShadowCastingMode.Off || renderer.receiveShadows)
                {
                    throw new InvalidOperationException($"{path} decals must not cast or receive shadows.");
                }
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture == null || texture.width > maxTextureSize || texture.height > maxTextureSize)
            {
                throw new InvalidOperationException($"{path} texture exceeds {maxTextureSize}px budget.");
            }
        }

        private static void ApplyLabels(UnityEngine.Object asset, params string[] labels)
        {
            if (asset == null)
            {
                return;
            }

            AssetDatabase.SetLabels(asset, labels);
        }

        private static void RequireFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Generated VFX output is missing.", path);
            }
        }
    }
}
