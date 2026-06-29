using System;

namespace FourfoldEchoes.BuilderPrototype
{
    public enum BuilderPrototypeLootRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum BuilderPrototypePressureBand
    {
        Quiet,
        Alert,
        Hunted,
        Critical
    }

    public enum BuilderPrototypeExtractionOutcome
    {
        NoCarriedLoot,
        Extracted,
        Lost
    }

    public sealed class BuilderPrototypeLootItem
    {
        public const int MaxPrototypeItemLevel = 100;

        public BuilderPrototypeLootItem(string itemId, BuilderPrototypeLootRarity rarity, int itemLevel, int requestedAffixSlots)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? "prototype-loot" : itemId;
            Rarity = NormalizeRarity(rarity);
            ItemLevel = Clamp(itemLevel, 1, MaxPrototypeItemLevel);
            AffixSlotCount = Clamp(requestedAffixSlots, 0, MaxAffixSlotsFor(Rarity));
            PowerBudget = CalculatePowerBudget(Rarity, ItemLevel, AffixSlotCount);
            ExtractionValue = CalculateExtractionValue(Rarity, PowerBudget);
            CarryRiskWeight = CalculateCarryRiskWeight(Rarity, AffixSlotCount, PowerBudget);
            PickupPressure = CalculatePickupPressure(Rarity, AffixSlotCount, PowerBudget);
        }

        public string ItemId { get; }
        public BuilderPrototypeLootRarity Rarity { get; }
        public int ItemLevel { get; }
        public int AffixSlotCount { get; }
        public int PowerBudget { get; }
        public int ExtractionValue { get; }
        public int CarryRiskWeight { get; }
        public int PickupPressure { get; }

        public static int MaxAffixSlotsFor(BuilderPrototypeLootRarity rarity)
        {
            switch (NormalizeRarity(rarity))
            {
                case BuilderPrototypeLootRarity.Uncommon:
                    return 1;
                case BuilderPrototypeLootRarity.Rare:
                    return 2;
                case BuilderPrototypeLootRarity.Epic:
                    return 3;
                case BuilderPrototypeLootRarity.Legendary:
                    return 4;
                default:
                    return 0;
            }
        }

        public static int CalculatePowerBudget(BuilderPrototypeLootRarity rarity, int itemLevel, int affixSlotCount)
        {
            var normalizedRarity = NormalizeRarity(rarity);
            var tier = (int)normalizedRarity;
            var level = Clamp(itemLevel, 1, MaxPrototypeItemLevel);
            var slots = Clamp(affixSlotCount, 0, MaxAffixSlotsFor(normalizedRarity));

            return 10 + (level * 4) + (tier * 12) + (slots * (5 + (tier * 2)));
        }

        private static int CalculateExtractionValue(BuilderPrototypeLootRarity rarity, int powerBudget)
        {
            return Math.Max(1, powerBudget * ValueMultiplierFor(rarity) / 2);
        }

        private static int CalculateCarryRiskWeight(BuilderPrototypeLootRarity rarity, int affixSlotCount, int powerBudget)
        {
            return 1 + ((int)NormalizeRarity(rarity) * 2) + affixSlotCount + (powerBudget / 30);
        }

        private static int CalculatePickupPressure(BuilderPrototypeLootRarity rarity, int affixSlotCount, int powerBudget)
        {
            return 2 + ((int)NormalizeRarity(rarity) * 3) + (affixSlotCount * 2) + (powerBudget / 35);
        }

        private static int ValueMultiplierFor(BuilderPrototypeLootRarity rarity)
        {
            switch (NormalizeRarity(rarity))
            {
                case BuilderPrototypeLootRarity.Uncommon:
                    return 3;
                case BuilderPrototypeLootRarity.Rare:
                    return 5;
                case BuilderPrototypeLootRarity.Epic:
                    return 8;
                case BuilderPrototypeLootRarity.Legendary:
                    return 13;
                default:
                    return 2;
            }
        }

        private static BuilderPrototypeLootRarity NormalizeRarity(BuilderPrototypeLootRarity rarity)
        {
            return (BuilderPrototypeLootRarity)Clamp((int)rarity, (int)BuilderPrototypeLootRarity.Common, (int)BuilderPrototypeLootRarity.Legendary);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }

    public sealed class BuilderPrototypeExtractionResult
    {
        internal BuilderPrototypeExtractionResult(
            BuilderPrototypeExtractionOutcome outcome,
            int rawRiskPercent,
            int adjustedRiskPercent,
            int riskReductionPercent,
            int safetyRollPercent,
            int bankedValue,
            int lostValue,
            int bankedItemCount,
            int lostItemCount)
        {
            Outcome = outcome;
            RawRiskPercent = rawRiskPercent;
            AdjustedRiskPercent = adjustedRiskPercent;
            RiskReductionPercent = riskReductionPercent;
            RiskPercent = adjustedRiskPercent;
            SafetyRollPercent = safetyRollPercent;
            BankedValue = bankedValue;
            LostValue = lostValue;
            BankedItemCount = bankedItemCount;
            LostItemCount = lostItemCount;
        }

        public BuilderPrototypeExtractionOutcome Outcome { get; }
        public int RawRiskPercent { get; }
        public int AdjustedRiskPercent { get; }
        public int RiskReductionPercent { get; }
        public int RiskPercent { get; }
        public int SafetyRollPercent { get; }
        public int BankedValue { get; }
        public int LostValue { get; }
        public int BankedItemCount { get; }
        public int LostItemCount { get; }
        public bool Succeeded => Outcome == BuilderPrototypeExtractionOutcome.Extracted;
    }

    public sealed class BuilderPrototypeLootPressureModel
    {
        public const int MaxPressureScore = 100;
        public const int MaxExtractionRiskPercent = 95;

        public int CarriedItemCount { get; private set; }
        public int CarriedLootValue { get; private set; }
        public int CarriedPowerBudget { get; private set; }
        public int CarriedRiskWeight { get; private set; }
        public int BankedItemCount { get; private set; }
        public int BankedLootValue { get; private set; }
        public int BankedPowerBudget { get; private set; }
        public int PressureScore { get; private set; }

        public bool HasCarriedLoot => CarriedItemCount > 0;
        public BuilderPrototypePressureBand PressureBand => BandFor(PressureScore);
        public int ExtractionRiskPercent => CalculateExtractionRiskPercent();

        public void CollectLoot(BuilderPrototypeLootItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            CarriedItemCount = AddClamped(CarriedItemCount, 1);
            CarriedLootValue = AddClamped(CarriedLootValue, item.ExtractionValue);
            CarriedPowerBudget = AddClamped(CarriedPowerBudget, item.PowerBudget);
            CarriedRiskWeight = AddClamped(CarriedRiskWeight, item.CarryRiskWeight);
            RaisePressure(item.PickupPressure);
        }

        public void AdvancePressure(int routeTicks, int combatNoise = 0)
        {
            var safeTicks = Math.Max(0, routeTicks);
            var safeNoise = Math.Max(0, combatNoise);
            if (safeTicks == 0 && safeNoise == 0)
            {
                return;
            }

            var carriedPace = HasCarriedLoot ? 1 + (CarriedRiskWeight / 10) : 1;
            RaisePressure((safeTicks * carriedPace) + (safeNoise * 6));
        }

        public void RaisePressure(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PressureScore = Clamp(AddClamped(PressureScore, amount), 0, MaxPressureScore);
        }

        public int CalculateAdjustedExtractionRiskPercent(int riskReductionPercent)
        {
            return AdjustExtractionRiskPercent(ExtractionRiskPercent, riskReductionPercent);
        }

        public BuilderPrototypeExtractionResult AttemptExtraction(int safetyRollPercent, int riskReductionPercent = 0)
        {
            var preview = PreviewExtraction(safetyRollPercent, riskReductionPercent);
            var powerBudget = CarriedPowerBudget;

            if (preview.Outcome == BuilderPrototypeExtractionOutcome.NoCarriedLoot)
            {
                return preview;
            }

            if (preview.Succeeded)
            {
                BankedItemCount = AddClamped(BankedItemCount, preview.BankedItemCount);
                BankedLootValue = AddClamped(BankedLootValue, preview.BankedValue);
                BankedPowerBudget = AddClamped(BankedPowerBudget, powerBudget);
                ClearCarriedRun();
                return preview;
            }

            ClearCarriedRun();
            return preview;
        }

        public BuilderPrototypeExtractionResult PreviewExtraction(int safetyRollPercent, int riskReductionPercent = 0)
        {
            var roll = Clamp(safetyRollPercent, 0, 100);
            if (!HasCarriedLoot)
            {
                return new BuilderPrototypeExtractionResult(BuilderPrototypeExtractionOutcome.NoCarriedLoot, 0, 0, 0, roll, 0, 0, 0, 0);
            }

            var rawRisk = ExtractionRiskPercent;
            var reduction = NormalizeRiskReductionPercent(riskReductionPercent);
            var risk = AdjustExtractionRiskPercent(rawRisk, reduction);
            var itemCount = CarriedItemCount;
            var lootValue = CarriedLootValue;

            if (roll >= risk)
            {
                return new BuilderPrototypeExtractionResult(BuilderPrototypeExtractionOutcome.Extracted, rawRisk, risk, reduction, roll, lootValue, 0, itemCount, 0);
            }

            return new BuilderPrototypeExtractionResult(BuilderPrototypeExtractionOutcome.Lost, rawRisk, risk, reduction, roll, 0, lootValue, 0, itemCount);
        }

        public void LoseCarriedLootAndResetPressure()
        {
            ClearCarriedRun();
        }

        public static BuilderPrototypePressureBand BandFor(int pressureScore)
        {
            var score = Clamp(pressureScore, 0, MaxPressureScore);
            if (score >= 75)
            {
                return BuilderPrototypePressureBand.Critical;
            }

            if (score >= 50)
            {
                return BuilderPrototypePressureBand.Hunted;
            }

            if (score >= 25)
            {
                return BuilderPrototypePressureBand.Alert;
            }

            return BuilderPrototypePressureBand.Quiet;
        }

        private int CalculateExtractionRiskPercent()
        {
            if (!HasCarriedLoot)
            {
                return 0;
            }

            var risk = 8 + (PressureScore / 2) + (CarriedRiskWeight * 2) + (CarriedItemCount * 2);
            return Clamp(risk, 5, MaxExtractionRiskPercent);
        }

        private static int AdjustExtractionRiskPercent(int rawRiskPercent, int riskReductionPercent)
        {
            var risk = Clamp(rawRiskPercent, 0, MaxExtractionRiskPercent);
            return Clamp(risk - NormalizeRiskReductionPercent(riskReductionPercent), 0, MaxExtractionRiskPercent);
        }

        private static int NormalizeRiskReductionPercent(int riskReductionPercent)
        {
            return Clamp(riskReductionPercent, 0, MaxExtractionRiskPercent);
        }

        private void ClearCarriedRun()
        {
            CarriedItemCount = 0;
            CarriedLootValue = 0;
            CarriedPowerBudget = 0;
            CarriedRiskWeight = 0;
            PressureScore = 0;
        }

        private static int AddClamped(int current, int amount)
        {
            if (amount <= 0)
            {
                return current;
            }

            if (current >= int.MaxValue - amount)
            {
                return int.MaxValue;
            }

            return current + amount;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
