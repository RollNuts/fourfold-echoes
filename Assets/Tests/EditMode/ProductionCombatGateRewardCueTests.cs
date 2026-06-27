using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatGateRewardCueTests
    {
        [Test]
        public void UI_GateRewardCue_ShowsOnlyForOpenUnclaimedGate()
        {
            Assert.That(ProductionCombatGateRewardCue.ShouldShowCue(ProductionCombatRunState.Playing, true, false), Is.True);
            Assert.That(ProductionCombatGateRewardCue.ShouldShowCue(ProductionCombatRunState.Playing, true, true), Is.False);
            Assert.That(ProductionCombatGateRewardCue.ShouldShowCue(ProductionCombatRunState.Paused, true, false), Is.False);
            Assert.That(ProductionCombatGateRewardCue.ShouldShowCue(ProductionCombatRunState.Playing, false, false), Is.False);
        }

        [Test]
        public void UI_GateRewardCue_UsesRewardClaimRange()
        {
            var reward = new Vector3(4f, 0f, 1f);
            Assert.That(ProductionCombatGateRewardCue.IsPlayerNearReward(reward + Vector3.right * 1.64f, reward), Is.True);
            Assert.That(ProductionCombatGateRewardCue.IsPlayerNearReward(reward + Vector3.right * 1.66f, reward), Is.False);
        }

        [Test]
        public void UI_GateRewardCue_KeepsPanelInsideSafeFrame()
        {
            var desktop = ProductionCombatGateRewardCue.CalculateCueRect(1920f, 1080f);
            Assert.That(desktop.xMin, Is.GreaterThanOrEqualTo(16f));
            Assert.That(desktop.yMin, Is.GreaterThanOrEqualTo(16f));
            Assert.That(desktop.xMax, Is.LessThanOrEqualTo(1920f - 16f));
            Assert.That(desktop.yMax, Is.LessThanOrEqualTo(1080f - 16f));

            var compact = ProductionCombatGateRewardCue.CalculateCueRect(360f, 640f);
            Assert.That(compact.xMin, Is.GreaterThanOrEqualTo(16f));
            Assert.That(compact.yMin, Is.GreaterThanOrEqualTo(16f));
            Assert.That(compact.xMax, Is.LessThanOrEqualTo(360f - 16f));
            Assert.That(compact.yMax, Is.LessThanOrEqualTo(640f - 16f));
        }

        [Test]
        public void UI_GateRewardCue_ChangesCopyNearReward()
        {
            Assert.That(ProductionCombatGateRewardCue.BuildBodyText(false), Does.Contain("chest"));
            Assert.That(ProductionCombatGateRewardCue.BuildBodyText(true), Does.Contain("Claim reward"));
        }
    }
}
