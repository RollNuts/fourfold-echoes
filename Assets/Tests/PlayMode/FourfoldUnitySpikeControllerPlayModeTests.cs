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
    }
}
