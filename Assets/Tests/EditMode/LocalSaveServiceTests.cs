using System;
using System.IO;
using FourfoldEchoes.Product;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class LocalSaveServiceTests
    {
        private string tempDirectory;

        [SetUp]
        public void SetUp()
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "fourfold-save-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void SAVE_LocalSaveService_LoadOrCreate_ReturnsNewGameWhenFileIsMissing()
        {
            var service = new LocalSaveService(Path.Combine(tempDirectory, "missing-save.json"));

            var data = service.LoadOrCreate();

            Assert.That(data, Is.Not.Null);
            Assert.That(data.version, Is.EqualTo(FourfoldSaveData.CurrentVersion));
            Assert.That(data.currentScene, Is.EqualTo("Hub_Crossroads"));
            Assert.That(data.IsRegionUnlocked("Region_01_GreenRuins"), Is.True);
            Assert.That(data.toolOwned, Is.True);
        }

        [Test]
        public void SAVE_LocalSaveService_SaveAndTryLoad_RoundTripsProgressFlagsAndSettings()
        {
            var savePath = Path.Combine(tempDirectory, "slot.json");
            var service = new LocalSaveService(savePath);
            var data = FourfoldSaveData.CreateNewGame();
            data.currentScene = "Region_01_GreenRuins";
            data.hubSpawnId = "hub.return.r01";
            data.SetShortcutOpened("shortcut.r01.bridge", true);
            data.SetBossDefeated("boss.r01.warden", true);
            data.SetRelicClaimed("relic.ember.seed", true);
            data.settings.masterVolume = 1.4f;
            data.settings.musicVolume = -0.2f;
            data.settings.sfxVolume = 0.65f;

            service.Save(data);

            Assert.That(File.Exists(savePath), Is.True);
            Assert.That(service.TryLoad(out var loaded), Is.True);
            Assert.That(loaded.currentScene, Is.EqualTo("Region_01_GreenRuins"));
            Assert.That(loaded.hubSpawnId, Is.EqualTo("hub.return.r01"));
            Assert.That(loaded.IsShortcutOpened("shortcut.r01.bridge"), Is.True);
            Assert.That(loaded.IsBossDefeated("boss.r01.warden"), Is.True);
            Assert.That(loaded.IsRelicClaimed("relic.ember.seed"), Is.True);
            Assert.That(loaded.settings.masterVolume, Is.EqualTo(1f));
            Assert.That(loaded.settings.musicVolume, Is.EqualTo(0f));
            Assert.That(loaded.settings.sfxVolume, Is.EqualTo(0.65f));
        }

        [Test]
        public void SAVE_LocalSaveService_TryLoad_ReturnsFalseForInvalidJson()
        {
            var savePath = Path.Combine(tempDirectory, "corrupt.json");
            File.WriteAllText(savePath, "{ this is not valid json");
            var service = new LocalSaveService(savePath);

            var loaded = service.TryLoad(out var data);

            Assert.That(loaded, Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void SAVE_FourfoldSaveData_Normalize_RemovesBlankAndDuplicateFlags()
        {
            var data = new FourfoldSaveData
            {
                regionsUnlocked = new[] { "Region_01_GreenRuins", "", "Region_01_GreenRuins", "Region_02_SunkenWorks" },
                shortcutsOpened = new[] { "shortcut.a", "shortcut.a", "shortcut.b", " " },
                bossDefeated = null,
                relicsClaimed = null
            };

            data.Normalize();

            CollectionAssert.AreEqual(new[] { "Region_01_GreenRuins", "Region_02_SunkenWorks" }, data.regionsUnlocked);
            CollectionAssert.AreEqual(new[] { "shortcut.a", "shortcut.b" }, data.shortcutsOpened);
            Assert.That(data.bossDefeated, Is.Empty);
            Assert.That(data.relicsClaimed, Is.Empty);
        }
    }
}
