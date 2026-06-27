using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatSliceControllerInputTests
    {
        [Test]
        public void UI_ProductionCombatRetry_AcceptsKeyboardAndGamepadMenu()
        {
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.R), Is.True);
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.JoystickButton7), Is.True);
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.JoystickButton0), Is.False);
            Assert.That(ProductionCombatSliceController.IsRetryKey(KeyCode.Escape), Is.False);
        }

        [Test]
        public void UI_ProductionCombatClaimReward_AcceptsKeyboardAndGamepadNorthButton()
        {
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.E), Is.True);
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.JoystickButton3), Is.True);
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.JoystickButton0), Is.False);
            Assert.That(ProductionCombatSliceController.IsClaimRewardKey(KeyCode.R), Is.False);
        }
    }
}
