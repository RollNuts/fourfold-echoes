using System.Collections.Generic;
using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.EditMode
{
    public sealed class ExplorationNodeTests
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
        public void CORE_EXPLORATION_NODE_CanUse_UsesLargestOfToolRangeAndNodeRadius()
        {
            var user = CreateObject("User").transform;
            var nodeObject = CreateObject("Node");
            nodeObject.transform.position = new Vector3(2.3f, 0f, 0f);

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.activationRadius = 2.4f;

            Assert.That(node.CanUse(user, 0.5f), Is.True);

            nodeObject.transform.position = new Vector3(2.55f, 0f, 0f);

            Assert.That(node.CanUse(user, 0.5f), Is.False);
            Assert.That(node.CanUse(null, 99f), Is.False);
        }

        [Test]
        public void CORE_EXPLORATION_NODE_TryActivate_RevealsResponseAndLocksNode()
        {
            var user = CreateObject("User").transform;
            var nodeObject = CreateObject("Node");
            var response = CreateObject("Shortcut Response");
            var idleRead = CreateObject("Idle Read");
            var activeRead = CreateObject("Active Read");
            var highlight = response.AddComponent<MeshRenderer>();

            user.position = Vector3.zero;
            nodeObject.transform.position = Vector3.right;

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.activationRadius = 2.4f;
            node.responseTarget = response;
            node.idleRead = idleRead;
            node.activeRead = activeRead;
            node.highlightRenderers = new Renderer[] { highlight, null };
            node.ResetNode();

            Assert.That(response.activeSelf, Is.False);
            Assert.That(idleRead.activeSelf, Is.True);
            Assert.That(activeRead.activeSelf, Is.False);
            Assert.That(highlight.enabled, Is.False);

            Assert.That(node.TryActivate(user, 1.25f), Is.True);

            Assert.That(node.Used, Is.True);
            Assert.That(node.IsSolved, Is.True);
            Assert.That(response.activeSelf, Is.True);
            Assert.That(idleRead.activeSelf, Is.False);
            Assert.That(activeRead.activeSelf, Is.True);
            Assert.That(highlight.enabled, Is.True);
            Assert.That(node.TryActivate(user, 99f), Is.False);
        }

        [Test]
        public void CORE_EXPLORATION_NODE_ResetNode_PreservesRevealOnAwakePresentationWithoutSolving()
        {
            var nodeObject = CreateObject("Node");
            var response = CreateObject("Always Visible Response");
            var idleRead = CreateObject("Idle Read");
            var activeRead = CreateObject("Active Read");

            var node = nodeObject.AddComponent<ExplorationNode>();
            node.revealOnAwake = true;
            node.responseTarget = response;
            node.idleRead = idleRead;
            node.activeRead = activeRead;

            node.SetSolved(true);
            node.ResetNode();

            Assert.That(node.Used, Is.False);
            Assert.That(node.IsSolved, Is.False);
            Assert.That(response.activeSelf, Is.True);
            Assert.That(idleRead.activeSelf, Is.False);
            Assert.That(activeRead.activeSelf, Is.True);
        }

        private GameObject CreateObject(string name)
        {
            var gameObject = new GameObject(name);
            createdObjects.Add(gameObject);
            return gameObject;
        }
    }
}
