using System.IO;
using FourfoldEchoes.Spike;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldUnitySpikeBuilder
    {
        private const string ScenePath = "Assets/Scenes/AshenThresholdSpike.unity";
        private const string MaterialFolder = "Assets/Generated/Materials";

        public static void Build()
        {
            EnsureFolders();
            var materials = CreateMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "AshenThresholdSpike";

            RenderSettings.ambientLight = new Color(0.16f, 0.15f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.04f, 0.05f, 0.055f);
            RenderSettings.fogDensity = 0.015f;

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
            cameraObject.transform.position = new Vector3(1.1f, 8.5f, -7.6f);
            cameraObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.8f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.04f, 0.045f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            return camera;
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Low Mythic Key Light");
            lightObject.transform.rotation = Quaternion.Euler(45f, -32f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.65f;
            light.color = new Color(1f, 0.82f, 0.58f);

            var fillObject = new GameObject("Phase Cool Fill Light");
            fillObject.transform.position = new Vector3(-2.5f, 4.4f, -2.8f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.intensity = 2.1f;
            fill.range = 7f;
            fill.color = new Color(0.28f, 0.72f, 1f);
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
                    tile.transform.localScale = new Vector3(0.98f, 0.24f, 0.98f);
                    tile.GetComponent<Renderer>().sharedMaterial = (x + z) % 2 == 0 ? materials.stoneA : materials.stoneB;
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
            return root;
        }

        private static void CreateWall(Transform parent, SpikeMaterials materials, Vector3 position)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Low Wall Block";
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = new Vector3(0.95f, 0.75f, 0.95f);
            wall.GetComponent<Renderer>().sharedMaterial = materials.wall;
        }

        private static GameObject CreatePlayer(SpikeMaterials materials)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Echo Bearer Player";
            player.transform.position = new Vector3(-3.6f, 0.62f, 0f);
            player.transform.localScale = new Vector3(0.55f, 0.72f, 0.55f);
            player.GetComponent<Renderer>().sharedMaterial = materials.ember;
            return player;
        }

        private static GameObject CreateEnemy(SpikeMaterials materials)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Hollow Grunt";
            enemy.transform.position = new Vector3(-1.45f, 0.62f, 0f);
            enemy.transform.localScale = new Vector3(0.62f, 0.78f, 0.62f);
            enemy.GetComponent<Renderer>().sharedMaterial = materials.enemy;
            return enemy;
        }

        private static GameObject CreateAltar(SpikeMaterials materials, out GameObject glow)
        {
            var altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name = "Ember Altar";
            altar.transform.position = new Vector3(1.3f, 0.24f, 0f);
            altar.transform.localScale = new Vector3(0.72f, 0.22f, 0.72f);
            altar.GetComponent<Renderer>().sharedMaterial = materials.altar;

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
            left.transform.localScale = new Vector3(0.26f, 1.65f, 0.22f);
            left.GetComponent<Renderer>().sharedMaterial = materials.gateClosed;

            right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "Gate Right Pier";
            right.transform.SetParent(root.transform);
            right.transform.localPosition = new Vector3(3.7f, 0.9f, 0.48f);
            right.transform.localScale = new Vector3(0.26f, 1.65f, 0.22f);
            right.GetComponent<Renderer>().sharedMaterial = materials.gateClosed;

            badge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            badge.name = "Claim Ready E Badge";
            badge.transform.SetParent(root.transform);
            badge.transform.localPosition = new Vector3(3.22f, 0.75f, 0f);
            badge.transform.localScale = new Vector3(0.32f, 0.32f, 0.08f);
            badge.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            badge.GetComponent<Renderer>().sharedMaterial = materials.gateReady;
            badge.SetActive(false);
            return root;
        }

        private static SpikeMaterials CreateMaterials()
        {
            return new SpikeMaterials
            {
                stoneA = MaterialAsset("StoneA", new Color(0.16f, 0.15f, 0.14f)),
                stoneB = MaterialAsset("StoneB", new Color(0.20f, 0.19f, 0.18f)),
                wall = MaterialAsset("AncientWall", new Color(0.10f, 0.105f, 0.105f)),
                ember = MaterialAsset("PhaseEmber", new Color(1.0f, 0.46f, 0.16f)),
                tide = MaterialAsset("PhaseTide", new Color(0.22f, 0.75f, 0.88f)),
                bloom = MaterialAsset("PhaseBloom", new Color(0.43f, 0.86f, 0.38f)),
                prism = MaterialAsset("PhasePrism", new Color(0.72f, 0.58f, 1.0f)),
                player = MaterialAsset("PlayerDefault", new Color(0.94f, 0.96f, 0.9f)),
                enemy = MaterialAsset("HollowEnemy", new Color(0.9f, 0.22f, 0.29f)),
                enemyDead = MaterialAsset("HollowDead", new Color(0.22f, 0.24f, 0.25f)),
                altar = MaterialAsset("AltarCore", new Color(1.0f, 0.56f, 0.18f)),
                altarGlow = MaterialAsset("AltarGlow", new Color(1.0f, 0.75f, 0.23f), 0.45f),
                gateClosed = MaterialAsset("GateClosed", new Color(0.36f, 0.38f, 0.40f)),
                gateOpen = MaterialAsset("GateOpen", new Color(0.38f, 0.9f, 0.62f)),
                gateReady = MaterialAsset("GateReady", new Color(1.0f, 0.82f, 0.28f))
            };
        }

        private static Material MaterialAsset(string name, Color color, float alpha = 1f)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            color.a = alpha;
            material.color = color;
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            if (alpha < 1f)
            {
                material.SetFloat("_Surface", 1f);
                material.renderQueue = 3000;
            }
            return material;
        }

        private sealed class SpikeMaterials
        {
            public Material stoneA;
            public Material stoneB;
            public Material wall;
            public Material ember;
            public Material tide;
            public Material bloom;
            public Material prism;
            public Material player;
            public Material enemy;
            public Material enemyDead;
            public Material altar;
            public Material altarGlow;
            public Material gateClosed;
            public Material gateOpen;
            public Material gateReady;
        }
    }
}
