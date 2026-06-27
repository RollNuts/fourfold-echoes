# Production Combat Reward Claim Prompt

## 1. Final Goal Support

This change improves the D020 vertical slice's first five minutes by making the
final reward interaction explicit at the moment the player reaches it. The slice
already supports controller reward claim on the current base; this pass adds a
near-chest prompt so keyboard, mouse, and controller players can finish the room
without guessing the claim input.

## 2. Systems Touched

- Runtime: `ProductionCombatRewardClaimPrompt`
- Runtime mapping visibility: `ProductionCombatSliceController`
- Tests: EditMode input and prompt coverage

## 3. Files Added / Changed

- `Assets/Scripts/ProductionCombatRewardClaimPrompt.cs`
- `Assets/Scripts/ProductionCombatSliceController.cs`
- `Assets/Tests/EditMode/ProductionCombatSliceInputTests.cs`
- `reports/production-combat-controller-reward-claim.md`

## 4. Implementation

`ProductionCombatRewardClaimPrompt` auto-registers for `ProductionCombatSlice`
and draws a compact bottom-center prompt only when all of these are true:

- the run is playing
- the boss gate is open
- the reward is not yet claimed
- the player is near the reward chest

The prompt reads `Claim reward` and shows `E / RMB / North Button`. The
controller now exposes the reward-claim mapping as constants so tests can
verify the player-facing instruction stays aligned with runtime input.

## 5. Tests

- `Gameplay_RewardClaimInput_MatchesTitlePrompt`
- `Gameplay_RewardClaimInput_DoesNotStealAttackButton`
- `UI_RewardClaimPrompt_ShowsOnlyNearOpenReward`
- `UI_RewardClaimPrompt_StaysInsideNarrowScreen`

## 6. Acceptance Conditions

- Near the open reward chest, the player sees how to claim the reward.
- The prompt hides outside the playing/open-gate/unclaimed-reward state.
- The attack button remains separate from reward claim input.
- No inventory, quest log, second tool, new scene, or new asset dependency is
  introduced.

## 7. Next Smallest Useful Task

Run a live controller pass through `ProductionCombatSlice` and confirm the
player can complete title, combat, Echo Tool shortcut, boss gate, reward prompt,
and reward claim without keyboard or mouse.
