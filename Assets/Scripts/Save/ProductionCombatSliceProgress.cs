namespace FourfoldEchoes.Product
{
    public readonly struct ProductionCombatSliceProgressSnapshot
    {
        public ProductionCombatSliceProgressSnapshot(bool shortcutOpen, bool bossDefeated, bool rewardClaimed)
        {
            ShortcutOpen = shortcutOpen;
            BossDefeated = bossDefeated;
            RewardClaimed = rewardClaimed;
        }

        public bool ShortcutOpen { get; }
        public bool BossDefeated { get; }
        public bool RewardClaimed { get; }
    }

    public static class ProductionCombatSliceProgress
    {
        public const string SceneId = "ProductionCombatSlice";
        public const string ShortcutId = "shortcut.production_combat_slice.bridge";
        public const string BossId = "boss.production_combat_slice.core";
        public const string RewardId = "relic.production_combat_slice.reward";

        public static ProductionCombatSliceProgressSnapshot Read(FourfoldSaveData data)
        {
            if (data == null)
            {
                return new ProductionCombatSliceProgressSnapshot(false, false, false);
            }

            var rewardClaimed = data.IsRelicClaimed(RewardId);
            var bossDefeated = rewardClaimed || data.IsBossDefeated(BossId);
            var shortcutOpen = bossDefeated || data.IsShortcutOpened(ShortcutId);
            return new ProductionCombatSliceProgressSnapshot(shortcutOpen, bossDefeated, rewardClaimed);
        }

        public static void Write(FourfoldSaveData data, ProductionCombatSliceProgressSnapshot snapshot)
        {
            if (data == null)
            {
                return;
            }

            data.currentScene = SceneId;

            if (snapshot.ShortcutOpen || snapshot.BossDefeated || snapshot.RewardClaimed)
            {
                data.SetShortcutOpened(ShortcutId, true);
            }

            if (snapshot.BossDefeated || snapshot.RewardClaimed)
            {
                data.SetBossDefeated(BossId, true);
            }

            if (snapshot.RewardClaimed)
            {
                data.SetRelicClaimed(RewardId, true);
            }

            data.Normalize();
        }
    }
}
