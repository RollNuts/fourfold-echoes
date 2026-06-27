using System.Collections;
using System.Collections.Generic;
using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FourfoldEchoes.Tests
{
    public sealed class EnemyControllerPlayModeTests
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        private readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var index = cleanup.Count - 1; index >= 0; index--)
            {
                if (cleanup[index] != null)
                {
                    Object.Destroy(cleanup[index]);
                }
            }
            cleanup.Clear();
        }

        [UnityTest]
        public IEnumerator EnemyController_TransitionsThroughCombatLoopAndDeath()
        {
            var definition = CreateDefinition("test_melee");
            definition.attackDamage = 1f;
            definition.telegraphTime = 0.05f;
            definition.activeTime = 0.02f;
            definition.recoveryTime = 0.05f;
            definition.retreatDuration = 0.15f;
            definition.retreatDistance = 1.7f;

            var target = CreateTarget(new Vector3(2.6f, 0f, 0f), 500f);
            var controller = CreateEnemy(Vector3.zero, definition, target.transform);
            var visited = new List<EnemyState>();
            controller.StateChanged += (_, next) => visited.Add(next);

            for (var index = 0; index < 48; index++)
            {
                controller.Tick(0.05f);
            }

            CollectionAssert.Contains(visited, EnemyState.Chase);
            CollectionAssert.Contains(visited, EnemyState.Telegraph);
            CollectionAssert.Contains(visited, EnemyState.Attack);
            CollectionAssert.Contains(visited, EnemyState.Recover);
            CollectionAssert.Contains(visited, EnemyState.Retreat);
            Assert.Less(target.GetComponent<Damageable>().CurrentHealth, 500f);

            controller.Damageable.ApplyDamage(999f, target);
            controller.Tick(0.02f);
            Assert.AreEqual(EnemyState.Dead, controller.CurrentState);

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyController_TintsRendererDuringReadableAttackStates()
        {
            var definition = CreateDefinition("test_readability");
            definition.attackRange = 1.4f;
            definition.telegraphTime = 0.12f;
            definition.activeTime = 0.08f;
            definition.recoveryTime = 0.12f;
            definition.retreatAfterAttack = false;

            var target = CreateTarget(new Vector3(0.9f, 0f, 0f), 100f);
            var controller = CreateEnemy(Vector3.zero, definition, target.transform);
            var enemyRenderer = controller.GetComponentInChildren<Renderer>();
            Assert.IsNotNull(enemyRenderer);

            controller.Tick(0.01f);
            controller.Tick(0.01f);
            Assert.AreEqual(EnemyState.Telegraph, controller.CurrentState);
            AssertColorApproximately(controller.telegraphTint, ReadTint(enemyRenderer));

            controller.Tick(definition.telegraphTime + 0.01f);
            Assert.AreEqual(EnemyState.Attack, controller.CurrentState);
            AssertColorApproximately(controller.attackTint, ReadTint(enemyRenderer));

            controller.Tick(definition.activeTime + 0.01f);
            Assert.AreEqual(EnemyState.Recover, controller.CurrentState);
            AssertColorApproximately(controller.recoverTint, ReadTint(enemyRenderer));

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyController_ShowsGroundMarkerForIncomingAttack()
        {
            var definition = CreateDefinition("test_ground_marker");
            definition.attackRange = 1.4f;
            definition.attackRadius = 0.45f;
            definition.telegraphTime = 0.12f;
            definition.activeTime = 0.08f;
            definition.recoveryTime = 0.12f;
            definition.retreatAfterAttack = false;

            var target = CreateTarget(new Vector3(0.9f, 0f, 0f), 100f);
            var controller = CreateEnemy(Vector3.zero, definition, target.transform);

            controller.Tick(0.01f);
            controller.Tick(0.01f);
            Assert.AreEqual(EnemyState.Telegraph, controller.CurrentState);

            var marker = controller.TelegraphGroundMarkerInstance;
            Assert.IsNotNull(marker);
            Assert.IsTrue(marker.activeSelf);
            Assert.That(marker.transform.position.x, Is.EqualTo(definition.attackRange).Within(0.05f));
            Assert.That(marker.transform.localScale.x, Is.EqualTo(definition.attackRadius * 2f).Within(0.01f));

            var markerCollider = marker.GetComponentInChildren<Collider>();
            Assert.IsNotNull(markerCollider);
            Assert.IsFalse(markerCollider.enabled);
            AssertColorApproximately(controller.telegraphGroundMarkerColor, ReadTint(marker.GetComponentInChildren<Renderer>()));

            controller.Tick(definition.telegraphTime + 0.01f);
            Assert.AreEqual(EnemyState.Attack, controller.CurrentState);
            Assert.IsTrue(marker.activeSelf);

            controller.Tick(definition.activeTime + 0.01f);
            Assert.AreEqual(EnemyState.Recover, controller.CurrentState);
            Assert.IsFalse(marker.activeSelf);

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyController_ReturnsHomeWhenTargetBreaksLeash()
        {
            var definition = CreateDefinition("test_leash");
            definition.detectionRadius = 4f;
            definition.loseSightRadius = 5f;
            definition.leashRadius = 3f;
            definition.attackRange = 0.35f;
            definition.moveSpeed = 2.2f;
            definition.retreatAfterAttack = false;

            var target = CreateTarget(new Vector3(2f, 0f, 0f), 100f);
            var controller = CreateEnemy(Vector3.zero, definition, target.transform);

            controller.Tick(0.1f);
            Assert.AreEqual(EnemyState.Chase, controller.CurrentState);

            target.transform.position = new Vector3(8f, 0f, 0f);
            for (var index = 0; index < 60; index++)
            {
                controller.Tick(0.1f);
            }

            Assert.AreEqual(EnemyState.Search, controller.CurrentState);
            Assert.LessOrEqual(Vector3.Distance(controller.transform.position, controller.HomePosition), definition.stoppingDistance + 0.15f);

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyController_ShowsTelegraphVfxDuringTellAndHidesAfterImpact()
        {
            var definition = CreateDefinition("test_vfx");
            definition.attackRange = 0.85f;
            definition.attackRadius = 0.35f;
            definition.telegraphTime = 0.12f;
            definition.activeTime = 0.04f;
            definition.recoveryTime = 0.08f;
            definition.retreatAfterAttack = false;

            var target = CreateTarget(new Vector3(0.65f, 0f, 0f), 100f);
            var controller = CreateEnemy(Vector3.zero, definition, target.transform, CreateTelegraphPrefab());
            var telegraphVfx = controller.GetComponent<EnemyTelegraphVfx>();

            controller.Tick(0.02f);
            controller.Tick(0.02f);

            Assert.AreEqual(EnemyState.Telegraph, controller.CurrentState);
            Assert.IsTrue(telegraphVfx.IsVisible);
            Assert.NotNull(telegraphVfx.ActiveInstance);
            Assert.Greater(telegraphVfx.ActiveInstance.transform.localScale.x, 0f);

            for (var index = 0; index < 12 && controller.CurrentState != EnemyState.Recover; index++)
            {
                controller.Tick(0.05f);
            }

            Assert.AreEqual(EnemyState.Recover, controller.CurrentState);
            Assert.IsFalse(telegraphVfx.IsVisible);

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyController_LineOfSightBlocksSearchThroughWall()
        {
            var definition = CreateDefinition("test_los");
            definition.requireLineOfSight = true;
            definition.lineOfSightMask = 1 << 0;

            var target = CreateTarget(new Vector3(2.5f, 0f, 0f), 100f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cleanup.Add(wall);
            wall.name = "AI Test LOS Wall";
            wall.transform.position = new Vector3(1.15f, 0.65f, 0f);
            wall.transform.localScale = new Vector3(0.25f, 1.5f, 2.4f);
            Physics.SyncTransforms();

            var controller = CreateEnemy(Vector3.zero, definition, target.transform);
            controller.Tick(0.1f);
            Assert.AreEqual(EnemyState.Search, controller.CurrentState);

            Object.Destroy(wall);
            yield return null;
            Physics.SyncTransforms();
            controller.Tick(0.1f);
            Assert.AreEqual(EnemyState.Chase, controller.CurrentState);
        }

        [UnityTest]
        public IEnumerator EnemyMotor_FallbackMovementDoesNotPassThroughObstacle()
        {
            var definition = CreateDefinition("test_obstacle");
            definition.requireLineOfSight = false;
            definition.attackRange = 0.1f;
            definition.stoppingDistance = 0.05f;
            definition.moveSpeed = 3f;
            definition.obstacleMask = 1 << 0;

            var target = CreateTarget(new Vector3(4f, 0f, 0f), 100f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cleanup.Add(wall);
            wall.name = "AI Test Movement Wall";
            wall.transform.position = new Vector3(1.15f, 0.5f, 0f);
            wall.transform.localScale = new Vector3(0.3f, 1.4f, 2.8f);

            var controller = CreateEnemy(Vector3.zero, definition, target.transform);
            for (var index = 0; index < 24; index++)
            {
                controller.Tick(0.1f);
            }

            Assert.Less(controller.transform.position.x, 0.85f);

            yield return null;
        }

        private EnemyController CreateEnemy(Vector3 position, EnemyDefinition definition, Transform target, GameObject telegraphPrefab = null)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cleanup.Add(enemy);
            enemy.name = "AI Test Enemy";
            enemy.transform.position = position;
            enemy.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);

            if (telegraphPrefab != null)
            {
                var telegraphVfx = enemy.AddComponent<EnemyTelegraphVfx>();
                telegraphVfx.telegraphPrefab = telegraphPrefab;
            }

            var controller = enemy.AddComponent<EnemyController>();
            controller.autoStart = false;
            controller.definition = definition;
            controller.ResetAi(target);
            controller.enabled = false;
            return controller;
        }

        private GameObject CreateTarget(Vector3 position, float health)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cleanup.Add(target);
            target.name = "AI Test Target";
            target.transform.position = position;
            var damageable = target.AddComponent<Damageable>();
            damageable.ConfigureMaxHealth(health, true);
            return target;
        }

        private GameObject CreateTelegraphPrefab()
        {
            var prefab = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cleanup.Add(prefab);
            prefab.name = "AI Test Enemy Telegraph VFX Prefab";
            prefab.SetActive(false);
            return prefab;
        }

        private EnemyDefinition CreateDefinition(string id)
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

        private static Color ReadTint(Renderer targetRenderer)
        {
            var block = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(block);
            return block.GetColor(BaseColorProperty);
        }

        private static void AssertColorApproximately(Color expected, Color actual)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.01f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.01f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.01f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.01f));
        }
    }
}
