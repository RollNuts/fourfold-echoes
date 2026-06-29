using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeRunStateTests
    {
        [Test]
        public void NewRunState_StartsInTraversalWithoutLootOrDanger()
        {
            var state = new BuilderPrototypeRunState();

            Assert.That(state.Mode, Is.EqualTo(BuilderPrototypeMode.Traverse));
            Assert.That(state.CarriedLootValue, Is.EqualTo(0));
            Assert.That(state.DangerTier, Is.EqualTo(0));
            Assert.IsFalse(state.IsInHookMode);
            Assert.IsFalse(state.IsCarryingLoot);
        }

        [Test]
        public void HookModes_KeepBuildSeparateFromLaterReservedSystems()
        {
            Assert.That(BuilderPrototypeSpineController.PromptFor(BuilderPrototypeMode.BuildHook), Does.Contain("Place A/J"));
            Assert.That(BuilderPrototypeSpineController.PromptFor(BuilderPrototypeMode.CombatHook), Does.Contain("reserved for PR-03"));
            Assert.That(BuilderPrototypeSpineController.PromptFor(BuilderPrototypeMode.LootHook), Does.Contain("collect prototype cache"));
            Assert.That(BuilderPrototypeSpineController.PromptFor(BuilderPrototypeMode.ExtractHook), Does.Contain("bank A/J"));
        }

        [Test]
        public void SetMode_TracksTheCurrentPrototypeHook()
        {
            var state = new BuilderPrototypeRunState();

            state.SetMode(BuilderPrototypeMode.BuildHook);
            Assert.That(state.Mode, Is.EqualTo(BuilderPrototypeMode.BuildHook));
            Assert.IsTrue(state.IsInHookMode);

            state.SetMode(BuilderPrototypeMode.Traverse);
            Assert.That(state.Mode, Is.EqualTo(BuilderPrototypeMode.Traverse));
            Assert.IsFalse(state.IsInHookMode);
        }

        [Test]
        public void PrototypeLootAndDanger_ClampToNonNegativeRunValues()
        {
            var state = new BuilderPrototypeRunState();

            state.SetCarriedLootForPrototype(-100);
            state.SetDangerForPrototype(-2);
            Assert.That(state.CarriedLootValue, Is.EqualTo(0));
            Assert.That(state.DangerTier, Is.EqualTo(0));

            state.SetCarriedLootForPrototype(120);
            state.SetDangerForPrototype(99);
            Assert.That(state.CarriedLootValue, Is.EqualTo(120));
            Assert.That(state.DangerTier, Is.EqualTo(BuilderPrototypeRunState.MaxDangerTier));
        }

        [Test]
        public void BankAndResetRun_MovesCarriedLootToBankAndClearsRunPressure()
        {
            var state = new BuilderPrototypeRunState();
            state.SetMode(BuilderPrototypeMode.ExtractHook);
            state.AddCarriedLoot(45);
            state.RaiseDanger(3);

            var banked = state.BankAndResetRun();

            Assert.That(banked, Is.EqualTo(45));
            Assert.That(state.BankedLootValue, Is.EqualTo(45));
            Assert.That(state.CarriedLootValue, Is.EqualTo(0));
            Assert.That(state.DangerTier, Is.EqualTo(0));
            Assert.That(state.Mode, Is.EqualTo(BuilderPrototypeMode.Traverse));
        }

        [Test]
        public void ClampToArena_PreservesHeightAndConstrainsHorizontalBounds()
        {
            var clamped = BuilderPrototypeSpineController.ClampToArena(
                new Vector3(9f, 1.25f, -9f),
                new Vector2(-6f, 6f),
                new Vector2(-4f, 4f));

            Assert.That(clamped.x, Is.EqualTo(6f).Within(0.001f));
            Assert.That(clamped.y, Is.EqualTo(1.25f).Within(0.001f));
            Assert.That(clamped.z, Is.EqualTo(-4f).Within(0.001f));
        }
    }
}
