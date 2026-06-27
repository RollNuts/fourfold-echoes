using System.Collections;
using FourfoldEchoes.Product;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FourfoldEchoes.Tests.PlayMode
{
    public sealed class ProductionCombatRewardFocusPlayModeTests
    {
        private GameObject reward;
        private ProductionCombatRewardFocus focus;

        [SetUp]
        public void SetUp()
        {
            reward = GameObject.CreatePrimitive(PrimitiveType.Cube);
            reward.name = "Reward Focus Test Chest";
            reward.transform.localScale = Vector3.one;

            var host = new GameObject("Reward Focus Test Host");
            focus = host.AddComponent<ProductionCombatRewardFocus>();
            focus.rewardTarget = reward.transform;
            focus.pulseAmplitude = 0.12f;
            focus.pulseFrequency = 1.5f;
            focus.settleSpeed = 99f;
        }

        [TearDown]
        public void TearDown()
        {
            if (focus != null)
            {
                Object.Destroy(focus.gameObject);
            }

            if (reward != null)
            {
                Object.Destroy(reward);
            }
        }

        [UnityTest]
        public IEnumerator RewardFocus_PulsesOnlyWhenGateOpenAndRewardWaiting()
        {
            focus.ApplyFocus(0.25f, gateOpen: false, rewardClaimed: false);
            Assert.That(focus.IsFocusing, Is.False);
            Assert.That(reward.transform.localScale.x, Is.EqualTo(1f).Within(0.001f));

            focus.ApplyFocus(0.25f, gateOpen: true, rewardClaimed: false);
            Assert.That(focus.IsFocusing, Is.True);
            Assert.That(reward.transform.localScale.x, Is.GreaterThan(1f));

            focus.ApplyFocus(0.25f, gateOpen: true, rewardClaimed: true);
            Assert.That(focus.IsFocusing, Is.False);

            reward.transform.localScale = Vector3.one * 1.08f;
            focus.ApplyFocus(0.25f, gateOpen: false, rewardClaimed: false);
            Assert.That(reward.transform.localScale.x, Is.EqualTo(1f).Within(0.001f));

            yield return null;
        }
    }
}
