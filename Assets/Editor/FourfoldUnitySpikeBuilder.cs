using System.IO;
using System.Collections.Generic;
using FourfoldEchoes.Spike;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldUnitySpikeBuilder
    {
        private const string ScenePath = "Assets/Scenes/AshenThresholdSpike.unity";
        private const string MaterialFolder = "Assets/Generated/Materials";

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
            scene.name = "AshenThresholdSpike";

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.105f, 0.12f, 0.145f);
            RenderSettings.ambientEquatorColor = new Color(0.06f, 0.055f, 0.052f);
            RenderSettings.ambientGroundColor = new Color(0.014f, 0.012f, 0.015f);
            RenderSettings.ambientIntensity = 0.62f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.018f, 0.019f, 0.024f);
            RenderSettings.fogDensity = 0.026f;

            CreateLight();
            var camera = CreateCamera();
            var floorRoot = CreateRoom(materials);
            var player = CreatePlayer(materials);
            var enemy = CreateEnemy(materials);
            var altar = CreateAltar(materials, out var altarGlow);
            var gateRoot = CreateGate(materials, out var gateLeft, out var gateRight, out var gateBadge);

            var controllerRoot = new GameObject("Fourfold Spike Controller");
            var controller = controllerRoot.AddComponent<FourfoldUnitySpikeController>();
            controller.player = player.transform;
            controller.enemy = enemy.transform;
            controller.altarCore = altar.transform;
            controller.altarGlow = altarGlow.transform;
            controller.gateLeft = gateLeft.transform;
            controller.gateRight = gateRight.transform;
            controller.gateClaimBadge = gateBadge.transform;
            controller.fixedCamera = camera;
            controller.emberMaterial = materials.ember;
            controller.tideMaterial = materials.tide;
            controller.bloomMaterial = materials.bloom;
            controller.prismMaterial = materials.prism;
            controller.playerMaterial = materials.player;
            controller.enemyMaterial = materials.enemy;
            controller.enemyDeadMaterial = materials.enemyDead;
            controller.altarMaterial = materials.altar;
            controller.gateClosedMaterial = materials.gateClosed;
            controller.gateOpenMaterial = materials.gateOpen;
            controller.gateReadyMaterial = materials.gateReady;

            floorRoot.name = "Block Diorama Terrain";
            gateRoot.name = "Claim Gate";

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log($"FOURFOLD Unity spike generated at {ScenePath}");
        }

        public static void ValidateGeneratedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();

            Require(scene.IsValid(), "Generated scene is not valid.", errors);
            Require(File.Exists(ScenePath), $"Generated scene is missing: {ScenePath}", errors);

            var controllerObject = GameObject.Find("Fourfold Spike Controller");
            Require(controllerObject != null, "Fourfold Spike Controller is missing.", errors);

            var controller = controllerObject != null ? controllerObject.GetComponent<FourfoldUnitySpikeController>() : null;
            Require(controller != null, "FourfoldUnitySpikeController component is missing.", errors);

            if (controller != null)
            {
                Require(controller.player != null, "Controller player reference is missing.", errors);
                Require(controller.enemy != null, "Controller enemy reference is missing.", errors);
                Require(controller.altarCore != null, "Controller altarCore reference is missing.", errors);
                Require(controller.altarGlow != null, "Controller altarGlow reference is missing.", errors);
                Require(controller.gateLeft != null, "Controller gateLeft reference is missing.", errors);
                Require(controller.gateRight != null, "Controller gateRight reference is missing.", errors);
                Require(controller.gateClaimBadge != null, "Controller gateClaimBadge reference is missing.", errors);
                Require(controller.fixedCamera != null, "Controller fixedCamera reference is missing.", errors);
                Require(controller.emberMaterial != null, "Ember material is missing.", errors);
                Require(controller.gateReadyMaterial != null, "Gate ready material is missing.", errors);
            }

            Require(GameObject.Find("Block Diorama Terrain") != null, "Block Diorama Terrain root is missing.", errors);
            Require(GameObject.Find("Claim Gate") != null, "Claim Gate root is missing.", errors);
            Require(Camera.main != null || controller?.fixedCamera != null, "No usable camera was generated.", errors);
            Require(EditorBuildSettings.scenes.Length > 0 && EditorBuildSettings.scenes[0].path == ScenePath, "Build Settings do not include the generated scene first.", errors);

            if (errors.Count > 0)
            {
                throw new System.InvalidOperationException("FOURFOLD Unity spike validation failed:\n- " + string.Join("\n- ", errors));
            }

            Debug.Log("FOURFOLD Unity spike validation passed.");
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
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Fixed Angle Camera");
            cameraObject.transform.position = new Vector3(1.65f, 7.4f, -7.1f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.75f, 0.18f, 0f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.95f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.012f, 0.014f, 0.018f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            return camera;
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Low Mythic Key Light");
            lightObject.transform.rotation = Quaternion.Euler(48f, -38f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.18f;
            light.color = new Color(1f, 0.73f, 0.45f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;

            var fillObject = new GameObject("Phase Cool Fill Light");
            fillObject.transform.position = new Vector3(-3.8f, 3.2f, -2.7f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.intensity = 0.95f;
            fill.range = 6.4f;
            fill.color = new Color(0.22f, 0.52f, 0.92f);

            var rimObject = new GameObject("Gate Ember Rim Light");
            rimObject.transform.position = new Vector3(3.45f, 2.2f, 0f);
            var rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.intensity = 3.2f;
            rim.range = 4.8f;
            rim.color = new Color(1f, 0.42f, 0.18f);

            var altarObject = new GameObject("Altar Objective Light");
            altarObject.transform.position = new Vector3(1.3f, 1.05f, 0f);
            var altar = altarObject.AddComponent<Light>();
            altar.type = LightType.Point;
            altar.intensity = 2.15f;
            altar.range = 3.2f;
            altar.color = new Color(1f, 0.5f, 0.16f);

            var enemyObject = new GameObject("Enemy Threat Light");
            enemyObject.transform.position = new Vector3(-1.45f, 1.15f, 0.35f);
            var enemy = enemyObject.AddComponent<Light>();
            enemy.type = LightType.Point;
            enemy.intensity = 1.55f;
            enemy.range = 3.0f;
            enemy.color = new Color(0.9f, 0.08f, 0.08f);

            var rewardObject = new GameObject("Gate Reward Light");
            rewardObject.transform.position = new Vector3(4.08f, 1.3f, 0f);
            var reward = rewardObject.AddComponent<Light>();
            reward.type = LightType.Point;
            reward.intensity = 2.45f;
            reward.range = 3.8f;
            reward.color = new Color(1f, 0.76f, 0.24f);
        }

        private static GameObject CreateRoom(SpikeMaterials materials)
        {
            var root = new GameObject("Terrain");
            for (var x = -5; x <= 5; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"Stone Block {x},{z}";
                    tile.transform.SetParent(root.transform);
                    tile.transform.position = new Vector3(x, -0.12f, z);
                    var chip = Mathf.Abs((x * 17 + z * 9) % 5) * 0.012f;
                    tile.transform.localScale = new Vector3(0.96f, 0.2f + chip, 0.96f);
                    var onPath = Mathf.Abs(z) <= 1 && x >= -4;
                    var onDuelLine = z == 0 && x <= 0;
                    tile.GetComponent<Renderer>().sharedMaterial = onDuelLine ? materials.combatLane : onPath ? materials.stonePath : ((x + z) % 2 == 0 ? materials.stoneA : materials.stoneB);
                }
            }

            for (var x = -5; x <= 5; x++)
            {
                CreateWall(root.transform, materials, new Vector3(x, 0.36f, -3.75f));
                CreateWall(root.transform, materials, new Vector3(x, 0.36f, 3.75f));
            }
            for (var z = -3; z <= 3; z++)
            {
                CreateWall(root.transform, materials, new Vector3(-5.75f, 0.36f, z));
                if (z != 0)
                {
                    CreateWall(root.transform, materials, new Vector3(5.75f, 0.36f, z));
                }
            }

            CreateBlock(root.transform, "Back Wall Shadow", materials.backdrop, new Vector3(0f, 1.02f, 4.32f), new Vector3(12.8f, 1.9f, 0.22f));
            CreateBlock(root.transform, "Left Wall Shadow", materials.backdrop, new Vector3(-6.28f, 0.78f, 0f), new Vector3(0.24f, 1.45f, 7.8f));
            CreateBlock(root.transform, "Right Wall Occlusion", materials.backdrop, new Vector3(6.18f, 0.82f, 2.65f), new Vector3(0.28f, 1.5f, 2.2f));
            CreateBlock(root.transform, "Player Start Dais", materials.threshold, new Vector3(-3.55f, 0.02f, 0f), new Vector3(1.42f, 0.075f, 1.42f));
            CreateBlock(root.transform, "Duel Lane Scar", materials.combatLane, new Vector3(-2.4f, 0.04f, 0f), new Vector3(2.55f, 0.04f, 0.24f));
            CreateBlock(root.transform, "Gate Threshold Slab", materials.threshold, new Vector3(3.55f, 0.02f, 0f), new Vector3(2.05f, 0.1f, 1.98f));
            CreateBlock(root.transform, "Gate Approach Ember Inlay", materials.emberDim, new Vector3(2.55f, 0.055f, 0f), new Vector3(2.2f, 0.04f, 0.24f));
            CreateBlock(root.transform, "Altar Ember Path", materials.emberDim, new Vector3(1.34f, 0.06f, 0f), new Vector3(1.72f, 0.05f, 0.38f));
            CreateBlock(root.transform, "Enemy Threat Slash North", materials.threatDim, new Vector3(-1.45f, 0.055f, 0.43f), new Vector3(1.55f, 0.04f, 0.09f), Quaternion.Euler(0f, 22f, 0f));
            CreateBlock(root.transform, "Enemy Threat Slash South", materials.threatDim, new Vector3(-1.45f, 0.055f, -0.43f), new Vector3(1.55f, 0.04f, 0.09f), Quaternion.Euler(0f, -22f, 0f));
            CreateCylinder(root.transform, "Enemy Threat Staging Ring", materials.threat, new Vector3(-1.45f, 0.03f, 0f), new Vector3(1.12f, 0.025f, 1.12f));
            CreateCylinder(root.transform, "Altar Objective Ring", materials.altarObjective, new Vector3(1.3f, 0.045f, 0f), new Vector3(1.55f, 0.03f, 1.55f));
            CreateCylinder(root.transform, "Gate Claim Circle", materials.gateReadyDim, new Vector3(3.55f, 0.045f, 0f), new Vector3(1.35f, 0.03f, 1.35f));
            CreateEncounterDressing(root.transform, materials);
            CreatePhaseTether(root.transform, materials.emberDim, new Vector3(-3.25f, 0.06f, -2.18f), new Vector3(-2.75f, 0.06f, -0.72f));
            CreatePhaseTether(root.transform, materials.tideDim, new Vector3(-1.25f, 0.06f, 2.18f), new Vector3(-1.05f, 0.06f, 0.72f));
            CreatePhaseTether(root.transform, materials.bloomDim, new Vector3(1.25f, 0.06f, 2.18f), new Vector3(1.05f, 0.06f, 0.72f));
            CreatePhaseTether(root.transform, materials.prismDim, new Vector3(3.08f, 0.06f, -2.18f), new Vector3(2.65f, 0.06f, -0.72f));
            CreatePhaseNode(root.transform, "Ember Phase Node", materials.ember, materials.emberDim, new Vector3(-3.25f, 0.05f, -2.18f));
            CreatePhaseNode(root.transform, "Tide Phase Node", materials.tide, materials.tideDim, new Vector3(-1.25f, 0.05f, 2.18f));
            CreatePhaseNode(root.transform, "Bloom Phase Node", materials.bloom, materials.bloomDim, new Vector3(1.25f, 0.05f, 2.18f));
            CreatePhaseNode(root.transform, "Prism Phase Node", materials.prism, materials.prismDim, new Vector3(3.08f, 0.05f, -2.18f));
            return root;
        }

        private static void CreateEncounterDressing(Transform parent, SpikeMaterials materials)
        {
            CreateBrokenPillar(parent, materials, "Left Combat Pillar", new Vector3(-2.85f, 0.12f, -2.95f), 1.05f, Quaternion.Euler(0f, 0f, -9f));
            CreateBrokenPillar(parent, materials, "Right Combat Pillar", new Vector3(0.85f, 0.1f, -2.95f), 0.82f, Quaternion.Euler(0f, 0f, 7f));
            CreateBrokenPillar(parent, materials, "Gate Watch Pillar", new Vector3(4.52f, 0.1f, -2.18f), 1.25f, Quaternion.Euler(0f, 0f, -5f));
            CreateBrokenPillar(parent, materials, "Altar Watch Pillar", new Vector3(0.35f, 0.1f, 2.92f), 0.95f, Quaternion.Euler(0f, 0f, 5f));
            CreateBlock(parent, "Back Wall Gate Shadow Arch", materials.gateVoid, new Vector3(3.72f, 1.15f, 3.98f), new Vector3(2.3f, 2.05f, 0.18f));
            CreateBlock(parent, "Back Wall Altar Shadow Arch", materials.backdrop, new Vector3(1.3f, 1.0f, 3.94f), new Vector3(1.65f, 1.72f, 0.16f));
            CreateBlock(parent, "Broken Stone Rib North", materials.wallEdge, new Vector3(-0.55f, 0.09f, -1.22f), new Vector3(0.18f, 0.14f, 1.42f), Quaternion.Euler(0f, 34f, 0f));
            CreateBlock(parent, "Broken Stone Rib South", materials.wallEdge, new Vector3(0.25f, 0.09f, 1.28f), new Vector3(0.18f, 0.14f, 1.18f), Quaternion.Euler(0f, -30f, 0f));
            CreateBlock(parent, "Reward Sightline Inlay", materials.gateReadyDim, new Vector3(3.72f, 0.072f, 0f), new Vector3(0.22f, 0.045f, 1.58f));
            CreateBlock(parent, "Start Sightline Inlay", materials.tideDim, new Vector3(-3.55f, 0.068f, 0f), new Vector3(0.18f, 0.04f, 1.22f));
        }

        private static void CreateBrokenPillar(Transform parent, SpikeMaterials materials, string name, Vector3 position, float height, Quaternion rotation)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent);
            root.transform.localPosition = position;
            root.transform.localRotation = rotation;
            CreateCylinder(root.transform, "Pillar Base", materials.wallEdge, Vector3.zero, new Vector3(0.36f, 0.13f, 0.36f));
            CreateCylinder(root.transform, "Pillar Shaft", materials.wall, new Vector3(0f, height * 0.45f, 0f), new Vector3(0.22f, height, 0.22f));
            CreateBlock(root.transform, "Pillar Broken Cap", materials.wallEdge, new Vector3(0.05f, height + 0.08f, 0.02f), new Vector3(0.42f, 0.18f, 0.36f), Quaternion.Euler(0f, 28f, 7f));
            CreateBlock(root.transform, "Pillar Ground Shadow", materials.shadow, new Vector3(0.08f, -0.04f, 0.1f), new Vector3(0.9f, 0.02f, 0.72f), Quaternion.Euler(0f, 24f, 0f));
        }

        private static void CreatePhaseTether(Transform parent, Material material, Vector3 start, Vector3 end)
        {
            var delta = end - start;
            var center = start + delta * 0.5f;
            var length = new Vector2(delta.x, delta.z).magnitude;
            var angle = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            CreateBlock(parent, "Phase Floor Tether", material, center, new Vector3(0.08f, 0.035f, length), Quaternion.Euler(0f, angle, 0f));
        }

        private static void CreateWall(Transform parent, SpikeMaterials materials, Vector3 position)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Low Wall Block";
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = new Vector3(0.95f, 0.88f, 0.95f);
            wall.GetComponent<Renderer>().sharedMaterial = materials.wall;

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "Low Wall Capstone";
            cap.transform.SetParent(parent);
            cap.transform.position = position + new Vector3(0f, 0.48f, 0f);
            cap.transform.localScale = new Vector3(0.82f, 0.13f, 0.82f);
            cap.GetComponent<Renderer>().sharedMaterial = materials.wallEdge;
        }

        private static GameObject CreatePlayer(SpikeMaterials materials)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Echo Bearer Player";
            player.transform.position = new Vector3(-3.6f, 0.62f, 0f);
            player.transform.localScale = new Vector3(0.55f, 0.72f, 0.55f);
            player.GetComponent<Renderer>().sharedMaterial = materials.player;
            AddBaseShadow(player.transform, materials, "Player Ground Shadow", 1.32f);
            CreateBlock(player.transform, "Echo Bearer Dark Mantle", materials.playerMantle, new Vector3(-0.08f, 0.2f, -0.18f), new Vector3(0.72f, 0.52f, 0.2f));
            CreateBlock(player.transform, "Echo Bearer Shoulder Line", materials.playerSteel, new Vector3(0.05f, 0.42f, 0f), new Vector3(0.72f, 0.12f, 0.2f));
            CreateBlock(player.transform, "Echo Bearer Ember Core", materials.ember, new Vector3(0.22f, 0.12f, 0f), new Vector3(0.08f, 0.34f, 0.18f));
            CreateBlock(player.transform, "Echo Bearer Face Glow", materials.gateReady, new Vector3(0.32f, 0.5f, 0f), new Vector3(0.08f, 0.16f, 0.22f));
            CreateBlock(player.transform, "Echo Blade Hilt", materials.playerSteel, new Vector3(0.36f, 0.1f, 0f), new Vector3(0.08f, 0.26f, 0.12f), Quaternion.Euler(0f, 0f, -18f));
            CreateBlock(player.transform, "Echo Blade Silhouette", materials.gateReady, new Vector3(0.62f, 0.18f, 0.04f), new Vector3(0.08f, 1.18f, 0.14f), Quaternion.Euler(0f, 0f, -32f));
            CreateBlock(player.transform, "Echo Blade Tip", materials.gateReady, new Vector3(0.92f, 0.62f, 0.04f), new Vector3(0.05f, 0.28f, 0.11f), Quaternion.Euler(0f, 0f, -32f));
            CreateBlock(player.transform, "Ready Attack Arc Near", materials.emberDim, new Vector3(0.48f, -0.5f, 0.48f), new Vector3(0.84f, 0.04f, 0.11f), Quaternion.Euler(0f, 28f, 0f));
            CreateBlock(player.transform, "Ready Attack Arc Far", materials.gateReadyDim, new Vector3(0.88f, -0.48f, 0f), new Vector3(0.74f, 0.04f, 0.1f), Quaternion.Euler(0f, 0f, 0f));
            CreateBlock(player.transform, "Ready Attack Arc Trail", materials.emberDim, new Vector3(0.48f, -0.5f, -0.48f), new Vector3(0.84f, 0.04f, 0.11f), Quaternion.Euler(0f, -28f, 0f));
            return player;
        }

        private static GameObject CreateEnemy(SpikeMaterials materials)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Hollow Grunt";
            enemy.transform.position = new Vector3(-1.45f, 0.62f, 0f);
            enemy.transform.localScale = new Vector3(0.62f, 0.78f, 0.62f);
            enemy.GetComponent<Renderer>().sharedMaterial = materials.enemy;
            AddBaseShadow(enemy.transform, materials, "Hollow Ground Shadow", 1.58f);
            CreateBlock(enemy.transform, "Hollow Back Spire", materials.enemyDark, new Vector3(-0.1f, 0.5f, -0.08f), new Vector3(0.2f, 1.15f, 0.16f), Quaternion.Euler(0f, 0f, 20f));
            CreateBlock(enemy.transform, "Hollow Shoulder Mass", materials.enemyDark, new Vector3(0.03f, 0.34f, 0f), new Vector3(0.7f, 0.22f, 0.22f));
            CreateBlock(enemy.transform, "Hollow Right Horn", materials.enemyEye, new Vector3(0.28f, 0.48f, 0.14f), new Vector3(0.08f, 0.38f, 0.08f), Quaternion.Euler(0f, 0f, -24f));
            CreateBlock(enemy.transform, "Hollow Left Horn", materials.enemyEye, new Vector3(0.28f, 0.48f, -0.14f), new Vector3(0.08f, 0.38f, 0.08f), Quaternion.Euler(0f, 0f, -24f));
            CreateBlock(enemy.transform, "Hollow Cleaver Shaft", materials.enemyDark, new Vector3(-0.5f, 0.14f, 0.12f), new Vector3(0.09f, 0.86f, 0.1f), Quaternion.Euler(0f, 0f, 36f));
            CreateBlock(enemy.transform, "Hollow Cleaver Head", materials.enemyEye, new Vector3(-0.78f, 0.5f, 0.12f), new Vector3(0.26f, 0.36f, 0.12f), Quaternion.Euler(0f, 0f, 36f));
            CreateBlock(enemy.transform, "Hollow Forward Threat Marker", materials.threat, new Vector3(-0.72f, -0.54f, 0f), new Vector3(0.9f, 0.035f, 0.2f), Quaternion.Euler(0f, 0f, 0f));
            return enemy;
        }

        private static GameObject CreateAltar(SpikeMaterials materials, out GameObject glow)
        {
            var altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name = "Ember Altar";
            altar.transform.position = new Vector3(1.3f, 0.24f, 0f);
            altar.transform.localScale = new Vector3(0.72f, 0.22f, 0.72f);
            altar.GetComponent<Renderer>().sharedMaterial = materials.altar;
            CreateCylinder(altar.transform, "Altar Lower Dais", materials.altarStone, new Vector3(0f, -0.08f, 0f), new Vector3(1.55f, 0.18f, 1.55f));
            CreateCylinder(altar.transform, "Altar Iron Crown", materials.iron, new Vector3(0f, 0.18f, 0f), new Vector3(1.26f, 0.18f, 1.26f));
            CreateCylinder(altar.transform, "Altar Ember Bowl", materials.ember, new Vector3(0f, 0.54f, 0f), new Vector3(0.56f, 0.2f, 0.56f));
            CreateBlock(altar.transform, "Altar North Rune", materials.gateReady, new Vector3(0f, 0.62f, 0.56f), new Vector3(0.12f, 0.09f, 0.42f));
            CreateBlock(altar.transform, "Altar South Rune", materials.gateReady, new Vector3(0f, 0.62f, -0.56f), new Vector3(0.12f, 0.09f, 0.42f));
            CreateBlock(altar.transform, "Altar East Rune", materials.gateReady, new Vector3(0.56f, 0.62f, 0f), new Vector3(0.42f, 0.09f, 0.12f));
            CreateBlock(altar.transform, "Altar West Rune", materials.gateReady, new Vector3(-0.56f, 0.62f, 0f), new Vector3(0.42f, 0.09f, 0.12f));
            CreateBlock(altar.transform, "Altar Flame Column Low", materials.altarGlow, new Vector3(0f, 0.86f, 0f), new Vector3(0.18f, 0.58f, 0.18f), Quaternion.Euler(0f, 45f, 0f));
            CreateBlock(altar.transform, "Altar Flame Column High", materials.gateReady, new Vector3(0f, 1.16f, 0f), new Vector3(0.11f, 0.46f, 0.11f), Quaternion.Euler(0f, 45f, 8f));
            CreateBlock(altar.transform, "Altar Objective Pointer", materials.gateReadyDim, new Vector3(-0.95f, 0.12f, 0f), new Vector3(0.58f, 0.08f, 0.18f));

            glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "Altar Heat Field";
            glow.transform.position = new Vector3(1.3f, 0.16f, 0f);
            glow.transform.localScale = Vector3.one * 1.05f;
            glow.GetComponent<Renderer>().sharedMaterial = materials.altarGlow;
            glow.SetActive(false);
            return altar;
        }

        private static GameObject CreateGate(SpikeMaterials materials, out GameObject left, out GameObject right, out GameObject badge)
        {
            var root = new GameObject("Gate Root");
            left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "Gate Left Pier";
            left.transform.SetParent(root.transform);
            left.transform.localPosition = new Vector3(3.7f, 0.9f, -0.48f);
            left.transform.localScale = new Vector3(0.38f, 2.08f, 0.34f);
            left.GetComponent<Renderer>().sharedMaterial = materials.gateClosed;

            right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "Gate Right Pier";
            right.transform.SetParent(root.transform);
            right.transform.localPosition = new Vector3(3.7f, 0.9f, 0.48f);
            right.transform.localScale = new Vector3(0.38f, 2.08f, 0.34f);
            right.GetComponent<Renderer>().sharedMaterial = materials.gateClosed;

            CreateBlock(root.transform, "Gate Outer Left Buttress", materials.iron, new Vector3(3.86f, 1.0f, -0.92f), new Vector3(0.48f, 2.28f, 0.28f));
            CreateBlock(root.transform, "Gate Outer Right Buttress", materials.iron, new Vector3(3.86f, 1.0f, 0.92f), new Vector3(0.48f, 2.28f, 0.28f));
            CreateBlock(root.transform, "Gate Black Lintel", materials.iron, new Vector3(3.7f, 2.0f, 0f), new Vector3(0.48f, 0.28f, 1.68f));
            CreateBlock(root.transform, "Gate Ember Crown", materials.gateReady, new Vector3(3.58f, 2.22f, 0f), new Vector3(0.24f, 0.18f, 0.78f));
            CreateBlock(root.transform, "Gate Left Rune Scar", materials.emberDim, new Vector3(3.48f, 1.05f, -0.5f), new Vector3(0.08f, 1.12f, 0.08f));
            CreateBlock(root.transform, "Gate Right Rune Scar", materials.emberDim, new Vector3(3.48f, 1.05f, 0.5f), new Vector3(0.08f, 1.12f, 0.08f));
            CreateBlock(root.transform, "Gate Void Backing", materials.gateVoid, new Vector3(4.02f, 1.12f, 0f), new Vector3(0.18f, 1.62f, 1.18f));
            CreateBlock(root.transform, "Gate Reward Plinth", materials.altarStone, new Vector3(4.18f, 0.34f, 0f), new Vector3(0.46f, 0.32f, 0.46f));
            CreateCylinder(root.transform, "Afterglow Reward Halo", materials.rewardHalo, new Vector3(3.32f, 1.1f, 0f), new Vector3(0.58f, 0.045f, 0.58f), Quaternion.Euler(90f, 0f, 0f));
            CreateSphere(root.transform, "Afterglow Reward Core", materials.rewardCore, new Vector3(3.26f, 1.1f, 0f), new Vector3(0.28f, 0.28f, 0.28f));
            CreateBlock(root.transform, "Afterglow Reward Beam", materials.rewardHalo, new Vector3(3.28f, 1.32f, 0f), new Vector3(0.08f, 0.86f, 0.12f));
            CreateBlock(root.transform, "Gate Left Prism Backlight", materials.prismDim, new Vector3(3.42f, 1.08f, -0.78f), new Vector3(0.07f, 1.34f, 0.08f));
            CreateBlock(root.transform, "Gate Right Prism Backlight", materials.prismDim, new Vector3(3.42f, 1.08f, 0.78f), new Vector3(0.07f, 1.34f, 0.08f));
            CreateBlock(root.transform, "Gate Upper Prism Backlight", materials.prismDim, new Vector3(3.42f, 1.82f, 0f), new Vector3(0.07f, 0.08f, 1.32f));

            badge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            badge.name = "Claim Ready E Badge";
            badge.transform.SetParent(root.transform);
            badge.transform.localPosition = new Vector3(3.22f, 0.75f, 0f);
            badge.transform.localScale = new Vector3(0.5f, 0.5f, 0.12f);
            badge.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            badge.GetComponent<Renderer>().sharedMaterial = materials.gateReady;
            badge.SetActive(false);
            return root;
        }

        private static GameObject CreateBlock(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            return CreateBlock(parent, name, material, localPosition, localScale, Quaternion.identity);
        }

        private static GameObject CreateBlock(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent);
            block.transform.localPosition = localPosition;
            block.transform.localRotation = localRotation;
            block.transform.localScale = localScale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            DisableCollider(block);
            return block;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            return CreateCylinder(parent, name, material, localPosition, localScale, Quaternion.identity);
        }

        private static GameObject CreateCylinder(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = localRotation;
            cylinder.transform.localScale = localScale;
            cylinder.GetComponent<Renderer>().sharedMaterial = material;
            DisableCollider(cylinder);
            return cylinder;
        }

        private static GameObject CreateSphere(Transform parent, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            sphere.transform.SetParent(parent);
            sphere.transform.localPosition = localPosition;
            sphere.transform.localScale = localScale;
            sphere.GetComponent<Renderer>().sharedMaterial = material;
            DisableCollider(sphere);
            return sphere;
        }

        private static void CreatePhaseNode(Transform parent, string name, Material material, Material dimMaterial, Vector3 position)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreateCylinder(root.transform, "Phase Node Base", dimMaterial, Vector3.zero, new Vector3(0.6f, 0.055f, 0.6f));
            CreateCylinder(root.transform, "Phase Node Dark Ring", MaterialAsset("PhaseNodeDarkRing", new Color(0.025f, 0.024f, 0.026f)), new Vector3(0f, 0.025f, 0f), new Vector3(0.62f, 0.035f, 0.62f));
            CreateCylinder(root.transform, "Phase Node Inner Glow", material, new Vector3(0f, 0.08f, 0f), new Vector3(0.32f, 0.04f, 0.32f));
            var shard = CreateBlock(root.transform, "Phase Node Obelisk", material, new Vector3(0f, 0.5f, 0f), new Vector3(0.18f, 0.92f, 0.18f), Quaternion.Euler(0f, 45f, 0f));
            shard.transform.localRotation *= Quaternion.Euler(0f, 0f, 8f);
            CreateBlock(root.transform, "Phase Node Shadow", MaterialAsset("PhaseNodeShadow", new Color(0f, 0f, 0f), 0.38f), new Vector3(0f, -0.01f, 0f), new Vector3(0.92f, 0.018f, 0.92f));
        }

        private static void AddBaseShadow(Transform parent, SpikeMaterials materials, string name, float width)
        {
            CreateCylinder(parent, name, materials.shadow, new Vector3(0f, -0.58f, 0f), new Vector3(width, 0.025f, width * 0.82f));
        }

        private static void DisableCollider(GameObject gameObject)
        {
            var collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private static SpikeMaterials CreateMaterials()
        {
            return new SpikeMaterials
            {
                stoneA = MaterialAsset("StoneA", new Color(0.095f, 0.09f, 0.088f)),
                stoneB = MaterialAsset("StoneB", new Color(0.13f, 0.12f, 0.112f)),
                stonePath = MaterialAsset("StonePath", new Color(0.18f, 0.145f, 0.115f)),
                combatLane = MaterialAsset("CombatLaneStone", new Color(0.24f, 0.18f, 0.13f)),
                wall = MaterialAsset("AncientWall", new Color(0.055f, 0.06f, 0.065f)),
                wallEdge = MaterialAsset("AncientWallEdge", new Color(0.16f, 0.15f, 0.135f)),
                backdrop = MaterialAsset("WallBackdrop", new Color(0.018f, 0.02f, 0.024f)),
                threshold = MaterialAsset("GateThreshold", new Color(0.12f, 0.105f, 0.095f)),
                altarStone = MaterialAsset("AltarObjectiveStone", new Color(0.22f, 0.14f, 0.095f)),
                gateVoid = MaterialAsset("GateVoid", new Color(0.006f, 0.008f, 0.012f)),
                iron = MaterialAsset("BlackenedIron", new Color(0.035f, 0.035f, 0.04f)),
                shadow = MaterialAsset("SoftContactShadow", new Color(0f, 0f, 0f), 0.42f),
                ember = MaterialAsset("PhaseEmber", new Color(1.0f, 0.38f, 0.1f), 1f, 1.6f),
                emberDim = MaterialAsset("PhaseEmberDim", new Color(0.52f, 0.12f, 0.055f), 1f, 0.75f),
                tide = MaterialAsset("PhaseTide", new Color(0.12f, 0.64f, 0.86f), 1f, 1.35f),
                tideDim = MaterialAsset("PhaseTideDim", new Color(0.045f, 0.26f, 0.36f), 1f, 0.42f),
                bloom = MaterialAsset("PhaseBloom", new Color(0.34f, 0.78f, 0.36f), 1f, 1.25f),
                bloomDim = MaterialAsset("PhaseBloomDim", new Color(0.11f, 0.28f, 0.12f), 1f, 0.36f),
                prism = MaterialAsset("PhasePrism", new Color(0.66f, 0.48f, 1.0f), 1f, 1.4f),
                prismDim = MaterialAsset("PhasePrismDim", new Color(0.33f, 0.22f, 0.58f), 0.62f, 0.65f),
                player = MaterialAsset("PlayerDefault", new Color(0.86f, 0.88f, 0.78f)),
                playerMantle = MaterialAsset("PlayerMantle", new Color(0.08f, 0.065f, 0.06f)),
                playerSteel = MaterialAsset("PlayerSteel", new Color(0.7f, 0.74f, 0.72f), 1f, 0.18f),
                enemy = MaterialAsset("HollowEnemy", new Color(0.82f, 0.08f, 0.14f), 1f, 0.9f),
                enemyDark = MaterialAsset("HollowEnemyDark", new Color(0.085f, 0.035f, 0.045f)),
                enemyEye = MaterialAsset("HollowEnemyEye", new Color(1.0f, 0.15f, 0.12f), 1f, 1.6f),
                enemyDead = MaterialAsset("HollowDead", new Color(0.18f, 0.18f, 0.2f)),
                threat = MaterialAsset("ThreatRed", new Color(0.72f, 0.04f, 0.04f), 1f, 0.7f),
                threatDim = MaterialAsset("ThreatRedDim", new Color(0.32f, 0.03f, 0.035f), 1f, 0.24f),
                altar = MaterialAsset("AltarCore", new Color(0.45f, 0.18f, 0.09f), 1f, 0.45f),
                altarObjective = MaterialAsset("AltarObjectiveRing", new Color(0.78f, 0.26f, 0.08f), 1f, 0.75f),
                altarGlow = MaterialAsset("AltarGlow", new Color(1.0f, 0.55f, 0.15f), 0.38f, 1.9f),
                gateClosed = MaterialAsset("GateClosed", new Color(0.17f, 0.17f, 0.18f)),
                gateOpen = MaterialAsset("GateOpen", new Color(0.28f, 0.82f, 0.52f), 1f, 1.25f),
                gateReady = MaterialAsset("GateReady", new Color(1.0f, 0.68f, 0.16f), 1f, 1.75f),
                gateReadyDim = MaterialAsset("GateReadyDim", new Color(0.62f, 0.36f, 0.08f), 1f, 0.55f),
                rewardHalo = MaterialAsset("AfterglowRewardHalo", new Color(1.0f, 0.72f, 0.2f), 1f, 1.35f),
                rewardCore = MaterialAsset("AfterglowRewardCore", new Color(1.0f, 0.92f, 0.44f), 1f, 2.4f)
            };
        }

        private static Material MaterialAsset(string name, Color color, float alpha = 1f, float emission = 0f)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            color = Color.Lerp(Color.black, color, alpha);
            color.a = 1f;
            material.color = color;
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
            material.SetFloat("_Surface", 0f);
            material.SetFloat("_Mode", 0f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHABLEND_ON");
            material.renderQueue = -1;
            return material;
        }

        private sealed class SpikeMaterials
        {
            public Material stoneA;
            public Material stoneB;
            public Material stonePath;
            public Material combatLane;
            public Material wall;
            public Material wallEdge;
            public Material backdrop;
            public Material threshold;
            public Material altarStone;
            public Material gateVoid;
            public Material iron;
            public Material shadow;
            public Material ember;
            public Material emberDim;
            public Material tide;
            public Material tideDim;
            public Material bloom;
            public Material bloomDim;
            public Material prism;
            public Material prismDim;
            public Material player;
            public Material playerMantle;
            public Material playerSteel;
            public Material enemy;
            public Material enemyDark;
            public Material enemyEye;
            public Material enemyDead;
            public Material threat;
            public Material threatDim;
            public Material altar;
            public Material altarObjective;
            public Material altarGlow;
            public Material gateClosed;
            public Material gateOpen;
            public Material gateReady;
            public Material gateReadyDim;
            public Material rewardHalo;
            public Material rewardCore;
        }
    }
}
