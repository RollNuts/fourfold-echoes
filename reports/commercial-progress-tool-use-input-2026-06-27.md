# Commercial Progress: Tool Use Input

Date: 2026-06-27

## 1. Final Goal Support

This change removes a controller-first completion risk in `ProductionCombatSlice`.
The Echo Tool now has a live controller use input and the HUD/onboarding prompts
match the current controller path used for tool and reward interactions.

## 2. Systems Touched

- Runtime exploration tool input
- Production combat HUD and onboarding control hints
- EditMode input-default coverage

No Input System package, EventSystem, scene, package, asmdef, save schema,
inventory, quest, network, or new gameplay system was added.

## 3. Files Added / Changed

- `Assets/Scripts/ExplorationTool.cs`
- `Assets/Scripts/ProductionCombatSliceUi.cs`
- `Assets/Scripts/ProductionCombatOnboardingHint.cs`
- `Assets/Tests/EditMode/ExplorationToolTests.cs`
- `reports/commercial-progress-tool-use-input-2026-06-27.md`

Existing dirty work in the main checkout remains outside this lane.

## 4. Implementation

- Added default North Button (`JoystickButton3`) and Right Mouse support to the
  live `ExplorationTool` use input.
- Updated HUD, title, and onboarding copy so visible prompts match the live use
  input: North Button / E / Right Mouse.
- Added an EditMode assertion that the exploration tool's default live input
  includes keyboard, controller, and mouse use paths.
- Reward claim controller input already exists in the current base through the
  production gate reward cue lane, so this PR does not duplicate that controller
  code.

## 5. Tests

- `git diff --check -- <changed exact paths>`: passed
- `node Scripts/Validation/validate_repo.mjs`: passed
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed
- `node tools/forge/check.mjs`: passed
- Exact-file secret/private path scan: passed
- Unity batchmode: not launched because active batchmode jobs were already
  running; exit code not produced for this lane.

## 6. Acceptance Conditions

- A controller player can use the Echo Tool without a mouse.
- HUD/title/onboarding prompts do not advertise a different controller button
  than the live input.
- No new MVP system or scope expansion is introduced.
- No raw Unity logs, local personal paths, tokens, secrets, or private URLs are
  committed.

## 7. Next Smallest Useful Task

Run a serialized Unity EditMode/PlayMode pass once no other batchmode process is
active, then capture a controller-device smoke for title, combat, Echo Tool,
reward claim, pause, retry, and completion.
