using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.StrategyLoop
{
    public enum PixelStrategyBoardPreviewCellKind
    {
        Empty,
        Route,
        Lair,
        Hazard,
        Reward,
        Extract
    }

    public readonly struct PixelStrategyBoardPreviewCell
    {
        public PixelStrategyBoardPreviewCell(Vector2Int cell, PixelStrategyBoardPreviewCellKind kind)
        {
            Cell = cell;
            Kind = kind;
        }

        public Vector2Int Cell { get; }
        public PixelStrategyBoardPreviewCellKind Kind { get; }
    }

    public sealed class PixelStrategyBoardPreviewState
    {
        internal PixelStrategyBoardPreviewState(
            int width,
            int height,
            PixelStrategyLoopRoute route,
            IReadOnlyList<PixelStrategyPlacement> placements,
            Vector2Int heroCell,
            Vector2Int extractCell,
            PixelStrategyRunSnapshot run)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Placements = placements ?? throw new ArgumentNullException(nameof(placements));
            HeroCell = heroCell;
            ExtractCell = extractCell;
            Run = run;
        }

        public int Width { get; }
        public int Height { get; }
        public PixelStrategyLoopRoute Route { get; }
        public IReadOnlyList<PixelStrategyPlacement> Placements { get; }
        public Vector2Int HeroCell { get; }
        public Vector2Int ExtractCell { get; }
        public PixelStrategyRunSnapshot Run { get; }

        public PixelStrategyBoardPreviewCellKind GetCellKind(Vector2Int cell)
        {
            if (cell == ExtractCell)
            {
                return PixelStrategyBoardPreviewCellKind.Extract;
            }

            for (var index = 0; index < Placements.Count; index++)
            {
                var placement = Placements[index];
                if (placement.Cell != cell)
                {
                    continue;
                }

                switch (placement.Kind)
                {
                    case PixelStrategyPlacementKind.Lair:
                        return PixelStrategyBoardPreviewCellKind.Lair;
                    case PixelStrategyPlacementKind.Hazard:
                        return PixelStrategyBoardPreviewCellKind.Hazard;
                    case PixelStrategyPlacementKind.RewardCache:
                        return PixelStrategyBoardPreviewCellKind.Reward;
                }
            }

            return Route.Contains(cell)
                ? PixelStrategyBoardPreviewCellKind.Route
                : PixelStrategyBoardPreviewCellKind.Empty;
        }

        public IReadOnlyList<PixelStrategyBoardPreviewCell> BuildCells()
        {
            var cells = new List<PixelStrategyBoardPreviewCell>(Width * Height);
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    cells.Add(new PixelStrategyBoardPreviewCell(cell, GetCellKind(cell)));
                }
            }

            return cells;
        }
    }

    public static class PixelStrategyBoardPreviewFactory
    {
        public static PixelStrategyBoardPreviewState CreateStreamerReadableSample()
        {
            var route = new PixelStrategyLoopRoute(new[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1),
                new Vector2Int(4, 1),
                new Vector2Int(4, 2),
                new Vector2Int(4, 3),
                new Vector2Int(3, 3),
                new Vector2Int(2, 3),
                new Vector2Int(1, 3),
                new Vector2Int(1, 2)
            });

            var placements = new List<PixelStrategyPlacement>
            {
                PixelStrategyPlacement.Lair(new Vector2Int(3, 2), threat: 2, loot: 1, damage: 0),
                PixelStrategyPlacement.Hazard(new Vector2Int(4, 2), threat: 1, damage: 1),
                PixelStrategyPlacement.RewardCache(new Vector2Int(1, 2), loot: 4, bagPressure: 3)
            };

            var config = new PixelStrategyRunConfig(
                maxLoops: 3,
                heroHealth: 12,
                bagCapacity: 12,
                extractLootTarget: 6,
                extractPressureThreshold: 6,
                retreatThreatThreshold: 20);

            return new PixelStrategyBoardPreviewState(
                width: 6,
                height: 5,
                route,
                placements,
                heroCell: new Vector2Int(2, 1),
                extractCell: new Vector2Int(4, 1),
                PixelStrategyLoopSimulator.Simulate(route, placements, config));
        }
    }
}
