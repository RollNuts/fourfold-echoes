using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.BuilderPrototype
{
    public enum BuilderPrototypeTelegraphShape
    {
        Circle,
        Donut,
        Rectangle,
        Cone
    }

    public enum BuilderPrototypeTelegraphTimingState
    {
        Pending,
        Telegraphing,
        Resolving,
        Expired
    }

    public enum BuilderPrototypePositionSafety
    {
        Safe,
        Threatened,
        Unsafe
    }

    public enum BuilderPrototypePositionalBonus
    {
        None,
        Flank,
        Rear
    }

    public struct BuilderPrototypeCastWindow
    {
        public const float DefaultResolveDuration = 0.25f;

        public BuilderPrototypeCastWindow(float telegraphStartsAt, float resolvesAt, float resolveDuration = DefaultResolveDuration)
        {
            TelegraphStartsAt = Mathf.Max(0f, telegraphStartsAt);
            ResolvesAt = Mathf.Max(TelegraphStartsAt, resolvesAt);
            ResolveEndsAt = ResolvesAt + Mathf.Max(0f, resolveDuration);
        }

        public float TelegraphStartsAt { get; }
        public float ResolvesAt { get; }
        public float ResolveEndsAt { get; }
        public float TelegraphDuration => ResolvesAt - TelegraphStartsAt;

        public BuilderPrototypeTelegraphTimingState StateAt(float time)
        {
            if (time < TelegraphStartsAt)
            {
                return BuilderPrototypeTelegraphTimingState.Pending;
            }

            if (time < ResolvesAt)
            {
                return BuilderPrototypeTelegraphTimingState.Telegraphing;
            }

            if (time <= ResolveEndsAt)
            {
                return BuilderPrototypeTelegraphTimingState.Resolving;
            }

            return BuilderPrototypeTelegraphTimingState.Expired;
        }
    }

    public struct BuilderPrototypeTelegraphZone
    {
        private const float Epsilon = 0.0001f;

        private BuilderPrototypeTelegraphZone(
            BuilderPrototypeTelegraphShape shape,
            Vector2 origin,
            Vector2 facing,
            float innerRadius,
            float outerRadius,
            Vector2 size,
            float angleDegrees,
            BuilderPrototypeCastWindow castWindow)
        {
            Shape = shape;
            Origin = origin;
            Facing = NormalizeOrFallback(facing);
            InnerRadius = Mathf.Max(0f, innerRadius);
            OuterRadius = Mathf.Max(0f, outerRadius);
            Size = new Vector2(Mathf.Max(0f, size.x), Mathf.Max(0f, size.y));
            AngleDegrees = Mathf.Clamp(angleDegrees, 0f, 360f);
            CastWindow = castWindow;
        }

        public BuilderPrototypeTelegraphShape Shape { get; }
        public Vector2 Origin { get; }
        public Vector2 Facing { get; }
        public float InnerRadius { get; }
        public float OuterRadius { get; }
        public Vector2 Size { get; }
        public float AngleDegrees { get; }
        public BuilderPrototypeCastWindow CastWindow { get; }

        public static BuilderPrototypeTelegraphZone Circle(
            Vector2 origin,
            float radius,
            BuilderPrototypeCastWindow castWindow)
        {
            return new BuilderPrototypeTelegraphZone(
                BuilderPrototypeTelegraphShape.Circle,
                origin,
                Vector2.up,
                0f,
                radius,
                Vector2.zero,
                360f,
                castWindow);
        }

        public static BuilderPrototypeTelegraphZone Donut(
            Vector2 origin,
            float innerRadius,
            float outerRadius,
            BuilderPrototypeCastWindow castWindow)
        {
            return new BuilderPrototypeTelegraphZone(
                BuilderPrototypeTelegraphShape.Donut,
                origin,
                Vector2.up,
                Mathf.Min(innerRadius, outerRadius),
                Mathf.Max(innerRadius, outerRadius),
                Vector2.zero,
                360f,
                castWindow);
        }

        public static BuilderPrototypeTelegraphZone Rectangle(
            Vector2 origin,
            Vector2 size,
            Vector2 facing,
            BuilderPrototypeCastWindow castWindow)
        {
            return new BuilderPrototypeTelegraphZone(
                BuilderPrototypeTelegraphShape.Rectangle,
                origin,
                facing,
                0f,
                0f,
                size,
                0f,
                castWindow);
        }

        public static BuilderPrototypeTelegraphZone Cone(
            Vector2 origin,
            float radius,
            float angleDegrees,
            Vector2 facing,
            BuilderPrototypeCastWindow castWindow)
        {
            return new BuilderPrototypeTelegraphZone(
                BuilderPrototypeTelegraphShape.Cone,
                origin,
                facing,
                0f,
                radius,
                Vector2.zero,
                angleDegrees,
                castWindow);
        }

        public bool Contains(Vector2 position)
        {
            var offset = position - Origin;
            switch (Shape)
            {
                case BuilderPrototypeTelegraphShape.Circle:
                    return offset.sqrMagnitude <= OuterRadius * OuterRadius + Epsilon;
                case BuilderPrototypeTelegraphShape.Donut:
                    return ContainsDonut(offset);
                case BuilderPrototypeTelegraphShape.Rectangle:
                    return ContainsRectangle(offset);
                case BuilderPrototypeTelegraphShape.Cone:
                    return ContainsCone(offset);
                default:
                    return false;
            }
        }

        private bool ContainsDonut(Vector2 offset)
        {
            var sqrDistance = offset.sqrMagnitude;
            return sqrDistance >= InnerRadius * InnerRadius - Epsilon
                && sqrDistance <= OuterRadius * OuterRadius + Epsilon;
        }

        private bool ContainsRectangle(Vector2 offset)
        {
            var right = new Vector2(Facing.y, -Facing.x);
            var localRight = Vector2.Dot(offset, right);
            var localForward = Vector2.Dot(offset, Facing);
            return Mathf.Abs(localRight) <= Size.x * 0.5f + Epsilon
                && Mathf.Abs(localForward) <= Size.y * 0.5f + Epsilon;
        }

        private bool ContainsCone(Vector2 offset)
        {
            if (offset.sqrMagnitude <= Epsilon)
            {
                return true;
            }

            if (offset.sqrMagnitude > OuterRadius * OuterRadius + Epsilon)
            {
                return false;
            }

            if (AngleDegrees >= 360f - Epsilon)
            {
                return true;
            }

            var directionToPosition = offset.normalized;
            var halfAngleRadians = AngleDegrees * 0.5f * Mathf.Deg2Rad;
            return Vector2.Dot(Facing, directionToPosition) >= Mathf.Cos(halfAngleRadians) - Epsilon;
        }

        private static Vector2 NormalizeOrFallback(Vector2 direction)
        {
            return direction.sqrMagnitude > Epsilon ? direction.normalized : Vector2.up;
        }
    }

    public struct BuilderPrototypeTacticalEvaluation
    {
        public BuilderPrototypeTacticalEvaluation(
            BuilderPrototypePositionSafety safety,
            int unsafeZoneCount,
            int threatenedZoneCount,
            float secondsUntilUnsafe)
        {
            Safety = safety;
            UnsafeZoneCount = Mathf.Max(0, unsafeZoneCount);
            ThreatenedZoneCount = Mathf.Max(0, threatenedZoneCount);
            SecondsUntilUnsafe = secondsUntilUnsafe >= 0f ? secondsUntilUnsafe : -1f;
        }

        public BuilderPrototypePositionSafety Safety { get; }
        public int UnsafeZoneCount { get; }
        public int ThreatenedZoneCount { get; }
        public float SecondsUntilUnsafe { get; }
        public bool IsSafe => Safety == BuilderPrototypePositionSafety.Safe;
    }

    public sealed class BuilderPrototypeTacticalModel
    {
        private const float NoKnownUnsafeTime = -1f;
        private const float RearDotThreshold = -0.5f;
        private const float FlankDotThreshold = 0.5f;

        private readonly List<BuilderPrototypeTelegraphZone> telegraphZones = new List<BuilderPrototypeTelegraphZone>();

        public IReadOnlyList<BuilderPrototypeTelegraphZone> TelegraphZones => telegraphZones;
        public int TelegraphZoneCount => telegraphZones.Count;

        public void AddTelegraphZone(BuilderPrototypeTelegraphZone zone)
        {
            telegraphZones.Add(zone);
        }

        public void ClearTelegraphZones()
        {
            telegraphZones.Clear();
        }

        public BuilderPrototypeTacticalEvaluation EvaluatePosition(Vector2 position, float time)
        {
            var unsafeZoneCount = 0;
            var threatenedZoneCount = 0;
            var secondsUntilUnsafe = float.PositiveInfinity;

            foreach (var zone in telegraphZones)
            {
                if (!zone.Contains(position))
                {
                    continue;
                }

                var timingState = zone.CastWindow.StateAt(time);
                if (timingState == BuilderPrototypeTelegraphTimingState.Resolving)
                {
                    unsafeZoneCount++;
                    secondsUntilUnsafe = 0f;
                }
                else if (timingState == BuilderPrototypeTelegraphTimingState.Telegraphing)
                {
                    threatenedZoneCount++;
                    secondsUntilUnsafe = Mathf.Min(secondsUntilUnsafe, zone.CastWindow.ResolvesAt - time);
                }
                else if (timingState == BuilderPrototypeTelegraphTimingState.Pending)
                {
                    secondsUntilUnsafe = Mathf.Min(secondsUntilUnsafe, zone.CastWindow.ResolvesAt - time);
                }
            }

            var safety = BuilderPrototypePositionSafety.Safe;
            if (unsafeZoneCount > 0)
            {
                safety = BuilderPrototypePositionSafety.Unsafe;
            }
            else if (threatenedZoneCount > 0)
            {
                safety = BuilderPrototypePositionSafety.Threatened;
            }

            return new BuilderPrototypeTacticalEvaluation(
                safety,
                unsafeZoneCount,
                threatenedZoneCount,
                float.IsPositiveInfinity(secondsUntilUnsafe) ? NoKnownUnsafeTime : Mathf.Max(0f, secondsUntilUnsafe));
        }

        public static BuilderPrototypePositionalBonus EvaluatePositionalBonus(
            Vector2 actorPosition,
            Vector2 targetPosition,
            Vector2 targetFacing)
        {
            var toActor = actorPosition - targetPosition;
            if (toActor.sqrMagnitude <= 0.0001f)
            {
                return BuilderPrototypePositionalBonus.None;
            }

            var facing = targetFacing.sqrMagnitude > 0.0001f ? targetFacing.normalized : Vector2.up;
            var dot = Vector2.Dot(facing, toActor.normalized);
            if (dot <= RearDotThreshold)
            {
                return BuilderPrototypePositionalBonus.Rear;
            }

            if (dot <= FlankDotThreshold)
            {
                return BuilderPrototypePositionalBonus.Flank;
            }

            return BuilderPrototypePositionalBonus.None;
        }
    }
}
