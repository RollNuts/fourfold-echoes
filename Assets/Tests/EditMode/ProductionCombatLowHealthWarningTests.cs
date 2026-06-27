using FourfoldEchoes.Product;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatLowHealthWarningTests
    {
        [Test]
        public void UI_LowHealthWarning_UsesInclusiveWarningThreshold()
        {
            Assert.That(ProductionCombatLowHealthWarning.IsWarningHealth(0.351f), Is.False);
            Assert.That(ProductionCombatLowHealthWarning.IsWarningHealth(0.35f), Is.True);
            Assert.That(ProductionCombatLowHealthWarning.IsWarningHealth(0.2f), Is.True);
            Assert.That(ProductionCombatLowHealthWarning.IsWarningHealth(0f), Is.False);
        }

        [Test]
        public void UI_LowHealthWarning_UsesLowerCriticalThreshold()
        {
            Assert.That(ProductionCombatLowHealthWarning.IsCriticalHealth(0.181f), Is.False);
            Assert.That(ProductionCombatLowHealthWarning.IsCriticalHealth(0.18f), Is.True);
            Assert.That(ProductionCombatLowHealthWarning.IsCriticalHealth(0.01f), Is.True);
            Assert.That(ProductionCombatLowHealthWarning.IsCriticalHealth(0f), Is.False);
        }

        [Test]
        public void UI_LowHealthWarning_ProvidesReadableWarningLabels()
        {
            Assert.That(ProductionCombatLowHealthWarning.HealthWarningLabel(0.5f), Is.Empty);
            Assert.That(ProductionCombatLowHealthWarning.HealthWarningLabel(0.3f), Is.EqualTo("Low health - create space"));
            Assert.That(ProductionCombatLowHealthWarning.HealthWarningLabel(0.18f), Is.EqualTo("Critical health - dodge now"));
            Assert.That(ProductionCombatLowHealthWarning.HealthWarningLabel(0f), Is.Empty);
        }
    }
}
