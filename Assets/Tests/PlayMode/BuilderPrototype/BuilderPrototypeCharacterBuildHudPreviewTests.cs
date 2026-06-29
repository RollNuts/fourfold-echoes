using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeCharacterBuildHudPreviewTests
    {
        [Test]
        public void CharacterBuildHud_ReportsDeterministicPrototypeIdentityAndStats()
        {
            var controllerObject = new GameObject("Character Build HUD Preview Test");
            try
            {
                var controller = controllerObject.AddComponent<BuilderPrototypeSpineController>();

                Assert.IsTrue(controller.IsPrototypeCharacterBuildValid);
                Assert.IsTrue(controller.CharacterBuildSnapshot.HasRole(BuilderPrototypeBuildRoleTag.Builder));
                Assert.IsTrue(controller.CharacterBuildSnapshot.HasRole(BuilderPrototypeBuildRoleTag.Striker));
                Assert.That(controller.CharacterBuildHudText, Does.Contain("Build: Echo Forgemason"));
                Assert.That(controller.CharacterBuildHudText, Does.Contain("Role: Builder/Striker"));
                Assert.That(controller.CharacterBuildHudText, Does.Contain("Build 18 Speed 1.25"));
                Assert.That(controller.CharacterBuildHudText, Does.Contain("Off 18 Break 16 Guard 15 Vit 120"));
                Assert.That(controller.CharacterBuildHudText, Does.Contain("Press 0/100 Quiet | Risk 0% -> 0% | Guard Buffer -2%"));
                Assert.That(controller.CharacterBuildIdentityHudText, Is.EqualTo("Build: Echo Forgemason | Role: Builder/Striker"));
                Assert.That(controller.CharacterBuildStatsHudText, Is.EqualTo("Stats: Build 18 Speed 1.25 | Off 18 Break 16 Guard 15 Vit 120"));
                Assert.That(controller.CharacterBuildPressureHudText, Is.EqualTo("Run: Press 0/100 Quiet | Risk 0% -> 0% | Guard Buffer -2% (SentinelGuard 15 + Vitality 120)"));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void CharacterBuildSourceHud_ReportsSelectedAffixSourceSummary()
        {
            var controllerObject = new GameObject("Character Build Source HUD Test");
            try
            {
                var controller = controllerObject.AddComponent<BuilderPrototypeSpineController>();

                Assert.That(controller.CharacterBuildSourceHudText, Is.EqualTo(
                    "Source: Echo Chisel (Gear) affix 4/4 | +BuilderPower +BuildSpeed +StrikerDamage"));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void CharacterBuildGameplayHooks_ApplyDeterministicStatsToBuildPreview()
        {
            var controllerObject = new GameObject("Character Build Gameplay Hook Test");
            try
            {
                var controller = controllerObject.AddComponent<BuilderPrototypeSpineController>();

                Assert.That(controller.BuilderPowerBuildBlockBonus, Is.EqualTo(2));
                Assert.That(controller.StartingBuildBlockCapacity, Is.EqualTo(20));
                Assert.That(controller.BuildBlocksAvailable, Is.EqualTo(20));
                Assert.That(controller.BuildCursorRepeatDuration, Is.EqualTo(0.144f).Within(0.001f));
                Assert.That(controller.BuildSpeedCursorRepeatMilliseconds, Is.EqualTo(144));
                Assert.That(controller.CharacterBuildGameplayHudText, Is.EqualTo(
                    "Hooks: Blocks 20 (+2 BuilderPower) | Cursor Repeat 0.14s BuildSpeed"));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void CharacterBuildHud_UsesRunPressureWithoutPermanentProgression()
        {
            var controllerObject = new GameObject("Character Build HUD Pressure Test");
            try
            {
                var controller = controllerObject.AddComponent<BuilderPrototypeSpineController>();
                var prototypeLoot = BuilderPrototypeSpineController.CreatePrototypeLootForPreview();

                controller.CollectPrototypeLootForPreview();

                Assert.That(controller.CharacterBuildHudText, Does.Contain(
                    "Press " + prototypeLoot.PickupPressure + "/100 Quiet | Risk " + controller.ExtractionRiskPercent + "% -> " + controller.AdjustedExtractionRiskPercent + "% | Guard Buffer -2%"));
                Assert.That(controller.CharacterBuildPressureHudText, Is.EqualTo(
                    "Run: Press " + prototypeLoot.PickupPressure + "/100 Quiet | Risk " + controller.ExtractionRiskPercent + "% -> " + controller.AdjustedExtractionRiskPercent + "% | Guard Buffer -2% (SentinelGuard 15 + Vitality 120)"));

                controller.ResetPrototypeRun();

                Assert.That(controller.CharacterBuildHudText, Does.Contain("Build: Echo Forgemason"));
                Assert.That(controller.CharacterBuildHudText, Does.Contain("Press 0/100 Quiet | Risk 0% -> 0%"));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }
    }
}
