using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldSteamDeckReadinessVerifier
    {
        private const int DeckWidth = 1280;
        private const int DeckHeight = 800;
        private const int DesktopWidth = 1920;
        private const int DesktopHeight = 1080;

        public static void VerifyProductLoopReadiness()
        {
            VerifyLayouts();
            VerifyInputManagerAxes();
            VerifyTitleBindings();
            VerifyHubBindings();
            VerifyD020Bindings();
            Debug.Log("FOURFOLD Steam Deck readiness verifier passed: 1280x800/1080p HUD layouts and controller-critical bindings are present.");
        }

        private static void VerifyLayouts()
        {
            Require(TitleSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var titleDeckReason), titleDeckReason);
            Require(TitleSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var titleSettingsDeckReason), titleSettingsDeckReason);
            Require(TitleSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var titleDesktopReason), titleDesktopReason);

            Require(HubSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var hubDeckReason), hubDeckReason);
            Require(HubSceneController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var hubPauseDeckReason), hubPauseDeckReason);
            Require(HubSceneController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var hubDesktopReason), hubDesktopReason);

            Require(D020SliceController.LayoutFitsResolution(DeckWidth, DeckHeight, false, out var d020DeckReason), d020DeckReason);
            Require(D020SliceController.LayoutFitsResolution(DeckWidth, DeckHeight, true, out var d020PauseDeckReason), d020PauseDeckReason);
            Require(D020SliceController.LayoutFitsResolution(DesktopWidth, DesktopHeight, false, out var d020DesktopReason), d020DesktopReason);
        }

        private static void VerifyInputManagerAxes()
        {
            var inputManagerPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ProjectSettings", "InputManager.asset"));
            if (!File.Exists(inputManagerPath))
            {
                throw new InvalidOperationException("Steam Deck readiness verifier failed: ProjectSettings/InputManager.asset is missing.");
            }

            var inputManager = File.ReadAllText(inputManagerPath);
            Require(inputManager.IndexOf("m_Name: Horizontal", StringComparison.Ordinal) >= 0, "Steam Deck readiness verifier failed: legacy Horizontal axis is missing.");
            Require(inputManager.IndexOf("m_Name: Vertical", StringComparison.Ordinal) >= 0, "Steam Deck readiness verifier failed: legacy Vertical axis is missing.");
        }

        private static void VerifyTitleBindings()
        {
            FourfoldTitleSceneBuilder.ValidateGeneratedScene();
            var controller = RequireController<TitleSceneController>("Title Runtime Hook");
            RequireKey(controller.gamepadConfirmKey, "Title confirm gamepad binding");
            RequireKey(controller.gamepadCancelKey, "Title cancel gamepad binding");
            RequireKey(controller.confirmKey, "Title keyboard confirm binding");
            RequireKey(controller.cancelKey, "Title keyboard cancel binding");
        }

        private static void VerifyHubBindings()
        {
            FourfoldHubSceneBuilder.ValidateGeneratedScene();
            var controller = RequireController<HubSceneController>("Hub Runtime Hook");
            RequireKey(controller.gamepadInteractKey, "Hub interact gamepad binding");
            RequireKey(controller.gamepadResetKey, "Hub reset gamepad binding");
            RequireKey(controller.gamepadPauseKey, "Hub pause gamepad binding");
            RequireKey(controller.interactKey, "Hub interact keyboard binding");
            RequireKey(controller.pauseKey, "Hub pause keyboard binding");
        }

        private static void VerifyD020Bindings()
        {
            FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();
            var controller = RequireController<D020SliceController>("D020 Runtime Hook");
            RequireKey(controller.gamepadAttackKey, "D-020 attack gamepad binding");
            RequireKey(controller.gamepadDodgeKey, "D-020 dodge gamepad binding");
            RequireKey(controller.gamepadInteractKey, "D-020 interact gamepad binding");
            RequireKey(controller.gamepadRetryKey, "D-020 retry gamepad binding");
            RequireKey(controller.gamepadPauseKey, "D-020 pause gamepad binding");
            RequireKey(controller.gamepadReturnToTitleKey, "D-020 return-to-title gamepad binding");
            RequireKey(controller.attackKey, "D-020 attack keyboard binding");
            RequireKey(controller.dodgeKey, "D-020 dodge keyboard binding");
            RequireKey(controller.interactKey, "D-020 interact keyboard binding");
            RequireKey(controller.retryKey, "D-020 retry keyboard binding");
            RequireKey(controller.pauseKey, "D-020 pause keyboard binding");
        }

        private static T RequireController<T>(string objectName) where T : Component
        {
            var gameObject = GameObject.Find(objectName);
            if (gameObject == null)
            {
                throw new InvalidOperationException($"Steam Deck readiness verifier failed: required object is missing: {objectName}.");
            }

            var controller = gameObject.GetComponent<T>();
            if (controller == null)
            {
                throw new InvalidOperationException($"Steam Deck readiness verifier failed: {typeof(T).Name} is missing on {objectName}.");
            }

            return controller;
        }

        private static void RequireKey(KeyCode key, string label)
        {
            Require(key != KeyCode.None, $"Steam Deck readiness verifier failed: {label} is unset.");
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
