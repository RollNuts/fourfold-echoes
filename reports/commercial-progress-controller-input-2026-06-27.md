# Commercial Progress: Controller Tool And Claim Input

Date: 2026-06-27

## 1. Final Goal Support

This pass makes the playable slice less keyboard-first by letting a controller
use the core exploration tool and claim the reward without reaching for `E` or
right mouse. It supports the Steam-first controller-first target without adding
new systems or scope.

## 2. Systems Touched

- Exploration tool input
- Production slice reward claim input
- Production slice onboarding control copy
- Exploration tool EditMode coverage

No inventory, crafting, quest log, social system, open world, networking
dependency, package, or scene content was added.

## 3. Files Added/Changed

- Changed: `Assets/Scripts/ExplorationTool.cs`
- Changed: `Assets/Scripts/ProductionCombatSliceController.cs`
- Changed: `Assets/Scripts/ProductionCombatOnboardingHint.cs`
- Changed: `Assets/Tests/EditMode/ExplorationToolTests.cs`
- Added: `reports/commercial-progress-controller-input-2026-06-27.md`

## 4. Implementation

- Added `ExplorationTool.gamepadUseKey`, defaulting to `JoystickButton3`.
- Routed exploration-tool activation through keyboard or controller use input.
- Added `ExplorationTool.AcceptsUseKey` so the controller default is covered by
  EditMode tests without simulating Unity input polling.
- Added `JoystickButton3` as a reward-claim input in the production combat
  slice, matching the exploration-tool controller button.
- Updated onboarding copy so players see the controller route.

Hardcoded text changed:

- `Echo Tool / Claim: North Button / E / Right Mouse    Pause: Menu / Esc / P`

## 5. Tests

Static checks:

- `git diff --check` on the exact touched code/test/report paths: passed.
- `git diff --cached --check` on the exact touched paths after staging: passed.
- `node Scripts/Validation/validate_repo.mjs`: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed.
- Secret/private path scan on the exact touched paths: passed.

Unity:

- EditMode `FourfoldEchoes.Tests.EditMode.ExplorationToolTests`: exit code 0,
  4 total, 4 passed, 0 failed.

Warnings observed and treated as environment/non-blocking for this focused
test: Unity CoreBusinessMetrics SQLite messages, licensing access-token message,
obsolete `NavigationStatic` warning in an editor importer outside this lane,
and shutdown noise from ADB/usbmuxd/.NET build-server.

## 6. Acceptance Conditions

Accepted for this pass when:

- The exploration tool accepts the controller use button by default.
- Reward claim accepts the same controller button.
- The on-screen onboarding copy names the controller route.
- Focused EditMode coverage passes.

## 7. Next Smallest Useful Task

Run the full updated PlayMode slice route in serialized Unity to prove
controller-facing title, pause, retry, tool, save, and reward flow still
cohere in the scene.
