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
        }

        [Test]
        public void UI_LowHealthWarning_UsesLowerCriticalThreshold()
        {
            Assert.That(ProductionCombatLowHealthWarning.IsCriticalHealth(0.181f), Is.False);
            Assert.That(ProductionCombatLowHealthWarning.IsCriticalHealth(0.18f), Is.True);
            Assert.That(ProductionCombatLowHealthWarning.IsCriticalHealth(0.01f), Is.True);
        }
    }
}
