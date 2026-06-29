using System;

namespace FourfoldEchoes.BuilderPrototype
{
    public sealed class BuilderPrototypeRunState
    {
        public const int MaxDangerTier = 5;

        public BuilderPrototypeMode Mode { get; private set; } = BuilderPrototypeMode.Traverse;
        public int CarriedLootValue { get; private set; }
        public int DangerTier { get; private set; }
        public int BankedLootValue { get; private set; }

        public bool IsCarryingLoot => CarriedLootValue > 0;
        public bool IsInHookMode => Mode != BuilderPrototypeMode.Traverse;

        public void SetMode(BuilderPrototypeMode mode)
        {
            Mode = mode;
        }

        public void AddCarriedLoot(int value)
        {
            if (value <= 0)
            {
                return;
            }

            CarriedLootValue = ClampNonNegative(CarriedLootValue + value);
        }

        public void SetCarriedLootForPrototype(int value)
        {
            CarriedLootValue = ClampNonNegative(value);
        }

        public void RaiseDanger(int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            DangerTier = Math.Min(MaxDangerTier, DangerTier + amount);
        }

        public void SetDangerForPrototype(int tier)
        {
            DangerTier = Math.Min(MaxDangerTier, ClampNonNegative(tier));
        }

        public int BankAndResetRun()
        {
            var banked = CarriedLootValue;
            BankedLootValue = ClampNonNegative(BankedLootValue + banked);
            ResetRun();
            return banked;
        }

        public void ResetRun()
        {
            Mode = BuilderPrototypeMode.Traverse;
            CarriedLootValue = 0;
            DangerTier = 0;
        }

        public static string LabelFor(BuilderPrototypeMode mode)
        {
            switch (mode)
            {
                case BuilderPrototypeMode.BuildHook:
                    return "Build Hook";
                case BuilderPrototypeMode.CombatHook:
                    return "Combat Hook";
                case BuilderPrototypeMode.LootHook:
                    return "Loot Hook";
                case BuilderPrototypeMode.ExtractHook:
                    return "Extract Hook";
                default:
                    return "Traverse";
            }
        }

        private static int ClampNonNegative(int value)
        {
            return Math.Max(0, value);
        }
    }
}
