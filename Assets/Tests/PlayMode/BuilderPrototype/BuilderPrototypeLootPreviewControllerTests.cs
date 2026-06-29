using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeLootPreviewControllerTests
    {
        [Test]
        public void CollectPrototypeLootForPreview_UsesLootPressureModelTotals()
        {
            var controllerObject = new GameObject("Loot Preview Controller Test");
            try
            {
                var controller = controllerObject.AddComponent<BuilderPrototypeSpineController>();
                var item = BuilderPrototypeSpineController.CreatePrototypeLootForPreview();

                controller.CollectPrototypeLootForPreview();

                Assert.That(controller.CarriedLootItemCount, Is.EqualTo(1));
                Assert.That(controller.CarriedLootValue, Is.EqualTo(item.ExtractionValue));
                Assert.That(controller.PressureScore, Is.EqualTo(item.PickupPressure));
                Assert.That(controller.ExtractionRiskPercent, Is.GreaterThan(0));
                Assert.That(controller.ExtractionGuardRiskReduction, Is.EqualTo(2));
                Assert.That(controller.AdjustedExtractionRiskPercent, Is.EqualTo(controller.ExtractionRiskPercent - controller.ExtractionGuardRiskReduction));
                Assert.That(controller.ExtractionRiskHudText, Is.EqualTo("Risk " + controller.ExtractionRiskPercent + "% -> " + controller.AdjustedExtractionRiskPercent + "%"));
                Assert.That(controller.ExtractionGuardHudText, Is.EqualTo("Guard Buffer -2% (SentinelGuard 15 + Vitality 120)"));
                Assert.That(controller.LastLootRunEvent, Does.Contain("Picked up"));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void ExtractPreview_DeterministicallyReportsBankedAndLostOutcomes()
        {
            var controllerObject = new GameObject("Loot Preview Extract Test");
            try
            {
                var controller = controllerObject.AddComponent<BuilderPrototypeSpineController>();
                controller.CollectPrototypeLootForPreview();
                var carriedValue = controller.CarriedLootValue;
                var rawRisk = controller.ExtractionRiskPercent;
                var adjustedRisk = controller.AdjustedExtractionRiskPercent;
                var guardRiskReduction = controller.ExtractionGuardRiskReduction;

                var extracted = controller.BankPrototypeLootForPreview();

                Assert.That(extracted.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Extracted));
                Assert.That(extracted.SafetyRollPercent, Is.EqualTo(100));
                Assert.That(extracted.RawRiskPercent, Is.EqualTo(rawRisk));
                Assert.That(extracted.RiskReductionPercent, Is.EqualTo(guardRiskReduction));
                Assert.That(extracted.RiskPercent, Is.EqualTo(adjustedRisk));
                Assert.That(extracted.BankedValue, Is.EqualTo(carriedValue));
                Assert.That(controller.BankedLootValue, Is.EqualTo(carriedValue));
                Assert.That(controller.CarriedLootValue, Is.EqualTo(0));
                Assert.That(controller.PressureScore, Is.EqualTo(0));
                Assert.That(controller.LastLootRunEvent, Does.Contain("Extracted"));
                Assert.That(controller.LastLootRunEvent, Does.Contain("risk " + rawRisk + "% -> " + adjustedRisk + "%"));

                controller.CollectPrototypeLootForPreview();
                carriedValue = controller.CarriedLootValue;
                rawRisk = controller.ExtractionRiskPercent;
                adjustedRisk = controller.AdjustedExtractionRiskPercent;

                var lost = controller.LosePrototypeLootForPreview();

                Assert.That(lost.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Lost));
                Assert.That(lost.SafetyRollPercent, Is.EqualTo(0));
                Assert.That(lost.RawRiskPercent, Is.EqualTo(rawRisk));
                Assert.That(lost.RiskReductionPercent, Is.EqualTo(guardRiskReduction));
                Assert.That(lost.RiskPercent, Is.EqualTo(adjustedRisk));
                Assert.That(lost.LostValue, Is.EqualTo(carriedValue));
                Assert.That(controller.BankedLootValue, Is.EqualTo(extracted.BankedValue));
                Assert.That(controller.CarriedLootValue, Is.EqualTo(0));
                Assert.That(controller.PressureScore, Is.EqualTo(0));
                Assert.That(controller.LastLootRunEvent, Does.Contain("Lost"));
                Assert.That(controller.LastLootRunEvent, Does.Contain("risk " + rawRisk + "% -> " + adjustedRisk + "%"));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }
    }
}
