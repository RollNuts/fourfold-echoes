using System;
using System.IO;
using FourfoldEchoes.Product;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldD022ProductContractVerifier
    {
        private const int DeckWidth = 1280;
        private const int DeckHeight = 800;
        private const int DesktopWidth = 1920;
        private const int DesktopHeight = 1080;

        private static readonly string[] RequiredDocs =
        {
            "docs/Production/D022_TOPDOWN_ADVENTURE_MVP/00_IMPLEMENTATION_SPEC.md",
            "docs/Production/D022_TOPDOWN_ADVENTURE_MVP/01_CORE_SYSTEMS.md",
            "docs/Production/D022_TOPDOWN_ADVENTURE_MVP/02_ART_AUDIO_DIRECTION.md",
            "docs/Production/D022_TOPDOWN_ADVENTURE_MVP/03_VERTICAL_SLICE_AND_SCOPE.md",
            "docs/Production/D022_TOPDOWN_ADVENTURE_MVP/04_QA_RELEASE_STORE.md"
        };

        private static readonly string[] UiCopyFiles =
        {
            "Assets/Scripts/TitleSceneController.cs",
            "Assets/Scripts/HubSceneController.cs",
            "Assets/Scripts/D020SliceController.cs",
            "Assets/Scripts/FourfoldInputPrompts.cs"
        };

        private const string InputPromptFile = "Assets/Scripts/FourfoldInputPrompts.cs";

        private static readonly string[] ForbiddenPlayerFacingCopy =
        {
            "hack-and-slash",
            "hack and slash",
            "extraction",
            "co-op",
            "co op",
            "coop",
            "open world",
            "open-world",
            "inventory",
            "crafting",
            "quest log",
            "Boss-run fantasy action RPG",
            "D-020 RUN IN PROGRESS",
            "D-020: FIRST ECHO RUN",
            "unbanked",
            "unconfirmed reward",
            "confirmed rewards",
            "confirmed progress",
            "E/Enter/Y",
            "E/Y",
            "E / Y",
            "Backspace/Select",
            "R/Start",
            "Enter/A",
            "Esc/B",
            "Esc/Menu",
            "arrows/stick",
            "Stick/WASD",
            "A/Space",
            "B/Shift",
            "X/Q",
            "Y/E",
            "Menu/Esc",
            "gamepad X",
            "tool node",
            "second node",
            "unbanked relic",
            "bank rewards",
            "drops unbanked"
        };

        public static void VerifyD022Contract()
        {
            VerifyRequiredDocs();
            VerifyD022PackContent();
            VerifyAgentsReferences();
            VerifyUiLayouts();
            VerifyRuntimeUiCopyContract();
            VerifyPlayerFacingCopy();
        }

        private static void VerifyRequiredDocs()
        {
            foreach (var relativePath in RequiredDocs)
            {
                if (!File.Exists(relativePath))
                {
                    throw new InvalidOperationException($"D022 contract missing required document: {relativePath}");
                }

                if (new FileInfo(relativePath).Length == 0)
                {
                    throw new InvalidOperationException($"D022 contract required document is empty: {relativePath}");
                }
            }
        }

        private static void VerifyD022PackContent()
        {
            var implementation = Read(RequiredDocs[0]);
            RequireContains(implementation, "Status: current sole product specification.", "D022 current spec status");
            RequireContains(implementation, "Steam-first, buy-to-own, single-player top-down classic", "D022 product direction");
            RequireContains(implementation, "The MVP promise is exactly 1 hub, 3 regions, 4 bosses, and 1 exploration tool.", "D022 MVP cap");
            RequireContains(implementation, "No open world", "D022 open-world rejection");
            RequireContains(implementation, "Inventory / crafting / quest log / social systems | 0", "D022 UI system exclusions");

            var systems = Read(RequiredDocs[1]);
            RequireContains(systems, "One exploration tool", "D022 one-tool system");
            RequireContains(systems, "UI/UX | title, HUD, boss HP, prompts, pause, settings, retry, result", "D022 UI/UX system scope");
            RequireContains(systems, "Validation | 1280x800, controller, save, crash, boss progression", "D022 validation system scope");

            var artAudio = Read(RequiredDocs[2]);
            RequireContains(artAudio, "Top-down camera distance decides the art.", "D022 top-down art contract");
            RequireContains(artAudio, "No market-facing gray boxes", "D022 market art bar");

            var slice = Read(RequiredDocs[3]);
            RequireContains(slice, "hub 1, exploration area 1, normal enemies 2, miniboss 1, boss 1, tool 1", "D022 slice cap");
            RequireContains(slice, "1280x800", "D022 Deck layout gate");

            var qa = Read(RequiredDocs[4]);
            RequireContains(qa, "1920x1080", "D022 desktop capture gate");
            RequireContains(qa, "Store | copy describes implemented features only", "D022 store copy honesty gate");
        }

        private static void VerifyAgentsReferences()
        {
            var agents = Read("AGENTS.md");
            RequireContains(agents, "D022_TOPDOWN_ADVENTURE_MVP", "AGENTS D022 source-of-truth folder");
            RequireContains(agents, "New work must use the D022 pack", "AGENTS D022-only work rule");
            foreach (var relativePath in RequiredDocs)
            {
                RequireContains(agents, relativePath, "AGENTS D022 required document reference");
            }
        }

        private static void VerifyUiLayouts()
        {
            Require(TitleSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var titleDeckReason), titleDeckReason);
            Require(TitleSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var titleSettingsDeckReason), titleSettingsDeckReason);
            Require(TitleSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var titleDesktopReason), titleDesktopReason);
            Require(TitleSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, true, out var titleSettingsDesktopReason), titleSettingsDesktopReason);

            Require(HubSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var hubDeckReason), hubDeckReason);
            Require(HubSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var hubPauseDeckReason), hubPauseDeckReason);
            Require(HubSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var hubDesktopReason), hubDesktopReason);
            Require(HubSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, true, out var hubPauseDesktopReason), hubPauseDesktopReason);

            Require(D020SliceController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var regionDeckReason), regionDeckReason);
            Require(D020SliceController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var regionPauseDeckReason), regionPauseDeckReason);
            Require(D020SliceController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var regionDesktopReason), regionDesktopReason);
            Require(D020SliceController.LayoutFitsResolution(DesktopWidth, DesktopHeight, true, out var regionPauseDesktopReason), regionPauseDesktopReason);
        }

        private static void VerifyRuntimeUiCopyContract()
        {
            var titleCopy = Read("Assets/Scripts/TitleSceneController.cs");
            RequireContains(titleCopy, "Hub prep -> Region 01 run -> rewards -> hub result", "Title loop summary");
            RequireContains(titleCopy, "New run: prepare in the hub, clear Region 01", "Title new-save summary");
            RequireContains(titleCopy, "REGION ATTEMPT IN PROGRESS", "Title in-progress run choice");
            RequireContains(titleCopy, "Saved skills:", "Title saved reward skill summary");
            RequireContains(titleCopy, "Equipped:", "Title equipped build summary");
            RequireContains(titleCopy, "FourfoldUiAudio.PlaySelect", "Title menu select SFX");
            RequireContains(titleCopy, "FourfoldUiAudio.PlayConfirm", "Title menu confirm SFX");
            RequireContains(titleCopy, "FourfoldUiAudio.PlayBack", "Title menu back SFX");

            var hubCopy = Read("Assets/Scripts/HubSceneController.cs");
            RequireContains(hubCopy, "HUB: Crossroads", "Hub identity");
            RequireContains(hubCopy, "RUN PLAN: tune loadout, open route, beat boss, claim skills, return.", "Hub run plan");
            RequireContains(hubCopy, "REGION 01: VERDANT STEPS", "R01 product region name");
            RequireContains(hubCopy, "Goal: use the exploration tool, defeat the boss, claim two reward skills, and return to the hub to save the result.", "Hub mission briefing");
            RequireContains(hubCopy, "LOADOUT", "Hub loadout panel");
            RequireContains(hubCopy, "Equipped reward skills", "Hub equipped reward HUD");
            RequireContains(hubCopy, "Current synergy: Lumen Link = Edge + Ward", "Hub reward-skill synergy briefing");
            RequireContains(hubCopy, "Loss risk: new R01 rewards save on hub return; fail or leave before return loses them.", "Hub R01 reward-loss risk briefing");
            RequireContains(hubCopy, "REGION CLEARED", "Hub result summary");
            RequireContains(hubCopy, "Last clear", "Hub last-clear result timing");
            RequireContains(hubCopy, "NEW BEST", "Hub new-best result timing");
            RequireContains(hubCopy, "ATTEMPT LOST", "Hub failed-return summary");
            RequireContains(hubCopy, "Hub-saved skills remain safe", "Hub failed-return saved skill safety copy");
            RequireContains(hubCopy, "RESET SAVE?", "Hub reset confirmation");
            RequireContains(hubCopy, "FourfoldUiAudio.PlaySelect", "Hub menu select SFX");
            RequireContains(hubCopy, "FourfoldUiAudio.PlayConfirm", "Hub menu confirm SFX");
            RequireContains(hubCopy, "FourfoldUiAudio.PlayBack", "Hub menu back SFX");
            RequireContains(hubCopy, "FourfoldUiAudio.PlayError", "Hub locked-loadout error SFX");
            RequireContains(hubCopy, "FourfoldUiAudio.PlayPause", "Hub pause SFX");

            var uiAudio = Read("Assets/Scripts/FourfoldUiAudio.cs");
            RequireContains(uiAudio, "PlaySelect", "UI select audio helper");
            RequireContains(uiAudio, "PlayConfirm", "UI confirm audio helper");
            RequireContains(uiAudio, "PlayBack", "UI back audio helper");
            RequireContains(uiAudio, "PlayError", "UI error audio helper");
            RequireContains(uiAudio, "PlayPause", "UI pause audio helper");
            RequireContains(uiAudio, "masterVolume", "UI audio respects master volume");
            RequireContains(uiAudio, "sfxVolume", "UI audio respects SFX volume");

            var regionCopy = Read("Assets/Scripts/D020SliceController.cs");
            var inputPrompts = Read(InputPromptFile);
            RequireContains(regionCopy, "Step 1/6", "Region objective step 1");
            RequireContains(regionCopy, "Step 2/6", "Region objective step 2");
            RequireContains(inputPrompts, "Step 3/6", "Region objective step 3");
            RequireContains(regionCopy, "Step 4/6", "Region objective step 4");
            RequireContains(inputPrompts, "Step 5/6", "Region objective step 5");
            RequireContains(regionCopy, "Step 6/6", "Region objective step 6");
            RequireContains(regionCopy, "use the tool to open the sealed route", "Region tool route cause/effect");
            RequireContains(regionCopy, "use the tool to open the shortcut seal", "Region shortcut cause/effect");
            RequireContains(regionCopy, "BOSS HP", "Region boss HP UI");
            RequireContains(regionCopy, "BOSS DOWN", "Region boss defeat beat");
            RequireContains(regionCopy, "BOSS TOOL", "Region boss tool opportunity objective");
            RequireContains(regionCopy, "BOSS OPEN", "Region boss opening objective");
            RequireContains(regionCopy, "BOSS OPENING", "Region boss tool-opening beat");
            RequireContains(regionCopy, "Tool opening active. Attack now.", "Region boss opening HUD hint");
            RequireContains(regionCopy, "AT RISK: return to hub to save", "Region reward risk UI");
            RequireContains(regionCopy, "Failed before hub return:", "Region failed-return reward loss UI");
            RequireContains(regionCopy, "ATTEMPT FAILED", "Region failure result UI");
            RequireContains(regionCopy, "Lumen Link", "Region combined reward-skill synergy");
            RequireContains(regionCopy, "FourfoldUiAudio.PlaySelect", "Region menu select SFX");
            RequireContains(regionCopy, "FourfoldUiAudio.PlayConfirm", "Region menu confirm SFX");
            RequireContains(regionCopy, "FourfoldUiAudio.PlayBack", "Region menu back SFX");
            RequireContains(regionCopy, "FourfoldUiAudio.PlayPause", "Region pause SFX");

            RequireContains(inputPrompts, "PreferGamepad", "Input prompt mode split");
            RequireContains(inputPrompts, "Move: D-pad/Left Stick", "Title controller prompt");
            RequireContains(inputPrompts, "Move: arrows", "Title keyboard prompt");
            RequireContains(inputPrompts, "Move Left Stick/D-pad", "Region controller movement prompt");
            RequireContains(inputPrompts, "Move WASD or arrows", "Region keyboard movement prompt");
            RequireContains(inputPrompts, "Attack A", "Region controller attack prompt");
            RequireContains(inputPrompts, "Attack Space", "Region keyboard attack prompt");
            RequireContains(inputPrompts, "Tool X", "Region controller tool prompt");
            RequireContains(inputPrompts, "Boss close: press X", "Region controller boss tool prompt");
            RequireContains(inputPrompts, "Boss close: press Q", "Region keyboard boss tool prompt");
            RequireContains(inputPrompts, "Press Start, A, or Y", "Region controller retry prompt");
            RequireContains(inputPrompts, "HubStartReady", "Hub start prompt helper");
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
                        throw new InvalidOperationException($"D022 UI copy contract failed: {file} still contains stale player-facing copy: {forbidden}");
                    }
                }
            }
        }

        private static string Read(string relativePath)
        {
            if (!File.Exists(relativePath))
            {
                throw new InvalidOperationException($"D022 contract file is missing: {relativePath}");
            }

            return File.ReadAllText(relativePath);
        }

        private static void RequireContains(string text, string required, string label)
        {
            if (text.IndexOf(required, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException($"D022 contract missing {label}: {required}");
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
