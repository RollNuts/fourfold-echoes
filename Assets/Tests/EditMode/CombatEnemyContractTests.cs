using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class CombatEnemyContractTests
    {
        private readonly System.Collections.Generic.List<Object> createdObjects = new System.Collections.Generic.List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var index = createdObjects.Count - 1; index >= 0; index--)
            {
                if (createdObjects[index] != null)
                {
                    Object.DestroyImmediate(createdObjects[index]);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void CORE_DAMAGEABLE_ApplyDamage_ClampsDamageAndRaisesDeathOnce()
        {
            var damageable = CreateDamageable("Player", 30f);
            var damagedEvents = 0;
            var deathEvents = 0;
            DamageInfo lastInfo = default;

            damageable.Damaged += (_, info) =>
            {
                damagedEvents++;
                lastInfo = info;
            };
            damageable.Died += (_, info) =>
            {
                deathEvents++;
                lastInfo = info;
            };

            Assert.That(damageable.ApplyDamage(12f), Is.True);
            Assert.That(damageable.CurrentHealth, Is.EqualTo(18f).Within(0.001f));
            Assert.That(damagedEvents, Is.EqualTo(1));
            Assert.That(deathEvents, Is.EqualTo(0));

            Assert.That(damageable.ApplyDamage(99f), Is.True);
            Assert.That(damageable.IsAlive, Is.False);
            Assert.That(damageable.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.That(lastInfo.Amount, Is.EqualTo(18f).Within(0.001f));
            Assert.That(damagedEvents, Is.EqualTo(2));
            Assert.That(deathEvents, Is.EqualTo(1));

            Assert.That(damageable.ApplyDamage(1f), Is.False);
            Assert.That(damagedEvents, Is.EqualTo(2));
            Assert.That(deathEvents, Is.EqualTo(1));
        }

        [Test]
        public void CORE_ENEMY_ATTACK_ResolveHit_DamagesOnlyLivingTargetInsideForwardArc()
        {
            var owner = CreatePrimitive("Enemy", Vector3.zero);
            owner.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            owner.AddComponent<Damageable>().ConfigureMaxHealth(40f, true);

            var driver = owner.AddComponent<EnemyAttackDriver>();
            var definition = CreateAttackDefinition();
            var frontTarget = CreateDamageable("Front Target", 50f, new Vector3(0.9f, 0f, 0f));
            var rearTarget = CreateDamageable("Rear Target", 50f, new Vector3(-0.9f, 0f, 0f));
            var deadTarget = CreateDamageable("Dead Target", 50f, new Vector3(0.65f, 0f, 0.5f));
            deadTarget.Kill(owner);

            Physics.SyncTransforms();

            var hits = driver.ResolveHit(definition, owner);

            Assert.That(hits, Is.EqualTo(1));
            Assert.That(frontTarget.CurrentHealth, Is.EqualTo(40f).Within(0.001f));
            Assert.That(rearTarget.CurrentHealth, Is.EqualTo(50f).Within(0.001f));
            Assert.That(deadTarget.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.That(owner.GetComponent<Damageable>().CurrentHealth, Is.EqualTo(40f).Within(0.001f));
        }

        [Test]
        public void CORE_ENEMY_ATTACK_ResolveHit_DoesNotApplyDuplicateDamageThroughMultipleColliders()
        {
            var owner = CreatePrimitive("Enemy", Vector3.zero);
            owner.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            var driver = owner.AddComponent<EnemyAttackDriver>();
            var definition = CreateAttackDefinition();
            var targetRoot = new GameObject("Target Root");
            createdObjects.Add(targetRoot);
            targetRoot.transform.position = new Vector3(0.85f, 0f, 0f);
            var damageable = targetRoot.AddComponent<Damageable>();
            damageable.ConfigureMaxHealth(50f, true);

            var childA = CreatePrimitive("Target Collider A", Vector3.zero);
            childA.transform.SetParent(targetRoot.transform, false);
            var childB = CreatePrimitive("Target Collider B", new Vector3(0.08f, 0f, 0f));
            childB.transform.SetParent(targetRoot.transform, false);

            Physics.SyncTransforms();

            var hits = driver.ResolveHit(definition, owner);

            Assert.That(hits, Is.EqualTo(1));
            Assert.That(damageable.CurrentHealth, Is.EqualTo(40f).Within(0.001f));
        }

        private Damageable CreateDamageable(string name, float health, Vector3 position = default)
        {
            var gameObject = CreatePrimitive(name, position);
            var damageable = gameObject.AddComponent<Damageable>();
            damageable.ConfigureMaxHealth(health, true);
            return damageable;
        }

        private GameObject CreatePrimitive(string name, Vector3 position)
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            createdObjects.Add(gameObject);
            gameObject.name = name;
            gameObject.transform.position = position;
            return gameObject;
        }

        private EnemyDefinition CreateAttackDefinition()
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            createdObjects.Add(definition);
            definition.attackDamage = 10f;
            definition.attackRange = 0.85f;
            definition.attackRadius = 0.4f;
            definition.attackArcDegrees = 95f;
            definition.targetLayers = ~0;
            definition.drawDebug = false;
            return definition;
        }
    }
}
