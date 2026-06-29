using System;
using System.Collections.Generic;
using FourfoldEchoes.StrategyLoop;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests
{
    public sealed class PixelStrategyDecisionScorerTests
    {
        [Test]
        public void ComparePlacements_LabelsGreedierRouteWithNumericDeltas()
        {
            var route = CreateSquareRoute();
            var config = new PixelStrategyRunConfig(
                maxLoops: 1,
                heroHealth: 12,
                bagCapacity: 20,
                extractLootTarget: 99,
                retreatThreatThreshold: 99);

            var result = PixelStrategyDecisionScorer.ComparePlacements(
                route,
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 1), threat: 1, damage: 1),
                PixelStrategyPlacement.Lair(new Vector2Int(1, 1), threat: 2, loot: 2, damage: 1),
                config);

            Assert.That(result.Label, Is.EqualTo(PixelStrategyDecisionLabel.Greedier));
            Assert.That(result.LabelText, Is.EqualTo("Greedier"));
            Assert.That(result.DeltaBMinusA.Loot, Is.EqualTo(8));
            Assert.That(result.DeltaBMinusA.Threat, Is.EqualTo(7));
            Assert.That(result.DeltaBMinusA.BagPressure, Is.EqualTo(4));
            Assert.That(result.DeltaBMinusA.Health, Is.EqualTo(-3));
            Assert.That(result.DeltaBMinusA.CompletedLoops, Is.EqualTo(0));
            Assert.That(result.DeltaBMinusA.ExtractReadiness, Is.EqualTo(0));
            Assert.That(result.Summary, Is.EqualTo("B vs A: loot +8, threat +7, bag +4, health -3, loop +0, extract +0."));
        }

        [Test]
        public void CompareWithBase_LabelsExtractReadyWhenCandidateUnlocksExtraction()
        {
            var route = CreateSquareRoute();
            var basePlacements = new[]
            {
                PixelStrategyPlacement.RewardCache(new Vector2Int(2, 1), loot: 4, bagPressure: 4)
            };
            var config = new PixelStrategyRunConfig(
                maxLoops: 1,
                heroHealth: 12,
                bagCapacity: 20,
                extractLootTarget: 8,
                extractPressureThreshold: 7,
                retreatThreatThreshold: 99);

            var result = PixelStrategyDecisionScorer.CompareWithBase(
                route,
                basePlacements,
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 1), threat: 1, damage: 0),
                PixelStrategyPlacement.Lair(new Vector2Int(1, 1), threat: 2, loot: 1, damage: 1),
                config);

            Assert.That(result.Label, Is.EqualTo(PixelStrategyDecisionLabel.ExtractReady));
            Assert.That(result.RecommendedChoice, Is.EqualTo(PixelStrategyDecisionChoice.CandidateB));
            Assert.That(result.CandidateA.Snapshot.ExtractReady, Is.False);
            Assert.That(result.CandidateB.Snapshot.ExtractReady, Is.True);
            Assert.That(result.DeltaBMinusA.ExtractReadiness, Is.EqualTo(1));
            Assert.That(result.DeltaBMinusA.Loot, Is.GreaterThan(0));
            Assert.That(result.DeltaBMinusA.BagPressure, Is.GreaterThan(0));
        }

        [Test]
        public void CompareWithBase_LabelsSaferExtractForLowerThreatReadyCandidate()
        {
            var route = CreateSquareRoute();
            var basePlacements = new[]
            {
                PixelStrategyPlacement.RewardCache(new Vector2Int(2, 1), loot: 4, bagPressure: 4)
            };
            var config = new PixelStrategyRunConfig(
                maxLoops: 2,
                heroHealth: 10,
                bagCapacity: 20,
                extractLootTarget: 6,
                extractPressureThreshold: 7,
                retreatThreatThreshold: 99);

            var result = PixelStrategyDecisionScorer.CompareWithBase(
                route,
                basePlacements,
                PixelStrategyPlacement.Lair(new Vector2Int(1, 1), threat: 2, loot: 1, damage: 1),
                PixelStrategyPlacement.RewardCache(new Vector2Int(0, 1), loot: 2, bagPressure: 2),
                config);

            Assert.That(result.Label, Is.EqualTo(PixelStrategyDecisionLabel.SaferExtract));
            Assert.That(result.RecommendedChoice, Is.EqualTo(PixelStrategyDecisionChoice.CandidateB));
            Assert.That(result.CandidateB.Snapshot.ExtractReady, Is.True);
            Assert.That(result.DeltaBMinusA.Threat, Is.LessThan(0));
            Assert.That(result.DeltaBMinusA.Health, Is.GreaterThan(0));
            Assert.That(result.DeltaBMinusA.ExtractReadiness, Is.EqualTo(0));
        }

        [Test]
        public void ComparePlacements_LabelsHigherThreatAndCanRecommendCandidateA()
        {
            var route = CreateSquareRoute();
            var config = new PixelStrategyRunConfig(
                maxLoops: 1,
                heroHealth: 12,
                bagCapacity: 20,
                extractLootTarget: 99,
                retreatThreatThreshold: 99);

            var result = PixelStrategyDecisionScorer.ComparePlacements(
                route,
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 1), threat: 1, damage: 0),
                PixelStrategyPlacement.Hazard(new Vector2Int(1, 0), threat: 4, damage: 2),
                config);

            Assert.That(result.Label, Is.EqualTo(PixelStrategyDecisionLabel.HigherThreat));
            Assert.That(result.RecommendedChoice, Is.EqualTo(PixelStrategyDecisionChoice.CandidateA));
            Assert.That(result.DeltaBMinusA.Loot, Is.EqualTo(0));
            Assert.That(result.DeltaBMinusA.Threat, Is.EqualTo(3));
            Assert.That(result.DeltaBMinusA.Health, Is.EqualTo(-2));
        }

        [Test]
        public void CompareWithBase_LabelsDesperateRetreatWhenCandidateTripsRetreat()
        {
            var route = CreateSquareRoute();
            var basePlacements = new[]
            {
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 1), threat: 1, damage: 0)
            };
            var config = new PixelStrategyRunConfig(
                maxLoops: 2,
                heroHealth: 12,
                bagCapacity: 20,
                extractLootTarget: 10,
                extractPressureThreshold: 9,
                retreatThreatThreshold: 12);

            var result = PixelStrategyDecisionScorer.CompareWithBase(
                route,
                basePlacements,
                PixelStrategyPlacement.RewardCache(new Vector2Int(0, 1), loot: 2, bagPressure: 2),
                PixelStrategyPlacement.Lair(new Vector2Int(1, 1), threat: 4, loot: 0, damage: 0),
                config);

            Assert.That(result.Label, Is.EqualTo(PixelStrategyDecisionLabel.DesperateRetreat));
            Assert.That(result.RecommendedChoice, Is.EqualTo(PixelStrategyDecisionChoice.CandidateA));
            Assert.That(result.CandidateA.Snapshot.Outcome, Is.EqualTo(PixelStrategyRunOutcome.Completed));
            Assert.That(result.CandidateB.Snapshot.Outcome, Is.EqualTo(PixelStrategyRunOutcome.Retreated));
            Assert.That(result.DeltaBMinusA.Threat, Is.GreaterThan(0));
        }

        [Test]
        public void Compare_RejectsMissingRoute()
        {
            Assert.Throws<ArgumentNullException>(() => PixelStrategyDecisionScorer.Compare(
                null,
                Array.Empty<PixelStrategyPlacement>(),
                Array.Empty<PixelStrategyPlacement>()));
        }

        private static PixelStrategyLoopRoute CreateSquareRoute()
        {
            return new PixelStrategyLoopRoute(new List<Vector2Int>
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(1, 2),
                new Vector2Int(0, 2),
                new Vector2Int(0, 1)
            });
        }
    }
}
