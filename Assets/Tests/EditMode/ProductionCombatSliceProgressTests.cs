using FourfoldEchoes.Product;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatSliceProgressTests
    {
        [Test]
        public void SAVE_ProductionCombatSliceProgress_Read_TreatsRewardAsBossAndShortcutComplete()
        {
            var data = FourfoldSaveData.CreateNewGame();
            data.SetRelicClaimed(ProductionCombatSliceProgress.RewardId, true);

            var snapshot = ProductionCombatSliceProgress.Read(data);

            Assert.That(snapshot.ShortcutOpen, Is.True);
            Assert.That(snapshot.BossDefeated, Is.True);
            Assert.That(snapshot.RewardClaimed, Is.True);
        }

        [Test]
        public void SAVE_ProductionCombatSliceProgress_Read_TreatsBossAsShortcutComplete()
        {
            var data = FourfoldSaveData.CreateNewGame();
            data.SetBossDefeated(ProductionCombatSliceProgress.BossId, true);

            var snapshot = ProductionCombatSliceProgress.Read(data);

            Assert.That(snapshot.ShortcutOpen, Is.True);
            Assert.That(snapshot.BossDefeated, Is.True);
            Assert.That(snapshot.RewardClaimed, Is.False);
        }

        [Test]
        public void SAVE_ProductionCombatSliceProgress_Write_RecordsSceneAndMonotonicFlags()
        {
            var data = FourfoldSaveData.CreateNewGame();
            var snapshot = new ProductionCombatSliceProgressSnapshot(
                shortcutOpen: false,
                bossDefeated: false,
                rewardClaimed: true);

            ProductionCombatSliceProgress.Write(data, snapshot);

            Assert.That(data.currentScene, Is.EqualTo(ProductionCombatSliceProgress.SceneId));
            Assert.That(data.IsShortcutOpened(ProductionCombatSliceProgress.ShortcutId), Is.True);
            Assert.That(data.IsBossDefeated(ProductionCombatSliceProgress.BossId), Is.True);
            Assert.That(data.IsRelicClaimed(ProductionCombatSliceProgress.RewardId), Is.True);
        }

        [Test]
        public void SAVE_ProductionCombatSliceProgress_NullData_IsSafeAndEmpty()
        {
            var snapshot = ProductionCombatSliceProgress.Read(null);

            Assert.That(snapshot.ShortcutOpen, Is.False);
            Assert.That(snapshot.BossDefeated, Is.False);
            Assert.That(snapshot.RewardClaimed, Is.False);
            Assert.DoesNotThrow(() => ProductionCombatSliceProgress.Write(null, snapshot));
        }
    }
}
