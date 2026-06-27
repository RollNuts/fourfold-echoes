using System;
using System.Collections;
using System.IO;
using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FourfoldEchoes.Tests.PlayMode
{
    public sealed class SliceSceneSmokeTests
    {
        private string temporarySaveDirectory;
        private string temporarySavePath;

        [SetUp]
        public void SetUp()
        {
            temporarySaveDirectory = Path.Combine(Path.GetTempPath(), "fourfold-playmode-save-" + Guid.NewGuid().ToString("N"));
            temporarySavePath = Path.Combine(temporarySaveDirectory, LocalSaveService.DefaultFileName);
            ProductionCombatSliceController.SaveServiceFactory = () => new LocalSaveService(temporarySavePath);
        }

        [TearDown]
        public void TearDown()
        {
            ProductionCombatSliceController.SaveServiceFactory = LocalSaveService.CreateDefault;
            if (!string.IsNullOrEmpty(temporarySaveDirectory) && Directory.Exists(temporarySaveDirectory))
            {
                Directory.Delete(temporarySaveDirectory, true);
            }
        }

        [UnityTest]
        public IEnumerator SLICE_D020_SceneLoadsWithOneToolNodeShortcutAndReadableCamera()
        {
            SceneManager.LoadScene("D020VerticalSlice", LoadSceneMode.Single);
            yield return null;

            var scene = SceneManager.GetActiveScene();
            Assert.That(scene.path, Is.EqualTo("Assets/Scenes/D020VerticalSlice.unity"));

            var tool = FindRequired<ExplorationTool>();
            var node = FindRequired<ExplorationNode>();
            var camera = Camera.main;

            Assert.That(tool.player, Is.Not.Null);
            Assert.That(tool.nodes, Has.Member(node));
            Assert.That(node.responseTarget, Is.Not.Null);
            Assert.That(node.responseTarget.name, Is.EqualTo("D020 Shortcut Route"));
            Assert.That(node.IsSolved, Is.False);
            Assert.That(node.responseTarget.activeSelf, Is.False);

            Assert.That(tool.TryUse(), Is.True);

            Assert.That(node.IsSolved, Is.True);
            Assert.That(node.responseTarget.activeSelf, Is.True);
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.orthographic, Is.True);
            Assert.That(VisibleRendererCount(), Is.GreaterThanOrEqualTo(24));
        }

        [UnityTest]
        public IEnumerator SLICE_PRODUCTION_SceneLoadsWithCombatGateRewardAndExplorationShortcut()
        {
            SceneManager.LoadScene("ProductionCombatSlice", LoadSceneMode.Single);
            yield return null;

            var scene = SceneManager.GetActiveScene();
            Assert.That(scene.path, Is.EqualTo("Assets/Scenes/ProductionCombatSlice.unity"));

            var controller = FindRequired<ProductionCombatSliceController>();
            var tool = FindRequired<ExplorationTool>();
            var node = FindRequired<ExplorationNode>();

            Assert.That(controller.HostileCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.enemies, Is.Not.Null);
            Assert.That(controller.enemies.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(controller.boss, Is.Not.Null);
            Assert.That(controller.rewardChest, Is.Not.Null);
            Assert.That(controller.rewardPad, Is.Not.Null);
            Assert.That(controller.explorationTool, Is.SameAs(tool));
            Assert.That(controller.shortcutNode, Is.SameAs(node));
            Assert.That(controller.BossUnlocked, Is.False);
            Assert.That(controller.GateOpen, Is.False);
            Assert.That(controller.RewardClaimed, Is.False);
            Assert.That(node.responseTarget, Is.Not.Null);
            Assert.That(node.responseTarget.activeSelf, Is.False);

            Assert.That(tool.TryUse(), Is.True);

            Assert.That(node.IsSolved, Is.True);
            Assert.That(node.responseTarget.activeSelf, Is.True);

            yield return null;

            Assert.That(controller.BossUnlocked, Is.False);
            Assert.That(controller.GateOpen, Is.False);
            Assert.That(VisibleRendererCount(), Is.GreaterThanOrEqualTo(120));
        }

        [UnityTest]
        public IEnumerator SLICE_PRODUCTION_SavedRewardRestoresShortcutGateAndCompletedState()
        {
            var service = new LocalSaveService(temporarySavePath);
            var data = FourfoldSaveData.CreateNewGame();
            ProductionCombatSliceProgress.Write(data, new ProductionCombatSliceProgressSnapshot(false, false, true));
            service.Save(data);

            SceneManager.LoadScene("ProductionCombatSlice", LoadSceneMode.Single);
            yield return null;

            var controller = FindRequired<ProductionCombatSliceController>();
            var node = FindRequired<ExplorationNode>();

            controller.BeginRun();
            yield return null;

            Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Completed));
            Assert.That(controller.ShortcutOpen, Is.True);
            Assert.That(controller.BossUnlocked, Is.True);
            Assert.That(controller.GateOpen, Is.True);
            Assert.That(controller.RewardClaimed, Is.True);
            Assert.That(node.IsSolved, Is.True);
            Assert.That(node.responseTarget, Is.Not.Null);
            Assert.That(node.responseTarget.activeSelf, Is.True);
            Assert.That(controller.rewardPad, Is.Not.Null);
            Assert.That(controller.rewardPad.activeSelf, Is.True);
        }

        private static T FindRequired<T>() where T : Object
        {
            var matches = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.That(matches, Is.Not.Empty, typeof(T).Name + " should exist in the loaded scene.");
            return matches[0];
        }

        private static int VisibleRendererCount()
        {
            return Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        }
    }
}
