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
            var originalFeedbackSeconds = tool.feedbackSeconds;
            var nodeSnapshots = new[]
            {
                new NodeSnapshot(nodes[0]),
                new NodeSnapshot(nodes[1])
            };

            try
            {
                VerifyToolNoTarget(tool);
                VerifyReadyTarget(tool, nodes[0], 0);
                VerifyCooldownReject(tool, nodes[0]);
                VerifyNodeUse(tool, nodes[0], 0);
                VerifyNodeUse(tool, nodes[1], 1);
                Debug.Log("FOURFOLD D-020 gameplay verifier passed: existing scene tool loop reports no-target, target-ready, cooldown, and activates both nodes.");
            }
            finally
            {
                tool.player.position = originalPlayerPosition;
                tool.inputEnabled = originalInputEnabled;
                tool.cooldownSeconds = originalCooldownSeconds;
                tool.feedbackSeconds = originalFeedbackSeconds;
                tool.ClearRuntimeState();

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

                    var runRiskText = InvokePrivateString(controller, "RunRiskStateText");
                    if (runRiskText.IndexOf("Lumen Edge", StringComparison.Ordinal) < 0
                        || runRiskText.IndexOf("Lumen Ward", StringComparison.Ordinal) < 0
                        || runRiskText.IndexOf("Lumen Link", StringComparison.Ordinal) < 0)
                    {
                        throw new InvalidOperationException("D-020 full-loop verifier failed: risk UI did not name the unbanked Lumen reward skills.");
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

                var lostRelicMask = GetPrivate<int>(controller, "lastLostRelicMask");
                var lostRelicText = InvokePrivateString(controller, "RewardMaskNames", lostRelicMask);
                if (lostRelicMask == 0 || lostRelicText.IndexOf("Lumen Edge", StringComparison.Ordinal) < 0)
                {
                    throw new InvalidOperationException("D-020 failure verifier failed: failure result did not retain the name of the lost unbanked reward skill.");
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

        public static void VerifyExistingSceneCombatDefeatPath()
        {
            FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();

            var hook = FindSceneObject("D020 Runtime Hook");
            if (hook == null)
            {
                throw new InvalidOperationException("D-020 combat verifier failed: required object is missing: D020 Runtime Hook.");
            }

            var controller = RequireComponent<D020SliceController>(hook, "D020 Runtime Hook");
            var tool = RequireComponent<ExplorationTool>(hook, "D020 Runtime Hook");
            ValidateRequiredReferences(controller, tool);
            PrepareControllerForCombat(controller);
            InvokePrivate(controller, "EnsureExplorationReferences");
            VerifyRelicIdentityEffects(controller);
            VerifyBossToolOpening(controller, tool);

            for (var i = 0; i < controller.enemies.Length; i++)
            {
                DefeatEnemyWithBasicAttacks(controller, i);
            }

            if (!InvokePrivateBool(controller, "AllEnemiesDefeated"))
            {
                throw new InvalidOperationException("D-020 combat verifier failed: basic attacks did not defeat the full enemy roster.");
            }

            if (!GetPrivateBool(controller, "bossDefeatedThisRun"))
            {
                throw new InvalidOperationException("D-020 combat verifier failed: defeating the boss through combat did not set the boss defeat beat.");
            }

            if (!InvokePrivateBool(controller, "RewardReady"))
            {
                throw new InvalidOperationException("D-020 combat verifier failed: rewards were not ready after combat defeat and first tool-node solve.");
            }

            Debug.Log("FOURFOLD D-020 combat verifier passed: basic attacks defeat melee, ranged, elite, and boss enemies, relic effects are distinct, the exploration tool exposes a boss opening, and reward readiness unlocks.");
        }

        public static void VerifyExistingSceneDeathRetryAndTitlePath()
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
                    throw new InvalidOperationException("D-020 death/retry verifier failed: required object is missing: D020 Runtime Hook.");
                }

                var controller = RequireComponent<D020SliceController>(hook, "D020 Runtime Hook");
                var tool = RequireComponent<ExplorationTool>(hook, "D020 Runtime Hook");
                ValidateRequiredReferences(controller, tool);
                PrepareControllerForCombat(controller);

                var enemy = controller.enemies[0];
                controller.player.position = Vector3.zero;
                enemy.position = new Vector3(0f, 0f, 0.72f);
                var toPlayer = controller.player.position - enemy.position;
                toPlayer.y = 0f;

                SetPrivate(controller, "playerHealth", 100f);
                SetPrivate(controller, "playerInvulnerableTimer", 0f);
                SetPrivate(controller, "dodgeTimer", 0.12f);
                SetPrivate(controller, "runFailed", false);
                InvokePrivate(controller, "ResolveEnemyAttack", 0, enemy, toPlayer);
                if (GetPrivate<float>(controller, "playerHealth") < 99.9f || GetPrivateBool(controller, "runFailed"))
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: dodge invulnerability did not prevent enemy damage.");
                }

                SetPrivate(controller, "playerHealth", 10f);
                SetPrivate(controller, "playerInvulnerableTimer", 0f);
                SetPrivate(controller, "dodgeTimer", 0f);
                InvokePrivate(controller, "ResolveEnemyAttack", 0, enemy, toPlayer);
                if (!GetPrivateBool(controller, "runFailed"))
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: lethal enemy hit did not fail the run.");
                }

                var failedSave = FourfoldProgressSave.Load();
                if (failedSave.d020FailureCount != 1 || failedSave.d020RewardClaimed || failedSave.d020SecondRewardClaimed)
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: failed run persistence did not record one failure and drop unreturned rewards.");
                }

                if (!controller.TryReturnToHubAfterFailure())
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: failed-run return-to-hub action was rejected.");
                }

                var failedHubSave = FourfoldProgressSave.Load();
                if (failedHubSave.currentScene != FourfoldGameIds.SceneHubCrossroads
                    || failedHubSave.d020FailureCount != 1
                    || failedHubSave.d020RewardClaimed
                    || failedHubSave.d020SecondRewardClaimed
                    || failedHubSave.d020Cleared
                    || failedHubSave.regionD020Cleared)
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: failed-run hub return did not preserve loss state without banking rewards.");
                }

                InvokePrivate(controller, "ResetRun");
                if (GetPrivateBool(controller, "runFailed") || GetPrivate<float>(controller, "playerHealth") < 99.9f)
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: retry did not restore player health and clear failure state.");
                }

                if (!enemy.gameObject.activeSelf)
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: retry did not reactivate enemies.");
                }

                SetPrivate(controller, "firstRewardClaimedThisRun", true);
                InvokePrivate(controller, "RequestRetryRun");
                if (GetPrivate<object>(controller, "pendingExitAction").ToString() != "RetryRun")
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: retry input did not require confirmation while carrying an unbanked relic.");
                }

                InvokePrivate(controller, "ResetRun");
                SetPrivate(controller, "firstRewardClaimedThisRun", true);
                InvokePrivate(controller, "SetPaused", true);
                InvokePrivate(controller, "RequestReturnToTitle");
                if (GetPrivate<object>(controller, "pendingExitAction").ToString() != "ReturnToTitle")
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: title return input did not require confirmation while carrying an unbanked relic.");
                }

                controller.TryReturnToTitle();
                var titleReturnSave = FourfoldProgressSave.Load();
                if (titleReturnSave.currentScene != FourfoldGameIds.SceneD020VerticalSlice)
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: return-to-title should preserve Continue target as D-020 while loading the title scene.");
                }

                if (titleReturnSave.d020RewardClaimed || titleReturnSave.d020SecondRewardClaimed)
                {
                    throw new InvalidOperationException("D-020 death/retry verifier failed: return-to-title banked unreturned run rewards.");
                }

                Debug.Log("FOURFOLD D-020 death/retry verifier passed: enemy hit, dodge invulnerability, failed-run persistence, failed-run hub return, retry, and title return are valid.");
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

            ValidateAudioFallbacks(controller, tool);
        }

        private static void ValidateAudioFallbacks(D020SliceController controller, ExplorationTool tool)
        {
            InvokePrivate(controller, "EnsureAudioSource");
            InvokePrivate(controller, "EnsureExplorationReferences");

            RequireAudioClip(controller.attackClip, "D020SliceController.attackClip");
            RequireAudioClip(controller.hitClip, "D020SliceController.hitClip");
            RequireAudioClip(controller.dodgeClip, "D020SliceController.dodgeClip");
            RequireAudioClip(controller.enemyTellClip, "D020SliceController.enemyTellClip");
            RequireAudioClip(controller.playerDamageClip, "D020SliceController.playerDamageClip");
            RequireAudioClip(controller.bossDefeatClip, "D020SliceController.bossDefeatClip");
            RequireAudioClip(controller.rewardClaimClip, "D020SliceController.rewardClaimClip");
            RequireAudioClip(controller.rewardReadyClip, "D020SliceController.rewardReadyClip");
            RequireAudioClip(controller.explorationMusicClip, "D020SliceController.explorationMusicClip");
            RequireAudioClip(controller.bossMusicClip, "D020SliceController.bossMusicClip");
            RequireAudioClip(tool.pulse, "ExplorationTool.pulse");
            RequireAudioClip(tool.targetHit, "ExplorationTool.targetHit");
            RequireAudioClip(tool.fail, "ExplorationTool.fail");
        }

        private static void RequireAudioClip(AudioClip clip, string name)
        {
            RequireReference(clip, name);
            if (clip.samples <= 0 || clip.frequency <= 0)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: {name} must contain playable PCM samples.");
            }
        }

        private static void VerifyNodeUse(ExplorationTool tool, ExplorationNode node, int index)
        {
            tool.player.position = node.transform.position;
            tool.inputEnabled = true;
            tool.cooldownSeconds = 0f;
            tool.ClearRuntimeState();
            node.ResetNode();

            if (!tool.TryUse())
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: ExplorationTool.TryUse returned false for node {index} ({node.name}).");
            }

            if (!node.IsSolved)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: node {index} ({node.name}) was not solved after TryUse.");
            }

            if (tool.LastUseResult != ExplorationToolUseResult.NodeActivated || tool.LastTarget != node || !tool.HasRecentFeedback)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: tool feedback did not report node activation for node {index} ({node.name}).");
            }

            if (node.responseTarget != null && !node.responseTarget.activeInHierarchy)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: node {index} ({node.name}) response target is not active after TryUse: {node.responseTarget.name}.");
            }
        }

        private static void VerifyReadyTarget(ExplorationTool tool, ExplorationNode node, int index)
        {
            tool.player.position = node.transform.position;
            tool.inputEnabled = true;
            tool.cooldownSeconds = 0f;
            tool.ClearRuntimeState();
            node.ResetNode();

            if (!tool.IsReady || !tool.HasReadyTarget)
            {
                throw new InvalidOperationException($"D-020 gameplay verifier failed: tool did not report a ready target for node {index} ({node.name}).");
            }
        }

        private static void VerifyToolNoTarget(ExplorationTool tool)
        {
            tool.player.position = new Vector3(999f, 0f, 999f);
            tool.inputEnabled = true;
            tool.cooldownSeconds = 0f;
            tool.ClearRuntimeState();

            if (tool.HasReadyTarget)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: tool reported a ready target while the player was far from all targets.");
            }

            if (tool.TryUse())
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: tool succeeded while the player was far from all targets.");
            }

            if (tool.LastUseResult != ExplorationToolUseResult.NoTarget || !tool.HasRecentFeedback)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: tool did not report no-target feedback after a far miss.");
            }
        }

        private static void VerifyCooldownReject(ExplorationTool tool, ExplorationNode node)
        {
            tool.player.position = node.transform.position;
            tool.inputEnabled = true;
            tool.cooldownSeconds = 1f;
            tool.feedbackSeconds = 1f;
            tool.ClearRuntimeState();
            node.ResetNode();

            if (!tool.TryUse())
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: setup use did not activate a node before cooldown check.");
            }

            if (tool.TryUse())
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: tool allowed a second use during cooldown.");
            }

            if (tool.LastUseResult != ExplorationToolUseResult.Cooldown || !tool.HasRecentFeedback)
            {
                throw new InvalidOperationException("D-020 gameplay verifier failed: tool did not report cooldown feedback after blocked reuse.");
            }

            tool.ClearRuntimeState();
            node.ResetNode();
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

        private static void PrepareControllerForCombat(D020SliceController controller)
        {
            RequireReference(controller.player, "D020SliceController.player");
            RequireReference(controller.enemies, "D020SliceController.enemies");
            SetPrivate(controller, "progressData", new FourfoldProgressData());
            SetPrivate(controller, "initialPlayerPosition", controller.player.position);
            SetPrivate(controller, "initialPlayerRotation", controller.player.rotation);
            ClearControllerProgressState(controller);

            var enemyCount = controller.enemies.Length;
            var enemyHealth = new float[enemyCount];
            var enemyAttackTimer = new float[enemyCount];
            var enemyWindupTimer = new float[enemyCount];
            var enemyAimDirections = new Vector3[enemyCount];
            var enemyAttackModes = new int[enemyCount];
            var bossEnraged = new bool[enemyCount];
            var bossOpeningTimer = new float[enemyCount];
            var initialEnemyPositions = new Vector3[enemyCount];
            var initialEnemyRotations = new Quaternion[enemyCount];
            var initialEnemyScales = new Vector3[enemyCount];

            for (var i = 0; i < enemyCount; i++)
            {
                enemyHealth[i] = InvokePrivateFloat(controller, "InitialEnemyHealth", i);
                enemyAimDirections[i] = Vector3.forward;
                enemyAttackModes[i] = EnemyNameContains(controller, i, "Boss") ? 1 : 0;
                if (controller.enemies[i] != null)
                {
                    controller.enemies[i].gameObject.SetActive(true);
                    initialEnemyPositions[i] = controller.enemies[i].position;
                    initialEnemyRotations[i] = controller.enemies[i].rotation;
                    initialEnemyScales[i] = controller.enemies[i].localScale;
                }
            }

            SetPrivate(controller, "enemyHealth", enemyHealth);
            SetPrivate(controller, "enemyAttackTimer", enemyAttackTimer);
            SetPrivate(controller, "enemyWindupTimer", enemyWindupTimer);
            SetPrivate(controller, "enemyAttackAimDirections", enemyAimDirections);
            SetPrivate(controller, "enemyAttackModes", enemyAttackModes);
            SetPrivate(controller, "bossEnraged", bossEnraged);
            SetPrivate(controller, "bossOpeningTimer", bossOpeningTimer);
            SetPrivate(controller, "initialEnemyPositions", initialEnemyPositions);
            SetPrivate(controller, "initialEnemyRotations", initialEnemyRotations);
            SetPrivate(controller, "initialEnemyScales", initialEnemyScales);
            SetPrivate(controller, "bossDefeatedThisRun", false);
            SetPrivate(controller, "bossDefeatTimer", 0f);

            if (controller.requiredToolNode != null)
            {
                controller.requiredToolNode.SetSolved(true);
            }
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

        private static void VerifyRelicIdentityEffects(D020SliceController controller)
        {
            SetPrivate(controller, "firstRewardClaimedThisRun", false);
            SetPrivate(controller, "secondRewardClaimedThisRun", false);
            var baseAttack = InvokePrivateFloat(controller, "CurrentAttackDamage", 0);
            var baseIncoming = InvokePrivateFloat(controller, "EnemyDamageFor", 0);

            SetPrivate(controller, "firstRewardClaimedThisRun", true);
            var edgeAttack = InvokePrivateFloat(controller, "CurrentAttackDamage", 0);
            var edgeText = InvokePrivateString(controller, "RelicStateText");
            if (edgeAttack <= baseAttack || edgeText.IndexOf("Lumen Edge", StringComparison.Ordinal) < 0 || edgeText.IndexOf("+DMG", StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException("D-020 combat verifier failed: first relic does not expose a distinct Lumen Edge damage role.");
            }

            SetPrivate(controller, "secondRewardClaimedThisRun", true);
            var wardIncoming = InvokePrivateFloat(controller, "EnemyDamageFor", 0);
            var bothText = InvokePrivateString(controller, "RelicStateText");
            var linkRecovery = InvokePrivateFloat(controller, "LumenLinkRecoveryAmount");
            if (wardIncoming >= baseIncoming
                || bothText.IndexOf("Lumen Link", StringComparison.Ordinal) < 0
                || bothText.IndexOf("-DMG", StringComparison.Ordinal) < 0
                || bothText.IndexOf("+HP", StringComparison.Ordinal) < 0
                || linkRecovery <= 0f)
            {
                throw new InvalidOperationException("D-020 combat verifier failed: combined relic loadout does not expose Lumen Link defense and hit-recovery synergy.");
            }

            SetPrivate(controller, "playerHealth", 50f);
            InvokePrivate(controller, "ApplyLumenLinkRecovery");
            var recoveredHealth = GetPrivate<float>(controller, "playerHealth");
            if (recoveredHealth <= 50f)
            {
                throw new InvalidOperationException("D-020 combat verifier failed: Lumen Link did not restore health on hit.");
            }

            SetPrivate(controller, "firstRewardClaimedThisRun", false);
            SetPrivate(controller, "secondRewardClaimedThisRun", false);
        }

        private static void VerifyBossToolOpening(D020SliceController controller, ExplorationTool tool)
        {
            var bossIndex = FindEnemyIndex(controller, "Boss");
            if (bossIndex < 0)
            {
                throw new InvalidOperationException("D-020 combat verifier failed: boss enemy is missing.");
            }

            var boss = controller.enemies[bossIndex];
            var openingTimers = GetPrivate<float[]>(controller, "bossOpeningTimer");
            if (openingTimers == null || bossIndex >= openingTimers.Length)
            {
                throw new InvalidOperationException("D-020 combat verifier failed: boss opening timer array is missing.");
            }

            if (controller.requiredToolNode != null)
            {
                controller.requiredToolNode.SetSolved(true);
            }

            if (controller.secondToolNode != null)
            {
                controller.secondToolNode.SetSolved(true);
            }

            var originalPosition = tool.player.position;
            var originalInputEnabled = tool.inputEnabled;
            var originalCooldown = tool.cooldownSeconds;
            var originalBossOpening = openingTimers[bossIndex];

            try
            {
                openingTimers[bossIndex] = 0f;
                var baseAttack = InvokePrivateFloat(controller, "CurrentAttackDamage", bossIndex);
                var baseCombatTextCount = InvokePrivateInt(controller, "CombatTextCount");
                tool.player.position = boss.position - Vector3.right * 1.2f;
                tool.inputEnabled = true;
                tool.cooldownSeconds = 0f;

                if (!tool.TryUse())
                {
                    throw new InvalidOperationException("D-020 combat verifier failed: exploration tool did not trigger a boss opening fallback.");
                }

                if (openingTimers[bossIndex] <= 0f)
                {
                    throw new InvalidOperationException("D-020 combat verifier failed: boss opening timer did not start after tool use.");
                }

                if (!InvokePrivateBool(controller, "AnyBossOpeningActive"))
                {
                    throw new InvalidOperationException("D-020 combat verifier failed: boss opening HUD state did not become active after tool use.");
                }

                if (InvokePrivateInt(controller, "CombatTextCount") <= baseCombatTextCount)
                {
                    throw new InvalidOperationException("D-020 combat verifier failed: boss opening did not create readable combat feedback text.");
                }

                var openingAttack = InvokePrivateFloat(controller, "CurrentAttackDamage", bossIndex);
                if (openingAttack <= baseAttack)
                {
                    throw new InvalidOperationException("D-020 combat verifier failed: boss opening did not increase player attack damage against the boss.");
                }
            }
            finally
            {
                tool.player.position = originalPosition;
                tool.inputEnabled = originalInputEnabled;
                tool.cooldownSeconds = originalCooldown;
                openingTimers[bossIndex] = originalBossOpening;
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

        private static void DefeatEnemyWithBasicAttacks(D020SliceController controller, int enemyIndex)
        {
            var enemy = controller.enemies != null && enemyIndex < controller.enemies.Length ? controller.enemies[enemyIndex] : null;
            if (enemy == null)
            {
                throw new InvalidOperationException($"D-020 combat verifier failed: missing enemy at index {enemyIndex}.");
            }

            var guard = 0;
            var baseCombatTextCount = InvokePrivateInt(controller, "CombatTextCount");
            while (GetPrivate<float[]>(controller, "enemyHealth")[enemyIndex] > 0f && guard++ < 24)
            {
                var attackDirection = Vector3.right;
                controller.player.position = enemy.position - attackDirection * 1.0f;
                SetPrivate(controller, "facing", attackDirection);
                SetPrivate(controller, "attackTimer", 0f);
                InvokePrivate(controller, "TryAttack");
            }

            if (GetPrivate<float[]>(controller, "enemyHealth")[enemyIndex] > 0f || enemy.gameObject.activeSelf)
            {
                throw new InvalidOperationException($"D-020 combat verifier failed: basic attacks did not defeat {enemy.name}.");
            }

            var combatTextCount = InvokePrivateInt(controller, "CombatTextCount");
            if (combatTextCount <= 0 || (baseCombatTextCount < 10 && combatTextCount <= baseCombatTextCount))
            {
                throw new InvalidOperationException($"D-020 combat verifier failed: defeating {enemy.name} did not create readable hit feedback text.");
            }
        }

        private static bool EnemyNameContains(D020SliceController controller, int enemyIndex, string text)
        {
            var enemy = controller.enemies != null && enemyIndex < controller.enemies.Length ? controller.enemies[enemyIndex] : null;
            return enemy != null && enemy.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FindEnemyIndex(D020SliceController controller, string text)
        {
            if (controller.enemies == null)
            {
                return -1;
            }

            for (var i = 0; i < controller.enemies.Length; i++)
            {
                if (EnemyNameContains(controller, i, text))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void MovePlayerTo(ExplorationTool tool, Transform target)
        {
            if (tool == null || tool.player == null || target == null)
            {
                throw new InvalidOperationException("D-020 full-loop verifier failed: cannot move player to a missing target.");
            }

            tool.player.position = target.position;
        }

        private static void InvokePrivate(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private method {methodName} on {target.GetType().Name}.");
            }

            try
            {
                method.Invoke(target, arguments ?? Array.Empty<object>());
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw new InvalidOperationException($"D-020 verifier failed inside {methodName}: {exception.InnerException.Message}", exception.InnerException);
            }
        }

        private static bool InvokePrivateBool(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || method.ReturnType != typeof(bool))
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private bool method {methodName} on {target.GetType().Name}.");
            }

            try
            {
                return (bool)method.Invoke(target, Array.Empty<object>());
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw new InvalidOperationException($"D-020 verifier failed inside {methodName}: {exception.InnerException.Message}", exception.InnerException);
            }
        }

        private static float InvokePrivateFloat(object target, string methodName, int argument)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || method.ReturnType != typeof(float))
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private float method {methodName} on {target.GetType().Name}.");
            }

            try
            {
                return (float)method.Invoke(target, new object[] { argument });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw new InvalidOperationException($"D-020 verifier failed inside {methodName}: {exception.InnerException.Message}", exception.InnerException);
            }
        }

        private static float InvokePrivateFloat(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || method.ReturnType != typeof(float))
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private float method {methodName} on {target.GetType().Name}.");
            }

            try
            {
                return (float)method.Invoke(target, Array.Empty<object>());
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw new InvalidOperationException($"D-020 verifier failed inside {methodName}: {exception.InnerException.Message}", exception.InnerException);
            }
        }

        private static int InvokePrivateInt(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || method.ReturnType != typeof(int))
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private int method {methodName} on {target.GetType().Name}.");
            }

            try
            {
                return (int)method.Invoke(target, Array.Empty<object>());
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw new InvalidOperationException($"D-020 verifier failed inside {methodName}: {exception.InnerException.Message}", exception.InnerException);
            }
        }

        private static string InvokePrivateString(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || method.ReturnType != typeof(string))
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private string method {methodName} on {target.GetType().Name}.");
            }

            try
            {
                return (string)method.Invoke(target, Array.Empty<object>());
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw new InvalidOperationException($"D-020 verifier failed inside {methodName}: {exception.InnerException.Message}", exception.InnerException);
            }
        }

        private static string InvokePrivateString(object target, string methodName, int argument)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
            if (method == null || method.ReturnType != typeof(string))
            {
                throw new InvalidOperationException($"D-020 verifier failed: missing private string method {methodName}(int) on {target.GetType().Name}.");
            }

            try
            {
                return (string)method.Invoke(target, new object[] { argument });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw new InvalidOperationException($"D-020 verifier failed inside {methodName}: {exception.InnerException.Message}", exception.InnerException);
            }
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
