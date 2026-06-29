using System.Collections;
using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeCombatPreviewTests
    {
        private readonly ArrayList objectsToDestroy = new ArrayList();
        private readonly ArrayList materialsToDestroy = new ArrayList();

        [TearDown]
        public void TearDown()
        {
            foreach (Object objectToDestroy in objectsToDestroy)
            {
                if (objectToDestroy != null)
                {
                    Object.DestroyImmediate(objectToDestroy);
                }
            }

            foreach (Object materialToDestroy in materialsToDestroy)
            {
                if (materialToDestroy != null)
                {
                    Object.DestroyImmediate(materialToDestroy);
                }
            }

            objectsToDestroy.Clear();
            materialsToDestroy.Clear();
        }

        [UnityTest]
        public IEnumerator CombatMode_ActivatesVisualTelegraphsAndHudReadout()
        {
            var controller = CreateControllerHarness(new Vector3(4f, 0.68f, 0f), new Vector3(2f, 0.34f, 0f));

            controller.SetModeForPrototypePreview(BuilderPrototypeMode.CombatHook);
            yield return null;

            Assert.That(controller.CurrentMode, Is.EqualTo(BuilderPrototypeMode.CombatHook));
            Assert.That(controller.CombatPreviewTelegraphCount, Is.EqualTo(2));
            Assert.That(controller.CombatPreviewSafety, Is.EqualTo(BuilderPrototypePositionSafety.Threatened));
            Assert.That(controller.CombatPreviewPositionalBonus, Is.EqualTo(BuilderPrototypePositionalBonus.Rear));
            Assert.That(controller.CombatBuildEdge, Is.EqualTo(12));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Threatened"));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Bonus: Rear"));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Combat Edge: +12"));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Telegraphs: 2"));
            Assert.IsTrue(GameObject.Find("Combat Hook Tactical Preview").activeInHierarchy);
            Assert.IsNotNull(GameObject.Find("Combat Telegraph Circle"));
            Assert.IsNotNull(GameObject.Find("Combat Telegraph Line"));
            Assert.IsNotNull(GameObject.Find("Combat Player Safety Marker"));
        }

        [UnityTest]
        public IEnumerator CombatPreview_UsesTacticalModelResolveWindowForUnsafeReadout()
        {
            var controller = CreateControllerHarness(new Vector3(4f, 0.68f, 0f), new Vector3(2f, 0.34f, 0f));

            controller.SetModeForPrototypePreview(BuilderPrototypeMode.CombatHook);
            yield return new WaitForSeconds(2.25f);

            Assert.That(controller.CombatPreviewSafety, Is.EqualTo(BuilderPrototypePositionSafety.Unsafe));
            Assert.That(controller.CombatPreviewPositionalBonus, Is.EqualTo(BuilderPrototypePositionalBonus.Rear));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Unsafe"));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Unsafe in: 0.0s"));
        }

        [UnityTest]
        public IEnumerator CombatPreview_ReportsSafeWhenPlayerIsOutsideTelegraphs()
        {
            var controller = CreateControllerHarness(new Vector3(-5f, 0.68f, 0f), new Vector3(2f, 0.34f, 0f));

            controller.SetModeForPrototypePreview(BuilderPrototypeMode.CombatHook);
            yield return null;

            Assert.That(controller.CombatPreviewSafety, Is.EqualTo(BuilderPrototypePositionSafety.Safe));
            Assert.That(controller.CombatPreviewPositionalBonus, Is.EqualTo(BuilderPrototypePositionalBonus.None));
            Assert.That(controller.CombatBuildEdge, Is.EqualTo(8));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Combat Preview: Safe"));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Bonus: None"));
            Assert.That(controller.CombatPreviewHudText, Does.Contain("Combat Edge: +8"));
        }

        private BuilderPrototypeSpineController CreateControllerHarness(Vector3 playerPosition, Vector3 combatAnchorPosition)
        {
            var root = new GameObject("Combat Preview Controller Test Harness");
            root.SetActive(false);
            objectsToDestroy.Add(root);

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Combat Preview Test Player";
            player.transform.position = playerPosition;
            objectsToDestroy.Add(player);

            var buildRoot = new GameObject("Combat Preview Test Build Root");
            objectsToDestroy.Add(buildRoot);

            var buildAnchor = CreateAnchor("Combat Preview Build Anchor", new Vector3(-2f, 0.34f, 0f));
            var combatAnchor = CreateAnchor("Combat Preview Combat Anchor", combatAnchorPosition);
            var lootAnchor = CreateAnchor("Combat Preview Loot Anchor", new Vector3(0f, 0.34f, -2f));
            var extractAnchor = CreateAnchor("Combat Preview Extract Anchor", new Vector3(2f, 0.34f, -2f));

            var controller = root.AddComponent<BuilderPrototypeSpineController>();
            controller.player = player.transform;
            controller.buildHookAnchor = buildAnchor.transform;
            controller.combatHookAnchor = combatAnchor.transform;
            controller.lootHookAnchor = lootAnchor.transform;
            controller.extractHookAnchor = extractAnchor.transform;
            controller.editableBlocksRoot = buildRoot.transform;
            controller.placedBlockMaterial = CreateMaterial(Color.gray);
            controller.buildCursorMaterial = CreateMaterial(Color.green);
            controller.combatTelegraphMaterial = CreateMaterial(Color.red);
            controller.combatSafeMarkerMaterial = CreateMaterial(Color.green);
            controller.combatThreatenedMarkerMaterial = CreateMaterial(Color.yellow);
            controller.combatUnsafeMarkerMaterial = CreateMaterial(Color.red);
            controller.showDebugHud = false;

            root.SetActive(true);
            return controller;
        }

        private GameObject CreateAnchor(string objectName, Vector3 position)
        {
            var anchor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            anchor.name = objectName;
            anchor.transform.position = position;
            objectsToDestroy.Add(anchor);
            return anchor;
        }

        private Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            material.color = color;
            materialsToDestroy.Add(material);
            return material;
        }
    }
}
