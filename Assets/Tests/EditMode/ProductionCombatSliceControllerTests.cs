using FourfoldEchoes.Product;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatSliceControllerTests
    {
        [Test]
        public void UI_DamageEventText_TellsPlayerHowToRecover()
        {
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0.8f),
                Is.EqualTo("Hit taken - dodge next tell"));
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0.3f),
                Is.EqualTo("Hit taken - create space"));
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0.18f),
                Is.EqualTo("Critical hit - dodge now"));
            Assert.That(
                ProductionCombatSliceController.BuildDamageEventText(0f),
                Is.EqualTo("Hero down - choose Retry"));
        }
    }
}
