using System.Collections.Generic;
using System.IO;
using FourfoldEchoes.BuilderPrototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor.BuilderPrototype
{
    public static class BuilderPrototypeSpineSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/BuilderPrototypeSpine.unity";
        private const string MaterialFolder = "Assets/Generated/BuilderPrototypeSpine/Materials";

        [MenuItem("FOURFOLD/Builder Prototype/Build Spine Scene")]
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
            scene.name = "BuilderPrototypeSpine";

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.18f, 0.2f);
            RenderSettings.ambientEquatorColor = new Color(0.095f, 0.105f, 0.11f);
            RenderSettings.ambientGroundColor = new Color(0.035f, 0.033f, 0.03f);
            RenderSettings.ambientIntensity = 0.72f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.028f, 0.032f, 0.036f);
            RenderSettings.fogDensity = 0.018f;

            CreateLights();
            var camera = CreateCamera();
            var roomRoot = CreateBlockRoom(materials);
            var player = CreatePlayer(materials);
            var buildAnchor = CreateHookAnchor("Build Hook Anchor", new Vector3(-4.5f, 0.34f, 3.05f), materials.buildHook);
            var combatAnchor = CreateHookAnchor("Combat Hook Anchor", new Vector3(4.5f, 0.34f, 3.05f), materials.combatHook);
            var lootAnchor = CreateHookAnchor("Loot Hook Anchor", new Vector3(-4.5f, 0.34f, -3.05f), materials.lootHook);
            var extractAnchor = CreateHookAnchor("Extract Hook Anchor", new Vector3(4.5f, 0.34f, -3.05f), materials.extractHook);
            var editableBlocksRoot = new GameObject("Editable Build Blocks");

            roomRoot.name = "Builder Prototype Block Room";

            var controllerRoot = new GameObject("Builder Prototype Spine Controller");
            var controller = controllerRoot.AddComponent<BuilderPrototypeSpineController>();
            controller.player = player.transform;
            controller.followCamera = camera;
            controller.buildHookAnchor = buildAnchor.transform;
            controller.combatHookAnchor = combatAnchor.transform;
            controller.lootHookAnchor = lootAnchor.transform;
            controller.extractHookAnchor = extractAnchor.transform;
            controller.editableBlocksRoot = editableBlocksRoot.transform;
            controller.placedBlockMaterial = materials.placedBlock;
            controller.buildCursorMaterial = materials.buildCursor;
            controller.combatTelegraphMaterial = materials.combatTelegraph;
            controller.combatSafeMarkerMaterial = materials.combatSafeMarker;
            controller.combatThreatenedMarkerMaterial = materials.combatThreatenedMarker;
            controller.combatUnsafeMarkerMaterial = materials.combatUnsafeMarker;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Builder prototype spine scene generated at {ScenePath}");
        }

        public static void ValidateGeneratedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();

            Require(scene.IsValid(), "Generated scene is not valid.", errors);
            Require(File.Exists(ScenePath), $"Generated scene is missing: {ScenePath}", errors);

            var controllerObject = GameObject.Find("Builder Prototype Spine Controller");
            Require(controllerObject != null, "Builder Prototype Spine Controller is missing.", errors);

            var controller = controllerObject != null ? controllerObject.GetComponent<BuilderPrototypeSpineController>() : null;
            Require(controller != null, "BuilderPrototypeSpineController component is missing.", errors);
            if (controller != null)
            {
                Require(controller.player != null, "Controller player reference is missing.", errors);
                Require(controller.followCamera != null, "Controller camera reference is missing.", errors);
                Require(controller.HasRequiredHookAnchors, "One or more subsystem hook anchors are missing.", errors);
                Require(controller.HasRequiredBuildReferences, "Build edit references are missing.", errors);
                Require(controller.HasRequiredCombatPreviewReferences, "Combat preview material references are missing.", errors);
                Require(controller.CurrentMode == BuilderPrototypeMode.Traverse, "Controller should start in Traverse mode.", errors);
            }

            Require(GameObject.Find("Builder Prototype Block Room") != null, "Block room root is missing.", errors);
            Require(GameObject.Find("Build Hook Anchor") != null, "Build hook anchor is missing.", errors);
            Require(GameObject.Find("Combat Hook Anchor") != null, "Combat hook anchor is missing.", errors);
            Require(GameObject.Find("Loot Hook Anchor") != null, "Loot hook anchor is missing.", errors);
            Require(GameObject.Find("Extract Hook Anchor") != null, "Extract hook anchor is missing.", errors);
            Require(GameObject.Find("Editable Build Blocks") != null, "Editable build block root is missing.", errors);
            Require(Camera.main != null || controller?.followCamera != null, "No usable camera was generated.", errors);

            if (errors.Count > 0)
            {
                throw new System.InvalidOperationException("Builder prototype spine validation failed:\n- " + string.Join("\n- ", errors));
            }

            Debug.Log("Builder prototype spine validation passed.");
        }

        private static void Require(bool condition, string message, List<string> errors)
        {
            if (!condition)
            {
                errors.Add(message);
            }
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Generated");
            Directory.CreateDirectory("Assets/Generated/BuilderPrototypeSpine");
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Builder Prototype Spine Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 8.4f, -7.2f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 0f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.027f, 0.029f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            return camera;
        }

        private static void CreateLights()
        {
            var keyObject = new GameObject("Builder Prototype Key Light");
            keyObject.transform.rotation = Quaternion.Euler(52f, -36f, 0f);
            var key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.15f;
            key.color = new Color(1f, 0.82f, 0.62f);
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.68f;

            var fillObject = new GameObject("Builder Prototype Hook Fill");
            fillObject.transform.position = new Vector3(-2.8f, 3.1f, -2.2f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.intensity = 1.1f;
            fill.range = 9f;
            fill.color = new Color(0.42f, 0.64f, 0.92f);
        }

        private static GameObject CreateBlockRoom(SpineMaterials materials)
        {
            var root = new GameObject("Block Room");
            for (var x = -6; x <= 6; x++)
            {
                for (var z = -4; z <= 4; z++)
                {
                    var tile = CreateBlock(root.transform, $"Floor Block {x},{z}", new Vector3(x, -0.12f, z), new Vector3(0.96f, 0.2f, 0.96f), ((x + z) & 1) == 0 ? materials.floorA : materials.floorB);
                    tile.transform.position += new Vector3(0f, Mathf.Abs((x * 13 + z * 7) % 3) * 0.008f, 0f);
                }
            }

            for (var x = -6; x <= 6; x++)
            {
                CreateBlock(root.transform, $"North Wall {x}", new Vector3(x, 0.48f, 4.85f), new Vector3(0.96f, 1.15f, 0.42f), materials.wall);
                CreateBlock(root.transform, $"South Wall {x}", new Vector3(x, 0.48f, -4.85f), new Vector3(0.96f, 1.15f, 0.42f), materials.wall);
            }

            for (var z = -4; z <= 4; z++)
            {
                CreateBlock(root.transform, $"West Wall {z}", new Vector3(-6.85f, 0.48f, z), new Vector3(0.42f, 1.15f, 0.96f), materials.wall);
                CreateBlock(root.transform, $"East Wall {z}", new Vector3(6.85f, 0.48f, z), new Vector3(0.42f, 1.15f, 0.96f), materials.wall);
            }

            return root;
        }

        private static GameObject CreatePlayer(SpineMaterials materials)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Builder Prototype Player";
            player.transform.position = new Vector3(0f, 0.68f, 0f);
            player.transform.localScale = new Vector3(0.72f, 0.82f, 0.72f);
            player.GetComponent<Renderer>().sharedMaterial = materials.player;
            return player;
        }

        private static GameObject CreateHookAnchor(string name, Vector3 position, Material material)
        {
            var anchor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            anchor.name = name;
            anchor.transform.position = position;
            anchor.transform.localScale = new Vector3(0.86f, 0.08f, 0.86f);
            anchor.GetComponent<Renderer>().sharedMaterial = material;
            return anchor;
        }

        private static GameObject CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent);
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        private static SpineMaterials CreateMaterials()
        {
            return new SpineMaterials(
                UpsertMaterial("MAT_BuilderSpine_FloorA", new Color(0.30f, 0.34f, 0.35f)),
                UpsertMaterial("MAT_BuilderSpine_FloorB", new Color(0.23f, 0.27f, 0.29f)),
                UpsertMaterial("MAT_BuilderSpine_Wall", new Color(0.18f, 0.20f, 0.21f)),
                UpsertMaterial("MAT_BuilderSpine_Player", new Color(0.92f, 0.63f, 0.27f)),
                UpsertMaterial("MAT_BuilderSpine_BuildHook", new Color(0.35f, 0.78f, 0.62f)),
                UpsertMaterial("MAT_BuilderSpine_CombatHook", new Color(0.82f, 0.25f, 0.22f)),
                UpsertMaterial("MAT_BuilderSpine_LootHook", new Color(0.78f, 0.68f, 0.25f)),
                UpsertMaterial("MAT_BuilderSpine_ExtractHook", new Color(0.35f, 0.48f, 0.86f)),
                UpsertMaterial("MAT_BuilderSpine_PlacedBlock", new Color(0.51f, 0.43f, 0.34f)),
                UpsertMaterial("MAT_BuilderSpine_BuildCursor", new Color(0.78f, 1f, 0.84f)),
                UpsertMaterial("MAT_BuilderSpine_CombatTelegraph", new Color(1f, 0.25f, 0.16f, 0.62f)),
                UpsertMaterial("MAT_BuilderSpine_CombatSafeMarker", new Color(0.31f, 0.92f, 0.53f, 0.82f)),
                UpsertMaterial("MAT_BuilderSpine_CombatThreatenedMarker", new Color(1f, 0.78f, 0.24f, 0.86f)),
                UpsertMaterial("MAT_BuilderSpine_CombatUnsafeMarker", new Color(1f, 0.18f, 0.15f, 0.92f)));
        }

        private static Material UpsertMaterial(string name, Color color)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(FindLitShader());
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader FindLitShader()
        {
            return Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        }

        private readonly struct SpineMaterials
        {
            public SpineMaterials(
                Material floorA,
                Material floorB,
                Material wall,
                Material player,
                Material buildHook,
                Material combatHook,
                Material lootHook,
                Material extractHook,
                Material placedBlock,
                Material buildCursor,
                Material combatTelegraph,
                Material combatSafeMarker,
                Material combatThreatenedMarker,
                Material combatUnsafeMarker)
            {
                this.floorA = floorA;
                this.floorB = floorB;
                this.wall = wall;
                this.player = player;
                this.buildHook = buildHook;
                this.combatHook = combatHook;
                this.lootHook = lootHook;
                this.extractHook = extractHook;
                this.placedBlock = placedBlock;
                this.buildCursor = buildCursor;
                this.combatTelegraph = combatTelegraph;
                this.combatSafeMarker = combatSafeMarker;
                this.combatThreatenedMarker = combatThreatenedMarker;
                this.combatUnsafeMarker = combatUnsafeMarker;
            }

            public readonly Material floorA;
            public readonly Material floorB;
            public readonly Material wall;
            public readonly Material player;
            public readonly Material buildHook;
            public readonly Material combatHook;
            public readonly Material lootHook;
            public readonly Material extractHook;
            public readonly Material placedBlock;
            public readonly Material buildCursor;
            public readonly Material combatTelegraph;
            public readonly Material combatSafeMarker;
            public readonly Material combatThreatenedMarker;
            public readonly Material combatUnsafeMarker;
        }
    }
}
