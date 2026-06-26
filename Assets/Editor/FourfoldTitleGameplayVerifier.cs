using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldTitleGameplayVerifier
    {
        public static void VerifyTitleEntryFlow()
        {
            var savePath = FourfoldProgressSave.SavePath();
            var backupPath = savePath + ".bak";
            byte[] previousSaveBytes = null;
            byte[] previousBackupBytes = null;
            var backupExists = File.Exists(backupPath);

            if (File.Exists(savePath))
            {
                previousSaveBytes = File.ReadAllBytes(savePath);
            }

            if (File.Exists(backupPath))
            {
                previousBackupBytes = File.ReadAllBytes(backupPath);
            }

            try
            {
                DeleteIfExists(savePath);
                DeleteIfExists(backupPath);

                FourfoldTitleSceneBuilder.ValidateGeneratedScene();
                var hook = GameObject.Find("Title Runtime Hook");
                if (hook == null)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: Title Runtime Hook is missing.");
                }

                var controller = hook.GetComponent<TitleSceneController>();
                if (controller == null)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: TitleSceneController is missing.");
                }

                var noSaveSummary = controller.ContinueSummary();
                if (noSaveSummary.IndexOf("defeat the boss", StringComparison.OrdinalIgnoreCase) < 0 || noSaveSummary.IndexOf("return", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: no-save title summary does not explain the core loop.");
                }

                var newGameScene = controller.StartNewGame();
                if (newGameScene != FourfoldGameIds.UnitySceneHubCrossroads || controller.LastRequestedUnityScene != FourfoldGameIds.UnitySceneHubCrossroads)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: New Game did not request HubCrossroads.");
                }

                var newGameSave = FourfoldProgressSave.Load();
                if (newGameSave.currentScene != FourfoldGameIds.SceneHubCrossroads || !newGameSave.hubUnlocked || !newGameSave.regionD020Unlocked || !newGameSave.lumenRodUnlocked)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: New Game did not initialize hub, region, or tool progress.");
                }

                var hubSummary = controller.ContinueSummary();
                if (hubSummary.IndexOf("Continue: Hub", StringComparison.Ordinal) < 0 || hubSummary.IndexOf("Relics returned", StringComparison.Ordinal) < 0)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: hub continue summary does not expose location and returned relic progress.");
                }

                newGameSave.currentScene = FourfoldGameIds.SceneD020VerticalSlice;
                newGameSave.d020FailureCount = 2;
                FourfoldProgressSave.Save(newGameSave);
                var d020Summary = controller.ContinueSummary();
                if (d020Summary.IndexOf("D-020 run in progress", StringComparison.Ordinal) < 0 || d020Summary.IndexOf("at risk", StringComparison.Ordinal) < 0)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: D-020 continue summary does not expose in-run risk.");
                }

                var continueScene = controller.ContinueGame();
                if (continueScene != FourfoldGameIds.UnitySceneD020VerticalSlice || controller.LastRequestedUnityScene != FourfoldGameIds.UnitySceneD020VerticalSlice)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: Continue did not request the saved D-020 scene.");
                }

                controller.OpenSettings();
                controller.AdjustSelectedSetting(-1f);
                var settingsSave = FourfoldProgressSave.Load();
                if (!settingsSave.settingsInitialized || settingsSave.masterVolume >= 1f || settingsSave.masterVolume < 0f)
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: settings volume adjustment was not persisted.");
                }

                controller.QuitGame();
                if (controller.LastRequestedUnityScene != "quit")
                {
                    throw new InvalidOperationException("Title gameplay verifier failed: Quit did not mark the quit request.");
                }

                Debug.Log("FOURFOLD Title gameplay verifier passed: New Game, Continue, Settings, and Quit entry flow work.");
            }
            finally
            {
                RestoreFile(savePath, previousSaveBytes);
                if (backupExists)
                {
                    RestoreFile(backupPath, previousBackupBytes);
                }
                else
                {
                    DeleteIfExists(backupPath);
                }
            }
        }

        private static void RestoreFile(string path, byte[] bytes)
        {
            if (bytes == null)
            {
                DeleteIfExists(path);
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, bytes);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
