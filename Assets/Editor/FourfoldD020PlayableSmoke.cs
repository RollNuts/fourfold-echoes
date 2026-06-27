using System;
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
            var reward = FindSceneObject("D020 Relic Chest");
            var shortcut = FindSceneObject("D020 Shortcut Route");
            var camera = Camera.main;

            Require(player != null, "D-020 smoke requires a D020PlayerController.");
            Require(enemy != null, "D-020 smoke requires a D020EnemyDummy.");
            Require(tool != null, "D-020 smoke requires an ExplorationTool.");
            Require(node != null, "D-020 smoke requires an ExplorationNode.");
            Require(reward != null, "D-020 smoke requires one visible reward chest.");
            Require(shortcut != null, "D-020 smoke requires one shortcut response object.");
            Require(camera != null && camera.orthographic, "D-020 smoke requires a fixed orthographic top-down camera.");
            Require(reward.GetComponentsInChildren<Renderer>(true).Length > 0, "D-020 reward has no readable renderer.");
            RequireCoreSfx();

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

            Debug.Log("FOURFOLD D-020 playable smoke passed: movement, fixed camera, dodge, normal attack, enemy defeat, one-tool shortcut response, reward presence, and core SFX assets.");
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
    }
}
