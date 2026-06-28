using System;
using System.IO;
using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatSliceControllerInputTests
    {
        [Test]
        public void UI_ProductionCombatRetry_AcceptsKeyboardAndGamepadMenu()
        {
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.R), Is.True);
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.JoystickButton7), Is.True);
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.JoystickButton0), Is.False);
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.Escape), Is.False);
        }

        [Test]
        public void UI_ProductionCombatDodge_AcceptsKeyboardAndGamepadEastButton()
        {
            Assert.That(ProductionCombatSliceController.IsDodgeKey(KeyCode.Space), Is.True);
            Assert.That(ProductionCombatSliceController.IsDodgeKey(KeyCode.JoystickButton1), Is.True);
            Assert.That(ProductionCombatSliceController.IsDodgeKey(KeyCode.JoystickButton0), Is.False);
            Assert.That(ProductionCombatSliceController.IsDodgeKey(KeyCode.R), Is.False);
        }

        [Test]
        public void UI_ProductionCombatClaimReward_AcceptsKeyboardAndGamepadNorthButton()
        {
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.E), Is.True);
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.JoystickButton3), Is.True);
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.JoystickButton0), Is.False);
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.R), Is.False);
        }

        [Test]
        public void UI_ProductionCombatTitleFlow_ContinueIsSafeWhenSaveIsMissing()
        {
            var tempDirectory = CreateTempDirectory();
            var savePath = Path.Combine(tempDirectory, LocalSaveService.DefaultFileName);
            var controllerObject = new GameObject("Title Flow Controller");
            ProductionCombatSliceController.SaveServiceFactory = () => new LocalSaveService(savePath);

            try
            {
                var controller = controllerObject.AddComponent<ProductionCombatSliceController>();

                Assert.That(controller.HasContinueSave(), Is.False);

                controller.ContinueRun();

                Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Title));
                Assert.That(controller.LastEvent, Is.EqualTo("No local save found"));
            }
            finally
            {
                ProductionCombatSliceController.SaveServiceFactory = LocalSaveService.CreateDefault;
                UnityEngine.Object.DestroyImmediate(controllerObject);
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void UI_ProductionCombatTitleFlow_NewGameClearsPriorSliceProgress()
        {
            var tempDirectory = CreateTempDirectory();
            var savePath = Path.Combine(tempDirectory, LocalSaveService.DefaultFileName);
            var service = new LocalSaveService(savePath);
            var oldData = FourfoldSaveData.CreateNewGame();
            ProductionCombatSliceProgress.Write(oldData, new ProductionCombatSliceProgressSnapshot(true, true, true));
            service.Save(oldData);

            var controllerObject = new GameObject("New Game Controller");
            ProductionCombatSliceController.SaveServiceFactory = () => new LocalSaveService(savePath);

            try
            {
                var controller = controllerObject.AddComponent<ProductionCombatSliceController>();

                Assert.That(controller.HasContinueSave(), Is.True);

                controller.StartNewGame();

                Assert.That(controller.State, Is.EqualTo(ProductionCombatRunState.Playing));
                Assert.That(service.TryLoad(out var newData), Is.True);
                var snapshot = ProductionCombatSliceProgress.Read(newData);
                Assert.That(snapshot.ShortcutOpen, Is.False);
                Assert.That(snapshot.BossDefeated, Is.False);
                Assert.That(snapshot.RewardClaimed, Is.False);
            }
            finally
            {
                ProductionCombatSliceController.SaveServiceFactory = LocalSaveService.CreateDefault;
                UnityEngine.Object.DestroyImmediate(controllerObject);
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void UI_ProductionCombatSettings_MasterVolumeClampsAppliesAndSaves()
        {
            var originalVolume = AudioListener.volume;
            var originalFullscreen = Screen.fullScreen;
            var tempDirectory = CreateTempDirectory();
            var savePath = Path.Combine(tempDirectory, LocalSaveService.DefaultFileName);
            var service = new LocalSaveService(savePath);
            var controllerObject = new GameObject("Settings Controller");
            ProductionCombatSliceController.SaveServiceFactory = () => new LocalSaveService(savePath);

            try
            {
                var controller = controllerObject.AddComponent<ProductionCombatSliceController>();

                controller.SetMasterVolume(0.35f);

                Assert.That(controller.MasterVolume01, Is.EqualTo(0.35f).Within(0.001f));
                Assert.That(AudioListener.volume, Is.EqualTo(0.35f).Within(0.001f));
                Assert.That(controller.SaveStatus, Is.EqualTo("Settings saved"));
                Assert.That(service.TryLoad(out var saved), Is.True);
                Assert.That(saved.settings.masterVolume, Is.EqualTo(0.35f).Within(0.001f));

                controller.SetMasterVolume(1.4f);

                Assert.That(controller.MasterVolume01, Is.EqualTo(1f));
                Assert.That(AudioListener.volume, Is.EqualTo(1f));
            }
            finally
            {
                AudioListener.volume = originalVolume;
                Screen.fullScreen = originalFullscreen;
                ProductionCombatSliceController.SaveServiceFactory = LocalSaveService.CreateDefault;
                UnityEngine.Object.DestroyImmediate(controllerObject);
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void UI_ProductionCombatSettings_FullscreenTogglesAndSaves()
        {
            var originalFullscreen = Screen.fullScreen;
            var tempDirectory = CreateTempDirectory();
            var savePath = Path.Combine(tempDirectory, LocalSaveService.DefaultFileName);
            var service = new LocalSaveService(savePath);
            var controllerObject = new GameObject("Display Settings Controller");
            ProductionCombatSliceController.SaveServiceFactory = () => new LocalSaveService(savePath);

            try
            {
                var controller = controllerObject.AddComponent<ProductionCombatSliceController>();

                controller.SetFullscreen(false);

                Assert.That(controller.FullscreenEnabled, Is.False);
                Assert.That(controller.SaveStatus, Is.EqualTo("Settings saved"));
                Assert.That(service.TryLoad(out var saved), Is.True);
                Assert.That(saved.settings.fullscreen, Is.False);

                controller.ToggleFullscreen();

                Assert.That(controller.FullscreenEnabled, Is.True);
                Assert.That(service.TryLoad(out saved), Is.True);
                Assert.That(saved.settings.fullscreen, Is.True);
            }
            finally
            {
                Screen.fullScreen = originalFullscreen;
                ProductionCombatSliceController.SaveServiceFactory = LocalSaveService.CreateDefault;
                UnityEngine.Object.DestroyImmediate(controllerObject);
                Directory.Delete(tempDirectory, true);
            }
        }

        private static string CreateTempDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), "fourfold-title-flow-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
