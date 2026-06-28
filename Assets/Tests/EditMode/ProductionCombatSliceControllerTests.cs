using FourfoldEchoes.Product;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatSliceControllerTests
    {
        [Test]
        public void UI_DamageEventText_TellsPlayerHowToRecover()
        {
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0.8f),
                Is.EqualTo("Hit taken - dodge next tell"));
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0.3f),
                Is.EqualTo("Hit taken - create space"));
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0.18f),
                Is.EqualTo("Critical hit - dodge now"));
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0f),
                Is.EqualTo("Hero down - choose Retry"));
        }

        [Test]
        public void VIS_RewardPickupRead_SwitchesFromIdleToClaimedState()
        {
            Assert.That(ProductionCombatSliceController.ShouldShowRewardPickupRead(false, false, false), Is.False);
            Assert.That(ProductionCombatSliceController.ShouldShowRewardPickupRead(false, true, true), Is.False);
            Assert.That(ProductionCombatSliceController.ShouldShowRewardPickupRead(true, false, false), Is.True);
            Assert.That(ProductionCombatSliceController.ShouldShowRewardPickupRead(true, false, true), Is.False);
            Assert.That(ProductionCombatSliceController.ShouldShowRewardPickupRead(true, true, false), Is.False);
            Assert.That(ProductionCombatSliceController.ShouldShowRewardPickupRead(true, true, true), Is.True);
        }
    }
}
