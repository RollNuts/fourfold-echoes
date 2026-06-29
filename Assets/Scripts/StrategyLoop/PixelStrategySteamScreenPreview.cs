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
            PixelStrategySteamChoiceKind choice,
            string title,
            string rewardText,
            string riskText,
            string footerText,
            PixelStrategySteamScreenCardTone tone,
            bool selected)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            Choice = choice;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            RewardText = rewardText ?? throw new ArgumentNullException(nameof(rewardText));
            RiskText = riskText ?? throw new ArgumentNullException(nameof(riskText));
            FooterText = footerText ?? throw new ArgumentNullException(nameof(footerText));
            Tone = tone;
            Selected = selected;
        }

        public string Option { get; }
        public PixelStrategySteamChoiceKind Choice { get; }
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

    public sealed class PixelStrategyFourfoldIdentityRead
    {
        public PixelStrategyFourfoldIdentityRead(
            IReadOnlyList<string> cornerSigils,
            IReadOnlyList<Vector2Int> echoCells,
            IReadOnlyList<Vector2Int> dangerRingCells,
            IReadOnlyList<string> carriedLoot,
            int litSealBeatCount,
            int crackedSealBeatCount)
        {
            CornerSigils = cornerSigils ?? throw new ArgumentNullException(nameof(cornerSigils));
            EchoCells = echoCells ?? throw new ArgumentNullException(nameof(echoCells));
            DangerRingCells = dangerRingCells ?? throw new ArgumentNullException(nameof(dangerRingCells));
            CarriedLoot = carriedLoot ?? throw new ArgumentNullException(nameof(carriedLoot));
            LitSealBeatCount = Mathf.Clamp(litSealBeatCount, 0, 4);
            CrackedSealBeatCount = Mathf.Clamp(crackedSealBeatCount, 0, 4);
        }

        public IReadOnlyList<string> CornerSigils { get; }
        public IReadOnlyList<Vector2Int> EchoCells { get; }
        public IReadOnlyList<Vector2Int> DangerRingCells { get; }
        public IReadOnlyList<string> CarriedLoot { get; }
        public int LitSealBeatCount { get; }
        public int CrackedSealBeatCount { get; }
    }

    public readonly struct PixelStrategySteamChoiceDelta
    {
        public PixelStrategySteamChoiceDelta(
            PixelStrategySteamChoiceKind choice,
            int loot,
            int threat,
            int gate,
            int pressure,
            int extract)
        {
            Choice = choice;
            Loot = loot;
            Threat = threat;
            Gate = gate;
            Pressure = pressure;
            Extract = extract;
        }

        public PixelStrategySteamChoiceKind Choice { get; }
        public int Loot { get; }
        public int Threat { get; }
        public int Gate { get; }
        public int Pressure { get; }
        public int Extract { get; }
    }

    public readonly struct PixelStrategySteamChoiceForecast
    {
        public PixelStrategySteamChoiceForecast(
            PixelStrategySteamChoiceKind choice,
            int loot,
            int threat,
            int gate,
            int pressure,
            int extract,
            bool extractReady,
            bool pressureWillCrack)
        {
            Choice = choice;
            Loot = loot;
            Threat = threat;
            Gate = gate;
            Pressure = pressure;
            Extract = extract;
            ExtractReady = extractReady;
            PressureWillCrack = pressureWillCrack;
        }

        public PixelStrategySteamChoiceKind Choice { get; }
        public int Loot { get; }
        public int Threat { get; }
        public int Gate { get; }
        public int Pressure { get; }
        public int Extract { get; }
        public bool ExtractReady { get; }
        public bool PressureWillCrack { get; }
    }

    public sealed class PixelStrategySteamChoicePreview
    {
        public PixelStrategySteamChoicePreview(
            int baseLoot,
            int baseThreat,
            int baseGate,
            int basePressure,
            int baseExtract,
            int extractGateTarget,
            int extractScoreTarget,
            int pressureCrackThreshold,
            IReadOnlyList<PixelStrategySteamChoiceDelta> choices)
        {
            BaseLoot = Mathf.Max(0, baseLoot);
            BaseThreat = Mathf.Max(0, baseThreat);
            BaseGate = Mathf.Max(0, baseGate);
            BasePressure = Mathf.Max(0, basePressure);
            BaseExtract = Mathf.Max(0, baseExtract);
            ExtractGateTarget = Mathf.Max(1, extractGateTarget);
            ExtractScoreTarget = Mathf.Max(1, extractScoreTarget);
            PressureCrackThreshold = Mathf.Max(1, pressureCrackThreshold);
            Choices = choices ?? throw new ArgumentNullException(nameof(choices));
        }

        public int BaseLoot { get; }
        public int BaseThreat { get; }
        public int BaseGate { get; }
        public int BasePressure { get; }
        public int BaseExtract { get; }
        public int ExtractGateTarget { get; }
        public int ExtractScoreTarget { get; }
        public int PressureCrackThreshold { get; }
        public IReadOnlyList<PixelStrategySteamChoiceDelta> Choices { get; }

        public PixelStrategySteamChoiceDelta GetDelta(PixelStrategySteamChoiceKind choice)
        {
            for (var index = 0; index < Choices.Count; index++)
            {
                if (Choices[index].Choice == choice)
                {
                    return Choices[index];
                }
            }

            throw new ArgumentException("Choice delta is not available for " + choice + ".", nameof(choice));
        }

        public PixelStrategySteamChoiceForecast Apply(PixelStrategySteamChoiceKind choice)
        {
            var delta = GetDelta(choice);
            var loot = Mathf.Max(0, BaseLoot + delta.Loot);
            var threat = Mathf.Max(0, BaseThreat + delta.Threat);
            var gate = Mathf.Max(0, BaseGate + delta.Gate);
            var pressure = Mathf.Max(0, BasePressure + delta.Pressure);
            var extract = Mathf.Max(0, BaseExtract + delta.Extract);
            return new PixelStrategySteamChoiceForecast(
                choice,
                loot,
                threat,
                gate,
                pressure,
                extract,
                gate >= ExtractGateTarget && extract >= ExtractScoreTarget,
                pressure >= PressureCrackThreshold);
        }
    }

    public sealed class PixelStrategySteamScreenPreviewState
    {
        public PixelStrategySteamScreenPreviewState(
            PixelStrategyBoardPreviewState board,
            IReadOnlyList<PixelStrategySteamScreenCard> cards,
            PixelStrategySteamChoiceImpact impact,
            PixelStrategyFourfoldIdentityRead identity,
            PixelStrategySteamChoicePreview choicePreview,
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
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            ChoicePreview = choicePreview ?? throw new ArgumentNullException(nameof(choicePreview));
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
        public PixelStrategyFourfoldIdentityRead Identity { get; }
        public PixelStrategySteamChoicePreview ChoicePreview { get; }
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
                    PixelStrategySteamChoiceKind.BaitLair,
                    "BAIT THE CHOSEN",
                    "+620",
                    "82%",
                    "wood stick, bad odds",
                    PixelStrategySteamScreenCardTone.Greedy,
                    selected: false),
                new PixelStrategySteamScreenCard(
                    "B",
                    PixelStrategySteamChoiceKind.CutToGate,
                    "CUT TO GATE",
                    "EXIT 86%",
                    "safe",
                    "selected: spare the fool",
                    PixelStrategySteamScreenCardTone.SafeSelected,
                    selected: true),
                new PixelStrategySteamScreenCard(
                    "C",
                    PixelStrategySteamChoiceKind.GreedRelic,
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

            var identity = new PixelStrategyFourfoldIdentityRead(
                new[] { "BLADE", "GATE", "RELIC", "SEAL" },
                impact.OpenedGateCells,
                new[]
                {
                    new Vector2Int(8, 2),
                    new Vector2Int(10, 4),
                    new Vector2Int(11, 4),
                    new Vector2Int(12, 5)
                },
                new[] { "COIN", "KEY", "SHARD", "SEAL" },
                litSealBeatCount: 1,
                crackedSealBeatCount: 2);

            var choicePreview = new PixelStrategySteamChoicePreview(
                baseLoot: 2,
                baseThreat: 4,
                baseGate: 4,
                basePressure: 5,
                baseExtract: 2,
                extractGateTarget: 6,
                extractScoreTarget: 3,
                pressureCrackThreshold: 7,
                choices: new[]
                {
                    new PixelStrategySteamChoiceDelta(PixelStrategySteamChoiceKind.BaitLair, loot: 1, threat: 2, gate: 0, pressure: 1, extract: 1),
                    new PixelStrategySteamChoiceDelta(PixelStrategySteamChoiceKind.CutToGate, loot: 0, threat: -1, gate: 2, pressure: 2, extract: 1),
                    new PixelStrategySteamChoiceDelta(PixelStrategySteamChoiceKind.GreedRelic, loot: 3, threat: 1, gate: -1, pressure: 2, extract: -1)
                });

            return new PixelStrategySteamScreenPreviewState(
                board,
                cards,
                impact,
                identity,
                choicePreview,
                loopNumber: 7,
                bagValue: 420,
                gatePercent: 68,
                pressurePercent: 82,
                starterWeapon: "WOOD STICK",
                starterCoins: 12);
        }
    }
}
