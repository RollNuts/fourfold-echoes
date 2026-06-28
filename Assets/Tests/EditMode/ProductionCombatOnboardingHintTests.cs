using FourfoldEchoes.Product;
using NUnit.Framework;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ProductionCombatOnboardingHintTests
    {
        [Test]
        public void UI_OnboardingHint_WaitsUntilGameplayStarts()
        {
            Assert.That(
                ProductionCombatOnboardingHint.ShouldShowHint(
                    ProductionCombatRunState.Title,
                    hasPlayingStarted: false,
                    secondsSincePlayingStarted: 20f),
                Is.False);

            Assert.That(
                ProductionCombatOnboardingHint.ShouldShowHint(
                    ProductionCombatRunState.Playing,
                    hasPlayingStarted: false,
                    secondsSincePlayingStarted: 0f),
                Is.False);

            Assert.That(
                ProductionCombatOnboardingHint.ShouldShowHint(
                    ProductionCombatRunState.Playing,
                    hasPlayingStarted: true,
                    secondsSincePlayingStarted: 0f),
                Is.True);
        }

        [Test]
        public void UI_OnboardingHint_ResetsWhenEnteringRunFromNonPlayableStates()
        {
            Assert.That(
                ProductionCombatOnboardingHint.ShouldResetTimer(
                    ProductionCombatRunState.Title,
                    ProductionCombatRunState.Playing),
                Is.True);
            Assert.That(
                ProductionCombatOnboardingHint.ShouldResetTimer(
                    ProductionCombatRunState.PlayerDown,
                    ProductionCombatRunState.Playing),
                Is.True);
            Assert.That(
                ProductionCombatOnboardingHint.ShouldResetTimer(
                    ProductionCombatRunState.Completed,
                    ProductionCombatRunState.Playing),
                Is.True);
        }

        [Test]
        public void UI_OnboardingHint_DoesNotRestartDuringPauseResume()
        {
            Assert.That(
                ProductionCombatOnboardingHint.ShouldResetTimer(
                    ProductionCombatRunState.Playing,
                    ProductionCombatRunState.Playing),
                Is.False);
            Assert.That(
                ProductionCombatOnboardingHint.ShouldResetTimer(
                    ProductionCombatRunState.Playing,
                    ProductionCombatRunState.Paused),
                Is.False);
            Assert.That(
                ProductionCombatOnboardingHint.ShouldResetTimer(
                    ProductionCombatRunState.Paused,
                    ProductionCombatRunState.Playing),
                Is.False);
        }

        [Test]
        public void UI_OnboardingHint_FadesAfterGameplayStart()
        {
            Assert.That(ProductionCombatOnboardingHint.Opacity01(0f), Is.EqualTo(1f));
            Assert.That(ProductionCombatOnboardingHint.Opacity01(6.5f), Is.EqualTo(1f));
            Assert.That(ProductionCombatOnboardingHint.Opacity01(7.25f), Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(ProductionCombatOnboardingHint.Opacity01(8f), Is.EqualTo(0f));
            Assert.That(ProductionCombatOnboardingHint.Opacity01(20f), Is.EqualTo(0f));
        }

        [Test]
        public void UI_OnboardingHint_HidesOutsidePlayingAndAfterFade()
        {
            Assert.That(
                ProductionCombatOnboardingHint.ShouldShowHint(
                    ProductionCombatRunState.Paused,
                    hasPlayingStarted: true,
                    secondsSincePlayingStarted: 1f),
                Is.False);
            Assert.That(
                ProductionCombatOnboardingHint.ShouldShowHint(
                    ProductionCombatRunState.PlayerDown,
                    hasPlayingStarted: true,
                    secondsSincePlayingStarted: 1f),
                Is.False);
            Assert.That(
                ProductionCombatOnboardingHint.ShouldShowHint(
                    ProductionCombatRunState.Completed,
                    hasPlayingStarted: true,
                    secondsSincePlayingStarted: 1f),
                Is.False);
            Assert.That(
                ProductionCombatOnboardingHint.ShouldShowHint(
                    ProductionCombatRunState.Playing,
                    hasPlayingStarted: true,
                    secondsSincePlayingStarted: 8.01f),
                Is.False);
        }
    }
}
