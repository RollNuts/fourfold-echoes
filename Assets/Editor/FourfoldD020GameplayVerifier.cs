using System;
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
