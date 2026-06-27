using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class BuildSettingsScopeTests
    {
        [Test]
        public void MVP_SCOPE_BuildSettingsContainOnlyCurrentSliceScenes()
        {
            var enabledScenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            CollectionAssert.Contains(enabledScenePaths, "Assets/Scenes/ProductionCombatSlice.unity");
            CollectionAssert.Contains(enabledScenePaths, "Assets/Scenes/D020VerticalSlice.unity");

            var retiredScenePresent = enabledScenePaths.Any(path =>
                path.IndexOf("AshenThresholdSpike", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("ProductReviewSandbox", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("GateA", StringComparison.OrdinalIgnoreCase) >= 0);

            Assert.That(retiredScenePresent, Is.False);
        }
    }
}
