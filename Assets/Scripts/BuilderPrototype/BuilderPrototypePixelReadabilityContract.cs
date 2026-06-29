using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.BuilderPrototype
{
    public enum BuilderPrototypePixelReadabilityIssueId
    {
        PixelsPerUnit,
        TileSizePixels,
        BlockFootprint,
        AvatarFootprint,
        TelegraphContrast,
        ExtractionThreshold,
        ExtractionLabel,
        ExtractionReadoutWidth
    }

    public readonly struct BuilderPrototypePixelReadabilityIssue
    {
        public BuilderPrototypePixelReadabilityIssue(BuilderPrototypePixelReadabilityIssueId id, string message)
        {
            Id = id;
            Message = string.IsNullOrWhiteSpace(message) ? id.ToString() : message;
        }

        public BuilderPrototypePixelReadabilityIssueId Id { get; }
        public string Message { get; }
    }

    public sealed class BuilderPrototypePixelReadabilityValidation
    {
        internal BuilderPrototypePixelReadabilityValidation(IReadOnlyList<BuilderPrototypePixelReadabilityIssue> issues)
        {
            Issues = issues;
        }

        public IReadOnlyList<BuilderPrototypePixelReadabilityIssue> Issues { get; }
        public bool IsValid => Issues.Count == 0;

        public bool Contains(BuilderPrototypePixelReadabilityIssueId issueId)
        {
            for (var index = 0; index < Issues.Count; index++)
            {
                if (Issues[index].Id == issueId)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class BuilderPrototypePixelReadabilitySpec
    {
        public BuilderPrototypePixelReadabilitySpec(
            int pixelsPerUnit,
            int tileSizePixels,
            Vector2Int blockFootprintPixels,
            Vector2Int avatarFootprintPixels,
            Color telegraphFill,
            Color telegraphBackground,
            int extractionThresholdPercent,
            string extractionThresholdLabel,
            int extractionReadoutPixelWidth)
        {
            PixelsPerUnit = pixelsPerUnit;
            TileSizePixels = tileSizePixels;
            BlockFootprintPixels = blockFootprintPixels;
            AvatarFootprintPixels = avatarFootprintPixels;
            TelegraphFill = telegraphFill;
            TelegraphBackground = telegraphBackground;
            ExtractionThresholdPercent = extractionThresholdPercent;
            ExtractionThresholdLabel = extractionThresholdLabel ?? string.Empty;
            ExtractionReadoutPixelWidth = extractionReadoutPixelWidth;
        }

        public int PixelsPerUnit { get; }
        public int TileSizePixels { get; }
        public Vector2Int BlockFootprintPixels { get; }
        public Vector2Int AvatarFootprintPixels { get; }
        public Color TelegraphFill { get; }
        public Color TelegraphBackground { get; }
        public int ExtractionThresholdPercent { get; }
        public string ExtractionThresholdLabel { get; }
        public int ExtractionReadoutPixelWidth { get; }
        public float TileWorldUnits => PixelsPerUnit <= 0 ? 0f : (float)TileSizePixels / PixelsPerUnit;

        public static BuilderPrototypePixelReadabilitySpec Default2DHD()
        {
            const int threshold = 68;
            return new BuilderPrototypePixelReadabilitySpec(
                BuilderPrototypePixelReadabilityContract.RequiredPixelsPerUnit,
                BuilderPrototypePixelReadabilityContract.RequiredTileSizePixels,
                BuilderPrototypePixelReadabilityContract.RequiredBlockFootprintPixels,
                new Vector2Int(24, 28),
                new Color(1f, 0.22f, 0.12f, 0.86f),
                new Color(0.08f, 0.10f, 0.13f, 1f),
                threshold,
                BuilderPrototypePixelReadabilityContract.FormatExtractionThresholdLabel(threshold),
                64);
        }
    }

    public static class BuilderPrototypePixelReadabilityContract
    {
        public const int RequiredPixelsPerUnit = 32;
        public const int RequiredTileSizePixels = 32;
        public const int MinExtractionReadoutPixelWidth = 56;
        public const int MaxExtractionLabelCharacters = 8;
        public const float MinTelegraphContrastRatio = 3f;

        public static readonly Vector2Int RequiredBlockFootprintPixels = new Vector2Int(32, 32);
        public static readonly Vector2Int MinAvatarFootprintPixels = new Vector2Int(18, 20);
        public static readonly Vector2Int MaxAvatarFootprintPixels = new Vector2Int(28, 32);

        public static BuilderPrototypePixelReadabilityValidation Validate(BuilderPrototypePixelReadabilitySpec spec)
        {
            if (spec == null)
            {
                throw new ArgumentNullException(nameof(spec));
            }

            var issues = new List<BuilderPrototypePixelReadabilityIssue>();

            if (spec.PixelsPerUnit != RequiredPixelsPerUnit)
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.PixelsPerUnit, "Builder prototype pixel art must use 32 pixels per Unity unit.");
            }

            if (spec.TileSizePixels != RequiredTileSizePixels)
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.TileSizePixels, "Builder prototype tiles must be authored as 32x32 pixel cells.");
            }

            if (spec.BlockFootprintPixels != RequiredBlockFootprintPixels)
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.BlockFootprint, "Build blocks must occupy one full 32x32 pixel tile.");
            }

            if (spec.AvatarFootprintPixels.x < MinAvatarFootprintPixels.x ||
                spec.AvatarFootprintPixels.y < MinAvatarFootprintPixels.y ||
                spec.AvatarFootprintPixels.x > MaxAvatarFootprintPixels.x ||
                spec.AvatarFootprintPixels.y > MaxAvatarFootprintPixels.y)
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.AvatarFootprint, "Avatar footprint must stay inside one tile while remaining large enough to read facing.");
            }

            if (ContrastRatio(spec.TelegraphFill, spec.TelegraphBackground) < MinTelegraphContrastRatio)
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.TelegraphContrast, "Combat telegraphs need at least 3:1 contrast against the playfield.");
            }

            if (spec.ExtractionThresholdPercent < 0 || spec.ExtractionThresholdPercent > 100)
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.ExtractionThreshold, "Extraction threshold must be a whole percent from 0 to 100.");
            }

            if (!IsValidExtractionThresholdLabel(spec.ExtractionThresholdLabel, spec.ExtractionThresholdPercent))
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.ExtractionLabel, "Extraction threshold label must be short, single-line, and match the percent readout.");
            }

            if (spec.ExtractionReadoutPixelWidth < MinExtractionReadoutPixelWidth)
            {
                AddIssue(issues, BuilderPrototypePixelReadabilityIssueId.ExtractionReadoutWidth, "Extraction readout needs a stable 56px minimum so 100% cannot resize the HUD.");
            }

            return new BuilderPrototypePixelReadabilityValidation(issues.AsReadOnly());
        }

        public static string FormatExtractionThresholdLabel(int thresholdPercent)
        {
            var clamped = Mathf.Clamp(thresholdPercent, 0, 100);
            return "EXT " + clamped + "%";
        }

        public static bool IsValidExtractionThresholdLabel(string label, int thresholdPercent)
        {
            if (string.IsNullOrWhiteSpace(label) ||
                label.Length > MaxExtractionLabelCharacters ||
                label.Contains("\n") ||
                label.Contains("\r"))
            {
                return false;
            }

            return string.Equals(label, FormatExtractionThresholdLabel(thresholdPercent), StringComparison.Ordinal);
        }

        public static float ContrastRatio(Color foreground, Color background)
        {
            var foregroundLuminance = RelativeLuminance(foreground);
            var backgroundLuminance = RelativeLuminance(background);
            var lighter = Mathf.Max(foregroundLuminance, backgroundLuminance);
            var darker = Mathf.Min(foregroundLuminance, backgroundLuminance);
            return (lighter + 0.05f) / (darker + 0.05f);
        }

        private static void AddIssue(
            ICollection<BuilderPrototypePixelReadabilityIssue> issues,
            BuilderPrototypePixelReadabilityIssueId id,
            string message)
        {
            issues.Add(new BuilderPrototypePixelReadabilityIssue(id, message));
        }

        private static float RelativeLuminance(Color color)
        {
            return (0.2126f * LinearizeSrgb(color.r)) +
                (0.7152f * LinearizeSrgb(color.g)) +
                (0.0722f * LinearizeSrgb(color.b));
        }

        private static float LinearizeSrgb(float channel)
        {
            var clamped = Mathf.Clamp01(channel);
            return clamped <= 0.03928f
                ? clamped / 12.92f
                : Mathf.Pow((clamped + 0.055f) / 1.055f, 2.4f);
        }
    }
}
