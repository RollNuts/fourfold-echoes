# Exploration Tool Controller Input

## Final Goal Support

This removes a controller-first mismatch in the D031 production combat slice. The HUD/objective copy already tells players to use North Button for the Echo Tool, and this change makes the runtime exploration tool honor that route.

## Systems Touched

- Exploration tool runtime input
- Production combat onboarding copy
- EditMode input-contract tests

## Files Added Or Changed

- `Assets/Scripts/ExplorationTool.cs`
- `Assets/Scripts/ProductionCombatOnboardingHint.cs`
- `Assets/Editor/ExplorationToolInputVerifier.cs`
- `Assets/Editor/ExplorationToolInputVerifier.cs.meta`
- `Assets/Tests/EditMode/ExplorationToolTests.cs`
- `reports/commercial-progress-exploration-tool-controller-input-2026-06-27.md`

## Implementation

- Added default input constants for keyboard, mouse, and gamepad use.
- `ExplorationTool` now fires on `E`, right mouse button, or gamepad North Button.
- Added small public helpers so the input contract can be covered without simulating Unity input events.
- Updated onboarding copy to match the controller-first prompt already used by the objective cue and title UI.

## Tests

- Existing exploration tool behavior tests still cover node activation and cooldown behavior.
- Added EditMode coverage that the default input contract includes `E`, right mouse button, and North Button.
- Added EditMode coverage that the attack button is not accepted as the exploration tool input.
- Added a Unity batchmode `-executeMethod` verifier for the same input contract.

## Verification

- `git diff --check` passed for the changed exact paths.
- `node Scripts/Validation/validate_repo.mjs` passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs` passed.
- Credential/private path scan returned no hits in the changed exact paths.
- Unity Test Runner CLI exited 0 but returned without a result XML in this environment, so it was not treated as proof.
- Unity batchmode `-executeMethod FourfoldEchoes.Editor.ExplorationToolInputVerifier.VerifyInputContract` passed with exit code 0.

## Acceptance Conditions

- Controller player can use the Echo Tool with North Button in the D031 production slice.
- Keyboard/mouse player can still use `E` and right mouse button.
- Onboarding copy no longer advertises a narrower input route than the runtime supports.

## Next Smallest Useful Task

Run a PlayMode route proof that starts the production slice, solves the shortcut with the controller-equivalent Echo Tool input path, opens the boss gate, and reaches reward completion.
