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

        private EnemyController CreateEnemy(Vector3 position, EnemyDefinition definition, Transform target)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cleanup.Add(enemy);
            enemy.name = "AI Test Enemy";
            enemy.transform.position = position;
            enemy.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);

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
    }
}
