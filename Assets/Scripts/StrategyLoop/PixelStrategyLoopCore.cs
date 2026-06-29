using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.StrategyLoop
{
    public enum PixelStrategyPlacementKind
    {
        Lair,
        Hazard,
        RewardCache
    }

    public enum PixelStrategyRunOutcome
    {
        Completed,
        Extracted,
        Retreated,
        Defeated
    }

    public enum PixelStrategyRunDecision
    {
        Continue,
        Extract,
        Retreat
    }

    public readonly struct PixelStrategyPlacement
    {
        public PixelStrategyPlacement(
            PixelStrategyPlacementKind kind,
            Vector2Int cell,
            int threat,
            int loot,
            int bagPressure,
            int damage)
        {
            Kind = kind;
            Cell = cell;
            Threat = Mathf.Max(0, threat);
            Loot = Mathf.Max(0, loot);
            BagPressure = Mathf.Max(0, bagPressure);
            Damage = Mathf.Max(0, damage);
        }

        public PixelStrategyPlacementKind Kind { get; }
        public Vector2Int Cell { get; }
        public int Threat { get; }
        public int Loot { get; }
        public int BagPressure { get; }
        public int Damage { get; }

        public static PixelStrategyPlacement Lair(Vector2Int cell, int threat = 4, int loot = 1, int damage = 1)
        {
            return new PixelStrategyPlacement(PixelStrategyPlacementKind.Lair, cell, threat, loot, 1, damage);
        }

        public static PixelStrategyPlacement Hazard(Vector2Int cell, int threat = 2, int damage = 2)
        {
            return new PixelStrategyPlacement(PixelStrategyPlacementKind.Hazard, cell, threat, 0, 0, damage);
        }

        public static PixelStrategyPlacement RewardCache(Vector2Int cell, int loot = 4, int bagPressure = 3)
        {
            return new PixelStrategyPlacement(PixelStrategyPlacementKind.RewardCache, cell, 1, loot, bagPressure, 0);
        }
    }

    public sealed class PixelStrategyLoopRoute
    {
        private readonly List<Vector2Int> cells;
        private readonly HashSet<Vector2Int> cellSet;

        public PixelStrategyLoopRoute(IEnumerable<Vector2Int> routeCells)
        {
            if (routeCells == null)
            {
                throw new ArgumentNullException(nameof(routeCells));
            }

            cells = new List<Vector2Int>(routeCells);
            cellSet = new HashSet<Vector2Int>(cells);

            if (cells.Count < 4)
            {
                throw new ArgumentException("A strategy loop route needs at least four cells.", nameof(routeCells));
            }

            if (cellSet.Count != cells.Count)
            {
                throw new ArgumentException("A strategy loop route cannot contain duplicate cells.", nameof(routeCells));
            }

            for (var index = 0; index < cells.Count; index++)
            {
                var nextIndex = (index + 1) % cells.Count;
                if (ManhattanDistance(cells[index], cells[nextIndex]) != 1)
                {
                    throw new ArgumentException("A strategy loop route must be orthogonally connected and closed.", nameof(routeCells));
                }
            }
        }

        public IReadOnlyList<Vector2Int> Cells => cells;
        public int Count => cells.Count;

        public bool Contains(Vector2Int cell)
        {
            return cellSet.Contains(cell);
        }

        public bool InfluencesRoute(Vector2Int cell)
        {
            if (Contains(cell))
            {
                return true;
            }

            for (var index = 0; index < cells.Count; index++)
            {
                if (ManhattanDistance(cells[index], cell) == 1)
                {
                    return true;
                }
            }

            return false;
        }

        internal static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }

    public sealed class PixelStrategyRunConfig
    {
        public PixelStrategyRunConfig(
            int maxLoops = 3,
            int heroHealth = 10,
            int bagCapacity = 10,
            int extractLootTarget = 6,
            int extractPressureThreshold = 7,
            int retreatThreatThreshold = 16)
        {
            MaxLoops = Mathf.Max(1, maxLoops);
            HeroHealth = Mathf.Max(1, heroHealth);
            BagCapacity = Mathf.Max(1, bagCapacity);
            ExtractLootTarget = Mathf.Max(1, extractLootTarget);
            ExtractPressureThreshold = Mathf.Max(1, extractPressureThreshold);
            RetreatThreatThreshold = Mathf.Max(1, retreatThreatThreshold);
        }

        public int MaxLoops { get; }
        public int HeroHealth { get; }
        public int BagCapacity { get; }
        public int ExtractLootTarget { get; }
        public int ExtractPressureThreshold { get; }
        public int RetreatThreatThreshold { get; }
    }

    public readonly struct PixelStrategyRunSnapshot
    {
        public PixelStrategyRunSnapshot(
            PixelStrategyRunOutcome outcome,
            PixelStrategyRunDecision decision,
            int completedLoops,
            int stepsTaken,
            int health,
            int threat,
            int loot,
            int bagPressure,
            bool extractReady)
        {
            Outcome = outcome;
            Decision = decision;
            CompletedLoops = completedLoops;
            StepsTaken = stepsTaken;
            Health = health;
            Threat = threat;
            Loot = loot;
            BagPressure = bagPressure;
            ExtractReady = extractReady;
        }

        public PixelStrategyRunOutcome Outcome { get; }
        public PixelStrategyRunDecision Decision { get; }
        public int CompletedLoops { get; }
        public int StepsTaken { get; }
        public int Health { get; }
        public int Threat { get; }
        public int Loot { get; }
        public int BagPressure { get; }
        public bool ExtractReady { get; }
        public bool BagIsPressured => BagPressure > 0;
        public float BagPressure01 => Loot <= 0 ? 0f : Mathf.Clamp01((float)BagPressure / Mathf.Max(1, Loot));
    }

    public static class PixelStrategyLoopSimulator
    {
        public static PixelStrategyRunSnapshot Simulate(
            PixelStrategyLoopRoute route,
            IEnumerable<PixelStrategyPlacement> placements,
            PixelStrategyRunConfig config = null)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            config = config ?? new PixelStrategyRunConfig();
            var activePlacements = FilterInfluencingPlacements(route, placements);
            var health = config.HeroHealth;
            var threat = 0;
            var loot = 0;
            var bagPressure = 0;
            var steps = 0;

            for (var loop = 1; loop <= config.MaxLoops; loop++)
            {
                for (var routeIndex = 0; routeIndex < route.Count; routeIndex++)
                {
                    var cell = route.Cells[routeIndex];
                    steps++;

                    for (var placementIndex = 0; placementIndex < activePlacements.Count; placementIndex++)
                    {
                        var placement = activePlacements[placementIndex];
                        if (!AffectsStep(placement, cell))
                        {
                            continue;
                        }

                        threat += placement.Threat;
                        health -= placement.Damage;
                        loot = Mathf.Min(config.BagCapacity, loot + placement.Loot);
                        bagPressure = Mathf.Min(config.BagCapacity, bagPressure + placement.BagPressure);
                    }

                    if (health <= 0)
                    {
                        return new PixelStrategyRunSnapshot(
                            PixelStrategyRunOutcome.Defeated,
                            PixelStrategyRunDecision.Continue,
                            loop - 1,
                            steps,
                            0,
                            threat,
                            loot,
                            bagPressure,
                            false);
                    }
                }

                var decision = DecideAfterLoop(threat, loot, bagPressure, config);
                if (decision == PixelStrategyRunDecision.Extract)
                {
                    return new PixelStrategyRunSnapshot(
                        PixelStrategyRunOutcome.Extracted,
                        decision,
                        loop,
                        steps,
                        health,
                        threat,
                        loot,
                        bagPressure,
                        true);
                }

                if (decision == PixelStrategyRunDecision.Retreat)
                {
                    return new PixelStrategyRunSnapshot(
                        PixelStrategyRunOutcome.Retreated,
                        decision,
                        loop,
                        steps,
                        health,
                        threat,
                        loot,
                        bagPressure,
                        false);
                }
            }

            return new PixelStrategyRunSnapshot(
                PixelStrategyRunOutcome.Completed,
                PixelStrategyRunDecision.Continue,
                config.MaxLoops,
                steps,
                health,
                threat,
                loot,
                bagPressure,
                loot >= config.ExtractLootTarget);
        }

        public static PixelStrategyRunDecision DecideAfterLoop(
            int threat,
            int loot,
            int bagPressure,
            PixelStrategyRunConfig config = null)
        {
            config = config ?? new PixelStrategyRunConfig();

            if (loot >= config.ExtractLootTarget && bagPressure >= config.ExtractPressureThreshold)
            {
                return PixelStrategyRunDecision.Extract;
            }

            if (threat >= config.RetreatThreatThreshold && loot < config.ExtractLootTarget)
            {
                return PixelStrategyRunDecision.Retreat;
            }

            return PixelStrategyRunDecision.Continue;
        }

        private static List<PixelStrategyPlacement> FilterInfluencingPlacements(
            PixelStrategyLoopRoute route,
            IEnumerable<PixelStrategyPlacement> placements)
        {
            var active = new List<PixelStrategyPlacement>();
            if (placements == null)
            {
                return active;
            }

            foreach (var placement in placements)
            {
                if (route.InfluencesRoute(placement.Cell))
                {
                    active.Add(placement);
                }
            }

            return active;
        }

        private static bool AffectsStep(PixelStrategyPlacement placement, Vector2Int routeCell)
        {
            var distance = PixelStrategyLoopRoute.ManhattanDistance(placement.Cell, routeCell);
            switch (placement.Kind)
            {
                case PixelStrategyPlacementKind.Lair:
                    return distance <= 1;
                case PixelStrategyPlacementKind.Hazard:
                case PixelStrategyPlacementKind.RewardCache:
                    return distance == 0;
                default:
                    return false;
            }
        }
    }
}
