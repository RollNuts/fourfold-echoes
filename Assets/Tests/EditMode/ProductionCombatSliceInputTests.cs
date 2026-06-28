using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatSliceInputTests
    {
        [Test]
        public void Gameplay_RewardClaimInput_MatchesTitlePrompt()
        {
            Assert.That(ProductionCombatSliceController.IsRewardClaimKey(KeyCode.E), Is.True);
            Assert.That(ProductionCombatSliceController.IsRewardClaimKey(KeyCode.JoystickButton3), Is.True);
            Assert.That(ProductionCombatSliceController.RewardClaimMouseButton, Is.EqualTo(1));
            Assert.That(ProductionCombatRewardClaimPrompt.BuildDetailText(), Is.EqualTo("North Button / E / RMB"));
        }

        [Test]
        public void Gameplay_RewardClaimInput_DoesNotStealAttackButton()
        {
            Assert.That(ProductionCombatSliceController.IsRewardClaimKey(KeyCode.J), Is.False);
            Assert.That(ProductionCombatSliceController.IsRewardClaimKey(KeyCode.JoystickButton0), Is.False);
        }

        [Test]
        public void UI_RewardClaimPrompt_ShowsOnlyNearOpenReward()
        {
            var reward = Vector3.zero;
            Assert.That(
                ProductionCombatRewardClaimPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    gateOpen: true,
                    rewardClaimed: false,
                    playerPosition: new Vector3(0f, 4f, 1.4f),
                    rewardPosition: reward,
                    promptRange: 1.65f),
                Is.True);

            Assert.That(
                ProductionCombatRewardClaimPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    gateOpen: false,
                    rewardClaimed: false,
                    playerPosition: Vector3.zero,
                    rewardPosition: reward,
                    promptRange: 1.65f),
                Is.False);

            Assert.That(
                ProductionCombatRewardClaimPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    gateOpen: true,
                    rewardClaimed: true,
                    playerPosition: Vector3.zero,
                    rewardPosition: reward,
                    promptRange: 1.65f),
                Is.False);

            Assert.That(
                ProductionCombatRewardClaimPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    gateOpen: true,
                    rewardClaimed: false,
                    playerPosition: new Vector3(4f, 0f, 0f),
                    rewardPosition: reward,
                    promptRange: 1.65f),
                Is.False);
        }

        [Test]
        public void UI_RewardClaimPrompt_StaysInsideNarrowScreen()
        {
            var rect = ProductionCombatRewardClaimPrompt.BuildPromptRect(320f, 240f);

            Assert.That(rect.x, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rect.y, Is.GreaterThanOrEqualTo(24f));
            Assert.That(rect.x + rect.width, Is.LessThanOrEqualTo(320f));
            Assert.That(rect.y + rect.height, Is.LessThanOrEqualTo(240f));
        }
    }
}
