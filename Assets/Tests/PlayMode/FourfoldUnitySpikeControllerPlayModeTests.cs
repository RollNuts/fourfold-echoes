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
            Assert.That(prompt, Does.Contain("Hold Altar Y/K"));
            Assert.That(prompt, Does.Contain("Claim Y/E"));
            Assert.That(prompt, Does.Contain("Phase LB/RB"));
            Assert.That(prompt, Does.Contain("Reset Start/R"));
        }

        [Test]
        public void DownedPromptText_NamesGamepadAndKeyboardReset()
        {
            Assert.That(FourfoldUnitySpikeController.DownedPromptText, Does.Contain("Start/R"));
        }

        [Test]
        public void CriticalHealthPromptText_NamesDodgeAndSpacingPriority()
        {
            var prompt = FourfoldUnitySpikeController.CriticalHealthPromptText;

            Assert.That(prompt, Does.Contain("Critical HP"));
            Assert.That(prompt, Does.Contain("dodge"));
            Assert.That(prompt, Does.Contain("create space"));
        }

        [Test]
        public void IsCriticalHealth_RequiresAliveHealthAtOrBelowThirtyPercent()
        {
            Assert.IsFalse(FourfoldUnitySpikeController.IsCriticalHealth(0f, 100f));
            Assert.IsFalse(FourfoldUnitySpikeController.IsCriticalHealth(31f, 100f));
            Assert.IsTrue(FourfoldUnitySpikeController.IsCriticalHealth(30f, 100f));
            Assert.IsTrue(FourfoldUnitySpikeController.IsCriticalHealth(1f, 100f));
        }
    }
}
