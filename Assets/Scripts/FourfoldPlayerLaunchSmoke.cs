using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class FourfoldPlayerLaunchSmoke : MonoBehaviour
    {
        private const string SmokeArg = "--fourfoldLaunchSmoke";
        private const string PassPrefix = "FOURFOLD PLAYER SMOKE PASS";
        private const string FailPrefix = "FOURFOLD PLAYER SMOKE FAIL";
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

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
            yield return null;

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
            yield return null;

            if (!TryStep(() =>
                {
                    var d020Controller = VerifyD020Scene();
                    var routed = FourfoldProgressSave.Load();
                    if (routed.currentScene != FourfoldGameIds.SceneD020VerticalSlice || !routed.hubUnlocked || !routed.regionD020Unlocked || !routed.lumenRodUnlocked)
                    {
                        throw new InvalidOperationException("packaged product route did not persist D-020 continue target");
                    }

                    CompleteD020Run(d020Controller);
                }, saveSnapshot, backupSnapshot))
            {
                yield break;
            }

            yield return WaitForSceneOrFail(FourfoldGameIds.UnitySceneHubCrossroads, saveSnapshot, backupSnapshot);
            if (!SceneLoaded(FourfoldGameIds.UnitySceneHubCrossroads))
            {
                yield break;
            }
            yield return null;

            if (!TryStep(() =>
                {
                    VerifyBankedProgress(FourfoldProgressSave.Load());
                    var hubController = VerifyHubScene();
                    hubController.TryReturnToTitle();
                }, saveSnapshot, backupSnapshot))
            {
                yield break;
            }

            yield return WaitForSceneOrFail(FourfoldGameIds.UnitySceneTitle, saveSnapshot, backupSnapshot);
            if (!SceneLoaded(FourfoldGameIds.UnitySceneTitle))
            {
                yield break;
            }
            yield return null;

            if (!TryStep(() =>
                {
                    var titleController = VerifyTitleScene();
                    var summary = titleController.ContinueSummary();
                    if (summary.IndexOf("Hub", StringComparison.Ordinal) < 0 || summary.IndexOf("Relics returned 2/2", StringComparison.Ordinal) < 0)
                    {
                        throw new InvalidOperationException("title continue summary does not expose banked Hub progress");
                    }

                    titleController.ContinueGame();
                }, saveSnapshot, backupSnapshot))
            {
                yield break;
            }

            yield return WaitForSceneOrFail(FourfoldGameIds.UnitySceneHubCrossroads, saveSnapshot, backupSnapshot);
            if (!SceneLoaded(FourfoldGameIds.UnitySceneHubCrossroads))
            {
                yield break;
            }
            yield return null;

            if (!TryStep(() =>
                {
                    VerifyHubScene();
                    var completed = FourfoldProgressSave.Load();
                    VerifyBankedProgress(completed);
                    RestoreSnapshots(saveSnapshot, backupSnapshot);
                    Debug.Log($"{PassPrefix} scenes=Title>HubCrossroads>D020VerticalSlice>HubCrossroads>Title>HubCrossroads clearCount={completed.d020ClearCount} relics=2/2 bestSeconds={completed.d020BestClearTimeSeconds:0.0}");
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

        private static D020SliceController VerifyD020Scene()
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

            if (controller.requiredToolNode == null || controller.secondToolNode == null || controller.rewardClaimPoint == null || controller.secondRewardClaimPoint == null || controller.returnGatePoint == null)
            {
                throw new InvalidOperationException("D-020 controller completion references missing");
            }

            return controller;
        }

        private static void CompleteD020Run(D020SliceController controller)
        {
            var tool = controller.explorationTool;
            SolveNodeWithTool(tool, controller.requiredToolNode, "first tool node");
            InvokePrivate(controller, "UpdateProgressFlags");
            DefeatEnemyRoster(controller);
            MovePlayerTo(controller, controller.rewardClaimPoint);
            if (!InvokePrivateBool(controller, "TryClaimFirstReward"))
            {
                throw new InvalidOperationException("packaged smoke could not claim first relic reward");
            }

            SolveNodeWithTool(tool, controller.secondToolNode, "second tool node");
            InvokePrivate(controller, "UpdateProgressFlags");
            SetPrivate(controller, "runTimerSeconds", 91f);
            MovePlayerTo(controller, controller.secondRewardClaimPoint);
            if (!InvokePrivateBool(controller, "TryClaimSecondReward"))
            {
                throw new InvalidOperationException("packaged smoke could not claim second relic reward");
            }

            MovePlayerTo(controller, controller.returnGatePoint);
            if (!InvokePrivateBool(controller, "TryReturnToHub"))
            {
                throw new InvalidOperationException("packaged smoke could not return to hub after clear");
            }
        }

        private static void SolveNodeWithTool(ExplorationTool tool, ExplorationNode node, string label)
        {
            if (tool == null || node == null || tool.player == null)
            {
                throw new InvalidOperationException($"packaged smoke missing {label} references");
            }

            tool.player.position = node.transform.position;
            tool.inputEnabled = true;
            SetPrivate(tool, "cooldownTimer", 0f);
            if (!node.IsSolved && !tool.TryUse())
            {
                throw new InvalidOperationException($"packaged smoke could not activate {label}");
            }

            if (!node.IsSolved)
            {
                throw new InvalidOperationException($"packaged smoke left {label} unsolved");
            }
        }

        private static void DefeatEnemyRoster(D020SliceController controller)
        {
            for (var i = 0; i < controller.enemies.Length; i++)
            {
                DefeatEnemyWithBasicAttacks(controller, i);
            }

            if (!InvokePrivateBool(controller, "AllEnemiesDefeated") || !GetPrivate<bool>(controller, "bossDefeatedThisRun"))
            {
                throw new InvalidOperationException("packaged smoke did not defeat the D-020 enemy roster and boss");
            }
        }

        private static void DefeatEnemyWithBasicAttacks(D020SliceController controller, int enemyIndex)
        {
            var enemy = controller.enemies[enemyIndex];
            if (enemy == null)
            {
                throw new InvalidOperationException($"packaged smoke missing enemy {enemyIndex}");
            }

            var guard = 0;
            while (EnemyHealth(controller, enemyIndex) > 0f && guard++ < 32)
            {
                var attackDirection = Vector3.right;
                controller.player.position = enemy.position - attackDirection * 1.0f;
                controller.player.rotation = Quaternion.LookRotation(attackDirection, Vector3.up);
                SetPrivate(controller, "facing", attackDirection);
                SetPrivate(controller, "attackTimer", 0f);
                InvokePrivate(controller, "TryAttack");
            }

            if (EnemyHealth(controller, enemyIndex) > 0f || enemy.gameObject.activeSelf)
            {
                throw new InvalidOperationException($"packaged smoke basic attacks did not defeat enemy {enemyIndex}");
            }
        }

        private static float EnemyHealth(D020SliceController controller, int enemyIndex)
        {
            var enemyHealth = GetPrivate<float[]>(controller, "enemyHealth");
            if (enemyHealth == null || enemyIndex < 0 || enemyIndex >= enemyHealth.Length)
            {
                throw new InvalidOperationException("packaged smoke could not read enemy health");
            }

            return enemyHealth[enemyIndex];
        }

        private static void MovePlayerTo(D020SliceController controller, Transform target)
        {
            if (controller.player == null || target == null)
            {
                throw new InvalidOperationException("packaged smoke cannot move player to a missing target");
            }

            controller.player.position = target.position;
        }

        private static void VerifyBankedProgress(FourfoldProgressData progress)
        {
            if (progress.currentScene != FourfoldGameIds.SceneHubCrossroads || !progress.hubUnlocked || !progress.regionD020Unlocked || !progress.lumenRodUnlocked)
            {
                throw new InvalidOperationException("packaged smoke completion did not persist Hub continue target and unlocks");
            }

            if (!progress.d020Cleared || !progress.regionD020Cleared || !progress.d020BossDefeated || !progress.d020ReturnedToHub)
            {
                throw new InvalidOperationException("packaged smoke completion did not persist clear, boss, or return state");
            }

            if (!progress.d020RewardClaimed || !progress.d020SecondRewardClaimed || !progress.d020SecondNodeOpened)
            {
                throw new InvalidOperationException("packaged smoke completion did not persist banked relic rewards");
            }

            if (progress.d020ClearCount < 1 || progress.d020BestClearTimeSeconds <= 0f || progress.lastCompletedRegion != FourfoldGameIds.RegionD020 || progress.hubSpawnId != FourfoldGameIds.HubSpawnReturnGate)
            {
                throw new InvalidOperationException("packaged smoke completion did not persist clear count, best time, completed region, or hub spawn");
            }
        }

        private static object InvokePrivate(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, PrivateInstance);
            if (method == null)
            {
                throw new MissingMethodException(target.GetType().Name, methodName);
            }

            try
            {
                return method.Invoke(target, args);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        private static bool InvokePrivateBool(object target, string methodName, params object[] args)
        {
            return (bool)InvokePrivate(target, methodName, args);
        }

        private static T GetPrivate<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, PrivateInstance);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            return (T)field.GetValue(target);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, PrivateInstance);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
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
