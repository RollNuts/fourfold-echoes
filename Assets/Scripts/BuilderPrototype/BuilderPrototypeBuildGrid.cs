using UnityEngine;

namespace FourfoldEchoes.BuilderPrototype
{
    public enum BuilderPrototypeBuildResult
    {
        Placed,
        Removed,
        NoBlocksAvailable,
        NoPlacedBlock,
        AtHeightLimit,
        OutsideGrid
    }

    public sealed class BuilderPrototypeBuildGrid
    {
        public const int DefaultWidth = 13;
        public const int DefaultDepth = 9;
        public const int DefaultMaxStackHeight = 3;

        private readonly int[,] stackHeights;

        public BuilderPrototypeBuildGrid(int width, int depth, int startingBlocks, int maxStackHeight = DefaultMaxStackHeight)
        {
            Width = Mathf.Max(1, width);
            Depth = Mathf.Max(1, depth);
            MaxStackHeight = Mathf.Max(1, maxStackHeight);
            BlocksAvailable = Mathf.Max(0, startingBlocks);
            SelectedCell = new Vector2Int(Width / 2, Depth / 2);
            stackHeights = new int[Width, Depth];
        }

        public int Width { get; }
        public int Depth { get; }
        public int MaxStackHeight { get; }
        public int BlocksAvailable { get; private set; }
        public int PlacedBlockCount { get; private set; }
        public Vector2Int SelectedCell { get; private set; }

        public bool HasBlocksAvailable => BlocksAvailable > 0;

        public int HeightAt(Vector2Int cell)
        {
            return IsInside(cell) ? stackHeights[cell.x, cell.y] : 0;
        }

        public bool IsInside(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Depth;
        }

        public void MoveSelection(int xDelta, int zDelta)
        {
            SelectedCell = new Vector2Int(
                Mathf.Clamp(SelectedCell.x + xDelta, 0, Width - 1),
                Mathf.Clamp(SelectedCell.y + zDelta, 0, Depth - 1));
        }

        public BuilderPrototypeBuildResult TryPlaceSelected()
        {
            return TryPlace(SelectedCell);
        }

        public BuilderPrototypeBuildResult TryRemoveSelected()
        {
            return TryRemove(SelectedCell);
        }

        public BuilderPrototypeBuildResult TryPlace(Vector2Int cell)
        {
            if (!IsInside(cell))
            {
                return BuilderPrototypeBuildResult.OutsideGrid;
            }

            if (BlocksAvailable <= 0)
            {
                return BuilderPrototypeBuildResult.NoBlocksAvailable;
            }

            if (stackHeights[cell.x, cell.y] >= MaxStackHeight)
            {
                return BuilderPrototypeBuildResult.AtHeightLimit;
            }

            stackHeights[cell.x, cell.y]++;
            BlocksAvailable--;
            PlacedBlockCount++;
            return BuilderPrototypeBuildResult.Placed;
        }

        public BuilderPrototypeBuildResult TryRemove(Vector2Int cell)
        {
            if (!IsInside(cell))
            {
                return BuilderPrototypeBuildResult.OutsideGrid;
            }

            if (stackHeights[cell.x, cell.y] <= 0)
            {
                return BuilderPrototypeBuildResult.NoPlacedBlock;
            }

            stackHeights[cell.x, cell.y]--;
            BlocksAvailable++;
            PlacedBlockCount--;
            return BuilderPrototypeBuildResult.Removed;
        }
    }
}
