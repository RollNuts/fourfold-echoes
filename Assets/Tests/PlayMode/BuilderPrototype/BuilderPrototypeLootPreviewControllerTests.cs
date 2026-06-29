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

                var extracted = controller.BankPrototypeLootForPreview();

                Assert.That(extracted.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Extracted));
                Assert.That(extracted.SafetyRollPercent, Is.EqualTo(100));
                Assert.That(extracted.BankedValue, Is.EqualTo(carriedValue));
                Assert.That(controller.BankedLootValue, Is.EqualTo(carriedValue));
                Assert.That(controller.CarriedLootValue, Is.EqualTo(0));
                Assert.That(controller.PressureScore, Is.EqualTo(0));
                Assert.That(controller.LastLootRunEvent, Does.Contain("Extracted"));

                controller.CollectPrototypeLootForPreview();
                carriedValue = controller.CarriedLootValue;

                var lost = controller.LosePrototypeLootForPreview();

                Assert.That(lost.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Lost));
                Assert.That(lost.SafetyRollPercent, Is.EqualTo(0));
                Assert.That(lost.LostValue, Is.EqualTo(carriedValue));
                Assert.That(controller.BankedLootValue, Is.EqualTo(extracted.BankedValue));
                Assert.That(controller.CarriedLootValue, Is.EqualTo(0));
                Assert.That(controller.PressureScore, Is.EqualTo(0));
                Assert.That(controller.LastLootRunEvent, Does.Contain("Lost"));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }
    }
}
