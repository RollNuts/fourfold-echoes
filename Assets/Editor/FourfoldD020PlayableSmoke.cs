using System;
using FourfoldEchoes.Product;
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
            var shortcutNode = FindNode("D020 Exploration Tool Node");
            var secondNode = FindNode("D020 Second Tool Node");

            Require(player != null, "D-020 smoke requires a D020PlayerController.");
            Require(enemy != null, "D-020 smoke requires a D020EnemyDummy.");
            Require(tool != null, "D-020 smoke requires an ExplorationTool.");
            Require(shortcutNode != null, "D-020 smoke requires the shortcut ExplorationNode.");
            Require(secondNode != null, "D-020 smoke requires the second-room ExplorationNode.");
            Require(tool.NodeCount >= 2, "D-020 smoke requires the tool to reference two exploration nodes.");
            Require(player.attackClip != null, "D-020 player attack SFX is not assigned.");
            Require(player.hitClip != null, "D-020 player hit SFX is not assigned.");
            Require(player.enemyDefeatClip != null, "D-020 enemy defeat SFX is not assigned.");
            Require(player.dodgeClip != null, "D-020 player dodge SFX is not assigned.");
            Require(tool.pulse != null, "D-020 tool pulse SFX is not assigned.");
            Require(tool.targetHit != null, "D-020 tool target-hit SFX is not assigned.");
            Require(tool.fail != null, "D-020 tool fail SFX is not assigned.");

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

            shortcutNode.ResetNode();
            player.ResetForSmoke(shortcutNode.transform.position + new Vector3(0f, 0f, 0.55f));
            tool.ResetForSmoke();
            Require(tool.TryUse(), "Exploration tool did not activate the nearby node.");
            Require(shortcutNode.IsSolved, "Shortcut exploration node was not solved by the tool.");

            secondNode.ResetNode();
            player.ResetForSmoke(secondNode.transform.position + new Vector3(0f, 0f, 0.55f));
            tool.ResetForSmoke();
            Require(tool.TryUse(), "Exploration tool did not activate the second room node.");
            Require(secondNode.IsSolved, "Second room exploration node was not solved by the same tool.");

            Debug.Log("FOURFOLD D-020 playable smoke passed: movement, dodge, attack, enemy hit, and two one-tool node activations.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static ExplorationNode FindNode(string name)
        {
            var nodes = UnityEngine.Object.FindObjectsByType<ExplorationNode>(FindObjectsSortMode.None);
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null && nodes[i].name == name)
                {
                    return nodes[i];
                }
            }

            return null;
        }
    }
}
