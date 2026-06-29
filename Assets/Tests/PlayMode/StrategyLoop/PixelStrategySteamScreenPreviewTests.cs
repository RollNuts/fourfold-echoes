using FourfoldEchoes.StrategyLoop;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests
{
    public sealed class PixelStrategySteamScreenPreviewTests
    {
        [Test]
        public void FirstSteamScreenSample_CentersAReadableLargeBoard()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            Assert.That(state.Board.Width, Is.EqualTo(14));
            Assert.That(state.Board.Height, Is.EqualTo(7));
            Assert.That(state.Board.Route.Count, Is.EqualTo(32));
            Assert.That(state.Board.HeroCell, Is.EqualTo(new Vector2Int(2, 4)));
            Assert.That(state.Board.ExtractCell, Is.EqualTo(new Vector2Int(12, 4)));
            Assert.That(state.Board.GetCellKind(new Vector2Int(4, 3)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Reward));
            Assert.That(state.Board.GetCellKind(new Vector2Int(6, 2)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Lair));
            Assert.That(state.Board.GetCellKind(new Vector2Int(9, 1)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Hazard));
            Assert.That(state.Board.GetCellKind(new Vector2Int(12, 4)), Is.EqualTo(PixelStrategyBoardPreviewCellKind.Extract));
        }

        [Test]
        public void FirstSteamScreenSample_HasThreeDecisionCardsWithSafeSelected()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            Assert.That(state.Cards.Count, Is.EqualTo(3));
            Assert.That(state.Cards[0].Choice, Is.EqualTo(PixelStrategySteamChoiceKind.BaitLair));
            Assert.That(state.Cards[0].Tone, Is.EqualTo(PixelStrategySteamScreenCardTone.Greedy));
            Assert.That(state.Cards[1].Choice, Is.EqualTo(PixelStrategySteamChoiceKind.CutToGate));
            Assert.That(state.Cards[1].Tone, Is.EqualTo(PixelStrategySteamScreenCardTone.SafeSelected));
            Assert.That(state.Cards[1].Selected, Is.True);
            Assert.That(state.Cards[2].Choice, Is.EqualTo(PixelStrategySteamChoiceKind.GreedRelic));
            Assert.That(state.Cards[2].Tone, Is.EqualTo(PixelStrategySteamScreenCardTone.Doom));
            Assert.That(state.Cards[0].Title, Does.Contain("CHOSEN"));
            Assert.That(state.Cards[0].FooterText, Does.Contain("wood stick"));
            Assert.That(state.Cards[2].RiskText, Is.EqualTo("DOOM"));
        }

        [Test]
        public void FirstSteamScreenSample_ConnectsSelectedChoiceToBoardImpact()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            Assert.That(state.Impact.SelectedChoice, Is.EqualTo(PixelStrategySteamChoiceKind.CutToGate));
            Assert.That(state.Impact.OpenedGateCells.Count, Is.EqualTo(5));
            Assert.That(state.Impact.OpenedGateCells[0], Is.EqualTo(new Vector2Int(8, 3)));
            Assert.That(state.Impact.OpenedGateCells[4], Is.EqualTo(state.Board.ExtractCell));
            Assert.That(state.Impact.SealedPressureCells.Count, Is.EqualTo(2));
            Assert.That(state.Impact.SealedPressureCells[0], Is.EqualTo(new Vector2Int(9, 1)));
            Assert.That(state.Impact.BoardCallout, Is.EqualTo("GATE CUT OPEN"));
        }

        [Test]
        public void FirstSteamScreenSample_CarriesFourfoldIdentityRead()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            Assert.That(state.Identity.CornerSigils, Is.EqualTo(new[] { "BLADE", "GATE", "RELIC", "SEAL" }));
            Assert.That(state.Identity.EchoCells, Is.EqualTo(state.Impact.OpenedGateCells));
            Assert.That(state.Identity.DangerRingCells.Count, Is.EqualTo(4));
            Assert.That(state.Identity.DangerRingCells, Does.Contain(new Vector2Int(12, 5)));
            Assert.That(state.Identity.CarriedLoot, Is.EqualTo(new[] { "COIN", "KEY", "SHARD", "SEAL" }));
            Assert.That(state.Identity.LitSealBeatCount, Is.EqualTo(1));
            Assert.That(state.Identity.CrackedSealBeatCount, Is.EqualTo(2));
        }

        [Test]
        public void FirstSteamScreenSample_OffersOneStepChoiceDeltas()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            var bait = state.ChoicePreview.GetDelta(PixelStrategySteamChoiceKind.BaitLair);
            Assert.That(bait.Loot, Is.EqualTo(1));
            Assert.That(bait.Threat, Is.EqualTo(2));
            Assert.That(bait.Gate, Is.EqualTo(0));
            Assert.That(bait.Pressure, Is.EqualTo(1));
            Assert.That(bait.Extract, Is.EqualTo(1));

            var cut = state.ChoicePreview.GetDelta(PixelStrategySteamChoiceKind.CutToGate);
            Assert.That(cut.Loot, Is.EqualTo(0));
            Assert.That(cut.Threat, Is.EqualTo(-1));
            Assert.That(cut.Gate, Is.EqualTo(2));
            Assert.That(cut.Pressure, Is.EqualTo(2));
            Assert.That(cut.Extract, Is.EqualTo(1));

            var greed = state.ChoicePreview.GetDelta(PixelStrategySteamChoiceKind.GreedRelic);
            Assert.That(greed.Loot, Is.EqualTo(3));
            Assert.That(greed.Threat, Is.EqualTo(1));
            Assert.That(greed.Gate, Is.EqualTo(-1));
            Assert.That(greed.Pressure, Is.EqualTo(2));
            Assert.That(greed.Extract, Is.EqualTo(-1));
        }

        [Test]
        public void FirstSteamScreenSample_ForecastsChoiceTradeoffs()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            var cut = state.ChoicePreview.Apply(PixelStrategySteamChoiceKind.CutToGate);
            Assert.That(cut.Loot, Is.EqualTo(state.ChoicePreview.BaseLoot));
            Assert.That(cut.Threat, Is.LessThan(state.ChoicePreview.BaseThreat));
            Assert.That(cut.Gate, Is.EqualTo(state.ChoicePreview.ExtractGateTarget));
            Assert.That(cut.Extract, Is.EqualTo(state.ChoicePreview.ExtractScoreTarget));
            Assert.IsTrue(cut.ExtractReady);
            Assert.IsTrue(cut.PressureWillCrack);
            Assert.That(cut.PendingPressureEvent, Is.EqualTo(PixelStrategySteamPressureEventKind.SealGate));

            var greed = state.ChoicePreview.Apply(PixelStrategySteamChoiceKind.GreedRelic);
            Assert.That(greed.Loot, Is.GreaterThan(cut.Loot));
            Assert.That(greed.Gate, Is.LessThan(state.ChoicePreview.BaseGate));
            Assert.That(greed.Extract, Is.LessThan(state.ChoicePreview.BaseExtract));
            Assert.IsFalse(greed.ExtractReady);
            Assert.IsTrue(greed.PressureWillCrack);
            Assert.That(greed.PendingPressureEvent, Is.EqualTo(PixelStrategySteamPressureEventKind.ExtractionHeat));

            var bait = state.ChoicePreview.Apply(PixelStrategySteamChoiceKind.BaitLair);
            Assert.That(bait.Threat, Is.GreaterThan(state.ChoicePreview.BaseThreat));
            Assert.That(bait.Extract, Is.EqualTo(state.ChoicePreview.ExtractScoreTarget));
            Assert.IsFalse(bait.ExtractReady);
            Assert.IsFalse(bait.PressureWillCrack);
            Assert.IsNull(bait.PendingPressureEvent);
        }

        [Test]
        public void FirstSteamScreenSample_ResolvesPressureCrackIntoChosenIntervention()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            var crack = state.ChoicePreview.ResolveCrack(PixelStrategySteamChoiceKind.CutToGate);

            Assert.That(state.ChoicePreview.PressureCrackCost, Is.EqualTo(5));
            Assert.That(crack.RemainingPressure, Is.EqualTo(2));
            Assert.That(crack.CrackCount, Is.EqualTo(1));
            Assert.That(crack.ChosenLevel, Is.EqualTo(1));
            Assert.That(crack.QueuedEvent, Is.EqualTo(PixelStrategySteamPressureEventKind.SealGate));
        }

        [Test]
        public void FirstSteamScreenSample_AppliesPressureEventsToNextChoiceOnly()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            var cleanCut = state.ChoicePreview.Apply(PixelStrategySteamChoiceKind.CutToGate);
            var sealedCut = state.ChoicePreview.Apply(
                PixelStrategySteamChoiceKind.CutToGate,
                PixelStrategySteamPressureEventKind.SealGate,
                chosenLevel: 1);
            Assert.That(sealedCut.Gate, Is.EqualTo(cleanCut.Gate - 1));
            Assert.That(sealedCut.Extract, Is.EqualTo(cleanCut.Extract));

            var cleanGreed = state.ChoicePreview.Apply(PixelStrategySteamChoiceKind.GreedRelic);
            var heatedGreed = state.ChoicePreview.Apply(
                PixelStrategySteamChoiceKind.GreedRelic,
                PixelStrategySteamPressureEventKind.ExtractionHeat,
                chosenLevel: 1);
            Assert.That(heatedGreed.Extract, Is.EqualTo(cleanGreed.Extract - 2));
            Assert.That(heatedGreed.Loot, Is.EqualTo(cleanGreed.Loot));

            var cleanBait = state.ChoicePreview.Apply(PixelStrategySteamChoiceKind.BaitLair);
            var ambushedBait = state.ChoicePreview.Apply(
                PixelStrategySteamChoiceKind.BaitLair,
                PixelStrategySteamPressureEventKind.Ambush,
                chosenLevel: 2);
            Assert.That(ambushedBait.Threat, Is.EqualTo(cleanBait.Threat + 3));
        }

        [Test]
        public void FirstSteamScreenSample_ExposesEdgeHudPressureAndExtractionRead()
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();

            Assert.That(state.LoopNumber, Is.EqualTo(7));
            Assert.That(state.BagValue, Is.EqualTo(420));
            Assert.That(state.GatePercent, Is.EqualTo(68));
            Assert.That(state.PressurePercent, Is.EqualTo(82));
            Assert.That(state.StarterWeapon, Is.EqualTo("WOOD STICK"));
            Assert.That(state.StarterCoins, Is.EqualTo(12));
            Assert.That(state.Board.Run.Loot, Is.GreaterThanOrEqualTo(10));
            Assert.That(state.Board.Run.BagPressure, Is.GreaterThanOrEqualTo(8));
            Assert.That(state.Board.Run.Threat, Is.GreaterThan(0));
            Assert.IsTrue(state.Board.Run.ExtractReady);
        }
    }
}
