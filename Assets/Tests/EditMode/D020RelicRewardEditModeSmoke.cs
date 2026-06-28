using System;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Tests
{
    public static class D020RelicRewardEditModeSmoke
    {
        public static void Run()
        {
            var objects = new GameObject[6];
            try
            {
                objects[0] = new GameObject("D020 Test Collector");
                objects[1] = new GameObject("D020 Test Reward");
                objects[2] = new GameObject("D020 Test Shortcut Node");
                objects[3] = new GameObject("D020 Test Reward Lens Node");
                objects[4] = new GameObject("D020 Test Critical Enemy");
                objects[5] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                objects[5].name = "D020 Test Critical Tell";

                objects[0].transform.position = new Vector3(0f, 0f, 0.35f);
                var reward = objects[1].AddComponent<D020RelicReward>();
                var shortcutNode = objects[2].AddComponent<ExplorationNode>();
                var rewardLensNode = objects[3].AddComponent<ExplorationNode>();

                reward.player = objects[0].transform;
                reward.pickupRadius = 1f;
                reward.autoCollectOnTouch = false;
                reward.requiredNode = shortcutNode;
                reward.requiredNodes = new[] { rewardLensNode };

                shortcutNode.ResetNode();
                rewardLensNode.ResetNode();
                reward.ResetReward();

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

                var enemy = objects[4].AddComponent<D020EnemyDummy>();
                enemy.maxHealth = 3;
                enemy.criticalHealthThreshold = 1;
                enemy.tellRead = objects[5];
                enemy.ResetEnemy();
                enemy.Tick(0.1f);
                var healthyTellScale = objects[5].transform.localScale.x;
                Require(!enemy.IsCriticalHealth, "Enemy started in critical health read.");

                enemy.TakeHit(1);
                Require(!enemy.IsCriticalHealth, "Enemy entered critical health before the final readable hit.");

                enemy.TakeHit(1);
                enemy.Tick(0.1f);
                Require(enemy.IsCriticalHealth, "Enemy did not expose a critical health read on its final hit.");
                Require(objects[5].transform.localScale.x > healthyTellScale, "Critical health read did not enlarge the enemy tell ring.");

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

            Debug.Log("FOURFOLD D-020 reward edit-mode smoke passed: reward unlock requires every configured one-tool node without adding packages.");
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
