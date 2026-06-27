using System.Collections.Generic;
using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ExplorationToolTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (var i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void CORE_EXPLORATION_TOOL_DefaultLiveInputIncludesControllerAndMouseUse()
        {
            var toolObject = CreateObject("Exploration Tool");
            var tool = toolObject.AddComponent<ExplorationTool>();

            Assert.That(tool.useKey, Is.EqualTo(KeyCode.E));
            Assert.That(tool.controllerUseKey, Is.EqualTo(KeyCode.JoystickButton3));
            Assert.That(tool.mouseUseButton, Is.EqualTo(1));
        }

        [Test]
        public void CORE_EXPLORATION_TOOL_TryUse_HandlesMissingNodesWithoutCooldownWhenDisabled()
        {
            var toolObject = CreateObject("Exploration Tool");
            var tool = toolObject.AddComponent<ExplorationTool>();

            tool.player = CreateObject("Player").transform;
            tool.nodes = null;
            tool.cooldownSeconds = 0f;

            Assert.That(tool.TryUse(), Is.False);
            Assert.That(tool.IsReady, Is.True);
            Assert.That(tool.Cooldown01, Is.EqualTo(0f));
        }

        [Test]
        public void CORE_EXPLORATION_TOOL_TryUse_ActivatesNearestUsableNodeOnly()
        {
            var player = CreateObject("Player").transform;
            var toolObject = CreateObject("Exploration Tool");
            var tool = toolObject.AddComponent<ExplorationTool>();
            var nearNode = CreateNode("Near Node", new Vector3(1f, 0f, 0f));
            var farNode = CreateNode("Far Node", new Vector3(2.25f, 0f, 0f));

            tool.player = player;
            tool.range = 3f;
            tool.cooldownSeconds = 0f;
            tool.nodes = new[] { farNode, nearNode };

            Assert.That(tool.TryUse(), Is.True);

            Assert.That(nearNode.IsSolved, Is.True);
            Assert.That(farNode.IsSolved, Is.False);
        }

        [Test]
        public void CORE_EXPLORATION_TOOL_TryUse_ConsumesCooldownAfterMiss()
        {
            var player = CreateObject("Player").transform;
            var toolObject = CreateObject("Exploration Tool");
            var tool = toolObject.AddComponent<ExplorationTool>();
            var node = CreateNode("Initially Out Of Range Node", new Vector3(5f, 0f, 0f));

            tool.player = player;
            tool.range = 1f;
            tool.cooldownSeconds = 0.5f;
            tool.nodes = new[] { node };

            Assert.That(tool.TryUse(), Is.False);
            Assert.That(tool.IsReady, Is.False);
            Assert.That(tool.Cooldown01, Is.GreaterThan(0f));

            node.transform.position = new Vector3(0.5f, 0f, 0f);

            Assert.That(tool.TryUse(), Is.False);
            Assert.That(node.IsSolved, Is.False);
        }

        private ExplorationNode CreateNode(string name, Vector3 position)
        {
            var nodeObject = CreateObject(name);
            var response = CreateObject(name + " Response");
            nodeObject.transform.position = position;

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.responseTarget = response;
            node.ResetNode();
            return node;
        }

        private GameObject CreateObject(string name)
        {
            var gameObject = new GameObject(name);
            createdObjects.Add(gameObject);
            return gameObject;
        }
    }
}
