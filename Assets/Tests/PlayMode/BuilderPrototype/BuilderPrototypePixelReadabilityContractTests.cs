using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypePixelReadabilityContractTests
    {
        [Test]
        public void Default2DHDSpec_MatchesReadableBuilderPrototypeContract()
        {
            var spec = BuilderPrototypePixelReadabilitySpec.Default2DHD();

            var validation = BuilderPrototypePixelReadabilityContract.Validate(spec);

            Assert.IsTrue(validation.IsValid);
            Assert.That(spec.PixelsPerUnit, Is.EqualTo(32));
            Assert.That(spec.TileSizePixels, Is.EqualTo(32));
            Assert.That(spec.TileWorldUnits, Is.EqualTo(1f));
            Assert.That(spec.BlockFootprintPixels, Is.EqualTo(new Vector2Int(32, 32)));
            Assert.That(spec.AvatarFootprintPixels, Is.EqualTo(new Vector2Int(24, 28)));
            Assert.That(spec.ExtractionThresholdLabel, Is.EqualTo("EXT 68%"));
            Assert.That(spec.ExtractionReadoutPixelWidth, Is.GreaterThanOrEqualTo(BuilderPrototypePixelReadabilityContract.MinExtractionReadoutPixelWidth));
            Assert.That(
                BuilderPrototypePixelReadabilityContract.ContrastRatio(spec.TelegraphFill, spec.TelegraphBackground),
                Is.GreaterThanOrEqualTo(BuilderPrototypePixelReadabilityContract.MinTelegraphContrastRatio));
        }

        [Test]
        public void Validate_FlagsEveryReadabilityRegression()
        {
            var invalid = new BuilderPrototypePixelReadabilitySpec(
                pixelsPerUnit: 16,
                tileSizePixels: 48,
                blockFootprintPixels: new Vector2Int(24, 32),
                avatarFootprintPixels: new Vector2Int(12, 36),
                telegraphFill: new Color(0.10f, 0.10f, 0.10f, 1f),
                telegraphBackground: new Color(0.12f, 0.12f, 0.12f, 1f),
                extractionThresholdPercent: 125,
                extractionThresholdLabel: "EXTRACT 125 PERCENT",
                extractionReadoutPixelWidth: 40);

            var validation = BuilderPrototypePixelReadabilityContract.Validate(invalid);

            Assert.IsFalse(validation.IsValid);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.PixelsPerUnit), Is.True);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.TileSizePixels), Is.True);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.BlockFootprint), Is.True);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.AvatarFootprint), Is.True);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.TelegraphContrast), Is.True);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.ExtractionThreshold), Is.True);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.ExtractionLabel), Is.True);
            Assert.That(validation.Contains(BuilderPrototypePixelReadabilityIssueId.ExtractionReadoutWidth), Is.True);
        }

        [Test]
        public void ExtractionThresholdLabel_IsShortStableAndPercentMatched()
        {
            Assert.That(BuilderPrototypePixelReadabilityContract.FormatExtractionThresholdLabel(-4), Is.EqualTo("EXT 0%"));
            Assert.That(BuilderPrototypePixelReadabilityContract.FormatExtractionThresholdLabel(1000), Is.EqualTo("EXT 100%"));
            Assert.IsTrue(BuilderPrototypePixelReadabilityContract.IsValidExtractionThresholdLabel("EXT 7%", 7));
            Assert.IsTrue(BuilderPrototypePixelReadabilityContract.IsValidExtractionThresholdLabel("EXT 100%", 100));
            Assert.IsFalse(BuilderPrototypePixelReadabilityContract.IsValidExtractionThresholdLabel("Extract at 68%", 68));
            Assert.IsFalse(BuilderPrototypePixelReadabilityContract.IsValidExtractionThresholdLabel("EXT 67%", 68));
            Assert.IsFalse(BuilderPrototypePixelReadabilityContract.IsValidExtractionThresholdLabel("EXT\n68%", 68));
        }
    }
}
