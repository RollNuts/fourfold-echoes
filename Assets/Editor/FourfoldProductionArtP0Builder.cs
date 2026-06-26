using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldProductionArtP0Builder
    {
        public const string RootFolder = "Assets/Art/Production/P0";
        public const string MaterialFolder = RootFolder + "/Materials";
        public const string MeshFolder = RootFolder + "/Meshes";
        public const string PrefabFolder = RootFolder + "/Prefabs";
        public const string PreviewScenePath = "Assets/Scenes/ProductionArtP0Preview.unity";

        public const string HeroPrefabPath = PrefabFolder + "/FE_HERO_P0.prefab";
        public const string ToolPrefabPath = PrefabFolder + "/FE_TOOL_LUMEN_ROD_P0.prefab";
        public const string EnemyPrefabPath = PrefabFolder + "/FE_ENEMY_MELEE_SHARDLING_P0.prefab";
        public const string PedestalPrefabPath = PrefabFolder + "/FE_PROP_TOOL_PEDESTAL_P0.prefab";
        public const string RewardChestPrefabPath = PrefabFolder + "/FE_PROP_REWARD_CHEST_P0.prefab";
        public const string ManifestPath = RootFolder + "/generated-assets-manifest.json";

        private const int PreviewWidth = 1280;
        private const int PreviewHeight = 800;

        public static void BuildAndValidate()
        {
            BuildAssetsOnly();
            BuildPreviewScene();
            CapturePreview();
            ValidateAssets();
        }

        public static void BuildAssetsOnly()
        {
            EnsureFolders();
            var materials = BuildMaterials();
            var meshes = BuildMeshes();

            SaveHeroPrefab(materials, meshes);
            SaveToolPrefab(materials, meshes);
            SaveEnemyPrefab(materials, meshes);
            SavePedestalPrefab(materials, meshes);
            SaveRewardChestPrefab(materials, meshes);
            WriteManifest();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FOURFOLD production art P0 assets generated.");
        }

        public static void ValidateAssets()
        {
            RequireAsset<GameObject>(HeroPrefabPath);
            RequireAsset<GameObject>(ToolPrefabPath);
            RequireAsset<GameObject>(EnemyPrefabPath);
            RequireAsset<GameObject>(PedestalPrefabPath);
            RequireAsset<GameObject>(RewardChestPrefabPath);

            var prefabs = new[]
            {
                HeroPrefabPath,
                ToolPrefabPath,
                EnemyPrefabPath,
                PedestalPrefabPath,
                RewardChestPrefabPath
            };

            foreach (var prefabPath in prefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var bounds = CalculateBounds(prefab);
                if (bounds.size.y <= 0f || bounds.size.y > 4.5f || bounds.size.x > 3.5f || bounds.size.z > 3.5f)
                {
                    throw new InvalidOperationException($"P0 prefab bounds look wrong: {prefabPath} {bounds.size}");
                }

                var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    throw new InvalidOperationException($"P0 prefab has no renderers: {prefabPath}");
                }

                foreach (var renderer in renderers)
                {
                    if (renderer.sharedMaterial == null)
                    {
                        throw new InvalidOperationException($"P0 prefab has missing material: {prefabPath}/{renderer.name}");
                    }
                }

                var colliders = prefab.GetComponentsInChildren<Collider>(true);
                if (colliders.Length == 0)
                {
                    throw new InvalidOperationException($"P0 prefab has no collider: {prefabPath}");
                }
            }

            Debug.Log("FOURFOLD production art P0 validation passed.");
        }

        public static void BuildPreviewScene()
        {
            BuildAssetsOnly();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ProductionArtP0Preview";

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.22f, 0.22f, 0.25f);
            RenderSettings.ambientEquatorColor = new Color(0.14f, 0.13f, 0.12f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.055f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.08f, 0.09f, 0.11f);
            RenderSettings.fogDensity = 0.01f;

            var key = new GameObject("P0 Warm Key Light");
            key.transform.rotation = Quaternion.Euler(52f, -36f, 0f);
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1f, 0.82f, 0.58f);
            keyLight.intensity = 1.2f;
            keyLight.shadows = LightShadows.Soft;

            var floorMat = CreateMaterial("FE_MAT_P0_PreviewFloor", new Color(0.34f, 0.31f, 0.27f), 0.04f, 0.42f, null);
            CreateCube("P0 Preview Floor", floorMat, new Vector3(0f, -0.1f, 0f), new Vector3(8.4f, 0.18f, 3.2f));

            InstantiatePrefab(HeroPrefabPath, "P0 Hero Preview", new Vector3(-3.2f, 0f, 0f), Quaternion.Euler(0f, 36f, 0f));
            InstantiatePrefab(ToolPrefabPath, "P0 Tool Preview", new Vector3(-1.55f, 0f, 0f), Quaternion.Euler(0f, 30f, 0f));
            InstantiatePrefab(EnemyPrefabPath, "P0 Enemy Preview", new Vector3(0.3f, 0f, 0f), Quaternion.Euler(0f, 220f, 0f));
            InstantiatePrefab(PedestalPrefabPath, "P0 Pedestal Preview", new Vector3(2.05f, 0f, 0f), Quaternion.identity);
            InstantiatePrefab(RewardChestPrefabPath, "P0 Reward Preview", new Vector3(3.6f, 0f, 0f), Quaternion.Euler(0f, -18f, 0f));

            var cameraObject = new GameObject("P0 Preview Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(5.8f, 7.2f, -6.4f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.15f, 0.6f, 0.0f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);

            EditorSceneManager.SaveScene(scene, PreviewScenePath);
            Selection.activeObject = cameraObject;
            Debug.Log($"FOURFOLD production art P0 preview scene generated at {PreviewScenePath}");
        }

        public static void CapturePreview()
        {
            if (!File.Exists(PreviewScenePath))
            {
                BuildPreviewScene();
            }

            EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
            var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("P0 preview capture requires a camera.");
            }

            var outputDirectory = "artifacts/Previews";
            Directory.CreateDirectory(outputDirectory);
            CaptureCamera(camera, Path.Combine(outputDirectory, "production-art-p0-contact-sheet.png"));
            Debug.Log("FOURFOLD production art P0 preview captured.");
        }

        private static void SaveHeroPrefab(GeneratedMaterials materials, GeneratedMeshes meshes)
        {
            var root = new GameObject("FE_HERO_P0");
            AddPart(root.transform, "FE_HERO_P0_ControlRing", meshes.disc, materials.heroAura, new Vector3(0f, 0.035f, 0f), Quaternion.identity, new Vector3(1.35f, 0.05f, 1.35f));
            AddPart(root.transform, "FE_HERO_P0_LeftChunkBoot", meshes.box, materials.dark, new Vector3(-0.22f, 0.17f, 0.16f), Quaternion.Euler(0f, -8f, 0f), new Vector3(0.34f, 0.34f, 0.52f));
            AddPart(root.transform, "FE_HERO_P0_RightChunkBoot", meshes.box, materials.dark, new Vector3(0.23f, 0.17f, -0.10f), Quaternion.Euler(0f, 8f, 0f), new Vector3(0.34f, 0.34f, 0.52f));
            AddPart(root.transform, "FE_HERO_P0_RobeSkirt", meshes.taperedBody, materials.heroCloak, new Vector3(0f, 0.55f, 0f), Quaternion.identity, new Vector3(0.92f, 0.62f, 0.70f));
            AddPart(root.transform, "FE_HERO_P0_RoundedTunic", meshes.sphere, materials.heroIvory, new Vector3(0f, 1.02f, 0f), Quaternion.identity, new Vector3(0.82f, 0.86f, 0.66f));
            AddPart(root.transform, "FE_HERO_P0_BackCloakShell", meshes.cloak, materials.heroCloak, new Vector3(-0.18f, 0.92f, -0.40f), Quaternion.Euler(0f, 0f, -2f), new Vector3(1.18f, 1.26f, 0.42f));
            AddPart(root.transform, "FE_HERO_P0_LeftMitten", meshes.sphere, materials.heroCloak, new Vector3(-0.52f, 1.06f, 0.14f), Quaternion.identity, new Vector3(0.30f, 0.34f, 0.26f));
            AddPart(root.transform, "FE_HERO_P0_RightMitten", meshes.sphere, materials.heroCloak, new Vector3(0.54f, 1.04f, -0.10f), Quaternion.identity, new Vector3(0.30f, 0.34f, 0.26f));
            AddPart(root.transform, "FE_HERO_P0_BigHead", meshes.sphere, materials.heroIvory, new Vector3(0f, 1.64f, 0.02f), Quaternion.identity, new Vector3(0.58f, 0.58f, 0.52f));
            AddPart(root.transform, "FE_HERO_P0_HoodCap", meshes.sphere, materials.heroCloak, new Vector3(-0.02f, 1.72f, -0.08f), Quaternion.identity, new Vector3(0.68f, 0.48f, 0.56f));
            AddPart(root.transform, "FE_HERO_P0_FaceMask", meshes.box, materials.dark, new Vector3(0.02f, 1.58f, 0.31f), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.34f, 0.18f, 0.045f));
            AddPart(root.transform, "FE_HERO_P0_BrowCrest", meshes.blade, materials.gold, new Vector3(0f, 1.98f, 0.02f), Quaternion.Euler(82f, 0f, 0f), new Vector3(0.44f, 0.24f, 0.22f));
            AddPart(root.transform, "FE_HERO_P0_ChunkSwordBlade", meshes.blade, materials.steel, new Vector3(0.64f, 1.02f, 0.36f), Quaternion.Euler(18f, 0f, -26f), new Vector3(0.34f, 1.08f, 0.20f));
            AddPart(root.transform, "FE_HERO_P0_ChunkSwordGuard", meshes.box, materials.gold, new Vector3(0.45f, 0.62f, 0.26f), Quaternion.Euler(18f, 0f, -26f), new Vector3(0.46f, 0.10f, 0.12f));
            AddPart(root.transform, "FE_HERO_P0_ChunkSwordGrip", meshes.box, materials.dark, new Vector3(0.36f, 0.48f, 0.22f), Quaternion.Euler(18f, 0f, -26f), new Vector3(0.16f, 0.34f, 0.12f));
            AddPart(root.transform, "FE_HERO_P0_ToolSocketGem", meshes.lowPolyGem, materials.toolGlow, new Vector3(0.74f, 1.20f, -0.20f), Quaternion.identity, new Vector3(0.28f, 0.34f, 0.28f));
            AddBoxCollider(root, new Vector3(0f, 0.92f, 0f), new Vector3(0.92f, 1.84f, 0.86f));
            SavePrefab(root, HeroPrefabPath);
        }

        private static void SaveToolPrefab(GeneratedMaterials materials, GeneratedMeshes meshes)
        {
            var root = new GameObject("FE_TOOL_LUMEN_ROD_P0");
            AddPart(root.transform, "FE_TOOL_P0_Grip", meshes.box, materials.dark, new Vector3(0f, 0.45f, 0f), Quaternion.identity, new Vector3(0.16f, 0.82f, 0.16f));
            AddPart(root.transform, "FE_TOOL_P0_CrescentA", meshes.blade, materials.gold, new Vector3(-0.15f, 0.96f, 0f), Quaternion.Euler(0f, 0f, 45f), new Vector3(0.18f, 0.52f, 0.12f));
            AddPart(root.transform, "FE_TOOL_P0_CrescentB", meshes.blade, materials.gold, new Vector3(0.15f, 0.96f, 0f), Quaternion.Euler(0f, 0f, -45f), new Vector3(0.18f, 0.52f, 0.12f));
            AddPart(root.transform, "FE_TOOL_P0_Core", meshes.lowPolyGem, materials.toolGlow, new Vector3(0f, 1.18f, 0f), Quaternion.identity, new Vector3(0.28f, 0.34f, 0.28f));
            AddPart(root.transform, "FE_TOOL_P0_FootRing", meshes.disc, materials.toolAura, new Vector3(0f, 0.04f, 0f), Quaternion.identity, new Vector3(0.82f, 0.04f, 0.82f));
            AddBoxCollider(root, new Vector3(0f, 0.62f, 0f), new Vector3(0.72f, 1.24f, 0.72f));
            SavePrefab(root, ToolPrefabPath);
        }

        private static void SaveEnemyPrefab(GeneratedMaterials materials, GeneratedMeshes meshes)
        {
            var root = new GameObject("FE_ENEMY_MELEE_SHARDLING_P0");
            AddPart(root.transform, "FE_ENEMY_P0_DangerRing", meshes.disc, materials.enemyTell, new Vector3(0f, 0.035f, -0.55f), Quaternion.identity, new Vector3(1.34f, 0.04f, 1.34f));
            AddPart(root.transform, "FE_ENEMY_P0_LeftClawFoot", meshes.blade, materials.enemyArmor, new Vector3(-0.42f, 0.20f, 0.30f), Quaternion.Euler(72f, 0f, 22f), new Vector3(0.34f, 0.48f, 0.20f));
            AddPart(root.transform, "FE_ENEMY_P0_RightClawFoot", meshes.blade, materials.enemyArmor, new Vector3(0.42f, 0.20f, -0.24f), Quaternion.Euler(72f, 0f, -22f), new Vector3(0.34f, 0.48f, 0.20f));
            AddPart(root.transform, "FE_ENEMY_P0_HunchedBody", meshes.sphere, materials.enemyInk, new Vector3(0f, 0.82f, 0f), Quaternion.identity, new Vector3(1.20f, 1.08f, 0.98f));
            AddPart(root.transform, "FE_ENEMY_P0_ArmorBelly", meshes.taperedBody, materials.enemyArmor, new Vector3(0f, 0.86f, -0.38f), Quaternion.Euler(8f, 0f, 0f), new Vector3(0.70f, 0.66f, 0.20f));
            AddPart(root.transform, "FE_ENEMY_P0_ShardHead", meshes.sphere, materials.enemyArmor, new Vector3(0f, 1.52f, -0.08f), Quaternion.identity, new Vector3(0.68f, 0.54f, 0.52f));
            AddPart(root.transform, "FE_ENEMY_P0_MaskFace", meshes.box, materials.enemyTell, new Vector3(0f, 1.42f, -0.42f), Quaternion.identity, new Vector3(0.42f, 0.20f, 0.055f));
            AddPart(root.transform, "FE_ENEMY_P0_LeftHorn", meshes.blade, materials.enemyTell, new Vector3(-0.42f, 1.90f, -0.02f), Quaternion.Euler(0f, 0f, 42f), new Vector3(0.22f, 0.58f, 0.14f));
            AddPart(root.transform, "FE_ENEMY_P0_RightHorn", meshes.blade, materials.enemyTell, new Vector3(0.42f, 1.90f, -0.02f), Quaternion.Euler(0f, 0f, -42f), new Vector3(0.22f, 0.58f, 0.14f));
            AddPart(root.transform, "FE_ENEMY_P0_LeftShoulderShard", meshes.blade, materials.enemyArmor, new Vector3(-0.76f, 1.18f, 0.04f), Quaternion.Euler(0f, 0f, 60f), new Vector3(0.36f, 0.78f, 0.22f));
            AddPart(root.transform, "FE_ENEMY_P0_RightShoulderShard", meshes.blade, materials.enemyArmor, new Vector3(0.76f, 1.18f, 0.04f), Quaternion.Euler(0f, 0f, -60f), new Vector3(0.36f, 0.78f, 0.22f));
            AddPart(root.transform, "D020 Enemy Heavy Club Read", meshes.club, materials.enemyArmor, new Vector3(0.96f, 0.90f, -0.44f), Quaternion.Euler(18f, 0f, -26f), new Vector3(0.54f, 1.08f, 0.36f));
            AddPart(root.transform, "FE_ENEMY_P0_TellCore", meshes.lowPolyGem, materials.enemyTell, new Vector3(0f, 1.08f, -0.56f), Quaternion.identity, new Vector3(0.46f, 0.42f, 0.24f));
            AddBoxCollider(root, new Vector3(0f, 0.82f, 0f), new Vector3(1.10f, 1.64f, 0.95f));
            SavePrefab(root, EnemyPrefabPath);
        }

        private static void SavePedestalPrefab(GeneratedMaterials materials, GeneratedMeshes meshes)
        {
            var root = new GameObject("FE_PROP_TOOL_PEDESTAL_P0");
            AddPart(root.transform, "FE_PEDESTAL_P0_Footprint", meshes.disc, materials.toolAura, new Vector3(0f, 0.03f, 0f), Quaternion.identity, new Vector3(1.05f, 0.04f, 1.05f));
            AddPart(root.transform, "FE_PEDESTAL_P0_Base", meshes.taperedBody, materials.stoneDark, new Vector3(0f, 0.25f, 0f), Quaternion.identity, new Vector3(0.78f, 0.48f, 0.78f));
            AddPart(root.transform, "FE_PEDESTAL_P0_GlyphA", meshes.blade, materials.gold, new Vector3(-0.13f, 0.72f, 0f), Quaternion.Euler(0f, 0f, 42f), new Vector3(0.14f, 0.50f, 0.10f));
            AddPart(root.transform, "FE_PEDESTAL_P0_GlyphB", meshes.blade, materials.gold, new Vector3(0.15f, 0.72f, 0f), Quaternion.Euler(0f, 0f, -42f), new Vector3(0.14f, 0.50f, 0.10f));
            AddPart(root.transform, "FE_PEDESTAL_P0_ActiveCore", meshes.lowPolyGem, materials.toolGlow, new Vector3(0f, 1.08f, 0f), Quaternion.identity, new Vector3(0.26f, 0.30f, 0.26f));
            AddBoxCollider(root, new Vector3(0f, 0.45f, 0f), new Vector3(0.92f, 0.90f, 0.92f));
            SavePrefab(root, PedestalPrefabPath);
        }

        private static void SaveRewardChestPrefab(GeneratedMaterials materials, GeneratedMeshes meshes)
        {
            var root = new GameObject("FE_PROP_REWARD_CHEST_P0");
            AddPart(root.transform, "FE_CHEST_P0_Footprint", meshes.disc, materials.rewardAura, new Vector3(0f, 0.03f, 0f), Quaternion.identity, new Vector3(1.28f, 0.04f, 1.28f));
            AddPart(root.transform, "FE_CHEST_P0_Base", meshes.box, materials.chestWood, new Vector3(0f, 0.28f, 0f), Quaternion.identity, new Vector3(1.05f, 0.48f, 0.74f));
            AddPart(root.transform, "FE_CHEST_P0_LidOpen", meshes.wedge, materials.gold, new Vector3(0f, 0.66f, -0.14f), Quaternion.Euler(-18f, 0f, 0f), new Vector3(1.10f, 0.24f, 0.80f));
            AddPart(root.transform, "FE_RELIC_SPARK_P0", meshes.lowPolyGem, materials.relicBlue, new Vector3(-0.18f, 1.02f, 0f), Quaternion.identity, new Vector3(0.24f, 0.36f, 0.24f));
            AddPart(root.transform, "FE_RELIC_SEAL_P0", meshes.lowPolyGem, materials.relicBlueAlt, new Vector3(0.22f, 1.02f, 0.02f), Quaternion.Euler(0f, 24f, 0f), new Vector3(0.20f, 0.30f, 0.20f));
            AddPart(root.transform, "D020 Reward Vertical Beam", meshes.disc, materials.relicBeam, new Vector3(0f, 1.86f, 0f), Quaternion.identity, new Vector3(0.13f, 1.35f, 0.13f));
            AddPart(root.transform, "D020 Reward Beacon", meshes.lowPolyGem, materials.relicBlue, new Vector3(0f, 3.18f, 0f), Quaternion.identity, new Vector3(0.22f, 0.34f, 0.22f));
            AddBoxCollider(root, new Vector3(0f, 0.42f, 0f), new Vector3(1.20f, 0.84f, 0.86f));
            SavePrefab(root, RewardChestPrefabPath);
        }

        private static GeneratedMaterials BuildMaterials()
        {
            return new GeneratedMaterials
            {
                heroIvory = CreateMaterial("FE_MAT_P0_HeroIvory", new Color(0.88f, 0.72f, 0.48f), 0.05f, 0.44f, null),
                heroCloak = CreateMaterial("FE_MAT_P0_HeroCloakBlue", new Color(0.14f, 0.22f, 0.60f), 0f, 0.36f, new Color(0.01f, 0.03f, 0.14f)),
                heroAura = CreateMaterial("FE_MAT_P0_HeroControlRing", new Color(0.20f, 0.64f, 1.0f), 0f, 0.50f, new Color(0.03f, 0.22f, 0.55f)),
                steel = CreateMaterial("FE_MAT_P0_SwordSteel", new Color(0.78f, 0.82f, 0.84f), 0.25f, 0.62f, null),
                dark = CreateMaterial("FE_MAT_P0_DarkLeather", new Color(0.12f, 0.10f, 0.09f), 0.05f, 0.35f, null),
                gold = CreateMaterial("FE_MAT_P0_WarmGold", new Color(0.94f, 0.70f, 0.30f), 0.08f, 0.50f, new Color(0.30f, 0.16f, 0.03f)),
                toolGlow = CreateMaterial("FE_MAT_P0_ToolGlow", new Color(0.88f, 0.96f, 0.58f), 0f, 0.52f, new Color(0.55f, 0.46f, 0.10f)),
                toolAura = CreateMaterial("FE_MAT_P0_ToolAura", new Color(0.56f, 0.95f, 0.86f), 0f, 0.45f, new Color(0.10f, 0.42f, 0.34f)),
                enemyInk = CreateMaterial("FE_MAT_P0_EnemyInk", new Color(0.055f, 0.048f, 0.065f), 0.08f, 0.34f, null),
                enemyArmor = CreateMaterial("FE_MAT_P0_EnemyArmor", new Color(0.18f, 0.14f, 0.12f), 0.13f, 0.42f, null),
                enemyTell = CreateMaterial("FE_MAT_P0_EnemyTell", new Color(1.0f, 0.18f, 0.08f), 0f, 0.50f, new Color(0.70f, 0.06f, 0.02f)),
                stoneDark = CreateMaterial("FE_MAT_P0_StoneDark", new Color(0.17f, 0.16f, 0.16f), 0.02f, 0.36f, null),
                chestWood = CreateMaterial("FE_MAT_P0_ChestWood", new Color(0.42f, 0.24f, 0.12f), 0f, 0.40f, null),
                rewardAura = CreateMaterial("FE_MAT_P0_RewardAura", new Color(0.20f, 0.70f, 1.0f), 0f, 0.50f, new Color(0.04f, 0.26f, 0.56f)),
                relicBlue = CreateMaterial("FE_MAT_P0_RelicBlue", new Color(0.22f, 0.72f, 0.96f), 0f, 0.54f, new Color(0.04f, 0.28f, 0.58f)),
                relicBlueAlt = CreateMaterial("FE_MAT_P0_RelicBlueAlt", new Color(0.42f, 0.86f, 1.0f), 0f, 0.50f, new Color(0.08f, 0.32f, 0.60f)),
                relicBeam = CreateMaterial("FE_MAT_P0_RelicBeam", new Color(0.34f, 0.80f, 1.0f), 0f, 0.65f, new Color(0.05f, 0.42f, 0.80f))
            };
        }

        private static GeneratedMeshes BuildMeshes()
        {
            return new GeneratedMeshes
            {
                box = BuiltinMesh("Cube"),
                disc = BuiltinMesh("Cylinder"),
                sphere = BuiltinMesh("Sphere"),
                taperedBody = SaveMesh("FE_MESH_P0_TaperedBody", CreateTaperedPrism(0.70f, 0.52f, 1.0f, 0.62f)),
                lowPolyGem = SaveMesh("FE_MESH_P0_LowPolyGem", CreateBipyramid(0.5f, 1.0f)),
                blade = SaveMesh("FE_MESH_P0_TaperedBlade", CreateBlade()),
                cloak = SaveMesh("FE_MESH_P0_CloakWedge", CreateCloakWedge()),
                wedge = SaveMesh("FE_MESH_P0_ChestLidWedge", CreateWedge()),
                club = SaveMesh("FE_MESH_P0_EnemyClub", CreateClub()),
                toolRod = SaveMesh("FE_MESH_P0_ToolRod", CreateToolRod())
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

        private static Mesh SaveMesh(string name, Mesh source)
        {
            var path = $"{MeshFolder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh();
                AssetDatabase.CreateAsset(mesh, path);
            }

            mesh.Clear();
            mesh.name = name;
            mesh.vertices = source.vertices;
            mesh.triangles = source.triangles;
            mesh.uv = source.uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static Mesh BuiltinMesh(string primitiveName)
        {
            var primitive = GameObject.CreatePrimitive(primitiveName switch
            {
                "Cylinder" => PrimitiveType.Cylinder,
                "Sphere" => PrimitiveType.Sphere,
                _ => PrimitiveType.Cube
            });
            var mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(primitive);
            return mesh;
        }

        private static GameObject AddPart(Transform parent, string name, Mesh mesh, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = localRotation;
            gameObject.transform.localScale = localScale;
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            return gameObject;
        }

        private static void AddBoxCollider(GameObject target, Vector3 center, Vector3 size)
        {
            var collider = target.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static GameObject InstantiatePrefab(string prefabPath, string name, Vector3 position, Quaternion rotation)
        {
            var prefab = RequireAsset<GameObject>(prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            return instance;
        }

        private static void CreateCube(string name, Material material, Vector3 position, Vector3 scale)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Required P0 production art asset missing: {path}");
            }

            return asset;
        }

        private static Bounds CalculateBounds(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static void WriteManifest()
        {
            Directory.CreateDirectory(RootFolder);
            File.WriteAllText(ManifestPath, string.Join(Environment.NewLine, new[]
            {
                "{",
                "  \"version\": 1,",
                "  \"batch_id\": \"production_art_p0\",",
                "  \"assets\": [",
                AssetJson("asset.hero.p0", "character", HeroPrefabPath, "box", 258, 9, "box", true) + ",",
                AssetJson("asset.tool.lumen_rod.p0", "tool", ToolPrefabPath, "box", 178, 5, "box", false) + ",",
                AssetJson("asset.enemy.shardling.p0", "enemy", EnemyPrefabPath, "box", 184, 7, "box", true) + ",",
                AssetJson("asset.prop.tool_pedestal.p0", "prop", PedestalPrefabPath, "box", 158, 5, "box", false) + ",",
                AssetJson("asset.prop.reward_chest.p0", "reward", RewardChestPrefabPath, "box", 174, 7, "box", false),
                "  ]",
                "}"
            }) + Environment.NewLine);
        }

        private static string AssetJson(string assetId, string category, string prefabPath, string collisionType, int triangleBudget, int materialBudget, string colliderType, bool rigReady)
        {
            return "    {"
                + $"\"asset_id\":\"{assetId}\","
                + $"\"category\":\"{category}\","
                + "\"source_strategy\":\"PROCEDURAL\","
                + $"\"source_script\":\"Assets/Editor/{nameof(FourfoldProductionArtP0Builder)}.cs\","
                + $"\"output_prefab\":\"{prefabPath}\","
                + "\"scale_meters\":\"1 Unity unit = 1 meter\","
                + "\"pivot_rule\":\"root at floor contact center\","
                + $"\"triangle_budget_lod0\":{triangleBudget},"
                + "\"triangle_budget_lod1\":0,"
                + "\"triangle_budget_lod2\":0,"
                + $"\"material_budget\":{materialBudget},"
                + "\"texture_size\":\"flat_materials\","
                + $"\"collision_type\":\"{collisionType}\","
                + "\"lod_count\":0,"
                + "\"preview_images\":[\"artifacts/Previews/production-art-p0-contact-sheet.png\"],"
                + "\"license\":\"repository_authored\","
                + "\"attribution\":\"not_required\","
                + $"\"rig_required\":{(rigReady ? "true" : "false")},"
                + $"\"validation_result\":\"pending\""
                + "}";
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(RootFolder);
            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory(MeshFolder);
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("artifacts/Previews");
            AssetDatabase.Refresh();
        }

        private static Mesh CreateTaperedPrism(float bottomWidth, float topWidth, float height, float depth)
        {
            var y0 = -height * 0.5f;
            var y1 = height * 0.5f;
            var b = bottomWidth * 0.5f;
            var t = topWidth * 0.5f;
            var d = depth * 0.5f;
            return MeshFrom(
                new[]
                {
                    new Vector3(-b, y0, -d), new Vector3(b, y0, -d), new Vector3(b, y0, d), new Vector3(-b, y0, d),
                    new Vector3(-t, y1, -d), new Vector3(t, y1, -d), new Vector3(t, y1, d), new Vector3(-t, y1, d)
                },
                BoxTriangles());
        }

        private static Mesh CreateBipyramid(float radius, float height)
        {
            var h = height * 0.5f;
            return MeshFrom(
                new[]
                {
                    new Vector3(0f, h, 0f),
                    new Vector3(radius, 0f, 0f),
                    new Vector3(0f, 0f, radius),
                    new Vector3(-radius, 0f, 0f),
                    new Vector3(0f, 0f, -radius),
                    new Vector3(0f, -h, 0f)
                },
                new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1, 5, 2, 1, 5, 3, 2, 5, 4, 3, 5, 1, 4 });
        }

        private static Mesh CreateBlade()
        {
            return MeshFrom(
                new[]
                {
                    new Vector3(-0.25f, -0.50f, -0.05f), new Vector3(0.25f, -0.50f, -0.05f), new Vector3(0.16f, 0.25f, -0.05f), new Vector3(0f, 0.55f, -0.05f), new Vector3(-0.16f, 0.25f, -0.05f),
                    new Vector3(-0.25f, -0.50f, 0.05f), new Vector3(0.25f, -0.50f, 0.05f), new Vector3(0.16f, 0.25f, 0.05f), new Vector3(0f, 0.55f, 0.05f), new Vector3(-0.16f, 0.25f, 0.05f)
                },
                new[]
                {
                    0, 1, 2, 0, 2, 4, 4, 2, 3,
                    5, 7, 6, 5, 9, 7, 9, 8, 7,
                    0, 5, 6, 0, 6, 1,
                    1, 6, 7, 1, 7, 2,
                    2, 7, 8, 2, 8, 3,
                    3, 8, 9, 3, 9, 4,
                    4, 9, 5, 4, 5, 0
                });
        }

        private static Mesh CreateCloakWedge()
        {
            return MeshFrom(
                new[]
                {
                    new Vector3(-0.42f, 0.50f, -0.05f), new Vector3(0.42f, 0.50f, -0.05f), new Vector3(0.32f, -0.50f, -0.06f), new Vector3(-0.32f, -0.50f, -0.06f),
                    new Vector3(-0.36f, 0.46f, 0.05f), new Vector3(0.36f, 0.46f, 0.05f), new Vector3(0.20f, -0.50f, 0.06f), new Vector3(-0.20f, -0.50f, 0.06f)
                },
                BoxTriangles());
        }

        private static Mesh CreateWedge()
        {
            return MeshFrom(
                new[]
                {
                    new Vector3(-0.5f, -0.18f, -0.35f), new Vector3(0.5f, -0.18f, -0.35f), new Vector3(0.5f, -0.18f, 0.35f), new Vector3(-0.5f, -0.18f, 0.35f),
                    new Vector3(-0.45f, 0.18f, -0.22f), new Vector3(0.45f, 0.18f, -0.22f), new Vector3(0.35f, 0.08f, 0.30f), new Vector3(-0.35f, 0.08f, 0.30f)
                },
                BoxTriangles());
        }

        private static Mesh CreateClub()
        {
            return MeshFrom(
                new[]
                {
                    new Vector3(-0.18f, -0.55f, -0.10f), new Vector3(0.18f, -0.55f, -0.10f), new Vector3(0.18f, -0.55f, 0.10f), new Vector3(-0.18f, -0.55f, 0.10f),
                    new Vector3(-0.30f, 0.55f, -0.18f), new Vector3(0.30f, 0.55f, -0.18f), new Vector3(0.30f, 0.55f, 0.18f), new Vector3(-0.30f, 0.55f, 0.18f)
                },
                BoxTriangles());
        }

        private static Mesh CreateToolRod()
        {
            return MeshFrom(
                new[]
                {
                    new Vector3(-0.10f, -0.50f, -0.08f), new Vector3(0.10f, -0.50f, -0.08f), new Vector3(0.10f, -0.50f, 0.08f), new Vector3(-0.10f, -0.50f, 0.08f),
                    new Vector3(-0.08f, 0.50f, -0.06f), new Vector3(0.08f, 0.50f, -0.06f), new Vector3(0.08f, 0.50f, 0.06f), new Vector3(-0.08f, 0.50f, 0.06f)
                },
                BoxTriangles());
        }

        private static int[] BoxTriangles()
        {
            return new[]
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7
            };
        }

        private static Mesh MeshFrom(Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = BuildUv(vertices.Length)
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector2[] BuildUv(int count)
        {
            var uv = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                uv[i] = new Vector2((i & 1) == 0 ? 0f : 1f, (i & 2) == 0 ? 0f : 1f);
            }

            return uv;
        }

        private static void CaptureCamera(Camera camera, string outputPath)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(PreviewWidth, PreviewHeight, 24);
            var texture = new Texture2D(PreviewWidth, PreviewHeight, TextureFormat.RGB24, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, PreviewWidth, PreviewHeight), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private sealed class GeneratedMaterials
        {
            public Material heroIvory;
            public Material heroCloak;
            public Material heroAura;
            public Material steel;
            public Material dark;
            public Material gold;
            public Material toolGlow;
            public Material toolAura;
            public Material enemyInk;
            public Material enemyArmor;
            public Material enemyTell;
            public Material stoneDark;
            public Material chestWood;
            public Material rewardAura;
            public Material relicBlue;
            public Material relicBlueAlt;
            public Material relicBeam;
        }

        private sealed class GeneratedMeshes
        {
            public Mesh box;
            public Mesh disc;
            public Mesh sphere;
            public Mesh taperedBody;
            public Mesh lowPolyGem;
            public Mesh blade;
            public Mesh cloak;
            public Mesh wedge;
            public Mesh club;
            public Mesh toolRod;
        }
    }
}
