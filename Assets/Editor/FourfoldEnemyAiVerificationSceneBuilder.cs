using System;
using System.Collections.Generic;
using System.IO;
using FourfoldEchoes.Product;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldEnemyAiVerificationSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/AI_EnemyController_Verification.unity";
        private const string DefinitionFolder = "Assets/Generated/AI/Definitions";
        private const string MaterialFolder = "Assets/Generated/AI/Materials";
        private const string RootName = "AI EnemyController Verification";

        [MenuItem("FOURFOLD/AI/Build EnemyController Verification Scene")]
        public static void BuildAndValidate()
        {
            Build();
            ValidateGeneratedScene();
        }

        [MenuItem("FOURFOLD/AI/Run EnemyController State Validation")]
        public static void RunStateTransitionValidation()
        {
            RunValidationCase(ValidateCombatLoopAndDeath);
            RunValidationCase(ValidateLeashReturn);
            RunValidationCase(ValidateLineOfSightWall);
            RunValidationCase(ValidateObstacleBlocking);
            Debug.Log("FOURFOLD enemy AI deterministic state validation passed.");
        }

        public static void Build()
        {
            EnsureFolders();
            var materials = CreateMaterials();
            var meleeDefinition = CreateOrUpdateDefinition(
                "FE_ENEMY_MELEE_Shardling_AI",
                EnemyArchetype.Melee,
                70f,
                1.45f,
                6.8f,
                1.05f,
                8f,
                0.42f,
                true);
            var rangedDefinition = CreateOrUpdateDefinition(
                "FE_ENEMY_RANGED_BloomSpitter_AI",
                EnemyArchetype.Ranged,
                55f,
                1.05f,
                7.6f,
                4.2f,
                6f,
                0.62f,
                true);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "AI_EnemyController_Verification";
            ConfigureRenderSettings();
            CreateLighting();
            CreateCamera();

            var root = new GameObject(RootName);
            CreateFloor(root.transform, materials.floor);
            var target = CreatePlayerTarget(root.transform, materials.player);
            CreateWall(root.transform, materials.wall);
            CreateEnemy(
                root.transform,
                "AI Verify Melee - FE_ENEMY_MELEE_Shardling",
                "Assets/Prefabs/Production/P0/FE_ENEMY_MELEE_Shardling.prefab",
                meleeDefinition,
                target.transform,
                new Vector3(-2.5f, 0.12f, -0.9f),
                Quaternion.LookRotation(Vector3.right, Vector3.up),
                materials.enemy);
            CreateEnemy(
                root.transform,
                "AI Verify Ranged - FE_ENEMY_RANGED_BloomSpitter",
                "Assets/Prefabs/Production/P0/FE_ENEMY_RANGED_BloomSpitter.prefab",
                rangedDefinition,
                target.transform,
                new Vector3(-2.5f, 0.12f, 1.25f),
                Quaternion.LookRotation(Vector3.right, Vector3.up),
                materials.ranged);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"FOURFOLD enemy AI verification scene generated at {ScenePath}");
        }

        public static void ValidateGeneratedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"Enemy AI verification scene is missing or invalid: {ScenePath}");
            }

            Require(RootName);
            Require("AI Test Player Target");
            Require("AI LOS And Movement Wall");
            var controllers = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            if (controllers.Length < 2)
            {
                throw new InvalidOperationException($"Enemy AI verification scene expected at least two EnemyController instances, found {controllers.Length}.");
            }

            for (var index = 0; index < controllers.Length; index++)
            {
                var controller = controllers[index];
                if (controller.definition == null)
                {
                    throw new InvalidOperationException($"Enemy AI controller has no definition: {controller.name}");
                }
                if (controller.targetOverride == null)
                {
                    throw new InvalidOperationException($"Enemy AI controller has no target override: {controller.name}");
                }
                RequireComponent<EnemySensor>(controller.gameObject);
                RequireComponent<EnemyMotor>(controller.gameObject);
                RequireComponent<EnemyAttackDriver>(controller.gameObject);
                RequireComponent<Damageable>(controller.gameObject);
            }

            Debug.Log($"FOURFOLD enemy AI verification scene validation passed; controllers={controllers.Length}");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory(DefinitionFolder);
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
        }

        private static EnemyDefinition CreateOrUpdateDefinition(
            string assetName,
            EnemyArchetype archetype,
            float hp,
            float moveSpeed,
            float detectionRadius,
            float attackRange,
            float damage,
            float telegraphTime,
            bool retreatAfterAttack)
        {
            var path = $"{DefinitionFolder}/{assetName}.asset";
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.enemyId = assetName.Replace("_AI", string.Empty);
            definition.archetype = archetype;
            definition.maxHealth = hp;
            definition.moveSpeed = moveSpeed;
            definition.detectionRadius = detectionRadius;
            definition.loseSightRadius = detectionRadius + 1.6f;
            definition.leashRadius = detectionRadius + 3.2f;
            definition.fieldOfViewDegrees = 240f;
            definition.requireLineOfSight = true;
            definition.lineOfSightMask = ~0;
            definition.targetLayers = ~0;
            definition.obstacleMask = ~0;
            definition.stoppingDistance = Mathf.Min(0.75f, attackRange * 0.65f);
            definition.characterRadius = 0.32f;
            definition.attackRange = attackRange;
            definition.attackRadius = archetype == EnemyArchetype.Ranged ? 0.55f : 0.42f;
            definition.attackArcDegrees = archetype == EnemyArchetype.Ranged ? 60f : 115f;
            definition.attackDamage = damage;
            definition.telegraphTime = telegraphTime;
            definition.activeTime = 0.08f;
            definition.recoveryTime = archetype == EnemyArchetype.Ranged ? 0.55f : 0.42f;
            definition.cooldownTime = archetype == EnemyArchetype.Ranged ? 1.2f : 0.85f;
            definition.retreatAfterAttack = retreatAfterAttack;
            definition.retreatDistance = archetype == EnemyArchetype.Ranged ? 3.1f : 1.8f;
            definition.retreatDuration = archetype == EnemyArchetype.Ranged ? 0.55f : 0.28f;
            definition.drawDebug = true;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.2f, 0.24f);
            RenderSettings.ambientEquatorColor = new Color(0.13f, 0.15f, 0.16f);
            RenderSettings.ambientGroundColor = new Color(0.055f, 0.06f, 0.065f);
        }

        private static void CreateLighting()
        {
            var keyObject = new GameObject("AI Verify Key Light");
            keyObject.transform.rotation = Quaternion.Euler(55f, -36f, 0f);
            var key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.1f;
            key.color = new Color(1f, 0.82f, 0.62f);

            var fillObject = new GameObject("AI Verify Fill Light");
            fillObject.transform.position = new Vector3(0f, 4f, 0f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.intensity = 2.2f;
            fill.range = 9f;
            fill.color = new Color(0.32f, 0.62f, 1f);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("AI Verify Top Down Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(3.7f, 9.2f, -5.6f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 0.1f) - cameraObject.transform.position, Vector3.up);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 60f;
            camera.backgroundColor = new Color(0.07f, 0.085f, 0.10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private static void CreateFloor(Transform parent, Material material)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "AI Verification Floor";
            floor.transform.SetParent(parent);
            floor.transform.position = new Vector3(0.7f, -0.05f, 0f);
            floor.transform.localScale = new Vector3(8.2f, 0.08f, 5.4f);
            ApplyMaterial(floor, material);
        }

        private static GameObject CreatePlayerTarget(Transform parent, Material material)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            target.name = "AI Test Player Target";
            target.transform.SetParent(parent);
            target.transform.position = new Vector3(2.3f, 0.52f, 0.1f);
            target.transform.localScale = new Vector3(0.72f, 0.72f, 0.72f);
            var damageable = target.AddComponent<Damageable>();
            damageable.maxHealth = 300f;
            damageable.ResetHealth();
            ApplyMaterial(target, material);
            return target;
        }

        private static void CreateWall(Transform parent, Material material)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "AI LOS And Movement Wall";
            wall.transform.SetParent(parent);
            wall.transform.position = new Vector3(-0.15f, 0.58f, 0.2f);
            wall.transform.localScale = new Vector3(0.28f, 1.35f, 1.7f);
            ApplyMaterial(wall, material);
        }

        private static void CreateEnemy(
            Transform parent,
            string name,
            string prefabPath,
            EnemyDefinition definition,
            Transform target,
            Vector3 position,
            Quaternion rotation,
            Material fallbackMaterial)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject enemy;
            if (prefab != null)
            {
                enemy = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            else
            {
                enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            }

            if (enemy == null)
            {
                throw new InvalidOperationException($"Could not create enemy from {prefabPath}");
            }

            enemy.name = name;
            enemy.transform.SetParent(parent);
            enemy.transform.position = position;
            enemy.transform.rotation = rotation;

            if (enemy.GetComponentInChildren<Renderer>() == null)
            {
                ApplyMaterial(enemy, fallbackMaterial);
            }

            EnsureComponent<CapsuleCollider>(enemy);
            EnsureComponent<Damageable>(enemy);
            EnsureComponent<EnemyMotor>(enemy);
            var sensor = EnsureComponent<EnemySensor>(enemy);
            EnsureComponent<EnemyAttackDriver>(enemy);
            EnsureComponent<EnemyAnimatorBridge>(enemy);
            var controller = EnsureComponent<EnemyController>(enemy);

            sensor.target = target;
            controller.definition = definition;
            controller.targetOverride = target;
            controller.autoStart = true;
        }

        private static SceneMaterials CreateMaterials()
        {
            return new SceneMaterials
            {
                floor = CreateMaterial("AI_Verify_Floor", new Color(0.16f, 0.20f, 0.19f)),
                wall = CreateMaterial("AI_Verify_Wall", new Color(0.28f, 0.30f, 0.35f)),
                player = CreateMaterial("AI_Verify_Player", new Color(0.35f, 0.85f, 0.72f)),
                enemy = CreateMaterial("AI_Verify_Melee", new Color(0.90f, 0.18f, 0.10f)),
                ranged = CreateMaterial("AI_Verify_Ranged", new Color(0.88f, 0.56f, 0.18f))
            };
        }

        private static void ValidateCombatLoopAndDeath(List<UnityEngine.Object> cleanup)
        {
            var definition = CreateValidationDefinition("editor_combat_loop", cleanup);
            definition.attackDamage = 1f;
            definition.telegraphTime = 0.05f;
            definition.activeTime = 0.02f;
            definition.recoveryTime = 0.05f;
            definition.retreatDuration = 0.15f;
            definition.retreatDistance = 1.7f;

            var target = CreateValidationTarget(new Vector3(2.6f, 0f, 0f), 500f, cleanup);
            var controller = CreateValidationEnemy(Vector3.zero, definition, target.transform, cleanup);
            var visited = new List<EnemyState>();
            controller.StateChanged += (_, next) => visited.Add(next);

            for (var index = 0; index < 48; index++)
            {
                TickValidation(controller, 0.05f);
            }

            Expect(visited.Contains(EnemyState.Chase), "combat loop did not enter Chase: " + string.Join(", ", visited));
            Expect(visited.Contains(EnemyState.Telegraph), "combat loop did not enter Telegraph: " + string.Join(", ", visited));
            Expect(visited.Contains(EnemyState.Attack), "combat loop did not enter Attack: " + string.Join(", ", visited));
            Expect(visited.Contains(EnemyState.Recover), "combat loop did not enter Recover: " + string.Join(", ", visited));
            Expect(visited.Contains(EnemyState.Retreat), "combat loop did not enter Retreat: " + string.Join(", ", visited));
            Expect(target.GetComponent<Damageable>().CurrentHealth < 500f, "attack did not damage the target.");

            controller.Damageable.ApplyDamage(999f, target);
            TickValidation(controller, 0.02f);
            Expect(controller.CurrentState == EnemyState.Dead, "enemy did not transition to Dead after lethal damage.");
        }

        private static void ValidateLeashReturn(List<UnityEngine.Object> cleanup)
        {
            var definition = CreateValidationDefinition("editor_leash", cleanup);
            definition.detectionRadius = 4f;
            definition.loseSightRadius = 5f;
            definition.leashRadius = 3f;
            definition.attackRange = 0.35f;
            definition.moveSpeed = 2.2f;
            definition.retreatAfterAttack = false;

            var target = CreateValidationTarget(new Vector3(2f, 0f, 0f), 100f, cleanup);
            var controller = CreateValidationEnemy(Vector3.zero, definition, target.transform, cleanup);
            TickValidation(controller, 0.1f);
            Expect(controller.CurrentState == EnemyState.Chase, "enemy did not acquire target before leash validation.");

            target.transform.position = new Vector3(8f, 0f, 0f);
            for (var index = 0; index < 60; index++)
            {
                TickValidation(controller, 0.1f);
            }

            Expect(controller.CurrentState == EnemyState.Search, "enemy did not return to Search after leash break.");
            Expect(Vector3.Distance(controller.transform.position, controller.HomePosition) <= definition.stoppingDistance + 0.15f, "enemy did not return close enough to home.");
        }

        private static void ValidateLineOfSightWall(List<UnityEngine.Object> cleanup)
        {
            var definition = CreateValidationDefinition("editor_los", cleanup);
            definition.requireLineOfSight = true;
            definition.lineOfSightMask = 1 << 0;

            var target = CreateValidationTarget(new Vector3(2.5f, 0f, 0f), 100f, cleanup);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cleanup.Add(wall);
            wall.name = "AI Editor Validation LOS Wall";
            wall.transform.position = new Vector3(1.15f, 0.65f, 0f);
            wall.transform.localScale = new Vector3(0.25f, 1.5f, 2.4f);

            var controller = CreateValidationEnemy(Vector3.zero, definition, target.transform, cleanup);
            TickValidation(controller, 0.1f);
            Expect(controller.CurrentState == EnemyState.Search, "enemy detected target through a line-of-sight wall.");

            UnityEngine.Object.DestroyImmediate(wall);
            cleanup.Remove(wall);
            TickValidation(controller, 0.1f);
            Expect(controller.CurrentState == EnemyState.Chase, "enemy did not detect target after line-of-sight wall was removed.");
        }

        private static void ValidateObstacleBlocking(List<UnityEngine.Object> cleanup)
        {
            var definition = CreateValidationDefinition("editor_obstacle", cleanup);
            definition.requireLineOfSight = false;
            definition.attackRange = 0.1f;
            definition.stoppingDistance = 0.05f;
            definition.moveSpeed = 3f;
            definition.obstacleMask = 1 << 0;

            var target = CreateValidationTarget(new Vector3(4f, 0f, 0f), 100f, cleanup);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cleanup.Add(wall);
            wall.name = "AI Editor Validation Movement Wall";
            wall.transform.position = new Vector3(1.15f, 0.5f, 0f);
            wall.transform.localScale = new Vector3(0.3f, 1.4f, 2.8f);

            var controller = CreateValidationEnemy(Vector3.zero, definition, target.transform, cleanup);
            for (var index = 0; index < 24; index++)
            {
                TickValidation(controller, 0.1f);
            }

            Expect(controller.transform.position.x < 0.85f, $"enemy crossed obstacle wall; x={controller.transform.position.x:0.00}");
        }

        private static EnemyDefinition CreateValidationDefinition(string id, List<UnityEngine.Object> cleanup)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            cleanup.Add(definition);
            definition.enemyId = id;
            definition.maxHealth = 50f;
            definition.detectionRadius = 6f;
            definition.loseSightRadius = 7f;
            definition.leashRadius = 8f;
            definition.fieldOfViewDegrees = 360f;
            definition.lineOfSightMask = 0;
            definition.targetLayers = ~0;
            definition.obstacleMask = 0;
            definition.moveSpeed = 3.2f;
            definition.angularSpeed = 1000f;
            definition.stoppingDistance = 0.2f;
            definition.characterRadius = 0.25f;
            definition.repathInterval = 0.01f;
            definition.attackRange = 0.85f;
            definition.attackRadius = 0.35f;
            definition.attackArcDegrees = 160f;
            definition.cooldownTime = 0.05f;
            definition.drawDebug = false;
            return definition;
        }

        private static GameObject CreateValidationTarget(Vector3 position, float health, List<UnityEngine.Object> cleanup)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cleanup.Add(target);
            target.name = "AI Editor Validation Target";
            target.transform.position = position;
            var damageable = target.AddComponent<Damageable>();
            damageable.ConfigureMaxHealth(health, true);
            return target;
        }

        private static EnemyController CreateValidationEnemy(Vector3 position, EnemyDefinition definition, Transform target, List<UnityEngine.Object> cleanup)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cleanup.Add(enemy);
            enemy.name = "AI Editor Validation Enemy";
            enemy.transform.position = position;
            enemy.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            var controller = enemy.AddComponent<EnemyController>();
            controller.autoStart = false;
            controller.definition = definition;
            controller.ResetAi(target);
            controller.enabled = false;
            return controller;
        }

        private static void TickValidation(EnemyController controller, float dt)
        {
            Physics.SyncTransforms();
            controller.Tick(dt);
            Physics.SyncTransforms();
        }

        private static void Expect(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void RunValidationCase(Action<List<UnityEngine.Object>> validate)
        {
            var cleanup = new List<UnityEngine.Object>();
            try
            {
                validate(cleanup);
            }
            finally
            {
                for (var index = cleanup.Count - 1; index >= 0; index--)
                {
                    if (cleanup[index] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(cleanup[index]);
                    }
                }
            }
        }

        private static Material CreateMaterial(string name, Color color)
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
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyMaterial(GameObject gameObject, Material material)
        {
            var renderer = gameObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void Require(string name)
        {
            if (GameObject.Find(name) == null)
            {
                throw new InvalidOperationException($"Required verification scene object missing: {name}");
            }
        }

        private static void RequireComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject.GetComponent<T>() == null)
            {
                throw new InvalidOperationException($"Required component {typeof(T).Name} missing on {gameObject.name}");
            }
        }

        private sealed class SceneMaterials
        {
            public Material floor;
            public Material wall;
            public Material player;
            public Material enemy;
            public Material ranged;
        }
    }
}
