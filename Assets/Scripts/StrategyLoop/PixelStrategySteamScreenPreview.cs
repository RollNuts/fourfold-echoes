using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.StrategyLoop
{
    public enum PixelStrategySteamScreenCardTone
    {
        Greedy,
        SafeSelected,
        Doom
    }

    public enum PixelStrategySteamChoiceKind
    {
        BaitLair,
        CutToGate,
        GreedRelic
    }

    public readonly struct PixelStrategySteamScreenCard
    {
        public PixelStrategySteamScreenCard(
            string option,
            string title,
            string rewardText,
            string riskText,
            string footerText,
            PixelStrategySteamScreenCardTone tone,
            bool selected)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            RewardText = rewardText ?? throw new ArgumentNullException(nameof(rewardText));
            RiskText = riskText ?? throw new ArgumentNullException(nameof(riskText));
            FooterText = footerText ?? throw new ArgumentNullException(nameof(footerText));
            Tone = tone;
            Selected = selected;
        }

        public string Option { get; }
        public string Title { get; }
        public string RewardText { get; }
        public string RiskText { get; }
        public string FooterText { get; }
        public PixelStrategySteamScreenCardTone Tone { get; }
        public bool Selected { get; }
    }

    public sealed class PixelStrategySteamChoiceImpact
    {
        public PixelStrategySteamChoiceImpact(
            PixelStrategySteamChoiceKind selectedChoice,
            IReadOnlyList<Vector2Int> openedGateCells,
            IReadOnlyList<Vector2Int> sealedPressureCells,
            string boardCallout)
        {
            SelectedChoice = selectedChoice;
            OpenedGateCells = openedGateCells ?? throw new ArgumentNullException(nameof(openedGateCells));
            SealedPressureCells = sealedPressureCells ?? throw new ArgumentNullException(nameof(sealedPressureCells));
            BoardCallout = boardCallout ?? throw new ArgumentNullException(nameof(boardCallout));
        }

        public PixelStrategySteamChoiceKind SelectedChoice { get; }
        public IReadOnlyList<Vector2Int> OpenedGateCells { get; }
        public IReadOnlyList<Vector2Int> SealedPressureCells { get; }
        public string BoardCallout { get; }
    }

    public sealed class PixelStrategySteamScreenPreviewState
    {
        public PixelStrategySteamScreenPreviewState(
            PixelStrategyBoardPreviewState board,
            IReadOnlyList<PixelStrategySteamScreenCard> cards,
            PixelStrategySteamChoiceImpact impact,
            int loopNumber,
            int bagValue,
            int gatePercent,
            int pressurePercent,
            string starterWeapon,
            int starterCoins)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Cards = cards ?? throw new ArgumentNullException(nameof(cards));
            Impact = impact ?? throw new ArgumentNullException(nameof(impact));
            LoopNumber = Mathf.Max(1, loopNumber);
            BagValue = Mathf.Max(0, bagValue);
            GatePercent = Mathf.Clamp(gatePercent, 0, 100);
            PressurePercent = Mathf.Clamp(pressurePercent, 0, 100);
            StarterWeapon = starterWeapon ?? throw new ArgumentNullException(nameof(starterWeapon));
            StarterCoins = Mathf.Max(0, starterCoins);
        }

        public PixelStrategyBoardPreviewState Board { get; }
        public IReadOnlyList<PixelStrategySteamScreenCard> Cards { get; }
        public PixelStrategySteamChoiceImpact Impact { get; }
        public int LoopNumber { get; }
        public int BagValue { get; }
        public int GatePercent { get; }
        public int PressurePercent { get; }
        public string StarterWeapon { get; }
        public int StarterCoins { get; }
    }

    public static class PixelStrategySteamScreenPreviewFactory
    {
        public static PixelStrategySteamScreenPreviewState CreateFirstSteamScreenSample()
        {
            var route = new PixelStrategyLoopRoute(new[]
            {
                new Vector2Int(1, 4),
                new Vector2Int(2, 4),
                new Vector2Int(3, 4),
                new Vector2Int(4, 4),
                new Vector2Int(4, 3),
                new Vector2Int(5, 3),
                new Vector2Int(5, 2),
                new Vector2Int(6, 2),
                new Vector2Int(6, 1),
                new Vector2Int(7, 1),
                new Vector2Int(8, 1),
                new Vector2Int(9, 1),
                new Vector2Int(10, 1),
                new Vector2Int(10, 2),
                new Vector2Int(11, 2),
                new Vector2Int(11, 3),
                new Vector2Int(12, 3),
                new Vector2Int(12, 4),
                new Vector2Int(12, 5),
                new Vector2Int(11, 5),
                new Vector2Int(11, 6),
                new Vector2Int(10, 6),
                new Vector2Int(9, 6),
                new Vector2Int(8, 6),
                new Vector2Int(7, 6),
                new Vector2Int(6, 6),
                new Vector2Int(5, 6),
                new Vector2Int(4, 6),
                new Vector2Int(3, 6),
                new Vector2Int(2, 6),
                new Vector2Int(1, 6),
                new Vector2Int(1, 5)
            });

            var placements = new List<PixelStrategyPlacement>
            {
                PixelStrategyPlacement.RewardCache(new Vector2Int(4, 3), loot: 5, bagPressure: 4),
                PixelStrategyPlacement.RewardCache(new Vector2Int(7, 1), loot: 6, bagPressure: 4),
                PixelStrategyPlacement.Lair(new Vector2Int(6, 2), threat: 3, loot: 1, damage: 1),
                PixelStrategyPlacement.Hazard(new Vector2Int(9, 1), threat: 3, damage: 1),
                PixelStrategyPlacement.Hazard(new Vector2Int(2, 5), threat: 2, damage: 0)
            };

            var config = new PixelStrategyRunConfig(
                maxLoops: 2,
                heroHealth: 14,
                bagCapacity: 20,
                extractLootTarget: 10,
                extractPressureThreshold: 8,
                retreatThreatThreshold: 28);

            var board = new PixelStrategyBoardPreviewState(
                width: 14,
                height: 7,
                route,
                placements,
                heroCell: new Vector2Int(2, 4),
                extractCell: new Vector2Int(12, 4),
                PixelStrategyLoopSimulator.Simulate(route, placements, config));

            var cards = new[]
            {
                new PixelStrategySteamScreenCard(
                    "A",
                    "BAIT THE CHOSEN",
                    "+620",
                    "82%",
                    "wood stick, bad odds",
                    PixelStrategySteamScreenCardTone.Greedy,
                    selected: false),
                new PixelStrategySteamScreenCard(
                    "B",
                    "CUT TO GATE",
                    "EXIT 86%",
                    "safe",
                    "selected: spare the fool",
                    PixelStrategySteamScreenCardTone.SafeSelected,
                    selected: true),
                new PixelStrategySteamScreenCard(
                    "C",
                    "GREED RELIC",
                    "+900",
                    "DOOM",
                    "king paid 12G, asks too much",
                    PixelStrategySteamScreenCardTone.Doom,
                    selected: false)
            };

            var impact = new PixelStrategySteamChoiceImpact(
                PixelStrategySteamChoiceKind.CutToGate,
                new[]
                {
                    new Vector2Int(8, 3),
                    new Vector2Int(9, 3),
                    new Vector2Int(10, 3),
                    new Vector2Int(11, 3),
                    new Vector2Int(12, 4)
                },
                new[]
                {
                    new Vector2Int(9, 1),
                    new Vector2Int(10, 2)
                },
                "GATE CUT OPEN");

            return new PixelStrategySteamScreenPreviewState(
                board,
                cards,
                impact,
                loopNumber: 7,
                bagValue: 420,
                gatePercent: 68,
                pressurePercent: 82,
                starterWeapon: "WOOD STICK",
                starterCoins: 12);
        }
    }
}
