using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeCharacterBuildModelTests
    {
        [Test]
        public void Evaluate_CombinesFlatAdditivePercentAndMoreMultiplierMath()
        {
            var sheet = new BuilderPrototypeCharacterBuildSheet()
                .AddSource(new BuilderPrototypeBuildSource("Forge Apron", BuilderPrototypeBuildSourceKind.Gear, affixBudget: 4))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuilderPower,
                    BuilderPrototypeStatModifierKind.Flat,
                    5d,
                    "Forge Apron",
                    affixCost: 1))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuilderPower,
                    BuilderPrototypeStatModifierKind.AdditivePercent,
                    0.2d,
                    "Forge Apron",
                    affixCost: 1))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuilderPower,
                    BuilderPrototypeStatModifierKind.MoreMultiplier,
                    0.5d,
                    "Forge Apron",
                    affixCost: 2));

            var snapshot = sheet.Evaluate();

            Assert.That(snapshot.GetStat(BuilderPrototypeBuildStatId.BuilderPower), Is.EqualTo(27d).Within(0.0001d));
            Assert.That(snapshot.AffixBudgets[0].Used, Is.EqualTo(4));
            Assert.That(snapshot.AffixBudgets[0].Remaining, Is.EqualTo(0));
            Assert.IsTrue(snapshot.HasRole(BuilderPrototypeBuildRoleTag.Builder));
        }

        [Test]
        public void Evaluate_DerivesRoleTagsInStableOrder()
        {
            var sheet = new BuilderPrototypeCharacterBuildSheet()
                .AddSource(new BuilderPrototypeBuildSource("Anvil Path", BuilderPrototypeBuildSourceKind.Passive))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuilderPower,
                    BuilderPrototypeStatModifierKind.Flat,
                    8d,
                    "Anvil Path"))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BreakerPower,
                    BuilderPrototypeStatModifierKind.Flat,
                    8d,
                    "Anvil Path"))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.SentinelGuard,
                    BuilderPrototypeStatModifierKind.Flat,
                    8d,
                    "Anvil Path"))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.StrikerDamage,
                    BuilderPrototypeStatModifierKind.Flat,
                    8d,
                    "Anvil Path"));

            var snapshot = sheet.Evaluate();

            Assert.That(snapshot.RoleTags, Is.EqualTo(new[]
            {
                BuilderPrototypeBuildRoleTag.Builder,
                BuilderPrototypeBuildRoleTag.Breaker,
                BuilderPrototypeBuildRoleTag.Sentinel,
                BuilderPrototypeBuildRoleTag.Striker
            }));
        }

        [Test]
        public void Validate_AcceptsGearAndPassiveSourcesInsideAffixBudget()
        {
            var sheet = new BuilderPrototypeCharacterBuildSheet()
                .AddSource(new BuilderPrototypeBuildSource("Smith Gloves", BuilderPrototypeBuildSourceKind.Gear, affixBudget: 3))
                .AddSource(new BuilderPrototypeBuildSource("Bulwark Path", BuilderPrototypeBuildSourceKind.Passive))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuildSpeed,
                    BuilderPrototypeStatModifierKind.AdditivePercent,
                    0.15d,
                    "Smith Gloves",
                    affixCost: 1))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.SentinelGuard,
                    BuilderPrototypeStatModifierKind.Flat,
                    6d,
                    "Smith Gloves",
                    affixCost: 2))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.Vitality,
                    BuilderPrototypeStatModifierKind.Flat,
                    25d,
                    "Bulwark Path"));

            var validation = sheet.Validate();

            Assert.IsTrue(validation.IsValid);
            Assert.That(validation.Errors, Is.Empty);
        }

        [Test]
        public void Validate_ReportsErrorsInDeterministicOrder()
        {
            var sheet = new BuilderPrototypeCharacterBuildSheet()
                .AddSource(new BuilderPrototypeBuildSource("Cracked Helm", BuilderPrototypeBuildSourceKind.Gear, affixBudget: 3))
                .AddSource(new BuilderPrototypeBuildSource("Cracked Helm", BuilderPrototypeBuildSourceKind.Passive))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.StrikerDamage,
                    BuilderPrototypeStatModifierKind.Flat,
                    2d,
                    "Cracked Helm",
                    affixCost: 4))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BuilderPower,
                    BuilderPrototypeStatModifierKind.Flat,
                    1d,
                    "Ghost Ring"))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.BreakerPower,
                    BuilderPrototypeStatModifierKind.Flat,
                    double.NaN,
                    "Cracked Helm"))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.SentinelGuard,
                    BuilderPrototypeStatModifierKind.MoreMultiplier,
                    -1d,
                    "Cracked Helm"))
                .AddModifier(new BuilderPrototypeStatModifier(
                    BuilderPrototypeBuildStatId.Vitality,
                    BuilderPrototypeStatModifierKind.AdditivePercent,
                    0.1d,
                    "Cracked Helm",
                    affixCost: -1));

            var validation = sheet.Validate();

            Assert.IsFalse(validation.IsValid);
            Assert.That(validation.Errors, Is.EqualTo(new[]
            {
                "Source 'Cracked Helm' is duplicated.",
                "Modifier 1 references unknown source 'Ghost Ring'.",
                "Modifier 2 for BreakerPower has a non-finite value.",
                "Modifier 3 more multiplier must be greater than -100%.",
                "Modifier 4 has a negative affix cost.",
                "Source 'Cracked Helm' uses 4/3 affix budget."
            }));
        }
    }
}
