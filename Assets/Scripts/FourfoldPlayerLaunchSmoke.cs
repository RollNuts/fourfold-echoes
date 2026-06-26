using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class FourfoldPlayerLaunchSmoke : MonoBehaviour
    {
        private const string SmokeArg = "--fourfoldLaunchSmoke";
        private const string PassPrefix = "FOURFOLD PLAYER SMOKE PASS";
        private const string FailPrefix = "FOURFOLD PLAYER SMOKE FAIL";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void MaybeRun()
        {
            if (!Requested())
            {
                return;
            }

            var runner = new GameObject("Fourfold Player Launch Smoke");
            DontDestroyOnLoad(runner);
            runner.AddComponent<FourfoldPlayerLaunchSmoke>();
        }

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return null;
            yield return null;

            var savePath = FourfoldProgressSave.SavePath();
            var backupPath = savePath + ".bak";
            var saveSnapshot = FileSnapshot.Capture(savePath);
            var backupSnapshot = FileSnapshot.Capture(backupPath);

            if (!TryStep(() =>
                {
                    var titleController = VerifyTitleScene();
                    var progress = FourfoldProgressSave.Load();
                    if (progress == null || progress.version != FourfoldProgressSave.CurrentVersion || !progress.settingsInitialized)
                    {
                        throw new InvalidOperationException("progress save defaults failed to load");
                    }

                    titleController.StartNewGame();
                }, saveSnapshot, backupSnapshot))
            {
                yield break;
            }

            yield return WaitForSceneOrFail(FourfoldGameIds.UnitySceneHubCrossroads, saveSnapshot, backupSnapshot);
            if (!SceneLoaded(FourfoldGameIds.UnitySceneHubCrossroads))
            {
                yield break;
            }

            if (!TryStep(() =>
                {
                    var hubController = VerifyHubScene();
                    hubController.player.position = hubController.d020RegionGate.position;
                    if (!hubController.TryEnterD020Region())
                    {
                        throw new InvalidOperationException("hub D-020 gate entry failed");
                    }
                }, saveSnapshot, backupSnapshot))
            {
                yield break;
            }

            yield return WaitForSceneOrFail(FourfoldGameIds.UnitySceneD020VerticalSlice, saveSnapshot, backupSnapshot);
            if (!SceneLoaded(FourfoldGameIds.UnitySceneD020VerticalSlice))
            {
                yield break;
            }

            if (!TryStep(() =>
                {
                    VerifyD020Scene();
                    var routed = FourfoldProgressSave.Load();
                    if (routed.currentScene != FourfoldGameIds.SceneD020VerticalSlice || !routed.hubUnlocked || !routed.regionD020Unlocked || !routed.lumenRodUnlocked)
                    {
                        throw new InvalidOperationException("packaged product route did not persist D-020 continue target");
                    }

                    RestoreSnapshots(saveSnapshot, backupSnapshot);
                    Debug.Log($"{PassPrefix} scenes=Title>HubCrossroads>D020VerticalSlice saveVersion={routed.version}");
                    Application.Quit(0);
                }, saveSnapshot, backupSnapshot))
            {
                yield break;
            }
        }

        private static TitleSceneController VerifyTitleScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != FourfoldGameIds.UnitySceneTitle)
            {
                throw new InvalidOperationException($"expected Title scene, got '{scene.name}'");
            }

            var titleHook = GameObject.Find("Title Runtime Hook");
            if (titleHook == null)
            {
                throw new InvalidOperationException("Title Runtime Hook missing");
            }

            var titleController = titleHook.GetComponent<TitleSceneController>();
            if (titleController == null)
            {
                throw new InvalidOperationException("TitleSceneController missing");
            }

            return titleController;
        }

        private static HubSceneController VerifyHubScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != FourfoldGameIds.UnitySceneHubCrossroads)
            {
                throw new InvalidOperationException($"expected HubCrossroads scene, got '{scene.name}'");
            }

            var hubHook = GameObject.Find("Hub Runtime Hook");
            if (hubHook == null)
            {
                throw new InvalidOperationException("Hub Runtime Hook missing");
            }

            var hubController = hubHook.GetComponent<HubSceneController>();
            if (hubController == null)
            {
                throw new InvalidOperationException("HubSceneController missing");
            }

            if (hubController.player == null || hubController.d020RegionGate == null)
            {
                throw new InvalidOperationException("Hub controller route references missing");
            }

            return hubController;
        }

        private static void VerifyD020Scene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != FourfoldGameIds.UnitySceneD020VerticalSlice)
            {
                throw new InvalidOperationException($"expected D020VerticalSlice scene, got '{scene.name}'");
            }

            var hook = GameObject.Find("D020 Runtime Hook");
            if (hook == null)
            {
                throw new InvalidOperationException("D020 Runtime Hook missing");
            }

            var controller = hook.GetComponent<D020SliceController>();
            if (controller == null)
            {
                throw new InvalidOperationException("D020SliceController missing");
            }

            if (controller.player == null || controller.explorationTool == null || controller.enemies == null || controller.enemies.Length < 4)
            {
                throw new InvalidOperationException("D-020 controller gameplay references missing");
            }
        }

        private static IEnumerator WaitForSceneOrFail(string sceneName, FileSnapshot saveSnapshot, FileSnapshot backupSnapshot)
        {
            const int MaxFrames = 180;
            for (var frame = 0; frame < MaxFrames; frame++)
            {
                if (SceneLoaded(sceneName))
                {
                    yield break;
                }

                yield return null;
            }

            Fail($"timed out waiting for scene '{sceneName}'", saveSnapshot, backupSnapshot);
        }

        private static bool SceneLoaded(string sceneName)
        {
            var scene = SceneManager.GetActiveScene();
            return scene.IsValid() && scene.name == sceneName;
        }

        private static bool TryStep(Action action, FileSnapshot saveSnapshot, FileSnapshot backupSnapshot)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception exception)
            {
                Fail(exception.Message, saveSnapshot, backupSnapshot);
                return false;
            }
        }

        private static void Fail(string message, FileSnapshot saveSnapshot, FileSnapshot backupSnapshot)
        {
            RestoreSnapshots(saveSnapshot, backupSnapshot);
            Debug.LogError($"{FailPrefix} {message}");
            Application.Quit(1);
        }

        private static void RestoreSnapshots(FileSnapshot saveSnapshot, FileSnapshot backupSnapshot)
        {
            saveSnapshot.Restore();
            backupSnapshot.Restore();
        }

        private readonly struct FileSnapshot
        {
            private readonly string path;
            private readonly bool existed;
            private readonly byte[] bytes;

            private FileSnapshot(string path, bool existed, byte[] bytes)
            {
                this.path = path;
                this.existed = existed;
                this.bytes = bytes;
            }

            public static FileSnapshot Capture(string path)
            {
                return File.Exists(path)
                    ? new FileSnapshot(path, true, File.ReadAllBytes(path))
                    : new FileSnapshot(path, false, null);
            }

            public void Restore()
            {
                try
                {
                    if (!existed)
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }

                        return;
                    }

                    var directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllBytes(path, bytes);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"FOURFOLD PLAYER SMOKE could not restore save snapshot '{Path.GetFileName(path)}': {exception.Message}");
                }
            }
        }

        private static bool Requested()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == SmokeArg)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
