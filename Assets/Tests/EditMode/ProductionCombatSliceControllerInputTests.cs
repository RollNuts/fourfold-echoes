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
    }
}
