# Controller Tool Input Progress

Date: 2026-06-27

## Purpose

This note records the narrow player-visible controller input improvement added
to the active D-031 production slice branch. It keeps the existing Legacy Input
path and does not migrate the project to the Input System.

## Player-Visible Change

- The Echo Tool now accepts the controller North Button by default.
- The production slice onboarding copy names the same controller route for Echo
  Tool and reward claim.
- Keyboard and mouse controls remain unchanged.

## Scope

Changed runtime/UI/test paths:

- `Assets/Scripts/ExplorationTool.cs`
- `Assets/Scripts/ProductionCombatOnboardingHint.cs`
- `Assets/Tests/EditMode/ExplorationToolTests.cs`

No scene content, assets, package settings, workflows, inventory, crafting,
quest log, social system, open-world system, networking dependency, or second
exploration tool were added.

## Validation

- `git diff --check` on exact changed paths: passed.
- `node Scripts/Validation/validate_repo.mjs`: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed.
- Secret/private-path scan on exact changed paths: passed.
- Unity EditMode `FourfoldEchoes.Tests.EditMode.ExplorationToolTests`: passed
  with Test Runner XML, 4 total, 4 passed, 0 failed.
- Unity Test Runner logged exit code 0. The editor process remained alive after
  shutdown and was terminated after the passing XML was written, matching the
  current wrapper's residual-editor handling.

## Unity Follow-Up

The stale-base `-quit` attempt produced no XML and is not counted as a pass.
Future Test Runner batchmode should follow the latest wrapper guidance and
avoid `-quit`; treat missing Test Runner XML as not verified.

## Notes

The project currently uses Legacy Input for this slice, so the change follows
the existing `KeyCode` route. If the project later adopts the Input System for
UI/gameplay input, this input contract should be moved through the same action
asset instead of leaving a parallel path.
