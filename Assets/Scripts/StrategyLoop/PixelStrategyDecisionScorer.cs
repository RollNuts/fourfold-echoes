using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.StrategyLoop
{
    public enum PixelStrategyDecisionLabel
    {
        Balanced,
        Greedier,
        SaferExtract,
        HigherThreat,
        ExtractReady,
        DesperateRetreat
    }

    public enum PixelStrategyDecisionChoice
    {
        CandidateA,
        CandidateB,
        Tie
    }

    public readonly struct PixelStrategyDecisionCandidate
    {
        public PixelStrategyDecisionCandidate(PixelStrategyRunSnapshot snapshot, int score)
        {
            Snapshot = snapshot;
            Score = score;
        }

        public PixelStrategyRunSnapshot Snapshot { get; }
        public int Score { get; }
    }

    public readonly struct PixelStrategyDecisionDelta
    {
        public PixelStrategyDecisionDelta(
            int loot,
            int threat,
            int bagPressure,
            int health,
            int completedLoops,
            int stepsTaken,
            int extractReadiness,
            int score)
        {
            Loot = loot;
            Threat = threat;
            BagPressure = bagPressure;
            Health = health;
            CompletedLoops = completedLoops;
            StepsTaken = stepsTaken;
            ExtractReadiness = extractReadiness;
            Score = score;
        }

        public int Loot { get; }
        public int Threat { get; }
        public int BagPressure { get; }
        public int Health { get; }
        public int CompletedLoops { get; }
        public int StepsTaken { get; }
        public int ExtractReadiness { get; }
        public int Score { get; }
    }

    public readonly struct PixelStrategyDecisionComparison
    {
        public PixelStrategyDecisionComparison(
            PixelStrategyDecisionCandidate candidateA,
            PixelStrategyDecisionCandidate candidateB,
            PixelStrategyDecisionDelta deltaBMinusA,
            PixelStrategyDecisionLabel label,
            PixelStrategyDecisionChoice recommendedChoice,
            string summary)
        {
            CandidateA = candidateA;
            CandidateB = candidateB;
            DeltaBMinusA = deltaBMinusA;
            Label = label;
            RecommendedChoice = recommendedChoice;
            Summary = summary;
        }

        public PixelStrategyDecisionCandidate CandidateA { get; }
        public PixelStrategyDecisionCandidate CandidateB { get; }
        public PixelStrategyDecisionDelta DeltaBMinusA { get; }
        public PixelStrategyDecisionLabel Label { get; }
        public PixelStrategyDecisionChoice RecommendedChoice { get; }
        public string LabelText => Label.ToString();
        public string Summary { get; }
    }

    public static class PixelStrategyDecisionScorer
    {
        private const int TieScoreWindow = 2;

        public static PixelStrategyDecisionComparison Compare(
            PixelStrategyLoopRoute route,
            IEnumerable<PixelStrategyPlacement> candidateA,
            IEnumerable<PixelStrategyPlacement> candidateB,
            PixelStrategyRunConfig config = null)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            config = config ?? new PixelStrategyRunConfig();

            var snapshotA = PixelStrategyLoopSimulator.Simulate(route, candidateA, config);
            var snapshotB = PixelStrategyLoopSimulator.Simulate(route, candidateB, config);
            var scoreA = Score(snapshotA, config);
            var scoreB = Score(snapshotB, config);
            var scoredA = new PixelStrategyDecisionCandidate(snapshotA, scoreA);
            var scoredB = new PixelStrategyDecisionCandidate(snapshotB, scoreB);
            var delta = CreateDelta(snapshotB, snapshotA, scoreB - scoreA);
            var label = ChooseLabel(snapshotA, snapshotB, delta);
            var recommendedChoice = ChooseRecommendedChoice(scoreA, scoreB);

            return new PixelStrategyDecisionComparison(
                scoredA,
                scoredB,
                delta,
                label,
                recommendedChoice,
                BuildSummary(delta));
        }

        public static PixelStrategyDecisionComparison ComparePlacements(
            PixelStrategyLoopRoute route,
            PixelStrategyPlacement candidateA,
            PixelStrategyPlacement candidateB,
            PixelStrategyRunConfig config = null)
        {
            return Compare(
                route,
                new[] { candidateA },
                new[] { candidateB },
                config);
        }

        public static PixelStrategyDecisionComparison CompareWithBase(
            PixelStrategyLoopRoute route,
            IEnumerable<PixelStrategyPlacement> basePlacements,
            PixelStrategyPlacement candidateA,
            PixelStrategyPlacement candidateB,
            PixelStrategyRunConfig config = null)
        {
            return CompareWithBase(
                route,
                basePlacements,
                new[] { candidateA },
                new[] { candidateB },
                config);
        }

        public static PixelStrategyDecisionComparison CompareWithBase(
            PixelStrategyLoopRoute route,
            IEnumerable<PixelStrategyPlacement> basePlacements,
            IEnumerable<PixelStrategyPlacement> candidateA,
            IEnumerable<PixelStrategyPlacement> candidateB,
            PixelStrategyRunConfig config = null)
        {
            return Compare(
                route,
                Merge(basePlacements, candidateA),
                Merge(basePlacements, candidateB),
                config);
        }

        private static PixelStrategyDecisionDelta CreateDelta(
            PixelStrategyRunSnapshot snapshotB,
            PixelStrategyRunSnapshot snapshotA,
            int scoreDelta)
        {
            return new PixelStrategyDecisionDelta(
                snapshotB.Loot - snapshotA.Loot,
                snapshotB.Threat - snapshotA.Threat,
                snapshotB.BagPressure - snapshotA.BagPressure,
                snapshotB.Health - snapshotA.Health,
                snapshotB.CompletedLoops - snapshotA.CompletedLoops,
                snapshotB.StepsTaken - snapshotA.StepsTaken,
                ToInt(snapshotB.ExtractReady) - ToInt(snapshotA.ExtractReady),
                scoreDelta);
        }

        private static int Score(PixelStrategyRunSnapshot snapshot, PixelStrategyRunConfig config)
        {
            var score = 0;
            score += snapshot.Loot * 6;
            score += snapshot.Health * 4;
            score -= snapshot.Threat * 2;
            score -= snapshot.BagPressure;
            score += snapshot.CompletedLoops;

            if (snapshot.ExtractReady)
            {
                score += 25;
            }

            if (snapshot.Loot >= config.ExtractLootTarget)
            {
                score += 8;
            }

            if (snapshot.BagPressure >= config.ExtractPressureThreshold)
            {
                score += 5;
            }

            if (snapshot.Threat >= config.RetreatThreatThreshold)
            {
                score -= 10;
            }

            if (snapshot.Health <= Mathf.Max(1, config.HeroHealth / 3))
            {
                score -= 10;
            }

            switch (snapshot.Outcome)
            {
                case PixelStrategyRunOutcome.Extracted:
                    score += 25;
                    break;
                case PixelStrategyRunOutcome.Completed:
                    score += 10;
                    break;
                case PixelStrategyRunOutcome.Retreated:
                    score -= 20;
                    break;
                case PixelStrategyRunOutcome.Defeated:
                    score -= 80;
                    break;
                default:
                    break;
            }

            return score;
        }

        private static PixelStrategyDecisionLabel ChooseLabel(
            PixelStrategyRunSnapshot snapshotA,
            PixelStrategyRunSnapshot snapshotB,
            PixelStrategyDecisionDelta delta)
        {
            if (IsFailedRun(snapshotB) && !IsFailedRun(snapshotA))
            {
                return PixelStrategyDecisionLabel.DesperateRetreat;
            }

            if (snapshotB.ExtractReady && !snapshotA.ExtractReady)
            {
                if (delta.Threat <= 0 && delta.Health >= 0)
                {
                    return PixelStrategyDecisionLabel.SaferExtract;
                }

                return PixelStrategyDecisionLabel.ExtractReady;
            }

            if (snapshotB.ExtractReady && snapshotA.ExtractReady && (delta.Threat < 0 || delta.Health > 0))
            {
                return PixelStrategyDecisionLabel.SaferExtract;
            }

            if (delta.Loot > 0 && (delta.Threat > 0 || delta.BagPressure > 0 || delta.Health < 0))
            {
                return PixelStrategyDecisionLabel.Greedier;
            }

            if (delta.Threat > 0 && delta.Loot <= 0)
            {
                return PixelStrategyDecisionLabel.HigherThreat;
            }

            if (delta.Threat < 0 || delta.Health > 0)
            {
                return PixelStrategyDecisionLabel.SaferExtract;
            }

            if (delta.Loot > 0)
            {
                return PixelStrategyDecisionLabel.Greedier;
            }

            return PixelStrategyDecisionLabel.Balanced;
        }

        private static PixelStrategyDecisionChoice ChooseRecommendedChoice(int scoreA, int scoreB)
        {
            var scoreDelta = scoreB - scoreA;
            if (scoreDelta > TieScoreWindow)
            {
                return PixelStrategyDecisionChoice.CandidateB;
            }

            if (scoreDelta < -TieScoreWindow)
            {
                return PixelStrategyDecisionChoice.CandidateA;
            }

            return PixelStrategyDecisionChoice.Tie;
        }

        private static string BuildSummary(PixelStrategyDecisionDelta delta)
        {
            return string.Format(
                "B vs A: loot {0}, threat {1}, bag {2}, health {3}, loop {4}, extract {5}.",
                FormatSigned(delta.Loot),
                FormatSigned(delta.Threat),
                FormatSigned(delta.BagPressure),
                FormatSigned(delta.Health),
                FormatSigned(delta.CompletedLoops),
                FormatSigned(delta.ExtractReadiness));
        }

        private static string FormatSigned(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }

        private static bool IsFailedRun(PixelStrategyRunSnapshot snapshot)
        {
            return snapshot.Outcome == PixelStrategyRunOutcome.Defeated ||
                snapshot.Outcome == PixelStrategyRunOutcome.Retreated;
        }

        private static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

        private static List<PixelStrategyPlacement> Merge(
            IEnumerable<PixelStrategyPlacement> basePlacements,
            IEnumerable<PixelStrategyPlacement> candidatePlacements)
        {
            var merged = new List<PixelStrategyPlacement>();
            AddRange(merged, basePlacements);
            AddRange(merged, candidatePlacements);
            return merged;
        }

        private static void AddRange(
            ICollection<PixelStrategyPlacement> destination,
            IEnumerable<PixelStrategyPlacement> placements)
        {
            if (placements == null)
            {
                return;
            }

            foreach (var placement in placements)
            {
                destination.Add(placement);
            }
        }
    }
}
