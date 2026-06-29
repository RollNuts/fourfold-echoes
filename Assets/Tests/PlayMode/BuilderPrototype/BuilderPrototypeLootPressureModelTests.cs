using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeLootPressureModelTests
    {
        [Test]
        public void RarityTiers_ClampAffixSlotsAndScalePowerBudget()
        {
            var common = new BuilderPrototypeLootItem("stone-chip", BuilderPrototypeLootRarity.Common, 5, 4);
            var legendary = new BuilderPrototypeLootItem("echo-core", BuilderPrototypeLootRarity.Legendary, 5, 4);

            Assert.That(common.AffixSlotCount, Is.EqualTo(0));
            Assert.That(legendary.AffixSlotCount, Is.EqualTo(4));
            Assert.That(BuilderPrototypeLootItem.MaxAffixSlotsFor(BuilderPrototypeLootRarity.Rare), Is.EqualTo(2));
            Assert.That(legendary.PowerBudget, Is.GreaterThan(common.PowerBudget));
            Assert.That(legendary.ExtractionValue, Is.GreaterThan(common.ExtractionValue));
            Assert.That(legendary.CarryRiskWeight, Is.GreaterThan(common.CarryRiskWeight));
        }

        [Test]
        public void ItemConstruction_ClampsPrototypeBounds()
        {
            var item = new BuilderPrototypeLootItem(string.Empty, (BuilderPrototypeLootRarity)999, -5, 99);

            Assert.That(item.ItemId, Is.EqualTo("prototype-loot"));
            Assert.That(item.Rarity, Is.EqualTo(BuilderPrototypeLootRarity.Legendary));
            Assert.That(item.ItemLevel, Is.EqualTo(1));
            Assert.That(item.AffixSlotCount, Is.EqualTo(4));
            Assert.That(item.PowerBudget, Is.EqualTo(BuilderPrototypeLootItem.CalculatePowerBudget(BuilderPrototypeLootRarity.Legendary, 1, 4)));
        }

        [Test]
        public void CollectLoot_AddsCarriedTotalsWithoutBankingAndRaisesPressure()
        {
            var model = new BuilderPrototypeLootPressureModel();
            var item = new BuilderPrototypeLootItem("rare-relic", BuilderPrototypeLootRarity.Rare, 12, 2);

            model.CollectLoot(item);

            Assert.IsTrue(model.HasCarriedLoot);
            Assert.That(model.CarriedItemCount, Is.EqualTo(1));
            Assert.That(model.CarriedLootValue, Is.EqualTo(item.ExtractionValue));
            Assert.That(model.CarriedPowerBudget, Is.EqualTo(item.PowerBudget));
            Assert.That(model.CarriedRiskWeight, Is.EqualTo(item.CarryRiskWeight));
            Assert.That(model.BankedLootValue, Is.EqualTo(0));
            Assert.That(model.PressureScore, Is.EqualTo(item.PickupPressure));
            Assert.That(model.ExtractionRiskPercent, Is.GreaterThan(0));
        }

        [Test]
        public void AdvancePressure_EscalatesFasterWhileLootIsCarriedAndCapsAtCritical()
        {
            var emptyRun = new BuilderPrototypeLootPressureModel();
            emptyRun.AdvancePressure(10);

            var loadedRun = new BuilderPrototypeLootPressureModel();
            var item = new BuilderPrototypeLootItem("loaded-cache", BuilderPrototypeLootRarity.Rare, 20, 2);
            loadedRun.CollectLoot(item);
            var pressureAfterPickup = loadedRun.PressureScore;
            loadedRun.AdvancePressure(10);

            Assert.That(emptyRun.PressureScore, Is.EqualTo(10));
            Assert.That(loadedRun.PressureScore, Is.GreaterThan(pressureAfterPickup + 10));

            loadedRun.AdvancePressure(999, combatNoise: 99);
            Assert.That(loadedRun.PressureScore, Is.EqualTo(BuilderPrototypeLootPressureModel.MaxPressureScore));
            Assert.That(loadedRun.PressureBand, Is.EqualTo(BuilderPrototypePressureBand.Critical));
            Assert.That(loadedRun.ExtractionRiskPercent, Is.GreaterThanOrEqualTo(80));
            Assert.That(loadedRun.ExtractionRiskPercent, Is.LessThanOrEqualTo(BuilderPrototypeLootPressureModel.MaxExtractionRiskPercent));
        }

        [Test]
        public void SuccessfulExtraction_MovesCarriedLootToBankAndResetsRunPressure()
        {
            var model = new BuilderPrototypeLootPressureModel();
            var first = new BuilderPrototypeLootItem("cache-a", BuilderPrototypeLootRarity.Uncommon, 8, 1);
            var second = new BuilderPrototypeLootItem("cache-b", BuilderPrototypeLootRarity.Epic, 10, 3);
            model.CollectLoot(first);
            model.CollectLoot(second);
            model.AdvancePressure(6, combatNoise: 1);
            var carriedValue = model.CarriedLootValue;
            var carriedPower = model.CarriedPowerBudget;
            var risk = model.ExtractionRiskPercent;

            var result = model.AttemptExtraction(100);

            Assert.IsTrue(result.Succeeded);
            Assert.That(result.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Extracted));
            Assert.That(result.RiskPercent, Is.EqualTo(risk));
            Assert.That(result.BankedValue, Is.EqualTo(carriedValue));
            Assert.That(result.BankedItemCount, Is.EqualTo(2));
            Assert.That(result.LostValue, Is.EqualTo(0));
            Assert.That(model.BankedLootValue, Is.EqualTo(carriedValue));
            Assert.That(model.BankedPowerBudget, Is.EqualTo(carriedPower));
            Assert.That(model.CarriedLootValue, Is.EqualTo(0));
            Assert.That(model.PressureScore, Is.EqualTo(0));
            Assert.IsFalse(model.HasCarriedLoot);
        }

        [Test]
        public void ExtractionRiskReduction_LowersAdjustedRiskAndAttemptUsesAdjustedRisk()
        {
            var item = new BuilderPrototypeLootItem("guarded-cache", BuilderPrototypeLootRarity.Rare, 12, 2);
            var unguarded = new BuilderPrototypeLootPressureModel();
            unguarded.CollectLoot(item);
            var rawRisk = unguarded.ExtractionRiskPercent;
            var guardedRisk = rawRisk - 2;

            Assert.That(rawRisk, Is.GreaterThan(2));
            Assert.That(unguarded.CalculateAdjustedExtractionRiskPercent(0), Is.EqualTo(rawRisk));
            Assert.That(unguarded.CalculateAdjustedExtractionRiskPercent(-5), Is.EqualTo(rawRisk));
            Assert.That(unguarded.CalculateAdjustedExtractionRiskPercent(2), Is.EqualTo(guardedRisk));
            Assert.That(unguarded.CalculateAdjustedExtractionRiskPercent(999), Is.EqualTo(0));

            var unguardedResult = unguarded.AttemptExtraction(rawRisk - 1);

            Assert.That(unguardedResult.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Lost));
            Assert.That(unguardedResult.RawRiskPercent, Is.EqualTo(rawRisk));
            Assert.That(unguardedResult.AdjustedRiskPercent, Is.EqualTo(rawRisk));
            Assert.That(unguardedResult.RiskPercent, Is.EqualTo(rawRisk));
            Assert.That(unguardedResult.RiskReductionPercent, Is.EqualTo(0));

            var guarded = new BuilderPrototypeLootPressureModel();
            guarded.CollectLoot(item);

            var guardedResult = guarded.AttemptExtraction(rawRisk - 1, 2);

            Assert.That(guardedResult.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Extracted));
            Assert.That(guardedResult.RawRiskPercent, Is.EqualTo(rawRisk));
            Assert.That(guardedResult.AdjustedRiskPercent, Is.EqualTo(guardedRisk));
            Assert.That(guardedResult.RiskPercent, Is.EqualTo(guardedRisk));
            Assert.That(guardedResult.RiskReductionPercent, Is.EqualTo(2));
        }

        [Test]
        public void PreviewExtraction_ReportsAdjustedOutcomeWithoutMutatingRun()
        {
            var model = new BuilderPrototypeLootPressureModel();
            var item = new BuilderPrototypeLootItem("preview-cache", BuilderPrototypeLootRarity.Rare, 12, 2);
            model.CollectLoot(item);
            model.AdvancePressure(6, combatNoise: 1);
            var carriedItemCount = model.CarriedItemCount;
            var carriedValue = model.CarriedLootValue;
            var carriedPower = model.CarriedPowerBudget;
            var carriedRiskWeight = model.CarriedRiskWeight;
            var pressure = model.PressureScore;
            var rawRisk = model.ExtractionRiskPercent;
            var adjustedRisk = model.CalculateAdjustedExtractionRiskPercent(2);

            var extractPreview = model.PreviewExtraction(adjustedRisk, 2);
            var losePreview = model.PreviewExtraction(adjustedRisk - 1, 2);

            Assert.That(extractPreview.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Extracted));
            Assert.That(extractPreview.RawRiskPercent, Is.EqualTo(rawRisk));
            Assert.That(extractPreview.RiskPercent, Is.EqualTo(adjustedRisk));
            Assert.That(extractPreview.RiskReductionPercent, Is.EqualTo(2));
            Assert.That(extractPreview.BankedValue, Is.EqualTo(carriedValue));
            Assert.That(extractPreview.BankedItemCount, Is.EqualTo(carriedItemCount));
            Assert.That(losePreview.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Lost));
            Assert.That(losePreview.RiskPercent, Is.EqualTo(adjustedRisk));
            Assert.That(losePreview.LostValue, Is.EqualTo(carriedValue));
            Assert.That(losePreview.LostItemCount, Is.EqualTo(carriedItemCount));
            Assert.That(model.CarriedItemCount, Is.EqualTo(carriedItemCount));
            Assert.That(model.CarriedLootValue, Is.EqualTo(carriedValue));
            Assert.That(model.CarriedPowerBudget, Is.EqualTo(carriedPower));
            Assert.That(model.CarriedRiskWeight, Is.EqualTo(carriedRiskWeight));
            Assert.That(model.PressureScore, Is.EqualTo(pressure));
            Assert.That(model.BankedLootValue, Is.EqualTo(0));
            Assert.That(model.BankedItemCount, Is.EqualTo(0));
            Assert.IsTrue(model.HasCarriedLoot);
        }

        [Test]
        public void FailedExtraction_LosesCarriedLootWithoutChangingExistingBank()
        {
            var model = new BuilderPrototypeLootPressureModel();
            model.CollectLoot(new BuilderPrototypeLootItem("banked-cache", BuilderPrototypeLootRarity.Common, 5, 0));
            var banked = model.AttemptExtraction(100).BankedValue;

            model.CollectLoot(new BuilderPrototypeLootItem("lost-cache", BuilderPrototypeLootRarity.Legendary, 20, 4));
            model.RaisePressure(BuilderPrototypeLootPressureModel.MaxPressureScore);
            var carriedValue = model.CarriedLootValue;
            var carriedItemCount = model.CarriedItemCount;

            var result = model.AttemptExtraction(0);

            Assert.That(result.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.Lost));
            Assert.That(result.RiskPercent, Is.EqualTo(BuilderPrototypeLootPressureModel.MaxExtractionRiskPercent));
            Assert.That(result.LostValue, Is.EqualTo(carriedValue));
            Assert.That(result.LostItemCount, Is.EqualTo(carriedItemCount));
            Assert.That(result.BankedValue, Is.EqualTo(0));
            Assert.That(model.BankedLootValue, Is.EqualTo(banked));
            Assert.That(model.CarriedLootValue, Is.EqualTo(0));
            Assert.That(model.PressureScore, Is.EqualTo(0));
        }

        [Test]
        public void ExtractionWithoutCarriedLoot_IsNoOp()
        {
            var model = new BuilderPrototypeLootPressureModel();

            var result = model.AttemptExtraction(50);

            Assert.That(result.Outcome, Is.EqualTo(BuilderPrototypeExtractionOutcome.NoCarriedLoot));
            Assert.That(result.RiskPercent, Is.EqualTo(0));
            Assert.That(result.BankedValue, Is.EqualTo(0));
            Assert.That(result.LostValue, Is.EqualTo(0));
            Assert.That(model.BankedLootValue, Is.EqualTo(0));
            Assert.That(model.PressureScore, Is.EqualTo(0));
        }
    }
}
