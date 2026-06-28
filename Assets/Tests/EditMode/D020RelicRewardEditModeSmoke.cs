using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Tests
{
    public static class D020RelicRewardEditModeSmoke
    {
        public static void Run()
        {
            var objects = new GameObject[8];
            try
            {
                objects[0] = new GameObject("D020 Test Collector");
                objects[1] = new GameObject("D020 Test Reward");
                objects[2] = new GameObject("D020 Test Shortcut Node");
                objects[3] = new GameObject("D020 Test Reward Lens Node");
                objects[4] = new GameObject("D020 Test Shortcut Reward");
                objects[5] = new GameObject("D020 Test Progress Save");
                objects[6] = new GameObject("D020 Test Critical Enemy");
                objects[7] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                objects[7].name = "D020 Test Critical Tell";

                objects[0].transform.position = new Vector3(0f, 0f, 0.35f);
                var reward = objects[1].AddComponent<D020RelicReward>();
                var shortcutNode = objects[2].AddComponent<ExplorationNode>();
                var rewardLensNode = objects[3].AddComponent<ExplorationNode>();
                var shortcutReward = objects[4].AddComponent<D020RelicReward>();
                var progressSave = objects[5].AddComponent<D020ProgressSave>();

                reward.player = objects[0].transform;
                reward.rewardId = "d020.test.relic.01";
                reward.pickupRadius = 1f;
                reward.autoCollectOnTouch = false;
                reward.requiredNode = shortcutNode;
                reward.requiredNodes = new[] { rewardLensNode };

                shortcutReward.player = objects[0].transform;
                shortcutReward.rewardId = "d020.test.relic.02";
                shortcutReward.pickupRadius = 1f;
                shortcutReward.autoCollectOnTouch = false;
                shortcutReward.requiredNode = shortcutNode;

                shortcutNode.ResetNode();
                rewardLensNode.ResetNode();
                reward.ResetReward();
                shortcutReward.ResetReward();

                Require(!reward.IsUnlocked, "Reward unlocked before either one-tool node was solved.");
                Require(!reward.TryCollect(objects[0].transform), "Reward collected before one-tool conditions were solved.");

                shortcutNode.SetSolved(true);
                Require(!reward.IsUnlocked, "Reward unlocked after only the shortcut node was solved.");

                rewardLensNode.SetSolved(true);
                Require(reward.IsUnlocked, "Reward did not unlock after both one-tool nodes were solved.");
                Require(reward.TryCollect(objects[0].transform), "Reward did not collect after both one-tool nodes were solved.");
                Require(reward.IsCollected, "Reward collection state did not persist after pickup.");

                reward.ResetReward();
                shortcutNode.ResetNode();
                rewardLensNode.SetSolved(true);
                Require(!reward.IsUnlocked, "Primary requiredNode was not enforced with requiredNodes present.");

                reward.requiredNode = null;
                Require(reward.IsUnlocked, "requiredNodes-only reward did not unlock when its node was solved.");

                reward.requiredNodes = new ExplorationNode[] { null };
                Require(reward.IsUnlocked, "Null optional required node entries should not block reward unlock.");
                Require(D020HudController.RoomTitleText.Contains("D020"), "D-020 HUD title must identify the active vertical slice.");
                Require(!D020HudController.RoomTitleText.Contains("Gate A"), "D-020 HUD title must not regress to legacy Gate A copy.");

                var enemy = objects[6].AddComponent<D020EnemyDummy>();
                enemy.maxHealth = 3;
                enemy.criticalHealthThreshold = 1;
                enemy.tellRead = objects[7];
                enemy.ResetEnemy();
                enemy.Tick(0.1f);
                var healthyTellScale = objects[7].transform.localScale.x;
                Require(!enemy.IsCriticalHealth, "Enemy started in critical health read.");

                enemy.TakeHit(1);
                Require(!enemy.IsCriticalHealth, "Enemy entered critical health before the final readable hit.");

                enemy.TakeHit(1);
                enemy.Tick(0.1f);
                Require(enemy.IsCriticalHealth, "Enemy did not expose a critical health read on its final hit.");
                Require(objects[7].transform.localScale.x > healthyTellScale, "Critical health read did not enlarge the enemy tell ring.");

                var player = objects[0].AddComponent<D020PlayerController>();
                player.ResetForSmoke(Vector3.zero);

                var hud = objects[0].AddComponent<D020HudController>();
                hud.player = player;
                hud.enemy = enemy;
                hud.RefreshNow();
                Require(hud.ActionRead.Contains("Atk Ready"), "D-020 HUD did not surface attack readiness.");
                Require(hud.ActionRead.Contains("Dodge Ready"), "D-020 HUD did not surface dodge readiness.");
                Require(hud.EnemyRead.Contains("Critical"), "D-020 HUD did not surface the enemy critical read.");

                player.TryDodge(Vector2.up);
                hud.RefreshNow();
                Require(hud.ActionRead.Contains("Dodge 0%"), "D-020 HUD did not surface dodge cooldown progress.");

                enemy.TakeHit(1);
                hud.RefreshNow();
                Require(hud.EnemyRead.Contains("Down"), "D-020 HUD did not surface the defeated enemy read.");

                shortcutNode.SetSolved(true);
                rewardLensNode.SetSolved(true);
                reward.SetCollected(true);
                shortcutReward.SetCollected(true);

                var smokeSavePath = Path.Combine(Path.GetTempPath(), "fourfold-d020-reward-editmode-smoke.json");
                TryDeleteSmokeSave(smokeSavePath);
                progressSave.overrideFilePath = smokeSavePath;
                progressSave.loadOnAwake = false;
                progressSave.saveOnProgressChanged = false;
                progressSave.nodes = new[] { shortcutNode, rewardLensNode };
                progressSave.nodeIds = new[] { "d020.test.shortcut", "d020.test.reward_lens" };
                progressSave.rewards = new[] { reward, shortcutReward };
                progressSave.rewardIds = new[] { "d020.test.relic.01", "d020.test.relic.02" };

                Require(progressSave.CollectedRewardCount == 2, "Progress save did not count both collected rewards before save.");
                Require(progressSave.SaveNow(), "Progress save failed to write two reward flags.");

                shortcutNode.ResetNode();
                rewardLensNode.ResetNode();
                reward.ResetReward();
                shortcutReward.ResetReward();
                Require(progressSave.LoadNow(), "Progress save failed to load two reward flags.");
                Require(shortcutNode.IsSolved && rewardLensNode.IsSolved, "Progress save did not restore both node flags.");
                Require(reward.IsCollected && shortcutReward.IsCollected, "Progress save did not restore both reward flags.");
                Require(progressSave.CollectedRewardCount == 2, "Progress save did not count both restored rewards.");
                Require(progressSave.ClearSave(false), "Progress save cleanup failed.");
            }
            finally
            {
                for (var i = 0; i < objects.Length; i++)
                {
                    if (objects[i] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(objects[i]);
                    }
                }
            }

            Debug.Log("FOURFOLD D-020 reward edit-mode smoke passed: reward unlock requires every configured one-tool node, enemy critical health read is preserved, and progress save restores two reward flags without adding packages.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void TryDeleteSmokeSave(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
