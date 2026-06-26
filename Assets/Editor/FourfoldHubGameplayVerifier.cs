using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldHubGameplayVerifier
    {
        public static void VerifyHubEnterRegionProgress()
        {
            var savePath = FourfoldProgressSave.SavePath();
            var backupSavePath = savePath + ".bak";
            byte[] previousSaveBytes = null;
            byte[] previousBackupBytes = null;
            var backupExists = File.Exists(backupSavePath);

            if (File.Exists(savePath))
            {
                previousSaveBytes = File.ReadAllBytes(savePath);
            }

            if (File.Exists(backupSavePath))
            {
                previousBackupBytes = File.ReadAllBytes(backupSavePath);
            }

            try
            {
                DeleteIfExists(savePath);
                DeleteIfExists(backupSavePath);

                FourfoldHubSceneBuilder.ValidateGeneratedScene();
                var hook = GameObject.Find("Hub Runtime Hook");
                if (hook == null)
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: Hub Runtime Hook is missing.");
                }

                var controller = hook.GetComponent<HubSceneController>();
                if (controller == null)
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: HubSceneController is missing.");
                }

                controller.InitializeHubProgress();
                var hubProgress = FourfoldProgressSave.Load();
                if (hubProgress.currentScene != FourfoldGameIds.SceneHubCrossroads || !hubProgress.hubUnlocked || !hubProgress.regionD020Unlocked || !hubProgress.lumenRodUnlocked)
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: entering the hub did not persist hub unlock, D-020 unlock, or starter tool unlock.");
                }

                controller.player.position = controller.d020RegionGate.position;
                if (!controller.TryEnterD020Region())
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: D-020 gate interaction did not enter the region.");
                }

                var regionProgress = FourfoldProgressSave.Load();
                if (regionProgress.currentScene != FourfoldGameIds.SceneD020VerticalSlice || !regionProgress.regionD020Unlocked || !regionProgress.lumenRodUnlocked)
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: D-020 entry did not persist the region scene target.");
                }

                controller.ResetProgressForNewGame();
                var resetProgress = FourfoldProgressSave.Load();
                if (resetProgress.currentScene != FourfoldGameIds.SceneHubCrossroads || !resetProgress.hubUnlocked || !resetProgress.regionD020Unlocked || resetProgress.d020Cleared || resetProgress.d020ClearCount != 0)
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: reset did not return to a clean hub-start progress state.");
                }

                if (!controller.TryReturnToTitle())
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: title return action failed.");
                }

                var titleReturnProgress = FourfoldProgressSave.Load();
                if (titleReturnProgress.currentScene != FourfoldGameIds.SceneHubCrossroads || !titleReturnProgress.hubUnlocked || !titleReturnProgress.regionD020Unlocked)
                {
                    throw new InvalidOperationException("Hub gameplay verifier failed: returning to title did not preserve the hub continue target.");
                }

                Debug.Log("FOURFOLD Hub gameplay verifier passed: hub unlock, D-020 entry, reset progress, and title return persist.");
            }
            finally
            {
                RestoreFile(savePath, previousSaveBytes);
                if (backupExists)
                {
                    RestoreFile(backupSavePath, previousBackupBytes);
                }
                else
                {
                    DeleteIfExists(backupSavePath);
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
