using System;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class ProductionCombatGateRewardCueVerifier
    {
        public static void VerifyGateRewardCueContract()
        {
            Expect(
                ProductionCombatGateRewardCue.ShouldShowCue(ProductionCombatRunState.Playing, true, false),
                "Cue should show only while the open gate has an unclaimed reward.");
            Expect(
                !ProductionCombatGateRewardCue.ShouldShowCue(ProductionCombatRunState.Playing, true, true),
                "Cue should hide after reward claim.");
            Expect(
                !ProductionCombatGateRewardCue.ShouldShowCue(ProductionCombatRunState.Paused, true, false),
                "Cue should hide outside active play.");

            var reward = new Vector3(4f, 0f, 1f);
            Expect(
                ProductionCombatGateRewardCue.IsPlayerNearReward(reward + Vector3.right * 1.64f, reward),
                "Cue should treat the player as near inside reward claim range.");
            Expect(
                !ProductionCombatGateRewardCue.IsPlayerNearReward(reward + Vector3.right * 1.66f, reward),
                "Cue should treat the player as far outside reward claim range.");

            ExpectInside(ProductionCombatGateRewardCue.CalculateCueRect(1920f, 1080f), 1920f, 1080f);
            ExpectInside(ProductionCombatGateRewardCue.CalculateCueRect(360f, 640f), 360f, 640f);

            Expect(
                ProductionCombatGateRewardCue.BuildBodyText(false).IndexOf("chest", StringComparison.Ordinal) >= 0,
                "Far cue copy should point the player to the chest.");
            Expect(
                ProductionCombatGateRewardCue.BuildBodyText(true).IndexOf("Claim reward", StringComparison.Ordinal) >= 0,
                "Near cue copy should expose the claim command.");

            Debug.Log("FOURFOLD gate reward cue contract verified.");
        }

        private static void ExpectInside(Rect rect, float screenWidth, float screenHeight)
        {
            Expect(rect.xMin >= 16f, "Cue left edge should respect safe frame.");
            Expect(rect.yMin >= 16f, "Cue top edge should respect safe frame.");
            Expect(rect.xMax <= screenWidth - 16f, "Cue right edge should respect safe frame.");
            Expect(rect.yMax <= screenHeight - 16f, "Cue bottom edge should respect safe frame.");
        }

        private static void Expect(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
