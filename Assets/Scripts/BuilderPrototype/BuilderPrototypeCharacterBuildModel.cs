using System;
using System.Collections.Generic;

namespace FourfoldEchoes.BuilderPrototype
{
    public enum BuilderPrototypeBuildStatId
    {
        BuilderPower,
        BuildSpeed,
        BreakerPower,
        SentinelGuard,
        StrikerDamage,
        Vitality
    }

    public enum BuilderPrototypeStatModifierKind
    {
        Flat,
        AdditivePercent,
        MoreMultiplier
    }

    public enum BuilderPrototypeBuildSourceKind
    {
        Gear,
        Passive
    }

    public enum BuilderPrototypeBuildRoleTag
    {
        Builder,
        Breaker,
        Sentinel,
        Striker
    }

    public readonly struct BuilderPrototypeBuildSource
    {
        public BuilderPrototypeBuildSource(string label, BuilderPrototypeBuildSourceKind kind, int affixBudget = 0)
        {
            Label = NormalizeLabel(label);
            Kind = kind;
            AffixBudget = affixBudget;
        }

        public string Label { get; }
        public BuilderPrototypeBuildSourceKind Kind { get; }
        public int AffixBudget { get; }

        internal static string NormalizeLabel(string label)
        {
            return (label ?? string.Empty).Trim();
        }
    }

    public readonly struct BuilderPrototypeStatModifier
    {
        public BuilderPrototypeStatModifier(
            BuilderPrototypeBuildStatId statId,
            BuilderPrototypeStatModifierKind kind,
            double value,
            string sourceLabel,
            int affixCost = 0)
        {
            StatId = statId;
            Kind = kind;
            Value = value;
            SourceLabel = BuilderPrototypeBuildSource.NormalizeLabel(sourceLabel);
            AffixCost = affixCost;
        }

        public BuilderPrototypeBuildStatId StatId { get; }
        public BuilderPrototypeStatModifierKind Kind { get; }
        public double Value { get; }
        public string SourceLabel { get; }
        public int AffixCost { get; }
    }

    public readonly struct BuilderPrototypeBuildStatValue
    {
        public BuilderPrototypeBuildStatValue(BuilderPrototypeBuildStatId statId, double value)
        {
            StatId = statId;
            Value = value;
        }

        public BuilderPrototypeBuildStatId StatId { get; }
        public double Value { get; }
    }

    public readonly struct BuilderPrototypeAffixBudgetUse
    {
        public BuilderPrototypeAffixBudgetUse(
            string sourceLabel,
            BuilderPrototypeBuildSourceKind sourceKind,
            int budget,
            int used)
        {
            SourceLabel = sourceLabel;
            SourceKind = sourceKind;
            Budget = budget;
            Used = used;
        }

        public string SourceLabel { get; }
        public BuilderPrototypeBuildSourceKind SourceKind { get; }
        public int Budget { get; }
        public int Used { get; }
        public int Remaining => Math.Max(0, Budget - Used);
        public bool IsOverBudget => Used > Budget;
    }

    public sealed class BuilderPrototypeCharacterBuildValidation
    {
        internal BuilderPrototypeCharacterBuildValidation(IReadOnlyList<string> errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
        public bool IsValid => Errors.Count == 0;
    }

    public sealed class BuilderPrototypeCharacterBuildSnapshot
    {
        private readonly Dictionary<BuilderPrototypeBuildStatId, double> statsById;

        internal BuilderPrototypeCharacterBuildSnapshot(
            IReadOnlyList<BuilderPrototypeBuildStatValue> stats,
            IReadOnlyList<BuilderPrototypeBuildRoleTag> roleTags,
            IReadOnlyList<BuilderPrototypeAffixBudgetUse> affixBudgets)
        {
            Stats = stats;
            RoleTags = roleTags;
            AffixBudgets = affixBudgets;

            statsById = new Dictionary<BuilderPrototypeBuildStatId, double>();
            for (var index = 0; index < stats.Count; index++)
            {
                statsById[stats[index].StatId] = stats[index].Value;
            }
        }

        public IReadOnlyList<BuilderPrototypeBuildStatValue> Stats { get; }
        public IReadOnlyList<BuilderPrototypeBuildRoleTag> RoleTags { get; }
        public IReadOnlyList<BuilderPrototypeAffixBudgetUse> AffixBudgets { get; }

        public double GetStat(BuilderPrototypeBuildStatId statId)
        {
            return statsById.TryGetValue(statId, out var value) ? value : 0d;
        }

        public bool HasRole(BuilderPrototypeBuildRoleTag roleTag)
        {
            for (var index = 0; index < RoleTags.Count; index++)
            {
                if (RoleTags[index] == roleTag)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class BuilderPrototypeCharacterBuildSheet
    {
        private static readonly BuilderPrototypeBuildStatId[] StatOrder =
        {
            BuilderPrototypeBuildStatId.BuilderPower,
            BuilderPrototypeBuildStatId.BuildSpeed,
            BuilderPrototypeBuildStatId.BreakerPower,
            BuilderPrototypeBuildStatId.SentinelGuard,
            BuilderPrototypeBuildStatId.StrikerDamage,
            BuilderPrototypeBuildStatId.Vitality
        };

        private readonly List<BuilderPrototypeBuildSource> sources = new List<BuilderPrototypeBuildSource>();
        private readonly List<BuilderPrototypeStatModifier> modifiers = new List<BuilderPrototypeStatModifier>();

        public IReadOnlyList<BuilderPrototypeBuildSource> Sources => sources;
        public IReadOnlyList<BuilderPrototypeStatModifier> Modifiers => modifiers;

        public BuilderPrototypeCharacterBuildSheet AddSource(BuilderPrototypeBuildSource source)
        {
            sources.Add(source);
            return this;
        }

        public BuilderPrototypeCharacterBuildSheet AddModifier(BuilderPrototypeStatModifier modifier)
        {
            modifiers.Add(modifier);
            return this;
        }

        public BuilderPrototypeCharacterBuildSnapshot Evaluate()
        {
            var flat = new double[StatOrder.Length];
            var additive = new double[StatOrder.Length];
            var more = new double[StatOrder.Length];
            for (var index = 0; index < more.Length; index++)
            {
                more[index] = 1d;
            }

            for (var index = 0; index < modifiers.Count; index++)
            {
                var modifier = modifiers[index];
                var statIndex = IndexOfStat(modifier.StatId);
                if (statIndex < 0 || !IsFinite(modifier.Value))
                {
                    continue;
                }

                switch (modifier.Kind)
                {
                    case BuilderPrototypeStatModifierKind.Flat:
                        flat[statIndex] += modifier.Value;
                        break;
                    case BuilderPrototypeStatModifierKind.AdditivePercent:
                        additive[statIndex] += modifier.Value;
                        break;
                    case BuilderPrototypeStatModifierKind.MoreMultiplier:
                        more[statIndex] *= 1d + modifier.Value;
                        break;
                }
            }

            var statValues = new double[StatOrder.Length];
            var stats = new List<BuilderPrototypeBuildStatValue>(StatOrder.Length);
            for (var index = 0; index < StatOrder.Length; index++)
            {
                var value = (BaseValueFor(StatOrder[index]) + flat[index]) * (1d + additive[index]) * more[index];
                statValues[index] = NormalizeStat(value);
                stats.Add(new BuilderPrototypeBuildStatValue(StatOrder[index], statValues[index]));
            }

            return new BuilderPrototypeCharacterBuildSnapshot(
                stats.AsReadOnly(),
                BuildRoleTags(statValues),
                BuildAffixBudgetUses());
        }

        public BuilderPrototypeCharacterBuildValidation Validate()
        {
            var errors = new List<string>();
            var sourceLabels = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < sources.Count; index++)
            {
                var source = sources[index];
                if (string.IsNullOrWhiteSpace(source.Label))
                {
                    errors.Add($"Source {index} has an empty label.");
                }
                else if (!sourceLabels.Add(source.Label))
                {
                    errors.Add($"Source '{source.Label}' is duplicated.");
                }

                if (!IsKnownSourceKind(source.Kind))
                {
                    errors.Add($"Source '{source.Label}' has an unknown kind.");
                }

                if (source.AffixBudget < 0)
                {
                    errors.Add($"Source '{source.Label}' has a negative affix budget.");
                }
            }

            for (var index = 0; index < modifiers.Count; index++)
            {
                var modifier = modifiers[index];
                if (IndexOfStat(modifier.StatId) < 0)
                {
                    errors.Add($"Modifier {index} has an unknown stat id.");
                }

                if (!IsKnownModifierKind(modifier.Kind))
                {
                    errors.Add($"Modifier {index} has an unknown modifier kind.");
                }

                if (string.IsNullOrWhiteSpace(modifier.SourceLabel))
                {
                    errors.Add($"Modifier {index} has an empty source label.");
                }
                else if (!sourceLabels.Contains(modifier.SourceLabel))
                {
                    errors.Add($"Modifier {index} references unknown source '{modifier.SourceLabel}'.");
                }

                if (!IsFinite(modifier.Value))
                {
                    errors.Add($"Modifier {index} for {modifier.StatId} has a non-finite value.");
                }

                if (modifier.Kind == BuilderPrototypeStatModifierKind.AdditivePercent && modifier.Value <= -1d)
                {
                    errors.Add($"Modifier {index} additive percent must be greater than -100%.");
                }

                if (modifier.Kind == BuilderPrototypeStatModifierKind.MoreMultiplier && modifier.Value <= -1d)
                {
                    errors.Add($"Modifier {index} more multiplier must be greater than -100%.");
                }

                if (modifier.AffixCost < 0)
                {
                    errors.Add($"Modifier {index} has a negative affix cost.");
                }
            }

            var affixBudgets = BuildAffixBudgetUses();
            for (var index = 0; index < affixBudgets.Count; index++)
            {
                var budget = affixBudgets[index];
                if (budget.Budget >= 0 && budget.IsOverBudget)
                {
                    errors.Add($"Source '{budget.SourceLabel}' uses {budget.Used}/{budget.Budget} affix budget.");
                }
            }

            return new BuilderPrototypeCharacterBuildValidation(errors.AsReadOnly());
        }

        private IReadOnlyList<BuilderPrototypeAffixBudgetUse> BuildAffixBudgetUses()
        {
            var used = new int[sources.Count];
            for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
            {
                var modifier = modifiers[modifierIndex];
                if (modifier.AffixCost <= 0)
                {
                    continue;
                }

                var sourceIndex = IndexOfSource(modifier.SourceLabel);
                if (sourceIndex >= 0)
                {
                    used[sourceIndex] += modifier.AffixCost;
                }
            }

            var budgets = new List<BuilderPrototypeAffixBudgetUse>(sources.Count);
            for (var sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
            {
                var source = sources[sourceIndex];
                budgets.Add(new BuilderPrototypeAffixBudgetUse(
                    source.Label,
                    source.Kind,
                    source.AffixBudget,
                    used[sourceIndex]));
            }

            return budgets.AsReadOnly();
        }

        private int IndexOfSource(string sourceLabel)
        {
            for (var index = 0; index < sources.Count; index++)
            {
                if (string.Equals(sources[index].Label, sourceLabel, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static IReadOnlyList<BuilderPrototypeBuildRoleTag> BuildRoleTags(double[] statValues)
        {
            var roleTags = new List<BuilderPrototypeBuildRoleTag>(4);
            if (statValues[IndexOfStat(BuilderPrototypeBuildStatId.BuilderPower)] >= 18d ||
                statValues[IndexOfStat(BuilderPrototypeBuildStatId.BuildSpeed)] >= 1.2d)
            {
                roleTags.Add(BuilderPrototypeBuildRoleTag.Builder);
            }

            if (statValues[IndexOfStat(BuilderPrototypeBuildStatId.BreakerPower)] >= 18d)
            {
                roleTags.Add(BuilderPrototypeBuildRoleTag.Breaker);
            }

            if (statValues[IndexOfStat(BuilderPrototypeBuildStatId.SentinelGuard)] >= 18d ||
                statValues[IndexOfStat(BuilderPrototypeBuildStatId.Vitality)] >= 135d)
            {
                roleTags.Add(BuilderPrototypeBuildRoleTag.Sentinel);
            }

            if (statValues[IndexOfStat(BuilderPrototypeBuildStatId.StrikerDamage)] >= 18d)
            {
                roleTags.Add(BuilderPrototypeBuildRoleTag.Striker);
            }

            return roleTags.AsReadOnly();
        }

        private static int IndexOfStat(BuilderPrototypeBuildStatId statId)
        {
            for (var index = 0; index < StatOrder.Length; index++)
            {
                if (StatOrder[index] == statId)
                {
                    return index;
                }
            }

            return -1;
        }

        private static double BaseValueFor(BuilderPrototypeBuildStatId statId)
        {
            switch (statId)
            {
                case BuilderPrototypeBuildStatId.BuildSpeed:
                    return 1d;
                case BuilderPrototypeBuildStatId.Vitality:
                    return 100d;
                default:
                    return 10d;
            }
        }

        private static double NormalizeStat(double value)
        {
            if (!IsFinite(value) || value <= 0d)
            {
                return 0d;
            }

            return Math.Round(value, 4, MidpointRounding.AwayFromZero);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool IsKnownModifierKind(BuilderPrototypeStatModifierKind kind)
        {
            switch (kind)
            {
                case BuilderPrototypeStatModifierKind.Flat:
                case BuilderPrototypeStatModifierKind.AdditivePercent:
                case BuilderPrototypeStatModifierKind.MoreMultiplier:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsKnownSourceKind(BuilderPrototypeBuildSourceKind kind)
        {
            switch (kind)
            {
                case BuilderPrototypeBuildSourceKind.Gear:
                case BuilderPrototypeBuildSourceKind.Passive:
                    return true;
                default:
                    return false;
            }
        }
    }
}
