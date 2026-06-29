using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeBuildGridTests
    {
        [Test]
        public void NewGrid_StartsCenteredWithAvailableBlocks()
        {
            var grid = new BuilderPrototypeBuildGrid(13, 9, 18);

            Assert.That(grid.SelectedCell, Is.EqualTo(new Vector2Int(6, 4)));
            Assert.That(grid.BlocksAvailable, Is.EqualTo(18));
            Assert.That(grid.PlacedBlockCount, Is.EqualTo(0));
            Assert.IsTrue(grid.HasBlocksAvailable);
        }

        [Test]
        public void MoveSelection_ClampsToGridBounds()
        {
            var grid = new BuilderPrototypeBuildGrid(3, 3, 1);

            grid.MoveSelection(-99, -99);
            Assert.That(grid.SelectedCell, Is.EqualTo(new Vector2Int(0, 0)));

            grid.MoveSelection(99, 99);
            Assert.That(grid.SelectedCell, Is.EqualTo(new Vector2Int(2, 2)));
        }

        [Test]
        public void TryPlaceSelected_ConsumesBlockAndRaisesStack()
        {
            var grid = new BuilderPrototypeBuildGrid(3, 3, 2);

            var result = grid.TryPlaceSelected();

            Assert.That(result, Is.EqualTo(BuilderPrototypeBuildResult.Placed));
            Assert.That(grid.BlocksAvailable, Is.EqualTo(1));
            Assert.That(grid.PlacedBlockCount, Is.EqualTo(1));
            Assert.That(grid.HeightAt(grid.SelectedCell), Is.EqualTo(1));
        }

        [Test]
        public void TryPlaceSelected_RespectsAvailableBlocksAndHeightLimit()
        {
            var grid = new BuilderPrototypeBuildGrid(3, 3, 1, maxStackHeight: 1);

            Assert.That(grid.TryPlaceSelected(), Is.EqualTo(BuilderPrototypeBuildResult.Placed));
            Assert.That(grid.TryPlaceSelected(), Is.EqualTo(BuilderPrototypeBuildResult.NoBlocksAvailable));

            var heightLimited = new BuilderPrototypeBuildGrid(3, 3, 2, maxStackHeight: 1);
            Assert.That(heightLimited.TryPlaceSelected(), Is.EqualTo(BuilderPrototypeBuildResult.Placed));
            Assert.That(heightLimited.TryPlaceSelected(), Is.EqualTo(BuilderPrototypeBuildResult.AtHeightLimit));
            Assert.That(heightLimited.BlocksAvailable, Is.EqualTo(1));
        }

        [Test]
        public void TryRemoveSelected_ReturnsBlockAndLowersStack()
        {
            var grid = new BuilderPrototypeBuildGrid(3, 3, 1);

            Assert.That(grid.TryRemoveSelected(), Is.EqualTo(BuilderPrototypeBuildResult.NoPlacedBlock));
            Assert.That(grid.TryPlaceSelected(), Is.EqualTo(BuilderPrototypeBuildResult.Placed));
            Assert.That(grid.TryRemoveSelected(), Is.EqualTo(BuilderPrototypeBuildResult.Removed));

            Assert.That(grid.BlocksAvailable, Is.EqualTo(1));
            Assert.That(grid.PlacedBlockCount, Is.EqualTo(0));
            Assert.That(grid.HeightAt(grid.SelectedCell), Is.EqualTo(0));
        }

        [Test]
        public void TryPlaceAndRemove_ReportOutsideGridForInvalidCells()
        {
            var grid = new BuilderPrototypeBuildGrid(3, 3, 1);
            var outside = new Vector2Int(-1, 3);

            Assert.That(grid.TryPlace(outside), Is.EqualTo(BuilderPrototypeBuildResult.OutsideGrid));
            Assert.That(grid.TryRemove(outside), Is.EqualTo(BuilderPrototypeBuildResult.OutsideGrid));
        }
    }
}
