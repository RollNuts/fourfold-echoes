using FourfoldEchoes.Product;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatSliceUiTests
    {
        [Test]
        public void UI_TitleSaveLine_DescribesEmptySave()
        {
            Assert.That(
                ProductionCombatSliceUi.BuildTitleSaveLine(false, false, false, "Local save ready"),
                Is.EqualTo("No saved slice progress yet."));
            Assert.That(
                ProductionCombatSliceUi.BuildStartButtonText(false, false, false),
                Is.EqualTo("Start Game"));
        }

        [Test]
        public void UI_TitleSaveLine_DescribesShortcutProgress()
        {
            Assert.That(
                ProductionCombatSliceUi.BuildTitleSaveLine(true, false, false, "Progress restored"),
                Is.EqualTo("Saved shortcut is open. Continue toward the wardens and boss gate."));
            Assert.That(
                ProductionCombatSliceUi.BuildStartButtonText(true, false, false),
                Is.EqualTo("Continue Saved Slice"));
        }

        [Test]
        public void UI_TitleSaveLine_DescribesGateAndRewardProgress()
        {
            Assert.That(
                ProductionCombatSliceUi.BuildTitleSaveLine(true, true, false, "Progress restored"),
                Is.EqualTo("Saved boss gate is open. Continue from the reward route."));
            Assert.That(
                ProductionCombatSliceUi.BuildTitleSaveLine(true, true, true, "Progress restored"),
                Is.EqualTo("Saved reward claimed. Continue to review the completed slice."));
        }

        [Test]
        public void UI_TitleSaveLine_PrioritizesSaveFailure()
        {
            Assert.That(
                ProductionCombatSliceUi.BuildTitleSaveLine(true, true, true, "Save failed - progress kept"),
                Is.EqualTo("Local save is unavailable; progress will stay in memory for this run."));
        }
    }
}
