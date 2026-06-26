using System;
using System.IO;
using System.Reflection;
using FourfoldEchoes.Product;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldD020GameplayVerifier
    {
        public static void VerifyExistingSceneToolLoop()
        {
            FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();

            var hook = FindSceneObject("D020 Runtime Hook");
            if (hook == null)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: required object is missing: D020 Runtime Hook.");
            }

            var controller = RequireComponent<D020SliceController>(hook, "D020 Runtime Hook");
            var tool = RequireComponent<ExplorationTool>(hook, "D020 Runtime Hook");
            ValidateRequiredReferences(controller, tool);

            var nodes = tool.nodes;
            if (nodes == null || nodes.Length < 2)
            {
                var count = nodes == null ? 0 : nodes.Length;
                throw new InvalidOperationException($"D-020 gameplay verifier failed: ExplorationTool needs at least two node entries; found {count}.");
            }

            if (nodes[0] == null)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: ExplorationTool node 0 is missing.");
            }

            if (nodes[1] == null)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: ExplorationTool node 1 is missing.");
            }

            if (controller.requiredToolNode != nodes[0])
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: controller.requiredToolNode must reference ExplorationTool node 0.");
            }

            if (controller.secondToolNode != nodes[1])
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: controller.secondToolNode must reference ExplorationTool node 1.");
            }

            var originalPlayerPosition = tool.player.position;
            var originalInputEnabled = tool.inputEnabled;
            var originalCooldownSeconds = tool.cooldownSeconds;
            var nodeSnapshots = new[]
            {
                new NodeSnapshot(nodes[0]),
                new NodeSnapshot(nodes[1])
            };

            try
            {
                VerifyNodeUse(tool, nodes[0], 0);
                VerifyNodeUse(tool, nodes[1], 1);
                Debug.Log("FOURFOLD D-020 gameplay verifier passed: existing scene tool loop activates both nodes.");
            }
            finally
            {
                tool.player.position = originalPlayerPosition;
                tool.inputEnabled = originalInputEnabled;
                tool.cooldownSeconds = originalCooldownSeconds;

                for (var i = 0; i < nodeSnapshots.Length; i++)
                {
                    nodeSnapshots[i].Restore();
                }
            }
        }

        public static void VerifyExistingSceneFullProgressionLoop()
        {
            var savePath = FourfoldProgressSave.SavePath();
            var backupSavePath = savePath + ".verifier.backup";
            var backupExists = File.Exists(backupSavePath);
            byte[] previousSaveBytes = null;
            byte[] previousBackupBytes = null;

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

                FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();

                var hook = FindSceneObject("D020 Runtime Hook");
                if (hook == null)
                {
                    throw new InvalidOperationException("D-020 full-loop verifier failed: required object is missing: D020 Runtime Hook.");
                }

                var controller = RequireComponent<D020SliceController>(hook, "D020 Runtime Hook");
                var tool = RequireComponent<ExplorationTool>(hook, "D020 Runtime Hook");
                ValidateRequiredReferences(controller, tool);
                PrepareControllerForFullLoop(controller);

                var nodes = tool.nodes;
                if (nodes == null || nodes.Length < 2 || nodes[0] == null || nodes[1] == null)
                {
                    throw new InvalidOperationException("D-020 full-loop verifier failed: the exploration tool needs two usable nodes.");
                }

                var nodeSnapshots = new[]
                {
                    new NodeSnapshot(nodes[0]),
                    new NodeSnapshot(nodes[1])
                };
                var originalPlayerPosition = tool.player.position;
                var originalInputEnabled = tool.inputEnabled;
                var originalCooldownSeconds = tool.cooldownSeconds;

                try
                {
                    VerifyNodeUse(tool, nodes[0], 0);
                    InvokePrivate(controller, "UpdateProgressFlags");
                    ForceAllEnemiesDefeated(controller);
                    MovePlayerTo(tool, controller.rewardClaimPoint);
                    if (!InvokePrivateBool(controller, "TryClaimFirstReward"))
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: first relic reward could not be claimed after node 0 and enemy defeat.");
                    }

                    if (!GetPrivateBool(controller, "firstRewardClaimedThisRun"))
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: first reward flag was not set.");
                    }

                    if (GetPrivateBool(controller, "runCleared"))
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: run cleared before the second node and second reward.");
                    }

                    VerifyNodeUse(tool, nodes[1], 1);
                    InvokePrivate(controller, "UpdateProgressFlags");
                    ForceAllEnemiesDefeated(controller);
                    MovePlayerTo(tool, controller.secondRewardClaimPoint);
                    if (!InvokePrivateBool(controller, "TryClaimSecondReward"))
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: second relic reward could not be claimed after node 1.");
                    }

                    if (!GetPrivateBool(controller, "secondRewardClaimedThisRun") || !GetPrivateBool(controller, "runCleared"))
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: second reward did not mark the run clear.");
                    }

                    SetPrivate(controller, "runTimerSeconds", 91f);
                    MovePlayerTo(tool, controller.returnGatePoint);
                    if (!InvokePrivateBool(controller, "TryReturnToHub"))
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: return gate did not persist the run.");
                    }

                    var saved = FourfoldProgressSave.Load();
                    if (!saved.d020Cleared || !saved.d020RewardClaimed || !saved.d020SecondNodeOpened || !saved.d020SecondRewardClaimed || !saved.d020ReturnedToHub)
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: persisted progress is missing clear, reward, second-node, second-reward, or return flags.");
                    }

                    if (saved.d020ClearCount < 1 || saved.d020BestClearTimeSeconds <= 0f)
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: clear count or best clear time was not persisted.");
                    }

                    if (saved.currentScene != FourfoldGameIds.SceneHubCrossroads || !saved.hubUnlocked || !saved.regionD020Unlocked || !saved.regionD020Cleared || !saved.lumenRodUnlocked)
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: hub, region, current scene, or exploration tool progress was not persisted.");
                    }

                    if (saved.lastCompletedRegion != FourfoldGameIds.RegionD020 || saved.hubSpawnId != FourfoldGameIds.HubSpawnReturnGate || !saved.d020BossDefeated)
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: completed region, hub spawn, or boss defeat progress was not persisted.");
                    }

                    Debug.Log("FOURFOLD D-020 full-loop verifier passed: tool nodes, two rewards, return, clear count, and best time persist.");
                }
                finally
                {
                    tool.player.position = originalPlayerPosition;
                    tool.inputEnabled = originalInputEnabled;
                    tool.cooldownSeconds = originalCooldownSeconds;

                    for (var i = 0; i < nodeSnapshots.Length; i++)
                    {
                        nodeSnapshots[i].Restore();
                    }
                }
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

        public static void VerifyExistingSceneFailureLoop()
        {
            var savePath = FourfoldProgressSave.SavePath();
            var saveBackupPath = savePath + ".bak";
            byte[] previousSaveBytes = null;
            byte[] previousSaveBackupBytes = null;

            if (File.Exists(savePath))
            {
                previousSaveBytes = File.ReadAllBytes(savePath);
            }

            if (File.Exists(saveBackupPath))
            {
                previousSaveBackupBytes = File.ReadAllBytes(saveBackupPath);
            }

            try
            {
                DeleteIfExists(savePath);
                DeleteIfExists(saveBackupPath);

                FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();

                var hook = FindSceneObject("D020 Runtime Hook");
                if (hook == null)
                {
                    throw new InvalidOperationException("D-020 failure verifier failed: required object is missing: D020 Runtime Hook.");
                }

                var controller = RequireComponent<D020SliceController>(hook, "D020 Runtime Hook");
                var tool = RequireComponent<ExplorationTool>(hook, "D020 Runtime Hook");
                ValidateRequiredReferences(controller, tool);
                PrepareControllerForFullLoop(controller);

                SetPrivate(controller, "previousShortcutLoaded", true);
                SetPrivate(controller, "firstRewardClaimedThisRun", true);
                SetPrivate(controller, "failureCount", 0);
                InvokePrivate(controller, "RegisterRunFailure");

                if (!GetPrivateBool(controller, "runFailed"))
                {
                    throw new InvalidOperationException("D-020 failure verifier failed: runFailed was not set.");
                }

                if (GetPrivateBool(controller, "firstRewardClaimedThisRun") || GetPrivateBool(controller, "secondRewardClaimedThisRun"))
                {
                    throw new InvalidOperationException("D-020 failure verifier failed: unreturned run rewards were not cleared.");
                }

                var saved = FourfoldProgressSave.Load();
                if (saved.currentScene != FourfoldGameIds.SceneD020VerticalSlice)
                {
                    throw new InvalidOperationException("D-020 failure verifier failed: death inside the region must persist the current scene as D-020.");
                }

                if (saved.d020FailureCount != 1 || !saved.d020ShortcutOpened)
                {
                    throw new InvalidOperationException("D-020 failure verifier failed: failure count or opened shortcut did not persist.");
                }

                if (saved.d020RewardClaimed || saved.d020SecondRewardClaimed || saved.d020Cleared || saved.regionD020Cleared || saved.d020ReturnedToHub)
                {
                    throw new InvalidOperationException("D-020 failure verifier failed: unreturned rewards or clear flags were incorrectly persisted.");
                }

                Debug.Log("FOURFOLD D-020 failure verifier passed: death persists failure/shortcut progress and drops unreturned rewards.");
            }
            finally
            {
                RestoreFile(savePath, previousSaveBytes);
                RestoreFile(saveBackupPath, previousSaveBackupBytes);
            }
        }

        private static void ValidateRequiredReferences(D020SliceController controller, ExplorationTool tool)
        {
            RequireReference(controller.player, "D020SliceController.player");
            RequireReference(controller.enemies, "D020SliceController.enemies");
            if (controller.enemies.Length == 0)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: D020SliceController.enemies must contain at least one enemy transform.");
            }

            for (var i = 0; i < controller.enemies.Length; i++)
            {
                RequireReference(controller.enemies[i], $"D020SliceController.enemies[{i}]");
            }

            RequireReference(controller.rewardClaimPoint, "D020SliceController.rewardClaimPoint");
            RequireReference(controller.secondRewardClaimPoint, "D020SliceController.secondRewardClaimPoint");
            RequireReference(controller.returnGatePoint, "D020SliceController.returnGatePoint");
            RequireReference(controller.explorationTool, "D020SliceController.explorationTool");
            RequireReference(controller.requiredToolNode, "D020SliceController.requiredToolNode");
            RequireReference(controller.shortcutLockedRead, "D020SliceController.shortcutLockedRead");
            RequireReference(controller.secondToolNode, "D020SliceController.secondToolNode");
            RequireReference(controller.secondRouteLockedRead, "D020SliceController.secondRouteLockedRead");
            RequireReference(controller.fixedCamera, "D020SliceController.fixedCamera");
            RequireReference(tool.player, "ExplorationTool.player");
            RequireReference(tool.nodes, "ExplorationTool.nodes");

            if (controller.explorationTool != tool)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: D020SliceController.explorationTool must reference the ExplorationTool on D020 Runtime Hook.");
            }

            if (controller.player != tool.player)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: D020SliceController.player and ExplorationTool.player must reference the same transform.");
            }
        }

        private static void VerifyNodeUse(ExplorationTool tool, ExplorationNode node, int index)
        {
            tool.player.position = node.transform.position;
            tool.inputEnabled = true;
            tool.cooldownSeconds = 0f;
            node.ResetNode();

            if (!tool.TryUse())
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: ExplorationTool.TryUse returned false for node {index} ({node.name}).");
            }

            if (!node.IsSolved)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: node {index} ({node.name}) was not solved after TryUse.");
            }

            if (node.responseTarget != null && !node.responseTarget.activeInHierarchy)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: node {index} ({node.name}) response target is not active after TryUse: {node.responseTarget.name}.");
            }
        }

        private static void PrepareControllerForFullLoop(D020SliceController controller)
        {
            RequireReference(controller.player, "D020SliceController.player");
            RequireReference(controller.enemies, "D020SliceController.enemies");
            SetPrivate(controller, "progressData", new FourfoldProgressData());
            SetPrivate(controller, "initialPlayerPosition", controller.player.position);
            SetPrivate(controller, "initialPlayerRotation", controller.player.rotation);
            SetPrivate(controller, "enemyHealth", new float[controller.enemies.Length]);
            ClearControllerProgressState(controller);
        }

        private static void ClearControllerProgressState(D020SliceController controller)
        {
            SetPrivate(controller, "previousClearLoaded", false);
            SetPrivate(controller, "previousShortcutLoaded", false);
            SetPrivate(controller, "previousRewardLoaded", false);
            SetPrivate(controller, "previousSecondNodeLoaded", false);
            SetPrivate(controller, "previousSecondRewardLoaded", false);
            SetPrivate(controller, "previousReturnedToHubLoaded", false);
            SetPrivate(controller, "firstRewardClaimedThisRun", false);
            SetPrivate(controller, "secondRewardClaimedThisRun", false);
            SetPrivate(controller, "returnedToHubThisRun", false);
            SetPrivate(controller, "returnRegisteredThisRun", false);
            SetPrivate(controller, "runCleared", false);
            SetPrivate(controller, "runFailed", false);
            SetPrivate(controller, "clearCount", 0);
            SetPrivate(controller, "runTimerSeconds", 0f);
            SetPrivate(controller, "bestClearTimeSeconds", 0f);
            if (controller.requiredToolNode != null)
            {
                controller.requiredToolNode.ResetNode();
            }

            if (controller.secondToolNode != null)
            {
                controller.secondToolNode.ResetNode();
            }
        }

        private static void ForceAllEnemiesDefeated(D020SliceController controller)
        {
            var enemyHealth = GetPrivate<float[]>(controller, "enemyHealth");
            if (enemyHealth == null || enemyHealth.Length == 0)
            {
                throw new InvalidOperationException("D-020 full-loop verifier failed: enemy health array is missing.");
            }

            for (var i = 0; i < enemyHealth.Length; i++)
            {
                enemyHealth[i] = 0f;
                if (controller.enemies != null && i < controller.enemies.Length && controller.enemies[i] != null)
                {
                    controller.enemies[i].gameObject.SetActive(false);
                }
            }
        }

        private static void MovePlayerTo(ExplorationTool tool, Transform target)
        {
            if (tool == null || tool.player == null || target == null)
            {
                throw new InvalidOperationException("D-020 full-loop verifier failed: cannot move player to a missing target.");
            }

            tool.player.position = target.position;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private method {methodName} on {target.GetType().Name}.");
            }

            method.Invoke(target, Array.Empty<object>());
        }

        private static bool InvokePrivateBool(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || method.ReturnType != typeof(bool))
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private bool method {methodName} on {target.GetType().Name}.");
            }

            return (bool)method.Invoke(target, Array.Empty<object>());
        }

        private static bool GetPrivateBool(object target, string fieldName)
        {
            return GetPrivate<bool>(target, fieldName);
        }

        private static T GetPrivate<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private field {fieldName} on {target.GetType().Name}.");
            }

            return (T)field.GetValue(target);
        }

        private static void SetPrivate<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private field {fieldName} on {target.GetType().Name}.");
            }

            field.SetValue(target, value);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
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

        private static T RequireComponent<T>(GameObject gameObject, string objectName) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: required component {typeof(T).Name} is missing on {objectName}.");
            }

            return component;
        }

        private static void RequireReference(UnityEngine.Object reference, string name)
        {
            if (reference == null)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: required reference is missing: {name}.");
            }
        }

        private static void RequireReference(Array reference, string name)
        {
            if (reference == null)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: required reference array is missing: {name}.");
            }
        }

        private static GameObject FindSceneObject(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var found = FindInChildren(roots[i].transform, name);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindInChildren(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindInChildren(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private sealed class NodeSnapshot
        {
            private readonly ExplorationNode node;
            private readonly bool solved;
            private readonly GameObject responseTarget;
            private readonly GameObject idleRead;
            private readonly GameObject activeRead;
            private readonly bool responseActiveSelf;
            private readonly bool idleActiveSelf;
            private readonly bool activeActiveSelf;
            private readonly Renderer[] highlightRenderers;
            private readonly bool[] highlightRendererEnabled;

            public NodeSnapshot(ExplorationNode node)
            {
                this.node = node;
                solved = node.IsSolved;
                responseTarget = node.responseTarget;
                idleRead = node.idleRead;
                activeRead = node.activeRead;
                responseActiveSelf = responseTarget != null && responseTarget.activeSelf;
                idleActiveSelf = idleRead != null && idleRead.activeSelf;
                activeActiveSelf = activeRead != null && activeRead.activeSelf;
                highlightRenderers = node.highlightRenderers;
                highlightRendererEnabled = SnapshotRendererState(highlightRenderers);
            }

            public void Restore()
            {
                node.SetSolved(solved);
                RestoreActive(responseTarget, responseActiveSelf);
                RestoreActive(idleRead, idleActiveSelf);
                RestoreActive(activeRead, activeActiveSelf);
                RestoreRendererState(highlightRenderers, highlightRendererEnabled);
            }

            private static bool[] SnapshotRendererState(Renderer[] renderers)
            {
                if (renderers == null)
                {
                    return null;
                }

                var result = new bool[renderers.Length];
                for (var i = 0; i < renderers.Length; i++)
                {
                    result[i] = renderers[i] != null && renderers[i].enabled;
                }

                return result;
            }

            private static void RestoreRendererState(Renderer[] renderers, bool[] enabledState)
            {
                if (renderers == null || enabledState == null)
                {
                    return;
                }

                var count = Math.Min(renderers.Length, enabledState.Length);
                for (var i = 0; i < count; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].enabled = enabledState[i];
                    }
                }
            }

            private static void RestoreActive(GameObject gameObject, bool activeSelf)
            {
                if (gameObject != null)
                {
                    gameObject.SetActive(activeSelf);
                }
            }
        }
    }
}
