using FourfoldEchoes.Spike;
using NUnit.Framework;

namespace FourfoldEchoes.Tests
{
    public sealed class FourfoldUnitySpikeControllerPlayModeTests
    {
        [Test]
        public void ControlPromptText_NamesConcreteControllerAndKeyboardInputs()
        {
            var prompt = FourfoldUnitySpikeController.ControlPromptText;

            Assert.That(prompt, Does.Contain("Move LS"));
            Assert.That(prompt, Does.Contain("Attack A/X"));
            Assert.That(prompt, Does.Contain("Dodge B"));
            Assert.That(prompt, Does.Contain("Altar/Claim Y/E"));
            Assert.That(prompt, Does.Contain("Phase LB/RB"));
            Assert.That(prompt, Does.Contain("Reset Start/R"));
        }

        [Test]
        public void CriticalHealthWarningText_NamesImmediateDefensiveAction()
        {
            var warning = FourfoldUnitySpikeController.CriticalHealthWarningText;

            Assert.That(warning, Does.Contain("Critical HP"));
            Assert.That(warning, Does.Contain("dodge"));
            Assert.That(warning, Does.Contain("create space"));
        }
    }
}
