using FourfoldEchoes.StrategyLoop;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests
{
    public sealed class PixelStrategyBoardPreviewTests
    {
        [Test]
        public void StreamerReadableSample_ContainsCoreBoardReads()
        {
            var state = PixelStrategyBoardPreviewFactory.CreateStreamerReadableSample();

            Assert.That(state.Width, Is.EqualTo(6));
            Assert.That(state.Height, Is.EqualTo(5));
            Assert.That(state.Route.Count, Is.EqualTo(10));
            Assert.That(state.HeroCell, Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(state.GetCellKind(new Vector2Int(2, 1)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Route));
            Assert.That(state.GetCellKind(new Vector2Int(3, 2)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Lair));
            Assert.That(state.GetCellKind(new Vector2Int(4, 2)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Hazard));
            Assert.That(state.GetCellKind(new Vector2Int(1, 2)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Reward));
            Assert.That(state.GetCellKind(new Vector2Int(4, 1)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Extract));
        }

        [Test]
        public void StreamerReadableSample_ProducesExtractionTensionAfterOneLoop()
        {
            var state = PixelStrategyBoardPreviewFactory.CreateStreamerReadableSample();

            Assert.That(state.Run.Outcome, Is.EqualTo(PixelStrategyRunOutcome.Extracted));
            Assert.That(state.Run.Decision, Is.EqualTo(PixelStrategyRunDecision.Extract));
            Assert.That(state.Run.CompletedLoops, Is.EqualTo(1));
            Assert.That(state.Run.Loot, Is.GreaterThanOrEqualTo(6));
            Assert.That(state.Run.BagPressure, Is.GreaterThanOrEqualTo(6));
            Assert.That(state.Run.Threat, Is.GreaterThan(0));
            Assert.IsTrue(state.Run.ExtractReady);
        }
    }
}
