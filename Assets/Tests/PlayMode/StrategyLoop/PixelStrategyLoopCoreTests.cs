using System;
using System.Collections.Generic;
using FourfoldEchoes.StrategyLoop;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests
{
    public sealed class PixelStrategyLoopCoreTests
    {
        [Test]
        public void Route_RequiresClosedOrthogonalBoardLoop()
        {
            var route = CreateSquareRoute();

            Assert.That(route.Count, Is.EqualTo(8));
            Assert.IsTrue(route.Contains(new Vector2Int(2, 1)));
            Assert.IsTrue(route.InfluencesRoute(new Vector2Int(1, 1)));
            Assert.IsFalse(route.InfluencesRoute(new Vector2Int(8, 8)));
        }

        [Test]
        public void Route_RejectsGapsAndDuplicateTiles()
        {
            Assert.Throws<ArgumentException>(() => new PixelStrategyLoopRoute(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(0, 1)
            }));

            Assert.Throws<ArgumentException>(() => new PixelStrategyLoopRoute(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(0, 1),
                new Vector2Int(0, 0)
            }));
        }

        [Test]
        public void Simulate_CombinesAdjacentLairsRouteHazardsAndRewardPressure()
        {
            var route = CreateSquareRoute();
            var placements = new[]
            {
                PixelStrategyPlacement.Lair(new Vector2Int(1, 1), threat: 2, loot: 1, damage: 0),
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 1), threat: 1, damage: 1),
                PixelStrategyPlacement.RewardCache(new Vector2Int(0, 1), loot: 3, bagPressure: 2),
                PixelStrategyPlacement.RewardCache(new Vector2Int(8, 8), loot: 99, bagPressure: 99)
            };

            var result = PixelStrategyLoopSimulator.Simulate(
                route,
                placements,
                new PixelStrategyRunConfig(maxLoops: 1, heroHealth: 10, bagCapacity: 20, extractLootTarget: 99));

            Assert.That(result.Outcome, Is.EqualTo(PixelStrategyRunOutcome.Completed));
            Assert.That(result.CompletedLoops, Is.EqualTo(1));
            Assert.That(result.StepsTaken, Is.EqualTo(8));
            Assert.That(result.Health, Is.EqualTo(9));
            Assert.That(result.Threat, Is.EqualTo(10));
            Assert.That(result.Loot, Is.EqualTo(7));
            Assert.That(result.BagPressure, Is.EqualTo(6));
            Assert.IsTrue(result.BagIsPressured);
        }

        [Test]
        public void Simulate_ExtractsWhenLootAndBagPressureCreateTensionAtLoopBoundary()
        {
            var route = CreateSquareRoute();
            var placements = new[]
            {
                PixelStrategyPlacement.Lair(new Vector2Int(1, 1), threat: 1, loot: 1, damage: 0),
                PixelStrategyPlacement.RewardCache(new Vector2Int(2, 1), loot: 4, bagPressure: 4)
            };

            var result = PixelStrategyLoopSimulator.Simulate(
                route,
                placements,
                new PixelStrategyRunConfig(
                    maxLoops: 3,
                    heroHealth: 10,
                    bagCapacity: 12,
                    extractLootTarget: 6,
                    extractPressureThreshold: 7,
                    retreatThreatThreshold: 30));

            Assert.That(result.Outcome, Is.EqualTo(PixelStrategyRunOutcome.Extracted));
            Assert.That(result.Decision, Is.EqualTo(PixelStrategyRunDecision.Extract));
            Assert.That(result.CompletedLoops, Is.EqualTo(1));
            Assert.That(result.Loot, Is.GreaterThanOrEqualTo(6));
            Assert.That(result.BagPressure, Is.GreaterThanOrEqualTo(7));
            Assert.IsTrue(result.ExtractReady);
        }

        [Test]
        public void Simulate_RetreatsWhenThreatRisesBeforeLootTarget()
        {
            var route = CreateSquareRoute();
            var placements = new[]
            {
                PixelStrategyPlacement.Lair(new Vector2Int(1, 1), threat: 4, loot: 0, damage: 0),
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 1), threat: 2, damage: 1)
            };

            var result = PixelStrategyLoopSimulator.Simulate(
                route,
                placements,
                new PixelStrategyRunConfig(
                    maxLoops: 3,
                    heroHealth: 10,
                    bagCapacity: 12,
                    extractLootTarget: 8,
                    extractPressureThreshold: 5,
                    retreatThreatThreshold: 12));

            Assert.That(result.Outcome, Is.EqualTo(PixelStrategyRunOutcome.Retreated));
            Assert.That(result.Decision, Is.EqualTo(PixelStrategyRunDecision.Retreat));
            Assert.That(result.CompletedLoops, Is.EqualTo(1));
            Assert.That(result.Loot, Is.LessThan(8));
            Assert.IsFalse(result.ExtractReady);
        }

        [Test]
        public void Simulate_DefeatCanInterruptBeforeExtractionDecision()
        {
            var route = CreateSquareRoute();
            var placements = new[]
            {
                PixelStrategyPlacement.Hazard(new Vector2Int(1, 0), threat: 1, damage: 4),
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 0), threat: 1, damage: 4)
            };

            var result = PixelStrategyLoopSimulator.Simulate(
                route,
                placements,
                new PixelStrategyRunConfig(maxLoops: 2, heroHealth: 6, bagCapacity: 10));

            Assert.That(result.Outcome, Is.EqualTo(PixelStrategyRunOutcome.Defeated));
            Assert.That(result.CompletedLoops, Is.EqualTo(0));
            Assert.That(result.Health, Is.EqualTo(0));
            Assert.IsFalse(result.ExtractReady);
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
