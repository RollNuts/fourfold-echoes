# Production Gate Reward Cue

## Final Goal Support

This adds one player-visible commercial-readiness improvement to the production combat slice: after the boss gate opens, the player gets a compact reward-claim cue instead of relying on memory or debug-style status text.

## Systems Touched

- Production combat runtime input
- Production combat runtime HUD cue
- EditMode UI logic coverage

## Files Added Or Changed

- `Assets/Scripts/ProductionCombatSliceController.cs`
- `Assets/Scripts/ProductionCombatGateRewardCue.cs`
- `Assets/Scripts/ProductionCombatGateRewardCue.cs.meta`
- `Assets/Editor/ProductionCombatGateRewardCueVerifier.cs`
- `Assets/Editor/ProductionCombatGateRewardCueVerifier.cs.meta`
- `Assets/Tests/EditMode/ProductionCombatGateRewardCueTests.cs`
- `Assets/Tests/EditMode/ProductionCombatGateRewardCueTests.cs.meta`
- `reports/commercial-progress-gate-reward-cue-2026-06-27.md`

## Implementation

- The reward cue is created at runtime only in `ProductionCombatSlice`.
- It appears only while the run is playing, the gate is open, and the reward is not claimed.
- The cue stays in the upper-right safe frame so it does not cover the combat center or existing upper-left HUD.
- The copy changes when the player is close enough to the chest.
- Reward claim input now accepts keyboard, mouse, and gamepad north button.

## Tests

- Added EditMode coverage for cue visibility gating.
- Added EditMode coverage for reward proximity threshold.
- Added EditMode coverage for desktop and compact safe-frame placement.
- Added EditMode coverage for the near/far body copy switch.
- Added a Unity batchmode `-executeMethod` verifier for the same cue contract.

## Verification

- `git diff --check` passed for the changed exact paths.
- `node Scripts/Validation/validate_repo.mjs` passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs` passed.
- Unity Test Runner CLI returned without a result XML in this environment, so it was not treated as proof.
- Unity batchmode `-executeMethod FourfoldEchoes.Editor.ProductionCombatGateRewardCueVerifier.VerifyGateRewardCueContract` passed with exit code 0.

## Acceptance Conditions

- Gate closed or reward already claimed: no cue.
- Gate open and reward waiting: cue appears outside the central combat view.
- Near reward chest: cue shows the explicit claim command.
- Gamepad north button can claim the reward from the same range as keyboard and mouse.

## Next Smallest Useful Task

Run a focused PlayMode proof that clears the boss gate, approaches the reward chest, and verifies the completion state from a controller-equivalent input path.
