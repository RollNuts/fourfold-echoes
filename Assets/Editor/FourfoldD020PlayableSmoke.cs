using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldD020PlayableSmoke
    {
        public static void Run()
        {
            EditorSceneManager.OpenScene(FourfoldD020SliceSceneBuilder.ScenePath, OpenSceneMode.Single);

            var player = UnityEngine.Object.FindFirstObjectByType<D020PlayerController>();
            var enemy = UnityEngine.Object.FindFirstObjectByType<D020EnemyDummy>();
            var tool = UnityEngine.Object.FindFirstObjectByType<ExplorationTool>();
            var node = UnityEngine.Object.FindFirstObjectByType<ExplorationNode>();
            var reward = UnityEngine.Object.FindFirstObjectByType<D020RelicReward>();
            var progressSave = UnityEngine.Object.FindFirstObjectByType<D020ProgressSave>();
            var hud = UnityEngine.Object.FindFirstObjectByType<D020HudController>();
            var shortcut = FindSceneObject("D020 Shortcut Route");
            var camera = Camera.main;

            Require(player != null, "D-020 smoke requires a D020PlayerController.");
            Require(enemy != null, "D-020 smoke requires a D020EnemyDummy.");
            Require(tool != null, "D-020 smoke requires an ExplorationTool.");
            Require(node != null, "D-020 smoke requires an ExplorationNode.");
            Require(reward != null, "D-020 smoke requires one collectible reward.");
            Require(progressSave != null, "D-020 smoke requires a progress save component.");
            Require(hud != null, "D-020 smoke requires a minimal HUD component.");
            Require(shortcut != null, "D-020 smoke requires one shortcut response object.");
            Require(camera != null && camera.orthographic, "D-020 smoke requires a fixed orthographic top-down camera.");
            Require(reward.GetComponentsInChildren<Renderer>(true).Length > 0, "D-020 reward has no readable renderer.");
            Require(player.useControllerAxes, "D-020 player movement must read controller axes.");
            Require(player.controllerAttackKey == KeyCode.JoystickButton0, "D-020 player attack must expose controller South Button input.");
            Require(player.controllerDodgeKey == KeyCode.JoystickButton1, "D-020 player dodge must expose controller East Button input.");
            Require(tool.controllerUseKey == KeyCode.JoystickButton3, "D-020 exploration tool must expose controller North Button input.");
            RequireCoreSfx();

            hud.RefreshNow();
            Require(hud.ToolRead == "Tool Ready", "HUD did not expose initial tool readiness.");
            Require(hud.RewardRead == "Relic Locked", "HUD did not expose initial reward lock state.");
            Require(hud.PromptRead == "Use tool on sigil", "HUD did not expose the initial one-tool prompt.");

            var start = player.transform.position;
            player.Tick(new Vector2(1f, 1f), false, false, 0.5f);
            Require(Vector3.Distance(start, player.transform.position) > 0.25f, "Player did not move during smoke input.");

            var beforeDodge = player.transform.position;
            Require(player.TryDodge(Vector2.left), "Player dodge command was rejected.");
            Require(player.DodgeCount == 1, "Player dodge count did not increment.");
            Require(Vector3.Distance(beforeDodge, player.transform.position) > 0.5f, "Player dodge did not move enough to read.");

            enemy.ResetEnemy();
            player.ResetForSmoke(enemy.transform.position + new Vector3(0f, 0f, -0.95f));
            Require(player.TryAttack(), "Player attack did not hit the nearby enemy.");
            Require(player.AttackCount == 1, "Player attack count did not increment.");
            Require(player.AttackHitCount == 1, "Player attack hit count did not increment.");
            Require(enemy.HitCount == 1, "Enemy did not record the attack hit.");
            player.Tick(Vector2.zero, false, false, 1f);
            Require(player.TryAttack(), "Player second attack did not hit the nearby enemy.");
            player.Tick(Vector2.zero, false, false, 1f);
            Require(player.TryAttack(), "Player third attack did not hit the nearby enemy.");
            Require(enemy.IsDefeated, "Enemy was not defeated by repeated normal attacks.");

            node.ResetNode();
            Require(!shortcut.activeSelf, "Shortcut response should start hidden before tool use.");
            player.ResetForSmoke(node.transform.position + new Vector3(0f, 0f, 0.55f));
            Require(tool.TryUse(), "Exploration tool did not activate the nearby node.");
            Require(node.IsSolved, "Exploration node was not solved by the tool.");
            Require(node.responseTarget != null && node.responseTarget.activeSelf, "Exploration tool did not reveal the shortcut response.");
            Require(node.activeRead != null && node.activeRead.activeSelf, "Exploration tool did not reveal the node active read.");
            hud.RefreshNow();
            Require(hud.ToolRead != "Tool Ready", "HUD did not expose tool cooldown after use.");

            reward.ResetReward();
            player.ResetForSmoke(reward.transform.position + new Vector3(0f, 0f, -0.55f));
            Require(reward.TryCollect(player.transform), "Reward pickup did not collect in range.");
            Require(reward.IsCollected, "Reward did not stay collected after pickup.");
            Require(reward.idleRead == null || !reward.idleRead.activeSelf, "Reward idle read stayed visible after pickup.");
            Require(reward.collectedRead == null || reward.collectedRead.activeSelf, "Reward collected read did not activate after pickup.");
            hud.RefreshNow();
            Require(hud.RewardRead == "Relic Claimed", "HUD did not expose collected reward state.");
            Require(hud.PromptRead == "Relic secured", "HUD did not expose the secured relic prompt.");

            var smokeSavePath = Path.Combine(Path.GetTempPath(), "fourfold-d020-progress-smoke.json");
            TryDeleteSmokeSave(smokeSavePath);
            progressSave.overrideFilePath = smokeSavePath;
            Require(progressSave.SaveNow(), "Progress save did not write solved shortcut and collected reward state.");
            hud.RefreshNow();
            Require(hud.ProgressRead == "Progress S1 R1", "HUD did not expose saved shortcut and reward counts.");
            node.ResetNode();
            reward.ResetReward();
            Require(!node.IsSolved && !reward.IsCollected, "Progress reset setup failed before load.");
            Require(progressSave.LoadNow(), "Progress save did not load the saved state.");
            Require(node.IsSolved, "Progress save did not restore the shortcut flag.");
            Require(reward.IsCollected, "Progress save did not restore the collected reward flag.");
            hud.RefreshNow();
            Require(hud.ProgressRead == "Progress S1 R1", "HUD did not expose loaded shortcut and reward counts.");
            Require(progressSave.ClearSave(false), "Progress save cleanup failed.");

            Debug.Log("FOURFOLD D-020 playable smoke passed: movement, fixed camera, dodge, normal attack, enemy defeat, one-tool shortcut response, reward pickup, minimal HUD, local progress save/load, and core SFX assets.");
        }

        private static void RequireCoreSfx()
        {
            RequireAudioClip("Assets/Audio/Generated/attack_basic.wav");
            RequireAudioClip("Assets/Audio/Generated/dodge.wav");
            RequireAudioClip("Assets/Audio/Generated/hit_enemy.wav");
            RequireAudioClip("Assets/Audio/Generated/tool_pulse.wav");
            RequireAudioClip("Assets/Audio/Generated/shortcut_open.wav");
            RequireAudioClip("Assets/Audio/Generated/relic_pickup.wav");
        }

        private static void RequireAudioClip(string path)
        {
            Require(AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null, $"Core SFX clip missing or not imported: {path}");
        }

        private static GameObject FindSceneObject(string name)
        {
            var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < transforms.Length; index++)
            {
                var target = transforms[index];
                if (target != null && string.Equals(target.name, name, StringComparison.Ordinal))
                {
                    return target.gameObject;
                }
            }

            return null;
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
            if (!File.Exists(path))
            {
                return;
            }

            File.Delete(path);
        }
    }
}
