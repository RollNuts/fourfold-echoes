using FourfoldEchoes.BuilderPrototype;
using NUnit.Framework;
using UnityEngine;

namespace FourfoldEchoes.Tests.BuilderPrototype
{
    public sealed class BuilderPrototypeTacticalModelTests
    {
        [Test]
        public void CastWindow_ReportsPendingTelegraphResolvingAndExpiredStates()
        {
            var window = new BuilderPrototypeCastWindow(1f, 3f, 0.5f);

            Assert.That(window.TelegraphDuration, Is.EqualTo(2f).Within(0.001f));
            Assert.That(window.StateAt(0.5f), Is.EqualTo(BuilderPrototypeTelegraphTimingState.Pending));
            Assert.That(window.StateAt(2f), Is.EqualTo(BuilderPrototypeTelegraphTimingState.Telegraphing));
            Assert.That(window.StateAt(3.25f), Is.EqualTo(BuilderPrototypeTelegraphTimingState.Resolving));
            Assert.That(window.StateAt(3.6f), Is.EqualTo(BuilderPrototypeTelegraphTimingState.Expired));
        }

        [Test]
        public void TelegraphShapes_ContainExpectedTopDownPositions()
        {
            var window = new BuilderPrototypeCastWindow(0f, 1f);
            var circle = BuilderPrototypeTelegraphZone.Circle(Vector2.zero, 2f, window);
            var donut = BuilderPrototypeTelegraphZone.Donut(Vector2.zero, 1f, 3f, window);
            var rectangle = BuilderPrototypeTelegraphZone.Rectangle(Vector2.zero, new Vector2(2f, 4f), Vector2.up, window);
            var cone = BuilderPrototypeTelegraphZone.Cone(Vector2.zero, 4f, 90f, Vector2.up, window);

            Assert.IsTrue(circle.Contains(new Vector2(1.8f, 0f)));
            Assert.IsFalse(circle.Contains(new Vector2(2.2f, 0f)));

            Assert.IsFalse(donut.Contains(new Vector2(0.25f, 0f)));
            Assert.IsTrue(donut.Contains(new Vector2(0f, 2f)));
            Assert.IsFalse(donut.Contains(new Vector2(0f, 3.5f)));

            Assert.IsTrue(rectangle.Contains(new Vector2(0.9f, 1.9f)));
            Assert.IsFalse(rectangle.Contains(new Vector2(1.1f, 0f)));

            Assert.IsTrue(cone.Contains(new Vector2(1f, 2f)));
            Assert.IsFalse(cone.Contains(new Vector2(2f, -1f)));
        }

        [Test]
        public void EvaluatePosition_ThreatensDuringTelegraphAndBecomesUnsafeDuringResolve()
        {
            var model = new BuilderPrototypeTacticalModel();
            model.AddTelegraphZone(BuilderPrototypeTelegraphZone.Circle(
                Vector2.zero,
                2f,
                new BuilderPrototypeCastWindow(0f, 2f, 1f)));

            var telegraphing = model.EvaluatePosition(new Vector2(1f, 0f), 1f);
            Assert.That(telegraphing.Safety, Is.EqualTo(BuilderPrototypePositionSafety.Threatened));
            Assert.That(telegraphing.ThreatenedZoneCount, Is.EqualTo(1));
            Assert.That(telegraphing.UnsafeZoneCount, Is.EqualTo(0));
            Assert.That(telegraphing.SecondsUntilUnsafe, Is.EqualTo(1f).Within(0.001f));

            var resolving = model.EvaluatePosition(new Vector2(1f, 0f), 2.25f);
            Assert.That(resolving.Safety, Is.EqualTo(BuilderPrototypePositionSafety.Unsafe));
            Assert.That(resolving.UnsafeZoneCount, Is.EqualTo(1));
            Assert.That(resolving.SecondsUntilUnsafe, Is.EqualTo(0f).Within(0.001f));

            var expired = model.EvaluatePosition(new Vector2(1f, 0f), 3.25f);
            Assert.That(expired.Safety, Is.EqualTo(BuilderPrototypePositionSafety.Safe));
            Assert.That(expired.SecondsUntilUnsafe, Is.EqualTo(-1f).Within(0.001f));
        }

        [Test]
        public void EvaluatePosition_UnsafeZonesWinOverOverlappingThreats()
        {
            var model = new BuilderPrototypeTacticalModel();
            model.AddTelegraphZone(BuilderPrototypeTelegraphZone.Circle(
                Vector2.zero,
                3f,
                new BuilderPrototypeCastWindow(0f, 5f, 1f)));
            model.AddTelegraphZone(BuilderPrototypeTelegraphZone.Rectangle(
                Vector2.zero,
                new Vector2(2f, 6f),
                Vector2.up,
                new BuilderPrototypeCastWindow(0f, 1f, 1f)));

            var evaluation = model.EvaluatePosition(new Vector2(0f, 1f), 1.5f);

            Assert.That(evaluation.Safety, Is.EqualTo(BuilderPrototypePositionSafety.Unsafe));
            Assert.That(evaluation.UnsafeZoneCount, Is.EqualTo(1));
            Assert.That(evaluation.ThreatenedZoneCount, Is.EqualTo(1));
            Assert.That(evaluation.SecondsUntilUnsafe, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void EvaluatePosition_PendingTelegraphsCanReportFutureUnsafeTimeWhileStillSafe()
        {
            var model = new BuilderPrototypeTacticalModel();
            model.AddTelegraphZone(BuilderPrototypeTelegraphZone.Cone(
                Vector2.zero,
                4f,
                120f,
                Vector2.up,
                new BuilderPrototypeCastWindow(2f, 5f, 0.5f)));

            var evaluation = model.EvaluatePosition(new Vector2(0f, 3f), 1f);

            Assert.That(evaluation.Safety, Is.EqualTo(BuilderPrototypePositionSafety.Safe));
            Assert.IsTrue(evaluation.IsSafe);
            Assert.That(evaluation.SecondsUntilUnsafe, Is.EqualTo(4f).Within(0.001f));
        }

        [Test]
        public void EvaluatePositionalBonus_DistinguishesFrontFlankAndRear()
        {
            var targetPosition = Vector2.zero;
            var targetFacing = Vector2.up;

            Assert.That(
                BuilderPrototypeTacticalModel.EvaluatePositionalBonus(new Vector2(0f, 3f), targetPosition, targetFacing),
                Is.EqualTo(BuilderPrototypePositionalBonus.None));
            Assert.That(
                BuilderPrototypeTacticalModel.EvaluatePositionalBonus(new Vector2(3f, 0f), targetPosition, targetFacing),
                Is.EqualTo(BuilderPrototypePositionalBonus.Flank));
            Assert.That(
                BuilderPrototypeTacticalModel.EvaluatePositionalBonus(new Vector2(0f, -3f), targetPosition, targetFacing),
                Is.EqualTo(BuilderPrototypePositionalBonus.Rear));
        }
    }
}
