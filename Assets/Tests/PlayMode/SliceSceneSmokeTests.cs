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

            Assert.That(controller.SaveStatus, Is.EqualTo("Progress restored"));
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

        [UnityTest]
        public IEnumerator SLICE_PRODUCTION_TitlePauseRetryClearRouteSavesReward()
        {
            SceneManager.LoadScene("ProductionCombatSlice", LoadSceneMode.Single);
            yield return null;

            var controller = FindRequired<ProductionCombatSliceController>();
            var tool = FindRequired<ExplorationTool>();

            Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Title));

            controller.BeginRun();
            yield return null;
            Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Playing));

            controller.SetPaused(true);
            yield return null;
            Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Paused));

            controller.RetryRun();
            yield return null;
            Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Playing));
            Assert.That(controller.ShortcutOpen, Is.False);
            Assert.That(controller.GateOpen, Is.False);
            Assert.That(controller.RewardClaimed, Is.False);

            Assert.That(tool.TryUse(), Is.True);
            yield return null;
            Assert.That(controller.ShortcutOpen, Is.True);

            Assert.That(controller.ClearMinorWardens(), Is.True);
            yield return null;
            Assert.That(controller.BossUnlocked, Is.True);

            Assert.That(controller.ClearBossGate(), Is.True);
            yield return null;
            Assert.That(controller.GateOpen, Is.True);

            Assert.That(controller.ClaimReward(), Is.True);
            yield return null;
            Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Completed));
            Assert.That(controller.RewardClaimed, Is.True);
            Assert.That(controller.SaveStatus, Is.EqualTo("Progress saved"));

            var service = new LocalSaveService(temporarySavePath);
            Assert.That(service.TryLoad(out var saved), Is.True);
            Assert.That(saved.IsShortcutOpened(ProductionCombatSliceProgress.ShortcutId), Is.True);
            Assert.That(saved.IsBossDefeated(ProductionCombatSliceProgress.BossId), Is.True);
            Assert.That(saved.IsRelicClaimed(ProductionCombatSliceProgress.RewardId), Is.True);

            controller.ReturnToTitle();
            yield return null;
            Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Title));
        }

        [UnityTest]
        public IEnumerator SLICE_PRODUCTION_ClaimedRewardRestoresAfterSceneReload()
        {
            SceneManager.LoadScene("ProductionCombatSlice", LoadSceneMode.Single);
            yield return null;

            var controller = FindRequired<ProductionCombatSliceController>();
            var tool = FindRequired<ExplorationTool>();

            controller.BeginRun();
            Assert.That(tool.TryUse(), Is.True);
            Assert.That(controller.ClearMinorWardens(), Is.True);
            Assert.That(controller.ClearBossGate(), Is.True);
            Assert.That(controller.ClaimReward(), Is.True);
            yield return null;

            SceneManager.LoadScene("ProductionCombatSlice", LoadSceneMode.Single);
            yield return null;

            var restoredController = FindRequired<ProductionCombatSliceController>();
            var restoredNode = FindRequired<ExplorationNode>();

            restoredController.BeginRun();
            yield return null;

            Assert.That(restoredController.State, Is.EqualTo(ProductionCombatRunState.Completed));
            Assert.That(restoredController.SaveStatus, Is.EqualTo("Progress restored"));
            Assert.That(restoredController.ShortcutOpen, Is.True);
            Assert.That(restoredController.BossUnlocked, Is.True);
            Assert.That(restoredController.GateOpen, Is.True);
            Assert.That(restoredController.RewardClaimed, Is.True);
            Assert.That(restoredNode.IsSolved, Is.True);
            Assert.That(restoredController.rewardPad, Is.Not.Null);
            Assert.That(restoredController.rewardPad.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator SLICE_PRODUCTION_FreshAppStartRestoresSavedRewardFromDisk()
        {
            SceneManager.LoadScene("ProductionCombatSlice", LoadSceneMode.Single);
            yield return null;

            var controller = FindRequired<ProductionCombatSliceController>();
            var tool = FindRequired<ExplorationTool>();

            controller.BeginRun();
            Assert.That(tool.TryUse(), Is.True);
            Assert.That(controller.ClearMinorWardens(), Is.True);
            Assert.That(controller.ClearBossGate(), Is.True);
            Assert.That(controller.ClaimReward(), Is.True);
            yield return null;

            Assert.That(controller.SaveStatus, Is.EqualTo("Progress saved"));
            Assert.That(File.Exists(temporarySavePath), Is.True);

            SceneManager.LoadScene("D020VerticalSlice", LoadSceneMode.Single);
            yield return null;

            var oldControllers = UnityEngine.Object.FindObjectsByType<ProductionCombatSliceController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            Assert.That(oldControllers, Is.Empty);

            ProductionCombatSliceController.SaveServiceFactory = () => new LocalSaveService(temporarySavePath);

            SceneManager.LoadScene("ProductionCombatSlice", LoadSceneMode.Single);
            yield return null;

            var restoredController = FindRequired<ProductionCombatSliceController>();
            var restoredNode = FindRequired<ExplorationNode>();

            restoredController.BeginRun();
            yield return null;

            Assert.That(restoredController.State, Is.EqualTo(ProductionCombatRunState.Completed));
            Assert.That(restoredController.SaveStatus, Is.EqualTo("Progress restored"));
            Assert.That(restoredController.ShortcutOpen, Is.True);
            Assert.That(restoredController.BossUnlocked, Is.True);
            Assert.That(restoredController.GateOpen, Is.True);
            Assert.That(restoredController.RewardClaimed, Is.True);
            Assert.That(restoredNode.IsSolved, Is.True);
            Assert.That(restoredController.rewardPad, Is.Not.Null);
            Assert.That(restoredController.rewardPad.activeSelf, Is.True);
        }

        private static T FindRequired<T>() where T : UnityEngine.Object
        {
            var matches = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.That(matches, Is.Not.Empty, typeof(T).Name + " should exist in the loaded scene.");
            return matches[0];
        }

        private static int VisibleRendererCount()
        {
            return UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        }
    }
}
