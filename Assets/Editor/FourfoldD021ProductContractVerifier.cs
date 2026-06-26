using System;
using System.IO;
using FourfoldEchoes.Product;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldD021ProductContractVerifier
    {
        private const int DeckWidth = 1280;
        private const int DeckHeight = 800;
        private const int DesktopWidth = 1920;
        private const int DesktopHeight = 1080;

        private static readonly string[] RequiredDocs =
        {
            "docs/Production/D021_COMPACT_ACTION_SPEC_PACK/00_CANON.md",
            "docs/Production/D021_COMPACT_ACTION_SPEC_PACK/01_ARCHITECTURE.md",
            "docs/Production/D021_COMPACT_ACTION_SPEC_PACK/02_OUT_OF_SCOPE.md",
            "docs/Production/D021_COMPACT_ACTION_SPEC_PACK/03_ART_AUDIO_UI.md",
            "docs/Production/D021_COMPACT_ACTION_SPEC_PACK/04_VERTICAL_SLICE_PLAN.md",
            "docs/Production/D021_COMPACT_ACTION_SPEC_PACK/05_SCOPE_AND_RELEASE.md",
            "docs/Production/D021_COMPACT_ACTION_SPEC_PACK/06_STEAM_STORE_PLAN.md"
        };

        private static readonly string[] UiCopyFiles =
        {
            "Assets/Scripts/TitleSceneController.cs",
            "Assets/Scripts/HubSceneController.cs",
            "Assets/Scripts/D020SliceController.cs"
        };

        private static readonly string[] ForbiddenPlayerFacingCopy =
        {
            "Boss-run fantasy action RPG",
            "D-020 RUN IN PROGRESS",
            "D-020: FIRST ECHO RUN",
            "unbanked",
            "unconfirmed reward",
            "unbanked relic",
            "bank rewards",
            "drops unbanked",
            "hack-and-slash",
            "extraction"
        };

        public static void VerifyD021Contract()
        {
            VerifyRequiredDocs();
            VerifyCanonContent();
            VerifyUiLayouts();
            VerifyPlayerFacingCopy();
        }

        private static void VerifyRequiredDocs()
        {
            foreach (var relativePath in RequiredDocs)
            {
                if (!File.Exists(relativePath))
                {
                    throw new InvalidOperationException($"D021 contract missing required document: {relativePath}");
                }
            }
        }

        private static void VerifyCanonContent()
        {
            var canon = Read("docs/Production/D021_COMPACT_ACTION_SPEC_PACK/00_CANON.md");
            RequireContains(canon, "current D021 source of truth", "D021 canon status");
            RequireContains(canon, "1 hub", "D021 hub cap");
            RequireContains(canon, "3 handcrafted regions", "D021 region cap");
            RequireContains(canon, "4 bosses", "D021 boss cap");
            RequireContains(canon, "1 exploration tool", "D021 tool cap");
            RequireContains(canon, "UI/UX is product scope", "D021 UI/UX contract");

            var agents = Read("AGENTS.md");
            RequireContains(agents, "D021_COMPACT_ACTION_SPEC_PACK", "AGENTS D021 source of truth");
        }

        private static void VerifyUiLayouts()
        {
            Require(TitleSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var titleDeckReason), titleDeckReason);
            Require(TitleSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var titleSettingsDeckReason), titleSettingsDeckReason);
            Require(TitleSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var titleDesktopReason), titleDesktopReason);

            Require(HubSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var hubDeckReason), hubDeckReason);
            Require(HubSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var hubPauseDeckReason), hubPauseDeckReason);
            Require(HubSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var hubDesktopReason), hubDesktopReason);

            Require(D020SliceController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var regionDeckReason), regionDeckReason);
            Require(D020SliceController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var regionPauseDeckReason), regionPauseDeckReason);
            Require(D020SliceController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var regionDesktopReason), regionDesktopReason);
        }

        private static void VerifyPlayerFacingCopy()
        {
            foreach (var file in UiCopyFiles)
            {
                var text = Read(file);
                foreach (var forbidden in ForbiddenPlayerFacingCopy)
                {
                    if (text.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new InvalidOperationException($"D021 UI copy contract failed: {file} still contains stale player-facing copy: {forbidden}");
                    }
                }
            }
        }

        private static string Read(string relativePath)
        {
            if (!File.Exists(relativePath))
            {
                throw new InvalidOperationException($"D021 contract file is missing: {relativePath}");
            }

            return File.ReadAllText(relativePath);
        }

        private static void RequireContains(string text, string required, string label)
        {
            if (text.IndexOf(required, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException($"D021 contract missing {label}: {required}");
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
