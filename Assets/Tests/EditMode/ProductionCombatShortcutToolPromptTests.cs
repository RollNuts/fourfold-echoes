using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatShortcutToolPromptTests
    {
        [Test]
        public void UI_ShortcutToolPrompt_ShowsOnlyAfterWardensNearUnsolvedShortcut()
        {
            var shortcut = Vector3.zero;

            Assert.That(
                ProductionCombatShortcutToolPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    wardensHealth01: 0f,
                    shortcutOpen: false,
                    gateOpen: false,
                    rewardClaimed: false,
                    playerPosition: new Vector3(1.2f, 4f, 0.2f),
                    shortcutPosition: shortcut,
                    promptRange: 2.8f),
                Is.True);

            Assert.That(
                ProductionCombatShortcutToolPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    wardensHealth01: 0.25f,
                    shortcutOpen: false,
                    gateOpen: false,
                    rewardClaimed: false,
                    playerPosition: Vector3.zero,
                    shortcutPosition: shortcut,
                    promptRange: 2.8f),
                Is.False);

            Assert.That(
                ProductionCombatShortcutToolPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    wardensHealth01: 0f,
                    shortcutOpen: true,
                    gateOpen: false,
                    rewardClaimed: false,
                    playerPosition: Vector3.zero,
                    shortcutPosition: shortcut,
                    promptRange: 2.8f),
                Is.False);

            Assert.That(
                ProductionCombatShortcutToolPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Paused,
                    wardensHealth01: 0f,
                    shortcutOpen: false,
                    gateOpen: false,
                    rewardClaimed: false,
                    playerPosition: Vector3.zero,
                    shortcutPosition: shortcut,
                    promptRange: 2.8f),
                Is.False);
        }

        [Test]
        public void UI_ShortcutToolPrompt_HidesAfterGateOrRewardAndWhenFar()
        {
            var shortcut = Vector3.zero;

            Assert.That(
                ProductionCombatShortcutToolPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    wardensHealth01: 0f,
                    shortcutOpen: false,
                    gateOpen: true,
                    rewardClaimed: false,
                    playerPosition: Vector3.zero,
                    shortcutPosition: shortcut,
                    promptRange: 2.8f),
                Is.False);

            Assert.That(
                ProductionCombatShortcutToolPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    wardensHealth01: 0f,
                    shortcutOpen: false,
                    gateOpen: false,
                    rewardClaimed: true,
                    playerPosition: Vector3.zero,
                    shortcutPosition: shortcut,
                    promptRange: 2.8f),
                Is.False);

            Assert.That(
                ProductionCombatShortcutToolPrompt.ShouldShowPrompt(
                    ProductionCombatRunState.Playing,
                    wardensHealth01: 0f,
                    shortcutOpen: false,
                    gateOpen: false,
                    rewardClaimed: false,
                    playerPosition: new Vector3(4f, 0f, 0f),
                    shortcutPosition: shortcut,
                    promptRange: 2.8f),
                Is.False);
        }

        [Test]
        public void UI_ShortcutToolPrompt_DescribesReadyAndRecoveringTool()
        {
            Assert.That(
                ProductionCombatShortcutToolPrompt.BuildDetailText(1f),
                Is.EqualTo("Use the Echo Tool at this signal"));
            Assert.That(
                ProductionCombatShortcutToolPrompt.BuildDetailText(0.5f),
                Is.EqualTo("Stay close while the tool recharges"));
        }

        [Test]
        public void UI_ShortcutToolPrompt_StaysInsideNarrowScreen()
        {
            var rect = ProductionCombatShortcutToolPrompt.BuildPromptRect(320f, 240f);

            Assert.That(rect.x, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rect.y, Is.GreaterThanOrEqualTo(24f));
            Assert.That(rect.x + rect.width, Is.LessThanOrEqualTo(320f));
            Assert.That(rect.y + rect.height, Is.LessThanOrEqualTo(240f));
        }
    }
}
