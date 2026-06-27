using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatObjectiveCueTests
    {
        [Test]
        public void UI_ObjectiveCue_HidesOutsidePlayableState()
        {
            Assert.That(Build(ProductionCombatRunState.Title), Is.Empty);
            Assert.That(Build(ProductionCombatRunState.Paused), Is.Empty);
            Assert.That(Build(ProductionCombatRunState.PlayerDown), Is.Empty);
            Assert.That(Build(ProductionCombatRunState.Completed), Is.Empty);
        }

        [Test]
        public void UI_ObjectiveCue_OrdersCoreLoopSteps()
        {
            Assert.That(Build(ProductionCombatRunState.Playing, wardensHealth01: 0.5f), Is.EqualTo("Defeat the wardens"));
            Assert.That(
                Build(ProductionCombatRunState.Playing, wardensHealth01: 0f, shortcutOpen: false, toolReady01: 1f),
                Is.EqualTo("Use the Echo Tool to reveal the shortcut"));
            Assert.That(
                Build(ProductionCombatRunState.Playing, wardensHealth01: 0f, shortcutOpen: false, toolReady01: 0.4f),
                Is.EqualTo("Hold near the shortcut while the Echo Tool recovers"));
            Assert.That(
                Build(ProductionCombatRunState.Playing, wardensHealth01: 0f, shortcutOpen: true, bossUnlocked: true, bossHealth01: 0.7f),
                Is.EqualTo("Break the boss gate"));
            Assert.That(
                Build(ProductionCombatRunState.Playing, gateOpen: true),
                Is.EqualTo("Claim the reward at the open gate"));
        }

        [Test]
        public void UI_ObjectiveCue_UsesReadablePanelPlacement()
        {
            var desktop = ProductionCombatObjectiveCue.BuildPanelRect(1280f, 720f);
            Assert.That(desktop.x, Is.GreaterThan(700f));
            Assert.That(desktop.y, Is.EqualTo(20f));

            var narrow = ProductionCombatObjectiveCue.BuildPanelRect(640f, 360f);
            Assert.That(narrow.x, Is.EqualTo(24f));
            Assert.That(narrow.width, Is.EqualTo(520f));
            Assert.That(narrow.y + narrow.height, Is.LessThanOrEqualTo(360f - 24f));
        }

        private static string Build(
            ProductionCombatRunState state,
            float wardensHealth01 = 1f,
            bool shortcutOpen = false,
            bool bossUnlocked = false,
            float bossHealth01 = 1f,
            bool gateOpen = false,
            bool rewardClaimed = false,
            float toolReady01 = 1f)
        {
            return ProductionCombatObjectiveCue.BuildObjectiveText(
                state,
                wardensHealth01,
                shortcutOpen,
                bossUnlocked,
                bossHealth01,
                gateOpen,
                rewardClaimed,
                toolReady01);
        }
    }
}
