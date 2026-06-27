# Production Combat Reward Focus

## 1. Final Goal Support

This change helps the D020 vertical slice read faster for a first-time player:
after the combat gate opens, the reward chest pulses until it is claimed. The
room already has a reward; this makes the next goal visible without adding
inventory, quest UI, or another system.

## 2. Systems Touched

- Runtime: `ProductionCombatRewardFocus`
- Target scene by convention: `ProductionCombatSlice`
- Tests: PlayMode reward focus behavior

## 3. Files Added / Changed

- `Assets/Scripts/ProductionCombatRewardFocus.cs`
- `Assets/Tests/PlayMode/ProductionCombatRewardFocusPlayModeTests.cs`
- `reports/production-combat-reward-focus.md`

## 4. Implementation

`ProductionCombatRewardFocus` auto-attaches to `ProductionCombatSliceController`
when `ProductionCombatSlice` loads. It finds the controller's `rewardChest` and
pulses its local scale only while the gate is open and the reward is still
waiting.

The component restores the original scale when focus is no longer needed and
does not override the claimed reward presentation.

## 5. Tests

- `RewardFocus_PulsesOnlyWhenGateOpenAndRewardWaiting`

The test verifies:

- no pulse before the gate opens
- pulse while reward is waiting
- focus stops after the reward is claimed
- scale restores when the gate is not open

## 6. Acceptance Conditions

- The reward is easier to identify after the boss gate opens.
- No scene file edit is required.
- No inventory, quest log, or extra UI system is added.
- The behavior is testable without relying on manual play.

## 7. Next Smallest Useful Task

Run a live controller playthrough of `ProductionCombatSlice` and confirm a
first-time route from title, combat, tool shortcut, boss gate, reward claim, and
retry/complete UI in one five-minute pass.
