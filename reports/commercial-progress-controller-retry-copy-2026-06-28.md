# Controller Retry Copy And Input

## Final Goal Support

This tightens the title -> game -> pause -> retry path for controller-first play. The retry state already had selectable UI buttons, but the direct retry copy and controller fallback did not explicitly name or accept the controller submit route.

## Systems Touched

- Production combat runtime retry input
- Production combat UI retry copy
- Production combat onboarding hint copy
- EditMode input/copy contract tests

## Files Added Or Changed

- `Assets/Scripts/ProductionCombatSliceController.cs`
- `Assets/Scripts/ProductionCombatSliceUi.cs`
- `Assets/Scripts/ProductionCombatOnboardingHint.cs`
- `Assets/Tests/EditMode/ProductionCombatSliceInputTests.cs`
- `reports/commercial-progress-controller-retry-copy-2026-06-28.md`

## Implementation

- Added a retry input contract for `R`, Enter, keypad Enter, Space, and gamepad South Button.
- PlayerDown and Completed runtime states now accept the retry input contract directly, so retry remains available even if UI button focus is not active.
- The retry overlay now tells players that South Button, Enter, or `R` restarts the room.
- The onboarding hint now names the same controller-first retry route while keeping the latest gameplay-start timer behavior.

## Tests

- Added EditMode coverage for the retry input contract.
- Added EditMode coverage that retry UI/onboarding copy names the controller submit route.

## Verification

- `git diff --check` on the changed exact paths: pass.
- `node Scripts/Validation/validate_repo.mjs`: pass.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: pass.
- Secret/private path scan on the changed exact paths: no hits.
- Unity EditMode `FourfoldEchoes.Tests.EditMode.ProductionCombatSliceInputTests`: exit code 0, XML 6 total, 6 passed, 0 failed.
- Unity PlayMode `FourfoldEchoes.Tests.PlayMode.SliceSceneSmokeTests.SLICE_PRODUCTION_TitlePauseRetryClearRouteSavesReward`: exit code 0, XML 1 total, 1 passed, 0 failed.

## Acceptance Conditions

- Controller players can retry from defeat/complete states with South Button.
- Keyboard players can retry with `R`, Enter, keypad Enter, or Space.
- On-screen copy no longer implies retry is keyboard-only.

## Next Smallest Useful Task

Run static validation, Unity EditMode, and PlayMode route validation, then open a ready PR with sanitized verifier evidence.
